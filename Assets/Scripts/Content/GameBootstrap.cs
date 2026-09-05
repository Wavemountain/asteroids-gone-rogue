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

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.18f);

            EnsureEventSystem();
            EnsureLight();

            GameSession session = new GameSession();
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
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureLight()
        {
            Light[] lights = FindObjectsOfType<Light>();
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
            light.color = new Color(0.92f, 0.95f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        }
    }
}
