using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Pooled debris, shock rings, and muzzle cards, plus three particle
    /// systems whose caps sum to <see cref="VfxBudget.MaxParticles"/>.
    /// Slots are built once. A full pool steals the oldest slot.
    /// The daily roll is not read here.
    /// </summary>
    public sealed class VfxKit : MonoBehaviour
    {
        private static VfxKit _live;

        private VfxSlot[] _debris;
        private VfxSlot[] _rings;
        private VfxSlot[] _muzzles;
        private ParticleSystem _chunks;
        private ParticleSystem _smoke;
        private ParticleSystem _sparks;
        private Material _ringShared;
        private Material _debrisShared;
        private Material _muzzleShared;
        private Material _chunkMat;
        private Material _smokeMat;
        private Material _sparkMat;

        public static void Explode(Vector3 position, int size, bool reduce)
        {
            VfxKit kit = Ensure();
            if (kit == null)
            {
                return;
            }

            kit.PlayExplosion(position, size, reduce);
        }

        public static void Shock(Vector3 position, float startScale, float endScale, float seconds, bool reduce)
        {
            VfxKit kit = Ensure();
            if (kit == null)
            {
                return;
            }

            kit.PlayShock(position, startScale, endScale, seconds, reduce);
        }

        /// <summary>
        /// Ages slots that were spawned this instant so a paused capture
        /// still shows debris and a shock ring, not a point at the origin.
        /// </summary>
        public static void PresentFresh(float age01)
        {
            if (_live == null)
            {
                return;
            }

            _live.PoseFresh(age01);
        }

        public static void Muzzle(Vector3 position, Vector3 forward, int mode, bool reduce)
        {
            VfxKit kit = Ensure();
            if (kit == null)
            {
                return;
            }

            kit.PlayMuzzle(position, forward, mode, reduce);
        }

        public static void Sparks(Vector3 position, int count, Color color, bool reduce)
        {
            VfxKit kit = Ensure();
            if (kit == null)
            {
                return;
            }

            int scaled = VfxBudget.ScaledCount(count, reduce);
            float size = VfxBudget.ScaledSize(0.08f, reduce);
            kit.Emit(kit._sparks, position, Vector3.up, 0.15f, size, color, scaled);
        }

        private static VfxKit Ensure()
        {
            if (_live != null)
            {
                return _live;
            }

            GameObject root = new GameObject("VfxKit");
            _live = root.AddComponent<VfxKit>();
            return _live;
        }

        private void Awake()
        {
            _live = this;
            BuildMaterials();
            _debris = BuildSlots(VfxBudget.DebrisCap, "Debris", true);
            _rings = BuildSlots(VfxBudget.ShockCap, "Shock", false);
            _muzzles = BuildSlots(VfxBudget.MuzzleCap, "Muzzle", false);
            _chunks = MakeParticles("Chunks", VfxBudget.ChunkParticles, _chunkMat);
            _smoke = MakeParticles("Smoke", VfxBudget.SmokeParticles, _smokeMat);
            _sparks = MakeParticles("Sparks", VfxBudget.SparkParticles, _sparkMat);
        }

        private void LateUpdate()
        {
            float now = Time.time;
            Camera camera = Camera.main;
            Tick(_debris, now, camera, false);
            Tick(_rings, now, camera, false);
            Tick(_muzzles, now, camera, true);
        }

        private void PlayExplosion(Vector3 position, int size, bool reduce)
        {
            int chunks = VfxBudget.Chunks(size, reduce);
            int smokes = VfxBudget.Smokes(size, reduce);
            float smokeLife = VfxBudget.SmokeSeconds(size, reduce);
            float chunkSize = 0.14f;
            if (size == VfxBudget.Small)
            {
                chunkSize = 0.10f;
            }
            else if (size == VfxBudget.Large)
            {
                chunkSize = 0.18f;
            }

            chunkSize = VfxBudget.ScaledSize(chunkSize, reduce);
            int piece = 0;
            while (piece < chunks)
            {
                VfxSlot slot = Take(_debris);
                if (slot == null)
                {
                    break;
                }

                float angle = piece * 2.399963f;
                Vector3 drift = new Vector3(Mathf.Cos(angle), 0.15f, Mathf.Sin(angle));
                float speed = 4.5f;
                if (size == VfxBudget.Mid)
                {
                    speed = 6f;
                }
                else if (size == VfxBudget.Large)
                {
                    speed = 8f;
                }

                Activate(slot, position, 0.35f + (size * 0.15f), chunkSize, chunkSize, drift, speed, 0);
                piece++;
            }

            Color ember = new Color(VfxBudget.EmberR, VfxBudget.EmberG, VfxBudget.EmberB, 1f);
            Color smoke = new Color(VfxBudget.SmokeR, VfxBudget.SmokeG, VfxBudget.SmokeB, VfxBudget.SmokeAlpha);
            Emit(_chunks, position, Vector3.up, 0.35f, chunkSize, ember, chunks);
            Emit(_smoke, position, Vector3.up * 0.4f, smokeLife, VfxBudget.ScaledSize(0.7f, reduce), smoke, smokes);
            if (size == VfxBudget.Large)
            {
                PlayShock(position, 0.4f, 2.4f, 0.4f, reduce);
            }
        }

        private void PlayShock(Vector3 position, float startScale, float endScale, float seconds, bool reduce)
        {
            VfxSlot slot = Take(_rings);
            if (slot == null)
            {
                return;
            }

            float start = VfxBudget.ScaledSize(startScale, reduce);
            float end = VfxBudget.ScaledSize(endScale, reduce);
            Activate(slot, position, seconds, start, end, Vector3.zero, 0f, 0);
            slot.Root.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private void PlayMuzzle(Vector3 position, Vector3 forward, int mode, bool reduce)
        {
            int cards = VfxBudget.MuzzleCards(mode);
            float life = VfxBudget.MuzzleSeconds(mode);
            float scale = VfxBudget.ScaledSize(VfxBudget.MuzzleScale(mode), reduce);
            Vector3 aim = forward;
            if (aim.sqrMagnitude < 0.0001f)
            {
                aim = Vector3.forward;
            }

            aim.Normalize();
            int card = 0;
            while (card < cards)
            {
                VfxSlot slot = Take(_muzzles);
                if (slot == null)
                {
                    break;
                }

                Vector3 side = new Vector3(-aim.z, 0f, aim.x);
                float offset = 0f;
                if (cards > 1)
                {
                    offset = card == 0 ? -0.18f : 0.18f;
                }

                Vector3 at = position + (side * offset);
                Activate(slot, at, life, scale, scale, aim, 0f, VfxBudget.Billboard);
                card++;
            }

            int sparks = VfxBudget.MuzzleSparks(mode, reduce);
            Color spark = new Color(VfxBudget.HotR, VfxBudget.HotG, VfxBudget.HotB, 1f);
            Emit(_sparks, position, aim * 6f, 0.12f, VfxBudget.ScaledSize(0.06f, reduce), spark, sparks);
        }

        private void Activate(VfxSlot slot, Vector3 position, float life, float startScale, float endScale, Vector3 drift, float speed, int billboard)
        {
            if (slot.Root == null)
            {
                return;
            }

            slot.Live = 1;
            slot.Billboard = billboard;
            slot.Life = life < 0.02f ? 0.02f : life;
            slot.Born = Time.time;
            slot.Until = slot.Born + slot.Life;
            slot.StartScale = startScale;
            slot.EndScale = endScale;
            slot.Origin = position;
            slot.Drift = drift;
            slot.DriftSpeed = speed;
            slot.Root.SetActive(true);
            slot.Root.transform.position = position;
            slot.Root.transform.localScale = new Vector3(startScale, startScale, startScale);
            ApplyTint(slot, 1f);
        }

        private void PoseFresh(float age01)
        {
            float shown = age01;
            if (shown < 0f)
            {
                shown = 0f;
            }

            if (shown > 0.85f)
            {
                shown = 0.85f;
            }

            float now = Time.time;
            PoseFreshPool(_debris, now, shown, true);
            PoseFreshPool(_rings, now, shown, false);
        }

        private void PoseFreshPool(VfxSlot[] pool, float now, float shown, bool drift)
        {
            if (pool == null)
            {
                return;
            }

            int index = 0;
            while (index < pool.Length)
            {
                VfxSlot slot = pool[index];
                index++;
                if (slot == null || slot.Live == 0 || slot.Root == null)
                {
                    continue;
                }

                if (now - slot.Born > 0.05f)
                {
                    continue;
                }

                float life = slot.Life;
                if (life < 0.02f)
                {
                    life = 0.02f;
                }

                float age = life * shown;
                slot.Born = now - age;
                slot.Until = slot.Born + life;
                float scale = VfxBudget.ShockScale(shown, slot.StartScale, slot.EndScale);
                slot.Root.transform.localScale = new Vector3(scale, scale, scale);
                if (drift && slot.DriftSpeed > 0f)
                {
                    slot.Root.transform.position = slot.Origin + (slot.Drift * (slot.DriftSpeed * age));
                }

                ApplyTint(slot, 1f - shown);
            }
        }

        private void Tick(VfxSlot[] pool, float now, Camera camera, bool billboard)
        {
            if (pool == null)
            {
                return;
            }

            int index = 0;
            while (index < pool.Length)
            {
                VfxSlot slot = pool[index];
                index++;
                if (slot == null || slot.Live == 0 || slot.Root == null)
                {
                    continue;
                }

                float life = slot.Life;
                if (life < 0.02f)
                {
                    life = 0.02f;
                }

                float age = now - slot.Born;
                float u = age / life;
                if (u >= 1f)
                {
                    slot.Live = 0;
                    slot.Root.SetActive(false);
                    continue;
                }

                float scale = VfxBudget.ShockScale(u, slot.StartScale, slot.EndScale);
                slot.Root.transform.localScale = new Vector3(scale, scale, scale);
                if (slot.DriftSpeed > 0f)
                {
                    slot.Root.transform.position = slot.Origin + (slot.Drift * (slot.DriftSpeed * age));
                }

                if (billboard && slot.Billboard != 0 && camera != null)
                {
                    Vector3 toCamera = camera.transform.position - slot.Root.transform.position;
                    if (toCamera.sqrMagnitude > 0.0001f)
                    {
                        slot.Root.transform.rotation = Quaternion.LookRotation(-toCamera, Vector3.up);
                    }
                }

                ApplyTint(slot, 1f - u);
            }
        }

        private void ApplyTint(VfxSlot slot, float alpha)
        {
            if (slot.Tint == null)
            {
                return;
            }

            float fade = alpha;
            if (fade < 0f)
            {
                fade = 0f;
            }

            if (slot.Billboard != 0 && SettingsState.ReduceEffectsEnabled)
            {
                fade *= VfxBudget.MuzzleAlphaMul;
            }

            Color baseColor = slot.BaseColor;
            Color faded = new Color(baseColor.r * fade, baseColor.g * fade, baseColor.b * fade, fade);
            slot.Tint.color = faded;
            slot.Tint.SetColor("_EmissionColor", faded);
        }

        private VfxSlot Take(VfxSlot[] pool)
        {
            if (pool == null || pool.Length == 0)
            {
                return null;
            }

            VfxSlot oldest = pool[0];
            int index = 0;
            while (index < pool.Length)
            {
                VfxSlot slot = pool[index];
                if (slot != null && slot.Live == 0)
                {
                    return slot;
                }

                if (slot != null && oldest != null && slot.Until < oldest.Until)
                {
                    oldest = slot;
                }

                index++;
            }

            return oldest;
        }

        private VfxSlot[] BuildSlots(int count, string label, bool debris)
        {
            VfxSlot[] pool = new VfxSlot[count];
            int index = 0;
            while (index < count)
            {
                VfxSlot slot = new VfxSlot();
                GameObject host = new GameObject(label + index);
                host.transform.SetParent(transform, false);
                host.SetActive(false);
                Material shared = _muzzleShared;
                string meshName = VfxBudget.MuzzleMesh;
                if (debris)
                {
                    shared = _debrisShared;
                    meshName = VfxBudget.DebrisName(index);
                }
                else if (label == "Shock")
                {
                    shared = _ringShared;
                    meshName = VfxBudget.ShockMesh;
                }

                GameObject visual;
                if (!ArtImport.TryInstantiate(meshName, host.transform, null, shared, out visual))
                {
                    visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    visual.name = meshName;
                    visual.transform.SetParent(host.transform, false);
                    Collider collider = visual.GetComponent<Collider>();
                    if (collider != null)
                    {
                        Destroy(collider);
                    }

                    Renderer fallback = visual.GetComponent<Renderer>();
                    if (fallback != null && shared != null)
                    {
                        fallback.sharedMaterial = shared;
                    }
                }

                Material tint = null;
                if (shared != null)
                {
                    tint = new Material(shared);
                    tint.name = shared.name;
                    Renderer[] renderers = host.GetComponentsInChildren<Renderer>(true);
                    int rendererIndex = 0;
                    while (rendererIndex < renderers.Length)
                    {
                        if (renderers[rendererIndex] != null)
                        {
                            renderers[rendererIndex].sharedMaterial = tint;
                        }

                        rendererIndex++;
                    }
                }

                slot.Root = host;
                slot.Tint = tint;
                slot.BaseColor = shared != null ? shared.color : Color.white;
                slot.Live = 0;
                pool[index] = slot;
                index++;
            }

            return pool;
        }

        private ParticleSystem MakeParticles(string name, int cap, Material material)
        {
            GameObject host = new GameObject(name);
            host.transform.SetParent(transform, false);
            ParticleSystem system = host.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.maxParticles = cap;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;
            system.Stop();
            system.Clear();
            ParticleSystemRenderer renderer = host.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return system;
        }

        private void Emit(ParticleSystem system, Vector3 position, Vector3 velocity, float life, float size, Color color, int count)
        {
            if (system == null || count < 1)
            {
                return;
            }

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams();
            emit.position = position;
            emit.velocity = velocity;
            emit.startLifetime = life;
            emit.startSize = size;
            emit.startColor = color;
            system.Emit(emit, count);
        }

        private void BuildMaterials()
        {
            Color steel = new Color(EmissionTargets.SteelR, EmissionTargets.SteelG, EmissionTargets.SteelB, 1f);
            Color ring = steel * EmissionTargets.FxRing;
            _ringShared = MakeMaterial(VfxBudget.RingMaterial, "Particles/Additive", ring);
            _debrisShared = MakeMaterial(
                "Mat_FX_Debris",
                "Particles/Standard Unlit",
                new Color(VfxBudget.EmberR, VfxBudget.EmberG, VfxBudget.EmberB, 1f));
            _muzzleShared = MakeMaterial(
                "Mat_FX_Muzzle",
                "Particles/Additive",
                new Color(EmissionTargets.PrimaryR, EmissionTargets.PrimaryG, EmissionTargets.PrimaryB, 1f));
            _chunkMat = _debrisShared;
            _sparkMat = _muzzleShared;
            _smokeMat = MakeMaterial(
                "Mat_FX_Smoke",
                "Particles/Standard Unlit",
                new Color(VfxBudget.SmokeR, VfxBudget.SmokeG, VfxBudget.SmokeB, VfxBudget.SmokeAlpha));
        }

        private static Material MakeMaterial(string name, string shaderName, Color color)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            if (name == VfxBudget.RingMaterial)
            {
                material.SetColor("_EmissionColor", color);
            }

            return material;
        }

        private sealed class VfxSlot
        {
            public GameObject Root;
            public Material Tint;
            public Color BaseColor;
            public Vector3 Origin;
            public Vector3 Drift;
            public float DriftSpeed;
            public float Born;
            public float Until;
            public float Life;
            public float StartScale;
            public float EndScale;
            public int Live;
            public int Billboard;
        }
    }
}
