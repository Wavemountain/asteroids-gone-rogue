using UnityEngine;
using UnityEngine.EventSystems;
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
        private Button _abortButton;
        private Text _abortLabel;
        private Button _muteButton;
        private Text _muteLabel;
        private Slider _sfxSlider;
        private Slider _musicSlider;
        private GameObject _tutorialRoot;
        private bool _tutorialDismissed;
        private float _worldFlashUntil;
        private int _flashedWorld = 1;
        private string _statusBase = string.Empty;
        private ShopItem _hoveredItem;

        public const string FirstHangarHintKey = "agr.ui.firstHangarHint";
        public const string HangarControlsHint =
            "Abort (Esc)  ·  Q / RMB fire modes (discover Spread / Pierce when owned)";
        public const string HangarHintBody =
            "WASD move · mouse aim\nLMB / Space shoot\nAbort (Esc) leaves the wave\n"
            + "Q / RMB fire modes\n(discover Spread / Pierce when owned)\n\n"
            + "Clear a wave to earn credits and upgrades.\n\n"
            + "Shop buys upgrades with those credits.\nStart Wave to fly.";

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
            if (_abortButton != null)
            {
                _abortButton.gameObject.SetActive(playing);
            }

            _hud.gameObject.SetActive(true);
            _hud.text = BuildHud(playing);
            _hint.text = playing
                ? "WASD move  ·  Mouse aim  ·  LMB / Space fire  ·  Q / RMB fire mode  ·  Esc abort"
                : "WASD move  ·  Mouse aim  ·  LMB / Space fire  ·  " + HangarControlsHint;
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
                    _statusBase = "Wave clear  ·  Score " + _session.Score + "  ·  Hangar open"
                        + "\n" + BestCardLine();
                    _primaryLabel.text = "Next Wave";
                    break;
                case GamePhase.Failed:
                    _statusBase = "Ship lost  ·  " + FailReasonText() + "  ·  Score " + _session.Score
                        + "  ·  Retry the wave"
                        + "\n" + BestCardLine();
                    _primaryLabel.text = "Retry Wave";
                    break;
                default:
                    _statusBase = HangarReadyStatus();
                    _primaryLabel.text = "Start Wave";
                    break;
            }

            ApplyStatusText();

            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                RefreshBuyButton(i, ShopCatalog.Items[i]);
            }

            RefreshAudioControls();
        }

        private string HangarReadyStatus()
        {
            string waveLine = !_tutorialDismissed && _session.WaveIndex == 1
                ? "Hangar  ·  Clear a wave to earn credits and upgrades."
                : "Hangar  ·  Wave " + _session.WaveIndex
                    + "  ·  World " + ContentFactory.WorldIndexForWave(_session.WaveIndex) + " ready";
            return waveLine + "\n" + HangarControlsHint;
        }

        private void ApplyStatusText()
        {
            if (_status == null)
            {
                return;
            }

            if (_hoveredItem != null)
            {
                _status.text = _hoveredItem.Title + "  —  " + _hoveredItem.Description;
                return;
            }

            _status.text = _statusBase;
        }

        private void OnShopHover(ShopItem item)
        {
            _hoveredItem = item;
            ApplyStatusText();
        }

        private void OnShopHoverExit(ShopItem item)
        {
            if (_hoveredItem == item)
            {
                _hoveredItem = null;
                ApplyStatusText();
            }
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

            _menuRoot = CreatePanel("HangarPanel", transform, new Color(0.03f, 0.045f, 0.07f, 0.92f),
                new Vector2(0.185f, 0.035f), new Vector2(0.815f, 0.725f));
            CreateFill("HangarHeader", _menuRoot.transform, new Color(1f, 0.58f, 0.16f, 0.2f),
                new Vector2(0f, 0.962f), new Vector2(1f, 1f));
            CreateFill("HangarRule", _menuRoot.transform, new Color(1f, 0.72f, 0.28f, 0.7f),
                new Vector2(0.04f, 0.955f), new Vector2(0.96f, 0.962f));

            _status = CreateText("Status", _menuRoot.transform, font, 20, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_status.rectTransform, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.95f));
            _status.color = new Color(0.96f, 0.93f, 0.84f);

            _credits = CreateText("Credits", _menuRoot.transform, font, 22, TextAnchor.UpperCenter, FontStyle.Normal);
            Stretch(_credits.rectTransform, new Vector2(0.06f, 0.785f), new Vector2(0.94f, 0.84f));
            _credits.color = new Color(0.55f, 0.88f, 1f);

            _primary = CreateButton("Primary", _menuRoot.transform, font, new Vector2(0.28f, 0.695f), new Vector2(0.72f, 0.78f));
            _primaryLabel = _primary.GetComponentInChildren<Text>();
            _primary.onClick.AddListener(OnPrimary);
            Image primaryPlate = _primary.targetGraphic as Image;
            if (primaryPlate != null)
            {
                primaryPlate.color = new Color(0.36f, 0.24f, 0.08f, 0.98f);
            }

            _abortButton = CreateButton("AbortWave", transform, font, new Vector2(0.78f, 0.09f), new Vector2(0.97f, 0.155f));
            _abortLabel = _abortButton.GetComponentInChildren<Text>();
            _abortLabel.text = "Abort → Hangar";
            _abortLabel.fontSize = 18;
            _abortButton.onClick.AddListener(OnAbort);
            _abortButton.gameObject.SetActive(false);

            BuildShop(font);
            BuildAudioControls(font);
            BuildFirstHangarHint(font);
        }

        private void BuildShop(Font font)
        {
            BuildGroupHeader(font, ShopCatalog.HullHeader, new Vector2(0.03f, 0.63f), new Vector2(0.56f, 0.685f));
            BuildGroupHeader(font, ShopCatalog.WeaponsHeader, new Vector2(0.575f, 0.63f), new Vector2(0.775f, 0.685f));
            BuildGroupHeader(font, ShopCatalog.DefenseHeader, new Vector2(0.79f, 0.63f), new Vector2(0.97f, 0.685f));

            int shopCount = ShopCatalog.Items.Length;
            _buyButtons = new Button[shopCount];
            _buyLabels = new Text[shopCount];
            int hullIndex = 0;
            int weaponIndex = 0;
            for (int i = 0; i < shopCount; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                Vector2 min;
                Vector2 max;
                ShopButtonRect(item.Group, ref hullIndex, ref weaponIndex, out min, out max);
                Button button = CreateButton("Buy_" + item.Id, _menuRoot.transform, font, min, max);
                int captured = i;
                button.onClick.AddListener(() => OnBuy(ShopCatalog.Items[captured].Id));
                BindShopHover(button, item);
                _buyButtons[i] = button;
                _buyLabels[i] = button.GetComponentInChildren<Text>();
                _buyLabels[i].fontSize = 15;
                _buyLabels[i].fontStyle = FontStyle.Bold;
            }
        }

        private static void ShopButtonRect(
            ShopGroup group,
            ref int hullIndex,
            ref int weaponIndex,
            out Vector2 min,
            out Vector2 max)
        {
            const float ButtonHeight = 0.10f;
            const float RowStep = 0.115f;
            if (group == ShopGroup.Weapons)
            {
                float top = 0.615f - weaponIndex * RowStep;
                min = new Vector2(0.575f, top - ButtonHeight);
                max = new Vector2(0.775f, top);
                weaponIndex++;
                return;
            }

            if (group == ShopGroup.Defense)
            {
                min = new Vector2(0.79f, 0.515f);
                max = new Vector2(0.97f, 0.615f);
                return;
            }

            int col = hullIndex % 3;
            int row = hullIndex / 3;
            float x0 = 0.03f + col * 0.175f;
            min = new Vector2(x0, 0.615f - row * RowStep - ButtonHeight);
            max = new Vector2(x0 + 0.165f, 0.615f - row * RowStep);
            hullIndex++;
        }

        private void BuildGroupHeader(Font font, string label, Vector2 min, Vector2 max)
        {
            Text header = CreateText("Group_" + label, _menuRoot.transform, font, 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(header.rectTransform, min, max);
            header.color = new Color(1f, 0.78f, 0.38f);
            header.text = label;
            CreateFill("Rule_" + label, _menuRoot.transform, new Color(1f, 0.62f, 0.2f, 0.45f),
                new Vector2(min.x, min.y), new Vector2(max.x, min.y + 0.008f));
        }

        private void BindShopHover(Button button, ShopItem item)
        {
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry enter = new EventTrigger.Entry();
            enter.eventID = EventTriggerType.PointerEnter;
            enter.callback.AddListener(_ => OnShopHover(item));
            trigger.triggers.Add(enter);
            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener(_ => OnShopHoverExit(item));
            trigger.triggers.Add(exit);
        }

        private void OnPrimary()
        {
            if (_game == null)
            {
                return;
            }

            DismissFirstHangarHint();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }

            _game.StartWave();
        }

        private void OnBuy(UpgradeId id)
        {
            if (_shop != null)
            {
                _shop.TryBuy(id);
            }
        }

        private void OnAbort()
        {
            if (_game != null)
            {
                _game.AbortWave();
            }
        }

        private void BuildFirstHangarHint(Font font)
        {
            _tutorialDismissed = PlayerPrefs.GetInt(FirstHangarHintKey, 0) == 1;
            _tutorialRoot = CreatePanel("FirstHangarHint", transform, new Color(0.04f, 0.055f, 0.08f, 0.94f),
                new Vector2(0.01f, 0.12f), new Vector2(0.178f, 0.72f));
            CreateFill("HintHeader", _tutorialRoot.transform, new Color(1f, 0.58f, 0.16f, 0.22f),
                new Vector2(0f, 0.94f), new Vector2(1f, 1f));
            CreateFill("HintRule", _tutorialRoot.transform, new Color(1f, 0.72f, 0.28f, 0.7f),
                new Vector2(0.08f, 0.932f), new Vector2(0.92f, 0.94f));

            Text title = CreateText("HintTitle", _tutorialRoot.transform, font, 18, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(title.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.97f));
            title.color = new Color(1f, 0.82f, 0.4f);
            title.text = "First flight";

            Text body = CreateText("HintBody", _tutorialRoot.transform, font, 14, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(body.rectTransform, new Vector2(0.07f, 0.2f), new Vector2(0.93f, 0.85f));
            body.color = new Color(0.9f, 0.9f, 0.86f);
            body.text = HangarHintBody;

            Button gotIt = CreateButton("DismissHint", _tutorialRoot.transform, font,
                new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.18f));
            gotIt.GetComponentInChildren<Text>().text = "Got it";
            gotIt.GetComponentInChildren<Text>().fontSize = 16;
            gotIt.onClick.AddListener(OnDismissHintClicked);
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

        private void OnDismissHintClicked()
        {
            DismissFirstHangarHint();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
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
            if (_session != null && _session.Phase == GamePhase.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                OnAbort();
            }

            if (_session != null && _session.Phase == GamePhase.Playing && _hud != null)
            {
                _hud.text = BuildHud(true);
            }

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
            bool locked = !owned && !canApply;
            bool tooPoor = !owned && canApply && _session.Credits < item.Cost;
            bool canBuy = _session.ShopOpen && canApply && !tooPoor;
            _buyButtons[index].interactable = canBuy;

            Image plate = _buyButtons[index].targetGraphic as Image;
            if (plate != null)
            {
                if (owned)
                {
                    plate.color = new Color(0.1f, 0.2f, 0.18f, 0.92f);
                }
                else if (locked)
                {
                    plate.color = new Color(0.07f, 0.07f, 0.08f, 0.88f);
                }
                else if (tooPoor)
                {
                    plate.color = new Color(0.12f, 0.1f, 0.09f, 0.9f);
                }
                else
                {
                    plate.color = new Color(0.28f, 0.2f, 0.08f, 0.98f);
                }
            }

            string costLine;
            Color labelColor;
            if (owned)
            {
                costLine = "OWNED";
                labelColor = new Color(0.55f, 0.82f, 0.72f, 0.95f);
            }
            else if (locked)
            {
                costLine = "LOCKED";
                labelColor = new Color(0.4f, 0.4f, 0.42f, 0.85f);
            }
            else if (tooPoor)
            {
                costLine = "need " + item.Cost + " cr";
                labelColor = new Color(0.62f, 0.54f, 0.46f, 0.92f);
            }
            else
            {
                costLine = item.Cost + " cr";
                labelColor = new Color(1f, 0.93f, 0.78f);
            }

            _buyLabels[index].text = item.Title + "\n" + costLine;
            _buyLabels[index].color = labelColor;
        }

        private string BestCardLine()
        {
            LocalBest best = _game != null && _game.Best != null ? _game.Best : LocalBest.Load();
            string line = best.CardLine();
            if (_game != null && _game.LastRunWasNewBest)
            {
                return line + "  ·  NEW BEST";
            }

            return line;
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
            string fireMode = string.Empty;
            if (playing && _ship != null && _ship.Shooter != null
                && _loadout != null && _loadout.State != null
                && (_loadout.State.SpreadBolt || _loadout.State.Pierce))
            {
                fireMode = "\nFire " + _ship.Shooter.Mode;
            }

            return "Wave " + _session.WaveIndex
                + "   ·   Score " + _session.Score
                + "\nHull " + hull + "   ·   Shield " + shield
                + remaining
                + fireMode;
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
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.92f, 0.62f, 1f);
            colors.pressedColor = new Color(0.85f, 0.65f, 0.28f, 1f);
            colors.disabledColor = new Color(0.82f, 0.82f, 0.82f, 1f);
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
