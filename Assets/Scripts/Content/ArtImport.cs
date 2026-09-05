using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Instantiates BlenderBot FBX at Play time with no Inspector mesh wiring.
    /// Prefers Resources.Load (Assets/Resources/Art/Import), then the artist
    /// drop folder via AssetDatabase in the Editor.
    /// </summary>
    public static class ArtImport
    {
        public const string ImportFolder = "Assets/Art/Import";
        public const string ResourcesKeyPrefix = "Art/Import/";

        public static readonly string[] PlayModeAssets =
        {
            "Ship_Body",
            "Ship_Nose",
            "Ship_Engine",
            "Ship_Nose_Upgrade01",
            "Ship_Engine_Upgrade01",
            "Enemy_01",
            "Enemy_Scout",
            "Enemy_Gunner",
            "Enemy_Drone",
            "Asteroid_Large",
            "Asteroid_Small",
            "Asteroid_VariantB_Large",
            "Asteroid_VariantB_Small",
            "Arena_Blockout",
            "Hangar_Crate",
            "Hangar_Terminal",
            "Hangar_LightPillar",
            "Projectile_Bolt",
            "Pickup_Score",
            "Pickup_Shield",
        };

        private static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();
        private static readonly HashSet<string> MissingLogged = new HashSet<string>();
        private static bool _summaryLogged;

        public static string AssetPath(string assetName)
        {
            return ImportFolder + "/" + StripExtension(assetName) + ".fbx";
        }

        public static GameObject LoadPrefab(string assetName)
        {
            string key = StripExtension(assetName);
            GameObject prefab;
            if (Cache.TryGetValue(key, out prefab))
            {
                return prefab;
            }

            string path = AssetPath(key);
            prefab = Resources.Load<GameObject>(ResourcesKeyPrefix + key);
#if UNITY_EDITOR
            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Resources/Art/Import/" + key + ".fbx");
            }
#endif

            Cache[key] = prefab;
            if (prefab == null && MissingLogged.Add(key))
            {
                Debug.LogWarning("ArtImport: missing Resources/" + ResourcesKeyPrefix + key
                    + " and " + path + " — using primitive fallback.");
            }

            return prefab;
        }

        public static void WarmPlayModeAssets()
        {
            int loaded = 0;
            for (int i = 0; i < PlayModeAssets.Length; i++)
            {
                if (LoadPrefab(PlayModeAssets[i]) != null)
                {
                    loaded++;
                }
            }

            if (!_summaryLogged)
            {
                _summaryLogged = true;
                Debug.Log("ArtImport: " + loaded + "/" + PlayModeAssets.Length
                    + " Play Mode FBX ready from " + ImportFolder + ".");
            }
        }

        public static bool TryInstantiate(
            string assetName,
            Transform parent,
            Func<string, Material, Material> remap,
            Material fallback)
        {
            GameObject instance;
            return TryInstantiate(assetName, parent, remap, fallback, out instance);
        }

        public static bool TryInstantiate(
            string assetName,
            Transform parent,
            Func<string, Material, Material> remap,
            Material fallback,
            out GameObject instance)
        {
            instance = null;
            GameObject prefab = LoadPrefab(assetName);
            if (prefab == null)
            {
                return false;
            }

            instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            instance.name = StripExtension(assetName);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            SanitizeImportedVisual(instance);
            ApplyPalette(instance, remap, fallback);
            return true;
        }

        public static void SanitizeImportedVisual(GameObject instance)
        {
            Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            Light[] lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = false;
            }

            Camera[] cameras = instance.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = false;
            }

            Animator[] animators = instance.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].enabled = false;
            }
        }

        public static void ApplyPalette(GameObject instance, Func<string, Material, Material> remap, Material fallback)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] shared = renderer.sharedMaterials;
                if (shared == null || shared.Length == 0)
                {
                    if (fallback != null)
                    {
                        renderer.sharedMaterial = fallback;
                    }

                    continue;
                }

                Material[] mapped = new Material[shared.Length];
                for (int m = 0; m < shared.Length; m++)
                {
                    string importedName = shared[m] != null ? shared[m].name : string.Empty;
                    mapped[m] = remap != null ? remap(importedName, fallback) : fallback;
                    if (mapped[m] == null)
                    {
                        mapped[m] = shared[m] != null ? shared[m] : fallback;
                    }
                }

                renderer.sharedMaterials = mapped;
            }
        }

        private static string StripExtension(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return assetName;
            }

            if (assetName.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                || assetName.EndsWith(".FBX", StringComparison.OrdinalIgnoreCase))
            {
                return assetName.Substring(0, assetName.Length - 4);
            }

            return assetName;
        }
    }
}
