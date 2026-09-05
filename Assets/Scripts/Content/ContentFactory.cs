using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Builds the Week 1 playable hierarchy. Visuals come from Assets/Art/Import
    /// FBX when present; primitives remain as a fallback.
    /// </summary>
    public sealed class ContentFactory : MonoBehaviour
    {
        public const float ShipLengthMeters = 3f;
        public const float LargeAsteroidMeters = 4.8f;
        public const float SmallAsteroidMeters = 1.8f;
        public const float EnemyMeters = 2f;

        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private Transform _threatRoot;
        private Transform _projectileRoot;
        private Transform _pickupRoot;
        private GameObject _hangarDressing;
        private GameObject _arenaRoot;
        private string _arenaVisualName;
        private Material _hull;
        private Material _accent;
        private Material _glass;
        private Material _glow;
        private Material _asteroid;
        private Material _enemy;
        private Material _arena;
        private Material _projectile;
        private Material _projectileSpread;
        private Material _projectilePierce;
        private Material _projectileEnemy;
        private Material _projectileHalo;
        private Material _shield;
        private Material _accentHot;
        private Material _accentWarm;
        private Material _asteroidB;
        private Material _hangarMetal;
        private Material _hangarAmber;
        private Material _hangarCyan;
        private Material _hangarGlow;
        private Material _launchAmber;
        private Material _launchGlow;
        private Material _launchDecal;
        private static readonly Color HangarAmbient = new Color(0.2f, 0.17f, 0.13f);
        private static readonly Color CombatAmbient = new Color(0.12f, 0.14f, 0.18f);

        public Material Hull
        {
            get { return _hull; }
        }

        public Material Accent
        {
            get { return _accent; }
        }

        public void BuildPalette()
        {
            _hull = MakeMaterial("Mat_Ship_Hull", new Color(0.45f, 0.52f, 0.58f), 0.45f, 0.35f);
            _accent = MakeMaterial("Mat_Ship_Accent", new Color(1f, 0.55f, 0.14f), 0.2f, 0.55f, new Color(1f, 0.4f, 0.05f) * 1.4f);
            _accentHot = MakeMaterial("Mat_Ship_Accent_Hot", new Color(1f, 0.38f, 0.08f), 0.15f, 0.45f, new Color(1f, 0.25f, 0.04f) * 1.8f);
            _accentWarm = MakeMaterial("Mat_Ship_Accent_Warm", new Color(1f, 0.68f, 0.28f), 0.18f, 0.5f, new Color(1f, 0.5f, 0.12f) * 1.1f);
            _glass = MakeTransparent("Mat_Ship_Glass", new Color(0.35f, 0.7f, 0.95f, 0.28f), new Color(0.2f, 0.5f, 0.8f) * 0.4f);
            _glow = MakeMaterial("Mat_Ship_Glow", new Color(1f, 0.55f, 0.15f), 0f, 0.15f, new Color(1f, 0.45f, 0.05f) * 2.2f);
            _asteroid = MakeMaterial("Mat_Asteroid", new Color(0.38f, 0.32f, 0.28f), 0.05f, 0.18f);
            _asteroidB = MakeMaterial("Mat_Asteroid_B", new Color(0.46f, 0.3f, 0.22f), 0.04f, 0.14f);
            _enemy = MakeMaterial("Mat_Enemy", new Color(0.72f, 0.16f, 0.18f), 0.25f, 0.4f, new Color(0.6f, 0.05f, 0.08f));
            _arena = MakeMaterial("Mat_Arena", new Color(0.07f, 0.11f, 0.14f), 0.1f, 0.12f);
            _projectile = MakeMaterial("Mat_Projectile", new Color(1f, 0.92f, 0.42f), 0f, 0.35f, new Color(1f, 0.78f, 0.18f) * 3.4f);
            _projectileSpread = MakeMaterial("Mat_Projectile_Spread", new Color(1f, 0.42f, 0.08f), 0f, 0.28f, new Color(1f, 0.32f, 0.02f) * 4.4f);
            _projectilePierce = MakeMaterial("Mat_Projectile_Pierce", new Color(0.28f, 0.95f, 1f), 0f, 0.32f, new Color(0.12f, 0.7f, 1f) * 4.8f);
            _projectileEnemy = MakeMaterial("Mat_Projectile_Enemy", new Color(1f, 0.28f, 0.22f), 0f, 0.3f, new Color(1f, 0.12f, 0.08f) * 3.4f);
            _projectileHalo = MakeTransparent("Mat_Projectile_Halo", new Color(1f, 0.85f, 0.35f, 0.28f), new Color(1f, 0.7f, 0.15f) * 1.8f);
            _hangarMetal = MakeMaterial("Mat_Hangar_Metal", new Color(0.28f, 0.32f, 0.36f), 0.55f, 0.42f);
            _hangarAmber = MakeMaterial("Mat_Hangar_Amber", new Color(1f, 0.58f, 0.16f), 0.2f, 0.5f, new Color(1f, 0.42f, 0.06f) * 1.6f);
            _hangarCyan = MakeMaterial("Mat_Hangar_Cyan", new Color(0.22f, 0.72f, 0.88f), 0.15f, 0.45f, new Color(0.15f, 0.55f, 0.8f) * 1.4f);
            _hangarGlow = MakeMaterial("Mat_Hangar_Glow", new Color(1f, 0.72f, 0.28f), 0f, 0.2f, new Color(1f, 0.55f, 0.12f) * 2.6f);
            _launchAmber = MakeMaterial("Mat_LaunchSign_Amber", new Color(1f, 0.62f, 0.14f), 0.12f, 0.42f, new Color(1f, 0.48f, 0.06f) * 3.8f);
            _launchGlow = MakeMaterial("Mat_LaunchSign_Glow", new Color(1f, 0.86f, 0.32f), 0f, 0.18f, new Color(1f, 0.7f, 0.12f) * 5.2f);
            _launchDecal = MakeMaterial("Mat_LaunchSign_Decal", new Color(1f, 0.94f, 0.42f), 0f, 0.12f, new Color(1f, 0.82f, 0.18f) * 6.4f);
            _shield = MakeTransparent("Mat_Shield", new Color(0.25f, 0.85f, 1f, 0.22f), new Color(0.2f, 0.7f, 1f) * 0.6f);
            ArtImport.WarmPlayModeAssets();
        }

        public void BuildArena()
        {
            _arenaRoot = new GameObject("Arena_Blockout");
            _arenaRoot.AddComponent<ArenaBounds>().Radius = WaveManager.ArenaRadius;
            ApplyArenaForWave(1);

            _threatRoot = new GameObject("Threats").transform;
            _projectileRoot = new GameObject("Projectiles").transform;
            _pickupRoot = new GameObject("Pickups").transform;
            BuildHangarDressing();
        }

        public static readonly string[] ArenaWorlds =
        {
            "Arena_Blockout",
            "Arena_World2_Blockout",
            "Arena_World3_Blockout",
            "Arena_World4_Blockout",
            "Arena_World5_Blockout",
            "Arena_World6_Blockout",
        };

        public static int WorldIndexForWave(int waveIndex)
        {
            return ((Mathf.Max(1, waveIndex) - 1) / 5 % ArenaWorlds.Length) + 1;
        }

        public static string ArenaVisualForWave(int waveIndex)
        {
            return ArenaWorlds[WorldIndexForWave(waveIndex) - 1];
        }

        public void ApplyArenaForWave(int waveIndex)
        {
            if (_arenaRoot == null)
            {
                return;
            }

            string visualName = ArenaVisualForWave(waveIndex);
            if (visualName == _arenaVisualName)
            {
                return;
            }

            bool announce = !string.IsNullOrEmpty(_arenaVisualName);

            for (int i = _arenaRoot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(_arenaRoot.transform.GetChild(i).gameObject);
            }

            _arenaRoot.name = visualName;
            if (!TryVisual(visualName, _arenaRoot.transform, _arena))
            {
                GameObject floor = CreatePrimitive(PrimitiveType.Cylinder, "Arena_Floor", _arenaRoot.transform, _arena);
                floor.transform.localScale = new Vector3(WaveManager.ArenaRadius * 2f, 0.04f, WaveManager.ArenaRadius * 2f);
                floor.transform.position = new Vector3(0f, -0.08f, 0f);
            }

            _arenaVisualName = visualName;
            if (announce)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayWorldChange();
                }

                if (GameUi.Instance != null)
                {
                    GameUi.Instance.AnnounceWorldChange(WorldIndexForWave(waveIndex));
                }
            }
        }

        public void SetHangarDressingVisible(bool visible)
        {
            if (_hangarDressing != null)
            {
                _hangarDressing.SetActive(visible);
            }

            RenderSettings.ambientLight = visible ? HangarAmbient : CombatAmbient;
        }

        private void BuildHangarDressing()
        {
            _hangarDressing = new GameObject("Hangar_Dressing");

            PlaceHangarProp("Hangar_Crate", "Hangar_Crate", new Vector3(-5.2f, 0f, -4.4f), _hangarMetal, PrimitiveType.Cube,
                new Vector3(1.3f, 1.1f, 1.3f), 1.1f, Color.clear, 0f);
            PlaceHangarProp("Hangar_Crate_2", "Hangar_Crate", new Vector3(-6.5f, 0f, -5.7f), _hangarMetal, PrimitiveType.Cube,
                new Vector3(1.05f, 0.9f, 1.05f), 0.9f, Color.clear, 0f);
            PlaceHangarProp("Hangar_Terminal", "Hangar_Terminal", new Vector3(5.4f, 0f, -3.6f), _hangarCyan, PrimitiveType.Cube,
                new Vector3(1.1f, 1.8f, 0.55f), 1.8f, new Color(0.25f, 0.7f, 1f), 1.1f);
            PlaceHangarProp("Hangar_LightPillar", "Hangar_LightPillar", new Vector3(0f, 0f, 7.5f), _hangarGlow, PrimitiveType.Cylinder,
                new Vector3(0.55f, 2.4f, 0.55f), 4.8f, new Color(1f, 0.7f, 0.25f), 1.8f);
            PlaceHangarProp("Hangar_LightPillar_2", "Hangar_LightPillar", new Vector3(-8.2f, 0f, -2.4f), _hangarGlow, PrimitiveType.Cylinder,
                new Vector3(0.45f, 2.1f, 0.45f), 4.2f, new Color(1f, 0.62f, 0.2f), 1.4f);
            PlaceHangarProp("Hangar_Workbench", "Hangar_Workbench", new Vector3(-7.4f, 0f, 1.8f), _hangarMetal, PrimitiveType.Cube,
                new Vector3(1.6f, 0.85f, 0.9f), 0.85f, Color.clear, 0f);
            PlaceHangarProp("Hangar_FuelCell", "Hangar_FuelCell", new Vector3(7.4f, 0f, 1.6f), _hangarGlow, PrimitiveType.Cylinder,
                new Vector3(0.7f, 1.1f, 0.7f), 2.2f, new Color(1f, 0.55f, 0.12f), 1.2f);
            PlaceHangarProp("Hangar_ShopKiosk", "Hangar_ShopKiosk", new Vector3(2.2f, 0f, -6.8f), _hangarAmber, PrimitiveType.Cube,
                new Vector3(1.2f, 1.6f, 0.7f), 1.6f, new Color(1f, 0.5f, 0.12f), 1.35f);
            PlaceHangarProp("Hangar_Banner", "Hangar_Banner", new Vector3(0f, 0f, 9.4f), _hangarAmber, PrimitiveType.Cube,
                new Vector3(2.4f, 1.6f, 0.2f), 1.6f, Color.clear, 0f);
            PlaceHangarProp("Hangar_AmmoRack", "Hangar_AmmoRack", new Vector3(-7.2f, 0f, 5.2f), _hangarMetal, PrimitiveType.Cube,
                new Vector3(1.4f, 1.4f, 0.55f), 1.4f, Color.clear, 0f);
            PlaceHangarProp("Hangar_AmmoRack_2", "Hangar_AmmoRack", new Vector3(7.1f, 0f, 5.0f), _hangarMetal, PrimitiveType.Cube,
                new Vector3(1.3f, 1.3f, 0.5f), 1.3f, Color.clear, 0f);
            PlaceHangarProp("Hangar_Console", "Hangar_Console", new Vector3(3.4f, 0f, -4.6f), _hangarCyan, PrimitiveType.Cube,
                new Vector3(1.35f, 1.15f, 0.7f), 1.15f, new Color(0.3f, 0.75f, 1f), 1.15f);
            PlaceHangarProp("Hangar_PowerBox", "Hangar_PowerBox", new Vector3(-3.8f, 0f, -3.2f), _hangarAmber, PrimitiveType.Cube,
                new Vector3(0.85f, 1.35f, 0.55f), 1.35f, new Color(1f, 0.45f, 0.1f), 0.85f);
            PlaceHangarProp("Hangar_FireExtinguisher", "Hangar_FireExtinguisher", new Vector3(1.35f, 0f, -2.9f), _accentHot, PrimitiveType.Cylinder,
                new Vector3(0.22f, 0.55f, 0.22f), 1.1f, Color.clear, 0f);
            PlaceHangarProp("Hangar_Locker", "Hangar_Locker", new Vector3(-4.8f, 0f, 6.6f), _hangarMetal, PrimitiveType.Cube,
                new Vector3(1.15f, 2.05f, 0.72f), 2.05f, Color.clear, 0f);
            GameObject launchSign = PlaceHangarProp("Hangar_LaunchSign", "Hangar_LaunchSign", new Vector3(1.95f, 0f, -2.55f), _launchAmber, PrimitiveType.Cube,
                new Vector3(1.28f, 2.05f, 0.2f), 2.05f, new Color(1f, 0.78f, 0.22f), 2.65f, 198f);
            DressLaunchSign(launchSign);
            PlaceHangarProp("Hangar_ShipComplete", "Ship_Complete", new Vector3(8.2f, 0f, 7.4f), _hull, PrimitiveType.Cube,
                new Vector3(1.1f, 0.55f, 2.4f), 0.55f, new Color(1f, 0.55f, 0.16f), 0.7f);

            CreatePrimitive(PrimitiveType.Cylinder, "Hangar_ShipPad", _hangarDressing.transform, _hangarAmber,
                new Vector3(0f, 0.02f, 0f), new Vector3(4.6f, 0.04f, 4.6f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Hangar_KioskPlate", _hangarDressing.transform, _hangarCyan,
                new Vector3(2.2f, 0.015f, -6.4f), new Vector3(2.4f, 0.03f, 1.8f), Quaternion.identity);
        }

        private GameObject PlaceHangarProp(
            string instanceName,
            string visualName,
            Vector3 floorPosition,
            Material material,
            PrimitiveType mesh,
            Vector3 meshScale,
            float meshHeight,
            Color lightColor,
            float lightIntensity)
        {
            return PlaceHangarProp(
                instanceName,
                visualName,
                floorPosition,
                material,
                mesh,
                meshScale,
                meshHeight,
                lightColor,
                lightIntensity,
                0f);
        }

        private GameObject PlaceHangarProp(
            string instanceName,
            string visualName,
            Vector3 floorPosition,
            Material material,
            PrimitiveType mesh,
            Vector3 meshScale,
            float meshHeight,
            Color lightColor,
            float lightIntensity,
            float yawDegrees)
        {
            GameObject root = new GameObject(instanceName);
            root.transform.SetParent(_hangarDressing.transform, false);
            root.transform.position = floorPosition;
            if (Mathf.Abs(yawDegrees) > 0.01f)
            {
                root.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            }
            PartSlot slot = root.AddComponent<PartSlot>();
            slot.SlotId = visualName;
            if (!TryVisual(visualName, root.transform, material)
                && !BuildHangarBufferFallback(visualName, root.transform))
            {
                CreatePrimitive(mesh, "Mesh", root.transform, material,
                    new Vector3(0f, meshHeight * 0.5f, 0f), meshScale, Quaternion.identity);
            }

            if (lightIntensity > 0.01f)
            {
                GameObject lamp = new GameObject("HangarLight");
                lamp.transform.SetParent(root.transform, false);
                lamp.transform.localPosition = new Vector3(0f, meshHeight * 0.65f + 0.4f, 0f);
                Light light = lamp.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = lightColor;
                light.intensity = lightIntensity;
                light.range = 7.5f;
            }

            return root;
        }

        private void DressLaunchSign(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Light lamp = root.GetComponentInChildren<Light>();
            if (lamp != null)
            {
                lamp.type = LightType.Spot;
                lamp.range = 11f;
                lamp.spotAngle = 68f;
                lamp.intensity = 3.15f;
                lamp.color = new Color(1f, 0.8f, 0.28f);
                lamp.transform.localPosition = new Vector3(0f, 2.15f, 0.55f);
                lamp.transform.localRotation = Quaternion.Euler(18f, 180f, 0f);
            }

            CreatePrimitive(PrimitiveType.Cube, "LaunchGoPlate", root.transform, _launchGlow,
                new Vector3(0f, 1.38f, 0.14f), new Vector3(0.92f, 0.62f, 0.04f), Quaternion.identity);
            GameObject decal = new GameObject("LaunchGoDecal");
            decal.transform.SetParent(root.transform, false);
            decal.transform.localPosition = new Vector3(0f, 1.38f, 0.18f);
            decal.transform.localRotation = Quaternion.identity;
            TextMesh go = decal.AddComponent<TextMesh>();
            go.text = "GO";
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
            {
                go.font = font;
            }

            go.fontSize = 72;
            go.characterSize = 0.085f;
            go.anchor = TextAnchor.MiddleCenter;
            go.alignment = TextAlignment.Center;
            go.color = new Color(1f, 0.95f, 0.42f);
            go.fontStyle = FontStyle.Bold;

            HangarSignPulse pulse = root.AddComponent<HangarSignPulse>();
            pulse.SignLight = lamp;
            pulse.BaseIntensity = 3.15f;
            pulse.BaseEmission = new Color(1f, 0.7f, 0.14f) * 4.6f;
        }

        private bool BuildHangarBufferFallback(string visualName, Transform parent)
        {
            if (visualName == "Hangar_Console")
            {
                CreatePrimitive(PrimitiveType.Cube, "Desk", parent, _hangarMetal,
                    new Vector3(0f, 0.42f, 0f), new Vector3(1.4f, 0.84f, 0.72f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Screen", parent, _hangarCyan,
                    new Vector3(0f, 1.18f, -0.18f), new Vector3(1.05f, 0.62f, 0.1f), Quaternion.identity);
                return true;
            }

            if (visualName == "Hangar_PowerBox")
            {
                CreatePrimitive(PrimitiveType.Cube, "Cabinet", parent, _hangarMetal,
                    new Vector3(0f, 0.68f, 0f), new Vector3(0.82f, 1.36f, 0.5f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Stripe", parent, _hangarAmber,
                    new Vector3(0f, 1.05f, 0.26f), new Vector3(0.7f, 0.12f, 0.06f), Quaternion.identity);
                return true;
            }

            if (visualName == "Hangar_FireExtinguisher")
            {
                CreatePrimitive(PrimitiveType.Cylinder, "Tank", parent, _accentHot,
                    new Vector3(0f, 0.55f, 0f), new Vector3(0.28f, 0.55f, 0.28f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Nozzle", parent, _hangarMetal,
                    new Vector3(0.16f, 1.05f, 0f), new Vector3(0.22f, 0.08f, 0.08f), Quaternion.identity);
                return true;
            }

            if (visualName == "Hangar_Locker")
            {
                CreatePrimitive(PrimitiveType.Cube, "Cabinet", parent, _hangarMetal,
                    new Vector3(0f, 1.02f, 0f), new Vector3(1.1f, 2.04f, 0.68f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Door", parent, _hangarAmber,
                    new Vector3(0f, 1.05f, 0.36f), new Vector3(0.92f, 1.7f, 0.06f), Quaternion.identity);
                return true;
            }

            if (visualName == "Ship_Complete")
            {
                CreatePrimitive(PrimitiveType.Cube, "Hull", parent, _hull,
                    new Vector3(0f, 0.35f, 0f), new Vector3(1.05f, 0.5f, 1.7f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Nose", parent, _accent,
                    new Vector3(0f, 0.32f, 1.05f), new Vector3(0.4f, 0.28f, 0.7f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Engine", parent, _glow,
                    new Vector3(0f, 0.3f, -1.05f), new Vector3(0.55f, 0.28f, 0.4f), Quaternion.identity);
                return true;
            }

            if (visualName == "Hangar_LaunchSign")
            {
                CreatePrimitive(PrimitiveType.Cube, "Post", parent, _hangarMetal,
                    new Vector3(0f, 0.92f, 0f), new Vector3(0.16f, 1.84f, 0.16f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Board", parent, _hangarAmber,
                    new Vector3(0f, 1.72f, 0.1f), new Vector3(1.42f, 0.82f, 0.1f), Quaternion.identity);
                return true;
            }

            return false;
        }

        public ShipController BuildShip(PlayerLoadout loadout, GameManager game, Camera camera)
        {
            GameObject root = new GameObject("Ship");
            root.tag = GameTags.Player;
            root.transform.position = Vector3.zero;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 1.4f;
            body.angularDamping = 4f;
            body.constraints = RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.mass = 1.2f;

            CapsuleCollider hull = root.AddComponent<CapsuleCollider>();
            hull.direction = 2;
            hull.radius = 0.55f;
            hull.height = ShipLengthMeters;
            hull.center = Vector3.zero;

            Transform slots = new GameObject("Slots").transform;
            slots.SetParent(root.transform, false);
            slots.localPosition = Vector3.zero;

            Transform bodySlot = CreateSlot("Ship_Body", slots);
            GameObject defaultBody;
            if (!TryVisual("Ship_Body", bodySlot, _hull, out defaultBody))
            {
                defaultBody = CreatePrimitive(PrimitiveType.Cube, "Mesh", bodySlot, _hull,
                    Vector3.zero, new Vector3(1.1f, 0.6f, 1.5f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Canopy", defaultBody.transform, _glass,
                    new Vector3(0f, 0.28f, 0.15f), new Vector3(0.55f, 0.22f, 0.7f), Quaternion.identity);
            }

            GameObject upgradedBody;
            if (!TryVisual("Ship_Body_Upgrade01", bodySlot, _hull, out upgradedBody))
            {
                upgradedBody = CreatePrimitive(PrimitiveType.Cube, "Ship_Body_Upgrade01", bodySlot, _accent,
                    Vector3.zero, new Vector3(1.25f, 0.7f, 1.65f), Quaternion.identity);
            }

            upgradedBody.SetActive(false);

            Transform noseSlot = CreateSlot("Ship_Nose", slots);
            GameObject defaultNose;
            if (!TryVisual("Ship_Nose", noseSlot, _accent, out defaultNose))
            {
                defaultNose = CreatePrimitive(PrimitiveType.Cube, "Mesh_Default", noseSlot, _accent,
                    new Vector3(0f, 0f, 1.1f), new Vector3(0.45f, 0.35f, 0.7f), Quaternion.identity);
            }

            GameObject upgradedNose;
            if (!TryVisual("Ship_Nose_Upgrade01", noseSlot, _accent, out upgradedNose))
            {
                upgradedNose = new GameObject("Ship_Nose_Upgrade01");
                upgradedNose.transform.SetParent(noseSlot, false);
                upgradedNose.transform.localPosition = Vector3.zero;
                CreatePrimitive(PrimitiveType.Cube, "Barrel_L", upgradedNose.transform, _accent,
                    new Vector3(-0.28f, 0f, 1.2f), new Vector3(0.2f, 0.2f, 1.05f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Barrel_R", upgradedNose.transform, _accent,
                    new Vector3(0.28f, 0f, 1.2f), new Vector3(0.2f, 0.2f, 1.05f), Quaternion.identity);
            }

            upgradedNose.SetActive(false);

            GameObject upgradedNose02;
            if (!TryVisual("Ship_Nose_Upgrade02", noseSlot, _accent, out upgradedNose02))
            {
                upgradedNose02 = CreatePrimitive(PrimitiveType.Cube, "Ship_Nose_Upgrade02", noseSlot, _accentHot,
                    new Vector3(0f, 0f, 1.25f), new Vector3(0.55f, 0.28f, 1.2f), Quaternion.identity);
            }

            upgradedNose02.SetActive(false);

            Transform engineSlot = CreateSlot("Ship_Engine", slots);
            GameObject defaultEngine;
            if (!TryVisual("Ship_Engine", engineSlot, _hull, out defaultEngine))
            {
                defaultEngine = new GameObject("Mesh_Default");
                defaultEngine.transform.SetParent(engineSlot, false);
                CreatePrimitive(PrimitiveType.Cube, "Mesh", defaultEngine.transform, _hull,
                    new Vector3(0f, 0f, -1.05f), new Vector3(0.8f, 0.45f, 0.55f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Glow", defaultEngine.transform, _glow,
                    new Vector3(0f, 0f, -1.38f), new Vector3(0.45f, 0.28f, 0.18f), Quaternion.identity);
            }

            GameObject upgradedEngine;
            if (!TryVisual("Ship_Engine_Upgrade01", engineSlot, _hull, out upgradedEngine))
            {
                upgradedEngine = new GameObject("Ship_Engine_Upgrade01");
                upgradedEngine.transform.SetParent(engineSlot, false);
                upgradedEngine.transform.localPosition = Vector3.zero;
                CreatePrimitive(PrimitiveType.Cube, "Mesh", upgradedEngine.transform, _hull,
                    new Vector3(0f, 0f, -1.1f), new Vector3(0.95f, 0.5f, 0.7f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Glow_L", upgradedEngine.transform, _glow,
                    new Vector3(-0.28f, 0f, -1.5f), new Vector3(0.32f, 0.24f, 0.28f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Glow_R", upgradedEngine.transform, _glow,
                    new Vector3(0.28f, 0f, -1.5f), new Vector3(0.32f, 0.24f, 0.28f), Quaternion.identity);
            }

            upgradedEngine.SetActive(false);

            GameObject upgradedEngine02;
            if (!TryVisual("Ship_Engine_Upgrade02", engineSlot, _hull, out upgradedEngine02))
            {
                upgradedEngine02 = CreatePrimitive(PrimitiveType.Cube, "Ship_Engine_Upgrade02", engineSlot, _glow,
                    new Vector3(0f, 0f, -1.2f), new Vector3(1.05f, 0.55f, 0.8f), Quaternion.identity);
            }

            upgradedEngine02.SetActive(false);

            GameObject shield = CreatePrimitive(PrimitiveType.Sphere, "ShieldBubble", slots, _shield,
                Vector3.zero, new Vector3(2.4f, 2.4f, 2.4f), Quaternion.identity);
            shield.SetActive(false);

            Transform muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(root.transform, false);
            muzzle.localPosition = new Vector3(0f, 0f, 1.65f);

            ShipVisuals visuals = root.AddComponent<ShipVisuals>();
            visuals.BodySlot = bodySlot;
            visuals.DefaultBody = defaultBody;
            visuals.UpgradedBody = upgradedBody;
            visuals.NoseSlot = noseSlot;
            visuals.EngineSlot = engineSlot;
            visuals.DefaultNose = defaultNose;
            visuals.UpgradedNose = upgradedNose;
            visuals.UpgradedNose02 = upgradedNose02;
            visuals.DefaultEngine = defaultEngine;
            visuals.UpgradedEngine = upgradedEngine;
            visuals.UpgradedEngine02 = upgradedEngine02;
            visuals.ShieldBubble = shield;

            ShipHealth health = root.AddComponent<ShipHealth>();
            ShipShooter shooter = root.AddComponent<ShipShooter>();
            ShipController controller = root.AddComponent<ShipController>();

            health.Bind(game, visuals);
            shooter.Bind(loadout, this, muzzle);
            controller.Bind(health, shooter, visuals, camera);
            ApplyLoadoutVisuals(controller, loadout.State);
            return controller;
        }

        public void ApplyLoadoutVisuals(ShipController ship, LoadoutState loadout)
        {
            if (ship != null && ship.Visuals != null)
            {
                ship.Visuals.ApplyLoadout(loadout);
            }
        }

        public Asteroid CreateLargeAsteroid(Vector3 position, WaveManager waves)
        {
            Vector3 tangent = Vector3.Cross(position.normalized, Vector3.up);
            if (tangent.sqrMagnitude < 0.01f)
            {
                tangent = Vector3.right;
            }

            Vector3 drift = tangent.normalized * Random.Range(1.6f, 3.2f);
            return CreateAsteroid(AsteroidSize.Large, position, drift, waves);
        }

        public Asteroid CreateSmallAsteroid(Vector3 position, Vector3 drift, WaveManager waves)
        {
            return CreateAsteroid(AsteroidSize.Small, position, drift, waves);
        }

        public EnemySeeker CreateEnemy(Vector3 position, Transform player, WaveManager waves)
        {
            return CreateEnemy(position, player, waves, "Enemy_01");
        }

        public EnemySeeker CreateEnemy(Vector3 position, Transform player, WaveManager waves, string visualName)
        {
            if (string.IsNullOrEmpty(visualName))
            {
                visualName = "Enemy_01";
            }

            GameObject root = new GameObject(visualName);
            root.tag = GameTags.Enemy;
            root.transform.SetParent(_threatRoot, false);
            root.transform.position = position;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 1f;
            body.angularDamping = 2f;
            body.constraints = RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            EnemyKind kind = EnemyCatalog.FromVisual(visualName);
            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 2;
            collider.radius = EnemyCatalog.ColliderRadius(kind);
            collider.height = EnemyCatalog.ColliderHeight(kind);

            if (!TryEnemyVisual(visualName, root.transform))
            {
                CreatePrimitive(PrimitiveType.Capsule, "Mesh", root.transform, _enemy,
                    Vector3.zero, new Vector3(0.7f, 0.7f, 0.7f), Quaternion.Euler(90f, 0f, 0f));
                CreatePrimitive(PrimitiveType.Cube, "Canard", root.transform, _accent,
                    new Vector3(0f, 0.15f, -0.35f), new Vector3(1.4f, 0.08f, 0.35f), Quaternion.identity);
            }
            else if (kind == EnemyKind.Mid01)
            {
                DressMidMesh(root.transform);
            }

            EnemySeeker seeker = root.AddComponent<EnemySeeker>();
            seeker.Initialize(player, waves, kind);
            if (kind == EnemyKind.SwarmPod && AudioCues.Instance != null)
            {
                AudioCues.Instance.PlaySwarmPodSpawn();
            }

            return seeker;
        }

        public Projectile SpawnProjectile(Vector3 origin, Vector3 direction, float speed, int damage)
        {
            return SpawnProjectile(origin, direction, speed, damage, false);
        }

        public Projectile SpawnProjectile(Vector3 origin, Vector3 direction, float speed, int damage, bool pierce)
        {
            return SpawnProjectile(origin, direction, speed, damage, pierce, false);
        }

        public Projectile SpawnProjectile(Vector3 origin, Vector3 direction, float speed, int damage, bool pierce, bool spread)
        {
            return SpawnBolt(origin, direction, speed, damage, pierce, spread, false, EnemyKind.Mid01);
        }

        public Projectile SpawnEnemyProjectile(
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            EnemyKind kind)
        {
            return SpawnBolt(origin, direction, speed, damage, false, false, true, kind);
        }

        private Projectile SpawnBolt(
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            bool pierce,
            bool spread,
            bool hostile,
            EnemyKind kind)
        {
            GameObject root = new GameObject(hostile ? "EnemyProjectile" : "Projectile");
            root.tag = GameTags.Projectile;
            root.transform.SetParent(_projectileRoot, false);
            root.transform.position = origin;
            root.layer = 0;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = hostile ? 0.2f : 0.18f;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;

            Material boltMat = hostile
                ? _projectileEnemy
                : (pierce ? _projectilePierce : (spread ? _projectileSpread : _projectile));
            string visual = hostile ? "Projectile_EnemyBolt" : "Projectile_Bolt";
            GameObject mesh;
            if (!TryVisual(visual, root.transform, boltMat, out mesh))
            {
                Vector3 fallbackScale = pierce
                    ? new Vector3(0.1f, 0.1f, 1.25f)
                    : (spread ? new Vector3(0.38f, 0.38f, 0.32f) : new Vector3(0.2f, 0.2f, 0.62f));
                mesh = CreatePrimitive(PrimitiveType.Sphere, "Mesh", root.transform, boltMat,
                    Vector3.zero, fallbackScale, Quaternion.identity);
            }
            else if (mesh != null)
            {
                if (pierce)
                {
                    mesh.transform.localScale = new Vector3(0.48f, 0.48f, 2.05f);
                }
                else if (spread)
                {
                    mesh.transform.localScale = new Vector3(1.65f, 1.65f, 0.48f);
                }
            }

            if (pierce)
            {
                CreatePrimitive(PrimitiveType.Capsule, "PierceNeedle", root.transform, boltMat,
                    Vector3.zero, new Vector3(0.12f, 0.12f, 1.55f), Quaternion.Euler(90f, 0f, 0f));
            }
            else if (spread)
            {
                CreatePrimitive(PrimitiveType.Sphere, "SpreadCore", root.transform, boltMat,
                    Vector3.zero, new Vector3(0.46f, 0.46f, 0.26f), Quaternion.identity);
            }

            Material haloMat = pierce ? _projectilePierce : (hostile ? _projectileEnemy : (spread ? _projectileSpread : _projectileHalo));
            Vector3 haloScale = pierce
                ? new Vector3(0.18f, 0.18f, 1.55f)
                : (spread ? new Vector3(0.74f, 0.74f, 0.46f) : new Vector3(0.38f, 0.38f, 0.85f));
            CreatePrimitive(PrimitiveType.Sphere, "BoltHalo", root.transform, haloMat,
                Vector3.zero, haloScale, Quaternion.identity);

            TrailRenderer trail = root.AddComponent<TrailRenderer>();
            trail.time = pierce ? 0.42f : (spread ? 0.14f : (hostile ? 0.16f : 0.12f));
            trail.startWidth = spread ? 0.52f : (pierce ? 0.07f : 0.2f);
            trail.endWidth = pierce ? 0.005f : (spread ? 0.04f : 0.02f);
            trail.minVertexDistance = 0.12f;
            trail.material = boltMat;
            if (pierce)
            {
                trail.startColor = new Color(0.35f, 0.9f, 1f, 0.95f);
                trail.endColor = new Color(0.1f, 0.45f, 1f, 0f);
            }
            else if (spread)
            {
                trail.startColor = new Color(1f, 0.45f, 0.08f, 0.95f);
                trail.endColor = new Color(1f, 0.2f, 0.02f, 0f);
            }
            else if (hostile)
            {
                trail.startColor = new Color(1f, 0.25f, 0.18f, 0.9f);
                trail.endColor = new Color(0.6f, 0.02f, 0.02f, 0f);
            }
            else
            {
                trail.startColor = new Color(1f, 0.85f, 0.3f, 0.9f);
                trail.endColor = new Color(1f, 0.7f, 0.15f, 0f);
            }

            Projectile projectile = root.AddComponent<Projectile>();
            projectile.Launch(direction, speed, damage, pierce, hostile, kind);
            _projectiles.Add(projectile);
            return projectile;
        }

        public void ClearProjectiles()
        {
            for (int i = 0; i < _projectiles.Count; i++)
            {
                if (_projectiles[i] != null)
                {
                    Destroy(_projectiles[i].gameObject);
                }
            }

            _projectiles.Clear();
        }

        private Asteroid CreateAsteroid(AsteroidSize size, Vector3 position, Vector3 drift, WaveManager waves)
        {
            float meters = size == AsteroidSize.Large ? LargeAsteroidMeters : SmallAsteroidMeters;
            string propName = PickAsteroidVisual(size);
            GameObject root = new GameObject(propName);
            root.tag = GameTags.Asteroid;
            root.transform.SetParent(_threatRoot, false);
            root.transform.position = position;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 0.05f;
            body.angularDamping = 0.05f;
            body.constraints = RigidbodyConstraints.FreezePositionY;
            body.mass = size == AsteroidSize.Large ? 4f : 1.2f;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.radius = meters * 0.5f;

            Material rock = propName.IndexOf("VariantB", System.StringComparison.Ordinal) >= 0
                ? _asteroidB
                : _asteroid;
            if (!TryVisual(propName, root.transform, rock)
                && !TryVisual(size == AsteroidSize.Large ? "Asteroid_Large" : "Asteroid_Small", root.transform, _asteroid))
            {
                float wobble = size == AsteroidSize.Large ? 0.18f : 0.12f;
                bool faceted = propName.IndexOf("Variant", System.StringComparison.Ordinal) >= 0;
                Vector3 scale = new Vector3(
                    meters * (1f + Random.Range(-wobble, wobble)),
                    meters * (faceted ? 0.7f : 0.82f),
                    meters * (1f + Random.Range(-wobble, wobble)));
                CreatePrimitive(faceted ? PrimitiveType.Cube : PrimitiveType.Sphere, "Mesh", root.transform, rock,
                    Vector3.zero, scale, Quaternion.identity);
            }

            Asteroid asteroid = root.AddComponent<Asteroid>();
            asteroid.Initialize(size, waves, this, drift);
            return asteroid;
        }

        public GameObject CreatePickup(string visualName, Vector3 position)
        {
            if (string.IsNullOrEmpty(visualName))
            {
                visualName = "Pickup_Score";
            }

            GameObject root = new GameObject(visualName);
            if (_pickupRoot != null)
            {
                root.transform.SetParent(_pickupRoot, false);
            }

            root.transform.position = position;
            Material fallback = visualName.IndexOf("Shield", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? _shield
                : _accent;
            if (!TryVisual(visualName, root.transform, fallback))
            {
                CreatePrimitive(PrimitiveType.Sphere, "Mesh", root.transform, fallback,
                    Vector3.zero, new Vector3(0.7f, 0.7f, 0.7f), Quaternion.identity);
            }

            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            root.AddComponent<Pickup>().Bind(Pickup.KindFromName(visualName));
            return root;
        }

        public void ClearPickups()
        {
            if (_pickupRoot == null)
            {
                return;
            }

            for (int i = _pickupRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_pickupRoot.GetChild(i).gameObject);
            }
        }

        public void SpawnVfx(string assetName, Vector3 position, float lifetime)
        {
            GameObject root = new GameObject(assetName);
            root.transform.position = position;
            if (!TryVisual(assetName, root.transform, _glow))
            {
                CreatePrimitive(PrimitiveType.Sphere, "Mesh", root.transform, _glow,
                    Vector3.zero, new Vector3(0.45f, 0.45f, 0.45f), Quaternion.identity);
            }

            Destroy(root, lifetime);
        }

        public void MaybeDropPickup(Vector3 position)
        {
            if (Random.value > 0.22f)
            {
                return;
            }

            string[] kinds = { "Pickup_Score", "Pickup_Shield", "Pickup_Health", "Pickup_RapidFire" };
            CreatePickup(kinds[Random.Range(0, kinds.Length)], position);
        }

        private static string PickAsteroidVisual(AsteroidSize size)
        {
            string suffix = size == AsteroidSize.Large ? "Large" : "Small";
            float roll = Random.value;
            if (roll < 0.25f)
            {
                return "Asteroid_VariantC_" + suffix;
            }

            if (roll < 0.5f)
            {
                return "Asteroid_VariantD_" + suffix;
            }

            if (roll < 0.75f)
            {
                return "Asteroid_VariantB_" + suffix;
            }

            return "Asteroid_" + suffix;
        }

        private bool TryEnemyVisual(string visualName, Transform parent)
        {
            return TryVisual(visualName, parent, _enemy);
        }

        private bool TryVisual(string assetName, Transform parent, Material fallback)
        {
            GameObject instance;
            return TryVisual(assetName, parent, fallback, out instance);
        }

        private bool TryVisual(string assetName, Transform parent, Material fallback, out GameObject instance)
        {
            return ArtImport.TryInstantiate(assetName, parent, RemapImported, fallback, out instance);
        }

        private void DressMidMesh(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_EmissionColor", new Color(0.82f, 0.1f, 0.12f));
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].SetPropertyBlock(block);
                }
            }
        }

        private Material RemapImported(string importedName, Material fallback)
        {
            string name = importedName ?? string.Empty;
            if (ContainsIgnoreCase(name, "LaunchSign") || ContainsIgnoreCase(name, "Launch_Go")
                || ContainsIgnoreCase(name, "LaunchGo"))
            {
                if (ContainsIgnoreCase(name, "Decal") || ContainsIgnoreCase(name, "Text")
                    || ContainsIgnoreCase(name, "Go"))
                {
                    return _launchDecal;
                }

                if (ContainsIgnoreCase(name, "Glow") || ContainsIgnoreCase(name, "Light")
                    || ContainsIgnoreCase(name, "Emissive"))
                {
                    return _launchGlow;
                }

                return _launchAmber;
            }

            if (ContainsIgnoreCase(name, "Enemy") || ContainsIgnoreCase(name, "Mid")
                || ContainsIgnoreCase(name, "Swarm") || ContainsIgnoreCase(name, "Bomber"))
            {
                if (ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Canopy")
                    || ContainsIgnoreCase(name, "Eye") || ContainsIgnoreCase(name, "Stripe")
                    || ContainsIgnoreCase(name, "Glow"))
                {
                    return _accentHot;
                }

                return _enemy;
            }

            if (ContainsIgnoreCase(name, "Glass"))
            {
                return _glass;
            }

            if (ContainsIgnoreCase(name, "Glow"))
            {
                return _glow;
            }

            if (ContainsIgnoreCase(name, "Hull"))
            {
                return _hull;
            }

            if (ContainsIgnoreCase(name, "Asteroid"))
            {
                return ContainsIgnoreCase(name, "_B") || ContainsIgnoreCase(name, "Variant")
                    ? _asteroidB
                    : _asteroid;
            }

            if (ContainsIgnoreCase(name, "Enemy"))
            {
                return ContainsIgnoreCase(name, "Accent") ? _accentHot : _enemy;
            }

            if (ContainsIgnoreCase(name, "Arena"))
            {
                return _arena;
            }

            if (ContainsIgnoreCase(name, "Extinguisher") || ContainsIgnoreCase(name, "Fire"))
            {
                return _accentHot;
            }

            if (ContainsIgnoreCase(name, "Hangar") || ContainsIgnoreCase(name, "Kiosk")
                || ContainsIgnoreCase(name, "Crate") || ContainsIgnoreCase(name, "Workbench")
                || ContainsIgnoreCase(name, "Ammo") || ContainsIgnoreCase(name, "Console")
                || ContainsIgnoreCase(name, "PowerBox") || ContainsIgnoreCase(name, "Locker")
                || ContainsIgnoreCase(name, "LaunchSign") || ContainsIgnoreCase(name, "ShipComplete"))
            {
                if (ContainsIgnoreCase(name, "Glow") || ContainsIgnoreCase(name, "Light")
                    || ContainsIgnoreCase(name, "Fuel"))
                {
                    return _hangarGlow;
                }

                if (ContainsIgnoreCase(name, "Kiosk") || ContainsIgnoreCase(name, "Banner")
                    || ContainsIgnoreCase(name, "Amber") || ContainsIgnoreCase(name, "Power")
                    || ContainsIgnoreCase(name, "LaunchSign"))
                {
                    return _hangarAmber;
                }

                if (ContainsIgnoreCase(name, "Terminal") || ContainsIgnoreCase(name, "Screen")
                    || ContainsIgnoreCase(name, "Cyan") || ContainsIgnoreCase(name, "Console"))
                {
                    return _hangarCyan;
                }

                return _hangarMetal;
            }

            if (ContainsIgnoreCase(name, "Projectile") || ContainsIgnoreCase(name, "Bolt"))
            {
                return fallback != null ? fallback : _projectile;
            }

            if (ContainsIgnoreCase(name, "Shield"))
            {
                return _shield;
            }

            if (ContainsIgnoreCase(name, "Score") || ContainsIgnoreCase(name, "Pickup"))
            {
                return _accent;
            }

            if (ContainsIgnoreCase(name, "Hot"))
            {
                return _accentHot;
            }

            if (ContainsIgnoreCase(name, "Warm"))
            {
                return _accentWarm;
            }

            if (ContainsIgnoreCase(name, "Accent"))
            {
                return _accent;
            }

            return fallback;
        }

        private static bool ContainsIgnoreCase(string value, string token)
        {
            return value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Transform CreateSlot(string slotId, Transform parent)
        {
            GameObject slot = new GameObject(slotId);
            slot.transform.SetParent(parent, false);
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localRotation = Quaternion.identity;
            slot.transform.localScale = Vector3.one;
            PartSlot marker = slot.AddComponent<PartSlot>();
            marker.SlotId = slotId.Replace("Ship_", string.Empty);
            return slot.transform;
        }

        private static GameObject CreatePrimitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Material material)
        {
            return CreatePrimitive(type, name, parent, material, Vector3.zero, Vector3.one, Quaternion.identity);
        }

        private static GameObject CreatePrimitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Material material,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            go.transform.localRotation = localRotation;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return go;
        }

        private static Material MakeMaterial(string materialName, Color color, float metallic, float smoothness)
        {
            return MakeMaterial(materialName, color, metallic, smoothness, Color.black);
        }

        private static Material MakeMaterial(string materialName, Color color, float metallic, float smoothness, Color emission)
        {
            Shader shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.name = materialName;
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            if (emission.maxColorComponent > 0.01f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }

            return material;
        }

        private static Material MakeTransparent(string materialName, Color color, Color emission)
        {
            Material material = MakeMaterial(materialName, color, 0f, 0.7f, emission);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            return material;
        }
    }
}
