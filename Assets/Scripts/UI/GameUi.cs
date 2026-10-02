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
        private GameObject _tutorialRoot;
        private GameObject _summaryRoot;
        private Image _summaryHeader;
        private Image _summaryRule;
        private Text _summaryTitle;
        private Text _summaryBody;
        private Text _waveMedal;
        private Text _continueHint;
        private Text _badgeRow;
        private Image _hitFlash;
        private GameObject _scrim;
        private GameObject _vignette;
        private GameObject _hudPlate;
        private GameObject _utilityHud;
        private Image _utilityRing;
        private Text _utilityHudLabel;
        private Text _utilityHudName;
        private Text _loadoutSlots;
        private GameObject _healthRoot;
        private Text _healthTitle;
        private Text _hullBarLabel;
        private Text _shieldBarLabel;
        private Text _hullBarCount;
        private Text _shieldBarCount;
        private Image _hullFill;
        private Image _shieldFill;
        private GameObject _shieldBarRow;
        private static Sprite _barFillSprite;
        private bool _tutorialDismissed;
        private bool _doctrineIntroDismissed;
        private float _worldFlashUntil;
        private int _flashedWorld = 1;
        private string _flashedLayout = string.Empty;
        private string _flashedBadge = string.Empty;
        private string _medalBeat = string.Empty;
        private string _statusBase = string.Empty;
        private ShopItem _hoveredItem;
        private float _hitFlashUntil;
        private float _hitFlashStrength;
        private GameObject _endCreditsRoot;
        private Text _endCreditsBody;
        private Button _creditsButton;
        private Text _creditsButtonLabel;
        private Text _creditsContinueLabel;
        private Text _firstFlightTitle;
        private Text _firstFlightBody;
        private Text _gotItLabel;
        private Button _gotItButton;
        private Button _creditsContinue;
        private GameObject _lastPadSelected;
        private bool _abortUrgent;
        private float _firstRunCoachUntil;
        private Image _primaryPlate;
        private Image _abortPlate;
        private Text _hullHeader;
        private Text _weaponsHeader;
        private Text _defenseHeader;
        private GameObject _langPanel;
        private Text _langTitle;
        private Image _enBezel;
        private Image _svBezel;
        private GameObject _diffPanel;
        private Text _diffTitle;
        private Image _easyBezel;
        private Image _normalBezel;
        private Image _hardBezel;
        private Text _easyLabel;
        private Text _normalLabel;
        private Text _hardLabel;
        private Text _livesHud;
        private GameObject _previewCanvas;
        private GameObject _previewRoot;
        private Text _previewCaption;
        private RawImage _previewViewport;
        private bool _creditsVisible;
        private float _creditsScroll;
        private static Texture2D _previewPlaceholder;
        private Text _achievementToast;
        private Text _achievementLadder;
        private float _achievementUntil;
        private int _padSlot;
        private bool _padHeld;
        private float _padRepeatAt;
        private bool[] _padSelectable;
        private GameObject _doctrineRoot;
        private Text _doctrineTitle;
        private Text _doctrineHint;
        private Button _barrageButton;
        private Button _lanceButton;
        private Button _hunterButton;
        private Text _barrageLabel;
        private Text _lanceLabel;
        private Text _hunterLabel;
        private GameObject _doctrineShopRow;
        private GameObject _doctrineIntro;
        private Text _doctrineIntroBody;
        private Text _doctrineIntroGotIt;
        private Button _doctrineIntroButton;
        private Text _doctrineBadge;
        private SettingsState _settings;
        private bool _settingsOpen;
        private int _settingsIndex;
        private bool _settingsNavHeld;
        private float _settingsNavRepeatAt;
        private GameObject _settingsRoot;
        private GameObject _settingsPanel;
        private Button _settingsGear;
        private Text _settingsGearLabel;
        private Text _settingsTitle;
        private Text _settingsLanguageLabel;
        private Text _settingsLanguageValue;
        private Image _settingsEnBezel;
        private Image _settingsSvBezel;
        private Text _settingsControlsTitle;
        private Text _settingsControlsBody;
        private Text _settingsMusicLabel;
        private Text _settingsSfxLabel;
        private Text _settingsMuteLabel;
        private Text _settingsMuteValue;
        private Text _settingsShakeLabel;
        private Text _settingsShakeValue;
        private Slider _settingsMusicSlider;
        private Slider _settingsSfxSlider;
        private Button[] _settingsRowButtons;

        private static readonly Color UiAmber = UiTheme.Primary;
        private static readonly Color UiBody = UiTheme.Accent;
        private static readonly Color UiHull = UiTheme.Primary;
        private static readonly Color UiShield = UiTheme.Secondary;

        public const float LanguageFlagScale = 0.48f;
        // Hangar left column: bottom matches LOADOUT frame (ShipPreviewMin.y 0.080);
        // top stays under the full-width top bar (0.905) so WAVE CLEAR / shop never sit on chrome.
        public static readonly Vector2 HangarPanelMin = new Vector2(0.014f, 0.080f);
        public static readonly Vector2 HangarPanelMax = new Vector2(0.55f, 0.888f);
        public static readonly Vector2 TopBarMin = new Vector2(0.012f, 0.905f);
        public static readonly Vector2 TopBarMax = new Vector2(0.988f, 0.995f);
        // Gear stays on the top-right. Mute / SFX / Music live in the settings panel.
        public static readonly Vector2 SettingsGearMin = new Vector2(0.900f, 0.905f);
        public static readonly Vector2 SettingsGearMax = new Vector2(0.988f, 0.995f);
        public static readonly Vector2 SettingsPanelMin = new Vector2(0.30f, 0.12f);
        public static readonly Vector2 SettingsPanelMax = new Vector2(0.70f, 0.88f);
        public static readonly Vector2 ShipPreviewMin = new Vector2(0.562f, 0.080f);
        public static readonly Vector2 ShipPreviewMax = new Vector2(0.986f, 0.596f);
        // Doctrine card sits above the loadout frame and under the top bar (0.905).
        // 1280x800: panel 543x232. Cards y 0.414–0.862 are ~104px, five 14px lines.
        public static readonly Vector2 DoctrinePanelMin = new Vector2(0.562f, 0.608f);
        public static readonly Vector2 DoctrinePanelMax = new Vector2(0.986f, 0.898f);
        // Shared shop cell + gutter (hangar-local). All 3 columns start at ShopGridTop.
        public const float ShopGridTop = 0.665f;
        public const float ShopCellHeight = 0.094f;
        public const float ShopCellGutter = 0.016f;
        public const int ShipPreviewSortOrder = 80;
        public const string ShipPreviewCanvasName = "ShipPreviewCanvas";
        public const string FirstHangarHintKey = "agr.ui.firstHangarHint";
        public const string DoctrineHintKey = "agr.ui.doctrineHint";
        public const string DoctrineHintTitle = "Doctrines";
        public const string DoctrineHintBody =
            "Doctrines are open. Pick Barrage, Lance, or Hunter.";
        public const string HangarControlsHint =
            "LT utility · LB cycle · A confirm · B / Esc Next Wave · Start launch wave";
        public const string MedalLadderPrefix = "MEDALS";
        public const string HangarHintBody =
            "LS / WASD fly  ·  RT / LMB shoot  ·  LT / E utility\n"
            + "Start = launch wave  ·  B / Esc = focus Next Wave\n"
            + "Clear a wave to earn credits and upgrades.\n"
            + "Medal ladder (top-left): ★ Scout Wing at wave 3.";
        public const string FirstWaveCoach = "Shoot rocks  ·  Esc / Start returns to hangar";
        public const string HintDual = "LT utility · LB cycle primary · RT fire";
        public const string HintRail = "Hold RT 0.55s, release — Rail. Miss or cancel pays half CD.";

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
            EnsureHangarPreview(ship);
            Refresh();
        }

        public void EnsureHangarPreview(ShipController ship)
        {
            EnsureShipPreviewFrame();
            HangarShipPreview preview = ship != null ? ship.GetComponent<HangarShipPreview>() : null;
            if (preview != null)
            {
                preview.BindViewport(_previewViewport);
                bool hangar = _session == null || _session.Phase != GamePhase.Playing;
                preview.SetActive(hangar);
            }

            ForcePreviewChrome();
        }

        public void Refresh()
        {
            if (_session == null)
            {
                return;
            }

            bool playing = _session.Phase == GamePhase.Playing;
            if (playing && _creditsVisible)
            {
                HideEndCredits(false);
            }

            _menuRoot.SetActive(!playing);
            if (_abortButton != null)
            {
                _abortButton.gameObject.SetActive(playing);
            }

            if (_creditsButton != null)
            {
                _creditsButton.gameObject.SetActive(!playing && !_creditsVisible);
            }

            _hud.gameObject.SetActive(playing);
            if (_hudPlate != null)
            {
                _hudPlate.SetActive(playing);
            }

            if (playing)
            {
                _hud.text = BuildHud(true);
            }

            RefreshHealthBar();
            RefreshUtilityHud(playing);
            if (_scrim != null)
            {
                _scrim.SetActive(!playing);
            }

            if (_vignette != null)
            {
                _vignette.SetActive(playing);
            }
            if (playing && _settingsOpen)
            {
                DismissSettingsForPlay();
            }

            if (_settingsGear != null)
            {
                _settingsGear.gameObject.SetActive(!playing);
            }

            if (_settingsRoot != null)
            {
                bool showSettings = _settingsOpen && !playing;
                _settingsRoot.SetActive(showSettings);
                if (showSettings)
                {
                    _settingsRoot.transform.SetAsLastSibling();
                    RefreshSettingsFocus();
                }
            }

            if (_langPanel != null)
            {
                _langPanel.SetActive(!playing);
            }

            if (_diffPanel != null)
            {
                _diffPanel.SetActive(!playing);
            }

            EnsureShipPreviewFrame();
            ForcePreviewChrome();

            ApplyLocalizedStaticLabels();
            RefreshLanguageChrome();
            RefreshDifficultyChrome();

            bool railHint = playing
                && _loadout != null
                && _loadout.State != null
                && _loadout.State.ResolvedPrimary() == FireMode.Rail;
            ApplyFooterHintSize();
            _hint.text = railHint
                ? Loc.T("ui.hint_rail", HintRail)
                : (playing
                    ? Loc.T("ui.hint_play", "WASD / LS move · Mouse / RS aim · LMB / RT fire · E / LT utility · Q / LB cycle · Esc / Start = back to hangar")
                    : Loc.Tf(
                        "ui.hint_hangar",
                        "LS move · {0}",
                        Loc.T("ui.hangar_controls", HangarControlsHint)));
            ClampOneLine(_hint);
            RefreshWorldBadge();
            RefreshBadgeRow(playing);
            RefreshDoctrineBadge(playing);
            RefreshAchievementLadder(playing);
            if (_doctrineRoot != null)
            {
                bool showDoctrine = !playing
                    && _session != null
                    && DoctrineRules.HangarUnlocked(_session.WaveIndex);
                _doctrineRoot.SetActive(showDoctrine);
            }
            RefreshFirstHangarHint();

            if (playing)
            {
                return;
            }

            if (_credits != null)
            {
                // Currency lives in the WAVE-CLEAR strip (r2). Hide the orphan hangar label.
                _credits.gameObject.SetActive(false);
            }

            switch (_session.Phase)
            {
                case GamePhase.WaveClear:
                    _statusBase = string.Empty;
                    _primaryLabel.text = Loc.T("ui.next_wave", "Next Wave");
                    break;
                case GamePhase.CampaignClear:
                    _statusBase = string.Empty;
                    _primaryLabel.text = Loc.T("ui.new_run", "New Run");
                    break;
                case GamePhase.Failed:
                    _statusBase = string.Empty;
                    _primaryLabel.text = Loc.T("ui.retry_hangar", "RETRY  ·  NEW RUN");
                    break;
                default:
                    _statusBase = string.Empty;
                    _primaryLabel.text = Loc.T("ui.start_wave", "Start Wave");
                    break;
            }

            RefreshRunSummary(playing);
            ApplyStatusText();
            RefreshDoctrinePicks();
            RefreshDoctrineIntro();

            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                RefreshBuyButton(i, ShopCatalog.Items[i]);
            }

            RefreshSettingsAudio();
            EnsurePrimaryClickable();
        }

        private string HangarReadyStatus()
        {
            string waveLine = !_tutorialDismissed && _session.WaveIndex == 1
                ? Loc.T("ui.hangar_clear_wave", "Hangar  ·  Clear a wave to earn credits and upgrades.")
                : Loc.Tf(
                    "ui.hangar_wave_line",
                    "Hangar  ·  Wave {0}  ·  World {1} layout: {2}",
                    _session.WaveIndex,
                    ContentFactory.WorldIndexForWave(_session.WaveIndex),
                    ArenaLayout.Title(ArenaLayout.ForWave(_session.WaveIndex)));
            string tease = RunSummary.MonsterTeaser(_session.WaveIndex);
            string hook = RunSummary.NextMedalHook(_session.WaveIndex);
            string extra = string.Empty;
            if (!string.IsNullOrEmpty(tease))
            {
                extra += "  ·  " + tease;
            }

            if (!string.IsNullOrEmpty(hook))
            {
                extra += "  ·  " + hook;
            }

            // Controls hint lives only in the screen-bottom row, never in WAVE CLEAR / shop.
            return waveLine + extra;
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
                ClampOneLine(_status);
                return;
            }

            _status.text = _statusBase;
            ClampOneLine(_status);
        }

        private void OnShopHover(ShopItem item)
        {
            _hoveredItem = item;
            ApplyStatusText();
            if (_game != null && item != null)
            {
                _game.PreviewUpgrade(item.Id);
            }
        }

        private void OnShopHoverExit(ShopItem item)
        {
            if (_hoveredItem == item)
            {
                _hoveredItem = null;
                ApplyStatusText();
                if (_game != null)
                {
                    _game.ClearUpgradePreview();
                }
            }
        }

        private void Construct(string productTitle)
        {
            Loc.EnsureLoaded();
            _settings = SettingsState.Load();
            Font display = UiFonts.Display();
            Font body = UiFonts.Body();
            _scrim = CreateFill("Scrim", transform, UiTheme.WithAlpha(UiTheme.Void, 0.22f), new Vector2(0f, 0f), new Vector2(1f, 1f));
            _vignette = BuildPlayVignette();
            _hitFlash = CreateFill("ScreenFlash", transform, new Color(1f, 0.96f, 0.92f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 1f)).GetComponent<Image>();

            _hudPlate = UiTheme.BuildPanel(
                "HudPlate",
                transform,
                new Vector2(0.012f, 0.555f),
                new Vector2(0.395f, 0.875f),
                0.982f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.HudPlate);
            _hudPlate.SetActive(false);

            _title = CreateText("Title", transform, display, UiTheme.HeaderMin, TextAnchor.MiddleLeft, FontStyle.Bold);
            // Top-bar left: title slot. Stretch anchors, zero offset so 16:9 scaler keeps one row.
            Stretch(_title.rectTransform, new Vector2(0.012f, 0.905f), new Vector2(0.205f, 0.995f));
            _title.text = productTitle;
            _title.color = UiAmber;
            ClampOneLine(_title);
            AddReadability(_title, true);

            _world = CreateText("WorldBadge", transform, display, 22, TextAnchor.MiddleLeft, FontStyle.Bold);
            // Top-bar left: WORLD badge beside title. Stretch anchors, zero offset.
            Stretch(_world.rectTransform, new Vector2(0.205f, 0.905f), new Vector2(0.355f, 0.995f));
            _world.color = UiAmber;
            ClampOneLine(_world);
            AddReadability(_world, true);

            _hud = CreateText("Hud", transform, body, 22, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(_hud.rectTransform, new Vector2(0.03f, 0.62f), new Vector2(0.5f, 0.775f));
            _hud.color = UiBody;
            AddReadability(_hud, false);
            _hud.gameObject.SetActive(false);
            BuildHealthRack(display, body);
            BuildUtilityHud(display, body);

            _badgeRow = CreateText("BadgeRow", transform, display, 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            // Top-bar left: MEDALS. Same row as title/WORLD; stretch anchors, zero offset.
            Stretch(_badgeRow.rectTransform, new Vector2(0.355f, 0.950f), new Vector2(0.478f, 0.995f));
            _badgeRow.color = UiAmber;
            ClampOneLine(_badgeRow);
            AddReadability(_badgeRow, false);

            _achievementLadder = CreateText("AchievementLadder", transform, body, SettingsMeasure.LadderFont, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(
                _achievementLadder.rectTransform,
                new Vector2(SettingsMeasure.LadderMinX, 0.905f),
                new Vector2(SettingsMeasure.LadderMaxX, 0.950f));
            _achievementLadder.color = UiTheme.WithAlpha(UiTheme.Primary, 0.95f);
            _achievementLadder.horizontalOverflow = HorizontalWrapMode.Wrap;
            _achievementLadder.verticalOverflow = VerticalWrapMode.Overflow;
            AddReadability(_achievementLadder, false);

            _hint = CreateText("Hint", transform, body, UiTheme.HintSize(Screen.width), TextAnchor.MiddleCenter, FontStyle.Normal);
            // Screen-bottom, outside hangar panel (min.y 0.080). Never in WAVE CLEAR / shop.
            Stretch(_hint.rectTransform, new Vector2(0.14f, 0.008f), new Vector2(0.86f, 0.072f));
            _hint.color = UiTheme.FooterHint;
            _hint.text = "WASD / LS move  ·  Mouse / RS aim  ·  LMB / Space / RT fire";
            ClampOneLine(_hint);
            AddReadability(_hint, false);

            _menuRoot = UiTheme.BuildPanel(
                "HangarPanel",
                transform,
                HangarPanelMin,
                HangarPanelMax,
                0.962f);
            CreateFill("HangarInner", _menuRoot.transform, UiTheme.InnerWash,
                new Vector2(0.012f, 0.018f), new Vector2(0.988f, 0.948f));

            BuildRunSummary(display, body);

            _achievementToast = CreateText("AchievementToast", transform, display, 18, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_achievementToast.rectTransform, new Vector2(0.18f, 0.52f), new Vector2(0.82f, 0.60f));
            _achievementToast.color = UiAmber;
            _achievementToast.gameObject.SetActive(false);
            AddReadability(_achievementToast, true);

            _status = CreateText("Status", _menuRoot.transform, body, UiTheme.BodyMin, TextAnchor.MiddleLeft, FontStyle.Bold);
            // Hover/focus description sits under the shop grid, not in WAVE CLEAR or shop cells.
            Stretch(_status.rectTransform, new Vector2(0.03f, 0.016f), new Vector2(0.97f, 0.088f));
            _status.color = UiTheme.Accent;
            ClampOneLine(_status);

            _credits = CreateText("Credits", _menuRoot.transform, body, 20, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_credits.rectTransform, new Vector2(0.06f, 0.785f), new Vector2(0.94f, 0.84f));
            _credits.color = UiTheme.Secondary;
            _credits.gameObject.SetActive(false);

            // Full-width left-column CTA: under WAVE-CLEAR strip, above shop headers.
            _primary = CreateButton(
                "Primary",
                _menuRoot.transform,
                display,
                new Vector2(0.03f, 0.735f),
                new Vector2(0.97f, 0.800f));
            _primaryLabel = _primary.GetComponentInChildren<Text>();
            _primary.onClick.AddListener(OnPrimary);
            _primaryPlate = _primary.targetGraphic as Image;
            UiTheme.ApplyButton(_primary, true, false, false);

            _abortButton = CreateButton("AbortWave", transform, body, new Vector2(0.78f, 0.09f), new Vector2(0.97f, 0.155f));
            _abortLabel = _abortButton.GetComponentInChildren<Text>();
            _abortLabel.text = "Abort → Hangar";
            _abortLabel.fontSize = 16;
            _abortButton.onClick.AddListener(OnAbort);
            _abortPlate = _abortButton.targetGraphic as Image;
            UiTheme.ApplyButton(_abortButton, false, true, false);
            _abortButton.gameObject.SetActive(false);

            // End-credits roll, not currency. Anchored under hangar min.y 0.080 so it
            // does not sit on the shop; WAVE-CLEAR strip owns CREDITS (+delta).
            _creditsButton = CreateButton("OpenCredits", transform, body, new Vector2(0.014f, 0.010f), new Vector2(0.128f, 0.070f));
            _creditsButtonLabel = _creditsButton.GetComponentInChildren<Text>();
            _creditsButtonLabel.text = "Credits";
            _creditsButtonLabel.fontSize = 14;
            _creditsButton.onClick.AddListener(ShowEndCredits);
            UiTheme.ApplyButton(_creditsButton, false, false, false);

            BuildDoctrine(display, body);
            BuildShop(display, body);
            BuildSettingsGear(body);
            BuildLanguagePicker(display, body);
            BuildDifficultyPicker(display, body);
            BuildFirstHangarHint(display, body);
            BuildEndCredits(display, body);
            BuildShipPreviewFrame(display);
            BuildSettingsPanel(display, body);
            ApplyLocalizedStaticLabels();
            RefreshLanguageChrome();
            RefreshDifficultyChrome();
            ApplyFooterHintSize();
            EnsurePrimaryClickable();
        }

        private void OnDestroy()
        {
            SetSettingsNavigationLock(false);
            if (_previewCanvas != null)
            {
                Destroy(_previewCanvas);
                _previewCanvas = null;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void BuildShipPreviewFrame(Font display)
        {
            EnsureShipPreviewFrame(display);
        }

        private void EnsureShipPreviewFrame()
        {
            EnsureShipPreviewFrame(UiFonts.Display());
        }

        private void EnsureShipPreviewFrame(Font display)
        {
            if (display == null)
            {
                display = UiFonts.Display();
            }

            if (_previewCanvas == null)
            {
                GameObject canvasGo = new GameObject(ShipPreviewCanvasName);
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = false;
                canvas.overrideSorting = true;
                canvas.sortingOrder = ShipPreviewSortOrder;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGo.AddComponent<GraphicRaycaster>();
                _previewCanvas = canvasGo;
            }

            Canvas overlay = _previewCanvas.GetComponent<Canvas>();
            if (overlay != null)
            {
                overlay.renderMode = RenderMode.ScreenSpaceOverlay;
                overlay.overrideSorting = true;
                overlay.sortingOrder = ShipPreviewSortOrder;
                overlay.enabled = true;
            }

            if (_previewRoot == null)
            {
                _previewRoot = CreatePanel("ShipPreviewFrame", _previewCanvas.transform, UiAmber,
                    ShipPreviewMin, ShipPreviewMax);
                CreateFill("PreviewHeader", _previewRoot.transform, UiTheme.WithAlpha(UiTheme.Primary, 0.55f),
                    new Vector2(0f, 0.922f), new Vector2(1f, 1f));
                CreateFill("PreviewRule", _previewRoot.transform, UiTheme.HeaderRule,
                    new Vector2(0.05f, 0.914f), new Vector2(0.95f, 0.922f));
                CreateFill("PreviewOuterBezel", _previewRoot.transform, UiTheme.Primary,
                    new Vector2(0f, 0f), new Vector2(1f, 0.908f));
                CreateFill("PreviewInnerBezel", _previewRoot.transform, UiTheme.Void,
                    new Vector2(0.028f, 0.028f), new Vector2(0.972f, 0.880f));
                CreateFill("PreviewInnerAmber", _previewRoot.transform, UiTheme.Primary,
                    new Vector2(0.036f, 0.036f), new Vector2(0.964f, 0.872f));

                _previewCaption = CreateText("PreviewCaption", _previewRoot.transform, display, 16, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(_previewCaption.rectTransform, new Vector2(0.06f, 0.922f), new Vector2(0.94f, 0.992f));
                _previewCaption.color = UiAmber;
                _previewCaption.text = "LOADOUT";
                AddReadability(_previewCaption, true);

                GameObject well = CreateFill("PreviewWell", _previewRoot.transform, UiTheme.Void,
                    new Vector2(0.048f, 0.048f), new Vector2(0.952f, 0.860f));

                GameObject view = new GameObject("PreviewViewport", typeof(RectTransform));
                view.transform.SetParent(well.transform, false);
                RawImage raw = view.AddComponent<RawImage>();
                raw.color = UiTheme.Surface2;
                raw.texture = PreviewPlaceholder();
                raw.raycastTarget = false;
                raw.enabled = true;
                Stretch(view.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
                _previewViewport = raw;
            }

            Stretch(_previewRoot.GetComponent<RectTransform>(), ShipPreviewMin, ShipPreviewMax);
            _previewRoot.transform.SetAsLastSibling();
            ForcePreviewChrome();
        }

        private void ForcePreviewChrome()
        {
            bool show = _session == null || (_session.Phase != GamePhase.Playing && !_creditsVisible && !_settingsOpen);
            if (_previewCanvas != null)
            {
                _previewCanvas.SetActive(show);
                Canvas overlay = _previewCanvas.GetComponent<Canvas>();
                if (overlay != null)
                {
                    overlay.enabled = show;
                    overlay.overrideSorting = true;
                    overlay.sortingOrder = ShipPreviewSortOrder;
                    overlay.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            if (_previewRoot != null)
            {
                _previewRoot.SetActive(show);
                _previewRoot.transform.SetAsLastSibling();
                Stretch(_previewRoot.GetComponent<RectTransform>(), ShipPreviewMin, ShipPreviewMax);
                Image plate = _previewRoot.GetComponent<Image>();
                if (plate != null)
                {
                    plate.enabled = true;
                    plate.color = UiAmber;
                    plate.raycastTarget = false;
                }
            }

            if (_previewViewport != null)
            {
                _previewViewport.enabled = true;
                _previewViewport.raycastTarget = false;
                if (!_previewViewport.gameObject.activeSelf)
                {
                    _previewViewport.gameObject.SetActive(true);
                }

                if (_previewViewport.texture == null)
                {
                    _previewViewport.texture = PreviewPlaceholder();
                    _previewViewport.color = UiTheme.Surface2;
                }
                else
                {
                    _previewViewport.color = Color.white;
                }
            }
        }

        private static Texture2D PreviewPlaceholder()
        {
            if (_previewPlaceholder != null)
            {
                return _previewPlaceholder;
            }

            Texture2D tex = new Texture2D(8, 8, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            Color fill = UiTheme.Surface2;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    tex.SetPixel(x, y, fill);
                }
            }

            tex.Apply();
            tex.name = "HangarPreviewPlaceholder";
            _previewPlaceholder = tex;
            return _previewPlaceholder;
        }

        private void BuildShop(Font display, Font body)
        {
            // Shared header Y so HULL / WEAPONS / DEFENSE start on one line.
            _hullHeader = BuildGroupHeader(display, ShopCatalog.HullHeader, new Vector2(0.02f, 0.675f), new Vector2(0.49f, 0.728f));
            // WEAPONS title stays in the header band; slot readout (P/U) sits under the rule.
            _weaponsHeader = BuildGroupHeader(display, ShopCatalog.WeaponsHeader, new Vector2(0.51f, 0.704f), new Vector2(0.735f, 0.728f));
            _defenseHeader = BuildGroupHeader(display, ShopCatalog.DefenseHeader, new Vector2(0.755f, 0.675f), new Vector2(0.98f, 0.728f));

            float rowStep = ShopCellHeight + ShopCellGutter;
            float gridBottom = ShopGridTop - 4f * rowStep - ShopCellHeight;
            CreateFill("HullCol", _menuRoot.transform, UiTheme.InnerWash,
                new Vector2(0.02f, gridBottom), new Vector2(0.49f, ShopGridTop));
            CreateFill("WeaponsCol", _menuRoot.transform, UiTheme.InnerWash,
                new Vector2(0.51f, gridBottom), new Vector2(0.735f, ShopGridTop));
            CreateFill("DefenseCol", _menuRoot.transform, UiTheme.InnerWash,
                new Vector2(0.755f, gridBottom), new Vector2(0.98f, ShopGridTop));

            // Slot readout sits in the WEAPONS header band (0.675–0.728), not on shop cells.
            _loadoutSlots = CreateText("LoadoutSlots", _menuRoot.transform, body, 12, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_loadoutSlots.rectTransform, new Vector2(0.51f, 0.675f), new Vector2(0.735f, 0.704f));
            _loadoutSlots.color = UiTheme.Accent;
            ClampOneLine(_loadoutSlots);

            int shopCount = ShopCatalog.Items.Length;
            _buyButtons = new Button[shopCount];
            _buyLabels = new Text[shopCount];
            int hullIndex = 0;
            int weaponIndex = 0;
            int defenseIndex = 0;
            int doctrineIndex = 0;
            for (int i = 0; i < shopCount; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                Vector2 min;
                Vector2 max;
                ShopButtonRect(item.Group, ref hullIndex, ref weaponIndex, ref defenseIndex, ref doctrineIndex, out min, out max);
                Transform parent = item.Group == ShopGroup.Doctrine && _doctrineShopRow != null
                    ? _doctrineShopRow.transform
                    : _menuRoot.transform;
                Button button = CreateButton("Buy_" + item.Id, parent, body, min, max);
                int captured = i;
                button.onClick.AddListener(() => OnBuy(ShopCatalog.Items[captured].Id));
                BindShopHover(button, item);
                _buyButtons[i] = button;
                _buyLabels[i] = button.GetComponentInChildren<Text>();
                _buyLabels[i].fontSize = UiTheme.BodyMin;
                _buyLabels[i].fontStyle = FontStyle.Bold;
                _buyLabels[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                _buyLabels[i].verticalOverflow = VerticalWrapMode.Truncate;
                UiTheme.ApplyButton(button, false, false, true);
            }
        }

        private static void ShopButtonRect(
            ShopGroup group,
            ref int hullIndex,
            ref int weaponIndex,
            ref int defenseIndex,
            ref int doctrineIndex,
            out Vector2 min,
            out Vector2 max)
        {
            if (group == ShopGroup.Doctrine)
            {
                int doctrineCol = doctrineIndex % 2;
                float dx0 = 0.04f + doctrineCol * 0.48f;
                min = new Vector2(dx0, 0.08f);
                max = new Vector2(dx0 + 0.44f, 0.92f);
                doctrineIndex++;
                return;
            }

            float rowStep = ShopCellHeight + ShopCellGutter;
            // Shared cell height + gutter; all three columns share ShopGridTop.
            if (group == ShopGroup.Weapons)
            {
                float top = ShopGridTop - weaponIndex * rowStep;
                min = new Vector2(0.51f, top - ShopCellHeight);
                max = new Vector2(0.735f, top);
                weaponIndex++;
                return;
            }

            if (group == ShopGroup.Defense)
            {
                float top = ShopGridTop - defenseIndex * rowStep;
                min = new Vector2(0.755f, top - ShopCellHeight);
                max = new Vector2(0.98f, top);
                defenseIndex++;
                return;
            }

            int col = hullIndex % 4;
            int row = hullIndex / 4;
            float x0 = 0.02f + col * 0.1175f;
            float topHull = ShopGridTop - row * rowStep;
            min = new Vector2(x0, topHull - ShopCellHeight);
            max = new Vector2(x0 + 0.110f, topHull);
            hullIndex++;
        }

        private Text BuildGroupHeader(Font font, string label, Vector2 min, Vector2 max)
        {
            Text header = CreateText("Group_" + label, _menuRoot.transform, font, 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(header.rectTransform, min, max);
            header.color = UiAmber;
            header.text = label;
            CreateFill("Rule_" + label, _menuRoot.transform, UiTheme.HeaderRule,
                new Vector2(min.x, min.y), new Vector2(max.x, min.y + 0.008f));
            return header;
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
            DismissDoctrineIntro();
            if (_session != null && _session.WaveIndex == 1)
            {
                _firstRunCoachUntil = Time.unscaledTime + 6.5f;
            }

            if (AudioCues.Instance != null)
            {
                if (_session != null && _session.Phase == GamePhase.Failed)
                {
                    AudioCues.Instance.PlayRetry();
                }
                else
                {
                    AudioCues.Instance.PlayUiClick();
                }
            }

            _game.StartWave();
        }

        private void OnBuy(UpgradeId id)
        {
            if (_shop == null)
            {
                return;
            }

            if (_loadout != null && _loadout.State != null
                && _loadout.State.Owns(id)
                && WeaponSlots.IsWeapon(id))
            {
                _shop.TryEquip(id);
                return;
            }

            _shop.TryBuy(id);
        }

        private void OnAbort()
        {
            if (_game != null)
            {
                _game.AbortWave();
            }
        }

        private void BuildRunSummary(Font display, Font body)
        {
            // WAVE-CLEAR strip: own zone under top bar, above NEXT WAVE. Three rows,
            // hangar-local anchors with zero offset so EN/SV cannot reflow the shop.
            _summaryRoot = UiTheme.BuildPanel(
                "RunSummaryCard",
                _menuRoot.transform,
                new Vector2(0.02f, 0.82f),
                new Vector2(0.98f, 0.995f),
                0.90f);
            _summaryHeader = _summaryRoot.transform.Find("RunSummaryCardHeader").GetComponent<Image>();
            _summaryRule = _summaryRoot.transform.Find("RunSummaryCardRule").GetComponent<Image>();

            _summaryTitle = CreateText("SummaryTitle", _summaryRoot.transform, display, UiTheme.HeaderMin, TextAnchor.MiddleCenter, FontStyle.Bold);
            // r1 WAVE CLEAR — stretch, zero offset, one line so copy cannot cover NEXT WAVE.
            Stretch(_summaryTitle.rectTransform, new Vector2(0.03f, 0.90f), new Vector2(0.97f, 0.995f));
            _summaryTitle.color = UiTheme.Primary;
            ClampOneLine(_summaryTitle);

            _summaryBody = CreateText("SummaryBody", _summaryRoot.transform, body, UiTheme.BodyMin, TextAnchor.MiddleCenter, FontStyle.Normal);
            // r2 SCORE · WAVE · WORLD · CREDITS (+delta). One line, truncate.
            Stretch(_summaryBody.rectTransform, new Vector2(0.03f, 0.855f), new Vector2(0.97f, 0.90f));
            _summaryBody.color = UiTheme.Accent;
            ClampOneLine(_summaryBody);

            _waveMedal = CreateText("WaveMedal", _summaryRoot.transform, display, 14, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_waveMedal.rectTransform, new Vector2(0.72f, 0.90f), new Vector2(0.97f, 0.995f));
            _waveMedal.color = UiTheme.Primary;
            ClampOneLine(_waveMedal);

            _continueHint = CreateText("ContinueHint", _summaryRoot.transform, body, UiTheme.BodyMin, TextAnchor.MiddleLeft, FontStyle.Bold);
            // r3 upgrades / short status. Truncate; controls hint stays on the screen-bottom row.
            Stretch(_continueHint.rectTransform, new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.855f));
            _continueHint.color = UiTheme.Secondary;
            ClampOneLine(_continueHint);
            _summaryRoot.SetActive(false);
        }

        private void RefreshRunSummary(bool playing)
        {
            if (_summaryRoot == null)
            {
                return;
            }

            bool summaryPhase = !playing
                && (_session.Phase == GamePhase.WaveClear
                    || _session.Phase == GamePhase.Failed
                    || _session.Phase == GamePhase.CampaignClear);
            _summaryRoot.SetActive(!playing);
            if (_credits != null)
            {
                _credits.gameObject.SetActive(false);
            }

            if (_status != null)
            {
                Stretch(_status.rectTransform, new Vector2(0.03f, 0.016f), new Vector2(0.97f, 0.088f));
            }

            int wave = _session.LastResolvedWave > 0 ? _session.LastResolvedWave : _session.WaveIndex;
            int world = ContentFactory.WorldIndexForWave(wave);
            LoadoutState loadout = _loadout != null ? _loadout.State : null;
            bool failed = _session.Phase == GamePhase.Failed;
            ApplyFailChrome(failed && summaryPhase);
            if (_session.Phase == GamePhase.CampaignClear && _summaryTitle != null)
            {
                _summaryTitle.color = UiTheme.Primary;
                _summaryTitle.fontSize = UiTheme.HeaderMin;
            }

            if (summaryPhase)
            {
                _summaryTitle.text = RunSummary.Title(_session.Phase, FailReasonText());
            }
            else
            {
                _summaryTitle.text = string.Empty;
                if (_summaryTitle != null)
                {
                    _summaryTitle.color = UiTheme.Primary;
                }
            }

            string stats = RunSummary.StatsLine(_session.Score, wave, world)
                + "  ·  "
                + RunSummary.CreditsLine(_session.Credits, summaryPhase ? _session.LastCreditsAwarded : 0);
            if (failed && summaryPhase)
            {
                string doctrineLine = RunSummary.DoctrineRunLine(
                    loadout != null ? loadout.Doctrine : DoctrineId.None,
                    _session.WaveIndex);
                if (!string.IsNullOrEmpty(doctrineLine))
                {
                    stats = doctrineLine + "  ·  " + stats;
                }
            }

            _summaryBody.text = stats;
            ClampOneLine(_summaryBody);

            bool medal = summaryPhase && RunSummary.ShowWaveMedal(_session.LastResolvedWave, _session.Phase);
            if (_waveMedal != null)
            {
                _waveMedal.gameObject.SetActive(medal);
                if (medal)
                {
                    _waveMedal.text = RunSummary.WaveMedal(_session.LastResolvedWave);
                    _waveMedal.color = RunSummary.IsWorld3EntryLine(_session.LastResolvedWave)
                        ? UiTheme.Secondary
                        : UiTheme.Primary;
                }
            }

            string row3 = summaryPhase
                ? RunSummary.UpgradesLine(loadout)
                : HangarReadyStatus();
            if (summaryPhase)
            {
                LocalBest sessionBest = _game != null ? _game.SessionBest : null;
                if (sessionBest != null)
                {
                    row3 += "  ·  " + sessionBest.DeathRetryLine(_session.LastRunScore);
                }

                if (_game != null && _game.LastRunWasNewBest)
                {
                    row3 += "  ·  " + Loc.T("ui.new_best", "NEW BEST");
                }

                if (_session.Phase == GamePhase.CampaignClear)
                {
                    row3 = RunSummary.CampaignWinHint() + "  ·  " + row3;
                }
                else if (failed && RunSummary.ShowFailContinue(_session.Phase))
                {
                    row3 = DamageCauseText.PlayerFaultLine(FailReasonText())
                        + "  ·  "
                        + RunSummary.FailContinueHint(
                            FailReasonText(),
                            _session.WaveIndex,
                            _session.FailRemainingThreats).Replace("\n", "  ·  ")
                        + "  ·  " + RunSummary.UpgradesLine(loadout);
                }
                else if (RunSummary.ShowContinueHint(_session.LastResolvedWave, _session.Phase))
                {
                    string continueLine = RunSummary.ContinueHint(
                        _session.LastResolvedWave,
                        _session.Credits,
                        loadout);
                    if (!string.IsNullOrEmpty(continueLine))
                    {
                        row3 += "  ·  " + continueLine.Replace("\n", "  ·  ");
                    }
                }
            }

            if (_continueHint != null)
            {
                _continueHint.text = row3;
                _continueHint.gameObject.SetActive(true);
                ClampOneLine(_continueHint);
            }
        }

        private void ApplyFailChrome(bool failed)
        {
            if (_summaryHeader != null)
            {
                _summaryHeader.color = failed ? UiTheme.DangerHeader : UiTheme.HeaderWash;
            }

            if (_summaryRule != null)
            {
                _summaryRule.color = failed ? UiTheme.Danger : UiTheme.HeaderRule;
            }

            if (_summaryTitle != null)
            {
                _summaryTitle.fontSize = UiTheme.HeaderMin;
                _summaryTitle.color = failed ? UiTheme.Danger : UiTheme.Primary;
            }

            if (_summaryRoot != null)
            {
                Image plate = _summaryRoot.GetComponent<Image>();
                if (plate != null)
                {
                    plate.color = UiTheme.PanelPlate;
                }
            }

            if (_continueHint != null)
            {
                _continueHint.color = failed ? UiTheme.Accent : UiTheme.Secondary;
            }

            if (_primary != null)
            {
                UiTheme.ApplyButton(_primary, true, false, false);
            }
        }

        public void SetAbortUrgent(bool stranded)
        {
            _abortUrgent = stranded;
            if (!stranded && _abortPlate != null)
            {
                _abortPlate.color = UiTheme.DangerTint;
            }
        }

        public void FlashHit(float strength)
        {
            _hitFlashStrength = Mathf.Max(_hitFlashStrength, Mathf.Clamp01(strength));
            _hitFlashUntil = Time.unscaledTime + 0.12f;
            ApplyHitFlash();
        }

        private void ApplyHitFlash()
        {
            if (_hitFlash == null)
            {
                return;
            }

            if (Time.unscaledTime >= _hitFlashUntil || _hitFlashStrength <= 0.01f)
            {
                _hitFlash.color = new Color(1f, 0.96f, 0.92f, 0f);
                _hitFlashStrength = 0f;
                return;
            }

            float pulse = Mathf.Clamp01((_hitFlashUntil - Time.unscaledTime) / 0.12f);
            _hitFlash.color = new Color(1f, 0.96f, 0.92f, _hitFlashStrength * pulse);
        }

        private void BuildFirstHangarHint(Font display, Font body)
        {
            _tutorialDismissed = PlayerPrefs.GetInt(FirstHangarHintKey, 0) == 1;
            _doctrineIntroDismissed = PlayerPrefs.GetInt(DoctrineHintKey, 0) == 1;
            // First-flight card occupies the WAVE-CLEAR strip zone (not the shop grid).
            _tutorialRoot = UiTheme.BuildPanel(
                "FirstHangarHint",
                transform,
                new Vector2(0.018f, 0.730f),
                new Vector2(0.545f, 0.888f),
                0.94f);

            _firstFlightTitle = CreateText("HintTitle", _tutorialRoot.transform, display, 16, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_firstFlightTitle.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.97f));
            _firstFlightTitle.color = UiTheme.Primary;
            _firstFlightTitle.text = "First flight";

            _firstFlightBody = CreateText("HintBody", _tutorialRoot.transform, body, UiTheme.HintSize(Screen.width), TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(_firstFlightBody.rectTransform, new Vector2(0.07f, 0.2f), new Vector2(0.93f, 0.85f));
            _firstFlightBody.color = UiTheme.Accent;
            _firstFlightBody.text = HangarHintBody;

            _gotItButton = CreateButton("DismissHint", _tutorialRoot.transform, display,
                new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.18f));
            _gotItLabel = _gotItButton.GetComponentInChildren<Text>();
            _gotItLabel.text = "Got it";
            _gotItLabel.fontSize = 15;
            _gotItButton.onClick.AddListener(OnDismissHintClicked);
            UiTheme.ApplyButton(_gotItButton, true, false, false);
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

        private bool DoctrineIntroOpen()
        {
            return _session != null
                && !_doctrineIntroDismissed
                && (_session.Phase == GamePhase.Hangar || _session.Phase == GamePhase.WaveClear)
                && DoctrineRules.HangarUnlocked(_session.WaveIndex);
        }

        private void RefreshDoctrineIntro()
        {
            bool open = DoctrineIntroOpen();
            if (_doctrineIntro != null)
            {
                _doctrineIntro.SetActive(open);
            }

            if (_doctrineHint != null)
            {
                _doctrineHint.gameObject.SetActive(!open);
            }

            if (!open || _doctrineIntroBody == null)
            {
                return;
            }

            _doctrineIntroBody.text = Loc.T("ui.doctrine.hint_body", DoctrineHintBody);
            if (_doctrineTitle != null)
            {
                _doctrineTitle.text = Loc.T("ui.doctrine.hint_title", DoctrineHintTitle);
            }

            if (_doctrineIntroGotIt != null)
            {
                _doctrineIntroGotIt.text = Loc.T("ui.got_it", "Got it");
            }
        }

        private void OnDismissDoctrineIntroClicked()
        {
            DismissDoctrineIntro();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void DismissDoctrineIntro()
        {
            if (_doctrineIntroDismissed)
            {
                return;
            }

            _doctrineIntroDismissed = true;
            PlayerPrefs.SetInt(DoctrineHintKey, 1);
            PlayerPrefs.Save();
            RefreshDoctrinePicks();
            RefreshDoctrineIntro();
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

        private void BuildEndCredits(Font display, Font body)
        {
            _endCreditsRoot = UiTheme.BuildPanel(
                "EndCredits",
                transform,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                0.86f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.WithAlpha(UiTheme.Void, 0.96f));
            Image scrim = _endCreditsRoot.GetComponent<Image>();
            if (scrim != null)
            {
                scrim.raycastTarget = true;
            }

            CreateFill("CreditsPlate", _endCreditsRoot.transform, UiTheme.WithAlpha(UiTheme.Surface, 0.72f),
                new Vector2(0.2f, 0.16f), new Vector2(0.8f, 0.84f));

            Text title = CreateText("CreditsTitle", _endCreditsRoot.transform, display, EndCredits.TitleSize,
                TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(title.rectTransform, new Vector2(0.22f, 0.78f), new Vector2(0.78f, 0.92f));
            title.color = EndCredits.TitleColor;
            title.text = EndCredits.Title();
            AddReadability(title, true);

            GameObject window = CreatePanel("CreditsWindow", _endCreditsRoot.transform, UiTheme.InnerWash,
                new Vector2(0.24f, 0.22f), new Vector2(0.76f, 0.76f));
            window.AddComponent<RectMask2D>();
            Image windowImage = window.GetComponent<Image>();
            if (windowImage != null)
            {
                windowImage.raycastTarget = false;
            }

            _endCreditsBody = CreateText("CreditsBody", window.transform, body, EndCredits.BodySize,
                TextAnchor.UpperCenter, FontStyle.Normal);
            Stretch(_endCreditsBody.rectTransform, new Vector2(0.04f, -1.4f), new Vector2(0.96f, 1f));
            _endCreditsBody.color = EndCredits.BodyColor;
            _endCreditsBody.text = EndCredits.Body();
            AddReadability(_endCreditsBody, false);

            _creditsContinue = CreateButton("CreditsContinue", _endCreditsRoot.transform, display,
                new Vector2(0.34f, 0.045f), new Vector2(0.66f, 0.13f));
            _creditsContinueLabel = _creditsContinue.GetComponentInChildren<Text>();
            _creditsContinueLabel.text = "Continue";
            _creditsContinueLabel.fontSize = 20;
            UiTheme.ApplyButton(_creditsContinue, true, false, false);

            _creditsContinue.onClick.AddListener(() => HideEndCredits(true));
            _endCreditsRoot.SetActive(false);
            _endCreditsRoot.transform.SetAsLastSibling();
        }

        private void ShowEndCredits()
        {
            if (_endCreditsRoot == null)
            {
                return;
            }

            DismissFirstHangarHint();
            _creditsVisible = true;
            _creditsScroll = 0f;
            if (_endCreditsBody != null)
            {
                _endCreditsBody.text = EndCredits.Body();
                _endCreditsBody.rectTransform.anchoredPosition = Vector2.zero;
            }

            _endCreditsRoot.SetActive(true);
            _endCreditsRoot.transform.SetAsLastSibling();
            ForcePreviewChrome();

            if (_creditsButton != null)
            {
                _creditsButton.gameObject.SetActive(false);
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayCreditsOpen();
                AudioCues.Instance.PlayCreditsLoop();
            }
        }

        private void HideEndCredits(bool restoreHangarMusic)
        {
            if (!_creditsVisible && (_endCreditsRoot == null || !_endCreditsRoot.activeSelf))
            {
                _creditsVisible = false;
                return;
            }

            _creditsVisible = false;
            if (_endCreditsRoot != null)
            {
                _endCreditsRoot.SetActive(false);
            }

            ForcePreviewChrome();

            if (_creditsButton != null && _session != null && _session.Phase != GamePhase.Playing)
            {
                _creditsButton.gameObject.SetActive(true);
            }

            if (AudioCues.Instance == null)
            {
                return;
            }

            if (restoreHangarMusic)
            {
                AudioCues.Instance.PlayCreditsClose();
                AudioCues.Instance.StopCreditsMusic();
            }
            else
            {
                AudioCues.Instance.StopCreditsMusic();
            }
        }

        private void BuildLanguagePicker(Font display, Font body)
        {
            // Top-bar center-right: LANG flags. Same top-bar row as difficulty. The gear sits further right.
            _langPanel = UiTheme.BuildPanel(
                "LanguagePanel",
                transform,
                new Vector2(0.635f, 0.905f),
                new Vector2(0.728f, 0.995f),
                0.02f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.WithAlpha(UiTheme.Surface, 0.88f));

            _langTitle = CreateText("LangTitle", _langPanel.transform, display, 10, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_langTitle.rectTransform, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.98f));
            _langTitle.color = UiTheme.Primary;

            Vector2 flagMin;
            Vector2 flagMax;
            LanguageFlagRect(true, out flagMin, out flagMax);
            _enBezel = CreateFlagButton(
                "UsFlag",
                _langPanel.transform,
                flagMin,
                flagMax,
                () => OnPickLanguage(GameLanguage.English));
            BuildUsFlag(_enBezel.transform);

            LanguageFlagRect(false, out flagMin, out flagMax);
            _svBezel = CreateFlagButton(
                "SvFlag",
                _langPanel.transform,
                flagMin,
                flagMax,
                () => OnPickLanguage(GameLanguage.Swedish));
            BuildSwedishFlag(_svBezel.transform);

            Text enCaption = CreateText("EnCaption", _langPanel.transform, body, 9, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(enCaption.rectTransform, new Vector2(0.10f, 0.04f), new Vector2(0.46f, 0.22f));
            enCaption.color = UiTheme.Accent;
            enCaption.text = "EN";

            Text svCaption = CreateText("SvCaption", _langPanel.transform, body, 9, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(svCaption.rectTransform, new Vector2(0.54f, 0.04f), new Vector2(0.90f, 0.22f));
            svCaption.color = UiTheme.Accent;
            svCaption.text = "SV";
        }

        private static void LanguageFlagRect(bool english, out Vector2 min, out Vector2 max)
        {
            float width = 0.40f * LanguageFlagScale / 0.60f;
            float height = 0.50f * LanguageFlagScale / 0.60f;
            float midX = english ? 0.28f : 0.72f;
            float midY = 0.50f;
            min = new Vector2(midX - width * 0.5f, midY - height * 0.5f);
            max = new Vector2(midX + width * 0.5f, midY + height * 0.5f);
        }

        private static Image CreateFlagButton(
            string name,
            Transform parent,
            Vector2 min,
            Vector2 max,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = UiTheme.Surface2;
            image.raycastTarget = true;
            Button button = go.AddComponent<Button>();
            button.colors = UiTheme.MenuButtonColors();
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            button.onClick.AddListener(onClick);
            Stretch(go.GetComponent<RectTransform>(), min, max);
            UiTheme.EnsureFocusFill(go);
            UiTheme.EnsureRing(go);
            return image;
        }

        private static void BuildUsFlag(Transform parent)
        {
            Color red = new Color(0.55f, 0.12f, 0.16f, 0.78f);
            Color white = UiTheme.WithAlpha(UiTheme.Secondary, 0.72f);
            Color blue = new Color(0.10f, 0.14f, 0.32f, 0.78f);
            const int Stripes = 7;
            for (int i = 0; i < Stripes; i++)
            {
                float y1 = 1f - (i / (float)Stripes);
                float y0 = 1f - ((i + 1) / (float)Stripes);
                CreateFill("UsStripe" + i, parent, i % 2 == 0 ? red : white, new Vector2(0.06f, y0 + 0.04f), new Vector2(0.94f, y1 - 0.02f));
            }

            CreateFill("UsCanton", parent, blue, new Vector2(0.06f, 0.46f), new Vector2(0.46f, 0.96f));
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float x = 0.12f + col * 0.1f;
                    float y = 0.56f + row * 0.12f;
                    CreateFill("UsStar" + row + col, parent, white, new Vector2(x, y), new Vector2(x + 0.045f, y + 0.055f));
                }
            }
        }

        private static void BuildSwedishFlag(Transform parent)
        {
            Color blue = new Color(0.08f, 0.36f, 0.52f, 0.78f);
            Color yellow = UiTheme.WithAlpha(UiTheme.Primary, 0.78f);
            CreateFill("SvField", parent, blue, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f));
            CreateFill("SvCrossH", parent, yellow, new Vector2(0.06f, 0.38f), new Vector2(0.94f, 0.62f));
            CreateFill("SvCrossV", parent, yellow, new Vector2(0.30f, 0.06f), new Vector2(0.50f, 0.94f));
        }

        private void OnPickLanguage(GameLanguage language)
        {
            Loc.SetLanguage(language);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }

            RefreshLanguageChrome();
            Refresh();
        }

        private void BuildDifficultyPicker(Font display, Font body)
        {
            // Top-bar center: DIFFICULTY chips. Same y as LANG / audio so they never sit on WAVE CLEAR.
            _diffPanel = UiTheme.BuildPanel(
                "DifficultyPanel",
                transform,
                new Vector2(0.478f, 0.905f),
                new Vector2(0.628f, 0.995f),
                0.02f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.WithAlpha(UiTheme.Surface, 0.88f));

            _diffTitle = CreateText("DiffTitle", _diffPanel.transform, display, 10, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_diffTitle.rectTransform, new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.98f));
            _diffTitle.color = UiTheme.Primary;

            _easyBezel = CreateDifficultyButton(
                "DiffEasy",
                _diffPanel.transform,
                new Vector2(0.04f, 0.10f),
                new Vector2(0.34f, 0.70f),
                () => OnPickDifficulty(DifficultyGrade.Easy),
                out _easyLabel,
                body);
            _normalBezel = CreateDifficultyButton(
                "DiffNormal",
                _diffPanel.transform,
                new Vector2(0.36f, 0.10f),
                new Vector2(0.66f, 0.70f),
                () => OnPickDifficulty(DifficultyGrade.Normal),
                out _normalLabel,
                body);
            _hardBezel = CreateDifficultyButton(
                "DiffHard",
                _diffPanel.transform,
                new Vector2(0.68f, 0.10f),
                new Vector2(0.96f, 0.70f),
                () => OnPickDifficulty(DifficultyGrade.Hard),
                out _hardLabel,
                body);
        }

        private static Image CreateDifficultyButton(
            string name,
            Transform parent,
            Vector2 min,
            Vector2 max,
            UnityEngine.Events.UnityAction onClick,
            out Text label,
            Font body)
        {
            Image bezel = CreateFlagButton(name, parent, min, max, onClick);
            label = CreateText(name + "Label", bezel.transform, body, 10, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(label.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
            label.color = UiBody;
            label.raycastTarget = false;
            return bezel;
        }

        private void OnPickDifficulty(DifficultyGrade grade)
        {
            if (_game != null)
            {
                _game.SetDifficulty(grade);
            }
            else
            {
                DifficultySettings.SetGrade(grade);
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }

            RefreshDifficultyChrome();
            Refresh();
        }

        private void RefreshDifficultyChrome()
        {
            DifficultyGrade grade = DifficultySettings.Current;
            EventSystem es = EventSystem.current;
            GameObject selected = es != null ? es.currentSelectedGameObject : null;
            PaintChip(_easyBezel, grade == DifficultyGrade.Easy, selected);
            PaintChip(_normalBezel, grade == DifficultyGrade.Normal, selected);
            PaintChip(_hardBezel, grade == DifficultyGrade.Hard, selected);
        }

        public void AnnounceLifeLost(int livesLeft)
        {
            string lives = Loc.Tf("ui.life_lost", "LIFE LOST  ·  {0} left", livesLeft);
            LocalBest session = _game != null ? _game.SessionBest : null;
            if (session != null && _session != null)
            {
                lives += "  ·  " + session.DeathRetryLine(_session.Score);
            }

            AnnounceMedalBeat(lives, 1.6f);
        }

        public void AnnounceAchievement(AchievementId id)
        {
            if (_achievementToast == null)
            {
                return;
            }

            _achievementToast.text = AchievementCatalog.ToastLine(id);
            _achievementToast.gameObject.SetActive(true);
            _achievementUntil = Time.unscaledTime + 2.8f;
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshLanguageChrome()
        {
            EventSystem es = EventSystem.current;
            GameObject selected = es != null ? es.currentSelectedGameObject : null;
            PaintChip(_enBezel, Loc.Language == GameLanguage.English, selected);
            PaintChip(_svBezel, Loc.Language == GameLanguage.Swedish, selected);
            PaintChip(_settingsEnBezel, Loc.Language == GameLanguage.English, selected);
            PaintChip(_settingsSvBezel, Loc.Language == GameLanguage.Swedish, selected);
            if (_settingsLanguageValue != null)
            {
                _settingsLanguageValue.text = Loc.Language == GameLanguage.Swedish
                    ? Loc.T("ui.settings.sv", "SV")
                    : Loc.T("ui.settings.en", "EN");
            }
        }

        private static void PaintChip(Image bezel, bool chosen, GameObject padFocus)
        {
            if (bezel == null)
            {
                return;
            }

            bool focused = padFocus != null && padFocus == bezel.gameObject;
            UiTheme.PaintLanguageChip(bezel, chosen, focused);
        }

        private void ApplyLocalizedStaticLabels()
        {
            if (_abortLabel != null)
            {
                _abortLabel.text = Loc.T("ui.abort", "Abort → Hangar");
            }

            if (_creditsButtonLabel != null)
            {
                _creditsButtonLabel.text = Loc.T("ui.credits", "Credits");
            }

            if (_creditsContinueLabel != null)
            {
                _creditsContinueLabel.text = Loc.T("ui.continue", "Continue");
            }

            if (_endCreditsBody != null)
            {
                _endCreditsBody.text = EndCredits.Body();
            }

            if (_firstFlightTitle != null)
            {
                _firstFlightTitle.text = Loc.T("ui.first_flight", "First flight");
            }

            if (_firstFlightBody != null)
            {
                _firstFlightBody.text = Loc.T("ui.hangar_hint_body", HangarHintBody);
            }

            if (_gotItLabel != null)
            {
                _gotItLabel.text = Loc.T("ui.got_it", "Got it");
            }

            if (_langTitle != null)
            {
                _langTitle.text = Loc.T("ui.lang", "LANG");
            }

            if (_diffTitle != null)
            {
                _diffTitle.text = Loc.T("ui.difficulty", "DIFFICULTY");
            }

            if (_easyLabel != null)
            {
                _easyLabel.text = Loc.T("ui.diff.easy", "Easy");
            }

            if (_normalLabel != null)
            {
                _normalLabel.text = Loc.T("ui.diff.normal", "Normal");
            }

            if (_hardLabel != null)
            {
                _hardLabel.text = Loc.T("ui.diff.hard", "Hard");
            }

            if (_healthTitle != null)
            {
                _healthTitle.text = Loc.T("ui.health", "HEALTH");
            }

            if (_hullBarLabel != null)
            {
                _hullBarLabel.text = Loc.T("ui.hull_label", "HULL");
            }

            if (_shieldBarLabel != null)
            {
                _shieldBarLabel.text = Loc.T("ui.shield_label", "SHIELD");
            }

            if (_previewCaption != null)
            {
                _previewCaption.text = Loc.T("ui.ship_preview", "LOADOUT");
            }

            RefreshLoadoutSlots();

            if (_hullHeader != null)
            {
                _hullHeader.text = ShopCatalog.HeaderFor(ShopGroup.Hull);
            }

            if (_weaponsHeader != null)
            {
                _weaponsHeader.text = ShopCatalog.HeaderFor(ShopGroup.Weapons);
            }

            if (_defenseHeader != null)
            {
                _defenseHeader.text = ShopCatalog.HeaderFor(ShopGroup.Defense);
            }

            if (_settingsGearLabel != null)
            {
                _settingsGearLabel.text = Loc.T("ui.settings", "Settings");
            }

            if (_settingsTitle != null)
            {
                _settingsTitle.text = Loc.T("ui.settings", "Settings");
            }

            if (_settingsLanguageLabel != null)
            {
                _settingsLanguageLabel.text = Loc.T("ui.settings.language", "Language");
            }

            if (_settingsControlsTitle != null)
            {
                _settingsControlsTitle.text = Loc.T("ui.settings.controls", "Controls");
            }

            if (_settingsControlsBody != null)
            {
                _settingsControlsBody.text = FullControlHint();
            }

            RefreshSettingsAudio();
            RefreshSettingsShake();

            if (_settingsRowButtons != null)
            {
                int closeIndex = IndexOfSettingsRow(SettingsRowId.Close);
                if (closeIndex >= 0 && closeIndex < _settingsRowButtons.Length)
                {
                    Button closeRow = _settingsRowButtons[closeIndex];
                    if (closeRow != null)
                    {
                        Text closeLabel = closeRow.GetComponentInChildren<Text>();
                        if (closeLabel != null)
                        {
                            closeLabel.text = Loc.T("ui.settings.close", "Close");
                        }
                    }
                }
            }
        }

        public void AnnounceWorldChange(int world)
        {
            _flashedWorld = world;
            _flashedLayout = ArenaLayout.Title(ArenaLayout.ForWorld(world));
            _flashedBadge = ArenaLayout.Badge(ArenaLayout.ForWorld(world));
            _worldFlashUntil = Time.unscaledTime + 2.2f;
            bool hangar = _session != null && _session.Phase != GamePhase.Playing;
            if (hangar)
            {
                Stretch(_world.rectTransform, new Vector2(0.205f, 0.905f), new Vector2(0.355f, 0.995f));
            }
            else
            {
                Stretch(_world.rectTransform, new Vector2(0.48f, 0.72f), new Vector2(0.97f, 0.98f));
            }
            RefreshWorldBadge();
        }

        public void AnnounceMedalBeat(string line)
        {
            AnnounceMedalBeat(line, MedalCatalog.World2FlashSeconds);
        }

        public void AnnounceMedalBeat(string line, float seconds)
        {
            _medalBeat = line ?? string.Empty;
            if (string.IsNullOrEmpty(_medalBeat))
            {
                return;
            }

            _worldFlashUntil = Mathf.Max(_worldFlashUntil, Time.unscaledTime + Mathf.Max(1.2f, seconds));
            bool hangar = _session != null && _session.Phase != GamePhase.Playing;
            if (hangar)
            {
                Stretch(_world.rectTransform, new Vector2(0.205f, 0.905f), new Vector2(0.355f, 0.995f));
            }
            else
            {
                Stretch(_world.rectTransform, new Vector2(0.52f, 0.76f), new Vector2(0.97f, 0.98f));
            }
            RefreshWorldBadge();
        }

        private void Update()
        {
            if (_settingsOpen && _session != null && _session.Phase == GamePhase.Playing)
            {
                DismissSettingsForPlay();
            }

            // Settings panel, then credits, then play abort / hangar. An open panel
            // consumes Esc, Start, B, and Submit so they cannot reach Next Wave.
            SettingsInputFlags settingsFlags = ReadSettingsFlags();
            SettingsRoute settingsRoute = SettingsInputRouter.Route(settingsFlags);
            if (settingsRoute == SettingsRoute.CloseSave)
            {
                CloseSettings();
            }
            else if (settingsRoute == SettingsRoute.Activate)
            {
                ActivateSettingsRow();
            }
            else if (settingsRoute == SettingsRoute.Move || settingsRoute == SettingsRoute.Nudge)
            {
                StepSettingsNav(settingsRoute, settingsFlags);
            }
            else if (settingsRoute == SettingsRoute.Open)
            {
                OpenSettings();
            }
            else if (settingsRoute == SettingsRoute.CreditsClose)
            {
                HideEndCredits(true);
            }
            else if (settingsRoute == SettingsRoute.PlayAbort)
            {
                OnAbort();
            }
            else if (settingsRoute == SettingsRoute.HangarEscape)
            {
                OnHangarEscape();
            }
            else if (settingsRoute == SettingsRoute.HangarStart)
            {
                OnHangarStart();
            }
            else if (settingsRoute == SettingsRoute.HangarBack)
            {
                OnHangarBack();
            }

            if (SettingsInputRouter.BlocksHangarPad(settingsFlags))
            {
                if (_settingsOpen)
                {
                    SetSettingsNavigationLock(true);
                    if (settingsRoute != SettingsRoute.Move && settingsRoute != SettingsRoute.Nudge)
                    {
                        _settingsNavHeld = false;
                    }
                }
            }
            else
            {
                NavigateHangarPad();
                SyncHangarPadSelection();
            }
            if (_session == null || _session.Phase != GamePhase.Playing)
            {
                EnsureHangarPreview(_ship);
            }
            else
            {
                ForcePreviewChrome();
            }

            if (_creditsVisible && _endCreditsBody != null)
            {
                _creditsScroll += EndCredits.ScrollSpeed * Time.unscaledDeltaTime;
                _endCreditsBody.rectTransform.anchoredPosition = new Vector2(0f, _creditsScroll);
            }

            if (_session != null && _session.Phase == GamePhase.Playing && _hud != null)
            {
                _hud.text = BuildHud(true);
                RefreshHealthBar();
                RefreshUtilityHud(true);
                if (_hint != null && Time.unscaledTime < _firstRunCoachUntil)
                {
                    ApplyFooterHintSize();
                    _hint.text = Loc.T("ui.first_wave_coach", FirstWaveCoach);
                    ClampOneLine(_hint);
                }
            }

            PulseHangarLaunch();
            PulseAbortIfUrgent();
            ApplyHitFlash();
            PulseAchievementToast();

            if (_world == null)
            {
                return;
            }

            if (Time.unscaledTime < _worldFlashUntil)
            {
                float pulse = Mathf.PingPong(Time.unscaledTime * 3.2f, 1f);
                _world.fontSize = 22 + (int)(4f * pulse);
                bool world3 = _flashedWorld == MedalCatalog.World3EntryWorld;
                Color flashTone = world3 ? UiTheme.Secondary : UiTheme.Primary;
                _world.color = Color.Lerp(flashTone, UiTheme.Brighten(flashTone, 0.18f), pulse);
                ArenaLayoutId flashId = ArenaLayout.ForWorld(_flashedWorld);
                _flashedLayout = ArenaLayout.Title(flashId);
                _flashedBadge = ArenaLayout.Badge(flashId);
                string flash = Loc.Tf("ui.layout_swap", "LAYOUT SWAP\nWORLD {0}  ONLINE", _flashedWorld);
                if (!string.IsNullOrEmpty(_flashedLayout))
                {
                    string badge = string.IsNullOrEmpty(_flashedBadge)
                        ? _flashedLayout.ToUpperInvariant()
                        : _flashedBadge;
                    flash += "\n" + badge + "  ·  " + _flashedLayout.ToUpperInvariant();
                }

                if (!string.IsNullOrEmpty(_medalBeat))
                {
                    flash += "\n" + _medalBeat;
                }

                _world.text = flash;
                return;
            }

            if (_world.fontSize != 22 || !string.IsNullOrEmpty(_medalBeat))
            {
                _world.fontSize = 22;
                _medalBeat = string.Empty;
                Stretch(_world.rectTransform, new Vector2(0.205f, 0.905f), new Vector2(0.355f, 0.995f));
                RefreshWorldBadge();
            }
        }

        private void PulseHangarLaunch()
        {
            if (_primaryPlate == null || _session == null || _session.Phase == GamePhase.Playing)
            {
                return;
            }

            bool first = !_tutorialDismissed && _session.WaveIndex == 1 && _session.Phase == GamePhase.Hangar;
            if (!first)
            {
                _primaryPlate.color = UiTheme.PrimaryCta;
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 2.4f, 1f);
            _primaryPlate.color = Color.Lerp(UiTheme.PrimaryCta, UiTheme.Brighten(UiTheme.Primary, 0.12f), pulse);
        }

        private void PulseAbortIfUrgent()
        {
            if (_abortPlate == null || !_abortUrgent || _session == null || _session.Phase != GamePhase.Playing)
            {
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 4.2f, 1f);
            _abortPlate.color = Color.Lerp(UiTheme.DangerTint, UiTheme.Brighten(UiTheme.Danger, 0.18f), pulse);
        }

        private void PulseAchievementToast()
        {
            if (_achievementToast == null)
            {
                return;
            }

            if (Time.unscaledTime >= _achievementUntil)
            {
                _achievementToast.gameObject.SetActive(false);
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 3.4f, 1f);
            _achievementToast.color = Color.Lerp(UiTheme.Primary, UiTheme.Focus, pulse);
        }

        private void OnHangarEscape()
        {
            DismissHangarHints();
            FocusHangarSlot(HangarPadNav.PrimarySlot);
        }

        private void OnHangarStart()
        {
            OnPrimary();
        }

        private void OnHangarBack()
        {
            DismissHangarHints();
            FocusHangarSlot(HangarPadNav.PrimarySlot);
        }

        private void DismissHangarHints()
        {
            if (_tutorialRoot != null && _tutorialRoot.activeSelf)
            {
                OnDismissHintClicked();
            }

            if (DoctrineIntroOpen())
            {
                OnDismissDoctrineIntroClicked();
            }
        }

        private void EnsurePrimaryClickable()
        {
            if (_primary == null)
            {
                return;
            }

            _primary.interactable = true;
            Image plate = _primary.targetGraphic as Image;
            if (plate != null)
            {
                plate.raycastTarget = true;
            }

            _primary.transform.SetAsLastSibling();
            if (_settingsOpen && _settingsRoot != null)
            {
                _settingsRoot.transform.SetAsLastSibling();
            }
        }

        private void FocusHangarSlot(int slot)
        {
            _padSlot = slot;
            EventSystem focus = EventSystem.current;
            if (focus == null)
            {
                return;
            }

            Button button = ButtonFromSlot(slot);
            if (button == null)
            {
                return;
            }

            focus.SetSelectedGameObject(button.gameObject);
        }

        private void FocusPrimaryIfSelectionInvalid()
        {
            if (_creditsVisible)
            {
                return;
            }

            EventSystem focus = EventSystem.current;
            if (focus == null)
            {
                return;
            }

            GameObject current = focus.currentSelectedGameObject;
            if (!SelectionNeedsPrimaryFallback(current))
            {
                _padSlot = SlotFromSelected(current);
                return;
            }

            FocusHangarSlot(HangarPadNav.PrimarySlot);
        }

        private bool SelectionNeedsPrimaryFallback(GameObject current)
        {
            if (_creditsVisible)
            {
                return current == null || !current.activeInHierarchy;
            }

            if (current == null || !current.activeInHierarchy)
            {
                return true;
            }

            Button button = current.GetComponent<Button>();
            if (button == null)
            {
                return false;
            }

            if (!button.IsInteractable())
            {
                return true;
            }

            if (_primary != null && current == _primary.gameObject)
            {
                return false;
            }

            int slot = SlotFromSelected(current);
            if (slot == HangarPadNav.PrimarySlot)
            {
                return false;
            }

            bool[] padMask = PadSelectableMask();
            if (slot < 0 || slot >= padMask.Length)
            {
                return true;
            }

            return !padMask[slot];
        }

        private bool[] PadSelectableMask()
        {
            int count = HangarPadNav.SlotCount;
            if (_padSelectable == null || _padSelectable.Length != count)
            {
                _padSelectable = new bool[count];
            }

            for (int index = 0; index < count; index++)
            {
                _padSelectable[index] = SlotIsSelectable(index);
            }

            HangarPadNav.ForcePrimarySelectable(_padSelectable);
            return _padSelectable;
        }

        private bool SlotIsSelectable(int slot)
        {
            if (slot == HangarPadNav.GotItSlot || slot == HangarPadNav.DoctrineHintSlot)
            {
                return false;
            }

            Button button = ButtonFromSlot(slot);
            if (button == null || !button.gameObject.activeInHierarchy || !button.IsInteractable())
            {
                return false;
            }

            return true;
        }

        private void NavigateHangarPad()
        {
            if (_session == null || _session.Phase == GamePhase.Playing)
            {
                _padHeld = false;
                EventSystem playing = EventSystem.current;
                if (playing != null && playing.currentSelectedGameObject != null)
                {
                    playing.SetSelectedGameObject(null);
                }

                return;
            }

            if (_creditsVisible)
            {
                _padHeld = false;
                return;
            }

            EventSystem es = EventSystem.current;
            if (es == null)
            {
                return;
            }

            Vector2 nav = GamepadInput.UiNavCombined();
            int dx = HangarPadNav.DominantStep(nav.x, nav.y, HangarPadNav.Flick);
            int dy = HangarPadNav.DominantStepY(nav.x, nav.y, HangarPadNav.Flick);
            if (dx == 0 && dy == 0)
            {
                _padHeld = false;
                FocusPrimaryIfSelectionInvalid();
                return;
            }

            DismissHangarHints();

            float now = Time.unscaledTime;
            if (_padHeld && now < _padRepeatAt)
            {
                return;
            }

            bool[] padMask = PadSelectableMask();
            GameObject selected = es.currentSelectedGameObject;
            int fromSlot = _padSlot;
            int selectedSlot = SlotFromSelected(selected);
            if (selected != null && HangarPadNav.ResolveFallback(selectedSlot, padMask) == selectedSlot)
            {
                fromSlot = selectedSlot;
            }

            _padSlot = HangarPadNav.StepSelectable(fromSlot, dx, dy, padMask);
            Button stepped = ButtonFromSlot(_padSlot);
            if (stepped != null && stepped.gameObject.activeInHierarchy && stepped.IsInteractable())
            {
                es.SetSelectedGameObject(stepped.gameObject);
            }
            else
            {
                FocusHangarSlot(HangarPadNav.PrimarySlot);
            }

            _padRepeatAt = now + (_padHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
            _padHeld = true;
        }

        private Button ButtonFromSlot(int slot)
        {
            if (slot == HangarPadNav.CreditsSlot)
            {
                return ButtonIfActive(_creditsButton);
            }

            if (slot == HangarPadNav.EasySlot)
            {
                return BezelButton(_easyBezel);
            }

            if (slot == HangarPadNav.NormalSlot)
            {
                return BezelButton(_normalBezel);
            }

            if (slot == HangarPadNav.HardSlot)
            {
                return BezelButton(_hardBezel);
            }

            if (slot == HangarPadNav.LangEnSlot)
            {
                return BezelButton(_enBezel);
            }

            if (slot == HangarPadNav.LangSvSlot)
            {
                return BezelButton(_svBezel);
            }

            if (slot == HangarPadNav.GotItSlot)
            {
                if (_tutorialRoot != null && _tutorialRoot.activeSelf)
                {
                    return _gotItButton;
                }

                return null;
            }

            if (slot == HangarPadNav.BarrageSlot)
            {
                return ButtonIfActive(_barrageButton);
            }

            if (slot == HangarPadNav.LanceSlot)
            {
                return ButtonIfActive(_lanceButton);
            }

            if (slot == HangarPadNav.HunterSlot)
            {
                return ButtonIfActive(_hunterButton);
            }

            if (slot == HangarPadNav.DoctrineHintSlot)
            {
                return ButtonIfActive(_doctrineIntroButton);
            }

            if (slot == HangarPadNav.SettingsSlot)
            {
                return ButtonIfActive(_settingsGear);
            }

            if (slot <= HangarPadNav.PrimarySlot)
            {
                return _primary;
            }

            int shopIndex;
            if (HangarPadNav.TryShopIndex(slot, out shopIndex)
                && _buyButtons != null
                && shopIndex >= 0
                && shopIndex < _buyButtons.Length)
            {
                return _buyButtons[shopIndex];
            }

            return _primary;
        }

        private static Button BezelButton(Image bezel)
        {
            if (bezel == null || !bezel.gameObject.activeInHierarchy)
            {
                return null;
            }

            return bezel.GetComponent<Button>();
        }

        private static Button ButtonIfActive(Button button)
        {
            if (button == null || !button.gameObject.activeInHierarchy)
            {
                return null;
            }

            return button;
        }

        private int SlotFromSelected(GameObject go)
        {
            if (go == null)
            {
                return HangarPadNav.PrimarySlot;
            }

            if (_primary != null && go == _primary.gameObject)
            {
                return HangarPadNav.PrimarySlot;
            }

            if (_creditsButton != null && go == _creditsButton.gameObject)
            {
                return HangarPadNav.CreditsSlot;
            }

            if (_easyBezel != null && go == _easyBezel.gameObject)
            {
                return HangarPadNav.EasySlot;
            }

            if (_normalBezel != null && go == _normalBezel.gameObject)
            {
                return HangarPadNav.NormalSlot;
            }

            if (_hardBezel != null && go == _hardBezel.gameObject)
            {
                return HangarPadNav.HardSlot;
            }

            if (_enBezel != null && go == _enBezel.gameObject)
            {
                return HangarPadNav.LangEnSlot;
            }

            if (_svBezel != null && go == _svBezel.gameObject)
            {
                return HangarPadNav.LangSvSlot;
            }

            if (_gotItButton != null && go == _gotItButton.gameObject)
            {
                return HangarPadNav.GotItSlot;
            }

            if (_barrageButton != null && go == _barrageButton.gameObject)
            {
                return HangarPadNav.BarrageSlot;
            }

            if (_lanceButton != null && go == _lanceButton.gameObject)
            {
                return HangarPadNav.LanceSlot;
            }

            if (_hunterButton != null && go == _hunterButton.gameObject)
            {
                return HangarPadNav.HunterSlot;
            }

            if (_doctrineIntroButton != null && go == _doctrineIntroButton.gameObject)
            {
                return HangarPadNav.DoctrineHintSlot;
            }

            if (_settingsGear != null && go == _settingsGear.gameObject)
            {
                return HangarPadNav.SettingsSlot;
            }

            if (_buyButtons != null)
            {
                for (int i = 0; i < _buyButtons.Length; i++)
                {
                    if (_buyButtons[i] != null && go == _buyButtons[i].gameObject)
                    {
                        return HangarPadNav.ShopSlot(i);
                    }
                }
            }

            return HangarPadNav.PrimarySlot;
        }

        private void SyncHangarPadSelection()
        {
            if (_session == null || _session.Phase == GamePhase.Playing)
            {
                _lastPadSelected = null;
                return;
            }

            EventSystem es = EventSystem.current;
            if (es == null)
            {
                return;
            }

            GameObject current = es.currentSelectedGameObject;
            if (SelectionNeedsPrimaryFallback(current))
            {
                Button pick = DefaultHangarButton();
                if (pick != null)
                {
                    es.SetSelectedGameObject(pick.gameObject);
                    current = pick.gameObject;
                    if (!_creditsVisible)
                    {
                        _padSlot = HangarPadNav.PrimarySlot;
                    }
                }
            }

            if (current == _lastPadSelected)
            {
                ApplyPadFocus(current);
                return;
            }

            ApplyPadFocus(current);
            _lastPadSelected = current;
            UpgradeId id;
            if (TryShopUpgradeFrom(current, out id))
            {
                ShopItem item = HangarShop.FindItem(id);
                if (item != null)
                {
                    OnShopHover(item);
                }
            }
            else if (_hoveredItem != null)
            {
                OnShopHoverExit(_hoveredItem);
            }
        }

        private Button DefaultHangarButton()
        {
            if (_creditsVisible && _creditsContinue != null)
            {
                return _creditsContinue;
            }

            return _primary;
        }

        private void ApplyPadFocus(GameObject current)
        {
            ApplyPadFocusOne(_lastPadSelected, false);
            ApplyPadFocusOne(current, true);
            RefreshLanguageChrome();
            RefreshDifficultyChrome();
        }

        private void ApplyPadFocusOne(GameObject go, bool focused)
        {
            if (go == null)
            {
                return;
            }

            bool shop = go.name.StartsWith("Buy_");
            UiTheme.SetPadFocus(go, focused, shop);
        }

        private static bool TryShopUpgradeFrom(GameObject go, out UpgradeId id)
        {
            id = UpgradeId.RapidFire;
            if (go == null || !go.name.StartsWith("Buy_", System.StringComparison.Ordinal))
            {
                return false;
            }

            return System.Enum.TryParse(go.name.Substring(4), out id);
        }

        private void RefreshWorldBadge()
        {
            if (_world == null || _session == null || Time.unscaledTime < _worldFlashUntil)
            {
                return;
            }

            int world = ContentFactory.WorldIndexForWave(_session.WaveIndex);
            _world.text = Loc.Tf(
                "ui.world_badge",
                "WORLD {0}  ·  {1}",
                world,
                ArenaLayout.Badge(ArenaLayout.ForWorld(world)));
            _world.color = UiTheme.Primary;
        }

        private void RefreshBadgeRow(bool playing)
        {
            if (_badgeRow == null)
            {
                return;
            }

            HangarPersist persist = _game != null && _game.Persist != null ? _game.Persist : HangarPersist.Load();
            string row = persist != null ? persist.LadderLine() : MedalCatalog.LadderLine(0);
            bool show = !string.IsNullOrEmpty(row);
            _badgeRow.gameObject.SetActive(show);
            if (show)
            {
                _badgeRow.text = Loc.T("ui.medals", MedalLadderPrefix) + "  " + row;
                _badgeRow.fontSize = playing ? 12 : 14;
                _badgeRow.color = playing
                    ? UiTheme.WithAlpha(UiTheme.Primary, 0.92f)
                    : UiTheme.Primary;
                ClampOneLine(_badgeRow);
            }
        }

        private void RefreshAchievementLadder(bool playing)
        {
            if (_achievementLadder == null)
            {
                return;
            }

            AchievementPersist persist = _game != null ? _game.Achievements : null;
            string count = persist != null
                ? persist.CompactCount()
                : AchievementCatalog.CompactCount(0);
            _achievementLadder.gameObject.SetActive(!playing);
            if (!playing)
            {
                string header = Loc.T("ach.header", "ACHIEVEMENTS");
                _achievementLadder.text = header + "\n" + count;
                _achievementLadder.horizontalOverflow = HorizontalWrapMode.Wrap;
                _achievementLadder.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }

        private void RefreshBuyButton(int index, ShopItem item)
        {
            bool doctrineRow = item.Group == ShopGroup.Doctrine;
            bool showRow = !doctrineRow || (_loadout != null && _loadout.State != null && _loadout.State.ShowDoctrineShopItem(item.Id));
            if (_buyButtons[index] != null)
            {
                _buyButtons[index].gameObject.SetActive(showRow);
            }

            if (!showRow)
            {
                return;
            }

            int price = _loadout.State.EffectiveCost(item);
            bool owned = _loadout.State.Owns(item.Id);
            bool canApply = _loadout.State.CanApply(item.Id);
            bool locked = !owned && !canApply;
            bool tooPoor = !owned && canApply && _session.Credits < price;
            bool weapon = WeaponSlots.IsWeapon(item.Id);
            bool equipped = owned && weapon && _loadout.State.IsEquipped(item.Id);
            _buyButtons[index].interactable = _session.ShopOpen
                && ((owned && weapon) || (!owned && !locked && !tooPoor));

            bool offPath = _loadout.State.IsOffPath(item.Id);
            Image plate = _buyButtons[index].targetGraphic as Image;
            UiTheme.PaintShopPlate(plate, _buyLabels[index], owned, locked, tooPoor);
            if (equipped && plate != null)
            {
                plate.color = UiTheme.WithAlpha(UiTheme.Primary, 0.34f);
            }
            if (equipped && _buyLabels[index] != null)
            {
                _buyLabels[index].color = UiTheme.Primary;
            }
            if (offPath)
            {
                UiTheme.PaintOffPathCue(plate, _buyLabels[index]);
            }
            EventSystem es = EventSystem.current;
            bool focused = es != null
                && es.currentSelectedGameObject != null
                && es.currentSelectedGameObject == _buyButtons[index].gameObject;
            UiTheme.SetPadFocus(_buyButtons[index].gameObject, focused, true);

            string costLine;
            if (owned)
            {
                costLine = Loc.T("ui.owned", "OWNED") + UiTheme.OwnedCheck;
            }
            else if (locked)
            {
                costLine = Loc.T("ui.locked", "LOCKED");
            }
            else if (tooPoor)
            {
                costLine = Loc.Tf("ui.need_cr", "need {0} cr", price);
            }
            else
            {
                costLine = Loc.Tf("ui.cost_cr", "{0} cr", price);
            }

            if (offPath)
            {
                costLine = Loc.T("ui.off_path", "off-path") + "  ·  " + costLine;
            }

            _buyLabels[index].text = item.Title + "\n" + costLine;
        }

        private string BestCardLine()
        {
            LocalBest best = _game != null && _game.Best != null ? _game.Best : LocalBest.Load();
            LocalBest session = _game != null ? _game.SessionBest : null;
            string line = session != null ? session.SessionCardLine() : Loc.T("session.empty", "Session —");
            line += "  ·  " + best.CardLine();
            if (_game != null && _game.LastRunWasNewBest)
            {
                return line + Loc.T("ui.new_best_dot", "  ·  NEW BEST");
            }

            return line;
        }

        private string FailReasonText()
        {
            if (_session == null)
            {
                return Loc.T("fail.unknown", "Unknown cause");
            }

            if (_session.HasStructuredFail)
            {
                return _session.FailCause == DamageCause.EnemyContact
                    ? DamageCauseText.FailReason(_session.FailCause, _session.FailEnemyKind)
                    : DamageCauseText.FailReason(_session.FailCause);
            }

            if (string.IsNullOrEmpty(_session.FailReason))
            {
                return Loc.T("fail.unknown", "Unknown cause");
            }

            return _session.FailReason;
        }

        private string BuildHud(bool playing)
        {
            int hull = _ship != null && _ship.Health != null ? _ship.Health.Hull : LoadoutState.HullHitPoints;
            int shield = _ship != null && _ship.Health != null ? _ship.Health.Shield : _loadout.State.ShieldCharges;
            string remaining = playing && _waves != null
                ? Loc.Tf("ui.hud_remaining", "   ·   Remaining {0}", _waves.RemainingThreats)
                : string.Empty;
            string fireMode = string.Empty;
            if (playing && _ship != null && _ship.Shooter != null)
            {
                string primary = Loc.FireModeName(_ship.Shooter.Mode);
                string utility = _ship.Shooter.HasUtility
                    ? Loc.FireModeName(_ship.Shooter.UtilityMode)
                    : Loc.T("ui.hud_dash", "—");
                fireMode = Loc.Tf("ui.hud_primary", "\nPRIMARY {0}", primary)
                    + Loc.Tf("ui.hud_utility", "\nUTILITY {0}", utility);
                if (!_ship.Shooter.HasUtility)
                {
                    fireMode += " " + Loc.T("ui.hud_empty", "empty");
                }

                if (_loadout != null && _loadout.State != null && _loadout.State.HasAltFire)
                {
                    fireMode += string.Empty;
                }
            }

            string scoreLine = Loc.Tf(
                "ui.hud_wave_score",
                "Wave {0}   ·   Score {1}",
                _session.WaveIndex,
                _session.Score);
            if (playing)
            {
                scoreLine += PlayBestCompare();
                LocalBest session = _game != null ? _game.SessionBest : null;
                if (session != null && session.HasRecord)
                {
                    scoreLine += Loc.Tf("session.slash", " / Sess {0}", session.Score);
                }
            }

            int lives = _session != null ? _session.Lives : DifficultySettings.StartLives;
            int maxLives = _session != null ? _session.MaxLives : DifficultySettings.MaxLives;
            string livesLine = "\n" + Loc.Tf("ui.hud_lives", "Lives {0} / {1}", lives, maxLives);
            string hangarBest = playing ? string.Empty : "\n" + BestCardLine();
            return scoreLine
                + "\n" + Loc.Tf("ui.hud_hull", "Hull {0}   ·   Shield {1}", hull, shield)
                + livesLine
                + remaining
                + fireMode
                + hangarBest;
        }

        private string PlayBestCompare()
        {
            LocalBest best = _game != null && _game.Best != null ? _game.Best : LocalBest.Load();
            if (best == null)
            {
                return string.Empty;
            }

            int world = ContentFactory.WorldIndexForWave(_session.WaveIndex);
            return best.PlayCompare(_session.Score, _session.WaveIndex, world);
        }

        private void RefreshLoadoutSlots()
        {
            if (_loadoutSlots == null)
            {
                return;
            }

            LoadoutState loadout = _loadout != null ? _loadout.State : null;
            string primary = Loc.FireModeName(loadout != null ? loadout.ResolvedPrimary() : FireMode.Bolt);
            string utility = loadout != null && loadout.HasUtility
                ? Loc.FireModeName(loadout.UtilityMode)
                : Loc.T("ui.hud_dash", "—");
            _loadoutSlots.text = Loc.Tf(
                "ui.loadout_slots",
                "P {0}  ·  U {1}",
                primary,
                utility);
            ClampOneLine(_loadoutSlots);
        }

        private void BuildUtilityHud(Font display, Font body)
        {
            // Compact utility plate in the HUD gutter between HEALTH (max.y 0.305) and HudPlate (min.y 0.555).
            _utilityHud = UiTheme.BuildPanel(
                "UtilityHud",
                transform,
                new Vector2(0.012f, 0.325f),
                new Vector2(0.28f, 0.445f),
                0.78f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.HudPlate);

            _utilityHudLabel = CreateText("UtilityHudLabel", _utilityHud.transform, display, 13, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_utilityHudLabel.rectTransform, new Vector2(0.28f, 0.72f), new Vector2(0.96f, 0.96f));
            _utilityHudLabel.color = UiAmber;
            ClampOneLine(_utilityHudLabel);
            AddReadability(_utilityHudLabel, true);

            _utilityHudName = CreateText("UtilityHudName", _utilityHud.transform, body, 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_utilityHudName.rectTransform, new Vector2(0.28f, 0.08f), new Vector2(0.96f, 0.48f));
            _utilityHudName.color = UiBody;
            ClampOneLine(_utilityHudName);
            AddReadability(_utilityHudName, false);

            GameObject icon = CreateFill(
                "UtilityIcon",
                _utilityHud.transform,
                UiTheme.WithAlpha(UiTheme.Secondary, 0.35f),
                new Vector2(0.05f, 0.14f),
                new Vector2(0.24f, 0.70f));
            GameObject ringGo = new GameObject("UtilityRing");
            ringGo.transform.SetParent(icon.transform, false);
            _utilityRing = ringGo.AddComponent<Image>();
            _utilityRing.sprite = BarFillSprite();
            _utilityRing.color = UiTheme.Secondary;
            _utilityRing.raycastTarget = false;
            _utilityRing.type = Image.Type.Filled;
            _utilityRing.fillMethod = Image.FillMethod.Radial360;
            _utilityRing.fillOrigin = (int)Image.Origin360.Top;
            _utilityRing.fillClockwise = true;
            Stretch(_utilityRing.rectTransform, Vector2.zero, Vector2.one);
            _utilityHud.SetActive(false);
        }

        private void RefreshUtilityHud(bool playing)
        {
            if (_utilityHud == null)
            {
                return;
            }

            _utilityHud.SetActive(playing);
            if (!playing)
            {
                return;
            }

            ShipShooter shooter = _ship != null ? _ship.Shooter : null;
            bool has = shooter != null && shooter.HasUtility;
            if (_utilityHudLabel != null)
            {
                _utilityHudLabel.text = Loc.T("ui.slot_utility", "UTILITY");
            }

            if (_utilityHudName != null)
            {
                if (!has)
                {
                    _utilityHudName.text = Loc.T("ui.hud_dash", "—")
                        + "  "
                        + Loc.T("ui.hud_empty", "empty");
                    _utilityHudName.color = UiTheme.WithAlpha(UiTheme.Accent, 0.7f);
                }
                else
                {
                    _utilityHudName.text = Loc.FireModeName(shooter.UtilityMode);
                    _utilityHudName.color = UiTheme.Accent;
                }
            }

            if (_utilityRing != null)
            {
                _utilityRing.fillAmount = has && shooter != null ? shooter.UtilityCooldown01 : 0f;
                _utilityRing.color = has ? UiTheme.Secondary : UiTheme.Disabled;
            }
        }

        private void BuildHealthRack(Font display, Font body)
        {
            _healthRoot = UiTheme.BuildPanel(
                "HealthRack",
                transform,
                new Vector2(0.012f, 0.105f),
                new Vector2(0.38f, 0.305f),
                0.82f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.HudPlate);

            _healthTitle = CreateText("HealthTitle", _healthRoot.transform, display, 15, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_healthTitle.rectTransform, new Vector2(0.07f, 0.8f), new Vector2(0.48f, 0.98f));
            _healthTitle.color = UiAmber;
            AddReadability(_healthTitle, true);

            _livesHud = CreateText("LivesHud", _healthRoot.transform, display, 13, TextAnchor.MiddleRight, FontStyle.Bold);
            Stretch(_livesHud.rectTransform, new Vector2(0.48f, 0.8f), new Vector2(0.95f, 0.98f));
            _livesHud.color = UiAmber;
            AddReadability(_livesHud, true);

            _shieldBarRow = CreateBarRow(
                "ShieldBar",
                _healthRoot.transform,
                display,
                body,
                new Vector2(0.05f, 0.42f),
                new Vector2(0.95f, 0.76f),
                UiShield,
                out _shieldBarLabel,
                out _shieldBarCount,
                out _shieldFill);
            CreateBarRow(
                "HullBar",
                _healthRoot.transform,
                display,
                body,
                new Vector2(0.05f, 0.04f),
                new Vector2(0.95f, 0.38f),
                UiHull,
                out _hullBarLabel,
                out _hullBarCount,
                out _hullFill);
            _healthRoot.SetActive(false);
        }

        private GameObject BuildPlayVignette()
        {
            GameObject go = CreateFill("PlayVignette", transform, Color.white, Vector2.zero, Vector2.one);
            Image image = go.GetComponent<Image>();
            image.sprite = MakeVignetteSprite();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            go.SetActive(false);
            go.transform.SetSiblingIndex(1);
            return go;
        }

        private static Sprite MakeVignetteSprite()
        {
            const int Size = 64;
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(0.5f, 0.5f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float nx = (x + 0.5f) / Size;
                    float ny = (y + 0.5f) / Size;
                    float d = Vector2.Distance(new Vector2(nx, ny), center) * 2f;
                    float a = Mathf.Clamp01((d - 0.55f) / 0.7f) * 0.35f;
                    tex.SetPixel(x, y, new Color(0f, 0f, 0.04f, a));
                }
            }

            tex.Apply();
            tex.name = "PlayVignette";
            return Sprite.Create(tex, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 64f);
        }

        private GameObject CreateBarRow(
            string name,
            Transform parent,
            Font display,
            Font body,
            Vector2 min,
            Vector2 max,
            Color fillColor,
            out Text label,
            out Text count,
            out Image fill)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            Stretch(row.AddComponent<RectTransform>(), min, max);

            label = CreateText(name + "Label", row.transform, display, 13, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(label.rectTransform, new Vector2(0f, 0.64f), new Vector2(0.58f, 1f));
            label.color = UiAmber;

            count = CreateText(name + "Count", row.transform, body, 14, TextAnchor.MiddleRight, FontStyle.Bold);
            Stretch(count.rectTransform, new Vector2(0.5f, 0.64f), new Vector2(1f, 1f));
            count.color = UiBody;

            CreateFill(name + "Bezel", row.transform, UiTheme.WithAlpha(UiTheme.Accent, 0.35f),
                new Vector2(0f, 0.02f), new Vector2(1f, 0.6f));
            GameObject track = CreateFill(name + "Track", row.transform, UiTheme.WithAlpha(UiTheme.Void, 0.96f),
                new Vector2(0.012f, 0.07f), new Vector2(0.988f, 0.55f));
            GameObject fillGo = new GameObject(name + "Fill");
            fillGo.transform.SetParent(track.transform, false);
            fill = fillGo.AddComponent<Image>();
            fill.sprite = BarFillSprite();
            fill.color = fillColor;
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            Stretch(fill.rectTransform, Vector2.zero, Vector2.one);
            return row;
        }

        private static Sprite BarFillSprite()
        {
            if (_barFillSprite != null)
            {
                return _barFillSprite;
            }

            Texture2D tex = new Texture2D(8, 8, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }

            tex.Apply();
            tex.name = "HealthBarFill";
            _barFillSprite = Sprite.Create(tex, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
            return _barFillSprite;
        }

        private void RefreshHealthBar()
        {
            if (_healthRoot == null)
            {
                return;
            }

            bool playing = _session != null && _session.Phase == GamePhase.Playing;
            bool failed = _session != null && _session.Phase == GamePhase.Failed;
            bool show = playing || failed;
            _healthRoot.SetActive(show);
            LayoutHealthRack(failed);
            if (!show)
            {
                return;
            }

            int hull = _ship != null && _ship.Health != null ? _ship.Health.Hull : LoadoutState.HullHitPoints;
            int maxHull = _ship != null && _ship.Health != null ? _ship.Health.MaxHull : LoadoutState.HullHitPoints;
            int shield = _ship != null && _ship.Health != null ? _ship.Health.Shield : 0;
            int maxShield = _ship != null && _ship.Health != null
                ? Mathf.Max(_ship.Health.MaxShield, _loadout != null && _loadout.State != null ? _loadout.State.CurrentMaxShield : 0)
                : (_loadout != null && _loadout.State != null ? _loadout.State.CurrentMaxShield : LoadoutState.MaxShieldCharges);
            if (maxHull < 1)
            {
                maxHull = LoadoutState.HullHitPoints;
            }

            if (_hullFill != null)
            {
                _hullFill.fillAmount = Mathf.Clamp01(hull / (float)maxHull);
                _hullFill.color = hull <= 1
                    ? UiTheme.Danger
                    : UiHull;
            }

            if (_hullBarCount != null)
            {
                _hullBarCount.text = hull + " / " + maxHull;
            }

            if (_shieldBarRow != null)
            {
                _shieldBarRow.SetActive(true);
            }

            if (_shieldFill != null)
            {
                int cap = Mathf.Max(1, maxShield);
                _shieldFill.fillAmount = maxShield > 0 ? Mathf.Clamp01(shield / (float)cap) : 0f;
            }

            if (_shieldBarCount != null)
            {
                _shieldBarCount.text = shield + " / " + maxShield;
            }

            if (_livesHud != null)
            {
                int lives = _session != null ? _session.Lives : 0;
                int cap = _session != null ? _session.MaxLives : DifficultySettings.MaxLives;
                _livesHud.text = LivesPips(lives, cap);
            }
        }

        private static string LivesPips(int lives, int cap)
        {
            if (cap < 1)
            {
                cap = DifficultySettings.MaxLives;
            }

            if (lives < 0)
            {
                lives = 0;
            }

            if (lives > cap)
            {
                lives = cap;
            }

            return Loc.T("ui.lives", "LIVES") + "  " + new string('●', lives) + new string('○', cap - lives);
        }

        private void LayoutHealthRack(bool failed)
        {
            if (_healthRoot == null)
            {
                return;
            }

            RectTransform rt = _healthRoot.GetComponent<RectTransform>();
            if (failed)
            {
                Stretch(rt, new Vector2(0.008f, 0.33f), new Vector2(0.178f, 0.62f));
                _healthRoot.transform.SetAsLastSibling();
                return;
            }

            Stretch(rt, new Vector2(0.012f, 0.105f), new Vector2(0.38f, 0.305f));
        }

        private static void AddReadability(Text text, bool strong)
        {
            if (text == null)
            {
                return;
            }

            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = UiTheme.WithAlpha(UiTheme.Void, strong ? 0.92f : 0.78f);
            outline.effectDistance = strong ? new Vector2(1.35f, -1.35f) : new Vector2(1f, -1f);
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, strong ? 0.55f : 0.4f);
            shadow.effectDistance = new Vector2(2f, -2f);
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

        private void BuildDoctrine(Font display, Font body)
        {
            _doctrineRoot = UiTheme.BuildPanel(
                "DoctrineCard",
                transform,
                DoctrinePanelMin,
                DoctrinePanelMax,
                0.875f);
            _doctrineRoot.SetActive(false);

            _doctrineTitle = CreateText("DoctrineTitle", _doctrineRoot.transform, display, UiTheme.DoctrineBadge, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_doctrineTitle.rectTransform, new Vector2(0.04f, 0.879f), new Vector2(0.96f, 0.983f));
            _doctrineTitle.color = UiTheme.Primary;
            ClampOneLine(_doctrineTitle);

            _barrageButton = CreateButton("DoctrineBarrage", _doctrineRoot.transform, body, new Vector2(0.008f, 0.414f), new Vector2(0.331f, 0.862f));
            _lanceButton = CreateButton("DoctrineLance", _doctrineRoot.transform, body, new Vector2(0.339f, 0.414f), new Vector2(0.662f, 0.862f));
            _hunterButton = CreateButton("DoctrineHunter", _doctrineRoot.transform, body, new Vector2(0.670f, 0.414f), new Vector2(0.993f, 0.862f));
            _barrageLabel = _barrageButton.GetComponentInChildren<Text>();
            _lanceLabel = _lanceButton.GetComponentInChildren<Text>();
            _hunterLabel = _hunterButton.GetComponentInChildren<Text>();
            FitDoctrineLabel(_barrageLabel);
            FitDoctrineLabel(_lanceLabel);
            FitDoctrineLabel(_hunterLabel);
            _barrageButton.onClick.AddListener(() => OnPickDoctrine(DoctrineId.Barrage));
            _lanceButton.onClick.AddListener(() => OnPickDoctrine(DoctrineId.Lance));
            _hunterButton.onClick.AddListener(() => OnPickDoctrine(DoctrineId.Hunter));
            UiTheme.ApplyButton(_barrageButton, false, false, true);
            UiTheme.ApplyButton(_lanceButton, false, false, true);
            UiTheme.ApplyButton(_hunterButton, false, false, true);

            _doctrineShopRow = CreateFill("DoctrineShopRow", _doctrineRoot.transform, UiTheme.InnerWash, new Vector2(0.012f, 0.190f), new Vector2(0.988f, 0.397f));
            _doctrineHint = CreateText("DoctrineHint", _doctrineRoot.transform, body, UiTheme.BodyMin, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_doctrineHint.rectTransform, new Vector2(0.04f, 0.017f), new Vector2(0.96f, 0.172f));
            _doctrineHint.color = UiTheme.Secondary;
            _doctrineHint.horizontalOverflow = HorizontalWrapMode.Wrap;
            _doctrineHint.verticalOverflow = VerticalWrapMode.Truncate;
            _doctrineHint.text = Loc.T("ui.hint_dual", HintDual);

            _doctrineIntro = CreateFill(
                "DoctrineIntro",
                _doctrineRoot.transform,
                UiTheme.WithAlpha(UiTheme.Surface2, 0.92f),
                new Vector2(0.012f, 0.017f),
                new Vector2(0.988f, 0.172f));
            _doctrineIntroBody = CreateText(
                "DoctrineIntroBody",
                _doctrineIntro.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleLeft,
                FontStyle.Bold);
            Stretch(_doctrineIntroBody.rectTransform, new Vector2(0.03f, 0f), new Vector2(0.72f, 1f));
            _doctrineIntroBody.color = UiTheme.Accent;
            _doctrineIntroBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _doctrineIntroBody.verticalOverflow = VerticalWrapMode.Truncate;
            _doctrineIntroBody.text = DoctrineHintBody;
            _doctrineIntroButton = CreateButton(
                "DoctrineIntroDismiss",
                _doctrineIntro.transform,
                body,
                new Vector2(0.74f, 0.14f),
                new Vector2(0.98f, 0.86f));
            _doctrineIntroGotIt = _doctrineIntroButton.GetComponentInChildren<Text>();
            _doctrineIntroGotIt.fontSize = UiTheme.BodyMin;
            _doctrineIntroGotIt.text = "Got it";
            ClampOneLine(_doctrineIntroGotIt);
            _doctrineIntroButton.onClick.AddListener(OnDismissDoctrineIntroClicked);
            UiTheme.ApplyButton(_doctrineIntroButton, true, false, false);
            _doctrineIntro.SetActive(false);

            _doctrineBadge = CreateText("DoctrineBadge", transform, display, UiTheme.DoctrineBadge, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_doctrineBadge.rectTransform, new Vector2(0.012f, 0.452f), new Vector2(0.30f, 0.540f));
            _doctrineBadge.color = UiTheme.Primary;
            ClampOneLine(_doctrineBadge);
            AddReadability(_doctrineBadge, true);
            _doctrineBadge.gameObject.SetActive(false);
        }

        private void OnPickDoctrine(DoctrineId id)
        {
            if (_shop == null || _loadout == null || _loadout.State == null || _session == null)
            {
                return;
            }

            LoadoutState picked = _loadout.State;
            bool chosenCard = picked.Doctrine == id;
            bool otherCard = picked.Doctrine != DoctrineId.None && !chosenCard;
            bool gateMet = picked.GateMet(id);
            int pickCost = picked.DoctrinePickCost(id);
            bool cannotAfford = !chosenCard && !otherCard && gateMet && pickCost > 0 && _session.Credits < pickCost;
            if (!DoctrineRules.CardIsAction(_session.ShopOpen, chosenCard, otherCard, gateMet, cannotAfford))
            {
                return;
            }

            _shop.TryPickDoctrine(id);
        }

        private void RefreshDoctrinePicks()
        {
            if (_doctrineTitle == null || _loadout == null || _loadout.State == null || _session == null)
            {
                return;
            }

            LoadoutState state = _loadout.State;
            if (state.Doctrine == DoctrineId.None)
            {
                _doctrineTitle.text = Loc.T("ui.doctrine.tip", "Choose a doctrine.");
            }
            else
            {
                _doctrineTitle.text = Loc.Tf(
                    "ui.hud_doctrine",
                    "DOCTRINE  ·  {0}",
                    DoctrineLabel(state.Doctrine));
            }

            if (_doctrineHint != null)
            {
                _doctrineHint.text = state.Rail
                    ? Loc.T("ui.hint_rail", HintRail)
                    : Loc.T("ui.hint_dual", HintDual);
            }

            PaintDoctrineButton(_barrageButton, _barrageLabel, DoctrineId.Barrage, state);
            PaintDoctrineButton(_lanceButton, _lanceLabel, DoctrineId.Lance, state);
            PaintDoctrineButton(_hunterButton, _hunterLabel, DoctrineId.Hunter, state);
        }

        private void PaintDoctrineButton(Button button, Text label, DoctrineId id, LoadoutState state)
        {
            if (button == null || label == null)
            {
                return;
            }

            bool chosen = state.Doctrine == id;
            bool other = state.Doctrine != DoctrineId.None && !chosen;
            bool gate = state.GateMet(id);
            int cost = state.DoctrinePickCost(id);
            bool tooPoor = !chosen && !other && gate && cost > 0 && _session.Credits < cost;
            bool locked = other || !gate;
            bool action = DoctrineRules.CardIsAction(_session.ShopOpen, chosen, other, gate, tooPoor);
            button.interactable = action;
            Image plate = button.targetGraphic as Image;
            if (plate != null)
            {
                plate.raycastTarget = action;
            }
            UiTheme.PaintShopPlate(plate, label, chosen, locked, tooPoor);
            label.text = DoctrineLabel(id) + "\n" + DoctrineBlurb(id) + "\n" + DoctrineStatusLine(id, gate, cost, chosen, other, tooPoor);
        }

        private static void FitDoctrineLabel(Text label)
        {
            if (label == null)
            {
                return;
            }

            label.fontSize = UiTheme.BodyMin;
            label.alignment = TextAnchor.UpperCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            Stretch(label.rectTransform, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.95f));
        }

        private static string DoctrineBlurb(DoctrineId id)
        {
            switch (id)
            {
                case DoctrineId.Barrage:
                    return Loc.T("ui.doctrine.path.barrage", "Wide shots\nSpread>Flak>Storm");
                case DoctrineId.Lance:
                    return Loc.T("ui.doctrine.path.lance", "Heavy line\nTwin>Rail>Lance");
                case DoctrineId.Hunter:
                    return Loc.T("ui.doctrine.path.hunter", "Homing\nSeek>Cadence>Twin");
                default:
                    return string.Empty;
            }
        }

        private static string DoctrineStatusLine(
            DoctrineId id,
            bool gate,
            int cost,
            bool chosen,
            bool other,
            bool tooPoor)
        {
            if (chosen)
            {
                return Loc.T("ui.owned", "OWNED") + UiTheme.OwnedCheck;
            }

            if (other)
            {
                return Loc.T("ui.doctrine.swap", "New Run to swap");
            }

            if (!gate)
            {
                return DoctrineGateNeed(id);
            }

            if (tooPoor)
            {
                return Loc.Tf("ui.need_cr", "need {0} cr", cost);
            }

            if (cost > 0)
            {
                return Loc.Tf("ui.cost_cr", "{0} cr", cost);
            }

            return Loc.T("ui.doctrine.pick", "Choose");
        }

        private static string DoctrineGateNeed(DoctrineId id)
        {
            switch (id)
            {
                case DoctrineId.Barrage:
                    return Loc.Tf("ui.doctrine.need", "Needs {0}", Loc.T("up.Spread", "Spread"));
                case DoctrineId.Lance:
                    return Loc.Tf(
                        "ui.doctrine.need_either",
                        "Needs {0}/{1}",
                        Loc.T("up.Pierce", "Pierce"),
                        Loc.T("up.Twin", "Twin"));
                case DoctrineId.Hunter:
                    return Loc.Tf("ui.doctrine.need", "Needs {0}", Loc.T("up.Seeker", "Seeker"));
                default:
                    return Loc.T("ui.doctrine.none", "—");
            }
        }

        private static string DoctrineLabel(DoctrineId id)
        {
            switch (id)
            {
                case DoctrineId.Barrage:
                    return Loc.T("ui.doctrine.barrage", "Barrage");
                case DoctrineId.Lance:
                    return Loc.T("ui.doctrine.lance", "Lance");
                case DoctrineId.Hunter:
                    return Loc.T("ui.doctrine.hunter", "Hunter");
                default:
                    return Loc.T("ui.doctrine.none", "—");
            }
        }

        private void RefreshDoctrineBadge(bool playing)
        {
            if (_doctrineBadge == null)
            {
                return;
            }

            LoadoutState state = _loadout != null ? _loadout.State : null;
            bool show = playing && state != null && state.Doctrine != DoctrineId.None;
            _doctrineBadge.gameObject.SetActive(show);
            if (!show)
            {
                return;
            }

            _doctrineBadge.fontSize = UiTheme.DoctrineBadge;
            _doctrineBadge.text = Loc.Tf(
                "ui.hud_doctrine",
                "DOCTRINE  ·  {0}",
                DoctrineLabel(state.Doctrine));
            _doctrineBadge.color = UiTheme.Primary;
            ClampOneLine(_doctrineBadge);
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
            image.color = UiTheme.Surface2;
            Button button = go.AddComponent<Button>();
            UiTheme.ApplyButton(button, false, false, false);
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
            background.color = UiTheme.Surface2;
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
            fillImage.color = UiTheme.Primary;
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
            Navigation nav = slider.navigation;
            nav.mode = Navigation.Mode.None;
            slider.navigation = nav;
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        private void ApplyFooterHintSize()
        {
            int footerSize = UiTheme.HintSize(Screen.width);
            if (_hint != null)
            {
                _hint.fontSize = footerSize;
            }

            if (_firstFlightBody != null)
            {
                _firstFlightBody.fontSize = footerSize;
            }

            if (_settingsControlsBody != null)
            {
                _settingsControlsBody.fontSize = footerSize;
            }
        }

        private static void ClampOneLine(Text text)
        {
            if (text == null)
            {
                return;
            }

            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void BuildSettingsGear(Font font)
        {
            _settingsGear = CreateButton("SettingsGear", transform, font, SettingsGearMin, SettingsGearMax);
            _settingsGearLabel = _settingsGear.GetComponentInChildren<Text>();
            _settingsGearLabel.fontSize = UiTheme.BodyMin;
            _settingsGearLabel.resizeTextForBestFit = true;
            _settingsGearLabel.resizeTextMinSize = 10;
            _settingsGearLabel.resizeTextMaxSize = UiTheme.BodyMin;
            _settingsGearLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _settingsGearLabel.verticalOverflow = VerticalWrapMode.Truncate;
            _settingsGear.onClick.AddListener(OpenSettings);
            UiTheme.ApplyButton(_settingsGear, false, false, false);
        }

        private void BuildSettingsPanel(Font display, Font body)
        {
            _settingsRoot = new GameObject("SettingsRoot");
            _settingsRoot.transform.SetParent(transform, false);
            Stretch(_settingsRoot.AddComponent<RectTransform>(), Vector2.zero, Vector2.one);

            GameObject scrim = CreateFill(
                "SettingsScrim",
                _settingsRoot.transform,
                UiTheme.WithAlpha(UiTheme.Void, 0.78f),
                Vector2.zero,
                Vector2.one);
            Image scrimImage = scrim.GetComponent<Image>();
            if (scrimImage != null)
            {
                scrimImage.raycastTarget = true;
            }

            Button scrimButton = scrim.AddComponent<Button>();
            scrimButton.transition = Selectable.Transition.None;
            scrimButton.onClick.AddListener(CloseSettingsFromScrim);
            Navigation scrimNav = scrimButton.navigation;
            scrimNav.mode = Navigation.Mode.None;
            scrimButton.navigation = scrimNav;

            _settingsPanel = UiTheme.BuildPanel(
                "SettingsPanel",
                _settingsRoot.transform,
                SettingsPanelMin,
                SettingsPanelMax,
                0.90f);
            Image panelImage = _settingsPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.raycastTarget = true;
            }

            _settingsTitle = CreateText(
                "SettingsTitle",
                _settingsPanel.transform,
                display,
                UiTheme.HeaderMin,
                TextAnchor.MiddleCenter,
                FontStyle.Bold);
            Stretch(_settingsTitle.rectTransform, new Vector2(0.06f, 0.905f), new Vector2(0.94f, 0.985f));
            _settingsTitle.color = UiTheme.Primary;
            AddReadability(_settingsTitle, true);

            int rowCount = SettingsRows.Count;
            _settingsRowButtons = new Button[rowCount];
            float bandY0;
            float bandY1;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                SettingsRows.RowBand(rowIndex, out bandY0, out bandY1);
                BuildSettingsRow(SettingsRows.Order[rowIndex], rowIndex, display, body, bandY0, bandY1);
            }

            _settingsRoot.SetActive(false);
        }

        private void BuildSettingsRow(SettingsRowId rowId, int rowIndex, Font display, Font body, float y0, float y1)
        {
            if (rowId == SettingsRowId.Language)
            {
                BuildSettingsLanguageRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.Music)
            {
                BuildSettingsVolumeRow(rowIndex, "SettingsMusic", body, y0, y1, true);
                return;
            }

            if (rowId == SettingsRowId.Sfx)
            {
                BuildSettingsVolumeRow(rowIndex, "SettingsSfx", body, y0, y1, false);
                return;
            }

            if (rowId == SettingsRowId.Mute)
            {
                BuildSettingsMuteRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.ScreenShake)
            {
                BuildSettingsShakeRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.Controls)
            {
                BuildSettingsControlsRow(display, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.Close)
            {
                BuildSettingsCloseRow(rowIndex, display, y0, y1);
            }
        }

        private void BuildSettingsVolumeRow(int rowIndex, string rowName, Font body, float y0, float y1, bool music)
        {
            Button row = CreateButton(
                rowName,
                _settingsPanel.transform,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            UiTheme.ApplyButton(row, false, false, false);

            Text caption = row.GetComponentInChildren<Text>();
            caption.fontSize = UiTheme.BodyMin;
            caption.alignment = TextAnchor.MiddleLeft;
            caption.fontStyle = FontStyle.Bold;
            caption.color = UiTheme.Accent;
            caption.raycastTarget = false;
            Stretch(caption.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.36f, 0.92f));

            float initial = AudioCues.DefaultSfxVolume;
            if (music)
            {
                initial = AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : AudioCues.DefaultMusicVolume;
            }
            else if (AudioCues.Instance != null)
            {
                initial = AudioCues.Instance.SfxVolume;
            }

            Slider slider;
            if (music)
            {
                slider = CreateSlider(
                    "SettingsMusicSlider",
                    row.transform,
                    new Vector2(SettingsMeasure.SliderMinX, 0.18f),
                    new Vector2(SettingsMeasure.SliderMaxX, 0.82f),
                    initial,
                    OnSettingsMusicVolume);
            }
            else
            {
                slider = CreateSlider(
                    "SettingsSfxSlider",
                    row.transform,
                    new Vector2(SettingsMeasure.SliderMinX, 0.18f),
                    new Vector2(SettingsMeasure.SliderMaxX, 0.82f),
                    initial,
                    OnSettingsSfxVolume);
            }

            if (music)
            {
                _settingsMusicLabel = caption;
                _settingsMusicSlider = slider;
            }
            else
            {
                _settingsSfxLabel = caption;
                _settingsSfxSlider = slider;
            }
        }

        private void BuildSettingsMuteRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsMute",
                _settingsPanel.transform,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            row.onClick.AddListener(ToggleSettingsMute);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsMuteLabel = row.GetComponentInChildren<Text>();
            _settingsMuteLabel.fontSize = UiTheme.BodyMin;
            _settingsMuteLabel.alignment = TextAnchor.MiddleLeft;
            _settingsMuteLabel.fontStyle = FontStyle.Bold;
            _settingsMuteLabel.color = UiTheme.Accent;
            _settingsMuteLabel.raycastTarget = false;
            Stretch(_settingsMuteLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.46f, 0.92f));

            _settingsMuteValue = CreateText(
                "SettingsMuteValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsMuteValue.rectTransform, new Vector2(0.50f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsMuteValue.color = UiTheme.Primary;
            _settingsMuteValue.raycastTarget = false;
        }

        private void BuildSettingsShakeRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsShake",
                _settingsPanel.transform,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            row.onClick.AddListener(ToggleScreenShake);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsShakeLabel = row.GetComponentInChildren<Text>();
            _settingsShakeLabel.fontSize = UiTheme.BodyMin;
            _settingsShakeLabel.alignment = TextAnchor.MiddleLeft;
            _settingsShakeLabel.fontStyle = FontStyle.Bold;
            _settingsShakeLabel.color = UiTheme.Accent;
            _settingsShakeLabel.raycastTarget = false;
            Stretch(_settingsShakeLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.62f, 0.92f));

            _settingsShakeValue = CreateText(
                "SettingsShakeValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsShakeValue.rectTransform, new Vector2(0.64f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsShakeValue.color = UiTheme.Primary;
            _settingsShakeValue.raycastTarget = false;
        }

        private void BuildSettingsLanguageRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsLanguage",
                _settingsPanel.transform,
                body,
                new Vector2(0.06f, y0),
                new Vector2(0.94f, y1));
            _settingsRowButtons[rowIndex] = row;
            row.onClick.AddListener(CycleSettingsLanguage);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsLanguageLabel = CreateText(
                "SettingsLanguageLabel",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleLeft,
                FontStyle.Bold);
            Stretch(_settingsLanguageLabel.rectTransform, new Vector2(0.04f, 0.12f), new Vector2(0.46f, 0.88f));
            _settingsLanguageLabel.color = UiTheme.Accent;

            _settingsLanguageValue = CreateText(
                "SettingsLanguageValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleCenter,
                FontStyle.Bold);
            Stretch(_settingsLanguageValue.rectTransform, new Vector2(0.48f, 0.56f), new Vector2(0.62f, 0.88f));
            _settingsLanguageValue.color = UiTheme.Primary;

            _settingsEnBezel = CreateFlagButton(
                "SettingsEn",
                row.transform,
                new Vector2(0.66f, 0.18f),
                new Vector2(0.80f, 0.82f),
                () => OnPickLanguage(GameLanguage.English));
            Text enLabel = CreateText("SettingsEnLabel", _settingsEnBezel.transform, body, UiTheme.BodyMin, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(enLabel.rectTransform, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f));
            enLabel.color = UiTheme.Accent;
            enLabel.text = Loc.T("ui.settings.en", "EN");

            _settingsSvBezel = CreateFlagButton(
                "SettingsSv",
                row.transform,
                new Vector2(0.82f, 0.18f),
                new Vector2(0.96f, 0.82f),
                () => OnPickLanguage(GameLanguage.Swedish));
            Text svLabel = CreateText("SettingsSvLabel", _settingsSvBezel.transform, body, UiTheme.BodyMin, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(svLabel.rectTransform, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f));
            svLabel.color = UiTheme.Accent;
            svLabel.text = Loc.T("ui.settings.sv", "SV");
        }

        private void BuildSettingsControlsRow(Font display, Font body, float y0, float y1)
        {
            _settingsControlsTitle = CreateText(
                "SettingsControlsTitle",
                _settingsPanel.transform,
                display,
                UiTheme.BodyMin,
                TextAnchor.UpperLeft,
                FontStyle.Bold);
            Stretch(
                _settingsControlsTitle.rectTransform,
                new Vector2(SettingsMeasure.BodyMinX, y1 - SettingsMeasure.ControlsHeaderInset),
                new Vector2(SettingsMeasure.BodyMaxX, y1));
            _settingsControlsTitle.color = UiTheme.Primary;

            _settingsControlsBody = CreateText(
                "SettingsControlsBody",
                _settingsPanel.transform,
                body,
                UiTheme.HintSize(Screen.width),
                TextAnchor.UpperLeft,
                FontStyle.Normal);
            Stretch(
                _settingsControlsBody.rectTransform,
                new Vector2(SettingsMeasure.BodyMinX, y0),
                new Vector2(SettingsMeasure.BodyMaxX, y1 - SettingsMeasure.ControlsHeaderInset));
            _settingsControlsBody.color = UiTheme.FooterHint;
            _settingsControlsBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _settingsControlsBody.verticalOverflow = VerticalWrapMode.Overflow;
            _settingsControlsBody.alignByGeometry = false;
            _settingsControlsBody.text = FullControlHint();
        }

        private void BuildSettingsCloseRow(int rowIndex, Font display, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsClose",
                _settingsPanel.transform,
                display,
                new Vector2(0.22f, y0),
                new Vector2(0.78f, y1));
            _settingsRowButtons[rowIndex] = row;
            Text closeLabel = row.GetComponentInChildren<Text>();
            closeLabel.fontSize = UiTheme.BodyMin;
            closeLabel.text = Loc.T("ui.settings.close", "Close");
            row.onClick.AddListener(CloseSettings);
            UiTheme.ApplyButton(row, true, false, false);
        }

        private static string FullControlHint()
        {
            string playHint = Loc.T(
                "ui.hint_play",
                "WASD / LS move · Mouse / RS aim · LMB / RT fire · E / LT utility · Q / LB cycle · Esc / Start = back to hangar");
            string hangarHint = Loc.Tf(
                "ui.hint_hangar",
                "LS move · {0}",
                Loc.T("ui.hangar_controls", HangarControlsHint));
            string playCaption = Loc.T("ui.settings.play", "Play");
            string hangarCaption = Loc.T("ui.settings.hangar", "Hangar");
            return playCaption + "\n" + playHint + "\n" + hangarCaption + "\n" + hangarHint;
        }

        private void OnSettingsMusicVolume(float value)
        {
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.SetMusicVolume(value);
            }
        }

        private void OnSettingsSfxVolume(float value)
        {
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.SetSfxVolume(value);
            }
        }

        private void ToggleSettingsMute()
        {
            if (AudioCues.Instance == null)
            {
                return;
            }

            if (!AudioCues.Instance.Muted)
            {
                AudioCues.Instance.PlayUiClick();
            }

            AudioCues.Instance.ToggleMute();
            if (!AudioCues.Instance.Muted)
            {
                AudioCues.Instance.PlayUiClick();
            }

            RefreshSettingsAudio();
        }

        private void StepSettingsVolume(bool music, int direction)
        {
            float current;
            if (music)
            {
                current = AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : AudioCues.DefaultMusicVolume;
            }
            else
            {
                current = AudioCues.Instance != null ? AudioCues.Instance.SfxVolume : AudioCues.DefaultSfxVolume;
            }

            float next = SettingsRows.StepVolume(current, direction);
            if (AudioCues.Instance == null)
            {
                return;
            }

            if (music)
            {
                AudioCues.Instance.SetMusicVolume(next);
            }
            else
            {
                AudioCues.Instance.SetSfxVolume(next);
            }

            RefreshSettingsAudio();
        }

        private void ToggleScreenShake()
        {
            if (_settings == null)
            {
                _settings = SettingsState.Load();
            }

            _settings.ScreenShake = !_settings.ScreenShake;
            _settings.Save();
            RefreshSettingsShake();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsShake()
        {
            if (_settingsShakeLabel != null)
            {
                _settingsShakeLabel.text = Loc.T("ui.settings.shake", "Screen shake");
            }

            if (_settingsShakeValue == null)
            {
                return;
            }

            bool enabled = true;
            if (_settings != null)
            {
                enabled = _settings.ScreenShake;
            }

            _settingsShakeValue.text = enabled
                ? Loc.T("ui.settings.on", "On")
                : Loc.T("ui.settings.off", "Off");
        }

        private void RefreshSettingsAudio()
        {
            if (_settingsMusicLabel != null)
            {
                _settingsMusicLabel.text = Loc.T("ui.music", "Music");
            }

            if (_settingsSfxLabel != null)
            {
                _settingsSfxLabel.text = Loc.T("ui.sfx", "SFX");
            }

            if (_settingsMuteLabel != null)
            {
                _settingsMuteLabel.text = Loc.T("ui.mute", "Mute");
            }

            if (_settingsMuteValue != null)
            {
                bool muted = AudioCues.Instance != null && AudioCues.Instance.Muted;
                _settingsMuteValue.text = muted
                    ? Loc.T("ui.settings.on", "On")
                    : Loc.T("ui.settings.off", "Off");
            }

            float musicVolume = AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : AudioCues.DefaultMusicVolume;
            float sfxVolume = AudioCues.Instance != null ? AudioCues.Instance.SfxVolume : AudioCues.DefaultSfxVolume;
            if (_settingsMusicSlider != null)
            {
                _settingsMusicSlider.SetValueWithoutNotify(musicVolume);
            }

            if (_settingsSfxSlider != null)
            {
                _settingsSfxSlider.SetValueWithoutNotify(sfxVolume);
            }
        }

        private static int IndexOfSettingsRow(SettingsRowId rowId)
        {
            for (int index = 0; index < SettingsRows.Count; index++)
            {
                if (SettingsRows.Order[index] == rowId)
                {
                    return index;
                }
            }

            return -1;
        }

        private SettingsInputFlags ReadSettingsFlags()
        {
            SettingsInputFlags flags = new SettingsInputFlags();
            flags.Open = _settingsOpen;
            flags.Playing = _session != null && _session.Phase == GamePhase.Playing;
            flags.CreditsVisible = _creditsVisible;
            flags.Escape = Input.GetKeyDown(KeyCode.Escape);
            flags.Start = GamepadInput.PausePressed() && !flags.Escape;
            flags.Cancel = GamepadInput.CancelPressed() && !flags.Escape;
            flags.Submit = GamepadInput.ConfirmPressed();
            flags.F1 = Input.GetKeyDown(KeyCode.F1);
            flags.Select = Input.GetKeyDown(KeyCode.JoystickButton6);
            Vector2 settingsStick = GamepadInput.UiNavCombined();
            flags.NavX = HangarPadNav.DominantStep(settingsStick.x, settingsStick.y, HangarPadNav.Flick);
            flags.NavY = SettingsInputRouter.ScreenStepY(settingsStick.x, settingsStick.y, HangarPadNav.Flick);
            return flags;
        }

        private void OpenSettings()
        {
            if (_settingsOpen)
            {
                return;
            }

            if (_session != null && _session.Phase == GamePhase.Playing)
            {
                return;
            }

            if (_creditsVisible)
            {
                return;
            }

            if (_settings == null)
            {
                _settings = SettingsState.Load();
            }

            _settingsOpen = true;
            _settingsIndex = SettingsRows.FirstNavigable();
            _settingsNavHeld = false;
            if (_settingsRoot != null)
            {
                _settingsRoot.SetActive(true);
                _settingsRoot.transform.SetAsLastSibling();
            }

            ApplyLocalizedStaticLabels();
            RefreshLanguageChrome();
            RefreshSettingsFocus();
            SetSettingsNavigationLock(true);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void CloseSettingsFromScrim()
        {
            CloseSettings();
        }

        private void CloseSettings()
        {
            PersistSettings();
            HideSettingsRoot();
            SetSettingsNavigationLock(false);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }

            FocusHangarSlot(HangarPadNav.SettingsSlot);
        }

        private void DismissSettingsForPlay()
        {
            if (!_settingsOpen)
            {
                return;
            }

            PersistSettings();
            HideSettingsRoot();
            SetSettingsNavigationLock(false);
        }

        private void PersistSettings()
        {
            if (_settings == null)
            {
                _settings = SettingsState.CreateDefault();
            }

            _settings.Normalize();
            _settings.Save();
        }

        private void HideSettingsRoot()
        {
            _settingsOpen = false;
            _settingsNavHeld = false;
            if (_settingsRoot != null)
            {
                _settingsRoot.SetActive(false);
            }
        }

        private void SetSettingsNavigationLock(bool locked)
        {
            EventSystem es = EventSystem.current;
            if (es == null)
            {
                return;
            }

            es.sendNavigationEvents = !locked;
            if (locked)
            {
                es.SetSelectedGameObject(null);
            }
        }

        private void StepSettingsNav(SettingsRoute route, SettingsInputFlags flags)
        {
            float now = Time.unscaledTime;
            if (_settingsNavHeld && now < _settingsNavRepeatAt)
            {
                return;
            }

            if (route == SettingsRoute.Move)
            {
                int delta = flags.NavY > 0 ? -1 : 1;
                _settingsIndex = SettingsRows.Move(_settingsIndex, delta);
                RefreshSettingsFocus();
            }
            else
            {
                NudgeSettingsValue(flags.NavX);
            }

            _settingsNavRepeatAt = now + (_settingsNavHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
            _settingsNavHeld = true;
        }

        private void NudgeSettingsValue(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SettingsRowId rowId = SettingsRows.At(_settingsIndex);
            if (!SettingsRows.IsValue(rowId))
            {
                return;
            }

            if (rowId == SettingsRowId.Language)
            {
                if (direction < 0)
                {
                    OnPickLanguage(GameLanguage.English);
                }
                else
                {
                    OnPickLanguage(GameLanguage.Swedish);
                }

                return;
            }

            if (rowId == SettingsRowId.Music)
            {
                StepSettingsVolume(true, direction);
                return;
            }

            if (rowId == SettingsRowId.Sfx)
            {
                StepSettingsVolume(false, direction);
                return;
            }

            if (rowId == SettingsRowId.Mute)
            {
                ToggleSettingsMute();
                return;
            }

            if (rowId == SettingsRowId.ScreenShake)
            {
                ToggleScreenShake();
            }
        }

        private void ActivateSettingsRow()
        {
            SettingsRowId rowId = SettingsRows.At(_settingsIndex);
            if (rowId == SettingsRowId.Language)
            {
                CycleSettingsLanguage();
                return;
            }

            if (rowId == SettingsRowId.Mute)
            {
                ToggleSettingsMute();
                return;
            }

            if (rowId == SettingsRowId.ScreenShake)
            {
                ToggleScreenShake();
                return;
            }

            if (rowId == SettingsRowId.Close)
            {
                CloseSettings();
            }
        }

        private void CycleSettingsLanguage()
        {
            GameLanguage next = Loc.Language == GameLanguage.English
                ? GameLanguage.Swedish
                : GameLanguage.English;
            OnPickLanguage(next);
        }

        private void RefreshSettingsFocus()
        {
            if (_settingsRowButtons == null)
            {
                return;
            }

            int focusIndex = SettingsRows.ClampIndex(_settingsIndex);
            for (int index = 0; index < _settingsRowButtons.Length; index++)
            {
                Button rowButton = _settingsRowButtons[index];
                if (rowButton == null)
                {
                    continue;
                }

                UiTheme.SetPadFocus(rowButton.gameObject, index == focusIndex, false);
            }
        }
    }
}
