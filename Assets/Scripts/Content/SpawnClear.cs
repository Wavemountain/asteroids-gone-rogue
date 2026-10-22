using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Drops leftover islands out of an imported arena mesh. The blockout is
    /// one renderer, so disabling it would hide the floor. A copy is edited
    /// only when an island is spawn clutter or a corner block on the lip.
    /// </summary>
    public static class SpawnClear
    {
        public static void ClearVisual(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            MeshFilter[] filters = visual.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                ClearFilter(filters[i]);
            }
        }

        public static void AppendIslands(MeshFilter filter, System.Collections.Generic.List<LayerGate.IslandFact> into)
        {
            if (filter == null || into == null)
            {
                return;
            }

            Renderer renderer = filter.GetComponent<Renderer>();
            if (renderer != null && !renderer.enabled)
            {
                return;
            }

            string name = DecorName(filter.transform);
            Mesh mesh = filter.sharedMesh;
            if (mesh == null || !mesh.isReadable)
            {
                if (renderer == null)
                {
                    return;
                }

                // An unreadable mesh used to emit a zero box at the origin, so the
                // floor and any child under the blockout root looked like spawn clutter.
                // The renderer's world bounds are the island we can actually see.
                Bounds bounds = renderer.bounds;
                LayerGate.IslandFact unread = new LayerGate.IslandFact();
                unread.Name = name;
                unread.MinX = bounds.min.x;
                unread.MaxX = bounds.max.x;
                unread.MinZ = bounds.min.z;
                unread.MaxZ = bounds.max.z;
                into.Add(unread);
                return;
            }

            int[][] subTriangles;
            int[] islandOfVertex;
            Island[] islands = Measure(mesh, filter.transform, out subTriangles, out islandOfVertex);
            if (islands == null)
            {
                return;
            }

            for (int i = 0; i < islands.Length; i++)
            {
                if (!islands[i].Used)
                {
                    continue;
                }

                LayerGate.IslandFact fact = new LayerGate.IslandFact();
                fact.Name = name;
                fact.MinX = islands[i].MinX;
                fact.MaxX = islands[i].MaxX;
                fact.MinZ = islands[i].MinZ;
                fact.MaxZ = islands[i].MaxZ;
                into.Add(fact);
            }
        }

        static void ClearFilter(MeshFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            Mesh source = filter.sharedMesh;
            if (source == null || !source.isReadable)
            {
                return;
            }

            int[][] subTriangles;
            int[] islandOfVertex;
            Island[] islands = Measure(source, filter.transform, out subTriangles, out islandOfVertex);
            if (islands == null || subTriangles == null)
            {
                return;
            }

            bool any = false;
            for (int i = 0; i < islands.Length; i++)
            {
                if (islands[i].Drop)
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            Mesh copy = Object.Instantiate(source);
            if (copy == null)
            {
                return;
            }

            copy.name = source.name;
            for (int submesh = 0; submesh < subTriangles.Length; submesh++)
            {
                copy.SetTriangles(KeepTriangles(subTriangles[submesh], islandOfVertex, islands), submesh);
            }

            copy.RecalculateBounds();
            filter.sharedMesh = copy;
        }

        struct Island
        {
            public float MinX;
            public float MaxX;
            public float MinZ;
            public float MaxZ;
            public bool Drop;
            public bool Seen;
            public bool Used;
        }

        static Island[] Measure(Mesh mesh, Transform xform, out int[][] subTriangles, out int[] islandOfVertex)
        {
            subTriangles = null;
            islandOfVertex = null;
            if (mesh == null)
            {
                return null;
            }

            Vector3[] vertices = mesh.vertices;
            if (vertices == null || vertices.Length == 0)
            {
                return null;
            }

            int subCount = mesh.subMeshCount;
            if (subCount < 1)
            {
                subCount = 1;
            }

            subTriangles = new int[subCount][];
            int[] parent = new int[vertices.Length];
            bool[] usedVertex = new bool[vertices.Length];
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
            }

            for (int submesh = 0; submesh < subCount; submesh++)
            {
                int[] triangles = mesh.GetTriangles(submesh);
                if (triangles == null)
                {
                    triangles = new int[0];
                }

                subTriangles[submesh] = triangles;
                for (int t = 0; t + 2 < triangles.Length; t += 3)
                {
                    MarkUsed(usedVertex, triangles[t]);
                    MarkUsed(usedVertex, triangles[t + 1]);
                    MarkUsed(usedVertex, triangles[t + 2]);
                    Union(parent, triangles[t], triangles[t + 1]);
                    Union(parent, triangles[t], triangles[t + 2]);
                }
            }

            islandOfVertex = new int[vertices.Length];
            int[] compact = new int[vertices.Length];
            for (int i = 0; i < compact.Length; i++)
            {
                compact[i] = -1;
            }

            int islandCount = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int root = Find(parent, i);
                if (compact[root] < 0)
                {
                    compact[root] = islandCount;
                    islandCount++;
                }

                islandOfVertex[i] = compact[root];
            }

            Island[] islands = new Island[islandCount];
            for (int i = 0; i < vertices.Length; i++)
            {
                int id = islandOfVertex[i];
                Vector3 world = xform != null ? xform.TransformPoint(vertices[i]) : vertices[i];
                if (!islands[id].Seen)
                {
                    islands[id].MinX = world.x;
                    islands[id].MaxX = world.x;
                    islands[id].MinZ = world.z;
                    islands[id].MaxZ = world.z;
                    islands[id].Seen = true;
                    continue;
                }

                if (world.x < islands[id].MinX)
                {
                    islands[id].MinX = world.x;
                }

                if (world.x > islands[id].MaxX)
                {
                    islands[id].MaxX = world.x;
                }

                if (world.z < islands[id].MinZ)
                {
                    islands[id].MinZ = world.z;
                }

                if (world.z > islands[id].MaxZ)
                {
                    islands[id].MaxZ = world.z;
                }
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                if (usedVertex[i])
                {
                    islands[islandOfVertex[i]].Used = true;
                }
            }

            float ring = WaveManager.ArenaRadius;
            for (int i = 0; i < islands.Length; i++)
            {
                if (!islands[i].Seen || !islands[i].Used)
                {
                    continue;
                }

                islands[i].Drop = LayerGate.IsSpawnClutter(islands[i].MinX, islands[i].MaxX, islands[i].MinZ, islands[i].MaxZ)
                    || LayerGate.IsRingCorner(islands[i].MinX, islands[i].MaxX, islands[i].MinZ, islands[i].MaxZ, ring)
                    || LayerGate.IsRingCorner(islands[i].MinX, islands[i].MaxX, islands[i].MinZ, islands[i].MaxZ, ArenaShapeCatalog.Active);
            }

            return islands;
        }

        static int[] KeepTriangles(int[] triangles, int[] islandOfVertex, Island[] islands)
        {
            if (triangles == null || triangles.Length == 0)
            {
                return new int[0];
            }

            int keep = 0;
            for (int t = 0; t + 2 < triangles.Length; t += 3)
            {
                if (!DropTriangle(triangles[t], islandOfVertex, islands))
                {
                    keep += 3;
                }
            }

            int[] kept = new int[keep];
            int write = 0;
            for (int t = 0; t + 2 < triangles.Length; t += 3)
            {
                if (DropTriangle(triangles[t], islandOfVertex, islands))
                {
                    continue;
                }

                kept[write] = triangles[t];
                kept[write + 1] = triangles[t + 1];
                kept[write + 2] = triangles[t + 2];
                write += 3;
            }

            return kept;
        }

        static bool DropTriangle(int vertex, int[] islandOfVertex, Island[] islands)
        {
            if (islandOfVertex == null || vertex < 0 || vertex >= islandOfVertex.Length)
            {
                return false;
            }

            int id = islandOfVertex[vertex];
            if (islands == null || id < 0 || id >= islands.Length)
            {
                return false;
            }

            return islands[id].Drop;
        }

        static void MarkUsed(bool[] used, int index)
        {
            if (used == null || index < 0 || index >= used.Length)
            {
                return;
            }

            used[index] = true;
        }

        static int Find(int[] parent, int index)
        {
            if (parent == null || index < 0 || index >= parent.Length)
            {
                return -1;
            }

            int root = index;
            while (parent[root] != root)
            {
                parent[root] = parent[parent[root]];
                root = parent[root];
            }

            return root;
        }

        static void Union(int[] parent, int a, int b)
        {
            int ra = Find(parent, a);
            int rb = Find(parent, b);
            if (ra < 0 || rb < 0 || ra == rb)
            {
                return;
            }

            parent[rb] = ra;
        }

        static string DecorName(Transform transform)
        {
            Transform cursor = transform;
            while (cursor != null)
            {
                string name = cursor.name;
                if (name != null && name.IndexOf("Blockout", System.StringComparison.Ordinal) >= 0)
                {
                    return name;
                }

                cursor = cursor.parent;
            }

            if (transform != null && transform.name != null)
            {
                return transform.name;
            }

            return "decor";
        }
    }
}
