using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    public static partial class AgrCapture
    {
        const string KeyArenaEdge = "AgrCapture.ArenaEdge";
        static GameObject _arenaFlyIn;

        static bool ArenaEdgeRequested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] != "-arenaEdge")
                {
                    continue;
                }

                if (index + 1 >= args.Length)
                {
                    return true;
                }

                return args[index + 1] != "0";
            }

            return false;
        }

        static void CaptureArenaEdge()
        {
            string folder = SessionState.GetString(KeyOut, string.Empty);
            int count = ArenaEdgeShots.Count;
            Debug.Log("AgrCapture arena edge count " + count.ToString());
            for (int index = 0; index < count; index++)
            {
                string fileName = ArenaEdgeShots.FileName(index);
                Debug.Log("AgrCapture arena file " + fileName);
                bool reduce = fileName.IndexOf("_reduce_", System.StringComparison.Ordinal) >= 0;
                SettingsState posed = SettingsState.CreateDefault();
                posed.ReduceEffects = reduce;
                posed.FilmGrain = false;
                SettingsState.Publish(posed);
                StageArenaEdge(fileName, reduce);
                byte[] png = RenderFrame();
                if (png == null || png.Length < 64)
                {
                    Fail("empty arena edge capture " + fileName);
                    return;
                }

                string path = System.IO.Path.Combine(folder, fileName);
                System.IO.File.WriteAllBytes(path, png);
                Debug.Log("AgrCapture: wrote " + path + " (" + png.Length.ToString() + " bytes)");
            }
        }

        static void StageArenaEdge(string fileName, bool reduce)
        {
            int world = 1;
            for (int candidate = 1; candidate <= ArenaEdgeShots.WorldCount; candidate++)
            {
                string token = "arena_w" + candidate.ToString() + "_";
                if (fileName.IndexOf(token, System.StringComparison.Ordinal) == 0)
                {
                    world = candidate;
                }
            }

            int wave = ((world - 1) * 5) + 1;
            ContentFactory factory = Object.FindAnyObjectByType<ContentFactory>();
            if (factory != null)
            {
                factory.ApplyArenaForWave(wave);
            }

            ArenaShape shape = ArenaShapeCatalog.ForWorld(world);
            float bestAngle = 0f;
            float best = 0f;
            for (int sample = 0; sample < ArenaShape.SampleCount; sample++)
            {
                float angle = sample * (6.28318530718f / ArenaShape.SampleCount);
                float radius = shape.RadiusAt(angle);
                if (radius > best)
                {
                    best = radius;
                    bestAngle = angle;
                }
            }

            float shipX;
            float shipZ;
            shape.SpawnRing(bestAngle, -ArenaEdge.HardInside, out shipX, out shipZ);
            Vector3 ship = new Vector3(shipX, 0.4f, shipZ);
            HoldCamera(ship + new Vector3(0f, 35f, -22f), ship, 54f);

            if (_arenaFlyIn != null)
            {
                Object.DestroyImmediate(_arenaFlyIn);
                _arenaFlyIn = null;
            }

            int fly = fileName.IndexOf("_flyin_", System.StringComparison.Ordinal);
            if (fly < 0)
            {
                return;
            }

            int frame = 0;
            string tail = fileName.Substring(fly + 7);
            if (tail.Length > 0 && tail[0] >= '0' && tail[0] <= '3')
            {
                frame = tail[0] - '0';
            }

            float outsideX;
            float outsideZ;
            float insideX;
            float insideZ;
            float crossX;
            float crossZ;
            EntryFlight.Resolve(shape, bestAngle + 1.7f, 0.45f, shipX, shipZ, out outsideX, out outsideZ, out insideX, out insideZ, out crossX, out crossZ);
            float along = frame / 3f;
            if (along < 0f)
            {
                along = 0f;
            }

            if (along > 1f)
            {
                along = 1f;
            }

            Vector3 at = Vector3.Lerp(new Vector3(outsideX, 0.4f, outsideZ), new Vector3(insideX, 0.4f, insideZ), along);
            GameObject flyIn = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            flyIn.name = "ArenaEdgeFlyIn";
            _arenaFlyIn = flyIn;
            flyIn.transform.position = at;
            flyIn.transform.rotation = Quaternion.LookRotation(new Vector3(insideX - outsideX, 0f, insideZ - outsideZ), Vector3.up);
            Collider collider = flyIn.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            if (reduce)
            {
                flyIn.transform.localScale = Vector3.one;
            }
        }
    }
}
