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
        public const float ArenaPlaySurfaceY = -0.08f;
        public const float AstroFloorVisualScale = 0.88f;

        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private Transform _threatRoot;
        private Transform _projectileRoot;
        private readonly List<Projectile> _playerFree = new List<Projectile>();
        private readonly List<Projectile> _enemyFree = new List<Projectile>();
        private bool _prewarming;
        private Transform _pickupRoot;
        private GameObject _hangarDressing;
        private ArenaEnv _arenaEnv;
        private int _hangarShown = 1;
        private GameObject _arenaRoot;
        private string _arenaVisualName;
        private bool _arenaApplied;
        private ArenaLayoutId _arenaLayout;
        private int _arenaWorldNumber;
        private bool _spawnOutside;
        private Material _hull;
        private Material _previewAccent;
        private Material _previewCyan;
        private Material _accent;
        private Material _glass;
        private Material _glow;
        private Material _asteroid;
        private Material _enemy;
        private Material _arena;
        private Material _w3Beam;
        private Material _astroRim;
        private Material _projectile;
        private Material _projectileSpread;
        private Material _projectilePierce;
        private Material _projectileSeeker;
        private Material _projectileRicochet;
        private Material _projectileEnemy;
        private Material _projectileHalo;
        private Material _shield;
        private Material _accentHot;
        private Material _accentHotHero;
        private Material _shipCyan;
        private Material _shipMagenta;
        private Material _accentWarm;
        private Material _asteroidB;
        private Material _hangarMetal;
        private Material _hangarAmber;
        private Material _hangarCyan;
        private Material _hangarGlow;
        private Material _launchAmber;
        private Material _launchGlow;
        private Material _launchDecal;
        private Material _brute;
        private Material _bruteGlow;
        private Material _swarm;
        private Material _swarmGlow;
        private Material _swarmling;
        private Material _enemyGlow;
        private Material _enemyBody;
        private Material _enemyTrim;
        private Material _enemyCore;
        private Material _enemyMidBody;
        private Material _enemyMidCore;
        private Material _enemyTelegraph;
        private Material _enemyThruster;
        private Material _projectileRail;
        private Material _pickupHeart;
        private Material _pickupGeneric;
        private Material _hazardSpike;
        private Material _layoutCyan;
        private Material _layoutMagenta;
        private Material _layoutOrange;
        private Material _layoutLime;
        private Material _layoutRust;
        private Material _washCyan;
        private Material _washMagenta;
        private Material _washOrange;
        private Material _washLime;
        private Material _washRust;
        private Material _layoutRail;
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
            float dangerR;
            float dangerG;
            float dangerB;
            float steelR;
            float steelG;
            float steelB;
            ReadabilityPalette.DangerRgb(out dangerR, out dangerG, out dangerB);
            ReadabilityPalette.SecondaryRgb(out steelR, out steelG, out steelB);
            Color dangerEmit = new Color(dangerR, dangerG, dangerB);
            Color steelEmit = new Color(steelR, steelG, steelB);
            Color primaryEmit = new Color(EmissionTargets.PrimaryR, EmissionTargets.PrimaryG, EmissionTargets.PrimaryB);
            Color hullEmit = steelEmit * ReadabilityPalette.HullRimScale * PremiumLook.CombatHullEmission;
            float bodyCap = ReadabilityPalette.HullBodyEmission;
            if (hullEmit.maxColorComponent > bodyCap && hullEmit.maxColorComponent > 0.0001f)
            {
                hullEmit *= bodyCap / hullEmit.maxColorComponent;
            }

            _hull = MakeMaterial(
                "Mat_Ship_Hull",
                new Color(ReadabilityPalette.HullR, ReadabilityPalette.HullG, ReadabilityPalette.HullB),
                0.45f,
                0.35f,
                hullEmit);
            // Ship_Hero accent islands share the hull shader in combat. The
            // hangar preview identifies them by name and writes the accent
            // colour onto that slot only.
            _previewAccent = MakeMaterial(
                "Mat_Preview_Accent",
                new Color(ReadabilityPalette.HullR, ReadabilityPalette.HullG, ReadabilityPalette.HullB),
                0.45f,
                0.35f,
                hullEmit);
            _previewCyan = MakeMaterial(
                "Mat_Preview_Cyan",
                new Color(ReadabilityPalette.HullR, ReadabilityPalette.HullG, ReadabilityPalette.HullB),
                0.45f,
                0.35f,
                hullEmit);
            _accent = MakeMaterial("Mat_Ship_Accent", new Color(1f, 0.55f, 0.14f), 0.2f, 0.55f, new Color(1f, 0.4f, 0.05f) * 1.4f);
            _accentHot = MakeMaterial("Mat_Ship_Accent_Hot", new Color(1f, 0.38f, 0.08f), 0.15f, 0.45f, new Color(1f, 0.25f, 0.04f) * 1.8f);
            _accentHotHero = MakeShipRim(
                "Mat_Ship_AccentHot_Hero",
                new Color(1f, 0.38f, 0.04f),
                primaryEmit,
                EmissionTargets.HullRimPower,
                EmissionTargets.HullRimStrength,
                primaryEmit * EmissionTargets.ShipAccentHotHero,
                0.15f,
                0.45f,
                false);
            _shipCyan = MakeShipRim(
                "Mat_Ship_Cyan_Hero",
                steelEmit,
                steelEmit,
                EmissionTargets.HullRimPower,
                EmissionTargets.HullRimStrength,
                steelEmit * EmissionTargets.ShipCyan,
                0.2f,
                0.45f,
                false);
            _shipMagenta = MakeShipRim(
                "Mat_Ship_Magenta_Hero",
                primaryEmit,
                primaryEmit,
                EmissionTargets.HullRimPower,
                EmissionTargets.GlowRimStrength,
                primaryEmit * EmissionTargets.MagentaMul,
                0.15f,
                0.4f,
                false);
            _accentWarm = MakeMaterial("Mat_Ship_Accent_Warm", new Color(1f, 0.68f, 0.28f), 0.18f, 0.5f, new Color(1f, 0.5f, 0.12f) * 1.1f);
            _glass = MakeTransparent("Mat_Ship_Glass", new Color(0.35f, 0.7f, 0.95f, 0.28f), new Color(0.2f, 0.5f, 0.8f) * 0.4f);
            _glow = MakeShipRim(
                "Mat_Ship_Glow",
                new Color(1f, 0.55f, 0.15f),
                steelEmit,
                EmissionTargets.HullRimPower,
                EmissionTargets.GlowRimStrength,
                primaryEmit * EmissionTargets.ShipGlow,
                0f,
                0.15f,
                false);
            _asteroid = MakeShipRim(
                "Mat_Asteroid",
                new Color(ReadabilityPalette.AsteroidR, ReadabilityPalette.AsteroidG, ReadabilityPalette.AsteroidB) * ReadabilityPalette.AsteroidAlbedoScale * PremiumLook.AsteroidAlbedoKeep,
                steelEmit * EmissionTargets.AsteroidRimColorMul,
                EmissionTargets.AsteroidRimPower,
                EmissionTargets.AsteroidRimStrength,
                steelEmit * EmissionTargets.AsteroidEmission(ReadabilityPalette.AsteroidEmissionScale),
                0.05f,
                0.18f,
                false);
            _asteroidB = MakeShipRim(
                "Mat_Asteroid_B",
                new Color(ReadabilityPalette.AsteroidVariantR, ReadabilityPalette.AsteroidVariantG, ReadabilityPalette.AsteroidVariantB) * ReadabilityPalette.AsteroidAlbedoScale * PremiumLook.AsteroidAlbedoKeep * PremiumLook.CraterDark,
                steelEmit * EmissionTargets.AsteroidRimColorMul,
                EmissionTargets.AsteroidRimPower,
                EmissionTargets.AsteroidRimStrength,
                steelEmit * EmissionTargets.AsteroidEmission(ReadabilityPalette.AsteroidEmissionScale),
                0.04f,
                0.14f,
                false);
            _enemy = MakeShipRim(
                "Mat_Enemy",
                new Color(ReadabilityPalette.EnemyBodyR, ReadabilityPalette.EnemyBodyG, ReadabilityPalette.EnemyBodyB) * PremiumLook.MinorEnemyLift,
                dangerEmit,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                dangerEmit * ReadabilityPalette.EnemyEmissionScale * PremiumLook.MinorEnemyLift,
                0.22f,
                0.35f,
                false);
            _enemyGlow = MakeShipRim(
                "Mat_Enemy_Glow",
                dangerEmit,
                dangerEmit,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                dangerEmit * EmissionTargets.EnemyGlow,
                0.15f,
                0.4f,
                false);
            Color enemySteel = new Color(LookPolish.EnemySteelR, LookPolish.EnemySteelG, LookPolish.EnemySteelB, 1f);
            Color enemyTrim = new Color(LookPolish.EnemyTrimR, LookPolish.EnemyTrimG, LookPolish.EnemyTrimB, 1f);
            Color enemyCore = new Color(LookPolish.EnemyRimR, LookPolish.EnemyRimG, LookPolish.EnemyRimB, 1f);
            Color midCore = new Color(LookPolish.EnemyMidCoreR, LookPolish.EnemyMidCoreG, LookPolish.EnemyMidCoreB, 1f);
            Color telegraph = new Color(LookPolish.EnemyCoreR, LookPolish.EnemyCoreG, LookPolish.EnemyCoreB, 1f);
            Color bodyGlow = enemyCore * LookPolish.EnemyBodyEmission;
            _enemyBody = MakeShipRim(
                "Mat_Enemy_V4_Body",
                enemySteel,
                enemyCore,
                EmissionTargets.EnemyRimPower,
                LookPolish.EnemyBodyRimStrength,
                bodyGlow,
                LookPolish.EnemyBodyMetallic,
                LookPolish.EnemyBodySmoothness,
                false);
            _enemyTrim = MakeShipRim(
                "Mat_Enemy_V4_Trim",
                enemyTrim,
                enemyCore,
                EmissionTargets.EnemyRimPower,
                LookPolish.EnemyBodyRimStrength * 0.65f,
                bodyGlow * 0.45f,
                0f,
                0.2f,
                false);
            _enemyCore = MakeShipRim(
                "Mat_Enemy_V4_Core",
                enemyCore,
                enemyCore,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                enemyCore * LookPolish.EnemySlotCoreEmission,
                0.1f,
                0.45f,
                false);
            _enemyMidBody = MakeShipRim(
                "Mat_Enemy_V4_MidBody",
                Color.Lerp(enemySteel, midCore, LookPolish.EnemyMidBodyMix),
                enemyCore,
                EmissionTargets.EnemyRimPower,
                LookPolish.EnemyBodyRimStrength,
                bodyGlow * 0.7f,
                LookPolish.EnemyBodyMetallic,
                LookPolish.EnemyBodySmoothness,
                false);
            _enemyMidCore = MakeShipRim(
                "Mat_Enemy_V4_MidCore",
                midCore,
                midCore,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                midCore * LookPolish.EnemySlotCoreEmission,
                0.1f,
                0.45f,
                false);
            _enemyTelegraph = MakeShipRim(
                "Mat_Enemy_V4_Telegraph",
                telegraph,
                telegraph,
                EmissionTargets.EnemyRimPower,
                0f,
                Color.black,
                0f,
                0.35f,
                false);
            _enemyThruster = MakeAdditiveThruster(
                "Mat_Enemy_V4_Thruster",
                new Color(1f, 0.72f, 0.42f, 1f));
            // 0.40 floor anchor stays in this file: 0.039f, 0.063f, 0.086f. Drawn albedo is the surface token.
            _arena = MakeMaterial(
                "Mat_Arena",
                new Color(PremiumLook.SurfaceR, PremiumLook.SurfaceG, PremiumLook.SurfaceB),
                PremiumLook.FloorMetallic,
                PremiumLook.FloorSmoothness);
            _arena.mainTexture = FloorNoise();
            // W3 edge posts are cool grey, not player amber and not a dim plate.
            _w3Beam = MakeEdgeBeam("Mat_Arena_W3_Beam");
            _astroRim = MakeMaterial("Mat_AstroRim", new Color(0.357f, 0.561f, 0.659f), 0.12f, 0.38f, new Color(0.357f, 0.561f, 0.659f) * 0.55f);
            _projectile = MakeMaterial("Mat_Projectile", primaryEmit, 0f, 0.32f, primaryEmit * EmissionTargets.PlayerShot);
            _projectileSpread = MakeMaterial("Mat_Projectile_Spread", primaryEmit, 0f, 0.28f, primaryEmit * EmissionTargets.PlayerShot);
            _projectilePierce = MakeMaterial("Mat_Projectile_Pierce", steelEmit, 0f, 0.3f, steelEmit * EmissionTargets.PlayerShot);
            _projectileSeeker = MakeMaterial("Mat_Projectile_Seeker", steelEmit, 0f, 0.28f, steelEmit * EmissionTargets.PlayerShot);
            _projectileRicochet = MakeMaterial("Mat_Projectile_Ricochet", primaryEmit, 0f, 0.3f, primaryEmit * EmissionTargets.PlayerShot);
            _projectileRail = MakeMaterial("Mat_Projectile_Rail", primaryEmit, 0f, 0.4f, primaryEmit * EmissionTargets.PlayerRail);
            _projectileEnemy = MakeMaterial(
                "Mat_Projectile_Enemy",
                dangerEmit,
                0f,
                0.28f,
                dangerEmit * EmissionTargets.EnemyShot * PremiumLook.EnemyCoreLift);
            _projectileHalo = MakeTransparent("Mat_Projectile_Halo", new Color(0.831f, 0.627f, 0.29f, 0.26f), new Color(0.831f, 0.627f, 0.29f) * 0.9f);
            _hangarMetal = MakeMaterial("Mat_Hangar_Metal", new Color(0.28f, 0.32f, 0.36f), 0.55f, 0.42f);
            _hangarAmber = MakeMaterial("Mat_Hangar_Amber", new Color(1f, 0.58f, 0.16f), 0.2f, 0.5f, new Color(1f, 0.42f, 0.06f) * 1.6f);
            _hangarCyan = MakeMaterial("Mat_Hangar_Cyan", new Color(0.22f, 0.72f, 0.88f), 0.15f, 0.45f, new Color(0.15f, 0.55f, 0.8f) * 1.4f);
            _hangarGlow = MakeMaterial("Mat_Hangar_Glow", new Color(1f, 0.72f, 0.28f), 0f, 0.2f, new Color(1f, 0.55f, 0.12f) * 2.6f);
            _launchAmber = MakeMaterial("Mat_LaunchSign_Amber", primaryEmit, 0.12f, 0.42f, primaryEmit * EmissionTargets.CapLaunchSign(3.8f));
            _launchGlow = MakeMaterial("Mat_LaunchSign_Glow", primaryEmit, 0f, 0.18f, primaryEmit * EmissionTargets.CapLaunchSign(5.2f));
            _launchDecal = MakeMaterial("Mat_LaunchSign_Decal", primaryEmit, 0f, 0.12f, primaryEmit * EmissionTargets.CapLaunchSign(6.4f));
            _brute = MakeShipRim(
                "Mat_Monster_Brute",
                new Color(0.58f, 0.34f, 0.36f),
                dangerEmit * EmissionTargets.BruteRimColorMul,
                EmissionTargets.BruteRimPower,
                EmissionTargets.BruteRimStrength,
                dangerEmit * ReadabilityPalette.EnemyEmissionScale,
                0.22f,
                0.36f,
                false);
            _bruteGlow = MakeShipRim(
                "Mat_Enemy_Brute_Glow",
                dangerEmit,
                dangerEmit * EmissionTargets.BruteRimColorMul,
                EmissionTargets.BruteRimPower,
                EmissionTargets.BruteRimStrength,
                dangerEmit * EmissionTargets.EnemyBrute,
                0.12f,
                0.45f,
                false);
            _swarm = MakeShipRim(
                "Mat_Monster_Swarm",
                new Color(0.42f, 0.28f, 0.26f),
                dangerEmit,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                dangerEmit * ReadabilityPalette.EnemyEmissionScale,
                0.18f,
                0.35f,
                false);
            _swarmGlow = MakeShipRim(
                "Mat_Enemy_Swarm_Glow",
                dangerEmit,
                dangerEmit,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                dangerEmit * EmissionTargets.EnemyGlow,
                0.12f,
                0.4f,
                false);
            _swarmling = MakeShipRim(
                "Mat_Monster_Swarmling",
                new Color(0.36f, 0.46f, 0.4f),
                dangerEmit,
                EmissionTargets.EnemyRimPower,
                EmissionTargets.EnemyRimStrength,
                dangerEmit * ReadabilityPalette.EnemyEmissionScale,
                0.12f,
                0.32f,
                false);
            Color healEmit = new Color(EmissionTargets.HealR, EmissionTargets.HealG, EmissionTargets.HealB);
            _pickupHeart = MakeMaterial(
                "Mat_FX_Pickup_Heart",
                healEmit,
                0.1f,
                0.62f,
                healEmit * EmissionTargets.FxPickupHeart);
            _pickupGeneric = MakeMaterial(
                "Mat_FX_Pickup_Generic",
                steelEmit,
                0.2f,
                0.4f,
                steelEmit * 0.35f);
            _hazardSpike = MakeMaterial("Mat_Arena_Hazard", new Color(0.722f, 0.353f, 0.157f), 0.28f, 0.24f, new Color(0.722f, 0.353f, 0.157f) * 0.45f);
            Color layoutDanger = dangerEmit * EmissionTargets.LayoutDangerAlbedoMul;
            _layoutCyan = MakeMaterial("Mat_Arena_Layout_Cyan", steelEmit, 0.15f, 0.2f, steelEmit * 1.3f);
            _layoutMagenta = MakeMaterial("Mat_Arena_Layout_Magenta", primaryEmit, 0.15f, 0.2f, primaryEmit * 1.2f);
            _layoutOrange = MakeMaterial("Mat_Arena_Layout_Orange", new Color(0.72f, 0.28f, 0.08f), 0.12f, 0.18f, new Color(1f, 0.35f, 0.05f) * 1.4f);
            _layoutLime = MakeMaterial("Mat_Arena_Layout_Lime", layoutDanger, 0.12f, 0.18f, dangerEmit * EmissionTargets.LayoutEmissionMul);
            _layoutRust = MakeMaterial("Mat_Arena_Layout_Rust", new Color(0.42f, 0.22f, 0.14f), 0.08f, 0.14f, new Color(0.7f, 0.25f, 0.08f) * 0.9f);
            _washCyan = MakeTransparent("Mat_Arena_Wash_Cyan", new Color(steelEmit.r, steelEmit.g, steelEmit.b, 0.05f), steelEmit * 0.2f);
            _washMagenta = MakeTransparent("Mat_Arena_Wash_Magenta", new Color(primaryEmit.r, primaryEmit.g, primaryEmit.b, 0.05f), primaryEmit * 0.18f);
            _washOrange = MakeTransparent("Mat_Arena_Wash_Orange", new Color(0.78f, 0.32f, 0.08f, 0.05f), new Color(1f, 0.32f, 0.05f) * 0.2f);
            _washLime = MakeTransparent("Mat_Arena_Wash_Lime", new Color(layoutDanger.r, layoutDanger.g, layoutDanger.b, 0.05f), dangerEmit * 0.2f);
            _washRust = MakeTransparent("Mat_Arena_Wash_Rust", new Color(0.42f, 0.2f, 0.12f, 0.05f), new Color(0.65f, 0.22f, 0.08f) * 0.16f);
            _layoutRail = MakeMaterial("Mat_Arena_Layout_Rail", new Color(0.95f, 0.82f, 0.42f), 0.35f, 0.55f, new Color(1f, 0.72f, 0.18f) * 1.8f);
            _shield = MakeTransparent("Mat_Shield", new Color(steelEmit.r, steelEmit.g, steelEmit.b, 0.22f), steelEmit * 0.6f);
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
            ProjectileLightPool.Ensure(transform);
            PrewarmProjectiles();
            try
            {
                ShaderWarmup.WarmShots();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Shot warm-up skipped: " + ex.Message);
            }
        }

        // Six meshes. World 7 reuses Arena_Blockout (index % 6) and gets its own
        // retint from WorldRules, so the spoke ring is not a copy of world 1.
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
            return ArenaLayout.WorldIndexForWave(waveIndex);
        }

        public static string ArenaVisualForWave(int waveIndex)
        {
            int index = (WorldIndexForWave(waveIndex) - 1) % ArenaWorlds.Length;
            if (index < 0)
            {
                index += ArenaWorlds.Length;
            }

            return ArenaWorlds[index];
        }

        public void SetSpawnOutside(bool outside)
        {
            _spawnOutside = outside;
        }

        public void ApplyArenaForWave(int waveIndex)
        {
            ArenaShapeCatalog.SetActive(WorldIndexForWave(waveIndex));
            if (_arenaRoot == null)
            {
                return;
            }

            string visualName = ArenaVisualForWave(waveIndex);
            ArenaLayoutId layout = ArenaLayout.ForWave(waveIndex);
            int worldNumber = WorldCatalog.NumberForWave(waveIndex);
            if (_arenaApplied && visualName == _arenaVisualName && layout == _arenaLayout && _arenaWorldNumber == worldNumber)
            {
                return;
            }

            bool announce = !string.IsNullOrEmpty(_arenaVisualName);

            for (int i = _arenaRoot.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = _arenaRoot.transform.GetChild(i);
                if (child.name == "ArenaEnv")
                {
                    continue;
                }

                Destroy(child.gameObject);
            }

            _arenaRoot.name = visualName;
            GameObject visual;
            GameObject floorVisual = null;
            if (TryVisual(visualName, _arenaRoot.transform, _arena, out visual))
            {
                float reach = ArenaShapeCatalog.FloorReach(ArenaShapeCatalog.Active);
                float scale = reach / WaveManager.ArenaDesignRadius;
                if (IsAstroPlayFloor(visualName))
                {
                    scale *= AstroFloorVisualScale;
                }

                visual.transform.localScale = Vector3.one * scale;
                if (IsAstroPlayFloor(visualName))
                {
                    SinkPlaySurface(visual, ArenaPlaySurfaceY);
                    DressArenaFloorRenderers(visual);
                    DressAstroFloorColliders(visual);
                }
                else
                {
                    StripImportedFloorColliders(visual);
                }

                floorVisual = visual;
                SpawnClear.ClearVisual(floorVisual);
                HideInactiveBeams(floorVisual);
                DecorDepthClamp.ClampTop(floorVisual, DecorDepthRules.MaxDecorTop);
            }
            else
            {
                GameObject floor = CreatePrimitive(PrimitiveType.Cylinder, "Arena_Floor", _arenaRoot.transform, _arena);
                float fallbackReach = ArenaShapeCatalog.FloorReach(ArenaShapeCatalog.Active);
                floor.transform.localScale = new Vector3(fallbackReach * 2f, 0.04f, fallbackReach * 2f);
                floor.transform.position = new Vector3(0f, ArenaPlaySurfaceY, 0f);
                floorVisual = floor;
            }

            // Hazards, beams, and mines are added after this, so they never join the floor whitelist.
            FloorPlate.Adopt(floorVisual);
            BuildArenaLayout(layout);
            ArenaEnv env = ArenaEnv.Ensure(_arenaRoot.transform);
            _arenaEnv = env;
            if (env != null)
            {
                env.Retint(worldNumber);
                env.TintFloor(floorVisual);
                if (worldNumber != 1)
                {
                    ClearSpawnContrast(env.transform);
                }
            }

            _arenaVisualName = visualName;
            _arenaLayout = layout;
            _arenaWorldNumber = worldNumber;
            _arenaApplied = true;
            DecorCameraStack.ApplyLayer(_arenaRoot);
            if (env != null && worldNumber == 1)
            {
                // ApplyLayer already ran. Dress pins the disc on the decor
                // camera itself, at the ship spawn, under the ring and grid.
                DressSpawnContrast(env.transform);
            }
            if (announce)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayWorldChange(WorldIndexForWave(waveIndex));
                }

                if (GameUi.Instance != null)
                {
                    GameUi.Instance.AnnounceWorldChange(waveIndex);
                }
            }
        }

        public void SetHangarDressingVisible(bool visible)
        {
            int wasShown = _hangarShown;
            if (_hangarDressing != null)
            {
                _hangarDressing.SetActive(visible);
            }

            bool reduce = SettingsState.ReduceEffectsEnabled;
            if (visible)
            {
                RenderSettings.ambientLight = HangarAmbient;
                PostStack.SnapHangar(reduce);
                if (_arenaEnv != null)
                {
                    _arenaEnv.RestoreHangarKey();
                }

                _hangarShown = 1;
                return;
            }

            Color combatFill = CombatAmbient;
            int litWorld = _arenaWorldNumber;
            if (litWorld < 1)
            {
                litWorld = 1;
            }

            WorldLightRow lit = WorldLight.Row(litWorld);
            combatFill = new Color(
                PremiumLook.AmbientChannel(lit.FillR),
                PremiumLook.AmbientChannel(lit.FillG),
                PremiumLook.AmbientChannel(lit.FillB),
                1f);
            RenderSettings.ambientLight = combatFill;
            if (wasShown != 0)
            {
                if (_arenaEnv != null)
                {
                    _arenaEnv.Retint(litWorld);
                }

                PostStack.SnapWorld(litWorld, reduce);
            }

            _hangarShown = 0;
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
            GameObject hangarShip = PlaceHangarProp("Hangar_ShipComplete", "Ship_Complete", new Vector3(-8.6f, 0f, 8.2f), _hull, PrimitiveType.Cube,
                new Vector3(1.1f, 0.55f, 2.4f), 0.55f, new Color(1f, 0.55f, 0.16f), 0.7f);
            DressShipComplete(hangarShip);

            CreatePrimitive(PrimitiveType.Cylinder, "Hangar_ShipPad", _hangarDressing.transform, _hangarAmber,
                new Vector3(0f, 0.02f, 0f), new Vector3(4.6f, 0.04f, 4.6f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Hangar_KioskPlate", _hangarDressing.transform, _hangarCyan,
                new Vector3(2.2f, 0.015f, -6.4f), new Vector3(2.4f, 0.03f, 1.8f), Quaternion.identity);
            DecorCameraStack.ApplyLayer(_hangarDressing);
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
            DressLaunchGoMeshDecal(root.transform);
            GameObject decal = new GameObject("LaunchGoDecal");
            decal.transform.SetParent(root.transform, false);
            decal.transform.localPosition = new Vector3(0f, 1.38f, 0.18f);
            decal.transform.localRotation = Quaternion.identity;
            TextMesh go = decal.AddComponent<TextMesh>();
            go.text = "GO";
            Font font = UiFonts.Display();
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
            Renderer decalRenderer = go.GetComponent<Renderer>();
            if (decalRenderer != null && decalRenderer.sharedMaterial != null)
            {
                Material named = new Material(decalRenderer.sharedMaterial);
                named.name = "Mat_LaunchGo";
                decalRenderer.sharedMaterial = named;
            }

            HangarSignPulse pulse = root.AddComponent<HangarSignPulse>();
            pulse.SignLight = lamp;
            pulse.BaseIntensity = 3.15f;
            pulse.BaseEmission = new Color(EmissionTargets.PrimaryR, EmissionTargets.PrimaryG, EmissionTargets.PrimaryB) * EmissionTargets.CapLaunchSign(4.6f);
        }

        private void DressLaunchGoMeshDecal(Transform parent)
        {
            GameObject mesh = new GameObject("LaunchGoMeshDecal");
            mesh.transform.SetParent(parent, false);
            mesh.transform.localPosition = new Vector3(0f, 1.38f, 0.165f);

            CreatePrimitive(PrimitiveType.Cube, "GoG_Top", mesh.transform, _launchDecal,
                new Vector3(-0.22f, 0.15f, 0f), new Vector3(0.24f, 0.05f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoG_Left", mesh.transform, _launchDecal,
                new Vector3(-0.32f, 0f, 0f), new Vector3(0.05f, 0.35f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoG_Bottom", mesh.transform, _launchDecal,
                new Vector3(-0.22f, -0.15f, 0f), new Vector3(0.24f, 0.05f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoG_Mid", mesh.transform, _launchDecal,
                new Vector3(-0.16f, 0f, 0f), new Vector3(0.12f, 0.05f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoG_Spur", mesh.transform, _launchDecal,
                new Vector3(-0.12f, -0.075f, 0f), new Vector3(0.05f, 0.16f, 0.03f), Quaternion.identity);

            CreatePrimitive(PrimitiveType.Cube, "GoO_Top", mesh.transform, _launchDecal,
                new Vector3(0.22f, 0.15f, 0f), new Vector3(0.22f, 0.05f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoO_Bottom", mesh.transform, _launchDecal,
                new Vector3(0.22f, -0.15f, 0f), new Vector3(0.22f, 0.05f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoO_Left", mesh.transform, _launchDecal,
                new Vector3(0.13f, 0f, 0f), new Vector3(0.05f, 0.35f, 0.03f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "GoO_Right", mesh.transform, _launchDecal,
                new Vector3(0.31f, 0f, 0f), new Vector3(0.05f, 0.35f, 0.03f), Quaternion.identity);
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
            root.transform.position = new Vector3(0f, ShipController.PlayHeight, 0f);

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
            Transform noseSlot = CreateSlot("Ship_Nose", slots);
            Transform engineSlot = CreateSlot("Ship_Engine", slots);
            GameObject defaultBody;
            GameObject defaultNose;
            GameObject defaultEngine;
            if (!TryAdoptHero(slots, bodySlot, noseSlot, engineSlot, out defaultBody, out defaultNose, out defaultEngine))
            {
                defaultBody = BuildLegacyBody(bodySlot);
                defaultNose = BuildLegacyNose(noseSlot);
                defaultEngine = BuildLegacyEngine(engineSlot);
            }

            GameObject upgradedBody;
            if (!TryVisual("Ship_Body_Upgrade01", bodySlot, _hull, out upgradedBody))
            {
                upgradedBody = CreatePrimitive(PrimitiveType.Cube, "Ship_Body_Upgrade01", bodySlot, _accent,
                    Vector3.zero, new Vector3(1.25f, 0.7f, 1.65f), Quaternion.identity);
            }

            upgradedBody.SetActive(false);

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

            GameObject hullPlates = new GameObject("HullPlates");
            hullPlates.transform.SetParent(slots, false);
            CreatePrimitive(PrimitiveType.Cube, "Plate_L", hullPlates.transform, _accentWarm,
                new Vector3(-0.72f, 0.08f, 0.05f), new Vector3(0.18f, 0.42f, 1.15f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Plate_R", hullPlates.transform, _accentWarm,
                new Vector3(0.72f, 0.08f, 0.05f), new Vector3(0.18f, 0.42f, 1.15f), Quaternion.identity);
            hullPlates.SetActive(false);

            GameObject spreadPods = new GameObject("SpreadPods");
            spreadPods.transform.SetParent(noseSlot, false);
            CreatePrimitive(PrimitiveType.Cube, "Pod_L", spreadPods.transform, _accentWarm,
                new Vector3(-0.42f, 0.08f, 1.05f), new Vector3(0.16f, 0.14f, 0.38f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Pod_C", spreadPods.transform, _accentWarm,
                new Vector3(0f, 0.16f, 1.12f), new Vector3(0.16f, 0.14f, 0.42f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "Pod_R", spreadPods.transform, _accentWarm,
                new Vector3(0.42f, 0.08f, 1.05f), new Vector3(0.16f, 0.14f, 0.38f), Quaternion.identity);
            spreadPods.SetActive(false);

            GameObject twinBarrels = AttachWeapon(WeaponSockets.Twin, noseSlot);
            GameObject pierceNeedle = AttachWeapon(WeaponSockets.Pierce, noseSlot);
            GameObject seekerRail = AttachWeapon(WeaponSockets.Seeker, noseSlot);

            GameObject ricochetFacet = CreatePrimitive(PrimitiveType.Cube, "RicochetFacet", noseSlot, _projectileRicochet,
                new Vector3(0f, -0.16f, 1.18f), new Vector3(0.28f, 0.16f, 0.28f), Quaternion.Euler(0f, 35f, 0f));
            ricochetFacet.SetActive(false);

            GameObject railHardpoint = AttachWeapon(WeaponSockets.Rail, noseSlot);

            GameObject overchargerGlow = CreatePrimitive(PrimitiveType.Sphere, "OverchargerGlow", noseSlot, _accentHot,
                new Vector3(0f, 0.05f, 1.42f), new Vector3(0.42f, 0.42f, 0.42f), Quaternion.identity);
            overchargerGlow.SetActive(false);

            GameObject afterburnerGlow = CreatePrimitive(PrimitiveType.Sphere, "AfterburnerGlow", engineSlot, _glow,
                new Vector3(0f, 0f, -1.62f), new Vector3(0.55f, 0.4f, 0.55f), Quaternion.identity);
            afterburnerGlow.SetActive(false);

            Transform muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(root.transform, false);
            muzzle.localPosition = new Vector3(0f, 0f, WeaponSockets.MuzzleZ);

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
            visuals.HullPlates = hullPlates;
            visuals.SpreadPods = spreadPods;
            visuals.TwinBarrels = twinBarrels;
            visuals.PierceNeedle = pierceNeedle;
            visuals.SeekerRail = seekerRail;
            visuals.RicochetFacet = ricochetFacet;
            visuals.RailHardpoint = railHardpoint;
            visuals.OverchargerGlow = overchargerGlow;
            visuals.AfterburnerGlow = afterburnerGlow;
            visuals.PreviewGhostMaterial = MakePreviewGhostMaterial();

            ShipHealth health = root.AddComponent<ShipHealth>();
            ShipShooter shooter = root.AddComponent<ShipShooter>();
            ShipController controller = root.AddComponent<ShipController>();

            health.Bind(game, visuals);
            shooter.Bind(loadout, this, muzzle);
            controller.Bind(health, shooter, visuals, camera);
            HangarShipPreview preview = root.AddComponent<HangarShipPreview>();
            preview.Bind(slots);
            ApplyLoadoutVisuals(controller, loadout.State);
            DecorDepthClamp.KeepGameplay(root);
            CastFormShadow(root.transform);
            return controller;
        }

        public GameObject CreateRailChargeVfx(Transform muzzle)
        {
            Transform parent = muzzle != null ? muzzle : transform;
            // Pierce/steel base. Mat_Ship_Glow is amber and reads as a flash.
            Material steel = _hull;
            if (_projectilePierce != null)
            {
                steel = _projectilePierce;
            }
            GameObject glow;
            if (!TryVisual("Rail_Muzzle", parent, steel, out glow))
            {
                glow = CreatePrimitive(PrimitiveType.Sphere, "RailChargeGlow", parent, steel,
                    Vector3.zero, new Vector3(0.42f, 0.42f, 0.72f), Quaternion.identity);
            }
            else
            {
                glow.name = "RailChargeGlow";
                glow.transform.localPosition = Vector3.zero;
                glow.transform.localRotation = Quaternion.identity;
                glow.transform.localScale = Vector3.one;
                StampSharedMaterial(glow, steel);
            }

            glow.SetActive(false);
            return glow;
        }

        private static void StampSharedMaterial(GameObject root, Material material)
        {
            if (root == null || material == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] shared = renderer.sharedMaterials;
                if (shared == null || shared.Length == 0)
                {
                    renderer.sharedMaterial = material;
                    continue;
                }

                Material[] stamped = new Material[shared.Length];
                for (int m = 0; m < stamped.Length; m++)
                {
                    stamped[m] = material;
                }

                renderer.sharedMaterials = stamped;
            }
        }

        public void ApplyLoadoutVisuals(ShipController ship, LoadoutState loadout)
        {
            ApplyLoadoutVisuals(ship, loadout, null);
        }

        public void ApplyLoadoutVisuals(ShipController ship, LoadoutState loadout, LoadoutState owned)
        {
            if (ship != null && ship.Visuals != null)
            {
                ship.Visuals.ApplyLoadout(loadout, owned);
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
            bool keepOutside = _spawnOutside;
            _spawnOutside = false;
            Vector3 cleared = position;
            if (!keepOutside && player != null)
            {
                int wave = waves != null ? waves.ActiveWave : 1;
                float safe = LookPolish.SafeMetres(wave);
                cleared = SpawnClearance.Place(position, player.position, WaveManager.ArenaRadius - 1.5f, 0f, safe);
            }

            root.transform.position = cleared;

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
            collider.center = EnemyCatalog.ColliderCenter(kind);

            string meshAsset = EnemyCatalog.MeshAsset(kind);
            bool premiumMesh = false;
            bool importedMesh = false;
            if (meshAsset != visualName)
            {
                importedMesh = TryEnemyVisual(meshAsset, root.transform);
                premiumMesh = importedMesh;
            }

            if (!importedMesh)
            {
                importedMesh = TryEnemyVisual(visualName, root.transform);
            }

            if (!importedMesh && kind == EnemyKind.Swarmling)
            {
                importedMesh = TryEnemyVisual("Monster_Swarm", root.transform);
            }

            if (!importedMesh)
            {
                if (!BuildMonsterPlaceholder(kind, root.transform))
                {
                    CreatePrimitive(PrimitiveType.Capsule, "Mesh", root.transform, _enemy,
                        Vector3.zero, new Vector3(0.7f, 0.7f, 0.7f), Quaternion.Euler(90f, 0f, 0f));
                    CreatePrimitive(PrimitiveType.Cube, "Canard", root.transform, _accent,
                        new Vector3(0f, 0.15f, -0.35f), new Vector3(1.4f, 0.08f, 0.35f), Quaternion.identity);
                    if (kind == EnemyKind.Sniper)
                    {
                        CreatePrimitive(PrimitiveType.Cylinder, "Scope", root.transform, _accentHot,
                            new Vector3(0f, 0.22f, 0.42f), new Vector3(0.16f, 0.28f, 0.16f), Quaternion.identity);
                    }
                }
            }
            else if (kind == EnemyKind.Mid01)
            {
                int midWorld = 1;
                if (waves != null)
                {
                    midWorld = WorldCatalog.NumberForWave(waves.ActiveWave);
                }

                DressMidMesh(root.transform, midWorld);
            }
            else if (kind == EnemyKind.Sniper)
            {
                DressSniperMesh(root.transform);
            }
            else if (!premiumMesh && kind == EnemyKind.Bomber)
            {
                DressBomberMesh(root.transform);
            }
            else if (!premiumMesh && kind == EnemyKind.Brute)
            {
                DressBruteMesh(root.transform);
            }
            else if (!premiumMesh && (kind == EnemyKind.Swarm || kind == EnemyKind.Swarmling))
            {
                DressSwarmMesh(root.transform, kind == EnemyKind.Swarmling);
            }

            if (importedMesh)
            {
                DressImportedEnemy(root.transform, kind);
                // MeshFit.LegacyRadius stays the old capsule target. Visual size uses the 0.48 max axis.
                ScaleImportedToExtent(root.transform, MeshFit.ReferenceExtent(EnemyCatalog.MeshFitId(kind)));
            }
            else if (kind == EnemyKind.Swarmling)
            {
                ScaleImportedVisual(root.transform, MeshFit.SwarmlingScale);
            }

            FitEnemyCollider(collider, root.transform);

            if (EnemyCatalog.IsMonster(kind) || kind == EnemyKind.Swarmling)
            {
                DressMonsterPresence(root, kind);
            }

            EnemySeeker seeker = root.AddComponent<EnemySeeker>();
            seeker.Initialize(player, waves, kind);
            EnemyMeshTelegraph telegraph = root.AddComponent<EnemyMeshTelegraph>();
            telegraph.Collect(root.transform);
            if (!keepOutside)
            {
                OpeningClearance.ScreenPlace(root.transform);
                seeker.BeginSpawnGrace();
            }
            DecorDepthClamp.KeepGameplay(root);
            CastFormShadow(root.transform);
            if (AudioCues.Instance != null)
            {
                if (kind == EnemyKind.SwarmPod)
                {
                    AudioCues.Instance.PlaySwarmPodSpawn();
                }
                else if (EnemyCatalog.IsMonster(kind) || kind == EnemyKind.Swarmling)
                {
                    AudioCues.Instance.PlayMonsterSpawn(kind);
                }
            }

            return seeker;
        }

        /// <summary>
        /// Elite mark: tint from the renderer's existing material plus an outline
        /// ring that reuses the hot accent material. No new art.
        /// </summary>
        public void MarkElite(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] eliteRenderers = root.GetComponentsInChildren<Renderer>(true);
            for (int eliteIndex = 0; eliteIndex < eliteRenderers.Length; eliteIndex++)
            {
                Renderer eliteRenderer = eliteRenderers[eliteIndex];
                if (eliteRenderer == null || eliteRenderer.sharedMaterial == null)
                {
                    continue;
                }

                string eliteName = eliteRenderer.gameObject.name;
                if (eliteName == "TelegraphRing" || eliteName == "Aura" || eliteName == "EliteOutline")
                {
                    continue;
                }

                Material eliteMat = new Material(eliteRenderer.sharedMaterial);
                eliteMat.color = UiTheme.Focus;
                if (eliteMat.HasProperty("_EmissionColor"))
                {
                    eliteMat.SetColor("_EmissionColor", new Color(1f, 0.55f, 0.12f) * 1.6f);
                }

                eliteRenderer.sharedMaterial = eliteMat;
            }

            MonsterPresence elitePresence = root.GetComponent<MonsterPresence>();
            if (elitePresence != null)
            {
                elitePresence.Configure(UiTheme.Focus, elitePresence.AuraRange);
            }

            if (_accentHot != null)
            {
                CreatePrimitive(
                    PrimitiveType.Cylinder,
                    "EliteOutline",
                    root,
                    _accentHot,
                    new Vector3(0f, 0.06f, 0f),
                    new Vector3(2.6f, 0.015f, 2.6f),
                    Quaternion.identity);
            }
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
            FireMode style = pierce ? FireMode.Pierce : (spread ? FireMode.Spread : FireMode.Bolt);
            return SpawnBolt(origin, direction, speed, damage, pierce, spread, false, EnemyKind.Mid01, style);
        }

        public Projectile SpawnStyledShot(
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            FireMode mode)
        {
            bool pierce = mode == FireMode.Pierce;
            bool spread = mode == FireMode.Spread;
            return SpawnBolt(origin, direction, speed, damage, pierce, spread, false, EnemyKind.Mid01, mode);
        }

        public Projectile SpawnEnemyProjectile(
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            EnemyKind kind)
        {
            return SpawnEnemyProjectile(origin, direction, speed, damage, kind, false);
        }

        public Projectile SpawnEnemyProjectile(
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            EnemyKind kind,
            bool boss)
        {
            return SpawnBolt(origin, direction, speed, damage, false, false, true, kind, FireMode.Bolt, boss);
        }

        private Projectile SpawnBolt(
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            bool pierce,
            bool spread,
            bool hostile,
            EnemyKind kind,
            FireMode style,
            bool boss = false)
        {
            if (!_prewarming)
            {
                Projectile pooled = TakePooled(hostile);
                if (pooled != null)
                {
                    return Relaunch(pooled, origin, direction, speed, damage, pierce, hostile, kind, style, boss);
                }
            }

            if (hostile)
            {
                GameObject playerGo = GameObject.FindGameObjectWithTag(GameTags.Player);
                if (playerGo != null)
                {
                    origin = SpawnClearance.Place(origin, playerGo.transform.position, WaveManager.ArenaRadius - 1.5f);
                }
            }

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

            bool seeker = !hostile && style == FireMode.Seeker;
            bool ricochet = !hostile && style == FireMode.Ricochet;
            bool twin = !hostile && style == FireMode.Twin;
            bool rail = !hostile && style == FireMode.Rail;
            Material railMat = _projectileRail != null ? _projectileRail : _glow;
            Material boltMat = hostile
                ? _projectileEnemy
                : (pierce ? _projectilePierce
                    : (spread ? _projectileSpread
                        : (seeker ? _projectileSeeker
                            : (ricochet ? _projectileRicochet : (rail ? railMat : _projectile)))));
            string visual = hostile ? "Projectile_EnemyBolt" : "Projectile_Bolt";
            GameObject mesh = null;
            bool railSeeker = false;
            if (seeker)
            {
                railSeeker = TryVisual("Rail_Seeker", root.transform, boltMat, out mesh);
            }

            if (railSeeker && mesh != null)
            {
                mesh.transform.localPosition = Vector3.zero;
                mesh.transform.localRotation = Quaternion.identity;
                mesh.transform.localScale = Vector3.one;
            }
            else if (!TryVisual(visual, root.transform, boltMat, out mesh))
            {
                Vector3 fallbackScale = pierce
                    ? new Vector3(0.1f, 0.1f, 1.25f)
                    : (spread ? new Vector3(0.38f, 0.38f, 0.32f)
                        : (seeker ? new Vector3(0.28f, 0.28f, 0.72f)
                            : (twin ? new Vector3(0.14f, 0.14f, 0.55f)
                                : (ricochet ? new Vector3(0.26f, 0.26f, 0.26f) : new Vector3(0.2f, 0.2f, 0.62f)))));
                PrimitiveType primitive = ricochet ? PrimitiveType.Cube : PrimitiveType.Sphere;
                mesh = CreatePrimitive(primitive, "Mesh", root.transform, boltMat,
                    Vector3.zero, fallbackScale, Quaternion.identity);
            }
            else if (mesh != null && !railSeeker)
            {
                if (pierce)
                {
                    mesh.transform.localScale = new Vector3(0.48f, 0.48f, 2.05f);
                }
                else if (spread)
                {
                    mesh.transform.localScale = new Vector3(1.65f, 1.65f, 0.48f);
                }
                else if (seeker)
                {
                    mesh.transform.localScale = new Vector3(1.15f, 1.15f, 1.45f);
                }
                else if (twin)
                {
                    mesh.transform.localScale = new Vector3(0.55f, 0.55f, 0.85f);
                }
                else if (ricochet)
                {
                    mesh.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
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
            else if (seeker && !railSeeker)
            {
                CreatePrimitive(PrimitiveType.Sphere, "SeekerCore", root.transform, boltMat,
                    Vector3.zero, new Vector3(0.42f, 0.42f, 0.42f), Quaternion.identity);
            }
            else if (rail)
            {
                CreatePrimitive(PrimitiveType.Capsule, "RailCore", root.transform, railMat,
                    Vector3.zero, new Vector3(0.16f, 0.16f, 1.8f), Quaternion.Euler(90f, 0f, 0f));
            }
            else if (ricochet)
            {
                CreatePrimitive(PrimitiveType.Cube, "RicochetFacet", root.transform, boltMat,
                    Vector3.zero, new Vector3(0.32f, 0.32f, 0.32f), Quaternion.Euler(45f, 20f, 10f));
            }
            else if (twin)
            {
                CreatePrimitive(PrimitiveType.Cube, "TwinCore", root.transform, boltMat,
                    Vector3.zero, new Vector3(0.12f, 0.12f, 0.7f), Quaternion.identity);
            }

            Material haloMat = pierce ? _projectilePierce
                : (hostile ? _projectileEnemy
                    : (spread ? _projectileSpread
                        : (seeker ? _projectileSeeker
                            : (ricochet ? _projectileRicochet : _projectileHalo))));
            Vector3 haloScale = pierce
                ? new Vector3(0.18f, 0.18f, 1.55f)
                : (spread ? new Vector3(0.74f, 0.74f, 0.46f)
                    : (seeker ? new Vector3(0.55f, 0.55f, 0.95f)
                        : (ricochet ? new Vector3(0.42f, 0.42f, 0.42f) : new Vector3(0.38f, 0.38f, 0.85f))));
            CreatePrimitive(PrimitiveType.Sphere, "BoltHalo", root.transform, haloMat,
                Vector3.zero, haloScale, Quaternion.identity);

            TrailRenderer trail = root.AddComponent<TrailRenderer>();
            const float TrailScale = 0.75f;
            trail.time = (pierce ? 0.42f : (spread ? 0.14f : (seeker ? 0.28f : (ricochet ? 0.18f : (hostile ? 0.16f : 0.12f))))) * TrailScale;
            trail.startWidth = EffectScale.TrailWidth(
                SettingsState.ReduceEffectsEnabled,
                (spread ? 0.52f : (pierce ? 0.07f : (seeker ? 0.28f : (twin ? 0.1f : 0.2f)))) * TrailScale);
            trail.endWidth = pierce ? 0.005f : (spread ? 0.04f : 0.02f);
            trail.minVertexDistance = 0.12f;
            trail.material = boltMat;
            if (pierce || seeker)
            {
                trail.startColor = new Color(0.373f, 0.627f, 0.722f, 0.9f);
                trail.endColor = new Color(0.18f, 0.38f, 0.48f, 0f);
            }
            else if (hostile)
            {
                trail.startColor = new Color(0.722f, 0.353f, 0.157f, 0.88f);
                trail.endColor = new Color(0.4f, 0.16f, 0.08f, 0f);
            }
            else
            {
                trail.startColor = new Color(0.831f, 0.627f, 0.29f, 0.9f);
                trail.endColor = new Color(0.55f, 0.38f, 0.14f, 0f);
            }

            if (!hostile)
            {
                float trailRed;
                float trailGreen;
                float trailBlue;
                if (ShipPaint.TryTrail(out trailRed, out trailGreen, out trailBlue))
                {
                    trail.startColor = new Color(trailRed, trailGreen, trailBlue, 0.9f);
                    trail.endColor = new Color(trailRed * 0.55f, trailGreen * 0.55f, trailBlue * 0.55f, 0f);
                }
            }

            Projectile projectile = root.AddComponent<Projectile>();
            projectile.BindPool(this);
            StyleShot(root, hostile, pierce, spread, style, kind, boss);
            projectile.PrepareReuse(origin);
            int bounces = ricochet ? LoadoutState.RicochetBounces : 0;
            projectile.Launch(direction, speed, damage, pierce, hostile, kind, seeker, bounces);
            if (!hostile && rail)
            {
                ProjectileLightPool.Pulse(root.transform.position, ProjectileLightBudget.KindRail, EmissionTargets.Primary, root.transform);
            }
            else if (!hostile && pierce)
            {
                ProjectileLightPool.Pulse(root.transform.position, ProjectileLightBudget.KindPierce, EmissionTargets.Primary, root.transform);
            }

            DecorDepthClamp.KeepGameplay(root);
            _projectiles.Add(projectile);
            return projectile;
        }

        public void ClearProjectiles()
        {
            Projectile[] live = _projectiles.ToArray();
            _projectiles.Clear();
            for (int i = 0; i < live.Length; i++)
            {
                if (live[i] != null)
                {
                    live[i].Retire();
                }
            }
        }

        private Asteroid CreateAsteroid(AsteroidSize size, Vector3 position, Vector3 drift, WaveManager waves)
        {
            float meters = size == AsteroidSize.Large ? LargeAsteroidMeters : SmallAsteroidMeters;
            int rockBucket;
            string propName = PickAsteroidVisual(size, out rockBucket);
            GameObject root = new GameObject(propName);
            root.tag = GameTags.Asteroid;
            root.transform.SetParent(_threatRoot, false);
            int asteroidSpeed = waves != null ? WorldRules.AsteroidSpeedPercent(waves.ActiveWave) : 100;
            if (asteroidSpeed != 100)
            {
                drift *= asteroidSpeed / 100f;
            }

            drift.y = 0f;
            root.transform.position = new Vector3(position.x, ArenaWrap.PlayY, position.z);

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 0.05f;
            body.angularDamping = 0.05f;
            body.constraints = RigidbodyConstraints.FreezePositionY;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.mass = size == AsteroidSize.Large ? 4f : 1.2f;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.radius = meters * 0.5f;

            Material rock = propName.IndexOf("_B", System.StringComparison.Ordinal) >= 0
                || propName.IndexOf("_C", System.StringComparison.Ordinal) >= 0
                || propName.IndexOf("VariantB", System.StringComparison.Ordinal) >= 0
                ? _asteroidB
                : _asteroid;
            string legacyName = size == AsteroidSize.Large ? "Asteroid_VariantC_Large" : "Asteroid_VariantD_Small";
            bool importedRock = TryVisual(propName, root.transform, rock)
                || TryVisual(size == AsteroidSize.Large ? "Asteroid_Large" : "Asteroid_Small", root.transform, _asteroid)
                || TryVisual(legacyName, root.transform, _asteroid);
            if (!importedRock)
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
            else
            {
                ScaleImportedToSphere(root.transform, MeshFit.AsteroidRadius(size == AsteroidSize.Large, rockBucket));
            }

            FitAsteroidCollider(collider, root.transform);
            CenterAsteroidOnPlayOrigin(collider, root.transform);
            ApplyWatchAsteroid(root.transform, rock, waves);
            CastFormShadow(root.transform);

            Asteroid asteroid = root.AddComponent<Asteroid>();
            asteroid.Initialize(size, waves, this, drift);
            DecorDepthClamp.KeepGameplay(root);
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
            Pickup.Kind kind = Pickup.KindFromName(visualName);
            Material fallback = _accent;
            string pickupMesh = "Pickup_Generic";
            if (kind == Pickup.Kind.Shield)
            {
                fallback = _shield;
            }
            else if (kind == Pickup.Kind.ExtraLife || kind == Pickup.Kind.Health)
            {
                pickupMesh = "Pickup_Heart";
                fallback = _pickupHeart != null ? _pickupHeart : fallback;
            }
            else
            {
                fallback = _pickupGeneric != null ? _pickupGeneric : fallback;
            }

            if (kind == Pickup.Kind.ExtraLife)
            {
                if (!TryVisual(pickupMesh, root.transform, fallback))
                {
                    Material heartFallback = MakeMaterial(
                        "Mat_Pickup_ExtraLife",
                        new Color(0.92f, 0.16f, 0.28f),
                        0.1f,
                        0.62f,
                        new Color(1f, 0.22f, 0.38f));
                    DressExtraLifeHeart(root.transform, heartFallback);
                }
            }
            else if (!TryVisual(pickupMesh, root.transform, fallback) && !TryVisual(visualName, root.transform, fallback))
            {
                CreatePrimitive(PrimitiveType.Sphere, "Mesh", root.transform, fallback,
                    Vector3.zero, new Vector3(0.7f, 0.7f, 0.7f), Quaternion.identity);
            }

            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            float pickupRadius = kind == Pickup.Kind.ExtraLife ? 0.9f : 0.7f;
            pickupRadius *= SinkRuntime.PickupReachMultiplier();
            trigger.radius = pickupRadius;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            float timeout = kind == Pickup.Kind.ExtraLife ? DifficultySettings.ExtraLifeTimeoutSeconds : 0f;
            root.AddComponent<Pickup>().Bind(kind, timeout);
            DecorDepthClamp.KeepGameplay(root);
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

        public void SpawnTelegraphRing(Vector3 position, Color color, float seconds)
        {
            SpawnTelegraphRing(position, color, seconds, TelegraphShape.Ring, Vector3.forward);
        }

        public void SpawnTelegraphRing(Vector3 position, Color color, float seconds, TelegraphShape shape, Vector3 forward)
        {
            GameObject root = new GameObject("TelegraphRing");
            root.transform.position = new Vector3(position.x, 0.04f, position.z);
            TelegraphRing ring = root.AddComponent<TelegraphRing>();
            ring.Play(color, seconds, shape, forward);
        }

        public void SpawnTelegraphLaser(Vector3 from, Vector3 to, Color color, float alpha, float seconds)
        {
            GameObject root = new GameObject("TelegraphLaser");
            TelegraphLaser laser = root.AddComponent<TelegraphLaser>();
            laser.Show(from, to, color, alpha, seconds);
        }

        public void MaybeDropPickup(Vector3 position)
        {
            GameManager dropHost = Object.FindAnyObjectByType<GameManager>();
            if (dropHost != null && dropHost.TutorialActive)
            {
                return;
            }

            float dropCeiling = MutatorRules.DropCeiling(MutatorRuntime.Mask);
            if (RunRng.Unit() > dropCeiling)
            {
                return;
            }

            string[] kinds = { "Pickup_Score", "Pickup_Shield", "Pickup_Health", "Pickup_RapidFire" };
            CreatePickup(kinds[RunRng.Index(kinds.Length)], position);
        }

        private static void DressExtraLifeHeart(Transform parent, Material material)
        {
            CreatePrimitive(PrimitiveType.Sphere, "HeartLobeL", parent, material,
                new Vector3(-0.20f, 0.18f, 0f), new Vector3(0.50f, 0.48f, 0.28f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Sphere, "HeartLobeR", parent, material,
                new Vector3(0.20f, 0.18f, 0f), new Vector3(0.50f, 0.48f, 0.28f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cube, "HeartPoint", parent, material,
                new Vector3(0f, -0.10f, 0f), new Vector3(0.56f, 0.56f, 0.24f), Quaternion.Euler(0f, 0f, 45f));
        }

        public void MaybeDropExtraLife(Vector3 position, EnemyKind kind)
        {
            if (kind == EnemyKind.Swarmling)
            {
                return;
            }

            GameManager game = Object.FindAnyObjectByType<GameManager>();
            if (game != null && game.TutorialActive)
            {
                return;
            }

            if (game != null && game.Session != null && game.Session.Lives >= DifficultySettings.MaxLives)
            {
                return;
            }

            if (RunRng.Unit() > DifficultySettings.ExtraLifeChance)
            {
                return;
            }

            CreatePickup("Pickup_ExtraLife", position);
        }

        private static string PickAsteroidVisual(AsteroidSize size, out int bucket)
        {
            float roll = RunRng.Unit();
            bucket = MeshFit.AsteroidBucket(roll);
            return MeshFit.AsteroidAsset(size == AsteroidSize.Large, bucket);
        }

        private bool TryEnemyVisual(string visualName, Transform parent)
        {
            return TryVisual(visualName, parent, _enemy);
        }

        private static bool IsAstroPlayFloor(string visualName)
        {
            return visualName == "Arena_AstroFloor"
                || visualName == "Arena_Blockout"
                || visualName == "AstroFloor_v2"
                || visualName == "Arena_AstroFloor_v2";
        }

        private static void SinkPlaySurface(GameObject visual, float surfaceY)
        {
            if (visual == null)
            {
                return;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            float delta = bounds.max.y - surfaceY;
            if (delta > 0.001f)
            {
                visual.transform.position += new Vector3(0f, -delta, 0f);
            }
        }

        private const float SpawnDiscRadius = 3.2f;
        private const int SpawnDiscQueue = 2700;
        private const string SpawnDiscObjectName = "Arena_SpawnContrast";
        private static Texture2D _spawnDiscTex;

        private static void ClearSpawnContrast(Transform parent)
        {
            DestroyChild(parent, "SpawnContrast");
            DestroyChild(parent, SpawnDiscObjectName);
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return;
            }

            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }
        }

        private static void DressSpawnContrast(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                return;
            }

            // The arena is dressed before the ship exists. SpawnContrastDisc
            // reads the live hull and parks the disc on that XZ.
            DestroyChild(parent, "SpawnContrast");
            Transform existing = parent.Find(SpawnDiscObjectName);
            GameObject go = null;
            if (existing != null)
            {
                go = existing.gameObject;
            }

            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = SpawnDiscObjectName;
                go.transform.SetParent(parent, false);
            }

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                // CreatePrimitive adds a MeshCollider. Destroy is deferred and
                // the next physics step logs a concave-collider error.
                DestroyImmediate(collider);
            }

            // The W1 centre plate is on the decor camera. A decor-layer disc
            // loses the depth test against that plate. Layer 0 is drawn after
            // the play camera clears depth, so the 3.2 m disc composites over
            // the plate and the ship still occludes it. The old 16 m square
            // reached the lip; this radius does not.
            go.layer = 0;
            go.transform.localScale = Vector3.one;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            float diameter = SpawnDiscRadius * 2f;
            Vector3 lossy = go.transform.lossyScale;
            float spanX = Mathf.Abs(lossy.x);
            float spanY = Mathf.Abs(lossy.y);
            if (spanX <= 0.001f)
            {
                spanX = 1f;
            }

            if (spanY <= 0.001f)
            {
                spanY = 1f;
            }

            go.transform.localScale = new Vector3(diameter / spanX, diameter / spanY, 1f);
            Material mat = new Material(shader);
            mat.name = "Mat_Env_SpawnContrast";
            mat.mainTexture = SpawnDiscTexture();
            mat.color = new Color(0.006f, 0.008f, 0.014f, 1f);
            mat.renderQueue = SpawnDiscQueue;
            if (mat.HasProperty("_ZWrite"))
            {
                mat.SetFloat("_ZWrite", 0f);
            }

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mat;
                renderer.sortingOrder = -2;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            SpawnContrastDisc tracker = go.GetComponent<SpawnContrastDisc>();
            if (tracker == null)
            {
                go.AddComponent<SpawnContrastDisc>();
            }
        }

        private static Texture2D SpawnDiscTexture()
        {
            if (_spawnDiscTex != null)
            {
                return _spawnDiscTex;
            }

            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "Tex_Env_SpawnContrast";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float radius = Mathf.Sqrt((dx * dx) + (dy * dy));
                    float alpha = 1f;
                    if (radius > 0.8f)
                    {
                        float edge = (radius - 0.8f) / 0.2f;
                        edge = Mathf.Clamp01(edge);
                        float smooth = edge * edge * (3f - (2f * edge));
                        alpha = 1f - smooth;
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true);
            _spawnDiscTex = tex;
            return tex;
        }

        private static void DressArenaFloorRenderers(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.sortingOrder = -2;
            }
        }

        private static void DressAstroFloorColliders(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            StripImportedFloorColliders(visual);

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer mesh = renderers[i] as MeshRenderer;
                if (mesh == null)
                {
                    continue;
                }

                if (ContainsIgnoreCase(mesh.name, "Rim") || ContainsIgnoreCase(mesh.sharedMaterial != null ? mesh.sharedMaterial.name : string.Empty, "Rim"))
                {
                    continue;
                }

                Bounds local = mesh.localBounds;
                if (local.size.x < 0.15f && local.size.z < 0.15f)
                {
                    continue;
                }

                BoxCollider box = mesh.gameObject.AddComponent<BoxCollider>();
                box.center = new Vector3(local.center.x, local.max.y - 0.06f, local.center.z);
                box.size = new Vector3(local.size.x, 0.12f, local.size.z);
                box.isTrigger = true;
            }
        }

        private static void StripImportedFloorColliders(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Collider[] existing = visual.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] == null)
                {
                    continue;
                }

                existing[i].enabled = false;
                Object.Destroy(existing[i]);
            }
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

        private void DressMidMesh(Transform root, int worldNumber)
        {
            float midScale = ReadabilityPalette.MidEmissionFor(worldNumber);
            DressEnemyEmission(
                root,
                new Color(ReadabilityPalette.MidR, ReadabilityPalette.MidG, ReadabilityPalette.MidB) * midScale);
        }

        /// <summary>
        /// Worlds 3 and 4 multiply the shared asteroid albedo. Other worlds
        /// keep the material colour. Only _Color is written so emission stays.
        /// </summary>
        private static void ApplyWatchAsteroid(Transform root, Material rock, WaveManager waves)
        {
            if (root == null)
            {
                return;
            }

            int rockWorld = 1;
            if (waves != null)
            {
                rockWorld = WorldCatalog.NumberForWave(waves.ActiveWave);
            }

            float albedoMul = ReadabilityPalette.AsteroidAlbedoMul(rockWorld);
            if (albedoMul <= 1.001f)
            {
                return;
            }

            Color tint = Color.white;
            if (rock != null)
            {
                tint = rock.color;
            }

            tint = new Color(tint.r * albedoMul, tint.g * albedoMul, tint.b * albedoMul, tint.a);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", tint);
            for (int rockIndex = 0; rockIndex < renderers.Length; rockIndex++)
            {
                if (renderers[rockIndex] != null)
                {
                    renderers[rockIndex].SetPropertyBlock(block);
                }
            }
        }

        private void DressSniperMesh(Transform root)
        {
            float sniperR;
            float sniperG;
            float sniperB;
            EmissionTargets.Desaturate(0.18f, 0.72f, 1f, EmissionTargets.SniperDesat, out sniperR, out sniperG, out sniperB);
            DressEnemyEmission(root, new Color(sniperR, sniperG, sniperB));
        }

        private void DressBomberMesh(Transform root)
        {
            DressEnemyEmission(root, new Color(1f, 0.42f, 0.08f) * 1.45f);
        }

        private void DressShipComplete(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            DressEnemyEmission(root.transform, new Color(1f, 0.58f, 0.18f) * 1.25f);
        }

        private void DressBruteMesh(Transform root)
        {
            DressEnemyEmission(root, new Color(1f, 0.28f, 0.08f) * 1.6f);
        }

        private void DressSwarmMesh(Transform root, bool swarmling)
        {
            DressEnemyEmission(root, swarmling
                ? new Color(0.25f, 1f, 0.45f) * 1.8f
                : new Color(0.12f, 0.85f, 1f) * 1.5f);
        }

        private static void FitEnemyCollider(CapsuleCollider collider, Transform root)
        {
            if (collider == null || root == null)
            {
                return;
            }

            Bounds local;
            if (!TryEnemyMeshBounds(root, out local))
            {
                return;
            }

            Vector3 size = local.size;
            float radius = 0.5f * Mathf.Max(size.x, size.y);
            float height = Mathf.Max(radius * 2f, size.z);
            if (radius < 0.01f)
            {
                return;
            }

            collider.center = local.center;
            collider.radius = radius;
            collider.height = height;
        }

        private static void FitAsteroidCollider(SphereCollider collider, Transform root)
        {
            if (collider == null || root == null)
            {
                return;
            }

            Bounds local;
            if (!TryRendererLocalBounds(root, out local, false))
            {
                return;
            }

            float radius = Mathf.Max(local.extents.x, Mathf.Max(local.extents.y, local.extents.z));
            if (radius < 0.01f)
            {
                return;
            }

            collider.center = local.center;
            collider.radius = radius;
        }

        private static void CenterAsteroidOnPlayOrigin(SphereCollider collider, Transform root)
        {
            if (collider == null || root == null)
            {
                return;
            }

            Vector3 shift = collider.center;
            if (shift.sqrMagnitude < 0.0001f)
            {
                return;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                child.localPosition -= shift;
            }

            collider.center = Vector3.zero;
        }

        private static bool TryEnemyMeshBounds(Transform root, out Bounds local)
        {
            return TryRendererLocalBounds(root, out local, true);
        }

        private static bool TryRendererLocalBounds(Transform root, out Bounds local, bool hullOnly)
        {
            local = new Bounds(Vector3.zero, Vector3.zero);
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                if (renderer == null || (hullOnly && !IsEnemyHullRenderer(renderer)))
                {
                    continue;
                }

                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    EncapsulateMeshBounds(root, renderer.transform, filter.sharedMesh.bounds, ref local, ref any);
                    continue;
                }

                EncapsulateWorldAabb(root, renderer.bounds, ref local, ref any);
            }

            return any && local.size.sqrMagnitude > 0.0001f;
        }

        private static void EncapsulateMeshBounds(
            Transform root,
            Transform source,
            Bounds meshBounds,
            ref Bounds local,
            ref bool any)
        {
            Vector3 center = meshBounds.center;
            Vector3 extents = meshBounds.extents;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + new Vector3(extents.x * x, extents.y * y, extents.z * z);
                        EncapsulateRootPoint(root, source.TransformPoint(corner), ref local, ref any);
                    }
                }
            }
        }

        private static void EncapsulateWorldAabb(Transform root, Bounds world, ref Bounds local, ref bool any)
        {
            Vector3 min = world.min;
            Vector3 max = world.max;
            EncapsulateRootPoint(root, new Vector3(min.x, min.y, min.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(min.x, min.y, max.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(min.x, max.y, min.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(min.x, max.y, max.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(max.x, min.y, min.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(max.x, min.y, max.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(max.x, max.y, min.z), ref local, ref any);
            EncapsulateRootPoint(root, new Vector3(max.x, max.y, max.z), ref local, ref any);
        }

        private static void EncapsulateRootPoint(Transform root, Vector3 world, ref Bounds local, ref bool any)
        {
            Vector3 p = root.InverseTransformPoint(world);
            if (!any)
            {
                local = new Bounds(p, Vector3.zero);
                any = true;
                return;
            }

            local.Encapsulate(p);
        }

        private static bool IsEnemyHullRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            return MeshFit.CountsForEnemyHitbox(renderer.gameObject.name);
        }

        private static void ScaleImportedVisual(Transform root, float scale)
        {
            if (root == null || root.childCount == 0)
            {
                return;
            }

            root.GetChild(0).localScale = Vector3.one * scale;
        }

        private static void ScaleImportedToExtent(Transform root, float authoredExtent)
        {
            if (root == null || authoredExtent <= 0.01f)
            {
                return;
            }

            Bounds local;
            if (!TryEnemyMeshBounds(root, out local))
            {
                return;
            }

            float measured = local.size.x;
            if (local.size.y > measured)
            {
                measured = local.size.y;
            }

            if (local.size.z > measured)
            {
                measured = local.size.z;
            }

            ScaleImportedVisual(root, MeshFit.Uniform(measured, authoredExtent));
        }

        private static void ScaleImportedToRadius(Transform root, float authoredRadius)
        {
            if (root == null || authoredRadius <= 0.01f)
            {
                return;
            }

            Bounds local;
            if (!TryEnemyMeshBounds(root, out local))
            {
                return;
            }

            float measured;
            float unusedHeight;
            MeshFit.Capsule(local.size.x, local.size.y, local.size.z, out measured, out unusedHeight);
            ScaleImportedVisual(root, MeshFit.Uniform(measured, authoredRadius));
        }

        private static void ScaleImportedToSphere(Transform root, float authoredRadius)
        {
            if (root == null || authoredRadius <= 0.01f)
            {
                return;
            }

            Bounds local;
            if (!TryRendererLocalBounds(root, out local, false))
            {
                return;
            }

            float measured = MeshFit.SphereRadius(local.size.x, local.size.y, local.size.z);
            ScaleImportedVisual(root, MeshFit.Uniform(measured, authoredRadius));
        }

        private bool BuildMonsterPlaceholder(EnemyKind kind, Transform parent)
        {
            if (kind == EnemyKind.Brute)
            {
                CreatePrimitive(PrimitiveType.Cube, "Hull", parent, _brute,
                    new Vector3(0f, 0.22f, 0.05f), new Vector3(1.55f, 1.05f, 1.35f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Chest", parent, _accentHot,
                    new Vector3(0f, 0.38f, 0.55f), new Vector3(0.85f, 0.55f, 0.28f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Shoulder_L", parent, _accentHot,
                    new Vector3(-0.92f, 0.62f, 0.08f), new Vector3(0.48f, 0.42f, 0.72f), Quaternion.Euler(0f, 0f, 12f));
                CreatePrimitive(PrimitiveType.Cube, "Shoulder_R", parent, _accentHot,
                    new Vector3(0.92f, 0.62f, 0.08f), new Vector3(0.48f, 0.42f, 0.72f), Quaternion.Euler(0f, 0f, -12f));
                CreatePrimitive(PrimitiveType.Cube, "Pauldron", parent, _brute,
                    new Vector3(0f, 0.78f, -0.15f), new Vector3(1.15f, 0.22f, 0.85f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Horn", parent, _brute,
                    new Vector3(0f, 0.98f, 0.52f), new Vector3(0.22f, 0.62f, 0.22f), Quaternion.Euler(22f, 0f, 0f));
                CreatePrimitive(PrimitiveType.Cube, "Tusk_L", parent, _accentWarm,
                    new Vector3(-0.28f, 0.55f, 0.78f), new Vector3(0.12f, 0.12f, 0.42f), Quaternion.Euler(12f, -8f, 0f));
                CreatePrimitive(PrimitiveType.Cube, "Tusk_R", parent, _accentWarm,
                    new Vector3(0.28f, 0.55f, 0.78f), new Vector3(0.12f, 0.12f, 0.42f), Quaternion.Euler(12f, 8f, 0f));
                return true;
            }

            if (kind == EnemyKind.Swarm)
            {
                CreatePrimitive(PrimitiveType.Sphere, "Core", parent, _swarm,
                    new Vector3(0f, 0.28f, 0f), new Vector3(1.28f, 1.18f, 1.28f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Sphere, "Halo", parent, _swarmling,
                    new Vector3(0f, 0.42f, 0f), new Vector3(1.62f, 0.22f, 1.62f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Sphere, "Nodule_A", parent, _swarmling,
                    new Vector3(0.72f, 0.48f, 0.22f), new Vector3(0.42f, 0.42f, 0.42f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Sphere, "Nodule_B", parent, _swarmling,
                    new Vector3(-0.58f, 0.58f, -0.28f), new Vector3(0.36f, 0.36f, 0.36f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Sphere, "Nodule_C", parent, _swarm,
                    new Vector3(0.15f, 0.82f, -0.55f), new Vector3(0.32f, 0.32f, 0.32f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cylinder, "Stalk", parent, _swarm,
                    new Vector3(0f, 1.02f, 0f), new Vector3(0.18f, 0.32f, 0.18f), Quaternion.identity);
                return true;
            }

            if (kind == EnemyKind.Swarmling)
            {
                CreatePrimitive(PrimitiveType.Sphere, "Body", parent, _swarmling,
                    Vector3.zero, new Vector3(0.52f, 0.4f, 0.52f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Sphere, "Eye", parent, _swarm,
                    new Vector3(0f, 0.08f, 0.22f), new Vector3(0.18f, 0.18f, 0.18f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Fin", parent, _swarm,
                    new Vector3(0f, 0.1f, -0.24f), new Vector3(0.48f, 0.05f, 0.2f), Quaternion.identity);
                return true;
            }

            return false;
        }

        private void BuildArenaLayout(ArenaLayoutId layout)
        {
            if (_arenaRoot == null)
            {
                return;
            }

            Transform root = new GameObject("Arena_Layout").transform;
            root.SetParent(_arenaRoot.transform, false);
            Material wash = LayoutWash(layout);
            float washReach = ArenaShapeCatalog.Active.BoundingRadius * 1.92f;
            CreatePrimitive(PrimitiveType.Cylinder, "LayoutWash", root, wash,
                new Vector3(0f, -0.02f, 0f),
                new Vector3(washReach, 0.03f, washReach),
                Quaternion.identity);

            switch (layout)
            {
                case ArenaLayoutId.PylonRing:
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = (Mathf.PI * 2f * i) / 6f;
                        PlaceHazardSpike(root, Ring(14f, angle), 1.35f, false, angle * Mathf.Rad2Deg);
                    }

                    break;
                case ArenaLayoutId.SplitTrench:
                    PlaceLayoutWall(root, new Vector3(-8.2f, 1.4f, 0f), new Vector3(1.3f, 2.8f, 22f), _layoutMagenta);
                    PlaceLayoutWall(root, new Vector3(8.2f, 1.4f, 0f), new Vector3(1.3f, 2.8f, 22f), _layoutMagenta);
                    break;
                case ArenaLayoutId.MineBelt:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (Mathf.PI * 2f * i) / 8f + 0.2f;
                        PlaceHazardSpike(root, Ring(18.5f, angle), 1.15f, true, angle * Mathf.Rad2Deg + 180f);
                    }

                    break;
                case ArenaLayoutId.CrossGates:
                    PlaceLayoutWall(root, new Vector3(-11f, 1.3f, 0f), new Vector3(10f, 2.6f, 1.4f), _layoutLime);
                    PlaceLayoutWall(root, new Vector3(11f, 1.3f, 0f), new Vector3(10f, 2.6f, 1.4f), _layoutLime);
                    PlaceLayoutWall(root, new Vector3(0f, 1.3f, -11f), new Vector3(1.4f, 2.6f, 10f), _layoutLime);
                    PlaceLayoutWall(root, new Vector3(0f, 1.3f, 11f), new Vector3(1.4f, 2.6f, 10f), _layoutLime);
                    PlaceHazardSpike(root, new Vector3(7.5f, 0f, 7.5f), 1.05f, false, 45f);
                    PlaceHazardSpike(root, new Vector3(-7.5f, 0f, -7.5f), 1.05f, false, 225f);
                    break;
                case ArenaLayoutId.DebrisIslands:
                    PlaceDebrisIsland(root, new Vector3(12.5f, 0f, 10.5f), 1f);
                    PlaceDebrisIsland(root, new Vector3(-13f, 0f, 9f), 0.85f);
                    PlaceDebrisIsland(root, new Vector3(11f, 0f, -12f), 0.95f);
                    PlaceDebrisIsland(root, new Vector3(-10.5f, 0f, -11.5f), 1.1f);
                    PlaceHazardSpike(root, new Vector3(0f, 0f, 16.5f), 1.25f, true, 0f);
                    break;
                case ArenaLayoutId.SpokeRing:
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = (Mathf.PI * 0.5f * i);
                        Vector3 mid = Ring(11.5f, angle);
                        Vector3 scale = Mathf.Abs(Mathf.Cos(angle)) > 0.5f
                            ? new Vector3(10.5f, 1.6f, 1.15f)
                            : new Vector3(1.15f, 1.6f, 10.5f);
                        PlaceLayoutWall(root, mid + new Vector3(0f, 0.8f, 0f), scale, _layoutCyan);
                        PlaceHazardSpike(root, Ring(7.2f, angle + 0.4f), 0.85f, false, angle * Mathf.Rad2Deg);
                    }

                    break;
                default:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (Mathf.PI * 2f * i) / 8f;
                        float rimX;
                        float rimZ;
                        ArenaShapeCatalog.Active.SpawnRing(angle, -0.8f, out rimX, out rimZ);
                        CreatePrimitive(PrimitiveType.Cylinder, "RimLight", root, _layoutCyan,
                            new Vector3(rimX, 0.35f, rimZ),
                            new Vector3(0.22f, 0.35f, 0.22f), Quaternion.identity);
                    }

                    break;
            }

            for (int layoutChild = 0; layoutChild < root.childCount; layoutChild++)
            {
                DecorDepthClamp.ClampTop(root.GetChild(layoutChild).gameObject, DecorDepthRules.MaxDecorTop);
            }

            if (ArenaLayout.UsesHazardSpikes(layout) && AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHazardActivate();
            }
        }

        private void PlaceDebrisIsland(Transform parent, Vector3 position, float scale)
        {
            GameObject island;
            if (TryVisual("Arena_RockIsland_A", parent, _layoutRust, out island))
            {
                island.transform.localPosition = new Vector3(position.x, 0f, position.z);
                island.transform.localScale = Vector3.one * (1.15f * scale);
                DressArenaFloorRenderers(island);
                BoxCollider box = island.GetComponent<BoxCollider>();
                if (box == null)
                {
                    box = island.AddComponent<BoxCollider>();
                }

                box.enabled = true;
                box.center = new Vector3(0.64f, 1.55f, 0.15f);
                box.size = new Vector3(6.8f, 3.1f, 4.36f);
                Rigidbody body = island.GetComponent<Rigidbody>();
                if (body == null)
                {
                    body = island.AddComponent<Rigidbody>();
                }

                body.useGravity = false;
                body.isKinematic = true;
                body.constraints = RigidbodyConstraints.FreezeAll;
                if (island.GetComponent<ArenaHazard>() == null)
                {
                    island.AddComponent<ArenaHazard>().Damaging = false;
                }
            }
            else
            {
                PlaceLayoutWall(parent, position + new Vector3(0f, 0.7f * scale, 0f),
                    new Vector3(3.2f * scale, 1.4f * scale, 2.6f * scale), _layoutRust);
            }

            PlaceHazardSpike(parent, position + new Vector3(1.4f * scale, 0f, 0.6f * scale), 0.85f * scale, false, 25f);
        }

        private void PlaceLayoutWall(Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = CreatePrimitive(PrimitiveType.Cube, "LayoutWall", parent, material,
                position, scale, Quaternion.identity);
            Collider existing = wall.GetComponent<Collider>();
            if (existing != null)
            {
                Destroy(existing);
            }

            BoxCollider box = wall.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = Vector3.one;
            Rigidbody body = wall.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeAll;
            wall.AddComponent<ArenaHazard>().Damaging = false;
            CreatePrimitive(PrimitiveType.Cube, "LayoutRail", parent, _layoutRail,
                position + new Vector3(0f, scale.y * 0.52f, 0f),
                new Vector3(scale.x * 1.04f, 0.07f, scale.z * 1.04f),
                Quaternion.identity);
        }

        private void PlaceHazardSpike(Transform parent, Vector3 position, float scale, bool damaging, float yaw)
        {
            GameObject root = new GameObject(damaging ? "Arena_Hazard_Spike" : "Arena_Pylon_Spike");
            root.transform.SetParent(parent, false);
            Vector3 clearedHazard = position;
            GameObject playerGo = GameObject.FindGameObjectWithTag(GameTags.Player);
            if (playerGo != null)
            {
                clearedHazard = SpawnClearance.Place(position, playerGo.transform.position, WaveManager.ArenaRadius - 1.5f);
            }

            root.transform.position = clearedHazard;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject visual;
            if (!TryVisual("Arena_Hazard_Spike", root.transform, _hazardSpike, out visual))
            {
                CreatePrimitive(PrimitiveType.Cylinder, "SpikeStem", root.transform, _hazardSpike,
                    new Vector3(0f, 1.4f, 0f), new Vector3(0.45f, 1.4f, 0.45f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "SpikeTip", root.transform, _accentHot,
                    new Vector3(0f, 3.05f, 0f), new Vector3(0.55f, 0.7f, 0.55f), Quaternion.identity);
            }

            root.transform.localScale = Vector3.one * Mathf.Max(0.35f, scale);
            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1.4f, 0f);
            collider.radius = 0.55f;
            collider.height = 3.2f;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeAll;
            ArenaHazard hazard = root.AddComponent<ArenaHazard>();
            hazard.Damaging = damaging;
            hazard.Damage = damaging ? ArenaHazard.DamagingSpikeDamage : 1;
            DressHazardSpike(root, hazard, damaging);
        }

        private static Vector3 Ring(float radius, float angle)
        {
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private Material LayoutWash(ArenaLayoutId layout)
        {
            switch (layout)
            {
                case ArenaLayoutId.PylonRing:
                    return _washCyan;
                case ArenaLayoutId.SplitTrench:
                    return _washMagenta;
                case ArenaLayoutId.MineBelt:
                    return _washOrange;
                case ArenaLayoutId.CrossGates:
                    return _washLime;
                case ArenaLayoutId.DebrisIslands:
                    return _washRust;
                case ArenaLayoutId.SpokeRing:
                    return _washCyan;
                default:
                    return _washCyan;
            }
        }

        private void DressMonsterPresence(GameObject root, EnemyKind kind)
        {
            if (root == null)
            {
                return;
            }

            Color aura;
            float range;
            if (kind == EnemyKind.Brute)
            {
                aura = new Color(1f, 0.32f, 0.1f);
                range = 8.2f;
            }
            else if (kind == EnemyKind.Swarmling)
            {
                aura = new Color(0.28f, 1f, 0.48f);
                range = 4.1f;
            }
            else
            {
                aura = new Color(0.12f, 0.88f, 1f);
                range = 6.4f;
            }

            MonsterPresence presence = root.GetComponent<MonsterPresence>();
            if (presence == null)
            {
                presence = root.AddComponent<MonsterPresence>();
            }

            presence.Configure(aura, range);
        }

        private void DressHazardSpike(GameObject root, ArenaHazard hazard, bool damaging)
        {
            if (root == null)
            {
                return;
            }

            Material ringMat = damaging ? _accentHot : _layoutRail;
            CreatePrimitive(PrimitiveType.Cylinder, "SpikeRing", root.transform, ringMat,
                new Vector3(0f, 0.06f, 0f), new Vector3(1.15f, 0.05f, 1.15f), Quaternion.identity);
            CreatePrimitive(PrimitiveType.Cylinder, "SpikeWell", root.transform, damaging ? _hazardSpike : _layoutCyan,
                new Vector3(0f, 0.02f, 0f), new Vector3(0.72f, 0.03f, 0.72f), Quaternion.identity);

            Transform existing = root.transform.Find("SpikeLight");
            GameObject lightGo = existing != null ? existing.gameObject : new GameObject("SpikeLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            Light glow = lightGo.GetComponent<Light>();
            if (glow == null)
            {
                glow = lightGo.AddComponent<Light>();
            }

            glow.type = LightType.Point;
            glow.shadows = LightShadows.None;
            glow.color = damaging ? new Color(1f, 0.38f, 0.08f) : new Color(0.35f, 0.78f, 1f);
            glow.range = damaging ? 6.4f : 4.6f;
            glow.intensity = damaging ? 1.55f : 0.78f;
            Color emit = damaging
                ? new Color(1f, 0.28f, 0.05f) * 2.2f
                : new Color(0.2f, 0.72f, 1f) * 1.15f;
            DressEnemyEmission(root.transform, emit);
            if (hazard != null)
            {
                hazard.DressPulse(emit, glow);
            }
        }

        private static void DressEnemyEmission(Transform root, Color emission)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_EmissionColor", emission);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && !UsesEnemySlotMaterials(renderers[i]))
                {
                    renderers[i].SetPropertyBlock(block);
                }
            }
        }

        private Material RemapImported(string importedName, Material fallback)
        {
            string name = importedName ?? string.Empty;
            Material enemySlot = RemapEnemySlot(name);
            if (enemySlot != null)
            {
                return enemySlot;
            }

            // W2–W6 Accent/Glow strips are the decor beams. W4 Glow used to
            // fall through to Mat_Ship_Glow (the white pillar). Edge posts use
            // the cool-grey unlit beam, not player amber.
            if (IsArenaDecorBeam(name))
            {
                return _w3Beam != null ? _w3Beam : _arena;
            }

            // Hero preview slots. Named so the hangar block can find them.
            // Albedo matches the hull until that block runs.
            if (ContainsIgnoreCase(name, "Mat_Preview_Cyan"))
            {
                return _previewCyan != null ? _previewCyan : fallback;
            }

            if (ContainsIgnoreCase(name, "Mat_Preview_Accent"))
            {
                return _previewAccent != null ? _previewAccent : fallback;
            }

            if ((ContainsIgnoreCase(name, "Glass") || ContainsIgnoreCase(name, "Tile"))
                && (ContainsIgnoreCase(name, "Arena")
                    || ContainsIgnoreCase(name, "Astro")
                    || ContainsIgnoreCase(name, "Floor")))
            {
                return _arena != null ? _arena : fallback;
            }

            if (ContainsIgnoreCase(name, "Mat_Ship_AccentHot_Hero"))
            {
                return _accentHotHero != null ? _accentHotHero : _accent;
            }

            if (ContainsIgnoreCase(name, "Magenta")
                && (ContainsIgnoreCase(name, "Ship") || ContainsIgnoreCase(name, "Weapon")))
            {
                return _shipMagenta != null ? _shipMagenta : fallback;
            }

            if (ContainsIgnoreCase(name, "Cyan")
                && (ContainsIgnoreCase(name, "Ship") || ContainsIgnoreCase(name, "Weapon"))
                && !ContainsIgnoreCase(name, "Hangar"))
            {
                return _shipCyan != null ? _shipCyan : fallback;
            }

            if (ContainsIgnoreCase(name, "Mat_Ship_Accent_Hot"))
            {
                return _accentHot;
            }

            if (ContainsIgnoreCase(name, "Mat_Ship_Glow"))
            {
                return _glow;
            }

            if (ContainsIgnoreCase(name, "Mat_Ship_Hull"))
            {
                return _hull;
            }

            if (ContainsIgnoreCase(name, "Mat_Ship_Accent"))
            {
                return _accent;
            }

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

            if (ContainsIgnoreCase(name, "Mat_FX_Pickup_Heart"))
            {
                return _pickupHeart != null ? _pickupHeart : fallback;
            }

            if (ContainsIgnoreCase(name, "Mat_FX_Pickup") || ContainsIgnoreCase(name, "Mat_FX_Cyan"))
            {
                if (fallback != null)
                {
                    return fallback;
                }

                return _pickupGeneric;
            }

            if (ContainsIgnoreCase(name, "Hazard") || ContainsIgnoreCase(name, "Spike"))
            {
                return _hazardSpike;
            }

            if (ContainsIgnoreCase(name, "Mat_Enemy_Brute_Glow")
                || (ContainsIgnoreCase(name, "Brute") && ContainsIgnoreCase(name, "Glow")))
            {
                return _bruteGlow != null ? _bruteGlow : _accentHot;
            }

            if ((ContainsIgnoreCase(name, "Swarm") || ContainsIgnoreCase(name, "Bomber"))
                && ContainsIgnoreCase(name, "Glow"))
            {
                return _swarmGlow != null ? _swarmGlow : _enemyGlow;
            }

            if (ContainsIgnoreCase(name, "Monster") || ContainsIgnoreCase(name, "Brute"))
            {
                if (ContainsIgnoreCase(name, "Swarm") || ContainsIgnoreCase(name, "Swarmling"))
                {
                    return ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Glow")
                        ? _swarmGlow
                        : _swarm;
                }

                return ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Glow")
                    ? _bruteGlow
                    : _brute;
            }

            if (ContainsIgnoreCase(name, "Bomber"))
            {
                if (ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Canopy")
                    || ContainsIgnoreCase(name, "Eye") || ContainsIgnoreCase(name, "Stripe")
                    || ContainsIgnoreCase(name, "Glow") || ContainsIgnoreCase(name, "Engine")
                    || ContainsIgnoreCase(name, "Bay"))
                {
                    return _enemyGlow != null ? _enemyGlow : _accentHot;
                }

                return _enemy;
            }

            if (ContainsIgnoreCase(name, "Complete"))
            {
                if (ContainsIgnoreCase(name, "Engine") || ContainsIgnoreCase(name, "Glow"))
                {
                    return _glow;
                }

                if (ContainsIgnoreCase(name, "Nose") || ContainsIgnoreCase(name, "Accent")
                    || ContainsIgnoreCase(name, "Canopy"))
                {
                    return _accent;
                }

                if (ContainsIgnoreCase(name, "Glass"))
                {
                    return _glass;
                }

                return _hull;
            }

            if (ContainsIgnoreCase(name, "Enemy") || ContainsIgnoreCase(name, "Mid")
                || ContainsIgnoreCase(name, "Swarm")
                || ContainsIgnoreCase(name, "Scout") || ContainsIgnoreCase(name, "Gunner")
                || ContainsIgnoreCase(name, "Drone") || ContainsIgnoreCase(name, "Sniper"))
            {
                if (ContainsIgnoreCase(name, "Swarm"))
                {
                    if (ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Glow"))
                    {
                        return _swarmGlow != null ? _swarmGlow : _enemy;
                    }

                    if (ContainsIgnoreCase(name, "Swarmling"))
                    {
                        return _swarmling != null ? _swarmling : _swarm;
                    }

                    return _swarm != null ? _swarm : _enemy;
                }

                if (ContainsIgnoreCase(name, "Glow") || ContainsIgnoreCase(name, "Core"))
                {
                    return _enemyGlow != null ? _enemyGlow : _accentHot;
                }

                if (ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Canopy")
                    || ContainsIgnoreCase(name, "Eye") || ContainsIgnoreCase(name, "Stripe"))
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
                return ContainsIgnoreCase(name, "Asteroid_B") || ContainsIgnoreCase(name, "Asteroid_C") || ContainsIgnoreCase(name, "Variant")
                    ? _asteroidB
                    : _asteroid;
            }

            if (ContainsIgnoreCase(name, "Enemy"))
            {
                return ContainsIgnoreCase(name, "Accent") ? _accentHot : _enemy;
            }

            if (ContainsIgnoreCase(name, "AstroRim") || name == "Mat_AstroRim")
            {
                return _astroRim;
            }

            if (ContainsIgnoreCase(name, "Astro") || ContainsIgnoreCase(name, "Arena"))
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

        private Material RemapEnemySlot(string name)
        {
            if (ContainsIgnoreCase(name, "Mat_Enemy_Thruster"))
            {
                return _enemyThruster;
            }

            if (ContainsIgnoreCase(name, "Mat_Enemy_Telegraph"))
            {
                return _enemyTelegraph;
            }

            if (ContainsIgnoreCase(name, "Mat_Enemy_Trim"))
            {
                return _enemyTrim;
            }

            if (ContainsIgnoreCase(name, "Mat_Enemy_Core"))
            {
                return _enemyCore;
            }

            if (ContainsIgnoreCase(name, "Mat_Enemy_Body"))
            {
                return _enemyBody;
            }

            return null;
        }

        private void DressImportedEnemy(Transform root, EnemyKind kind)
        {
            HideEnemyDebris(root);
            FollowBomberHatch(root, kind);
            PaintMidSlots(root, kind);
        }

        /// <summary>
        /// Look capture and other previews. Same slots as a live enemy, no seeker.
        /// </summary>
        public bool PresentEnemyMesh(EnemyKind kind, Transform parent)
        {
            if (parent == null)
            {
                return false;
            }

            GameObject instance;
            string meshAsset = EnemyCatalog.MeshAsset(kind);
            bool imported = TryVisual(meshAsset, parent, _enemyBody != null ? _enemyBody : _enemy, out instance);
            if (!imported)
            {
                imported = TryVisual(EnemyCatalog.VisualName(kind), parent, _enemy, out instance);
            }

            if (!imported || instance == null)
            {
                return false;
            }

            DressImportedEnemy(instance.transform, kind);
            ScaleImportedToExtent(parent, MeshFit.ReferenceExtent(EnemyCatalog.MeshFitId(kind)));
            return true;
        }

        private void PaintMidSlots(Transform root, EnemyKind kind)
        {
            if (root == null || kind != EnemyKind.Mid01)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] slots = renderer.sharedMaterials;
                if (slots == null || slots.Length == 0)
                {
                    continue;
                }

                bool changed = false;
                for (int slot = 0; slot < slots.Length; slot++)
                {
                    Material mat = slots[slot];
                    if (mat == null)
                    {
                        continue;
                    }

                    if (mat == _enemyCore || mat.name.IndexOf("Mat_Enemy_V4_Core", System.StringComparison.Ordinal) >= 0)
                    {
                        slots[slot] = _enemyMidCore;
                        changed = true;
                    }
                    else if (mat == _enemyBody || mat.name.IndexOf("Mat_Enemy_V4_Body", System.StringComparison.Ordinal) >= 0)
                    {
                        slots[slot] = _enemyMidBody;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = slots;
                }
            }
        }

        private static void HideEnemyDebris(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string part = renderer.gameObject.name;
                if (!MeshFit.IsEnemyDebrisName(part))
                {
                    continue;
                }

                renderer.enabled = false;
            }
        }

        private static void FollowBomberHatch(Transform root, EnemyKind kind)
        {
            if (root == null || kind != EnemyKind.Bomber)
            {
                return;
            }

            Transform hatch = null;
            Transform[] parts = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] != null && parts[i].name == "Hatch_Open")
                {
                    hatch = parts[i];
                    break;
                }
            }

            if (hatch == null)
            {
                return;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                Transform part = parts[i];
                if (part == null || part == hatch || part == root)
                {
                    continue;
                }

                if (part.name.IndexOf("Core", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (part.IsChildOf(hatch) || hatch.IsChildOf(part))
                {
                    continue;
                }

                part.SetParent(hatch, true);
            }
        }

        private static bool UsesEnemySlotMaterials(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            Material[] slots = renderer.sharedMaterials;
            if (slots == null)
            {
                return false;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                Material mat = slots[i];
                if (mat != null && mat.name.IndexOf("Mat_Enemy_V4_", System.StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsIgnoreCase(string value, string token)
        {
            return value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// A renderer that is only a decor beam is turned off.
        /// Mixed meshes keep the floor and draw the beam submesh in cool grey.
        /// </summary>
        private static void HideInactiveBeams(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                Material[] slots = renderer.sharedMaterials;
                if (slots == null || slots.Length == 0)
                {
                    continue;
                }

                bool anyBeam = false;
                bool onlyBeam = true;
                for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
                {
                    Material slot = slots[slotIndex];
                    bool beam = slot != null && slot.name.IndexOf("W3_Beam", System.StringComparison.Ordinal) >= 0;
                    if (beam)
                    {
                        anyBeam = true;
                    }
                    else
                    {
                        onlyBeam = false;
                    }
                }

                if (anyBeam && onlyBeam)
                {
                    renderer.enabled = false;
                }
            }
        }

        private static Material MakeAdditiveThruster(string materialName, Color color)
        {
            Shader shader = Shader.Find("Particles/Additive");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Particles/Additive");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                return MakeMaterial(materialName, color, 0f, 0.2f, color);
            }

            Material material = new Material(shader);
            material.name = materialName;
            material.color = color;
            if (material.HasProperty("_TintColor"))
            {
                material.SetColor("_TintColor", color);
            }

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.renderQueue = 3000;
            return material;
        }

        private static Material MakeEdgeBeam(string materialName)
        {
            Color steel = new Color(PremiumLook.BeamPostR, PremiumLook.BeamPostG, PremiumLook.BeamPostB, 1f);
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                return MakeMaterial(materialName, steel, 0f, 0.2f);
            }

            Material material = new Material(shader);
            material.name = materialName;
            material.color = steel;
            material.renderQueue = 2000;
            return material;
        }

        /// <summary>
        /// Arena blockout Accent and Glow strips (Mat_Arena_W4_Glow and the
        /// same pair on worlds 2–6). Not ship or hangar materials.
        /// </summary>
        private static bool IsArenaDecorBeam(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (!ContainsIgnoreCase(name, "Arena"))
            {
                return false;
            }

            if (ContainsIgnoreCase(name, "Ship") || ContainsIgnoreCase(name, "Hangar"))
            {
                return false;
            }

            bool strip = ContainsIgnoreCase(name, "Accent") || ContainsIgnoreCase(name, "Glow");
            if (!strip)
            {
                return false;
            }

            return ContainsIgnoreCase(name, "W2")
                || ContainsIgnoreCase(name, "W3")
                || ContainsIgnoreCase(name, "W4")
                || ContainsIgnoreCase(name, "W5")
                || ContainsIgnoreCase(name, "W6");
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

        private bool TryAdoptHero(
            Transform slots,
            Transform bodySlot,
            Transform noseSlot,
            Transform engineSlot,
            out GameObject bodyPart,
            out GameObject nosePart,
            out GameObject enginePart)
        {
            bodyPart = null;
            nosePart = null;
            enginePart = null;
            GameObject heroRoot;
            if (!TryVisual("Ship_Hero", slots, _hull, out heroRoot) || heroRoot == null)
            {
                return false;
            }

            FitImportedExtent(heroRoot, WeaponSockets.HeroSpanMeters);
            Transform bodyNode = FindNamed(heroRoot.transform, "Ship_Body");
            Transform noseNode = FindNamed(heroRoot.transform, "Ship_Nose");
            Transform engineNode = FindNamed(heroRoot.transform, "Ship_Engine");
            if (bodyNode == null || noseNode == null || engineNode == null)
            {
                Destroy(heroRoot);
                return false;
            }

            bodyNode.SetParent(bodySlot, true);
            noseNode.SetParent(noseSlot, true);
            engineNode.SetParent(engineSlot, true);
            bodyPart = bodyNode.gameObject;
            nosePart = noseNode.gameObject;
            enginePart = engineNode.gameObject;
            heroRoot.SetActive(false);
            Destroy(heroRoot);
            return true;
        }

        private GameObject BuildLegacyBody(Transform bodySlot)
        {
            GameObject legacyBody;
            if (!TryVisual("Ship_Body", bodySlot, _hull, out legacyBody))
            {
                legacyBody = CreatePrimitive(PrimitiveType.Cube, "Mesh", bodySlot, _hull,
                    Vector3.zero, new Vector3(1.1f, 0.6f, 1.5f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Canopy", legacyBody.transform, _glass,
                    new Vector3(0f, 0.28f, 0.15f), new Vector3(0.55f, 0.22f, 0.7f), Quaternion.identity);
            }

            return legacyBody;
        }

        private GameObject BuildLegacyNose(Transform noseSlot)
        {
            GameObject legacyNose;
            if (!TryVisual("Ship_Nose", noseSlot, _accent, out legacyNose))
            {
                legacyNose = CreatePrimitive(PrimitiveType.Cube, "Mesh_Default", noseSlot, _accent,
                    new Vector3(0f, 0f, 1.1f), new Vector3(0.45f, 0.35f, 0.7f), Quaternion.identity);
            }

            return legacyNose;
        }

        private GameObject BuildLegacyEngine(Transform engineSlot)
        {
            GameObject legacyEngine;
            if (!TryVisual("Ship_Engine", engineSlot, _hull, out legacyEngine))
            {
                legacyEngine = new GameObject("Mesh_Default");
                legacyEngine.transform.SetParent(engineSlot, false);
                CreatePrimitive(PrimitiveType.Cube, "Mesh", legacyEngine.transform, _hull,
                    new Vector3(0f, 0f, -1.05f), new Vector3(0.8f, 0.45f, 0.55f), Quaternion.identity);
                CreatePrimitive(PrimitiveType.Cube, "Glow", legacyEngine.transform, _glow,
                    new Vector3(0f, 0f, -1.38f), new Vector3(0.45f, 0.28f, 0.18f), Quaternion.identity);
            }

            return legacyEngine;
        }

        private GameObject AttachWeapon(int socket, Transform noseSlot)
        {
            float mountX;
            float mountY;
            float mountZ;
            WeaponSockets.Mount(socket, out mountX, out mountY, out mountZ);
            GameObject mountObject = new GameObject(WeaponSockets.Name(socket));
            mountObject.transform.SetParent(noseSlot, false);
            mountObject.transform.localPosition = new Vector3(mountX, mountY, mountZ);
            mountObject.transform.localRotation = Quaternion.identity;
            mountObject.transform.localScale = Vector3.one;
            Transform mount = mountObject.transform;

            GameObject visual;
            bool heroWeapon = TryVisual(WeaponSockets.AssetName(socket), mount, WeaponFallback(socket), out visual);
            if (heroWeapon && visual != null)
            {
                visual.name = WeaponSockets.VisualName(socket);
                FitImportedExtent(visual, WeaponSockets.AuthoredLength(socket));
                visual.SetActive(false);
                return visual;
            }

            if (socket == WeaponSockets.Rail)
            {
                GameObject legacyRail;
                if (TryVisual("Rail_Hardpoint", mount, _hull, out legacyRail))
                {
                    legacyRail.name = "RailHardpoint";
                    legacyRail.transform.localPosition = Vector3.zero;
                    legacyRail.transform.localRotation = Quaternion.identity;
                    legacyRail.transform.localScale = Vector3.one;
                    legacyRail.SetActive(false);
                    return legacyRail;
                }
            }

            visual = CreateWeaponPrimitive(socket, mount);
            if (visual != null)
            {
                visual.SetActive(false);
            }

            return visual;
        }

        private Material WeaponFallback(int socket)
        {
            if (socket == WeaponSockets.Pierce)
            {
                return _projectilePierce;
            }

            if (socket == WeaponSockets.Seeker)
            {
                return _projectileSeeker;
            }

            if (socket == WeaponSockets.Twin)
            {
                return _accent;
            }

            return _hull;
        }

        private GameObject CreateWeaponPrimitive(int socket, Transform mount)
        {
            if (socket == WeaponSockets.Twin)
            {
                GameObject twin = new GameObject("TwinBarrels");
                twin.transform.SetParent(mount, false);
                CreatePrimitive(PrimitiveType.Cylinder, "Twin_L", twin.transform, _accent,
                    new Vector3(-0.18f, 0f, 0.24f), new Vector3(0.12f, 0.26f, 0.12f), Quaternion.Euler(90f, 0f, 0f));
                CreatePrimitive(PrimitiveType.Cylinder, "Twin_R", twin.transform, _accent,
                    new Vector3(0.18f, 0f, 0.24f), new Vector3(0.12f, 0.26f, 0.12f), Quaternion.Euler(90f, 0f, 0f));
                return twin;
            }

            if (socket == WeaponSockets.Pierce)
            {
                return CreatePrimitive(PrimitiveType.Cube, "PierceNeedle", mount, _projectilePierce,
                    new Vector3(0f, 0.02f, 0.30f), new Vector3(0.08f, 0.08f, 0.60f), Quaternion.identity);
            }

            if (socket == WeaponSockets.Seeker)
            {
                return CreatePrimitive(PrimitiveType.Cube, "SeekerRail", mount, _projectileSeeker,
                    new Vector3(0f, 0f, 0.16f), new Vector3(0.22f, 0.12f, 0.33f), Quaternion.identity);
            }

            if (socket == WeaponSockets.Rail)
            {
                return CreatePrimitive(PrimitiveType.Cylinder, "RailHardpoint", mount, _accentHot,
                    new Vector3(0f, 0.04f, 0.36f), new Vector3(0.08f, 0.36f, 0.08f), Quaternion.Euler(90f, 0f, 0f));
            }

            return null;
        }

        private static void FitImportedExtent(GameObject visual, float authoredExtent)
        {
            if (visual == null || authoredExtent <= 0f)
            {
                return;
            }

            float measured = MaxRendererAxis(visual);
            float factor = WeaponSockets.SnapFactor(measured, authoredExtent);
            if (factor < 0.99f || factor > 1.01f)
            {
                visual.transform.localScale = new Vector3(factor, factor, factor);
            }
        }

        private static float MaxRendererAxis(GameObject visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            Bounds box = new Bounds(visual.transform.position, Vector3.zero);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer piece = renderers[rendererIndex];
                if (piece == null)
                {
                    continue;
                }

                if (!any)
                {
                    box = piece.bounds;
                    any = true;
                }
                else
                {
                    box.Encapsulate(piece.bounds);
                }
            }

            if (!any)
            {
                return 0f;
            }

            return Mathf.Max(box.size.x, Mathf.Max(box.size.y, box.size.z));
        }

        private static Transform FindNamed(Transform root, string wanted)
        {
            if (root == null || string.IsNullOrEmpty(wanted))
            {
                return null;
            }

            if (root.name == wanted)
            {
                return root;
            }

            for (int childIndex = 0; childIndex < root.childCount; childIndex++)
            {
                Transform found = FindNamed(root.GetChild(childIndex), wanted);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Texture2D FloorNoise()
        {
            const int Size = 64;
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGB24, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "ArenaFloorNoise";
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    int hash = ((x * 73) + (y * 19) + ((x * y) % 11)) & 255;
                    float n = 0.90f + ((hash / 255f) * 0.10f);
                    tex.SetPixel(x, y, new Color(n, n, n, 1f));
                }
            }

            tex.Apply(false, true);
            return tex;
        }

        private static void CastFormShadow(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer form = renderers[i];
                if (form == null || form.gameObject.name == "ContactBlob")
                {
                    continue;
                }

                form.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                form.receiveShadows = true;
            }

            ContactBlob.Attach(root);
        }

        private Material MakeShipRim(
            string materialName,
            Color albedo,
            Color rimColor,
            float rimPower,
            float rimStrength,
            Color emission,
            float metallic,
            float smoothness,
            bool clearEmissionWhenRim)
        {
            Shader rimShader = Shader.Find("AGR/RimLit");
            bool rimLit = rimShader != null;
            Shader shader = rimShader;
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return MakeMaterial(materialName, albedo, metallic, smoothness, emission);
            }

            Material material = new Material(shader);
            material.name = materialName;
            material.color = albedo;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            Color emit = emission;
            if (rimLit && clearEmissionWhenRim)
            {
                emit = Color.black;
            }

            if (rimLit)
            {
                material.SetColor("_RimColor", rimColor);
                material.SetFloat("_RimPower", rimPower);
                material.SetFloat("_RimStrength", rimStrength);
            }

            if (emit.maxColorComponent > 0.01f || rimLit)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emit);
            }

            return material;
        }

        private static Material MakePreviewGhostMaterial()
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader);
            material.name = "Mat_HangarPreviewGhost";
            Color ghost = new Color(0.831f, 0.627f, 0.29f, 0.42f);
            material.color = ghost;
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.55f, 0.38f, 0.12f));
            return material;
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

        private void PrewarmProjectiles()
        {
            if (_projectileRoot == null)
            {
                return;
            }

            _prewarming = true;
            Vector3 park = new Vector3(0f, -80f, 0f);
            int player = 0;
            while (player < LookPolish.PlayerPool)
            {
                Projectile shot = SpawnBolt(park, Vector3.forward, 1f, 1, false, false, false, EnemyKind.Mid01, FireMode.Bolt, false);
                if (shot != null)
                {
                    shot.Retire();
                }

                player++;
            }

            int enemy = 0;
            while (enemy < LookPolish.EnemyPool)
            {
                Projectile shot = SpawnBolt(park, Vector3.forward, 1f, 1, false, false, true, EnemyKind.Mid01, FireMode.Bolt, false);
                if (shot != null)
                {
                    shot.Retire();
                }

                enemy++;
            }

            _prewarming = false;
        }

        private Projectile TakePooled(bool hostile)
        {
            List<Projectile> pool = hostile ? _enemyFree : _playerFree;
            int index = pool.Count - 1;
            while (index >= 0)
            {
                Projectile shot = pool[index];
                pool.RemoveAt(index);
                if (shot != null)
                {
                    return shot;
                }

                index--;
            }

            return null;
        }

        public void Reclaim(Projectile shot)
        {
            if (shot == null)
            {
                return;
            }

            _projectiles.Remove(shot);
            shot.transform.position = new Vector3(0f, -80f, 0f);
            List<Projectile> pool = shot.Hostile ? _enemyFree : _playerFree;
            if (!pool.Contains(shot))
            {
                pool.Add(shot);
            }
        }

        private Projectile Relaunch(
            Projectile shot,
            Vector3 origin,
            Vector3 direction,
            float speed,
            int damage,
            bool pierce,
            bool hostile,
            EnemyKind kind,
            FireMode style,
            bool boss)
        {
            GameObject root = shot.gameObject;
            root.SetActive(true);
            root.transform.position = origin;
            bool spread = !hostile && style == FireMode.Spread;
            bool seeker = !hostile && style == FireMode.Seeker;
            bool ricochet = !hostile && style == FireMode.Ricochet;
            StyleShot(root, hostile, pierce, spread, style, kind, boss);
            shot.PrepareReuse(origin);
            int bounces = ricochet ? LoadoutState.RicochetBounces : 0;
            shot.Launch(direction, speed, damage, pierce, hostile, kind, seeker, bounces);
            if (!hostile && style == FireMode.Rail)
            {
                ProjectileLightPool.Pulse(root.transform.position, ProjectileLightBudget.KindRail, EmissionTargets.Primary, root.transform);
            }
            else if (!hostile && pierce)
            {
                ProjectileLightPool.Pulse(root.transform.position, ProjectileLightBudget.KindPierce, EmissionTargets.Primary, root.transform);
            }

            if (!_projectiles.Contains(shot))
            {
                _projectiles.Add(shot);
            }

            return shot;
        }

        private void StyleShot(
            GameObject root,
            bool hostile,
            bool pierce,
            bool spread,
            FireMode style,
            EnemyKind kind,
            bool boss)
        {
            if (root == null)
            {
                return;
            }

            bool reduce = SettingsState.ReduceEffectsEnabled;
            bool seeker = !hostile && style == FireMode.Seeker;
            bool ricochet = !hostile && style == FireMode.Ricochet;
            bool twin = !hostile && style == FireMode.Twin;
            bool rail = !hostile && style == FireMode.Rail;
            float length = LookPolish.BoltLength;
            float width = LookPolish.BoltWidth;
            float haloLength = LookPolish.BoltHaloLength;
            float haloWidth = LookPolish.BoltHaloWidth;
            float emission = LookPolish.BoltEmission;
            float trailTime = LookPolish.BoltTrail;
            float trailWidth = LookPolish.BoltTrailWidth;
            Color core = new Color(LookPolish.CoreR, LookPolish.CoreG, LookPolish.CoreB, 1f);
            Color halo = new Color(LookPolish.HaloR, LookPolish.HaloG, LookPolish.HaloB, LookPolish.HaloAlpha(reduce));
            if (pierce)
            {
                length = LookPolish.PierceLength;
                width = LookPolish.PierceWidth;
                haloLength = LookPolish.PierceHaloLength;
                haloWidth = LookPolish.PierceHaloWidth;
                emission = LookPolish.PierceEmission;
                trailTime = LookPolish.PierceTrail;
                trailWidth = 0.04f;
                halo = new Color(LookPolish.SteelR, LookPolish.SteelG, LookPolish.SteelB, LookPolish.HaloAlpha(reduce));
            }
            else if (spread)
            {
                length = LookPolish.SpreadLength;
                width = LookPolish.SpreadWidth;
                haloLength = LookPolish.SpreadLength + 0.12f;
                haloWidth = LookPolish.SpreadWidth + 0.08f;
                emission = LookPolish.SpreadEmission;
                trailTime = LookPolish.SpreadTrail;
                trailWidth = 0.06f;
            }
            else if (twin)
            {
                length = LookPolish.TwinLength;
                width = LookPolish.TwinWidth;
                haloLength = LookPolish.TwinLength + 0.10f;
                haloWidth = LookPolish.TwinWidth + 0.06f;
                emission = LookPolish.TwinEmission;
                trailTime = LookPolish.TwinTrail;
                trailWidth = 0.05f;
            }
            else if (seeker)
            {
                length = LookPolish.SeekerLength;
                width = LookPolish.SeekerWidth;
                haloLength = LookPolish.SeekerLength + 0.12f;
                haloWidth = LookPolish.SeekerWidth + 0.08f;
                emission = LookPolish.SeekerEmission;
                trailTime = LookPolish.SeekerTrail;
                trailWidth = 0.06f;
                halo = new Color(LookPolish.SteelR, LookPolish.SteelG, LookPolish.SteelB, LookPolish.HaloAlpha(reduce));
            }
            else if (ricochet)
            {
                length = LookPolish.RicochetLength;
                width = LookPolish.RicochetWidth;
                haloLength = LookPolish.RicochetLength + 0.10f;
                haloWidth = LookPolish.RicochetWidth + 0.08f;
                emission = LookPolish.RicochetEmission;
                trailTime = LookPolish.RicochetTrail;
                trailWidth = 0.06f;
            }
            else if (rail)
            {
                length = LookPolish.RailLength;
                width = LookPolish.RailWidth;
                haloLength = LookPolish.RailLength;
                haloWidth = LookPolish.RailWidth + 0.04f;
                emission = LookPolish.RailEmission;
                trailTime = LookPolish.RailBeamFade;
                trailWidth = 0.04f;
                core = Color.white;
            }

            Material bodyMat = hostile ? _projectileEnemy : (rail ? _projectileRail : _projectile);
            Material haloMat = pierce ? _projectilePierce : _projectileHalo;
            bool diamond = hostile && LookPolish.EnemyIsDiamond((int)kind, boss);
            float diameter = LookPolish.EnemySize(boss);
            PrimitiveType bodyType = PrimitiveType.Capsule;
            if (hostile)
            {
                bodyType = PrimitiveType.Sphere;
            }

            GameObject body = EnsurePiece(root.transform, "ShotBody", bodyType, bodyMat);
            GameObject haloPiece = EnsurePiece(root.transform, "ShotHalo", PrimitiveType.Capsule, haloMat);
            GameObject gem = EnsurePiece(root.transform, "ShotDiamond", PrimitiveType.Cube, bodyMat);
            GameObject rim = EnsurePiece(root.transform, "ShotRim", PrimitiveType.Sphere, _projectileEnemy);
            GameObject rimGem = EnsurePiece(root.transform, "ShotRimDiamond", PrimitiveType.Cube, _projectileEnemy);
            if (rim.GetComponent<ShotRim>() == null)
            {
                rim.AddComponent<ShotRim>();
            }

            if (rimGem.GetComponent<ShotRim>() == null)
            {
                rimGem.AddComponent<ShotRim>();
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = false;
            }

            if (hostile)
            {
                float rimSize = diameter + (LookPolish.EnemyRimThickness * 2f);
                GameObject shown = diamond ? gem : body;
                GameObject rimShown = diamond ? rimGem : rim;
                shown.SetActive(true);
                body.SetActive(!diamond);
                gem.SetActive(diamond);
                rim.SetActive(!diamond);
                rimGem.SetActive(diamond);
                if (diamond)
                {
                    shown.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    shown.transform.localScale = new Vector3(diameter, diameter, diameter * 0.35f);
                    rimShown.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    rimShown.transform.localScale = new Vector3(rimSize, rimSize, rimSize * 0.35f);
                }
                else
                {
                    shown.transform.localRotation = Quaternion.identity;
                    shown.transform.localScale = new Vector3(diameter, diameter, diameter);
                    rimShown.transform.localRotation = Quaternion.identity;
                    rimShown.transform.localScale = new Vector3(rimSize, rimSize, rimSize);
                }

                haloPiece.SetActive(false);
                TintPiece(shown, new Color(LookPolish.EnemyCoreR, LookPolish.EnemyCoreG, LookPolish.EnemyCoreB, 1f), LookPolish.EnemyCoreEmission);
                TintPiece(rimShown, new Color(LookPolish.EnemyRimR, LookPolish.EnemyRimG, LookPolish.EnemyRimB, 1f), LookPolish.EnemyRimEmission);
            }
            else
            {
                gem.SetActive(false);
                rim.SetActive(false);
                rimGem.SetActive(false);
                body.SetActive(true);
                haloPiece.SetActive(true);
                Quaternion along = Quaternion.Euler(90f, 0f, 0f);
                body.transform.localRotation = along;
                body.transform.localScale = new Vector3(width, length * 0.5f, width);
                haloPiece.transform.localRotation = along;
                haloPiece.transform.localScale = new Vector3(haloWidth, haloLength * 0.5f, haloWidth);
                TintPiece(body, core, emission);
                TintPiece(haloPiece, halo, emission * 0.35f);
            }

            TrailRenderer trail = root.GetComponent<TrailRenderer>();
            if (trail == null)
            {
                return;
            }

            float specWidth = LookPolish.ShotTrailWidth(reduce, trailWidth);
            float legacyWidth = EffectScale.TrailWidth(reduce, trailWidth);
            trail.time = LookPolish.ShotTrailTime(reduce, hostile, trailTime);
            trail.startWidth = specWidth;
            if (legacyWidth < 0f)
            {
                trail.startWidth = legacyWidth;
            }

            trail.endWidth = 0f;
            trail.minVertexDistance = seeker ? LookPolish.SeekerMinVertex : 0.08f;
            trail.emitting = !hostile && trail.time > 0.001f;
            if (!hostile)
            {
                trail.startColor = new Color(halo.r, halo.g, halo.b, 0.9f);
                trail.endColor = new Color(halo.r * 0.55f, halo.g * 0.55f, halo.b * 0.55f, 0f);
                float trailRed;
                float trailGreen;
                float trailBlue;
                if (ShipPaint.TryTrail(out trailRed, out trailGreen, out trailBlue))
                {
                    trail.startColor = new Color(trailRed, trailGreen, trailBlue, 0.9f);
                    trail.endColor = new Color(trailRed * 0.55f, trailGreen * 0.55f, trailBlue * 0.55f, 0f);
                }
            }
        }

        private GameObject EnsurePiece(Transform parent, string pieceName, PrimitiveType type, Material material)
        {
            Transform found = parent.Find(pieceName);
            if (found != null)
            {
                return found.gameObject;
            }

            Material paint = material != null ? material : _projectile;
            return CreatePrimitive(type, pieceName, parent, paint, Vector3.zero, Vector3.one, Quaternion.identity);
        }

        private static void TintPiece(GameObject piece, Color color, float emission)
        {
            if (piece == null)
            {
                return;
            }

            Renderer renderer = piece.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.enabled = true;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            block.SetColor("_EmissionColor", color * emission);
            renderer.SetPropertyBlock(block);
        }
    }

    /// <summary>
    /// Parks the W1 spawn disc on the ship's start XZ. Gameplay spawn is the
    /// hull centre the first time the ship is in the arena, and again after a
    /// teleport (hangar studio, ResetForWave, capture pin). Flight is ignored.
    /// The disc sits above the floor plate and below the hull. Keep-out must
    /// not slide it; OpeningClearance treats Arena_SpawnContrast as fixed.
    /// </summary>
    public sealed class SpawnContrastDisc : MonoBehaviour
    {
        public const float ShipMatchMetres = 0.5f;
        private const float TeleportMetres = 2f;

        private Vector3 _spawn;
        private bool _hasSpawn;
        private Vector3 _lastHull;
        private bool _seenHull;

        private void LateUpdate()
        {
            Vector3 hull;
            float shipBottom;
            float ignored;
            if (!TryShipHull(out hull, out ignored, out shipBottom))
            {
                return;
            }

            if (!_seenHull)
            {
                _seenHull = true;
                _lastHull = hull;
                if (InArena(hull))
                {
                    _spawn = hull;
                    _hasSpawn = true;
                }
            }
            else
            {
                float jump = Planar(_lastHull, hull);
                _lastHull = hull;
                if (jump > TeleportMetres && InArena(hull))
                {
                    _spawn = hull;
                    _hasSpawn = true;
                }
            }

            if (!_hasSpawn)
            {
                return;
            }

            gameObject.layer = 0;
            float y = DiscY(_spawn.x, _spawn.z, shipBottom);
            transform.SetPositionAndRotation(new Vector3(_spawn.x, y, _spawn.z), Quaternion.Euler(-90f, 0f, 0f));
        }

        public static bool TryShipHull(out Vector3 center, out float radius, out float bottom)
        {
            center = Vector3.zero;
            radius = 0.8f;
            bottom = ShipController.PlayHeight;
            ShipController ship = UnityEngine.Object.FindAnyObjectByType<ShipController>();
            if (ship == null)
            {
                return false;
            }

            Renderer[] renderers = ship.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            Bounds box = new Bounds(ship.transform.position, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!HullRenderer(renderer))
                {
                    continue;
                }

                if (!any)
                {
                    box = renderer.bounds;
                    any = true;
                }
                else
                {
                    box.Encapsulate(renderer.bounds);
                }
            }

            if (!any)
            {
                center = ship.transform.position;
                bottom = ship.transform.position.y;
                return true;
            }

            center = box.center;
            bottom = box.min.y;
            float wide = box.extents.x;
            if (box.extents.z > wide)
            {
                wide = box.extents.z;
            }

            if (wide > 0.2f)
            {
                radius = wide;
            }

            return true;
        }

        public static bool HullRenderer(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
            {
                return false;
            }

            string name = renderer.gameObject.name;
            if (name == "ContactBlob" || name == "ShieldBubble" || name == "Arena_SpawnContrast")
            {
                return false;
            }

            return renderer.GetComponentInParent<Canvas>() == null;
        }

        private static bool InArena(Vector3 hull)
        {
            return hull.z > -40f && (hull.x * hull.x) + (hull.z * hull.z) < 6400f;
        }

        private static float Planar(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt((dx * dx) + (dz * dz));
        }

        private static float DiscY(float x, float z, float shipBottom)
        {
            float surface;
            if (!FloorSurface.TryTop(x, z, shipBottom, out surface))
            {
                return 0.05f;
            }

            return FloorSurface.PlaceY(surface, shipBottom);
        }
    }
}
