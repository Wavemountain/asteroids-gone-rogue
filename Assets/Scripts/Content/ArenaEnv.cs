using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar-safe space shell: parallax stars, nebula dome, asteroid belt, cyan grid fade.
    /// Lives under the arena root so world swaps retint instead of rebuilding the sky.
    /// </summary>
    public sealed class ArenaEnv : MonoBehaviour
    {
        public const float NebulaOpacity = 0.38f;
        public const float NebulaInnerOpacity = 0.2f;
        public const float NebulaRadiusScale = 3f;
        public const float NebulaInnerRadiusScale = 2.15f;
        public const float BeltRadiusScale = 1.35f;
        public const float BeltOuterRadiusScale = 1.52f;
        public const float BeltSpinDegrees = 3.8f;
        public const string StarAKey = "Art/Env/Starfield_A";
        public const string StarBKey = "Art/Env/Starfield_B";
        public const string NebulaBlueKey = "Art/Env/Nebula_Blue";
        public const string NebulaPurpleKey = "Art/Env/Nebula_Purple";

        private static readonly string[] BeltRocks =
        {
            "Asteroid_Large",
            "Asteroid_VariantB_Large",
            "Asteroid_VariantC_Large",
            "Asteroid_VariantD_Large"
        };

        private Material _starFar;
        private Material _starMid;
        private Material _starNear;
        private Material _nebula;
        private Material _nebulaInner;
        private Material _grid;
        private Material _dust;
        private Transform _belt;
        private Transform[] _beltSlots;
        private Texture2D _nebulaBlue;
        private Texture2D _nebulaPurple;

        public static ArenaEnv Ensure(Transform arenaRoot)
        {
            if (arenaRoot == null)
            {
                return null;
            }

            Transform existing = arenaRoot.Find("ArenaEnv");
            if (existing != null)
            {
                return existing.GetComponent<ArenaEnv>();
            }

            GameObject go = new GameObject("ArenaEnv");
            go.transform.SetParent(arenaRoot, false);
            ArenaEnv env = go.AddComponent<ArenaEnv>();
            env.Build();
            return env;
        }

        public void Retint(int world)
        {
            int index = world < 1 ? 1 : world;
            Color star = new Color(0.82f, 0.9f, 1f, 1f);
            Color nebula = new Color(0.42f, 0.58f, 1f, NebulaOpacity);
            Color grid = new Color(0.2f, 0.85f, 1f, 0.1f);
            Texture2D nebulaTex = _nebulaBlue;
            switch (index % 7)
            {
                case 2:
                    star = new Color(1f, 0.82f, 0.7f, 1f);
                    nebula = new Color(0.85f, 0.28f, 0.55f, 0.34f);
                    grid = new Color(1f, 0.45f, 0.18f, 0.14f);
                    nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
                    break;
                case 3:
                    star = new Color(0.7f, 1f, 0.92f, 1f);
                    nebula = new Color(0.15f, 0.75f, 0.62f, 0.3f);
                    grid = new Color(0.2f, 1f, 0.75f, 0.15f);
                    break;
                case 4:
                    star = new Color(1f, 0.88f, 0.55f, 1f);
                    nebula = new Color(0.95f, 0.5f, 0.12f, 0.3f);
                    grid = new Color(1f, 0.7f, 0.2f, 0.14f);
                    nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
                    break;
                case 5:
                    star = new Color(0.78f, 1f, 0.7f, 1f);
                    nebula = new Color(0.35f, 0.9f, 0.28f, 0.28f);
                    grid = new Color(0.45f, 1f, 0.3f, 0.15f);
                    break;
                case 6:
                    star = new Color(1f, 0.72f, 0.62f, 1f);
                    nebula = new Color(0.62f, 0.22f, 0.12f, 0.36f);
                    grid = new Color(0.85f, 0.35f, 0.15f, 0.14f);
                    nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
                    break;
            }

            ApplyTint(_starFar, star * 0.55f);
            ApplyTint(_starMid, star * 0.88f);
            ApplyTint(_starNear, star * 1.08f);
            if (_nebula != null)
            {
                _nebula.color = nebula;
                if (nebulaTex != null)
                {
                    _nebula.mainTexture = nebulaTex;
                }
            }

            if (_nebulaInner != null)
            {
                Color inner = nebula;
                inner.a = NebulaInnerOpacity;
                inner.r = Mathf.Clamp01(inner.r + 0.12f);
                _nebulaInner.color = inner;
                Texture2D innerTex = nebulaTex == _nebulaBlue ? _nebulaPurple : _nebulaBlue;
                if (innerTex != null)
                {
                    _nebulaInner.mainTexture = innerTex;
                }
            }

            if (_grid != null)
            {
                _grid.color = grid;
            }

            if (_dust != null)
            {
                _dust.color = new Color(star.r, star.g, star.b, 0.1f);
            }
        }

        private void LateUpdate()
        {
            Scroll(_starFar, 0.007f, 0.0024f);
            Scroll(_starMid, -0.016f, 0.006f);
            Scroll(_starNear, 0.032f, -0.007f);
            Scroll(_nebula, -0.008f, 0.0045f);
            Scroll(_nebulaInner, 0.011f, -0.006f);
            if (_belt != null)
            {
                _belt.Rotate(0f, BeltSpinDegrees * Time.unscaledDeltaTime, 0f, Space.World);
            }

            if (_beltSlots != null)
            {
                float tumble = 18f * Time.unscaledDeltaTime;
                for (int i = 0; i < _beltSlots.Length; i++)
                {
                    if (_beltSlots[i] != null)
                    {
                        _beltSlots[i].Rotate(tumble * (0.4f + (i % 3) * 0.25f), tumble, -tumble * 0.35f, Space.Self);
                    }
                }
            }
        }

        private void Build()
        {
            float radius = WaveManager.ArenaRadius;
            Texture2D starA = Resources.Load<Texture2D>(StarAKey);
            Texture2D starB = Resources.Load<Texture2D>(StarBKey);
            _nebulaBlue = Resources.Load<Texture2D>(NebulaBlueKey);
            _nebulaPurple = Resources.Load<Texture2D>(NebulaPurpleKey);

            _starFar = UnlitLayer("Mat_Env_StarFar", starA != null ? starA : starB, new Color(0.62f, 0.72f, 0.92f, 1f), 3000, 2.35f);
            _starMid = UnlitLayer("Mat_Env_StarMid", starB != null ? starB : starA, new Color(0.88f, 0.92f, 1f, 1f), 3010, 1.65f);
            _starNear = UnlitLayer("Mat_Env_StarNear", starA != null ? starA : starB, Color.white, 3020, 1.12f);
            _nebula = UnlitTransparent(
                "Mat_Env_Nebula",
                _nebulaBlue,
                new Color(0.42f, 0.58f, 1f, NebulaOpacity),
                3100);
            _nebulaInner = UnlitTransparent(
                "Mat_Env_NebulaInner",
                _nebulaPurple != null ? _nebulaPurple : _nebulaBlue,
                new Color(0.7f, 0.35f, 0.95f, NebulaInnerOpacity),
                3110);
            _grid = UnlitTransparent("Mat_Env_Grid", MakeGridTexture(), new Color(0.2f, 0.85f, 1f, 0.1f), 2900);
            _dust = UnlitTransparent("Mat_Env_Dust", starB != null ? starB : starA, new Color(0.7f, 0.8f, 1f, 0.1f), 3050);

            CreateDome("StarFar", radius * 8.8f, _starFar);
            CreateDome("StarMid", radius * 7.5f, _starMid);
            CreateDome("StarNear", radius * 6.4f, _starNear);
            CreateDome("NebulaDome", radius * NebulaRadiusScale, _nebula);
            CreateDome("NebulaInner", radius * NebulaInnerRadiusScale, _nebulaInner);
            CreateDustRing(radius * 2.35f);
            CreateGrid(radius);
            CreateBelt(radius * BeltRadiusScale);
        }

        private void CreateDome(string name, float radius, Material material)
        {
            GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = name;
            dome.transform.SetParent(transform, false);
            dome.transform.localScale = new Vector3(-radius * 2f, radius * 2f, radius * 2f);
            Collider collider = dome.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = dome.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void CreateGrid(float radius)
        {
            GameObject grid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grid.name = "AstroGrid";
            grid.transform.SetParent(transform, false);
            grid.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            grid.transform.localScale = new Vector3(radius * 2.02f, 0.01f, radius * 2.02f);
            Collider collider = grid.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = grid.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _grid;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void CreateBelt(float radius)
        {
            GameObject root = new GameObject("AsteroidBelt");
            root.transform.SetParent(transform, false);
            _belt = root.transform;
            int count = 16;
            _beltSlots = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                float ring = (i % 2 == 0) ? radius : radius * (BeltOuterRadiusScale / BeltRadiusScale);
                float angle = (Mathf.PI * 2f * i) / count + (i % 2) * 0.18f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * ring, 0.4f + Mathf.Sin(i * 1.35f) * 2.2f, Mathf.Sin(angle) * ring);
                string rock = BeltRocks[i % BeltRocks.Length];
                Transform slot = new GameObject("Belt_" + i).transform;
                slot.SetParent(_belt, false);
                slot.position = pos;
                slot.localRotation = Quaternion.Euler(18f * i, 40f * i, 12f * i);
                slot.localScale = Vector3.one * (0.72f + (i % 4) * 0.14f);
                _beltSlots[i] = slot;
                GameObject instance;
                if (!ArtImport.TryInstantiate(rock, slot, KeepImported, null, out instance))
                {
                    GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    fallback.name = "BeltRock";
                    fallback.transform.SetParent(slot, false);
                    fallback.transform.localScale = Vector3.one * 1.6f;
                    Collider collider = fallback.GetComponent<Collider>();
                    if (collider != null)
                    {
                        Destroy(collider);
                    }
                }
            }
        }

        private static Material KeepImported(string meshName, Material imported)
        {
            return imported;
        }

        private static void Scroll(Material material, float x, float y)
        {
            if (material == null || material.mainTexture == null)
            {
                return;
            }

            Vector2 offset = material.mainTextureOffset;
            offset.x += x * Time.unscaledDeltaTime;
            offset.y += y * Time.unscaledDeltaTime;
            material.mainTextureOffset = offset;
        }

        private static void ApplyTint(Material material, Color color)
        {
            if (material != null)
            {
                material.color = color;
            }
        }

        private void CreateDustRing(float radius)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "DustRing";
            ring.transform.SetParent(transform, false);
            ring.transform.localScale = new Vector3(radius * 2f, 0.08f, radius * 2f);
            Collider collider = ring.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = ring.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _dust;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material UnlitLayer(string name, Texture2D texture, Color tint, int queue, float tiling)
        {
            Shader shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = tint;
            if (texture != null)
            {
                material.mainTexture = texture;
                material.mainTextureScale = new Vector2(tiling, tiling);
            }

            material.renderQueue = queue;
            return material;
        }

        private static Material UnlitTransparent(string name, Texture2D texture, Color tint, int queue)
        {
            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = tint;
            if (texture != null)
            {
                material.mainTexture = texture;
            }

            material.renderQueue = queue;
            return material;
        }

        private static Texture2D MakeGridTexture()
        {
            const int Size = 64;
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Color clear = new Color(0.15f, 0.85f, 1f, 0f);
            Color line = new Color(0.25f, 0.95f, 1f, 0.85f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float nx = (x / (float)(Size - 1)) * 2f - 1f;
                    float ny = (y / (float)(Size - 1)) * 2f - 1f;
                    float fade = 1f - Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));
                    bool grid = x % 8 == 0 || y % 8 == 0;
                    Color c = grid ? line : clear;
                    c.a *= fade;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.name = "AstroGridFade";
            return tex;
        }
    }
}
