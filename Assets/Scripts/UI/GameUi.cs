using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    public sealed class GameUi : MonoBehaviour
    {
        private GameManager _game;
        private HangarShop _shop;
        private GameSession _session;
        private PlayerLoadout _loadout;
        private ShipController _ship;
        private WaveManager _waves;

        private Text _title;
        private Text _world;
        private Text _hud;
        private Text _status;
        private Text _credits;
        private Text _hint;
        private GameObject _menuRoot;
        private Button _primary;
        private Text _primaryLabel;
        private Button[] _buyButtons;
        private Text[] _buyLabels;
        private Button _muteButton;
        private Text _muteLabel;
        private Slider _sfxSlider;
        private Slider _musicSlider;
        private GameObject _tutorialRoot;
        private bool _tutorialDismissed;
        private float _worldFlashUntil;
        private int _flashedWorld = 1;

        public const string FirstHangarHintKey = "agr.ui.firstHangarHint";

        public static GameUi Instance { get; private set; }

        public static GameUi Build(string productTitle)
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameUi ui = canvasObject.AddComponent<GameUi>();
            ui.Construct(productTitle);
            Instance = ui;
            return ui;
        }

        public void Initialize(
            GameManager game,
            HangarShop shop,
            GameSession session,
            PlayerLoadout loadout,
            ShipController ship)
        {
            _game = game;
            _shop = shop;
            _session = session;
            _loadout = loadout;
            _ship = ship;
            _waves = game.GetComponent<WaveManager>();
            Refresh();
        }

        public void Refresh()
        {
            if (_session == null)
            {
                return;
            }

            bool playing = _session.Phase == GamePhase.Playing;
            _menuRoot.SetActive(!playing);
            _hud.gameObject.SetActive(true);
            _hud.text = BuildHud(playing);
            RefreshWorldBadge();
            RefreshFirstHangarHint();

            if (playing)
            {
                return;
            }

            _credits.text = "Credits: " + _session.Credits;
            switch (_session.Phase)
            {
                case GamePhase.WaveClear:
                    _status.text = "Wave clear  ·  Score " + _session.Score + "  ·  Hangar open";
                    _primaryLabel.text = "Next Wave";
                    break;
                case GamePhase.Failed:
                    _status.text = "Ship lost  ·  " + FailReasonText() + "  ·  Score " + _session.Score
                        + "  ·  Retry the wave";
                    _primaryLabel.text = "Retry Wave";
                    break;
                default:
                    _status.text = !_tutorialDismissed && _session.WaveIndex == 1
                        ? "Hangar  ·  Clear a wave to earn credits and upgrades."
                        : "Hangar  ·  Wave " + _session.WaveIndex
                            + "  ·  World " + ContentFactory.WorldIndexForWave(_session.WaveIndex) + " ready";
                    _primaryLabel.text = "Start Wave";
                    break;
            }

            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                RefreshBuyButton(i, ShopCatalog.Items[i]);
            }

            RefreshAudioControls();
        }

        private void Construct(string productTitle)
        {
            Font font = ResolveFont();
            CreateFill("Scrim", transform, new Color(0.02f, 0.03f, 0.05f, 0.18f), new Vector2(0f, 0f), new Vector2(1f, 1f));

            _title = CreateText("Title", transform, font, 54, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_title.rectTransform, new Vector2(0.18f, 0.86f), new Vector2(0.82f, 0.98f));
            _title.text = productTitle;
            _title.color = new Color(1f, 0.78f, 0.32f);

            _world = CreateText("WorldBadge", transform, font, 34, TextAnchor.UpperRight, FontStyle.Bold);
            Stretch(_world.rectTransform, new Vector2(0.62f, 0.86f), new Vector2(0.97f, 0.98f));
            _world.color = new Color(1f, 0.82f, 0.28f);

            _hud = CreateText("Hud", transform, font, 26, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(_hud.rectTransform, new Vector2(0.03f, 0.72f), new Vector2(0.42f, 0.86f));
            _hud.color = Color.white;

            _hint = CreateText("Hint", transform, font, 20, TextAnchor.LowerCenter, FontStyle.Normal);
            Stretch(_hint.rectTransform, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.08f));
            _hint.color = new Color(0.75f, 0.8f, 0.85f);
            _hint.text = "WASD move  ·  Mouse aim  ·  Left mouse / Space fire";

            _menuRoot = CreatePanel("HangarPanel", transform, new Color(0.04f, 0.06f, 0.09f, 0.82f),
                new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.72f));

            _status = CreateText("Status", _menuRoot.transform, font, 28, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_status.rectTransform, new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.98f));
            _status.color = new Color(0.95f, 0.95f, 0.9f);

            _credits = CreateText("Credits", _menuRoot.transform, font, 24, TextAnchor.UpperCenter, FontStyle.Normal);
            Stretch(_credits.rectTransform, new Vector2(0.06f, 0.83f), new Vector2(0.94f, 0.9f));
            _credits.color = new Color(0.7f, 0.9f, 1f);

            _primary = CreateButton("Primary", _menuRoot.transform, font, new Vector2(0.3f, 0.74f), new Vector2(0.7f, 0.82f));
            _primaryLabel = _primary.GetComponentInChildren<Text>();
            _primary.onClick.AddListener(OnPrimary);

            int shopCount = ShopCatalog.Items.Length;
            _buyButtons = new Button[shopCount];
            _buyLabels = new Text[shopCount];
            for (int i = 0; i < shopCount; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                int col = i % 2;
                int row = i / 2;
                float x0 = col == 0 ? 0.05f : 0.52f;
                float x1 = col == 0 ? 0.48f : 0.95f;
                float top = 0.7f - row * 0.21f;
                float bot = top - 0.18f;
                Button button = CreateButton("Buy_" + item.Id, _menuRoot.transform, font,
                    new Vector2(x0, bot), new Vector2(x1, top));
                int captured = i;
                button.onClick.AddListener(() => OnBuy(ShopCatalog.Items[captured].Id));
                _buyButtons[i] = button;
                _buyLabels[i] = button.GetComponentInChildren<Text>();
                _buyLabels[i].fontSize = 16;
            }

            BuildAudioControls(font);
            BuildFirstHangarHint(font);
        }

        private void OnPrimary()
        {
            if (_game == null)
            {
                return;
            }

            DismissFirstHangarHint();
            _game.StartWave();
        }

        private void OnBuy(UpgradeId id)
        {
            if (_shop != null)
            {
                _shop.TryBuy(id);
            }
        }

        private void BuildFirstHangarHint(Font font)
        {
            _tutorialDismissed = PlayerPrefs.GetInt(FirstHangarHintKey, 0) == 1;
            _tutorialRoot = CreatePanel("FirstHangarHint", transform, new Color(0.05f, 0.07f, 0.1f, 0.92f),
                new Vector2(0.015f, 0.16f), new Vector2(0.195f, 0.7f));

            Text title = CreateText("HintTitle", _tutorialRoot.transform, font, 20, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(title.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.97f));
            title.color = new Color(1f, 0.82f, 0.4f);
            title.text = "First flight";

            Text body = CreateText("HintBody", _tutorialRoot.transform, font, 16, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(body.rectTransform, new Vector2(0.07f, 0.22f), new Vector2(0.93f, 0.85f));
            body.color = new Color(0.86f, 0.9f, 0.94f);
            body.text = "WASD move · mouse aim\nLMB / Space shoot\n\n"
                + "Clear a wave to earn credits and upgrades.\n\n"
                + "Shop buys upgrades with those credits.\nStart Wave to fly.";

            Button gotIt = CreateButton("DismissHint", _tutorialRoot.transform, font,
                new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.18f));
            gotIt.GetComponentInChildren<Text>().text = "Got it";
            gotIt.GetComponentInChildren<Text>().fontSize = 16;
            gotIt.onClick.AddListener(DismissFirstHangarHint);
            _tutorialRoot.SetActive(false);
        }

        private void RefreshFirstHangarHint()
        {
            if (_tutorialRoot == null)
            {
                return;
            }

            bool firstHangar = _session != null
                && !_tutorialDismissed
                && _session.Phase == GamePhase.Hangar
                && _session.WaveIndex == 1;
            _tutorialRoot.SetActive(firstHangar);
        }

        private void DismissFirstHangarHint()
        {
            if (_tutorialDismissed)
            {
                return;
            }

            _tutorialDismissed = true;
            PlayerPrefs.SetInt(FirstHangarHintKey, 1);
            PlayerPrefs.Save();
            if (_tutorialRoot != null)
            {
                _tutorialRoot.SetActive(false);
            }
        }

        private void BuildAudioControls(Font font)
        {
            GameObject panel = CreatePanel("AudioPanel", transform, new Color(0.04f, 0.06f, 0.09f, 0.75f),
                new Vector2(0.68f, 0.72f), new Vector2(0.98f, 0.86f));

            _muteButton = CreateButton("Mute", panel.transform, font, new Vector2(0.04f, 0.55f), new Vector2(0.36f, 0.9f));
            _muteLabel = _muteButton.GetComponentInChildren<Text>();
            _muteButton.onClick.AddListener(OnMute);

            CreateText("SfxLabel", panel.transform, font, 16, TextAnchor.MiddleLeft, FontStyle.Normal).text = "SFX";
            Stretch(panel.transform.Find("SfxLabel").GetComponent<RectTransform>(), new Vector2(0.4f, 0.55f), new Vector2(0.55f, 0.9f));
            _sfxSlider = CreateSlider("SfxSlider", panel.transform, new Vector2(0.56f, 0.58f), new Vector2(0.96f, 0.88f),
                AudioCues.Instance != null ? AudioCues.Instance.SfxVolume : AudioCues.DefaultSfxVolume, OnSfxVolume);

            CreateText("MusicLabel", panel.transform, font, 16, TextAnchor.MiddleLeft, FontStyle.Normal).text = "Music";
            Stretch(panel.transform.Find("MusicLabel").GetComponent<RectTransform>(), new Vector2(0.04f, 0.08f), new Vector2(0.28f, 0.48f));
            _musicSlider = CreateSlider("MusicSlider", panel.transform, new Vector2(0.3f, 0.1f), new Vector2(0.96f, 0.46f),
                AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : AudioCues.DefaultMusicVolume, OnMusicVolume);

            RefreshAudioControls();
        }

        private void OnMute()
        {
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.ToggleMute();
                RefreshAudioControls();
            }
        }

        private void OnSfxVolume(float value)
        {
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.SetSfxVolume(value);
            }
        }

        private void OnMusicVolume(float value)
        {
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.SetMusicVolume(value);
            }
        }

        private void RefreshAudioControls()
        {
            if (_muteLabel == null || AudioCues.Instance == null)
            {
                return;
            }

            _muteLabel.text = AudioCues.Instance.Muted ? "Unmute" : "Mute";
            if (_sfxSlider != null)
            {
                _sfxSlider.SetValueWithoutNotify(AudioCues.Instance.SfxVolume);
            }

            if (_musicSlider != null)
            {
                _musicSlider.SetValueWithoutNotify(AudioCues.Instance.MusicVolume);
            }
        }

        public void AnnounceWorldChange(int world)
        {
            _flashedWorld = world;
            _worldFlashUntil = Time.unscaledTime + 1.6f;
            RefreshWorldBadge();
        }

        private void Update()
        {
            if (_world == null)
            {
                return;
            }

            if (Time.unscaledTime < _worldFlashUntil)
            {
                float pulse = Mathf.PingPong(Time.unscaledTime * 7f, 1f);
                _world.fontSize = 38 + (int)(10f * pulse);
                _world.color = Color.Lerp(new Color(1f, 0.95f, 0.5f), new Color(1f, 0.45f, 0.08f), pulse);
                _world.text = "WORLD " + _flashedWorld + "  ONLINE";
                return;
            }

            if (_world.fontSize != 34)
            {
                _world.fontSize = 34;
                RefreshWorldBadge();
            }
        }

        private void RefreshWorldBadge()
        {
            if (_world == null || _session == null || Time.unscaledTime < _worldFlashUntil)
            {
                return;
            }

            int world = ContentFactory.WorldIndexForWave(_session.WaveIndex);
            _world.text = "WORLD " + world;
            _world.color = new Color(1f, 0.82f, 0.28f);
        }

        private void RefreshBuyButton(int index, ShopItem item)
        {
            bool owned = _loadout.State.Owns(item.Id);
            bool canApply = _loadout.State.CanApply(item.Id);
            bool tooPoor = !owned && _session.Credits < item.Cost;
            bool canBuy = _session.ShopOpen && canApply && !tooPoor;
            _buyButtons[index].interactable = canBuy;

            Image plate = _buyButtons[index].targetGraphic as Image;
            if (plate != null)
            {
                plate.color = canBuy
                    ? new Color(0.18f, 0.22f, 0.28f, 0.95f)
                    : new Color(0.1f, 0.11f, 0.12f, 0.78f);
            }

            string suffix;
            if (owned)
            {
                suffix = "  [owned]";
            }
            else if (!canApply)
            {
                suffix = "  [locked]";
            }
            else if (tooPoor)
            {
                suffix = "  —  need " + item.Cost + " cr";
            }
            else
            {
                suffix = "  —  " + item.Cost + " cr";
            }

            _buyLabels[index].text = item.Title + suffix + "\n" + item.Description;
            _buyLabels[index].color = canBuy
                ? Color.white
                : new Color(0.55f, 0.56f, 0.58f, 0.95f);
        }

        private string FailReasonText()
        {
            if (_session == null || string.IsNullOrEmpty(_session.FailReason))
            {
                return "Unknown cause";
            }

            return _session.FailReason;
        }

        private string BuildHud(bool playing)
        {
            int hull = _ship != null && _ship.Health != null ? _ship.Health.Hull : LoadoutState.HullHitPoints;
            int shield = _ship != null && _ship.Health != null ? _ship.Health.Shield : _loadout.State.ShieldCharges;
            string remaining = playing && _waves != null
                ? "   ·   Remaining " + _waves.RemainingThreats
                : string.Empty;
            return "Wave " + _session.WaveIndex
                + "   ·   Score " + _session.Score
                + "\nHull " + hull + "   ·   Shield " + shield
                + remaining;
        }

        private static Font ResolveFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }

            return font;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = CreateFill(name, parent, color, min, max);
            return go;
        }

        private static GameObject CreateFill(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Stretch(go.GetComponent<RectTransform>(), min, max);
            return go;
        }

        private static Text CreateText(string name, Transform parent, Font font, int size, TextAnchor anchor, FontStyle style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.fontStyle = style;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Font font, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);
            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.35f, 0.32f, 0.18f, 1f);
            colors.pressedColor = new Color(0.55f, 0.4f, 0.12f, 1f);
            colors.disabledColor = new Color(0.12f, 0.13f, 0.15f, 0.7f);
            button.colors = colors;
            Stretch(go.GetComponent<RectTransform>(), min, max);

            Text label = CreateText("Label", go.transform, font, 20, TextAnchor.MiddleCenter, FontStyle.Normal);
            Stretch(label.rectTransform, Vector2.zero, Vector2.one);
            label.raycastTarget = false;
            return button;
        }

        private static Slider CreateSlider(
            string name,
            Transform parent,
            Vector2 min,
            Vector2 max,
            float value,
            UnityEngine.Events.UnityAction<float> onChanged)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            Image background = root.AddComponent<Image>();
            background.color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
            Stretch(root.GetComponent<RectTransform>(), min, max);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(root.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            Stretch(fillAreaRect, Vector2.zero, Vector2.one);
            fillAreaRect.offsetMin = new Vector2(8f, 6f);
            fillAreaRect.offsetMax = new Vector2(-8f, -6f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.9f, 0.65f, 0.2f, 1f);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            Stretch(fillRect, Vector2.zero, Vector2.one);

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(root.transform, false);
            Stretch(handleArea.AddComponent<RectTransform>(), Vector2.zero, Vector2.one);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16f, 22f);

            Slider slider = root.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
