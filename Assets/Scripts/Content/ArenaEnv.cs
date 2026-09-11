using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar-safe space shell: parallax stars, nebula dome, asteroid belt, cyan grid fade.
    /// Lives under the arena root so world swaps retint instead of rebuilding the sky.
    /// </summary>
    public sealed class ArenaEnv : MonoBehaviour
    {
        public const float NebulaOpacity = 0.34f;
        public const float NebulaInnerOpacity = 0.18f;
        public const float RetintChroma = 0.55f;
        public const float NebulaRadiusScale = 3.8f;
        public const float NebulaInnerRadiusScale = 2.4f;
        public const float BeltRadiusScale = 1.58f;
        public const float BeltOuterRadiusScale = 1.68f;
        public const float BeltSpinDegrees = 6f;
        public const int BeltCount = 22;
        public const float StarFarTint = 0.45f;
        public const float StarNearTint = 1.05f;
        public const float StarFarTiling = 3.6f;
        public const float GridFadeRadiusScale = 0.7f;
        public const float GridAlpha = 0.045f;
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
        private Transform _belt;
        private Transform[] _beltSlots;
        private Material _beltTint;
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
            Color star = Soften(new Color(0.82f, 0.9f, 1f, 1f));
            Color nebula = Soften(new Color(0.42f, 0.58f, 1f, NebulaOpacity));
            Color grid = Soften(new Color(0.2f, 0.85f, 1f, GridAlpha));
            Texture2D nebulaTex = _nebulaBlue;
            switch (index % 7)
            {
                case 2:
                    star = Soften(new Color(0.86f, 0.8f, 0.84f, 1f));
                    nebula = Soften(new Color(0.46f, 0.4f, 0.5f, NebulaOpacity));
                    grid = Soften(new Color(0.52f, 0.5f, 0.58f, GridAlpha));
                    nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
                    break;
                case 3:
                    star = Soften(new Color(0.72f, 0.88f, 0.9f, 1f));
                    nebula = Soften(new Color(0.22f, 0.52f, 0.55f, NebulaOpacity));
                    grid = Soften(new Color(0.32f, 0.62f, 0.68f, GridAlpha));
                    break;
                case 4:
                    star = Soften(new Color(0.92f, 0.84f, 0.7f, 1f));
                    nebula = Soften(new Color(0.62f, 0.46f, 0.28f, NebulaOpacity));
                    grid = Soften(new Color(0.7f, 0.58f, 0.38f, GridAlpha));
                    nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
                    break;
                case 5:
                    star = Soften(new Color(0.78f, 0.84f, 0.9f, 1f));
                    nebula = Soften(new Color(0.3f, 0.42f, 0.52f, NebulaOpacity));
                    grid = Soften(new Color(0.4f, 0.55f, 0.64f, GridAlpha));
                    break;
                case 6:
                    star = Soften(new Color(0.9f, 0.78f, 0.72f, 1f));
                    nebula = Soften(new Color(0.48f, 0.32f, 0.28f, NebulaOpacity));
                    grid = Soften(new Color(0.58f, 0.42f, 0.36f, GridAlpha));
                    nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
                    break;
            }

            ApplyTint(_starFar, star * StarFarTint);
            ApplyTint(_starMid, star * 0.8f);
            ApplyTint(_starNear, star * StarNearTint);
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
        }

        private void LateUpdate()
        {
            Scroll(_starFar, 0.01f, 0.0036f);
            Scroll(_starMid, -0.02f, 0.0075f);
            Scroll(_starNear, 0.035f, -0.008f);
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

            _starFar = UnlitLayer("Mat_Env_StarFar", starA != null ? starA : starB, new Color(StarFarTint, StarFarTint, StarFarTint, 0.85f), 3000, StarFarTiling);
            _starMid = UnlitLayer("Mat_Env_StarMid", starB != null ? starB : starA, new Color(0.72f, 0.78f, 0.92f, 0.9f), 3010, 1.65f);
            _starNear = UnlitLayer("Mat_Env_StarNear", starA != null ? starA : starB, new Color(StarNearTint, StarNearTint, StarNearTint, 1f), 3020, 1.05f);
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
            _grid = UnlitTransparent("Mat_Env_Grid", MakeGridTexture(), new Color(0.2f, 0.85f, 1f, GridAlpha), 2900);

            CreateDome("StarFar", radius * 8.8f, _starFar);
            CreateDome("StarMid", radius * 7.5f, _starMid);
            CreateDome("StarNear", radius * 6.4f, _starNear);
            CreateDome("NebulaDome", radius * NebulaRadiusScale, _nebula);
            CreateDome("NebulaInner", radius * NebulaInnerRadiusScale, _nebulaInner);
            CreateGrid(radius);
            CreatePlatformLip(radius);
            CreateBelt(radius * BeltRadiusScale);
            CreateRimLight();
            CreateUnderGlow(radius);
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
            float gridScale = radius * 2f * GridFadeRadiusScale;
            grid.transform.localScale = new Vector3(gridScale, 0.01f, gridScale);
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

        private void CreatePlatformLip(float radius)
        {
            GameObject go = new GameObject("ArenaLip");
            go.transform.SetParent(transform, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 72;
            line.widthMultiplier = 0.38f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Additive");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.name = "Mat_Env_Lip";
            material.color = new Color(0.42f, 0.88f, 1f, 1f);
            line.sharedMaterial = material;
            line.startColor = material.color;
            line.endColor = material.color;
            for (int i = 0; i < 72; i++)
            {
                float ang = (i / 72f) * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(ang) * radius, 0.09f, Mathf.Sin(ang) * radius));
            }
        }

        private void CreateBelt(float radius)
        {
            GameObject root = new GameObject("AsteroidBelt");
            root.transform.SetParent(transform, false);
            _belt = root.transform;
            int count = BeltCount;
            _beltSlots = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                float ring = (i % 2 == 0) ? radius : radius * (BeltOuterRadiusScale / BeltRadiusScale);
                float angle = (Mathf.PI * 2f * i) / count + (i % 2) * 0.18f;
                float y = Mathf.Lerp(-0.5f, 3.2f, ((i * 3) % 11) / 10f);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * ring, y, Mathf.Sin(angle) * ring);
                string rock = BeltRocks[i % BeltRocks.Length];
                Transform slot = new GameObject("Belt_" + i).transform;
                slot.SetParent(_belt, false);
                slot.position = pos;
                slot.localRotation = Quaternion.Euler(18f * i, 40f * i, 12f * i);
                slot.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.4f, (i % 5) / 4f);
                _beltSlots[i] = slot;
                _beltTint = DarkBeltMaterial(i);
                GameObject instance;
                if (!ArtImport.TryInstantiate(rock, slot, UseBeltTint, _beltTint, out instance))
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

                    MeshRenderer renderer = fallback.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = _beltTint;
                    }
                }
            }
        }

        private Material UseBeltTint(string meshName, Material imported)
        {
            return _beltTint != null ? _beltTint : imported;
        }

        private static Material DarkBeltMaterial(int index)
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            float t = (index % 4) / 3f;
            Color tint = Color.Lerp(new Color(0.055f, 0.058f, 0.07f), new Color(0.12f, 0.11f, 0.13f), t);
            Material material = new Material(shader);
            material.name = "Mat_Env_Belt";
            material.color = tint;
            return material;
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

        private static Color Soften(Color color)
        {
            float y = 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
            return new Color(
                y + (color.r - y) * RetintChroma,
                y + (color.g - y) * RetintChroma,
                y + (color.b - y) * RetintChroma,
                color.a);
        }

        private void CreateRimLight()
        {
            Transform existing = transform.Find("ArenaRimLight");
            if (existing != null)
            {
                return;
            }

            GameObject rimGo = new GameObject("ArenaRimLight");
            rimGo.transform.SetParent(transform, false);
            Light rim = rimGo.AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.color = new Color(0.42f, 0.7f, 1f);
            rim.intensity = 0.32f;
            rim.shadows = LightShadows.None;
            rimGo.transform.rotation = Quaternion.Euler(16f, 214f, 0f);
        }

        private void CreateUnderGlow(float radius)
        {
            Transform existing = transform.Find("ArenaUnderGlow");
            if (existing != null)
            {
                return;
            }

            GameObject glowGo = new GameObject("ArenaUnderGlow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = new Vector3(0f, -4.8f, 0f);
            Light glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.78f, 0.88f, 1f);
            glow.intensity = 2.1f;
            glow.range = radius * 1.35f;
            glow.shadows = LightShadows.None;
        }

        private static Material UnlitLayer(string name, Texture2D texture, Color tint, int queue, float tiling)
        {
            Shader shader = Shader.Find("Particles/Additive");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Texture");
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
            material.SetInt("_ZWrite", 0);
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
