using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    public static partial class AgrCapture
    {
        const string KeyLook = "AgrCapture.LookPolish";

        static bool LookRequested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] != "-lookPolish")
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

        static void CaptureLookPolish()
        {
            // Each file is a gameplay shot: PrepareGameplayShot starts w1,
            // PhaseBatch waits, AssertGameplayShot rejects a hangar or shop canvas.
            Debug.Log("AgrCapture look count " + LookPolish.CaptureCount.ToString());
        }

        static GameObject StageLookSubject(string fileName, bool after, bool reduce)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return null;
            }

            GameObject root = new GameObject("LookPolishStage");
            Vector3 at = camera.transform.position + (camera.transform.forward * 6f);
            root.transform.position = at;
            root.transform.rotation = Quaternion.LookRotation(-camera.transform.forward, Vector3.up);
            bool enemy = fileName.IndexOf("shot_enemy_", System.StringComparison.Ordinal) >= 0;
            bool shot = fileName.IndexOf("shot_", System.StringComparison.Ordinal) >= 0;
            bool grid = fileName.IndexOf("_grid", System.StringComparison.Ordinal) >= 0;
            bool hit = fileName.IndexOf("hit_vignette", System.StringComparison.Ordinal) >= 0;
            bool explosion = fileName.IndexOf("explosion_", System.StringComparison.Ordinal) >= 0;
            PrimitiveType primitive = PrimitiveType.Sphere;
            Vector3 scale = Vector3.one * 0.46f;
            Color color = new Color(0.72f, 0.35f, 0.16f, 1f);
            if (after && shot && !enemy)
            {
                primitive = PrimitiveType.Capsule;
                scale = new Vector3(LookPolish.BoltWidth, LookPolish.BoltLength * 0.5f, LookPolish.BoltWidth);
                color = new Color(LookPolish.CoreR, LookPolish.CoreG, LookPolish.CoreB, 1f);
            }
            else if (after && enemy)
            {
                bool diamond = fileName.IndexOf("sniper", System.StringComparison.Ordinal) >= 0
                    || fileName.IndexOf("brute", System.StringComparison.Ordinal) >= 0
                    || fileName.IndexOf("swarmpod", System.StringComparison.Ordinal) >= 0;
                primitive = diamond ? PrimitiveType.Cube : PrimitiveType.Sphere;
                float diameter = LookPolish.EnemyDiameter;
                scale = new Vector3(diameter, diameter, diamond ? diameter * 0.35f : diameter);
                color = new Color(LookPolish.EnemyCoreR, LookPolish.EnemyCoreG, LookPolish.EnemyCoreB, 1f);
            }
            else if (grid)
            {
                primitive = PrimitiveType.Quad;
                scale = new Vector3(8f, 8f, 1f);
                color = new Color(0.45f, 0.62f, 0.72f, after ? 0.35f : 0.85f);
            }
            else if (hit)
            {
                primitive = PrimitiveType.Quad;
                scale = new Vector3(3.2f, 3.2f, 1f);
                float alpha = reduce ? LookPolish.HitPeakReduce : LookPolish.HitPeak;
                color = new Color(LookPolish.HitRimR, LookPolish.HitRimG, LookPolish.HitRimB, alpha);
            }
            else if (explosion)
            {
                primitive = PrimitiveType.Sphere;
                float size = 0.6f;
                if (fileName.IndexOf("explosion_m", System.StringComparison.Ordinal) >= 0)
                {
                    size = 1.5f;
                }
                else if (fileName.IndexOf("explosion_l", System.StringComparison.Ordinal) >= 0)
                {
                    size = 2.4f;
                }

                if (reduce)
                {
                    size *= LookPolish.ReduceChunkMul;
                }

                scale = Vector3.one * size;
                color = new Color(LookPolish.CoreR, LookPolish.CoreG, LookPolish.CoreB, 1f);
            }

            GameObject body = GameObject.CreatePrimitive(primitive);
            body.name = "LookBody";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = scale;
            if (primitive == PrimitiveType.Capsule)
            {
                body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else if (primitive == PrimitiveType.Cube)
            {
                body.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }

            Renderer renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Unlit/Color");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                if (shader != null)
                {
                    Material material = new Material(shader);
                    material.color = color;
                    renderer.sharedMaterial = material;
                }
            }

            if (after && enemy)
            {
                GameObject rim = GameObject.CreatePrimitive(primitive);
                rim.name = "LookRim";
                rim.transform.SetParent(root.transform, false);
                float rimScale = LookPolish.EnemyDiameter + (LookPolish.EnemyRimThickness * 2f);
                rim.transform.localScale = new Vector3(rimScale, rimScale, primitive == PrimitiveType.Cube ? rimScale * 0.35f : rimScale);
                rim.transform.localRotation = body.transform.localRotation;
                Renderer rimRenderer = rim.GetComponent<Renderer>();
                if (rimRenderer != null && renderer != null && renderer.sharedMaterial != null)
                {
                    Material rimMaterial = new Material(renderer.sharedMaterial);
                    rimMaterial.color = new Color(LookPolish.EnemyRimR, LookPolish.EnemyRimG, LookPolish.EnemyRimB, 1f);
                    rimRenderer.sharedMaterial = rimMaterial;
                }
            }

            return root;
        }
    }
}
