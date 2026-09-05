using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Builds Week 1 stub visuals from primitives. Hierarchy names and part
    /// origins match Assets/Art/Import so FBX can replace meshes later.
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
        private GameObject _hangarDressing;
        private Material _hull;
        private Material _accent;
        private Material _glass;
        private Material _glow;
        private Material _asteroid;
        private Material _enemy;
        private Material _arena;
        private Material _projectile;
        private Material _shield;

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
            _glass = MakeTransparent("Mat_Ship_Glass", new Color(0.35f, 0.7f, 0.95f, 0.28f), new Color(0.2f, 0.5f, 0.8f) * 0.4f);
            _glow = MakeMaterial("Mat_Ship_Glow", new Color(1f, 0.55f, 0.15f), 0f, 0.15f, new Color(1f, 0.45f, 0.05f) * 2.2f);
            _asteroid = MakeMaterial("Mat_Asteroid", new Color(0.38f, 0.32f, 0.28f), 0.05f, 0.18f);
            _enemy = MakeMaterial("Mat_Enemy", new Color(0.72f, 0.16f, 0.18f), 0.25f, 0.4f, new Color(0.6f, 0.05f, 0.08f));
            _arena = MakeMaterial("Mat_Arena", new Color(0.07f, 0.11f, 0.14f), 0.1f, 0.12f);
            _projectile = MakeMaterial("Mat_Projectile", new Color(1f, 0.85f, 0.25f), 0f, 0.2f, new Color(1f, 0.7f, 0.1f) * 2f);
            _shield = MakeTransparent("Mat_Shield", new Color(0.25f, 0.85f, 1f, 0.22f), new Color(0.2f, 0.7f, 1f) * 0.6f);
        }

        public void BuildArena()
        {
            GameObject root = new GameObject("Arena_Blockout");
            root.AddComponent<ArenaBounds>().Radius = WaveManager.ArenaRadius;

            GameObject floor = CreatePrimitive(PrimitiveType.Cylinder, "Arena_Floor", root.transform, _arena);
            floor.transform.localScale = new Vector3(WaveManager.ArenaRadius * 2f, 0.04f, WaveManager.ArenaRadius * 2f);
            floor.transform.position = new Vector3(0f, -0.08f, 0f);

            int pads = 16;
            for (int i = 0; i < pads; i++)
            {
                float angle = (Mathf.PI * 2f * i) / pads;
                Vector3 pos = WaveManager.RingPoint(angle, WaveManager.ArenaRadius);
                GameObject block = CreatePrimitive(PrimitiveType.Cube, "Arena_Pad_" + i, root.transform, _arena);
                block.transform.position = pos + Vector3.up * 0.35f;
                block.transform.localScale = new Vector3(1.6f, 0.7f, 1.6f);
                block.transform.LookAt(Vector3.zero);
            }

            _threatRoot = new GameObject("Threats").transform;
            _projectileRoot = new GameObject("Projectiles").transform;
            BuildHangarDressing();
        }

        public void SetHangarDressingVisible(bool visible)
        {
            if (_hangarDressing != null)
            {
                _hangarDressing.SetActive(visible);
            }
        }

        private void BuildHangarDressing()
        {
            _hangarDressing = new GameObject("Hangar_Dressing");

            PlaceHangarProp("Hangar_Crate", new Vector3(-5.2f, 0f, -4.4f), _arena, PrimitiveType.Cube,
                new Vector3(1.3f, 1.1f, 1.3f), 1.1f);
            PlaceHangarProp("Hangar_Terminal", new Vector3(5.4f, 0f, -3.6f), _hull, PrimitiveType.Cube,
                new Vector3(1.1f, 1.8f, 0.55f), 1.8f);
            PlaceHangarProp("Hangar_LightPillar", new Vector3(0f, 0f, 7.5f), _glow, PrimitiveType.Cylinder,
                new Vector3(0.55f, 2.4f, 0.55f), 4.8f);
        }

        private void PlaceHangarProp(
            string propName,
            Vector3 floorPosition,
            Material material,
            PrimitiveType mesh,
            Vector3 meshScale,
            float meshHeight)
        {
            GameObject root = new GameObject(propName);
            root.transform.SetParent(_hangarDressing.transform, false);
            root.transform.position = floorPosition;
            PartSlot slot = root.AddComponent<PartSlot>();
            slot.SlotId = propName;
            CreatePrimitive(mesh, "Mesh", root.transform, material,
                new Vector3(0f, meshHeight * 0.5f, 0f), meshScale, Quaternion.identity);
        }

        public ShipController BuildShip(PlayerLoadout loadout, GameManager game, Camera camera)
        {
            GameObject root = new GameObject("Ship");
            root.tag = GameTags.Player;
            root.transform.position = Vector3.zero;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.drag = 1.4f;
            body.angularDrag = 4f;
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
            CreatePrimitive(PrimitiveType.Cube, "Mesh", bodySlot, _hull,
                Vector3.zero, new Vector3(1.1f, 0.6f, 1.5f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Canopy", bodySlot, _glass,
                new Vector3(0f, 0.28f, 0.15f), new Vector3(0.55f, 0.22f, 0.7f), Quaternion.identity);

            Transform noseSlot = CreateSlot("Ship_Nose", slots);
            GameObject defaultNose = CreatePrimitive(PrimitiveType.Cube, "Mesh_Default", noseSlot, _accent,
                new Vector3(0f, 0f, 1.1f), new Vector3(0.45f, 0.35f, 0.7f), Quaternion.identity);

            GameObject upgradedNose = new GameObject("Ship_Nose_Upgrade01");
            upgradedNose.transform.SetParent(noseSlot, false);
            upgradedNose.transform.localPosition = Vector3.zero;
            upgradedNose.SetActive(false);
            CreatePrimitive(PrimitiveType.Cube, "Barrel_L", upgradedNose.transform, _accent,
                new Vector3(-0.28f, 0f, 1.2f), new Vector3(0.2f, 0.2f, 1.05f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Barrel_R", upgradedNose.transform, _accent,
                new Vector3(0.28f, 0f, 1.2f), new Vector3(0.2f, 0.2f, 1.05f), Quaternion.identity);

            Transform engineSlot = CreateSlot("Ship_Engine", slots);
            GameObject defaultEngine = new GameObject("Mesh_Default");
            defaultEngine.transform.SetParent(engineSlot, false);
            CreatePrimitive(PrimitiveType.Cube, "Mesh", defaultEngine.transform, _hull,
                new Vector3(0f, 0f, -1.05f), new Vector3(0.8f, 0.45f, 0.55f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Glow", defaultEngine.transform, _glow,
                new Vector3(0f, 0f, -1.38f), new Vector3(0.45f, 0.28f, 0.18f), Quaternion.identity);

            GameObject upgradedEngine = new GameObject("Ship_Engine_Upgrade01");
            upgradedEngine.transform.SetParent(engineSlot, false);
            upgradedEngine.transform.localPosition = Vector3.zero;
            upgradedEngine.SetActive(false);
            CreatePrimitive(PrimitiveType.Cube, "Mesh", upgradedEngine.transform, _hull,
                new Vector3(0f, 0f, -1.1f), new Vector3(0.95f, 0.5f, 0.7f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Glow_L", upgradedEngine.transform, _glow,
                new Vector3(-0.28f, 0f, -1.5f), new Vector3(0.32f, 0.24f, 0.28f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Glow_R", upgradedEngine.transform, _glow,
                new Vector3(0.28f, 0f, -1.5f), new Vector3(0.32f, 0.24f, 0.28f), Quaternion.identity);

            GameObject shield = CreatePrimitive(PrimitiveType.Sphere, "ShieldBubble", slots, _shield,
                Vector3.zero, new Vector3(2.4f, 2.4f, 2.4f), Quaternion.identity);
            shield.SetActive(false);

            Transform muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(root.transform, false);
            muzzle.localPosition = new Vector3(0f, 0f, 1.65f);

            ShipVisuals visuals = root.AddComponent<ShipVisuals>();
            visuals.BodySlot = bodySlot;
            visuals.NoseSlot = noseSlot;
            visuals.EngineSlot = engineSlot;
            visuals.DefaultNose = defaultNose;
            visuals.UpgradedNose = upgradedNose;
            visuals.DefaultEngine = defaultEngine;
            visuals.UpgradedEngine = upgradedEngine;
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
            GameObject root = new GameObject("Enemy_01");
            root.tag = GameTags.Enemy;
            root.transform.SetParent(_threatRoot, false);
            root.transform.position = position;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.drag = 1f;
            body.angularDrag = 2f;
            body.constraints = RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 2;
            collider.radius = 0.45f;
            collider.height = EnemyMeters;

            CreatePrimitive(PrimitiveType.Capsule, "Mesh", root.transform, _enemy,
                Vector3.zero, new Vector3(0.7f, 0.7f, 0.7f), Quaternion.Euler(90f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Cube, "Canard", root.transform, _accent,
                new Vector3(0f, 0.15f, -0.35f), new Vector3(1.4f, 0.08f, 0.35f), Quaternion.identity);

            EnemySeeker seeker = root.AddComponent<EnemySeeker>();
            seeker.Initialize(player, waves);
            return seeker;
        }

        public Projectile SpawnProjectile(Vector3 origin, Vector3 direction, float speed, int damage)
        {
            GameObject root = new GameObject("Projectile");
            root.tag = GameTags.Projectile;
            root.transform.SetParent(_projectileRoot, false);
            root.transform.position = origin;
            root.layer = 0;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.18f;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;

            CreatePrimitive(PrimitiveType.Sphere, "Mesh", root.transform, _projectile,
                Vector3.zero, new Vector3(0.22f, 0.22f, 0.55f), Quaternion.identity);

            Projectile projectile = root.AddComponent<Projectile>();
            projectile.Launch(direction, speed, damage);
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
            bool variantB = Random.value < 0.45f;
            string propName = size == AsteroidSize.Large
                ? (variantB ? "Asteroid_VariantB_Large" : "Asteroid_Large")
                : (variantB ? "Asteroid_VariantB_Small" : "Asteroid_Small");
            GameObject root = new GameObject(propName);
            root.tag = GameTags.Asteroid;
            root.transform.SetParent(_threatRoot, false);
            root.transform.position = position;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.drag = 0.05f;
            body.angularDrag = 0.05f;
            body.constraints = RigidbodyConstraints.FreezePositionY;
            body.mass = size == AsteroidSize.Large ? 4f : 1.2f;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.radius = meters * 0.5f;

            float wobble = size == AsteroidSize.Large ? 0.18f : 0.12f;
            Vector3 scale = new Vector3(
                meters * (1f + Random.Range(-wobble, wobble)),
                meters * (variantB ? 0.7f : 0.82f),
                meters * (1f + Random.Range(-wobble, wobble)));
            CreatePrimitive(variantB ? PrimitiveType.Cube : PrimitiveType.Sphere, "Mesh", root.transform, _asteroid,
                Vector3.zero, scale, Quaternion.identity);

            Asteroid asteroid = root.AddComponent<Asteroid>();
            asteroid.Initialize(size, waves, this, drift);
            return asteroid;
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
