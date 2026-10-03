using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Squashes a decor root so its world top stays at <see cref="DecorDepthRules.MaxDecorTop"/>
    /// while the bottom stays put, and pins gameplay renderers to the default layer
    /// so the decor camera cannot draw them.
    /// </summary>
    public static class DecorDepthClamp
    {
        public static void ClampTop(GameObject root, float cap)
        {
            if (root == null)
            {
                return;
            }

            float low;
            float high;
            if (!WorldSpanY(root, out low, out high))
            {
                return;
            }

            if (high <= cap + 0.001f)
            {
                return;
            }

            if (low >= cap - 0.02f)
            {
                root.transform.position += new Vector3(0f, cap - high, 0f);
                return;
            }

            float span = high - low;
            if (span < 0.001f)
            {
                return;
            }

            float allowed = cap - low;
            if (allowed < 0.05f)
            {
                allowed = 0.05f;
            }

            float factor = allowed / span;
            if (factor >= 0.999f)
            {
                return;
            }

            if (factor < 0.02f)
            {
                factor = 0.02f;
            }

            Transform decor = root.transform;
            Vector3 scale = decor.localScale;
            decor.localScale = new Vector3(scale.x, scale.y * factor, scale.z);
            float shiftedLow;
            float shiftedHigh;
            if (!WorldSpanY(root, out shiftedLow, out shiftedHigh))
            {
                return;
            }

            decor.position += new Vector3(0f, low - shiftedLow, 0f);
        }

        public static void KeepGameplay(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
            for (int nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                if (nodes[nodeIndex] != null)
                {
                    nodes[nodeIndex].gameObject.layer = 0;
                }
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                renderer.sortingOrder = 0;
                Material material = renderer.sharedMaterial;
                if (material != null && material.renderQueue < 3000 && material.renderQueue != DecorDepthRules.GameplayQueue)
                {
                    material.renderQueue = DecorDepthRules.GameplayQueue;
                }
            }
        }

        private static bool WorldSpanY(GameObject root, out float low, out float high)
        {
            low = 0f;
            high = 0f;
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            bool any = false;
            float foundLow = 0f;
            float foundHigh = 0f;
            for (int filterIndex = 0; filterIndex < filters.Length; filterIndex++)
            {
                MeshFilter filter = filters[filterIndex];
                if (filter == null || filter.sharedMesh == null)
                {
                    continue;
                }

                Bounds local = filter.sharedMesh.bounds;
                Transform meshTransform = filter.transform;
                Vector3 center = local.center;
                Vector3 extent = local.extents;
                for (int corner = 0; corner < 8; corner++)
                {
                    float sx = (corner & 1) == 0 ? -extent.x : extent.x;
                    float sy = (corner & 2) == 0 ? -extent.y : extent.y;
                    float sz = (corner & 4) == 0 ? -extent.z : extent.z;
                    Vector3 world = meshTransform.TransformPoint(center + new Vector3(sx, sy, sz));
                    if (!any)
                    {
                        foundLow = world.y;
                        foundHigh = world.y;
                        any = true;
                    }
                    else
                    {
                        if (world.y < foundLow)
                        {
                            foundLow = world.y;
                        }

                        if (world.y > foundHigh)
                        {
                            foundHigh = world.y;
                        }
                    }
                }
            }

            if (!any)
            {
                return false;
            }

            low = foundLow;
            high = foundHigh;
            return true;
        }
    }
}
