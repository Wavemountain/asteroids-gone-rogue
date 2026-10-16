using UnityEngine;
using UnityEngine.EventSystems;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Wires the Week 1 loop in Play. Press Play in the Editor to start in the hangar.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public const string ProductTitle = "Asteroids gone rogue";

        private void Awake()
        {
            if (GetComponent<GameManager>() != null)
            {
                return;
            }

            Boot();
        }

        private void Boot()
        {
            Application.targetFrameRate = 60;
            Physics.gravity = Vector3.zero;

            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            ShaderWarmup.Touch();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(PremiumLook.OutsideR, PremiumLook.OutsideG, PremiumLook.OutsideB, 1f);
            camera.fieldOfView = 54f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 280f;
            camera.allowHDR = true;
            QualitySettings.pixelLightCount = 6;
            DecorCameraStack.Attach(camera);
            PostStack.Attach(camera);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(
                PremiumLook.AmbientChannel(0.12f),
                PremiumLook.AmbientChannel(0.14f),
                PremiumLook.AmbientChannel(0.18f));

            EnsureEventSystem();
            EnsureLight();

            DifficultySettings.EnsureLoaded();
            GameSession session = new GameSession();
            session.ResetLives(DifficultySettings.StartLives);
            LoadoutState loadoutState = new LoadoutState();

            gameObject.AddComponent<AudioCues>();

            ContentFactory factory = gameObject.AddComponent<ContentFactory>();
            factory.BuildPalette();
            factory.BuildArena();

            PlayerLoadout loadout = gameObject.AddComponent<PlayerLoadout>();
            loadout.Bind(loadoutState);

            WaveManager waves = gameObject.AddComponent<WaveManager>();
            HangarShop shop = gameObject.AddComponent<HangarShop>();
            GameManager game = gameObject.AddComponent<GameManager>();
            GameUi ui = GameUi.Build(ProductTitle);

            ShipController ship = factory.BuildShip(loadout, game, camera);

            game.Initialize(session, loadout, waves, shop, ui, factory, ship);
            waves.Initialize(factory, game, ship.transform);
            shop.Initialize(loadout, session, game);
            ui.Initialize(game, shop, session, loadout, ship);

            FollowCamera follow = camera.GetComponent<FollowCamera>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<FollowCamera>();
            }

            follow.SetTarget(ship.transform);
            game.EnterHangar();
            ui.EnsureHangarPreview(ship);
            StoreCaptureDirector.Ensure();
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
            GameObject eventGo = eventSystem != null ? eventSystem.gameObject : new GameObject("EventSystem");
            eventGo.name = "EventSystem";
            if (eventSystem == null)
            {
                eventSystem = eventGo.AddComponent<EventSystem>();
            }

            eventSystem.enabled = true;

            StandaloneInputModule module = eventGo.GetComponent<StandaloneInputModule>();
            if (module == null)
            {
                module = eventGo.AddComponent<StandaloneInputModule>();
            }

            module.enabled = true;
            module.horizontalAxis = "Horizontal";
            module.verticalAxis = "Vertical";
            module.submitButton = "Submit";
            module.cancelButton = "Cancel";
        }

        private static void EnsureLight()
        {
            Light[] lights = FindObjectsByType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    return;
                }
            }

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            float warmR;
            float warmG;
            float warmB;
            PremiumLook.WarmKey(0.92f, 0.95f, 1f, out warmR, out warmG, out warmB);
            light.color = new Color(warmR, warmG, warmB, 1f);
            light.shadows = LightShadows.None;
            light.shadowStrength = PremiumLook.ShadowStrength;
            lightObject.transform.rotation = Quaternion.Euler(PremiumLook.KeyElevation, PremiumLook.KeyYaw, 0f);
        }
    }
}
