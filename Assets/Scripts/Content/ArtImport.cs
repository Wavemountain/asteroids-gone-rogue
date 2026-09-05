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
            "Ship_Body_Upgrade01",
            "Ship_Nose_Upgrade01",
            "Ship_Engine_Upgrade01",
            "Ship_Nose_Upgrade02",
            "Ship_Engine_Upgrade02",
            "Enemy_01",
            "Enemy_Scout",
            "Enemy_Gunner",
            "Enemy_Drone",
            "Enemy_Bomber",
            "Enemy_Sniper",
            "Enemy_SwarmPod",
            "Asteroid_Large",
            "Asteroid_Small",
            "Asteroid_VariantB_Large",
            "Asteroid_VariantB_Small",
            "Asteroid_VariantC_Large",
            "Asteroid_VariantC_Small",
            "Asteroid_VariantD_Large",
            "Asteroid_VariantD_Small",
            "Arena_Blockout",
            "Arena_World2_Blockout",
            "Arena_World3_Blockout",
            "Arena_World4_Blockout",
            "Arena_World5_Blockout",
            "Arena_World6_Blockout",
            "Hangar_Crate",
            "Hangar_Terminal",
            "Hangar_LightPillar",
            "Hangar_Workbench",
            "Hangar_FuelCell",
            "Hangar_AmmoRack",
            "Hangar_Banner",
            "Hangar_ShopKiosk",
            "Hangar_Console",
            "Hangar_PowerBox",
            "Hangar_FireExtinguisher",
            "Hangar_Locker",
            "Hangar_LaunchSign",
            "Ship_Complete",
            "Projectile_Bolt",
            "Projectile_EnemyBolt",
            "Pickup_Score",
            "Pickup_Shield",
            "Pickup_Health",
            "Pickup_RapidFire",
            "Vfx_MuzzleFlash",
            "Vfx_Explosion_Lowpoly",
        };

        private static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();
        private static readonly HashSet<string> MissingLogged = new HashSet<string>();
        private static bool _summaryLogged;

        public static string AssetPath(string assetName)
        {
            return ImportFolder + "/" + StripExtension(assetName) + ".fbx";
        }

        /// <summary>
        /// Primary Play Mode name first, then optional GameBot Buffer_* aliases.
        /// Canonical files live as Enemy_Scout.fbx / Enemy_01.fbx / Ship_Complete.fbx / Hangar_LaunchSign.fbx (not Buffer_*).
        /// </summary>
        public static string[] CandidateNames(string assetName)
        {
            string key = StripExtension(assetName);
            switch (key)
            {
                case "Enemy_Scout":
                    return new[] { "Enemy_Scout", "Enemy_Scout_Buffer_v5", "Enemy_Scout_Buffer_v4" };
                case "Enemy_Gunner":
                    return new[] { "Enemy_Gunner", "Enemy_Gunner_Buffer_v5", "Enemy_Gunner_Buffer_v4" };
                case "Enemy_Drone":
                    return new[] { "Enemy_Drone", "Enemy_Drone_Buffer_v4" };
                case "Enemy_01":
                    return new[] { "Enemy_01", "Enemy_01_Buffer_v8" };
                case "Enemy_Bomber":
                    return new[] { "Enemy_Bomber", "Enemy_Bomber_Buffer_v6", "Enemy_Bomber_Buffer_v5" };
                case "Enemy_SwarmPod":
                    return new[] { "Enemy_SwarmPod", "Enemy_SwarmPod_Buffer_v6" };
                case "Ship_Complete":
                    return new[] { "Ship_Complete", "Ship_Complete_Buffer_v4" };
                case "Enemy_Sniper":
                    return new[] { "Enemy_Sniper", "Enemy_Sniper_Buffer_v5" };
                case "Hangar_LaunchSign":
                    return new[] { "Hangar_LaunchSign", "Hangar_LaunchSign_Buffer_v2" };
                case "Projectile_Bolt":
                    return new[] { "Projectile_Bolt", "Projectile_Bolt_Buffer_v2" };
                case "Projectile_EnemyBolt":
                    return new[] { "Projectile_EnemyBolt", "Projectile_EnemyBolt_Buffer" };
                default:
                    return new[] { key };
            }
        }

        public static GameObject LoadPrefab(string assetName)
        {
            string key = StripExtension(assetName);
            GameObject prefab;
            if (Cache.TryGetValue(key, out prefab))
            {
                return prefab;
            }

            string[] names = CandidateNames(key);
            for (int i = 0; i < names.Length; i++)
            {
                prefab = LoadPrefabExact(names[i]);
                if (prefab != null)
                {
                    break;
                }
            }

            Cache[key] = prefab;
            if (prefab == null && MissingLogged.Add(key))
            {
                Debug.LogWarning("ArtImport: missing Resources/" + ResourcesKeyPrefix + key
                    + " and " + AssetPath(key) + " — using primitive fallback.");
            }

            return prefab;
        }

        private static GameObject LoadPrefabExact(string key)
        {
            GameObject prefab = Resources.Load<GameObject>(ResourcesKeyPrefix + key);
#if UNITY_EDITOR
            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath(key));
            }

            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Resources/Art/Import/" + key + ".fbx");
            }
#endif
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
                    + " Play Mode FBX ready (Resources/Art/Import + " + ImportFolder + ").");
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
