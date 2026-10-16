using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar-safe space shell: parallax stars, nebula dome, asteroid belt, steel grid fade.
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
            "Asteroid_Frag_01",
            "Asteroid_Frag_02",
            "Asteroid_Frag_03",
            "Asteroid_Frag_04",
            "Asteroid_M_C",
            "Asteroid_L_A"
        };

        private Material _starFar;
        private Material _starMid;
        private Material _starNear;
        private Material _nebula;
        private Material _nebulaInner;
        private Material _grid;
        private Material _voidField;
        private Transform _belt;
        private Transform[] _beltSlots;
        private Material _beltTint;
        private Texture2D _nebulaBlue;
        private Texture2D _nebulaPurple;
        private Color _floorTint = new Color(0.04f, 0.14f, 0.42f, 1f);
        private Color _gridQuiet = new Color(1f, 1f, 1f, 0f);
        private int _world = 1;
        private float _nebulaBaseA = NebulaOpacity;
        private float _innerBaseA = NebulaInnerOpacity;
        private ParticleSystem _dust;
        private ParticleSystem.Particle[] _dustBuf;
        private Vector3 _dustCam;
        private int _dustReady;
        private Transform _player;

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
            WorldTint tint = WorldRules.TintForWorld(world);
            float bright = tint.Brightness;
            Color star = SoftenAmount(ScaleColor(new Color(tint.StarR, tint.StarG, tint.StarB, 1f), bright), tint.Chroma);
            Color nebula = SoftenAmount(ScaleColor(new Color(tint.NebulaR, tint.NebulaG, tint.NebulaB, NebulaOpacity), bright), tint.Chroma);
            nebula.a = NebulaOpacity;
            Color grid = SoftenAmount(ScaleColor(new Color(tint.GridR, tint.GridG, tint.GridB, PremiumLook.GridVisibleAlpha), bright), tint.Chroma);
            grid.a = PremiumLook.GridVisibleAlpha;
            _gridQuiet = grid;
            _gridQuiet.a = 0f;
            if (world == 3)
            {
                _gridQuiet.a = PremiumLook.W3GridQuietAlpha;
                float pulledR;
                float pulledG;
                float pulledB;
                PremiumLook.PullLumaTowardVoid(grid.r, grid.g, grid.b, PremiumLook.W3GridLumaKeep, out pulledR, out pulledG, out pulledB);
                grid.r = pulledR;
                grid.g = pulledG;
                grid.b = pulledB;
                grid.a = PremiumLook.W3GridAlpha;
            }
            _floorTint = ScaleColor(new Color(tint.FloorR, tint.FloorG, tint.FloorB, 1f), bright);
            Texture2D nebulaTex = _nebulaBlue;
            if (tint.PurpleNebula != 0)
            {
                nebulaTex = _nebulaPurple != null ? _nebulaPurple : _nebulaBlue;
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

            _world = world;
            _nebulaBaseA = NebulaOpacity;
            _innerBaseA = NebulaInnerOpacity;

            TintNamedLight("ArenaUnderGlow", star);
            ApplyWorldLight(world);
            PostStack.SetWorld(world, SettingsState.ReduceEffectsEnabled);
        }

        public void RestoreHangarKey()
        {
            ApplyKey(WorldLight.HangarKeyIntensity, WorldLight.HangarKeyR, WorldLight.HangarKeyG, WorldLight.HangarKeyB);
            Transform rimNode = transform.Find("ArenaRimLight");
            if (rimNode != null)
            {
                Light hangarRim = rimNode.GetComponent<Light>();
                if (hangarRim != null)
                {
                    hangarRim.intensity = WorldLight.LegacyRimIntensity;
                    hangarRim.color = new Color(0.42f, 0.7f, 1f, 1f);
                    hangarRim.shadows = LightShadows.None;
                }
            }

            Transform underNode = transform.Find("ArenaUnderGlow");
            if (underNode != null)
            {
                Light hangarUnder = underNode.GetComponent<Light>();
                if (hangarUnder != null)
                {
                    hangarUnder.intensity = WorldLight.LegacyUnderGlow;
                    hangarUnder.shadows = LightShadows.None;
                }
            }
        }

        private void ApplyWorldLight(int world)
        {
            WorldLightRow lit = WorldLight.Row(world);
            ApplyKey(lit.KeyIntensity, lit.KeyR, lit.KeyG, lit.KeyB);
            Transform rimNode = transform.Find("ArenaRimLight");
            if (rimNode != null)
            {
                Light worldRim = rimNode.GetComponent<Light>();
                if (worldRim != null)
                {
                    worldRim.intensity = lit.RimIntensity;
                    worldRim.shadows = LightShadows.None;
                    if (lit.RimIsSteel != 0)
                    {
                        worldRim.color = new Color(lit.RimR, lit.RimG, lit.RimB, 1f);
                    }
                    else
                    {
                        worldRim.color = _floorTint;
                    }
                }
            }

            Transform underNode = transform.Find("ArenaUnderGlow");
            if (underNode != null)
            {
                Light worldUnder = underNode.GetComponent<Light>();
                if (worldUnder != null)
                {
                    worldUnder.intensity = lit.UnderGlow;
                    worldUnder.shadows = LightShadows.None;
                }
            }
        }

        private void ApplyKey(float intensity, float red, float green, float blue)
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>();
            for (int lightIndex = 0; lightIndex < lights.Length; lightIndex++)
            {
                Light keyLight = lights[lightIndex];
                if (keyLight == null || keyLight.type != LightType.Directional)
                {
                    continue;
                }

                if (keyLight.gameObject != null && keyLight.gameObject.name == "ArenaRimLight")
                {
                    continue;
                }

                keyLight.intensity = intensity;
                float warmR;
                float warmG;
                float warmB;
                PremiumLook.WarmKey(red, green, blue, out warmR, out warmG, out warmB);
                keyLight.color = new Color(warmR, warmG, warmB, 1f);
                // Floor is on the decor camera, so this shadow map never lands. Contact blobs ground the bodies.
                keyLight.shadows = LightShadows.None;
                keyLight.shadowStrength = PremiumLook.ShadowStrength;
                keyLight.transform.rotation = Quaternion.Euler(PremiumLook.KeyElevation, PremiumLook.KeyYaw, 0f);
            }
        }

        public void TintFloor(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Renderer[] floorRenderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int floorIndex = 0; floorIndex < floorRenderers.Length; floorIndex++)
            {
                Renderer floorRenderer = floorRenderers[floorIndex];
                if (floorRenderer == null)
                {
                    continue;
                }

                string painted = floorRenderer.sharedMaterial != null ? floorRenderer.sharedMaterial.name : string.Empty;
                if (painted.IndexOf("W3_Beam", System.StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                float floorR;
                float floorG;
                float floorB;
                PremiumLook.FloorRgb(_world, _floorTint.r, _floorTint.g, _floorTint.b, out floorR, out floorG, out floorB);
                Material paintedMat = floorRenderer.material;
                Shader floorFade = Shader.Find("AGR/FloorFade");
                if (floorFade != null)
                {
                    paintedMat.shader = floorFade;
                    paintedMat.SetColor("_Void", new Color(PremiumLook.OutsideR, PremiumLook.OutsideG, PremiumLook.OutsideB, 1f));
                    float dome = WaveManager.ArenaRadius;
                    paintedMat.SetFloat("_FadeStart", dome * 0.82f);
                    paintedMat.SetFloat("_FadeEnd", dome);
                }

                paintedMat.color = new Color(floorR, floorG, floorB, 1f);
                if (paintedMat.HasProperty("_Glossiness"))
                {
                    paintedMat.SetFloat("_Glossiness", PremiumLook.FloorSmoothness);
                }

                if (paintedMat.HasProperty("_Metallic"))
                {
                    paintedMat.SetFloat("_Metallic", PremiumLook.FloorMetallic);
                }
            }
        }

        private void TintNamedLight(string lightName, Color color)
        {
            Transform lightNode = transform.Find(lightName);
            if (lightNode == null)
            {
                return;
            }

            Light worldLight = lightNode.GetComponent<Light>();
            if (worldLight != null)
            {
                worldLight.color = color;
            }
        }

        private void LateUpdate()
        {
            float time = Time.unscaledTime;
            bool reduce = SettingsState.ReduceEffectsEnabled;
            float camX = 0f;
            float camZ = 0f;
            Camera view = Camera.main;
            if (view != null)
            {
                camX = view.transform.position.x;
                camZ = view.transform.position.z;
            }

            float offsetX;
            float offsetY;
            ArenaDepth.Parallax(camX, camZ, time, ArenaDepth.StarFarK, ArenaDepth.StarFarDriftX, ArenaDepth.StarFarDriftY, reduce, out offsetX, out offsetY);
            Scroll(_starFar, offsetX, offsetY);
            ArenaDepth.Parallax(camX, camZ, time, ArenaDepth.StarMidK, ArenaDepth.StarMidDriftX, ArenaDepth.StarMidDriftY, reduce, out offsetX, out offsetY);
            Scroll(_starMid, offsetX, offsetY);
            ArenaDepth.Parallax(camX, camZ, time, ArenaDepth.StarNearK, ArenaDepth.StarNearDriftX, ArenaDepth.StarNearDriftY, reduce, out offsetX, out offsetY);
            Scroll(_starNear, offsetX, offsetY);
            float driftScale = reduce ? ArenaDepth.ReduceMotion : 1f;
            Scroll(_nebula, ArenaDepth.NebulaDrift * driftScale * time, 0f);
            Scroll(_nebulaInner, ArenaDepth.NebulaInnerDrift * driftScale * time, 0f);
            float breathe = ArenaDepth.Breathe(time, reduce);
            ApplyLayerAlpha(_nebula, _nebulaBaseA + breathe);
            ApplyLayerAlpha(_nebulaInner, _innerBaseA + breathe);
            float phase = 0f;
            if (AudioCues.Instance != null)
            {
                phase = AudioCues.Instance.DominantMusicSeconds();
            }

            float gridMul = ArenaDepth.GridAlphaMul(MusicPlan.TrackBpm(_world), phase, reduce);
            float resting = PremiumLook.GridVisibleAlpha;
            float quiet = 0f;
            if (_world == 3)
            {
                resting = PremiumLook.W3GridAlpha;
                quiet = PremiumLook.W3GridQuietRadius;
                if (gridMul < PremiumLook.W3GridPulseFloor)
                {
                    gridMul = PremiumLook.W3GridPulseFloor;
                }
            }

            ApplyGrid(_grid, resting, gridMul, quiet);
            if (_voidField != null)
            {
                float motion = 1f;
                if (reduce)
                {
                    motion = ArenaDepth.ReduceMotion;
                }

                _voidField.SetFloat("_Motion", motion);
            }
            TickNearDust(view, reduce);
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
            _grid = GridLayer(
                "Mat_Env_Grid",
                MakeGridTexture(),
                new Color(EmissionTargets.SteelR, EmissionTargets.SteelG, EmissionTargets.SteelB, PremiumLook.GridVisibleAlpha),
                2900);
            _voidField = VoidFieldMaterial();

            CreateDome("VoidField", radius * 7.2f, _voidField);
            CreateDome("OuterNebula", radius * 11.5f, OuterNebulaMaterial());
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
            CreateNearDust();
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
            MeshFilter filter = go.AddComponent<MeshFilter>();
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            filter.sharedMesh = DomeRingMesh(radius, 0.22f, 0.09f, 72);
            renderer.sharedMaterial = DomeRingMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Mesh DomeRingMesh(float radius, float halfWidth, float height, int segments)
        {
            int count = segments;
            if (count < 8)
            {
                count = 8;
            }

            float inner = radius - halfWidth;
            if (inner < 0.2f)
            {
                inner = 0.2f;
            }

            float outer = radius + halfWidth;
            Vector3[] vertices = new Vector3[count * 2];
            Vector2[] uv = new Vector2[count * 2];
            int[] triangles = new int[count * 6];
            for (int segment = 0; segment < count; segment++)
            {
                float angle = (segment / (float)count) * Mathf.PI * 2f;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                vertices[segment * 2] = new Vector3(cosine * inner, height, sine * inner);
                vertices[segment * 2 + 1] = new Vector3(cosine * outer, height, sine * outer);
                uv[segment * 2] = new Vector2(0f, segment / (float)count);
                uv[segment * 2 + 1] = new Vector2(1f, segment / (float)count);
                int next = (segment + 1) % count;
                int tri = segment * 6;
                triangles[tri] = segment * 2;
                triangles[tri + 1] = next * 2;
                triangles[tri + 2] = segment * 2 + 1;
                triangles[tri + 3] = segment * 2 + 1;
                triangles[tri + 4] = next * 2;
                triangles[tri + 5] = next * 2 + 1;
            }

            Mesh mesh = new Mesh();
            mesh.name = "DomeRing";
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material DomeRingMaterial()
        {
            Shader shader = Shader.Find("AGR/DomeRing");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader);
            material.name = "Mat_Env_Lip";
            material.color = new Color(
                EmissionTargets.SteelR,
                EmissionTargets.SteelG,
                EmissionTargets.SteelB,
                PremiumLook.DomeRimAlpha);
            material.renderQueue = 3000;
            material.SetInt("_ZWrite", 0);
            if (shader != null && shader.name == "AGR/DomeRing")
            {
                material.SetFloat("_ZTest", (float)LayerGate.ZTestLEqual);
            }

            return material;
        }

        private static Material VoidFieldMaterial()
        {
            Shader shader = Shader.Find("AGR/VoidField");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader);
            material.name = "Mat_Env_VoidField";
            material.color = new Color(PremiumLook.OutsideR, PremiumLook.OutsideG, PremiumLook.OutsideB, 1f);
            material.renderQueue = 2600;
            material.SetFloat("_Motion", 1f);
            material.SetInt("_ZWrite", 0);
            return material;
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
                if (y > DecorDepthRules.MaxDecorTop)
                {
                    y = DecorDepthRules.MaxDecorTop;
                }

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

                OpeningClearance.FitBelt(slot);
                DecorDepthClamp.ClampTop(slot.gameObject, DecorDepthRules.MaxDecorTop);
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

            material.mainTextureOffset = new Vector2(x, y);
        }

        private void ApplyGrid(Material material, float restingAlpha, float pulse, float quietRadius)
        {
            if (material == null)
            {
                return;
            }

            float resting = restingAlpha;
            if (resting < 0f)
            {
                resting = 0f;
            }

            if (resting > 1f)
            {
                resting = 1f;
            }

            float beat = pulse;
            if (beat < 0f)
            {
                beat = 0f;
            }

            Color color = material.color;
            bool masked = material.HasProperty("_Pulse") && material.HasProperty("_QuietRadius");
            if (!masked)
            {
                float alpha = resting * beat;
                if (_world == 3)
                {
                    alpha = resting;
                }

                ApplyLayerAlpha(material, alpha);
                return;
            }

            color.a = resting;
            material.color = color;
            material.SetFloat("_Pulse", beat);
            material.SetFloat("_QuietRadius", quietRadius);
            float shipX;
            float shipZ;
            ShipFloor(out shipX, out shipZ);
            material.SetFloat("_ShipX", shipX);
            material.SetFloat("_ShipZ", shipZ);
            if (material.HasProperty("_QuietColor"))
            {
                material.SetColor("_QuietColor", _gridQuiet);
            }
        }

        private void ShipFloor(out float x, out float z)
        {
            x = 0f;
            z = 0f;
            if (_player == null)
            {
                ShipController[] ships = UnityEngine.Object.FindObjectsByType<ShipController>();
                if (ships != null && ships.Length > 0 && ships[0] != null)
                {
                    _player = ships[0].transform;
                }
            }

            if (_player == null)
            {
                return;
            }

            x = _player.position.x;
            z = _player.position.z;
        }

        private static void ApplyLayerAlpha(Material material, float alpha)
        {
            if (material == null)
            {
                return;
            }

            float a = alpha;
            if (a < 0f)
            {
                a = 0f;
            }

            if (a > 1f)
            {
                a = 1f;
            }

            Color color = material.color;
            color.a = a;
            material.color = color;
        }

        private void CreateNearDust()
        {
            GameObject dustObject = new GameObject("NearDust");
            dustObject.transform.SetParent(transform, false);
            _dust = dustObject.AddComponent<ParticleSystem>();
            _dustBuf = new ParticleSystem.Particle[ArenaDepth.DustFull];
            ParticleSystem.MainModule main = _dust.main;
            main.playOnAwake = true;
            main.loop = true;
            main.duration = ArenaDepth.DustLife;
            main.startLifetime = ArenaDepth.DustLife;
            main.startSpeed = 0f;
            main.maxParticles = ArenaDepth.DustFull;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSize = new ParticleSystem.MinMaxCurve(ArenaDepth.DustSizeMin, ArenaDepth.DustSizeMax);
            main.startColor = new Color(ArenaDepth.AccentR, ArenaDepth.AccentG, ArenaDepth.AccentB, ArenaDepth.DustAlphaMax);
            ParticleSystem.EmissionModule emission = _dust.emission;
            emission.enabled = true;
            emission.rateOverTime = ArenaDepth.DustFull / ArenaDepth.DustLife;
            ParticleSystem.ShapeModule shape = _dust.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 2.2f;
            ParticleSystemRenderer renderer = dustObject.GetComponent<ParticleSystemRenderer>();
            Material material = NearDustMaterial();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            if (material == null)
            {
                _dust.Stop();
                dustObject.SetActive(false);
            }
        }

        private static Material NearDustMaterial()
        {
            Shader shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Additive");
            }

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
            material.name = "Mat_Env_NearDust";
            material.color = new Color(ArenaDepth.AccentR, ArenaDepth.AccentG, ArenaDepth.AccentB, ArenaDepth.DustAlphaMax);
            material.renderQueue = 3000;
            return material;
        }

        private void TickNearDust(Camera view, bool reduce)
        {
            if (_dust == null)
            {
                return;
            }

            int count = ArenaDepth.DustCount(reduce, Screen.width);
            ParticleSystem.MainModule main = _dust.main;
            if (main.maxParticles != count)
            {
                main.maxParticles = count;
                ParticleSystem.EmissionModule emission = _dust.emission;
                emission.rateOverTime = count / ArenaDepth.DustLife;
            }

            if (view == null)
            {
                return;
            }

            Vector3 camPos = view.transform.position;
            Vector3 play = new Vector3(camPos.x, DecorDepthRules.PlayY, camPos.z);
            _dust.transform.position = Vector3.Lerp(camPos, play, 0.35f);
            float dt = Time.unscaledDeltaTime;
            if (_dustReady != 0 && dt > 0.0001f)
            {
                Vector3 velocity = (camPos - _dustCam) / dt;
                Vector3 drift = velocity * ArenaDepth.DustParallax;
                ParticleSystem.VelocityOverLifetimeModule motion = _dust.velocityOverLifetime;
                motion.enabled = true;
                motion.space = ParticleSystemSimulationSpace.World;
                motion.x = drift.x;
                motion.y = drift.y;
                motion.z = drift.z;
            }

            _dustCam = camPos;
            _dustReady = 1;
            if (_dustBuf == null)
            {
                return;
            }

            int alive = _dust.GetParticles(_dustBuf);
            Vector3 ship = _dust.transform.position;
            GameObject player = GameObject.FindGameObjectWithTag(GameTags.Player);
            if (player != null)
            {
                ship = player.transform.position;
            }

            float resting = reduce ? ArenaDepth.DustAlphaMax * ArenaDepth.DustReduceAlpha : ArenaDepth.DustAlphaMax;
            byte red = (byte)(ArenaDepth.AccentR * 255f);
            byte green = (byte)(ArenaDepth.AccentG * 255f);
            byte blue = (byte)(ArenaDepth.AccentB * 255f);
            for (int i = 0; i < alive; i++)
            {
                Vector3 at = _dustBuf[i].position;
                float dx = at.x - ship.x;
                float dy = at.y - ship.y;
                float dz = at.z - ship.z;
                float distance = Mathf.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
                float alpha = ArenaDepth.DustAlpha(resting, distance, reduce);
                byte a = (byte)(alpha * 255f);
                _dustBuf[i].startColor = new Color32(red, green, blue, a);
            }

            if (alive > 0)
            {
                _dust.SetParticles(_dustBuf, alive);
            }
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
            return SoftenAmount(color, RetintChroma);
        }

        private static Color SoftenAmount(Color color, float chroma)
        {
            float mix = chroma;
            if (mix < 0f)
            {
                mix = 0f;
            }

            if (mix > 1f)
            {
                mix = 1f;
            }

            float y = 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
            return new Color(
                y + (color.r - y) * mix,
                y + (color.g - y) * mix,
                y + (color.b - y) * mix,
                color.a);
        }

        private static Color ScaleColor(Color color, float scale)
        {
            float amount = scale;
            if (amount < 0f)
            {
                amount = 0f;
            }

            return new Color(
                Mathf.Clamp01(color.r * amount),
                Mathf.Clamp01(color.g * amount),
                Mathf.Clamp01(color.b * amount),
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

        private static Material OuterNebulaMaterial()
        {
            const int Size = 32;
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "OuterNebula";
            for (int y = 0; y < Size; y++)
            {
                float v = y / (float)(Size - 1);
                float r = PremiumLook.OutsideR + (0.03f * v);
                float g = PremiumLook.OutsideG + (0.015f * (1f - v));
                float b = PremiumLook.OutsideB + (0.05f * v);
                for (int x = 0; x < Size; x++)
                {
                    tex.SetPixel(x, y, new Color(r, g, b, 1f));
                }
            }

            tex.Apply(false, true);
            return UnlitLayer("Mat_Env_OuterNebula", tex, Color.white, 2990, 1f);
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

        private static Material GridLayer(string name, Texture2D texture, Color tint, int queue)
        {
            Shader shader = Shader.Find("AGR/Grid");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
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
            if (material.HasProperty("_Pulse"))
            {
                material.SetFloat("_Pulse", 1f);
            }

            if (material.HasProperty("_QuietRadius"))
            {
                material.SetFloat("_QuietRadius", 0f);
            }

            if (material.HasProperty("_QuietColor"))
            {
                material.SetColor("_QuietColor", new Color(1f, 1f, 1f, 0f));
            }

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
            Color clear = new Color(EmissionTargets.SteelR, EmissionTargets.SteelG, EmissionTargets.SteelB, 0f);
            Color line = new Color(
                EmissionTargets.SteelR + ((1f - EmissionTargets.SteelR) * 0.12f),
                EmissionTargets.SteelG + ((1f - EmissionTargets.SteelG) * 0.12f),
                EmissionTargets.SteelB + ((1f - EmissionTargets.SteelB) * 0.12f),
                0.85f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float nx = (x / (float)(Size - 1)) * 2f - 1f;
                    float ny = (y / (float)(Size - 1)) * 2f - 1f;
                    float radius01 = Mathf.Sqrt(nx * nx + ny * ny);
                    float fade = 1f;
                    if (radius01 > PremiumLook.GridEdgeStart)
                    {
                        float edge = (radius01 - PremiumLook.GridEdgeStart) / (1f - PremiumLook.GridEdgeStart);
                        if (edge < 0f)
                        {
                            edge = 0f;
                        }

                        if (edge > 1f)
                        {
                            edge = 1f;
                        }

                        fade = 1f - edge;
                    }
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
