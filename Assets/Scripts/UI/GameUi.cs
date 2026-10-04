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
        private Button _newRunButton;
        private Text _newRunLabel;
        private Button[] _legacyButtons;
        private Text[] _legacyLabels;
        private Button[] _buyButtons;
        private Text[] _buyLabels;
        private Button _hullRepairButton;
        private Button _extraLifeButton;
        private Button _shieldRefillButton;
        private Button _bankButton;
        private Text _hullRepairLabel;
        private Text _extraLifeLabel;
        private Text _shieldRefillLabel;
        private Text _bankLabel;
        private Button _sinkEntry;
        private Text _sinkEntryLabel;
        private GameObject _sinkRoot;
        private Button[] _sinkButtons;
        private Text[] _sinkLabels;
        private Button _sinkClose;
        private Text _sinkHeader;
        private Text _sinkBlurb;
        private Text _sinkCloseLabel;
        private bool _sinkOpen;
        private int _sinkFocus;
        private int _sinkOpenedFrame = -1;
        private bool _sinkHeld;
        private float _sinkRepeatAt;
        private GameObject _setupRoot;
        private Button[] _setupButtons;
        private Text _setupTitle;
        private Text _setupBlurb;
        private bool _setupOpen;
        private int _setupFocus;
        private int _setupOpenedFrame = -1;
        private bool _setupHeld;
        private float _setupRepeatAt;
        private RunSetupKind _setupKind;
        private int _setupMask;
        private bool _setupRejected;
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
        private Text _summaryRecord;
        private GameObject _boonCanvas;
        private GameObject _boonRoot;
        private Text _boonTitle;
        private Text _boonHint;
        private Button[] _boonCards;
        private Text[] _boonCardLabels;
        private Image[] _boonCardPlates;
        private Text _boonRow;
        private int _boonFocus;
        private bool _boonNavHeld;
        private float _boonRepeatAt;
        private bool _boonShown;
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
        private GameObject _bossRoot;
        private Text _bossLabel;
        private Text _bossCount;
        private Image _bossFill;
        private string _eliteBanner = string.Empty;
        private static Sprite _barFillSprite;
        private bool _tutorialDismissed;
        private bool _doctrineIntroDismissed;
        private float _worldFlashUntil;
        private int _flashedWorld = 1;
        private int _flashedWave = 1;
        private string _flashedLayout = string.Empty;
        private string _flashedBadge = string.Empty;
        private string _medalBeat = string.Empty;
        private string _statusBase = string.Empty;
        private ShopItem _hoveredItem;
        private float _hitFlashUntil;
        private float _hitFlashStart;
        private float _hitFlashStrength;
        private Color _hitFlashColor = new Color(0.722f, 0.353f, 0.157f, 1f);
        private GameObject _endCreditsRoot;
        private Text _endCreditsBody;
        private Button _creditsButton;
        private Text _creditsButtonLabel;
        private Text _creditsContinueLabel;
        private Text _firstFlightTitle;
        private Text _firstFlightBody;
        private Text _gotItLabel;
        private Button _gotItButton;
        private GameObject _firstStartRoot;
        private Button _firstEasy;
        private Button _firstNormal;
        private Button _firstHard;
        private Button _firstGo;
        private Button _firstSkip;
        private Text _firstEasyLabel;
        private Text _firstNormalLabel;
        private Text _firstHardLabel;
        private Text _firstGoLabel;
        private Text _firstSkipLabel;
        private Text _firstStartTitle;
        private DifficultyGrade _firstStartPick = DifficultyGrade.Normal;
        private bool _firstStartArmed;
        private Text _tutorialBanner;
        private Button _tutorialSkipPlay;
        private int _pulseShopIndex = -1;
        private GamePhase _failFocusPhase = GamePhase.Hangar;
        private Button _creditsContinue;
        private GameObject _lastPadSelected;
        private bool _abortUrgent;
        private Image _primaryPlate;
        private Image _abortPlate;
        private Text _hullHeader;
        private Text _weaponsHeader;
        private Text _defenseHeader;
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
        private float _settingsShift;
        private bool _settingsScrollMute;
        private GameObject _settingsScrollTrack;
        private Scrollbar _settingsScrollbar;
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
        private Text _settingsReduceLabel;
        private Text _settingsReduceValue;
        private Text _settingsAssistLabel;
        private Text _settingsAssistValue;
        private Text _settingsShakeValue;
        private Text _settingsHintModeLabel;
        private Text _settingsHintModeValue;
        private Text _settingsHintSizeLabel;
        private Text _settingsHintSizeValue;
        private Text _settingsConfirmAbortLabel;
        private Text _settingsConfirmAbortValue;
        private Text _settingsConfirmNewRunLabel;
        private Text _settingsConfirmNewRunValue;
        private Text _settingsPadNavLabel;
        private Text _settingsPadNavValue;
        private Text _settingsPromptLabel;
        private Text _settingsPromptValue;
        private Text _settingsRebindLabel;
        private RebindOverlay _rebindOverlay;
        private Text _settingsWindowLabel;
        private Text _settingsWindowValue;
        private Text _settingsResolutionLabel;
        private Text _settingsResolutionValue;
        private Text _settingsVSyncLabel;
        private Text _settingsVSyncValue;
        private Text _settingsFpsLabel;
        private Text _settingsFpsValue;
        private GameObject _settingsViewport;
        private Transform _settingsRows;
        private Slider _settingsMusicSlider;
        private Slider _settingsSfxSlider;
        private Button[] _settingsRowButtons;
        private bool _confirmOpen;
        private ConfirmKind _confirmKind;
        private int _confirmFocus;
        private bool _confirmNavHeld;
        private float _confirmNavRepeatAt;
        private int _confirmOpenedFrame = -1;
        private bool _confirmSilencedShip;
        private GameObject _confirmRoot;
        private Text _confirmTitle;
        private Text _confirmBody;
        private Button _confirmYes;
        private Button _confirmNo;
        private Text _confirmYesLabel;
        private Text _confirmNoLabel;

        private static readonly Color UiAmber = UiTheme.Primary;
        private static readonly Color UiBody = UiTheme.Accent;
        private static readonly Color UiHull = UiTheme.Primary;
        private static readonly Color UiShield = UiTheme.Secondary;

        // Hangar left column: bottom matches LOADOUT frame (ShipPreviewMin.y 0.080);
        // top stays under the full-width top bar (0.905) so WAVE CLEAR / shop never sit on chrome.
        public static readonly Vector2 HangarPanelMin = new Vector2(0.014f, 0.080f);
        public static readonly Vector2 HangarPanelMax = new Vector2(0.55f, 0.888f);
        public static readonly Vector2 TopBarMin = new Vector2(0.012f, 0.905f);
        public static readonly Vector2 TopBarMax = new Vector2(0.988f, 0.995f);
        // Gear stays on the top-right. Mute / SFX / Music live in the settings panel.
        public static readonly Vector2 SettingsGearMin = new Vector2(0.900f, 0.905f);
        public static readonly Vector2 SettingsGearMax = new Vector2(0.988f, 0.995f);
        public static readonly Vector2 DifficultyMin = new Vector2(0.748f, 0.905f);
        public static readonly Vector2 DifficultyMax = new Vector2(0.888f, 0.995f);
        public static readonly Vector2 FirstFlightMin = new Vector2(0.02f, 0.800f);
        public static readonly Vector2 FirstFlightMax = new Vector2(0.98f, 0.995f);
        public const float FailedPreviewMaxY = 0.468f;
        public static readonly Vector2 FailedHealthMin = new Vector2(0.562f, 0.480f);
        public static readonly Vector2 FailedHealthMax = new Vector2(0.986f, 0.596f);
        public static readonly Vector2 ToastMin = new Vector2(0.500f, 0.912f);
        public static readonly Vector2 ToastMax = new Vector2(0.735f, 0.988f);
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
        public const int ShipPreviewSortOrder = CanvasOrder.ShipPreview;
        public const string ShipPreviewCanvasName = "ShipPreviewCanvas";
        public const string FirstHangarHintKey = "agr.ui.firstHangarHint";
        public const string DoctrineHintKey = "agr.ui.doctrineHint";
        public const string DoctrineHintTitle = "Doctrines";
        public const string DoctrineHintBody =
            "Doctrines are open. Pick Barrage, Lance, or Hunter.";
        public const string HangarControlsHint =
            "{utility} utility · {cycle} cycle · {confirm} confirm · {cancel} Next Wave · {pause} launch wave";
        public const string MedalLadderPrefix = "MEDALS";
        public const string HangarHintBody =
            "{move} fly  ·  {fire} shoot  ·  {utility} utility\n"
            + "{pause} = launch wave  ·  {cancel} = focus Next Wave";
        public const string HangarHintDetail =
            "Clear a wave to earn credits and upgrades.\n"
            + "Medal ladder (top-left): \u2022 Scout Wing at wave 3.";
        public const string FirstWaveCoach = "Shoot rocks  ·  {pause} returns to hangar";
        public const string HintPlay =
            "{move} move · {aim} aim · {fire} fire · {utility} utility · {cycle} cycle · {pause} = back to hangar";
        public const string HintFooter = "{confirm} Select · {pause} Launch wave · {settings} Settings";
        public const string HintDual = "{utility} utility · {cycle} cycle primary · {fire} fire";
        public const string HintRail = "Hold {fire} 0.55s, release — Rail. Miss or cancel pays half CD.";

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
                bool tutorialPlay = playing && _game != null && _game.TutorialActive;
                _abortButton.gameObject.SetActive(playing && !tutorialPlay);
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
            RefreshBossBar();
            RefreshUtilityHud(playing);
            RefreshSinkChrome(playing);
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

            if (_diffPanel != null)
            {
                _diffPanel.SetActive(!playing);
            }

            EnsureShipPreviewFrame();
            ForcePreviewChrome();

            ApplyLocalizedStaticLabels();
            RefreshLanguageChrome();
            RefreshDifficultyChrome();
            RefreshContinueChrome();

            ApplyBottomHint(playing);
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
            RefreshFirstStart();
            RefreshTutorialChrome();
            RefreshBoonChrome();

            if (playing)
            {
                _failFocusPhase = GamePhase.Playing;
                RefreshFirstStart();
                RefreshTutorialChrome();
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
                    ApplyPrimaryCaption(GamePhase.WaveClear, _session.WorldCleared, _session.WaveIndex);
                    break;
                case GamePhase.CampaignClear:
                    _statusBase = string.Empty;
                    ApplyPrimaryCaption(
                        GamePhase.WaveClear,
                        CampaignCap.FinalWorld,
                        CampaignCap.FinalWave + 1);
                    break;
                case GamePhase.Failed:
                    _statusBase = LegacyBalanceText();
                    ApplyPrimaryCaption(GamePhase.Failed, 0, _session.WaveIndex);
                    break;
                default:
                    _statusBase = string.Empty;
                    ApplyPrimaryCaption(GamePhase.Hangar, 0, _session.WaveIndex);
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

            RefreshServiceButtons();

            RefreshSettingsAudio();
            EnsurePrimaryClickable();
            RememberRecommendedUpgrade();
            NotePhaseFocus();
        }

        private string HangarReadyStatus()
        {
            string waveLine = !_tutorialDismissed && _session.WaveIndex == 1
                ? Loc.T("ui.hangar_clear_wave", "Hangar  ·  Clear a wave to earn credits and upgrades.")
                : Loc.Tf(
                    "ui.hangar_wave_line",
                    "Hangar  ·  Wave {0}  ·  World {1} layout: {2}",
                    _session.WaveIndex,
                    WorldCatalog.NumberForWave(_session.WaveIndex),
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

            string dailyHangar = _game != null ? DailyCopy.Stamp(_game.ActiveDailyDate, _game.ActiveDailySeed) : string.Empty;
            if (dailyHangar.Length > 0)
            {
                extra += "  ·  " + dailyHangar;
            }

            string mutatorHangar = _game != null ? MutatorCopy.Hud(_game.ActiveMutatorMask) : string.Empty;
            if (mutatorHangar.Length > 0)
            {
                extra += "  ·  " + mutatorHangar;
            }

            // Controls hint lives only in the screen-bottom row, never in WAVE CLEAR / shop.
            return waveLine + extra;
        }

        private string LegacyBalanceText()
        {
            int points = 0;
            int bestScore = 0;
            int bestWave = 0;
            int bestWorld = 0;
            if (_game != null && _game.Meta != null)
            {
                points = _game.Meta.LegacyPoints;
                LegacyProgress.ReadBest(
                    _game.Meta,
                    (int)DifficultySettings.Current,
                    out bestScore,
                    out bestWave,
                    out bestWorld);
            }

            return LegacyProgress.HangarLine(points, bestScore, bestWave, bestWorld);
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
            DisplayRuntime.Apply(_settings);
            Font display = UiFonts.Display();
            Font body = UiFonts.Body();
            _scrim = CreateFill("Scrim", transform, UiTheme.WithAlpha(UiTheme.Void, 0.22f), new Vector2(0f, 0f), new Vector2(1f, 1f));
            _vignette = BuildPlayVignette();
            _hitFlash = CreateFill("ScreenFlash", transform, new Color(1f, 0.96f, 0.92f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 1f)).GetComponent<Image>();
            RaiseCanvas(_hitFlash.gameObject, CanvasOrder.Toast);

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
            BuildBossBar(display, body);

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
            _hint.text = HintFooter;
            ClampOneLine(_hint);
            AddReadability(_hint, false);

            _menuRoot = UiTheme.BuildPanel(
                "HangarPanel",
                transform,
                HangarPanelMin,
                HangarPanelMax,
                0.962f);
            RaiseShopAbovePreview();
            CreateFill("HangarInner", _menuRoot.transform, UiTheme.InnerWash,
                new Vector2(0.012f, 0.018f), new Vector2(0.988f, 0.948f));

            BuildRunSummary(display, body);

            _achievementToast = CreateText("AchievementToast", transform, display, 18, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_achievementToast.rectTransform, ToastMin, ToastMax);
            ClampOneLine(_achievementToast);
            _achievementToast.color = UiAmber;
            _achievementToast.raycastTarget = false;
            _achievementToast.gameObject.SetActive(false);
            AddReadability(_achievementToast, true);
            RaiseCanvas(_achievementToast.gameObject, CanvasOrder.Toast);

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
            _primary.onClick.AddListener(OnPrimaryClicked);
            _primaryPlate = _primary.targetGraphic as Image;
            UiTheme.ApplyButton(_primary, true, false, false);

            _abortButton = CreateButton("AbortWave", transform, body, new Vector2(0.78f, 0.09f), new Vector2(0.97f, 0.155f));
            _abortLabel = _abortButton.GetComponentInChildren<Text>();
            _abortLabel.text = "Abort > Hangar";
            _abortLabel.fontSize = 18;
            _abortButton.onClick.AddListener(RequestAbort);
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
            BuildSinkOverlay(display, body);
            BuildRunSetup(display, body);
            BuildSettingsGear(body);
            BuildDifficultyPicker(display, body);
            BuildFirstHangarHint(display, body);
            BuildEndCredits(display, body);
            BuildShipPreviewFrame(display);
            BuildSettingsPanel(display, body);
            BuildConfirmDialog(display, body);
            BuildFirstStart(display, body);
            BuildTutorialPlayChrome(display);
            BuildContinueAndLegacy(body);
            ApplyLocalizedStaticLabels();
            RefreshLanguageChrome();
            RefreshDifficultyChrome();
            ApplyFooterHintSize();
            EnsurePrimaryClickable();
        }

        private void BuildContinueAndLegacy(Font body)
        {
            _newRunButton = CreateButton(
                "NewRun",
                _summaryRoot.transform,
                body,
                new Vector2(0.72f, 0.55f),
                new Vector2(0.97f, 0.94f));
            _newRunLabel = _newRunButton.GetComponentInChildren<Text>();
            _newRunLabel.fontSize = UiTheme.BodyMin;
            _newRunLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _newRunLabel.verticalOverflow = VerticalWrapMode.Truncate;
            _newRunButton.onClick.AddListener(OnNewRunButton);
            LockButtonNavigation(_newRunButton);
            _newRunButton.gameObject.SetActive(false);

            _legacyButtons = new Button[LegacyProgress.PerkCount];
            _legacyLabels = new Text[LegacyProgress.PerkCount];
            for (int perkIndex = 0; perkIndex < LegacyProgress.PerkCount; perkIndex++)
            {
                int capturedPerk = perkIndex;
                float column = 0.02f + (capturedPerk * 0.245f);
                Button perkButton = CreateButton(
                    "LegacyPerk" + capturedPerk,
                    _menuRoot.transform,
                    body,
                    new Vector2(column, 0.092f),
                    new Vector2(column + 0.23f, 0.126f));
                Text perkLabel = perkButton.GetComponentInChildren<Text>();
                perkLabel.fontSize = UiTheme.BodyMin;
                perkLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                perkLabel.verticalOverflow = VerticalWrapMode.Truncate;
                perkButton.onClick.AddListener(() => OnLegacyPerk(capturedPerk));
                LockButtonNavigation(perkButton);
                perkButton.gameObject.SetActive(false);
                _legacyButtons[capturedPerk] = perkButton;
                _legacyLabels[capturedPerk] = perkLabel;
            }

            _boonRow = CreateText("BoonRow", _menuRoot.transform, body, 12, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_boonRow.rectTransform, new Vector2(0.03f, 0.801f), new Vector2(0.97f, 0.819f));
            _boonRow.color = UiTheme.Secondary;
            _boonRow.horizontalOverflow = HorizontalWrapMode.Wrap;
            _boonRow.verticalOverflow = VerticalWrapMode.Truncate;
            _boonRow.gameObject.SetActive(false);
            BuildBoonModal(UiFonts.Display(), body);
        }

        private void OnLegacyPerk(int perk)
        {
            if (_game == null)
            {
                return;
            }

            if (_game.TryBuyLegacy(perk) && AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHangarPurchase();
            }
        }

        private void RequestAbandonSavedRun()
        {
            ConfirmRequest request = ReadConfirmRequest();
            request.AbandonClick = true;
            request.NewRunClick = false;
            request.AbortClick = false;
            request.Escape = false;
            request.Start = false;
            request.Submit = false;
            request.Cancel = false;
            request.Scrim = false;
            request.FocusDelta = 0;
            RunSaveData pending = _game != null ? _game.PendingContinue : null;
            bool savedPurchase = pending != null && (pending.UpgradeMask != 0 || pending.Doctrine != 0 || pending.Shield > 0);
            int savedWave = pending != null ? pending.WaveIndex : 1;
            int savedScore = pending != null ? pending.Score : 0;
            int savedCredits = pending != null ? pending.Credits : 0;
            bool savedProgress = GameSession.HasRunProgress(savedWave, savedScore, savedCredits, savedPurchase);
            EnsureSettings();
            request.ConfirmNewRun = GameSession.ShouldConfirmNewRun(_settings.ConfirmRestartNewRun, savedProgress);
            ApplyConfirmRoute(ConfirmDialogRouter.Route(request), request);
            ApplyConfirmClock();
        }

        private void OnNewRunButton()
        {
            if (_setupOpen || _confirmOpen || _settingsOpen || _sinkOpen)
            {
                return;
            }

            if (_game != null && _game.HasContinueOffer)
            {
                RequestAbandonSavedRun();
                return;
            }

            if (_session != null && _session.Phase == GamePhase.Failed)
            {
                OpenRunSetup(RunSetupKind.Retry);
                return;
            }

            OpenRunSetup(RunSetupKind.Hangar);
        }

        private void BuildRunSetup(Font display, Font body)
        {
            GameObject canvasGo = new GameObject("RunSetupCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = CanvasOrder.Overlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ShopGridLayout.RefWidth, ShopGridLayout.RefHeight);
            scaler.matchWidthOrHeight = ShopGridLayout.Match;
            canvasGo.AddComponent<GraphicRaycaster>();
            _setupRoot = canvasGo;

            GameObject plate = CreateFill(
                "RunSetupPlate",
                canvasGo.transform,
                UiTheme.Surface,
                new Vector2(RunSetupLayout.PanelMinX, RunSetupLayout.PanelMinY),
                new Vector2(RunSetupLayout.PanelMaxX, RunSetupLayout.PanelMaxY));
            Image plateImage = plate.GetComponent<Image>();
            if (plateImage != null)
            {
                plateImage.raycastTarget = true;
            }

            float headerMinX;
            float headerMinY;
            float headerMaxX;
            float headerMaxY;
            RunSetupLayout.Header(out headerMinX, out headerMinY, out headerMaxX, out headerMaxY);
            _setupTitle = CreateText("RunSetupTitle", canvasGo.transform, display, RunSetupLayout.Font, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_setupTitle.rectTransform, new Vector2(headerMinX, headerMinY), new Vector2(headerMaxX, headerMaxY));
            _setupTitle.color = UiTheme.Primary;
            _setupTitle.resizeTextForBestFit = false;
            _setupTitle.raycastTarget = false;

            float blurbMinX;
            float blurbMinY;
            float blurbMaxX;
            float blurbMaxY;
            RunSetupLayout.Blurb(out blurbMinX, out blurbMinY, out blurbMaxX, out blurbMaxY);
            _setupBlurb = CreateText("RunSetupBlurb", canvasGo.transform, body, RunSetupLayout.Font, TextAnchor.MiddleCenter, FontStyle.Normal);
            Stretch(_setupBlurb.rectTransform, new Vector2(blurbMinX, blurbMinY), new Vector2(blurbMaxX, blurbMaxY));
            _setupBlurb.color = UiTheme.Accent;
            _setupBlurb.resizeTextForBestFit = false;
            _setupBlurb.horizontalOverflow = HorizontalWrapMode.Wrap;
            _setupBlurb.verticalOverflow = VerticalWrapMode.Truncate;
            _setupBlurb.raycastTarget = false;

            _setupButtons = new Button[RunSetupNav.SlotCount];
            for (int setupIndex = 0; setupIndex < RunSetupNav.SlotCount; setupIndex++)
            {
                int capturedSlot = setupIndex;
                float choiceMinX;
                float choiceMinY;
                float choiceMaxX;
                float choiceMaxY;
                RunSetupLayout.Choice(setupIndex, out choiceMinX, out choiceMinY, out choiceMaxX, out choiceMaxY);
                Button choice = CreateButton(
                    "RunSetup" + setupIndex,
                    canvasGo.transform,
                    body,
                    new Vector2(choiceMinX, choiceMinY),
                    new Vector2(choiceMaxX, choiceMaxY));
                Text choiceLabel = choice.GetComponentInChildren<Text>();
                choiceLabel.fontSize = RunSetupLayout.Font;
                choiceLabel.resizeTextForBestFit = false;
                choiceLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                choiceLabel.verticalOverflow = VerticalWrapMode.Truncate;
                choice.onClick.AddListener(() => ApplyRunSetup(capturedSlot));
                LockButtonNavigation(choice);
                UiTheme.ApplyButton(choice, false, false, false);
                _setupButtons[setupIndex] = choice;
            }

            canvasGo.SetActive(false);
        }

        private void OpenRunSetup(RunSetupKind kind)
        {
            if (_setupRoot == null || _confirmOpen || _settingsOpen || _creditsVisible || FirstStartOpen())
            {
                return;
            }

            if (_session != null && _session.Phase == GamePhase.Playing)
            {
                return;
            }

            CloseSinkShop();
            _setupKind = kind;
            _setupMask = 0;
            _setupRejected = false;
            _setupOpen = true;
            _setupFocus = RunSetupNav.DefaultSlot(kind);
            _setupHeld = false;
            _setupOpenedFrame = Time.frameCount;
            _setupRoot.SetActive(true);
            if (_setupTitle != null)
            {
                _setupTitle.text = DailyCopy.Title();
            }

            if (_setupButtons != null)
            {
                if (_setupButtons[RunSetupNav.NormalSlot] != null)
                {
                    Text normalLabel = _setupButtons[RunSetupNav.NormalSlot].GetComponentInChildren<Text>();
                    if (normalLabel != null)
                    {
                        normalLabel.text = DailyCopy.Normal();
                    }
                }

                if (_setupButtons[RunSetupNav.DailySlot] != null)
                {
                    Text dailyLabel = _setupButtons[RunSetupNav.DailySlot].GetComponentInChildren<Text>();
                    if (dailyLabel != null)
                    {
                        dailyLabel.text = DailyCopy.Daily();
                    }
                }

                if (_setupButtons[RunSetupNav.CancelSlot] != null)
                {
                    Text cancelLabel = _setupButtons[RunSetupNav.CancelSlot].GetComponentInChildren<Text>();
                    if (cancelLabel != null)
                    {
                        cancelLabel.text = DailyCopy.Cancel();
                    }
                }
            }

            int boardScore = 0;
            int boardWave = 0;
            int clockYear;
            int clockMonth;
            int clockDay;
            DailySeed.UtcToday(out clockYear, out clockMonth, out clockDay);
            int packedDate = DailySeed.PackDate(clockYear, clockMonth, clockDay);
            if (_game != null && _game.Daily != null)
            {
                DailyBoardRules.TryRead(_game.Daily, packedDate, out boardScore, out boardWave);
            }

            if (_setupBlurb != null)
            {
                _setupBlurb.text = DailyCopy.Hint() + "\n" + DailyCopy.Blurb(boardScore, boardWave);
            }

            FocusRunSetup(_setupFocus);
        }

        private void CloseRunSetup()
        {
            _setupOpen = false;
            _setupHeld = false;
            if (_setupRoot != null)
            {
                _setupRoot.SetActive(false);
            }
        }

        private void TickRunSetup()
        {
            if (!_setupOpen)
            {
                return;
            }

            if (GamepadInput.CancelPressed() || GamepadInput.PausePressed())
            {
                CloseRunSetup();
                return;
            }

            Vector2 setupNav = GamepadInput.UiNavCombined();
            int setupDx = HangarPadNav.DominantStep(setupNav.x, setupNav.y, HangarPadNav.Flick);
            int setupDy = HangarPadNav.DominantStepY(setupNav.x, setupNav.y, HangarPadNav.Flick);
            if (setupDx != 0 || setupDy != 0)
            {
                float setupNow = Time.unscaledTime;
                if (!_setupHeld || setupNow >= _setupRepeatAt)
                {
                    _setupFocus = RunSetupNav.Step(_setupFocus, setupDx, setupDy);
                    _setupRepeatAt = setupNow + (_setupHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
                    _setupHeld = true;
                    FocusRunSetup(_setupFocus);
                }
            }
            else
            {
                _setupHeld = false;
            }

            if (GamepadInput.ConfirmPressed() && _setupOpenedFrame != Time.frameCount)
            {
                ApplyRunSetup(_setupFocus);
            }
        }

        private void FocusRunSetup(int slot)
        {
            if (_setupButtons == null)
            {
                return;
            }

            for (int setupIndex = 0; setupIndex < _setupButtons.Length; setupIndex++)
            {
                Button choice = _setupButtons[setupIndex];
                if (choice != null)
                {
                    UiTheme.SetPadFocus(choice.gameObject, setupIndex == slot, true);
                }
            }

            PaintRunSetup(slot);
        }

        private void PaintRunSetup(int slot)
        {
            if (_setupButtons != null)
            {
                for (int paintIndex = 0; paintIndex < _setupButtons.Length; paintIndex++)
                {
                    Button paintButton = _setupButtons[paintIndex];
                    if (paintButton == null)
                    {
                        continue;
                    }

                    Text paintLabel = paintButton.GetComponentInChildren<Text>();
                    if (paintLabel == null)
                    {
                        continue;
                    }

                    if (paintIndex == RunSetupNav.NormalSlot)
                    {
                        paintLabel.text = DailyCopy.Normal();
                    }
                    else if (paintIndex == RunSetupNav.DailySlot)
                    {
                        paintLabel.text = DailyCopy.Daily();
                    }
                    else if (paintIndex == RunSetupNav.CancelSlot)
                    {
                        paintLabel.text = DailyCopy.Cancel();
                    }
                    else if (RunSetupNav.IsMutator(paintIndex))
                    {
                        int paintId = RunSetupNav.MutatorId(paintIndex);
                        bool paintOn = MutatorRules.Has(_setupMask, paintId);
                        paintLabel.text = MutatorCopy.Row(paintId, paintOn);
                    }
                }
            }

            if (_setupBlurb == null)
            {
                return;
            }

            int paintYear;
            int paintMonth;
            int paintDay;
            DailySeed.UtcToday(out paintYear, out paintMonth, out paintDay);
            int paintDate = DailySeed.PackDate(paintYear, paintMonth, paintDay);
            int paintScore = 0;
            int paintWave = 0;
            if (_game != null && _game.Daily != null)
            {
                DailyBoardRules.TryRead(_game.Daily, paintDate, out paintScore, out paintWave);
            }

            string paintSecond;
            if (_setupRejected)
            {
                paintSecond = MutatorCopy.Rejected();
            }
            else if (RunSetupNav.IsMutator(slot))
            {
                paintSecond = MutatorCopy.Description(RunSetupNav.MutatorId(slot));
            }
            else if (_setupMask != 0)
            {
                paintSecond = MutatorCopy.StackLine(_setupMask);
            }
            else
            {
                paintSecond = DailyCopy.Blurb(paintScore, paintWave);
            }

            _setupBlurb.text = DailyCopy.Hint() + "\n" + paintSecond;
        }

        private void ApplyRunSetup(int slot)
        {
            if (RunSetupNav.IsMutator(slot))
            {
                int toggledMask;
                if (MutatorRules.TryToggle(_setupMask, RunSetupNav.MutatorId(slot), out toggledMask))
                {
                    _setupMask = toggledMask;
                    _setupRejected = false;
                }
                else
                {
                    _setupRejected = true;
                }

                FocusRunSetup(slot);
                return;
            }

            int chosenMask = _setupMask;
            RunSetupKind kind = _setupKind;
            CloseRunSetup();
            if (slot == RunSetupNav.CancelSlot || _game == null)
            {
                return;
            }

            int setupYear;
            int setupMonth;
            int setupDay;
            DailySeed.UtcToday(out setupYear, out setupMonth, out setupDay);
            bool daily = slot == RunSetupNav.DailySlot;
            if (kind == RunSetupKind.Abandon)
            {
                if (daily)
                {
                    _game.AbandonForDaily(setupYear, setupMonth, setupDay);
                }
                else
                {
                    _game.AbandonSavedRun();
                }

                _game.ApplyMutators(chosenMask);
                _game.RefreshHud();
                return;
            }

            if (kind == RunSetupKind.Retry)
            {
                _game.BeginRetry(daily, setupYear, setupMonth, setupDay, chosenMask);
                return;
            }

            _game.ApplyMutators(chosenMask);
            if (daily)
            {
                _game.ApplyDailyClock(setupYear, setupMonth, setupDay);
            }
            else
            {
                _game.ClearDailyStamp();
            }
        }

        private bool ShowDailyEntry(bool playing, bool offer)
        {
            if (playing || offer || _session == null)
            {
                return false;
            }

            if (_session.Phase == GamePhase.Failed)
            {
                return true;
            }

            return _session.Phase == GamePhase.Hangar && _session.WaveIndex <= 1 && _session.Score <= 0;
        }

        private void RefreshContinueChrome()
        {
            bool playing = _session != null && _session.Phase == GamePhase.Playing;
            bool offer = !playing && _game != null && _game.HasContinueOffer;
            bool dailyEntry = ShowDailyEntry(playing, offer);
            bool blockRunButton = _creditsVisible || FirstStartOpen() || _setupOpen;
            if (_newRunButton != null)
            {
                _newRunButton.gameObject.SetActive((offer || dailyEntry) && !blockRunButton);
                if (_newRunLabel != null)
                {
                    _newRunLabel.text = offer
                        ? Loc.T("ui.new_run", "New Run")
                        : DailyCopy.Daily();
                }
            }

            if (playing || _creditsVisible || FirstStartOpen())
            {
                CloseRunSetup();
            }

            bool showLegacy = !playing && _game != null && _game.LegacyShopOpen;
            if (_legacyButtons == null)
            {
                return;
            }

            for (int perkIndex = 0; perkIndex < _legacyButtons.Length; perkIndex++)
            {
                Button perkButton = _legacyButtons[perkIndex];
                if (perkButton == null)
                {
                    continue;
                }

                perkButton.gameObject.SetActive(showLegacy);
                if (showLegacy && _legacyLabels != null && perkIndex < _legacyLabels.Length && _legacyLabels[perkIndex] != null)
                {
                    _legacyLabels[perkIndex].text = LegacyProgress.PerkLabel(_game.Meta, perkIndex);
                }
            }
        }

        private void OnDisable()
        {
            Time.timeScale = ConfirmPause.TimeScale(false, false);
            if (_confirmSilencedShip && _ship != null && _session != null && _session.Phase == GamePhase.Playing)
            {
                _ship.SetInputEnabled(true);
            }

            _confirmSilencedShip = false;
        }

        private void OnDestroy()
        {
            Time.timeScale = ConfirmPause.TimeScale(false, false);
            SetSettingsNavigationLock(false);
            if (_previewCanvas != null)
            {
                Destroy(_previewCanvas);
                _previewCanvas = null;
            }

            if (_boonCanvas != null)
            {
                Destroy(_boonCanvas);
                _boonCanvas = null;
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
                canvas.sortingOrder = CanvasOrder.ShipPreview;
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
                overlay.sortingOrder = CanvasOrder.ShipPreview;
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
                    new Vector2(HangarPreviewRig.WellMinX, HangarPreviewRig.WellMinY),
                    new Vector2(HangarPreviewRig.WellMaxX, HangarPreviewRig.WellMaxY));

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

            ApplyShipPreviewAnchors();
            _previewRoot.transform.SetAsLastSibling();
            ForcePreviewChrome();
        }

        private void ApplyShipPreviewAnchors()
        {
            if (_previewRoot == null)
            {
                return;
            }

            bool failed = _session != null && _session.Phase == GamePhase.Failed;
            Vector2 max = failed
                ? new Vector2(ShipPreviewMax.x, FailedPreviewMaxY)
                : ShipPreviewMax;
            Stretch(_previewRoot.GetComponent<RectTransform>(), ShipPreviewMin, max);
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
                    overlay.sortingOrder = CanvasOrder.ShipPreview;
                    overlay.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            if (_previewCanvas != null)
            {
                GraphicRaycaster previewCaster = _previewCanvas.GetComponent<GraphicRaycaster>();
                if (previewCaster != null)
                {
                    // The ship frame sits on a higher overlay than the main canvas.
                    // While the boon picker is open its raycaster must not eat clicks.
                    previewCaster.enabled = !BoonModalOpen();
                }
            }

            if (_previewRoot != null)
            {
                _previewRoot.SetActive(show);
                _previewRoot.transform.SetAsLastSibling();
                ClipPreviewToFrame();
                ApplyShipPreviewAnchors();
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

                ApplyPreviewAspect();
            }

            RaiseShopAbovePreview();
            RaiseBoonModal();
        }

        private static void RaiseCanvas(GameObject root, int sortOrder)
        {
            if (root == null)
            {
                return;
            }

            Canvas overlayCanvas = root.GetComponent<Canvas>();
            if (overlayCanvas == null)
            {
                overlayCanvas = root.AddComponent<Canvas>();
            }

            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = sortOrder;
            if (root.GetComponent<GraphicRaycaster>() == null)
            {
                root.AddComponent<GraphicRaycaster>();
            }
        }

        private void RaiseShopAbovePreview()
        {
            if (_menuRoot == null)
            {
                return;
            }

            Canvas shopCanvas = _menuRoot.GetComponent<Canvas>();
            if (shopCanvas == null)
            {
                shopCanvas = _menuRoot.AddComponent<Canvas>();
            }

            shopCanvas.overrideSorting = true;
            shopCanvas.sortingOrder = CanvasOrder.HangarShop;
            GraphicRaycaster shopCaster = _menuRoot.GetComponent<GraphicRaycaster>();
            if (shopCaster == null)
            {
                _menuRoot.AddComponent<GraphicRaycaster>();
            }

            Image hangarPlate = _menuRoot.GetComponent<Image>();
            if (hangarPlate != null)
            {
                Color opaquePlate = hangarPlate.color;
                opaquePlate.a = 1f;
                hangarPlate.color = opaquePlate;
            }
        }

        private void ClipPreviewToFrame()
        {
            if (_previewRoot == null)
            {
                return;
            }

            if (_previewRoot.GetComponent<RectMask2D>() == null)
            {
                _previewRoot.AddComponent<RectMask2D>();
            }

            Transform previewWell = _previewRoot.transform.Find("PreviewWell");
            if (previewWell != null && previewWell.GetComponent<RectMask2D>() == null)
            {
                previewWell.gameObject.AddComponent<RectMask2D>();
            }
        }

        private void EnsureBoonModalCanvas()
        {
            if (_boonCanvas != null)
            {
                return;
            }

            GameObject canvasGo = new GameObject(BoonCardLayout.CanvasName);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = CanvasOrder.Boon;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(BoonCardLayout.RefWidth, BoonCardLayout.RefHeight);
            scaler.matchWidthOrHeight = BoonCardLayout.Match;
            canvasGo.AddComponent<GraphicRaycaster>();
            _boonCanvas = canvasGo;
        }

        private void RaiseBoonModal()
        {
            if (_boonCanvas == null)
            {
                return;
            }

            Canvas boonOverlay = _boonCanvas.GetComponent<Canvas>();
            if (boonOverlay == null)
            {
                return;
            }

            boonOverlay.renderMode = RenderMode.ScreenSpaceOverlay;
            boonOverlay.overrideSorting = true;
            boonOverlay.sortingOrder = CanvasOrder.Boon;
            boonOverlay.enabled = _boonCanvas.activeSelf;
            GraphicRaycaster boonCaster = _boonCanvas.GetComponent<GraphicRaycaster>();
            if (boonCaster != null)
            {
                boonCaster.enabled = true;
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
            CreateFill("HullCol", _menuRoot.transform, ShopTileChrome.Column,
                new Vector2(0.02f, gridBottom), new Vector2(0.49f, ShopGridTop));
            CreateFill("WeaponsCol", _menuRoot.transform, ShopTileChrome.Column,
                new Vector2(0.51f, gridBottom), new Vector2(0.735f, ShopGridTop));
            CreateFill("DefenseCol", _menuRoot.transform, ShopTileChrome.Column,
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
                FitShopLabel(_buyLabels[i], item.Group);
                UiTheme.ApplyButton(button, false, false, true);
                button.transform.SetAsLastSibling();
            }

            BuildServiceButtons(body);
        }

        private void BuildServiceButtons(Font body)
        {
            float rowStep = ShopCellHeight + ShopCellGutter;
            _hullRepairButton = CreateButton("Buy_HullRepair", _menuRoot.transform, body, DefenseServiceRect(2, rowStep).min, DefenseServiceRect(2, rowStep).max);
            _extraLifeButton = CreateButton("Buy_ExtraLife", _menuRoot.transform, body, DefenseServiceRect(3, rowStep).min, DefenseServiceRect(3, rowStep).max);
            _shieldRefillButton = CreateButton("Buy_ShieldRefill", _menuRoot.transform, body, DefenseServiceRect(4, rowStep).min, DefenseServiceRect(4, rowStep).max);
            float bankX = 0.02f + 2f * 0.1175f;
            float bankTop = ShopGridTop - 2f * rowStep;
            _bankButton = CreateButton(
                "Buy_Bank",
                _menuRoot.transform,
                body,
                new Vector2(bankX, bankTop - ShopCellHeight),
                new Vector2(bankX + 0.110f, bankTop));
            _hullRepairButton.onClick.AddListener(OnHullRepair);
            _extraLifeButton.onClick.AddListener(OnExtraLife);
            _shieldRefillButton.onClick.AddListener(OnShieldRefill);
            _bankButton.onClick.AddListener(OnBankCredits);
            _hullRepairLabel = _hullRepairButton.GetComponentInChildren<Text>();
            _extraLifeLabel = _extraLifeButton.GetComponentInChildren<Text>();
            _shieldRefillLabel = _shieldRefillButton.GetComponentInChildren<Text>();
            _bankLabel = _bankButton.GetComponentInChildren<Text>();
            FitShopLabel(_hullRepairLabel, ShopGroup.Defense);
            FitShopLabel(_extraLifeLabel, ShopGroup.Defense);
            FitShopLabel(_shieldRefillLabel, ShopGroup.Defense);
            FitShopLabel(_bankLabel, ShopGroup.Hull);
            UiTheme.ApplyButton(_hullRepairButton, false, false, true);
            UiTheme.ApplyButton(_extraLifeButton, false, false, true);
            UiTheme.ApplyButton(_shieldRefillButton, false, false, true);
            UiTheme.ApplyButton(_bankButton, false, false, true);
            BuildSinkEntry(body);
        }

        private void BuildSinkEntry(Font body)
        {
            float entryMinX;
            float entryMinY;
            float entryMaxX;
            float entryMaxY;
            SinkShopLayout.EntryLocal(out entryMinX, out entryMinY, out entryMaxX, out entryMaxY);
            _sinkEntry = CreateButton(
                "SinkBay",
                _menuRoot.transform,
                body,
                new Vector2(entryMinX, entryMinY),
                new Vector2(entryMaxX, entryMaxY));
            _sinkEntryLabel = _sinkEntry.GetComponentInChildren<Text>();
            _sinkEntryLabel.fontSize = SinkShopLayout.Font;
            _sinkEntryLabel.resizeTextForBestFit = false;
            _sinkEntryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _sinkEntryLabel.verticalOverflow = VerticalWrapMode.Truncate;
            _sinkEntry.onClick.AddListener(OpenSinkShop);
            LockButtonNavigation(_sinkEntry);
            UiTheme.ApplyButton(_sinkEntry, false, false, true);
        }

        private void BuildSinkOverlay(Font display, Font body)
        {
            GameObject canvasGo = new GameObject("SinkShopCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = CanvasOrder.Overlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ShopGridLayout.RefWidth, ShopGridLayout.RefHeight);
            scaler.matchWidthOrHeight = ShopGridLayout.Match;
            canvasGo.AddComponent<GraphicRaycaster>();
            _sinkRoot = canvasGo;

            GameObject plate = CreateFill(
                "SinkPlate",
                canvasGo.transform,
                UiTheme.Surface,
                new Vector2(SinkShopLayout.PanelMinX, SinkShopLayout.PanelMinY),
                new Vector2(SinkShopLayout.PanelMaxX, SinkShopLayout.PanelMaxY));
            Image plateImage = plate.GetComponent<Image>();
            if (plateImage != null)
            {
                plateImage.raycastTarget = true;
            }

            float headerMinX;
            float headerMinY;
            float headerMaxX;
            float headerMaxY;
            SinkShopLayout.Header(out headerMinX, out headerMinY, out headerMaxX, out headerMaxY);
            _sinkHeader = CreateText("SinkHeader", canvasGo.transform, display, SinkShopLayout.Font, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(_sinkHeader.rectTransform, new Vector2(headerMinX, headerMinY), new Vector2(headerMaxX, headerMaxY));
            _sinkHeader.color = UiTheme.Primary;
            _sinkHeader.text = SinkCopy.Title();

            float blurbMinX;
            float blurbMinY;
            float blurbMaxX;
            float blurbMaxY;
            SinkShopLayout.Blurb(out blurbMinX, out blurbMinY, out blurbMaxX, out blurbMaxY);
            _sinkBlurb = CreateText("SinkBlurb", canvasGo.transform, body, SinkShopLayout.Font, TextAnchor.MiddleLeft, FontStyle.Normal);
            Stretch(_sinkBlurb.rectTransform, new Vector2(blurbMinX, blurbMinY), new Vector2(blurbMaxX, blurbMaxY));
            _sinkBlurb.color = UiTheme.Accent;
            _sinkBlurb.text = SinkCopy.Blurb();

            _sinkButtons = new Button[ShopSinkCatalog.Count];
            _sinkLabels = new Text[ShopSinkCatalog.Count];
            for (int sinkIndex = 0; sinkIndex < ShopSinkCatalog.Count; sinkIndex++)
            {
                float tileMinX;
                float tileMinY;
                float tileMaxX;
                float tileMaxY;
                SinkShopLayout.Tile(sinkIndex, out tileMinX, out tileMinY, out tileMaxX, out tileMaxY);
                int captured = sinkIndex;
                Button tile = CreateButton(
                    "SinkTile" + sinkIndex,
                    canvasGo.transform,
                    body,
                    new Vector2(tileMinX, tileMinY),
                    new Vector2(tileMaxX, tileMaxY));
                Text tileLabel = tile.GetComponentInChildren<Text>();
                tileLabel.fontSize = SinkShopLayout.Font;
                tileLabel.resizeTextForBestFit = false;
                tileLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                tileLabel.verticalOverflow = VerticalWrapMode.Truncate;
                tile.onClick.AddListener(() => OnSinkTile(captured));
                LockButtonNavigation(tile);
                UiTheme.ApplyButton(tile, false, false, true);
                _sinkButtons[sinkIndex] = tile;
                _sinkLabels[sinkIndex] = tileLabel;
            }

            float closeMinX;
            float closeMinY;
            float closeMaxX;
            float closeMaxY;
            SinkShopLayout.CloseButton(out closeMinX, out closeMinY, out closeMaxX, out closeMaxY);
            _sinkClose = CreateButton(
                "SinkClose",
                canvasGo.transform,
                body,
                new Vector2(closeMinX, closeMinY),
                new Vector2(closeMaxX, closeMaxY));
            _sinkCloseLabel = _sinkClose.GetComponentInChildren<Text>();
            _sinkCloseLabel.fontSize = SinkShopLayout.Font;
            _sinkCloseLabel.resizeTextForBestFit = false;
            _sinkCloseLabel.text = SinkCopy.Close();
            _sinkClose.onClick.AddListener(CloseSinkShop);
            LockButtonNavigation(_sinkClose);
            UiTheme.ApplyButton(_sinkClose, false, false, false);
            canvasGo.SetActive(false);
        }

        private void RefreshSinkChrome(bool playing)
        {
            if (playing || _creditsVisible || FirstStartOpen())
            {
                CloseSinkShop();
            }

            if (_sinkEntry != null)
            {
                bool showEntry = !playing && !_creditsVisible && !FirstStartOpen();
                _sinkEntry.gameObject.SetActive(showEntry);
                if (_sinkEntryLabel != null)
                {
                    _sinkEntryLabel.text = SinkCopy.Entry();
                }
            }

            if (_sinkHeader != null)
            {
                _sinkHeader.text = SinkCopy.Title();
            }

            if (_sinkBlurb != null)
            {
                _sinkBlurb.text = SinkCopy.Hint() + "\n" + SinkCopy.Blurb();
            }

            if (_sinkCloseLabel != null)
            {
                _sinkCloseLabel.text = SinkCopy.Close();
            }

            if (!_sinkOpen || _sinkLabels == null || _game == null || _session == null)
            {
                return;
            }

            SinkProfileData profile = _game.Sinks;
            int credits = _session.Credits;
            for (int sinkIndex = 0; sinkIndex < _sinkLabels.Length; sinkIndex++)
            {
                Text tileLabel = _sinkLabels[sinkIndex];
                if (tileLabel == null)
                {
                    continue;
                }

                int state = SinkRules.TileState(profile, sinkIndex, credits);
                int rank = SinkRank(profile, sinkIndex);
                int price = ShopSinkCatalog.Price(sinkIndex, rank);
                string name = SinkCopy.Name(sinkIndex);
                string detail = SinkCopy.Detail(sinkIndex);
                string status = SinkCopy.Status(state, price);
                tileLabel.text = name + "\n" + status + "\n" + detail;
            }

            FocusSinkSlot(_sinkFocus);
        }

        private static int SinkRank(SinkProfileData profile, int sinkId)
        {
            if (profile == null)
            {
                return 0;
            }

            if (sinkId == ShopSinkCatalog.StartShield)
            {
                return profile.ShieldRank;
            }

            if (sinkId == ShopSinkCatalog.PickupReach)
            {
                return profile.ReachRank;
            }

            if (SinkProfileCodec.Owns(profile, sinkId))
            {
                return 1;
            }

            return 0;
        }

        private void OpenSinkShop()
        {
            if (_sinkRoot == null || _confirmOpen || _settingsOpen || _creditsVisible)
            {
                return;
            }

            if (_session == null || _session.Phase == GamePhase.Playing || FirstStartOpen())
            {
                return;
            }

            _sinkOpen = true;
            _sinkFocus = 0;
            _sinkHeld = false;
            _sinkOpenedFrame = Time.frameCount;
            _sinkRoot.SetActive(true);
            HoldPreviewSpin(true);
            RefreshSinkChrome(false);
        }

        private void CloseSinkShop()
        {
            bool wasOpen = _sinkOpen;
            _sinkOpen = false;
            _sinkHeld = false;
            if (_sinkRoot != null)
            {
                _sinkRoot.SetActive(false);
            }

            if (wasOpen && _hoveredItem == null)
            {
                HoldPreviewSpin(false);
            }
        }

        private void OnSinkTile(int sinkId)
        {
            if (_game == null)
            {
                return;
            }

            _game.TryBuySink(sinkId);
            RefreshSinkChrome(false);
        }

        private void TickSinkShop()
        {
            if (!_sinkOpen)
            {
                return;
            }

            if (GamepadInput.CancelPressed() || GamepadInput.PausePressed())
            {
                CloseSinkShop();
                return;
            }

            Vector2 sinkNav = GamepadInput.UiNavCombined();
            int sinkDx = HangarPadNav.DominantStep(sinkNav.x, sinkNav.y, HangarPadNav.Flick);
            int sinkDy = HangarPadNav.DominantStepY(sinkNav.x, sinkNav.y, HangarPadNav.Flick);
            if (sinkDx != 0 || sinkDy != 0)
            {
                float sinkNow = Time.unscaledTime;
                if (!_sinkHeld || sinkNow >= _sinkRepeatAt)
                {
                    _sinkFocus = SinkPadNav.Step(_sinkFocus, sinkDx, sinkDy);
                    _sinkRepeatAt = sinkNow + (_sinkHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
                    _sinkHeld = true;
                    FocusSinkSlot(_sinkFocus);
                }
            }
            else
            {
                _sinkHeld = false;
            }

            if (GamepadInput.ConfirmPressed() && _sinkOpenedFrame != Time.frameCount)
            {
                ActivateSinkSlot(_sinkFocus);
            }
        }

        private void FocusSinkSlot(int slot)
        {
            if (_sinkButtons == null)
            {
                return;
            }

            for (int sinkIndex = 0; sinkIndex < _sinkButtons.Length; sinkIndex++)
            {
                Button tile = _sinkButtons[sinkIndex];
                if (tile != null)
                {
                    UiTheme.SetPadFocus(tile.gameObject, sinkIndex == slot, true);
                }
            }

            if (_sinkClose != null)
            {
                UiTheme.SetPadFocus(_sinkClose.gameObject, slot == SinkPadNav.CloseSlot, false);
            }

            EventSystem sinkEvents = EventSystem.current;
            if (sinkEvents == null)
            {
                return;
            }

            Button sinkPick = SinkButton(slot);
            if (sinkPick != null)
            {
                sinkEvents.SetSelectedGameObject(sinkPick.gameObject);
            }
        }

        private Button SinkButton(int slot)
        {
            if (slot == SinkPadNav.CloseSlot)
            {
                return _sinkClose;
            }

            if (_sinkButtons == null || slot < 0 || slot >= _sinkButtons.Length)
            {
                return null;
            }

            return _sinkButtons[slot];
        }

        private void ActivateSinkSlot(int slot)
        {
            if (slot == SinkPadNav.CloseSlot)
            {
                CloseSinkShop();
                return;
            }

            OnSinkTile(slot);
        }

        private void HoldPreviewSpin(bool hold)
        {
            if (_ship == null)
            {
                return;
            }

            HangarShipPreview previewHold = _ship.GetComponent<HangarShipPreview>();
            if (previewHold != null)
            {
                previewHold.SetInteractionHold(hold);
            }
        }

        private struct ServiceRect
        {
            public Vector2 min;
            public Vector2 max;
        }

        private ServiceRect DefenseServiceRect(int row, float rowStep)
        {
            float top = ShopGridTop - row * rowStep;
            ServiceRect rect = new ServiceRect();
            rect.min = new Vector2(0.755f, top - ShopCellHeight);
            rect.max = new Vector2(0.98f, top);
            return rect;
        }

        private static void FitShopLabel(Text label, ShopGroup group)
        {
            if (label == null)
            {
                return;
            }

            int size = group == ShopGroup.Hull ? UiTheme.ShopHullSize : UiTheme.ShopNameSize;
            label.font = UiFonts.Body();
            label.fontSize = size;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = size;
            label.fontStyle = FontStyle.Normal;
            label.lineSpacing = UiTheme.ShopLineSpacing;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.color = UiTheme.Accent;
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
            Text header = CreateText("Group_" + label, _menuRoot.transform, font, UiTheme.ShopHeaderSize, TextAnchor.MiddleLeft, FontStyle.Bold);
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

        private void OnPrimaryClicked()
        {
            if (FirstStartOpen())
            {
                return;
            }

            if (_confirmOpen)
            {
                return;
            }

            if (FailedRetryPrimary())
            {
                OnOneMoreTry();
                return;
            }

            OnPrimary();
        }

        private bool FailedRetryPrimary()
        {
            return _session != null
                && FirstRunRules.OneMoreTryIsPrimary(_session.Phase)
                && (_game == null || !_game.HasContinueOffer);
        }

        private void OnOneMoreTry()
        {
            if (_confirmOpen || _settingsOpen || _creditsVisible || _game == null)
            {
                return;
            }

            DismissFirstHangarHint();
            DismissDoctrineIntro();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayRetry();
            }

            _game.OneMoreTry();
        }

        private void OnPrimary()
        {
            if (FirstStartOpen())
            {
                return;
            }

            if (_settingsOpen || _creditsVisible)
            {
                return;
            }

            if (_game == null)
            {
                return;
            }

            DismissFirstHangarHint();
            DismissDoctrineIntro();

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

            if (_game.HasContinueOffer)
            {
                _game.AcceptContinue();
                return;
            }

            _game.StartWave();
        }

        private void OnBuy(UpgradeId id)
        {
            if (_shop == null)
            {
                return;
            }

            bool mk2Offer = _loadout != null && _loadout.State != null
                && _loadout.State.Owns(id)
                && _loadout.State.CanBuyMk2(id);
            if (!mk2Offer
                && _loadout != null && _loadout.State != null
                && _loadout.State.Owns(id)
                && WeaponSlots.IsWeapon(id))
            {
                _shop.TryEquip(id);
                return;
            }

            _shop.TryBuy(id);
        }

        private void OnHullRepair()
        {
            if (_shop != null)
            {
                _shop.TryRepairHull();
            }
        }

        private void OnExtraLife()
        {
            if (_shop != null)
            {
                _shop.TryBuyExtraLife();
            }
        }

        private void OnShieldRefill()
        {
            if (_shop != null)
            {
                _shop.TryRefillShield();
            }
        }

        private void OnBankCredits()
        {
            if (_shop != null)
            {
                _shop.TryBankCredits();
            }
        }

        private void OnAbort()
        {
            if (_game != null)
            {
                _game.AbortWave();
            }
        }

        private void RequestAbort()
        {
            ConfirmRequest request = ReadConfirmRequest();
            request.AbortClick = true;
            request.Escape = false;
            request.Start = false;
            request.NewRunClick = false;
            request.Submit = false;
            request.Cancel = false;
            request.Scrim = false;
            request.FocusDelta = 0;
            ApplyConfirmRoute(ConfirmDialogRouter.Route(request), request);
            ApplyConfirmClock();
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
            // r1 headline. One line, left of the medal chip, above the stats row.
            Stretch(_summaryTitle.rectTransform, new Vector2(0.03f, 0.78f), new Vector2(0.70f, 0.96f));
            _summaryTitle.color = UiTheme.Primary;
            ClampOneLine(_summaryTitle);

            _summaryBody = CreateText("SummaryBody", _summaryRoot.transform, body, UiTheme.BodyMin, TextAnchor.MiddleCenter, FontStyle.Normal);
            // r2 SCORE · WAVE · WORLD · CREDITS (+delta). One line, above the upgrades row.
            Stretch(_summaryBody.rectTransform, new Vector2(0.03f, 0.58f), new Vector2(0.97f, 0.76f));
            _summaryBody.color = UiTheme.Accent;
            ClampOneLine(_summaryBody);

            _waveMedal = CreateText("WaveMedal", _summaryRoot.transform, display, 14, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_waveMedal.rectTransform, new Vector2(0.72f, 0.78f), new Vector2(0.97f, 0.96f));
            _waveMedal.color = UiTheme.Primary;
            ClampOneLine(_waveMedal);

            _continueHint = CreateText("ContinueHint", _summaryRoot.transform, body, UiTheme.ShopHeaderSize, TextAnchor.MiddleLeft, FontStyle.Normal);
            _continueHint.lineSpacing = UiTheme.ShopLineSpacing;
            // r3 upgrades or the run-over explanation. Wrap inside the card; do not cover NEXT WAVE.
            Stretch(_continueHint.rectTransform, new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.52f));
            _continueHint.color = UiTheme.Secondary;
            _continueHint.horizontalOverflow = HorizontalWrapMode.Wrap;
            _continueHint.verticalOverflow = VerticalWrapMode.Truncate;
            _summaryRecord = CreateText("SummaryRecord", _summaryRoot.transform, body, UiTheme.ShopHeaderSize, TextAnchor.MiddleLeft, FontStyle.Normal);
            Stretch(_summaryRecord.rectTransform, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.20f));
            _summaryRecord.color = UiTheme.Secondary;
            _summaryRecord.horizontalOverflow = HorizontalWrapMode.Wrap;
            _summaryRecord.verticalOverflow = VerticalWrapMode.Truncate;
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
            bool firstFlightOpen = _tutorialRoot != null && _tutorialRoot.activeSelf;
            _summaryRoot.SetActive(!playing && !firstFlightOpen);
            if (_credits != null)
            {
                _credits.gameObject.SetActive(false);
            }

            if (_status != null)
            {
                Stretch(_status.rectTransform, new Vector2(0.03f, 0.016f), new Vector2(0.97f, 0.088f));
            }

            int wave = _session.LastResolvedWave > 0 ? _session.LastResolvedWave : _session.WaveIndex;
            int world = WorldCatalog.NumberForWave(wave);
            LoadoutState loadout = _loadout != null ? _loadout.State : null;
            bool failed = _session.Phase == GamePhase.Failed;
            ApplyFailChrome(failed && summaryPhase);
            if (_session.Phase == GamePhase.CampaignClear && _summaryTitle != null)
            {
                _summaryTitle.color = UiTheme.Primary;
                _summaryTitle.fontSize = UiTheme.HeaderMin;
            }

            if (_session.Phase == GamePhase.Failed)
            {
                _summaryTitle.fontSize = UiTheme.HeaderMin;
                _summaryTitle.text = RunSummary.RunOverTitle(wave);
            }
            else if (_session.Phase == GamePhase.WaveClear && _session.WorldCleared > 0)
            {
                _summaryTitle.fontSize = UiTheme.HeaderMin;
                _summaryTitle.text = CampaignCap.SectorClearTitle(_session.WorldCleared);
            }
            else if (summaryPhase)
            {
                _summaryTitle.fontSize = UiTheme.HeaderMin;
                _summaryTitle.text = RunSummary.Title(_session.Phase, FailReasonText());
            }
            else
            {
                _summaryTitle.fontSize = UiTheme.BodyMin;
                _summaryTitle.color = UiTheme.Secondary;
                _summaryTitle.text = LegacyBalanceText();
            }

            string stats = RunSummary.StatsLine(_session.Score, wave, world)
                + "  ·  "
                + RunSummary.CreditsLine(_session.Credits, summaryPhase ? _session.LastCreditsAwarded : 0);
            if (failed && summaryPhase && _session != null && !string.IsNullOrEmpty(_session.DeathCard))
            {
                stats = _session.DeathCard.Replace("\n", "  ·  ") + "  ·  " + stats;
            }

            if (_session != null && _session.AssistUsed)
            {
                stats += "  ·  " + Loc.T("ui.hud.assist", "Assist");
            }

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

            _summaryBody.fontSize = failed && summaryPhase ? 18 : UiTheme.BodyMin;
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
            if (failed && summaryPhase)
            {
                row3 = RunSummary.ReachedLine(wave) + "  ·  " + RunSummary.RunOverExplain();
            }
            else if (summaryPhase)
            {
                if (_session.Phase == GamePhase.CampaignClear)
                {
                    row3 = RunSummary.CampaignWinHint() + "  ·  " + row3;
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
                _continueHint.horizontalOverflow = HorizontalWrapMode.Wrap;
                _continueHint.verticalOverflow = VerticalWrapMode.Truncate;
                bool ultraWide = Screen.width * 10 >= Screen.height * 21;
                if (summaryPhase && ultraWide)
                {
                    Stretch(_continueHint.rectTransform, new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.56f));
                    if (_summaryBody != null)
                    {
                        Stretch(_summaryBody.rectTransform, new Vector2(0.03f, 0.60f), new Vector2(0.97f, 0.76f));
                    }
                }
                else if (summaryPhase)
                {
                    Stretch(_continueHint.rectTransform, new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.52f));
                    if (_summaryBody != null)
                    {
                        Stretch(_summaryBody.rectTransform, new Vector2(0.03f, 0.58f), new Vector2(0.97f, 0.76f));
                    }
                }
                else
                {
                    Stretch(_continueHint.rectTransform, new Vector2(0.03f, 0.10f), new Vector2(0.97f, 0.34f));
                    if (_summaryBody != null)
                    {
                        Stretch(_summaryBody.rectTransform, new Vector2(0.03f, 0.58f), new Vector2(0.97f, 0.76f));
                    }
                }
            }

            if (_summaryRecord != null)
            {
                string recordLine = string.Empty;
                if (summaryPhase)
                {
                    LocalBest recordBest = _game != null ? _game.SessionBest : null;
                    if (recordBest != null)
                    {
                        recordLine = recordBest.DeathRetryLine(_session.LastRunScore);
                    }

                    if (_game != null && _game.LastRunWasNewBest)
                    {
                        if (recordLine.Length > 0)
                        {
                            recordLine += "  ·  ";
                        }

                        recordLine += Loc.T("ui.new_best", "NEW BEST");
                    }

                    string dailyCard = _game != null ? DailyCopy.Stamp(_game.ActiveDailyDate, _game.ActiveDailySeed) : string.Empty;
                    if (dailyCard.Length > 0)
                    {
                        if (recordLine.Length > 0)
                        {
                            recordLine += "  ·  ";
                        }

                        recordLine += dailyCard;
                    }

                    string mutatorCard = _game != null ? MutatorCopy.Hud(_game.ActiveMutatorMask) : string.Empty;
                    if (mutatorCard.Length > 0)
                    {
                        if (recordLine.Length > 0)
                        {
                            recordLine += "  ·  ";
                        }

                        recordLine += mutatorCard;
                    }
                }

                _summaryRecord.text = recordLine;
                _summaryRecord.gameObject.SetActive(summaryPhase && recordLine.Length > 0);
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
            FlashHit(strength, UiTheme.Danger, HitFlashLimiter.DefaultDecay);
        }

        public void FlashHit(float strength, Color color, float decay)
        {
            float alpha;
            float start;
            float until;
            float maxAlpha;
            float minGap;
            float usedDecay;
            EffectScale.FlashWindow(SettingsState.ReduceEffectsEnabled, decay, out maxAlpha, out minGap, out usedDecay);
            if (!HitFlashLimiter.TryBegin(
                Time.unscaledTime,
                _hitFlashStart,
                _hitFlashUntil,
                strength,
                usedDecay,
                maxAlpha,
                minGap,
                out alpha,
                out start,
                out until))
            {
                return;
            }

            _hitFlashStrength = alpha;
            _hitFlashStart = start;
            _hitFlashUntil = until;
            _hitFlashColor = color;
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
                _hitFlash.color = new Color(_hitFlashColor.r, _hitFlashColor.g, _hitFlashColor.b, 0f);
                _hitFlashStrength = 0f;
                return;
            }

            float span = _hitFlashUntil - _hitFlashStart;
            if (span < 0.05f)
            {
                span = 0.05f;
            }

            float pulse = Mathf.Clamp01((_hitFlashUntil - Time.unscaledTime) / span);
            _hitFlash.color = new Color(_hitFlashColor.r, _hitFlashColor.g, _hitFlashColor.b, _hitFlashStrength * pulse);
        }

        private void BuildFirstHangarHint(Font display, Font body)
        {
            _tutorialDismissed = PlayerPrefs.GetInt(FirstHangarHintKey, 0) == 1;
            _doctrineIntroDismissed = PlayerPrefs.GetInt(DoctrineHintKey, 0) == 1;
            // First-flight card sits in the wave-clear strip inside the hangar, above Start Wave.
            _tutorialRoot = UiTheme.BuildPanel(
                "FirstHangarHint",
                _menuRoot.transform,
                FirstFlightMin,
                FirstFlightMax,
                0.72f);

            _firstFlightTitle = CreateText("HintTitle", _tutorialRoot.transform, display, 16, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_firstFlightTitle.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.97f));
            _firstFlightTitle.color = UiTheme.Primary;
            _firstFlightTitle.text = "First flight";

            _firstFlightBody = CreateText("HintBody", _tutorialRoot.transform, body, UiTheme.HintSize(Screen.width), TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(_firstFlightBody.rectTransform, new Vector2(0.07f, 0.14f), new Vector2(0.93f, 0.85f));
            _firstFlightBody.color = UiTheme.Accent;
            PaintPrompt(_firstFlightBody, HangarHintBody);

            _gotItButton = CreateButton("DismissHint", _tutorialRoot.transform, display,
                new Vector2(0.12f, 0.02f), new Vector2(0.88f, 0.13f));
            _gotItLabel = _gotItButton.GetComponentInChildren<Text>();
            _gotItLabel.text = "Got it";
            _gotItLabel.fontSize = 15;
            _gotItButton.onClick.AddListener(OnDismissHintClicked);
            UiTheme.ApplyButton(_gotItButton, true, false, false);
            RaiseCanvas(_tutorialRoot, CanvasOrder.Overlay);
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
                && _session.WaveIndex == 1
                && !FirstStartOpen();
            _tutorialRoot.SetActive(firstHangar);
            ApplyHangarSkipLabel();
        }

        private void OnDismissHintClicked()
        {
            bool pending = _game != null && _game.TutorialPending;
            DismissFirstHangarHint();
            if (pending && _game != null)
            {
                _game.SkipTutorial();
            }
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
            RaiseCanvas(_endCreditsRoot, CanvasOrder.Overlay);
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
            _endCreditsBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _endCreditsBody.verticalOverflow = VerticalWrapMode.Overflow;
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
            // Top bar, just left of the gear. Language lives in the settings panel.
            _diffPanel = UiTheme.BuildPanel(
                "DifficultyPanel",
                transform,
                DifficultyMin,
                DifficultyMax,
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
            AnnounceLifeLost(livesLeft, string.Empty);
        }

        public void AnnounceLifeLost(int livesLeft, string deathCard)
        {
            string lives = Loc.Tf("ui.life_lost", "LIFE LOST  ·  {0} left", livesLeft);
            if (!string.IsNullOrEmpty(deathCard))
            {
                lives = deathCard.Replace("\n", "  ·  ") + "  ·  " + lives;
            }
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
                _abortLabel.text = Loc.T("ui.abort", "Abort > Hangar");
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
                PaintPrompt(_firstFlightBody, Loc.T("ui.hangar_hint_body", HangarHintBody));
            }

            if (_gotItLabel != null)
            {
                _gotItLabel.text = Loc.T("ui.got_it", "Got it");
            }

            ApplyHangarSkipLabel();

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

            if (_settingsRebindLabel != null)
            {
                _settingsRebindLabel.text = Loc.T("ui.settings.rebind", "Change controls");
            }

            if (_settingsControlsBody != null)
            {
                PaintPrompt(_settingsControlsBody, FullControlHint());
            }

            RefreshSettingsPrompt();

            RefreshSettingsAudio();
            RefreshSettingsShake();
            RefreshSettingsReduce();
            RefreshSettingsAssist();
            RefreshSettingsHint();
            RefreshSettingsConfirm();
            RefreshSettingsPadNav();
            RefreshSettingsDisplay();
            RefreshConfirmCopy();

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

        private void ApplyPrimaryCaption(GamePhase phase, int worldCleared, int upcomingWave)
        {
            if (_primaryLabel == null)
            {
                return;
            }

            _primaryLabel.fontSize = RunSummary.PrimaryFont;
            _primaryLabel.fontStyle = FontStyle.Normal;
            if (_game != null && _game.HasContinueOffer && _game.PendingContinue != null)
            {
                RunSaveData pending = _game.PendingContinue;
                int savedWorld = WorldCatalog.NumberForWave(pending.WaveIndex);
                _primaryLabel.text = RunSummary.ContinueRunLabel(savedWorld, pending.WaveIndex);
                _primaryLabel.lineSpacing = 1f;
                _primaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                _primaryLabel.verticalOverflow = VerticalWrapMode.Truncate;
                return;
            }

            int nextNumber = WorldCatalog.NumberForWave(upcomingWave);
            string actionLabel = RunSummary.PrimaryActionLabel(phase, worldCleared, nextNumber);
            string nextSubtitle = string.Empty;
            if (phase == GamePhase.WaveClear && worldCleared > 0)
            {
                string withLoop = RunSummary.ContinueSubtitle(upcomingWave);
                string ruleLine = RunSummary.ContinueRuleLine(upcomingWave);
                string loopAndRule = string.IsNullOrEmpty(ruleLine) ? withLoop : withLoop + "  ·  " + ruleLine;
                if (!string.IsNullOrEmpty(ruleLine) && RunSummary.PrimarySubtitleFits(actionLabel, loopAndRule))
                {
                    nextSubtitle = loopAndRule;
                }
                else if (RunSummary.PrimarySubtitleFits(actionLabel, withLoop))
                {
                    nextSubtitle = withLoop;
                }
                else
                {
                    string nameOnly = WorldCatalog.ContinueSubtitle(upcomingWave);
                    string nameAndRule = string.IsNullOrEmpty(ruleLine) ? nameOnly : nameOnly + "  ·  " + ruleLine;
                    if (!string.IsNullOrEmpty(ruleLine) && RunSummary.PrimarySubtitleFits(actionLabel, nameAndRule))
                    {
                        nextSubtitle = nameAndRule;
                    }
                    else if (RunSummary.PrimarySubtitleFits(actionLabel, nameOnly))
                    {
                        nextSubtitle = nameOnly;
                    }
                }
            }

            if (!string.IsNullOrEmpty(nextSubtitle))
            {
                _primaryLabel.text = actionLabel + "\n" + nextSubtitle;
                _primaryLabel.lineSpacing = 0.8f;
            }
            else
            {
                _primaryLabel.text = actionLabel;
                _primaryLabel.lineSpacing = 1f;
            }

            _primaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _primaryLabel.verticalOverflow = VerticalWrapMode.Truncate;
            if (phase == GamePhase.Failed && (_game == null || !_game.HasContinueOffer))
            {
                _primaryLabel.fontSize = 28;
                _primaryLabel.fontStyle = FontStyle.Bold;
            }
        }

        private bool BoonModalOpen()
        {
            return _game != null
                && _game.BoonChoiceOpen
                && (_session == null || _session.Phase != GamePhase.Playing);
        }

        private void BuildBoonModal(Font display, Font body)
        {
            EnsureBoonModalCanvas();
            _boonRoot = new GameObject("BoonRoot");
            _boonRoot.transform.SetParent(_boonCanvas.transform, false);
            Stretch(_boonRoot.AddComponent<RectTransform>(), Vector2.zero, Vector2.one);

            GameObject boonScrim = CreateFill(
                "BoonScrim",
                _boonRoot.transform,
                UiTheme.WithAlpha(UiTheme.Void, 0.78f),
                Vector2.zero,
                Vector2.one);
            Image boonScrimImage = boonScrim.GetComponent<Image>();
            if (boonScrimImage != null)
            {
                boonScrimImage.raycastTarget = true;
            }

            GameObject boonPanel = UiTheme.BuildPanel(
                "BoonPanel",
                _boonRoot.transform,
                new Vector2(BoonCardLayout.PanelMinX, BoonCardLayout.PanelMinY),
                new Vector2(BoonCardLayout.PanelMaxX, BoonCardLayout.PanelMaxY),
                BoonCardLayout.HeaderMinY);
            Image boonPanelImage = boonPanel.GetComponent<Image>();
            if (boonPanelImage != null)
            {
                boonPanelImage.raycastTarget = true;
            }

            _boonTitle = CreateText("BoonTitle", boonPanel.transform, display, BoonCardLayout.TitleFont, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(
                _boonTitle.rectTransform,
                new Vector2(BoonCardLayout.TitleMinX, BoonCardLayout.TitleMinY),
                new Vector2(BoonCardLayout.TitleMaxX, BoonCardLayout.TitleMaxY));
            _boonTitle.color = UiTheme.Primary;
            _boonTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            _boonTitle.verticalOverflow = VerticalWrapMode.Truncate;
            _boonTitle.raycastTarget = false;
            _boonTitle.text = Loc.T("boon.pick", "Choose 1 bonus");

            _boonCards = new Button[BoonCatalog.OfferCount];
            _boonCardLabels = new Text[BoonCatalog.OfferCount];
            _boonCardPlates = new Image[BoonCatalog.OfferCount];
            for (int boonSlot = 0; boonSlot < BoonCatalog.OfferCount; boonSlot++)
            {
                int boonPick = boonSlot;
                float cardMinX;
                float cardMinY;
                float cardMaxX;
                float cardMaxY;
                BoonCardLayout.CardAnchors(boonPick, out cardMinX, out cardMinY, out cardMaxX, out cardMaxY);
                Button boonCard = CreateButton(
                    "BoonCard" + boonPick,
                    boonPanel.transform,
                    body,
                    new Vector2(cardMinX, cardMinY),
                    new Vector2(cardMaxX, cardMaxY));
                Text boonCardLabel = boonCard.GetComponentInChildren<Text>();
                boonCardLabel.fontSize = BoonCardLayout.CardFont;
                boonCardLabel.alignment = TextAnchor.MiddleCenter;
                boonCardLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                boonCardLabel.verticalOverflow = VerticalWrapMode.Truncate;
                boonCardLabel.raycastTarget = false;
                Stretch(
                    boonCardLabel.rectTransform,
                    new Vector2(BoonCardLayout.LabelMinX, BoonCardLayout.LabelMinY),
                    new Vector2(BoonCardLayout.LabelMaxX, BoonCardLayout.LabelMaxY));
                boonCard.onClick.AddListener(() => ChooseBoon(boonPick));
                LockButtonNavigation(boonCard);
                Image boonPlate = boonCard.targetGraphic as Image;
                if (boonPlate != null)
                {
                    boonPlate.raycastTarget = true;
                }

                _boonCards[boonPick] = boonCard;
                _boonCardLabels[boonPick] = boonCardLabel;
                _boonCardPlates[boonPick] = boonPlate;
            }

            _boonHint = CreateText("BoonHint", boonPanel.transform, body, BoonCardLayout.HintFont, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(
                _boonHint.rectTransform,
                new Vector2(BoonCardLayout.HintMinX, BoonCardLayout.HintMinY),
                new Vector2(BoonCardLayout.HintMaxX, BoonCardLayout.HintMaxY));
            _boonHint.color = UiTheme.Secondary;
            _boonHint.horizontalOverflow = HorizontalWrapMode.Wrap;
            _boonHint.verticalOverflow = VerticalWrapMode.Truncate;
            _boonHint.raycastTarget = false;
            PaintPrompt(_boonHint, Loc.T("boon.pad", "{confirm} select  ·  {nav} moves"));
            _boonCanvas.SetActive(false);
            _boonRoot.SetActive(false);
            RaiseBoonModal();
        }

        private void RefreshBoonChrome()
        {
            bool boonOpen = BoonModalOpen();
            if (_boonCanvas != null && _boonCanvas.activeSelf != boonOpen)
            {
                _boonCanvas.SetActive(boonOpen);
            }

            if (_boonRoot != null)
            {
                if (_boonRoot.activeSelf != boonOpen)
                {
                    _boonRoot.SetActive(boonOpen);
                }

                if (boonOpen)
                {
                    RaiseBoonModal();
                    _boonRoot.transform.SetAsLastSibling();
                    if (!_boonShown)
                    {
                        _boonFocus = 0;
                        _boonNavHeld = false;
                    }

                    _boonShown = true;
                    PaintBoonCards();
                    FocusBoonCard(_boonFocus);
                }
                else
                {
                    _boonShown = false;
                }
            }

            PaintBoonRow();
        }

        private void PaintBoonCards()
        {
            if (_boonCards == null || _game == null)
            {
                return;
            }

            BoonRun boonRun = _game.Boons;
            for (int boonSlot = 0; boonSlot < _boonCards.Length; boonSlot++)
            {
                int offerId = boonRun != null ? boonRun.OfferAt(boonSlot) : -1;
                int ownedLevel = 0;
                if (boonRun != null && boonRun.Levels != null && offerId >= 0 && offerId < boonRun.Levels.Length)
                {
                    ownedLevel = boonRun.Levels[offerId];
                }

                if (_boonCardLabels != null && boonSlot < _boonCardLabels.Length && _boonCardLabels[boonSlot] != null)
                {
                    string boonCopy = offerId < 0
                        ? string.Empty
                        : BoonCatalog.CardLine(offerId, ownedLevel);
                    _boonCardLabels[boonSlot].text = boonCopy;
                    float boonBoxW = BoonCardLayout.CardInnerCanvasWidth(Screen.width, Screen.height);
                    float boonBoxH = BoonCardLayout.CardInnerCanvasHeight(Screen.width, Screen.height);
                    int boonFont = BoonCardLayout.FitFont(boonBoxW, boonBoxH, boonCopy, BoonCardLayout.CardFont);
                    _boonCardLabels[boonSlot].fontSize = boonFont;
                    _boonCardLabels[boonSlot].horizontalOverflow = HorizontalWrapMode.Wrap;
                    _boonCardLabels[boonSlot].verticalOverflow = VerticalWrapMode.Truncate;
                    _boonCardLabels[boonSlot].raycastTarget = false;
                }
            }

            PaintBoonFocus();
        }

        private void PaintBoonFocus()
        {
            if (_boonCardPlates == null)
            {
                return;
            }

            for (int boonSlot = 0; boonSlot < _boonCardPlates.Length; boonSlot++)
            {
                Image boonPlate = _boonCardPlates[boonSlot];
                if (boonPlate == null)
                {
                    continue;
                }

                bool boonPicked = boonSlot == _boonFocus;
                boonPlate.color = boonPicked ? UiTheme.Focus : UiTheme.Surface2;
                boonPlate.raycastTarget = true;
                UiTheme.SetPadFocus(boonPlate.gameObject, boonPicked, false);
                if (_boonCardLabels != null && boonSlot < _boonCardLabels.Length && _boonCardLabels[boonSlot] != null)
                {
                    _boonCardLabels[boonSlot].color = boonPicked ? UiTheme.Void : UiTheme.Accent;
                    _boonCardLabels[boonSlot].raycastTarget = false;
                }
            }
        }

        private void FocusBoonCard(int slot)
        {
            if (_boonCards == null || slot < 0 || slot >= _boonCards.Length || _boonCards[slot] == null)
            {
                return;
            }

            EventSystem boonFocus = EventSystem.current;
            if (boonFocus == null)
            {
                return;
            }

            boonFocus.SetSelectedGameObject(_boonCards[slot].gameObject);
        }

        private void PaintBoonRow()
        {
            if (_boonRow == null)
            {
                return;
            }

            int[] boonLevels = _game != null && _game.Boons != null ? _game.Boons.Levels : null;
            string boonLine = RunSummary.BoonLine(boonLevels);
            _boonRow.text = boonLine;
            _boonRow.gameObject.SetActive(!string.IsNullOrEmpty(boonLine));
        }

        private void TickBoonChoice()
        {
            if (_boonCards == null || _boonCards.Length == 0)
            {
                return;
            }

            Vector2 boonNav = GamepadInput.UiNavCombined(PadNavSource.Both);
            int boonDx = HangarPadNav.DominantStep(boonNav.x, boonNav.y, HangarPadNav.Flick);
            if (boonDx == 0)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    boonDx = -1;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    boonDx = 1;
                }
            }

            if (boonDx != 0)
            {
                float boonNow = Time.unscaledTime;
                if (!_boonNavHeld || boonNow >= _boonRepeatAt)
                {
                    _boonFocus = BoonPadNav.Step(_boonFocus, boonDx, _boonCards.Length);
                    _boonRepeatAt = boonNow + (_boonNavHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
                    _boonNavHeld = true;
                    FocusBoonCard(_boonFocus);
                    PaintBoonFocus();
                }
            }
            else
            {
                _boonNavHeld = false;
            }

            bool boonConfirm = GamepadInput.ConfirmPressed()
                || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.KeypadEnter);
            if (boonConfirm)
            {
                ChooseBoon(_boonFocus);
            }

            EventSystem boonEs = EventSystem.current;
            if (boonEs == null)
            {
                return;
            }

            GameObject boonSelected = boonEs.currentSelectedGameObject;
            bool boonLost = boonSelected == null;
            if (!boonLost)
            {
                boonLost = true;
                for (int boonCardIndex = 0; boonCardIndex < _boonCards.Length; boonCardIndex++)
                {
                    if (_boonCards[boonCardIndex] != null && boonSelected == _boonCards[boonCardIndex].gameObject)
                    {
                        boonLost = false;
                        _boonFocus = boonCardIndex;
                        break;
                    }
                }
            }

            if (boonLost)
            {
                FocusBoonCard(_boonFocus);
            }
        }

        private void ChooseBoon(int index)
        {
            if (_game == null)
            {
                return;
            }

            if (_game.TryChooseBoon(index) && AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        public void AnnounceEliteWave(int waveIndex)
        {
            string eliteLine = WaveModifier.Banner(waveIndex);
            if (string.IsNullOrEmpty(eliteLine) || _world == null)
            {
                return;
            }

            _eliteBanner = eliteLine;
            _medalBeat = string.Empty;
            _flashedWave = waveIndex < 1 ? 1 : waveIndex;
            _worldFlashUntil = Time.unscaledTime + 2.4f;
            _world.horizontalOverflow = HorizontalWrapMode.Wrap;
            _world.verticalOverflow = VerticalWrapMode.Overflow;
            Stretch(_world.rectTransform, new Vector2(0.42f, 0.80f), new Vector2(0.97f, 0.895f));
            _world.text = _eliteBanner;
        }

        public void AnnounceWorldChange(int waveIndex)
        {
            _eliteBanner = string.Empty;
            int flashedWave = waveIndex < 1 ? 1 : waveIndex;
            _flashedWave = flashedWave;
            _flashedWorld = ArenaLayout.WorldIndexForWave(flashedWave);
            _flashedLayout = ArenaLayout.Title(ArenaLayout.ForWorld(_flashedWorld));
            _flashedBadge = ArenaLayout.Badge(ArenaLayout.ForWorld(_flashedWorld));
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
            NoteControlDevice();
            TickTutorialPrompts();
            if (_settingsOpen && _session != null && _session.Phase == GamePhase.Playing)
            {
                DismissSettingsForPlay();
            }

            // Confirm dialog consumes Esc, Start, B, and A before settings,
            // abort, New Run, and hangar navigation. A click that opened the
            // dialog this frame must not also cancel it.
            ApplyConfirmClock();
            if (BoonModalOpen())
            {
                TickBoonChoice();
            }
            else
            {
            bool skipLiveTutorial = ConsumeTutorialSkip();
            bool holdFirstStart = FirstStartOpen();
            if (!skipLiveTutorial && !holdFirstStart)
            {
            ConfirmAction confirmAction = ConfirmAction.None;
            if (_confirmOpenedFrame != Time.frameCount)
            {
                ConfirmRequest confirmRequest = ReadConfirmRequest();
                confirmAction = ConfirmDialogRouter.Route(confirmRequest);
                if (confirmAction != ConfirmAction.None)
                {
                    ApplyConfirmRoute(confirmAction, confirmRequest);
                }
            }

            ApplyConfirmClock();

            // Settings panel, then credits, then hangar. An open panel consumes
            // Esc, Start, B, and Submit so they cannot reach Next Wave.
            // In-wave Esc/Start is handled by the confirm router above.
            if (!_confirmOpen && confirmAction == ConfirmAction.None)
            {
                if (RebindConsumesInput())
                {
                    TickRebind();
                }
                else if (_setupOpen)
                {
                    TickRunSetup();
                }
                else if (_sinkOpen)
                {
                    TickSinkShop();
                }
                else
                {
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

                        if (settingsRoute != SettingsRoute.Move && !RebindConsumesInput())
                        {
                            TickSettingsWheel();
                        }
                    }
                }
                else
                {
                    NavigateHangarPad();
                    SyncHangarPadSelection();
                }
                }
            }
            }
            if (holdFirstStart)
            {
                ConsumeFirstStartInput();
                NavigateHangarPad();
                SyncHangarPadSelection();
            }
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
                RefreshBossBar();
                RefreshUtilityHud(true);
                ApplyBottomHint(true);
            }

            PulseHangarLaunch();
            PulseRecommendedUpgrade();
            PulseAbortIfUrgent();
            ApplyHitFlash();
            PulseAchievementToast();

            if (_world == null)
            {
                return;
            }

            if (Time.unscaledTime < _worldFlashUntil)
            {
                if (!string.IsNullOrEmpty(_eliteBanner))
                {
                    float elitePulse = EffectScale.UiPulse(
                        SettingsState.ReduceEffectsEnabled,
                        Mathf.PingPong(Time.unscaledTime * 3.2f, 1f));
                    if (EffectScale.FreezePulse(SettingsState.ReduceEffectsEnabled))
                    {
                        elitePulse = 0f;
                    }
                    _world.fontSize = 22 + (int)(4f * elitePulse);
                    _world.color = Color.Lerp(UiTheme.Danger, UiTheme.Brighten(UiTheme.Danger, 0.18f), elitePulse);
                    _world.text = _eliteBanner;
                    return;
                }

                float pulse = Mathf.PingPong(Time.unscaledTime * 3.2f, 1f);
                if (EffectScale.FreezePulse(SettingsState.ReduceEffectsEnabled))
                {
                    pulse = 0f;
                }

                _world.fontSize = 22 + (int)(4f * pulse);
                bool world3 = _flashedWorld == MedalCatalog.World3EntryWorld;
                Color flashTone = world3 ? UiTheme.Secondary : UiTheme.Primary;
                _world.color = Color.Lerp(flashTone, UiTheme.Brighten(flashTone, 0.18f), pulse);
                ArenaLayoutId flashId = ArenaLayout.ForWorld(_flashedWorld);
                _flashedLayout = ArenaLayout.Title(flashId);
                _flashedBadge = ArenaLayout.Badge(flashId);
                string flash = Loc.Tf(
                    "ui.layout_swap",
                    "LAYOUT SWAP\nWORLD {0}  ONLINE",
                    WorldCatalog.NumberForWave(_flashedWave));
                string intro = WorldCatalog.IntroBanner(_flashedWave);
                if (string.IsNullOrEmpty(intro))
                {
                    intro = _flashedBadge + "  ·  " + _flashedLayout;
                }

                flash += "\n" + intro;

                if (!string.IsNullOrEmpty(_medalBeat))
                {
                    flash += "\n" + _medalBeat;
                }

                _world.text = flash;
                return;
            }

            if (_world.fontSize != 22 || !string.IsNullOrEmpty(_medalBeat) || !string.IsNullOrEmpty(_eliteBanner))
            {
                _world.fontSize = 22;
                _medalBeat = string.Empty;
                _eliteBanner = string.Empty;
                _world.horizontalOverflow = HorizontalWrapMode.Overflow;
                _world.verticalOverflow = VerticalWrapMode.Truncate;
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
            bool retry = _session.Phase == GamePhase.Failed;
            if (!first && !retry)
            {
                _primaryPlate.color = UiTheme.PrimaryCta;
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 2.4f, 1f);
            if (EffectScale.FreezePulse(SettingsState.ReduceEffectsEnabled))
            {
                pulse = 0f;
            }

            _primaryPlate.color = Color.Lerp(UiTheme.PrimaryCta, UiTheme.Brighten(UiTheme.Primary, 0.12f), pulse);
        }

        private void PulseAbortIfUrgent()
        {
            if (_abortPlate == null || !_abortUrgent || _session == null || _session.Phase != GamePhase.Playing)
            {
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 4.2f, 1f);
            if (EffectScale.FreezePulse(SettingsState.ReduceEffectsEnabled))
            {
                pulse = 0f;
            }

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
            if (EffectScale.FreezePulse(SettingsState.ReduceEffectsEnabled))
            {
                pulse = 0f;
            }

            _achievementToast.color = Color.Lerp(UiTheme.Primary, UiTheme.Focus, pulse);
        }

        private void OnHangarEscape()
        {
            DismissHangarHints();
            FocusHangarSlot(HangarPadNav.PrimarySlot);
        }

        private void OnHangarStart()
        {
            if (FirstStartOpen())
            {
                return;
            }

            if (FailedRetryPrimary())
            {
                OnOneMoreTry();
                return;
            }

            OnPrimary();
        }

        private void OnHangarBack()
        {
            DismissHangarHints();
            FocusHangarSlot(HangarPadNav.PrimarySlot);
        }

        private void DismissHangarHints()
        {
            bool keepSkip = _game != null && _game.TutorialPending;
            if (!keepSkip && _tutorialRoot != null && _tutorialRoot.activeSelf)
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

            if (FirstStartOpen())
            {
                _primary.interactable = false;
                Image hiddenPlate = _primary.targetGraphic as Image;
                if (hiddenPlate != null)
                {
                    hiddenPlate.raycastTarget = false;
                }

                if (_firstStartRoot != null)
                {
                    _firstStartRoot.transform.SetAsLastSibling();
                }

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

            if (FirstStartOpen())
            {
                FocusHangarSlot(HangarPadNav.FirstNormalSlot);
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

            if (!FirstStartOpen())
            {
                HangarPadNav.ForcePrimarySelectable(_padSelectable);
            }

            return _padSelectable;
        }

        private bool SlotIsSelectable(int slot)
        {
            if (FirstStartOpen())
            {
                return FirstStartSlotLive(slot);
            }

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

            bool restrictOverlay = FirstStartOpen();
            if (restrictOverlay)
            {
                _padSlot = HangarPadNav.StepOverlay(fromSlot, dx, dy, padMask);
            }
            else
            {
                _padSlot = HangarPadNav.StepSelectable(fromSlot, dx, dy, padMask);
            }
            Button stepped = ButtonFromSlot(_padSlot);
            if (stepped != null && stepped.gameObject.activeInHierarchy && stepped.IsInteractable())
            {
                es.SetSelectedGameObject(stepped.gameObject);
            }
            else if (FirstStartOpen())
            {
                FocusHangarSlot(HangarPadNav.OverlayHome(padMask));
            }
            else
            {
                FocusHangarSlot(HangarPadNav.PrimarySlot);
            }

            _padRepeatAt = now + (_padHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
            _padHeld = true;
            SyncFirstStartPick();
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

            if (slot == HangarPadNav.NewRunSlot)
            {
                return ButtonIfActive(_newRunButton);
            }

            if (slot == HangarPadNav.HullRepairSlot)
            {
                return ButtonIfActive(_hullRepairButton);
            }

            if (slot == HangarPadNav.ExtraLifeSlot)
            {
                return ButtonIfActive(_extraLifeButton);
            }

            if (slot == HangarPadNav.ShieldRefillSlot)
            {
                return ButtonIfActive(_shieldRefillButton);
            }

            if (slot == HangarPadNav.BankSlot)
            {
                return ButtonIfActive(_bankButton);
            }

            if (slot == HangarPadNav.SinkSlot)
            {
                return ButtonIfActive(_sinkEntry);
            }

            if (slot == HangarPadNav.TutorialSkipSlot)
            {
                return ButtonIfActive(_gotItButton);
            }

            if (slot == HangarPadNav.FirstEasySlot)
            {
                return ButtonIfActive(_firstEasy);
            }

            if (slot == HangarPadNav.FirstNormalSlot)
            {
                return ButtonIfActive(_firstNormal);
            }

            if (slot == HangarPadNav.FirstHardSlot)
            {
                return ButtonIfActive(_firstHard);
            }

            if (slot == HangarPadNav.FirstGoSlot)
            {
                return ButtonIfActive(_firstGo);
            }

            if (slot == HangarPadNav.FirstSkipSlot)
            {
                return ButtonIfActive(_firstSkip);
            }

            if (slot >= HangarPadNav.LegacySlot0 && slot < HangarPadNav.LegacySlot0 + HangarPadNav.LegacyPerkSlots)
            {
                int legacyIndex = slot - HangarPadNav.LegacySlot0;
                if (_legacyButtons != null && legacyIndex >= 0 && legacyIndex < _legacyButtons.Length)
                {
                    return ButtonIfActive(_legacyButtons[legacyIndex]);
                }

                return null;
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

            if (_gotItButton != null && go == _gotItButton.gameObject)
            {
                if (_game != null && _game.TutorialPending)
                {
                    return HangarPadNav.TutorialSkipSlot;
                }

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

            if (_newRunButton != null && go == _newRunButton.gameObject)
            {
                return HangarPadNav.NewRunSlot;
            }

            if (_hullRepairButton != null && go == _hullRepairButton.gameObject)
            {
                return HangarPadNav.HullRepairSlot;
            }

            if (_extraLifeButton != null && go == _extraLifeButton.gameObject)
            {
                return HangarPadNav.ExtraLifeSlot;
            }

            if (_shieldRefillButton != null && go == _shieldRefillButton.gameObject)
            {
                return HangarPadNav.ShieldRefillSlot;
            }

            if (_bankButton != null && go == _bankButton.gameObject)
            {
                return HangarPadNav.BankSlot;
            }

            if (_sinkEntry != null && go == _sinkEntry.gameObject)
            {
                return HangarPadNav.SinkSlot;
            }

            if (_firstEasy != null && go == _firstEasy.gameObject)
            {
                return HangarPadNav.FirstEasySlot;
            }

            if (_firstNormal != null && go == _firstNormal.gameObject)
            {
                return HangarPadNav.FirstNormalSlot;
            }

            if (_firstHard != null && go == _firstHard.gameObject)
            {
                return HangarPadNav.FirstHardSlot;
            }

            if (_firstGo != null && go == _firstGo.gameObject)
            {
                return HangarPadNav.FirstGoSlot;
            }

            if (_firstSkip != null && go == _firstSkip.gameObject)
            {
                return HangarPadNav.FirstSkipSlot;
            }

            if (_legacyButtons != null)
            {
                for (int legacyIndex = 0; legacyIndex < _legacyButtons.Length; legacyIndex++)
                {
                    Button legacyButton = _legacyButtons[legacyIndex];
                    if (legacyButton != null && go == legacyButton.gameObject)
                    {
                        return HangarPadNav.LegacySlot0 + legacyIndex;
                    }
                }
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

            if (FirstStartOpen())
            {
                Button firstNormal = ButtonFromSlot(HangarPadNav.FirstNormalSlot);
                if (firstNormal != null)
                {
                    return firstNormal;
                }
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

            bool shop = go.name.StartsWith("Buy_") || go.name == "SinkBay" || go.name.StartsWith("SinkTile");
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

            int shownWorld = WorldCatalog.NumberForWave(_session.WaveIndex);
            int layoutWorld = ArenaLayout.WorldIndexForWave(_session.WaveIndex);
            _world.text = Loc.Tf(
                "ui.world_badge",
                "WORLD {0}  ·  {1}",
                shownWorld,
                ArenaLayout.Badge(ArenaLayout.ForWorld(layoutWorld)));
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
                HangarPersist medalPersist = _game != null && _game.Persist != null
                    ? _game.Persist
                    : HangarPersist.Load();
                int medalMask = medalPersist != null ? medalPersist.MedalMask : 0;
                string medalBoard = MedalCatalog.WorldClearBoard(medalMask);
                string achBlock = header + "  " + count + "\n" + medalBoard;
                if (!SettingsMeasure.LadderBlockFits(achBlock, Screen.width, Screen.height))
                {
                    achBlock = header + "\n" + count;
                }

                _achievementLadder.text = achBlock;
                _achievementLadder.lineSpacing = SettingsMeasure.LadderLineSpacing;
                _achievementLadder.horizontalOverflow = HorizontalWrapMode.Wrap;
                _achievementLadder.verticalOverflow = VerticalWrapMode.Truncate;
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

            int shopWorld = _session != null ? WorldCatalog.NumberForWave(_session.WaveIndex) : 1;
            bool owned = _loadout.State.Owns(item.Id);
            bool mk2Offer = owned && _loadout.State.CanBuyMk2(item.Id);
            int price = mk2Offer
                ? _loadout.State.Mk2Price(item, shopWorld)
                : _loadout.State.EffectiveCost(item, shopWorld);
            bool runOver = GameSession.ShopLockedForPhase(_session.Phase);
            bool canApply = _loadout.State.CanApply(item.Id);
            bool weapon = WeaponSlots.IsWeapon(item.Id);
            bool equipped = owned && weapon && _loadout.State.IsEquipped(item.Id);
            bool offPath = _loadout.State.IsOffPath(item.Id);
            EventSystem es = EventSystem.current;
            bool focused = es != null
                && es.currentSelectedGameObject != null
                && es.currentSelectedGameObject == _buyButtons[index].gameObject;
            ShopTileInput spec = new ShopTileInput();
            spec.Title = item.Title;
            spec.Price = price;
            spec.Credits = _session != null ? _session.Credits : 0;
            spec.Owned = owned;
            spec.Mk2Owned = _loadout.State.OwnsMk2(item.Id);
            spec.Mk2Offer = mk2Offer;
            spec.CanApply = canApply;
            spec.OffPath = offPath;
            spec.Weapon = weapon;
            spec.Equipped = equipped;
            spec.RunOver = runOver;
            spec.ShopOpen = _session != null && _session.ShopOpen;
            spec.Focused = focused;
            spec.Swedish = Loc.IsSwedish;
            ShopTileModel tileModel = ShopTileView.Build(spec);
            Image plate = _buyButtons[index].targetGraphic as Image;
            UiTheme.ApplyShopHex(plate, _buyLabels[index], tileModel.FillHex, tileModel.TextHex);
            _buyButtons[index].interactable = tileModel.Interactable;
            _buyLabels[index].enabled = true;
            _buyLabels[index].text = tileModel.Label;
            UiTheme.PlaceShopFocus(_buyButtons[index].gameObject, _buyLabels[index], tileModel.Focused);
        }

        private void RefreshServiceButtons()
        {
            int serviceWorld = _session != null ? WorldCatalog.NumberForWave(_session.WaveIndex) : 1;
            bool runOver = _session == null || GameSession.ShopLockedForPhase(_session.Phase) || !_session.ShopOpen;
            int repairPrice = ShopPrices.ApplyWorld(ShopPrices.HullRepairCost, serviceWorld);
            int lifePrice = ShopPrices.ApplyWorld(ShopPrices.ExtraLifeCost, serviceWorld);
            int refillPrice = ShopPrices.ApplyWorld(ShopPrices.ShieldRefillCost, serviceWorld);
            bool hullHurt = _game != null && _game.PlayerHealth != null && _game.PlayerHealth.Hull > 0 && _game.PlayerHealth.Hull < _game.PlayerHealth.MaxHull;
            bool shieldLow = _game != null && _game.PlayerHealth != null && _game.PlayerHealth.MaxShield > 0 && _game.PlayerHealth.Shield < _game.PlayerHealth.MaxShield;
            bool lifeOk = _session != null && _session.CanBuyExtraLife(serviceWorld);
            bool bankOk = _session != null && _session.CanBankCredits();
            PaintService(_hullRepairButton, _hullRepairLabel, Loc.T("shop.hull_repair", "Hull repair"), repairPrice, !runOver && hullHurt && _session.Credits >= repairPrice);
            PaintService(_extraLifeButton, _extraLifeLabel, Loc.T("shop.extra_life", "Extra life"), lifePrice, !runOver && lifeOk && _session.Credits >= lifePrice);
            PaintService(_shieldRefillButton, _shieldRefillLabel, Loc.T("shop.shield_refill", "Shield refill"), refillPrice, !runOver && shieldLow && _session.Credits >= refillPrice);
            PaintService(_bankButton, _bankLabel, Loc.T("shop.bank", "Bank credits"), ShopPrices.BankCreditsPerPoint, !runOver && bankOk);
        }

        private static void PaintService(Button button, Text label, string title, int price, bool canBuy)
        {
            if (button == null || label == null)
            {
                return;
            }

            button.interactable = canBuy;
            label.text = title + "\n" + Loc.Tf("ui.cost_cr", "{0} cr", price);
            Image plate = button.targetGraphic as Image;
            UiTheme.PaintShopPlate(plate, label, false, !canBuy, !canBuy);
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
            string dailyHud = _game != null ? DailyCopy.Stamp(_game.ActiveDailyDate, _game.ActiveDailySeed) : string.Empty;
            if (dailyHud.Length > 0)
            {
                scoreLine += "\n" + dailyHud;
            }

            string mutatorHud = _game != null ? MutatorCopy.Hud(_game.ActiveMutatorMask) : string.Empty;
            if (mutatorHud.Length > 0)
            {
                scoreLine += "\n" + mutatorHud;
            }

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
            if (SettingsState.AssistEnabled || (_session != null && _session.AssistUsed))
            {
                livesLine += "  ·  " + Loc.T("ui.hud.assist", "Assist");
            }
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

            int shownWorld = WorldCatalog.NumberForWave(_session.WaveIndex);
            return best.PlayCompare(_session.Score, _session.WaveIndex, shownWorld);
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

        private void BuildBossBar(Font display, Font body)
        {
            // Sits in the gap between UtilityHud (max.y 0.445) and HudPlate (min.y 0.555).
            _bossRoot = UiTheme.BuildPanel(
                "BossBar",
                transform,
                new Vector2(0.012f, 0.458f),
                new Vector2(0.38f, 0.542f),
                0.9f,
                UiTheme.HeaderWash,
                UiTheme.HeaderRule,
                UiTheme.HudPlate);
            CreateBarRow(
                "BossHp",
                _bossRoot.transform,
                display,
                body,
                new Vector2(0.04f, 0.06f),
                new Vector2(0.96f, 0.94f),
                UiTheme.Danger,
                out _bossLabel,
                out _bossCount,
                out _bossFill);
            if (_bossLabel != null)
            {
                _bossLabel.text = Loc.T("ui.boss", "WORLD GUARDIAN");
            }

            _bossRoot.SetActive(false);
        }

        private void RefreshBossBar()
        {
            if (_bossRoot == null)
            {
                return;
            }

            bool playing = _session != null && _session.Phase == GamePhase.Playing;
            bool showBoss = playing && _waves != null && _waves.HasBoss;
            _bossRoot.SetActive(showBoss);
            if (!showBoss)
            {
                return;
            }

            if (_bossLabel != null)
            {
                _bossLabel.text = Loc.T("ui.boss", "WORLD GUARDIAN");
            }

            int bossHp = _waves.BossHp;
            int bossMax = _waves.BossMaxHp;
            if (bossMax < 1)
            {
                bossMax = 1;
            }

            if (_bossFill != null)
            {
                _bossFill.fillAmount = Mathf.Clamp01(bossHp / (float)bossMax);
                _bossFill.color = UiTheme.Danger;
            }

            if (_bossCount != null)
            {
                _bossCount.text = bossHp + " / " + bossMax;
            }
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

            return Loc.T("ui.lives", "LIVES") + "  " + new string(UiGlyph.LifeFull, lives) + new string(UiGlyph.LifeEmpty, cap - lives);
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
                Stretch(rt, FailedHealthMin, FailedHealthMax);
                _healthRoot.transform.SetAsLastSibling();
                ApplyShipPreviewAnchors();
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
            PaintPrompt(_doctrineHint, Loc.T("ui.hint_dual", HintDual));

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
                PaintPrompt(
                    _doctrineHint,
                    state.Rail
                        ? Loc.T("ui.hint_rail", HintRail)
                        : Loc.T("ui.hint_dual", HintDual));
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
            bool runOver = GameSession.ShopLockedForPhase(_session.Phase);
            bool tooPoor = !runOver && !chosen && !other && gate && cost > 0 && _session.Credits < cost;
            bool locked = runOver || other || !gate;
            bool action = !runOver && DoctrineRules.CardIsAction(_session.ShopOpen, chosen, other, gate, tooPoor);
            button.interactable = action;
            button.colors = UiTheme.ShopButtonColors();
            Image plate = button.targetGraphic as Image;
            if (plate != null)
            {
                plate.raycastTarget = action;
                plate.canvasRenderer.SetColor(Color.white);
            }
            UiTheme.PaintShopPlate(plate, label, chosen, locked, tooPoor);
            string statusLine = runOver
                ? Loc.T("ui.run_over", "Run is over")
                : DoctrineStatusLine(id, gate, cost, chosen, other, tooPoor);
            label.text = DoctrineLabel(id) + "\n" + DoctrineBlurb(id) + "\n" + statusLine;
        }

        private static void FitDoctrineLabel(Text label)
        {
            if (label == null)
            {
                return;
            }

            label.font = UiFonts.Body();
            label.fontSize = UiTheme.DoctrineCardSize;
            label.fontStyle = FontStyle.Normal;
            label.lineSpacing = UiTheme.ShopLineSpacing;
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

        private int CurrentHintSize()
        {
            int step = _settings != null ? _settings.HintSizeStep : SettingsState.DefaultHintSizeStep;
            int floor = UiTheme.HintSize(Screen.width);
            return SettingsState.EffectiveHintSize(step, Screen.width, floor);
        }

        private void ApplyFooterHintSize()
        {
            int footerSize = CurrentHintSize();
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
                _settingsControlsBody.fontSize = UiTheme.HintSize(Screen.width);
            }
        }

        private void ApplyBottomHint(bool playing)
        {
            if (_hint == null)
            {
                return;
            }

            ApplyFooterHintSize();
            if (_game != null && _game.TutorialActive)
            {
                _hint.gameObject.SetActive(true);
                PaintPrompt(_hint, FirstRunRules.SkipHint());
                return;
            }

            HintMode mode = _settings != null ? _settings.HintMode : HintMode.HangarFooter;
            bool firstWave = playing && _session != null && _session.WaveIndex == 1;
            bool coach = SettingsState.ShowsFirstWaveCoach(firstWave, mode);
            bool show = coach || (playing
                ? SettingsState.ShowsPlayHint(mode)
                : SettingsState.ShowsHangarFooter(mode));
            _hint.gameObject.SetActive(show);
            if (!show)
            {
                PaintPrompt(_hint, string.Empty);
                return;
            }

            if (coach)
            {
                PaintPrompt(_hint, Loc.T("ui.first_wave_coach", FirstWaveCoach));
            }
            else if (playing)
            {
                bool rail = _loadout != null
                    && _loadout.State != null
                    && _loadout.State.ResolvedPrimary() == FireMode.Rail;
                if (rail)
                {
                    PaintPrompt(_hint, Loc.T("ui.hint_rail", HintRail));
                }
                else
                {
                    PaintPrompt(_hint, Loc.T("ui.hint_play", HintPlay));
                }
            }
            else
            {
                PaintPrompt(_hint, Loc.T("ui.hint_footer", HintFooter));
            }

            ClampOneLine(_hint);
        }

        private void PaintPrompt(Text host, string source)
        {
            if (host == null)
            {
                return;
            }

            float promptScale = SettingsMeasure.CanvasScale(Screen.width, Screen.height);
            PromptLineView.Paint(host, source, InputSchemeDriver.Current, promptScale);
        }

        private void RestylePrompts()
        {
            float promptScale = SettingsMeasure.CanvasScale(Screen.width, Screen.height);
            InputScheme schemeNow = InputSchemeDriver.Current;
            PromptLineView.Restyle(_hint, schemeNow, promptScale);
            PromptLineView.Restyle(_settingsControlsBody, schemeNow, promptScale);
            PromptLineView.Restyle(_doctrineHint, schemeNow, promptScale);
            PromptLineView.Restyle(_firstFlightBody, schemeNow, promptScale);
            PromptLineView.Restyle(_boonHint, schemeNow, promptScale);
            PromptLineView.Restyle(_tutorialBanner, schemeNow, promptScale);
            if (_settingsConfirmAbortLabel != null)
            {
                _settingsConfirmAbortLabel.text = PromptText.Flatten(
                    Loc.T("ui.settings.confirm_abort", "Confirm abort ({pause}) during wave"),
                    schemeNow);
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
            RaiseCanvas(_settingsRoot, CanvasOrder.Overlay);

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

            _settingsViewport = new GameObject("SettingsViewport");
            _settingsViewport.transform.SetParent(_settingsPanel.transform, false);
            Stretch(
                _settingsViewport.AddComponent<RectTransform>(),
                new Vector2(0f, SettingsScroll.ViewportBottom),
                new Vector2(0.955f, SettingsScroll.ViewportTop));
            _settingsViewport.AddComponent<RectMask2D>();
            _settingsRows = _settingsViewport.transform;
            BuildSettingsScrollbar();

            int rowCount = SettingsRows.Count;
            _settingsRowButtons = new Button[rowCount];
            float bandY0;
            float bandY1;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                SettingsScroll.ViewportBand(rowIndex, 0, out bandY0, out bandY1);
                BuildSettingsRow(SettingsRows.Order[rowIndex], rowIndex, display, body, bandY0, bandY1);
            }

            ApplySettingsFonts();
            _rebindOverlay = RebindOverlay.Create(transform);
            _rebindOverlay.SetChanged(OnBindingsChanged);
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

            if (rowId == SettingsRowId.ReduceEffects)
            {
                BuildSettingsReduceRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.AssistMode)
            {
                BuildSettingsAssistRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.HintMode)
            {
                BuildSettingsHintModeRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.HintSize)
            {
                BuildSettingsHintSizeRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.ConfirmAbort)
            {
                BuildSettingsConfirmAbortRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.ConfirmNewRun)
            {
                BuildSettingsConfirmNewRunRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.PadNav)
            {
                BuildSettingsPadNavRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.WindowMode)
            {
                BuildSettingsWindowRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.Resolution)
            {
                BuildSettingsResolutionRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.VSync)
            {
                BuildSettingsVSyncRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.FpsCap)
            {
                BuildSettingsFpsRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.PromptScheme)
            {
                BuildSettingsPromptRow(rowIndex, body, y0, y1);
                return;
            }

            if (rowId == SettingsRowId.ChangeControls)
            {
                BuildSettingsRebindRow(rowIndex, body, y0, y1);
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
                _settingsRows,
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

            float initial = MixCurve.DefaultSfxSlider;
            if (music)
            {
                initial = AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : MixCurve.DefaultMusicSlider;
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

            BindSettingsClick(row, rowIndex, null);
        }

        private void BuildSettingsMuteRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsMute",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleSettingsMute);
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
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleScreenShake);
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

        private void BuildSettingsReduceRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsReduce",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleReduceEffects);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsReduceLabel = row.GetComponentInChildren<Text>();
            _settingsReduceLabel.fontSize = 12;
            _settingsReduceLabel.alignment = TextAnchor.MiddleLeft;
            _settingsReduceLabel.fontStyle = FontStyle.Bold;
            _settingsReduceLabel.color = UiTheme.Accent;
            _settingsReduceLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _settingsReduceLabel.verticalOverflow = VerticalWrapMode.Truncate;
            _settingsReduceLabel.raycastTarget = false;
            Stretch(_settingsReduceLabel.rectTransform, new Vector2(0.03f, 0.06f), new Vector2(0.78f, 0.94f));

            _settingsReduceValue = CreateText(
                "SettingsReduceValue",
                row.transform,
                body,
                12,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsReduceValue.rectTransform, new Vector2(0.78f, 0.12f), new Vector2(0.97f, 0.88f));
            _settingsReduceValue.color = UiTheme.Primary;
            _settingsReduceValue.raycastTarget = false;
        }

        private void BuildSettingsAssistRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsAssist",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleAssistMode);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsAssistLabel = row.GetComponentInChildren<Text>();
            _settingsAssistLabel.fontSize = 12;
            _settingsAssistLabel.alignment = TextAnchor.MiddleLeft;
            _settingsAssistLabel.fontStyle = FontStyle.Bold;
            _settingsAssistLabel.color = UiTheme.Accent;
            _settingsAssistLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _settingsAssistLabel.verticalOverflow = VerticalWrapMode.Truncate;
            _settingsAssistLabel.raycastTarget = false;
            Stretch(_settingsAssistLabel.rectTransform, new Vector2(0.03f, 0.06f), new Vector2(0.78f, 0.94f));

            _settingsAssistValue = CreateText(
                "SettingsAssistValue",
                row.transform,
                body,
                12,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsAssistValue.rectTransform, new Vector2(0.78f, 0.12f), new Vector2(0.97f, 0.88f));
            _settingsAssistValue.color = UiTheme.Primary;
            _settingsAssistValue.raycastTarget = false;
        }

        private void BuildSettingsHintModeRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsHintMode",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, CycleHintMode);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsHintModeLabel = row.GetComponentInChildren<Text>();
            _settingsHintModeLabel.fontSize = UiTheme.BodyMin;
            _settingsHintModeLabel.alignment = TextAnchor.MiddleLeft;
            _settingsHintModeLabel.fontStyle = FontStyle.Bold;
            _settingsHintModeLabel.color = UiTheme.Accent;
            _settingsHintModeLabel.raycastTarget = false;
            Stretch(_settingsHintModeLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.52f, 0.92f));

            _settingsHintModeValue = CreateText(
                "SettingsHintModeValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsHintModeValue.rectTransform, new Vector2(0.54f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsHintModeValue.color = UiTheme.Primary;
            _settingsHintModeValue.raycastTarget = false;
        }

        private void BuildSettingsHintSizeRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsHintSize",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, CycleHintSize);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsHintSizeLabel = row.GetComponentInChildren<Text>();
            _settingsHintSizeLabel.fontSize = UiTheme.BodyMin;
            _settingsHintSizeLabel.alignment = TextAnchor.MiddleLeft;
            _settingsHintSizeLabel.fontStyle = FontStyle.Bold;
            _settingsHintSizeLabel.color = UiTheme.Accent;
            _settingsHintSizeLabel.raycastTarget = false;
            Stretch(_settingsHintSizeLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.68f, 0.92f));

            _settingsHintSizeValue = CreateText(
                "SettingsHintSizeValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsHintSizeValue.rectTransform, new Vector2(0.70f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsHintSizeValue.color = UiTheme.Primary;
            _settingsHintSizeValue.raycastTarget = false;
        }

        private void BuildSettingsConfirmAbortRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsConfirmAbort",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleConfirmAbort);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsConfirmAbortLabel = row.GetComponentInChildren<Text>();
            _settingsConfirmAbortLabel.fontSize = UiTheme.BodyMin;
            _settingsConfirmAbortLabel.alignment = TextAnchor.MiddleLeft;
            _settingsConfirmAbortLabel.fontStyle = FontStyle.Bold;
            _settingsConfirmAbortLabel.color = UiTheme.Accent;
            _settingsConfirmAbortLabel.raycastTarget = false;
            Stretch(_settingsConfirmAbortLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.76f, 0.92f));

            _settingsConfirmAbortValue = CreateText(
                "SettingsConfirmAbortValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsConfirmAbortValue.rectTransform, new Vector2(0.78f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsConfirmAbortValue.color = UiTheme.Primary;
            _settingsConfirmAbortValue.raycastTarget = false;
        }

        private void BuildSettingsConfirmNewRunRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsConfirmNewRun",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleConfirmNewRun);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsConfirmNewRunLabel = row.GetComponentInChildren<Text>();
            _settingsConfirmNewRunLabel.fontSize = UiTheme.BodyMin;
            _settingsConfirmNewRunLabel.alignment = TextAnchor.MiddleLeft;
            _settingsConfirmNewRunLabel.fontStyle = FontStyle.Bold;
            _settingsConfirmNewRunLabel.color = UiTheme.Accent;
            _settingsConfirmNewRunLabel.raycastTarget = false;
            Stretch(_settingsConfirmNewRunLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.76f, 0.92f));

            _settingsConfirmNewRunValue = CreateText(
                "SettingsConfirmNewRunValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsConfirmNewRunValue.rectTransform, new Vector2(0.78f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsConfirmNewRunValue.color = UiTheme.Primary;
            _settingsConfirmNewRunValue.raycastTarget = false;
        }

        private void BuildSettingsPadNavRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsPadNav",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, CyclePadNav);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsPadNavLabel = row.GetComponentInChildren<Text>();
            _settingsPadNavLabel.fontSize = UiTheme.BodyMin;
            _settingsPadNavLabel.alignment = TextAnchor.MiddleLeft;
            _settingsPadNavLabel.fontStyle = FontStyle.Bold;
            _settingsPadNavLabel.color = UiTheme.Accent;
            _settingsPadNavLabel.raycastTarget = false;
            Stretch(_settingsPadNavLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.62f, 0.92f));

            _settingsPadNavValue = CreateText(
                "SettingsPadNavValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsPadNavValue.rectTransform, new Vector2(0.64f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsPadNavValue.color = UiTheme.Primary;
            _settingsPadNavValue.raycastTarget = false;
        }

        private void BuildSettingsWindowRow(int rowIndex, Font body, float y0, float y1)
        {
            BuildSettingsChoiceRow(rowIndex, "SettingsWindow", body, y0, y1, CycleWindowMode, out _settingsWindowLabel, out _settingsWindowValue);
        }

        private void BuildSettingsResolutionRow(int rowIndex, Font body, float y0, float y1)
        {
            BuildSettingsChoiceRow(rowIndex, "SettingsResolution", body, y0, y1, CycleResolution, out _settingsResolutionLabel, out _settingsResolutionValue);
        }

        private void BuildSettingsVSyncRow(int rowIndex, Font body, float y0, float y1)
        {
            BuildSettingsChoiceRow(rowIndex, "SettingsVSync", body, y0, y1, ToggleVSync, out _settingsVSyncLabel, out _settingsVSyncValue);
        }

        private void BuildSettingsFpsRow(int rowIndex, Font body, float y0, float y1)
        {
            BuildSettingsChoiceRow(rowIndex, "SettingsFps", body, y0, y1, CycleFpsCap, out _settingsFpsLabel, out _settingsFpsValue);
        }

        private void BuildSettingsChoiceRow(
            int rowIndex,
            string rowName,
            Font body,
            float y0,
            float y1,
            UnityEngine.Events.UnityAction onClick,
            out Text label,
            out Text value)
        {
            float x0;
            float x1;
            SettingsScroll.RowX(SettingsRowId.Language, out x0, out x1);
            Button row = CreateButton(rowName, _settingsRows, body, new Vector2(x0, y0), new Vector2(x1, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, onClick);
            UiTheme.ApplyButton(row, false, false, false);
            label = row.GetComponentInChildren<Text>();
            label.fontSize = SettingsScroll.RowFont;
            label.alignment = TextAnchor.MiddleLeft;
            label.fontStyle = FontStyle.Bold;
            label.color = UiTheme.Accent;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            Stretch(label.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.48f, 0.92f));
            value = CreateText(rowName + "Value", row.transform, body, SettingsScroll.RowFont, TextAnchor.MiddleRight, FontStyle.Bold);
            Stretch(value.rectTransform, new Vector2(0.50f, 0.08f), new Vector2(0.96f, 0.92f));
            value.color = UiTheme.Primary;
            value.horizontalOverflow = HorizontalWrapMode.Wrap;
            value.verticalOverflow = VerticalWrapMode.Truncate;
            value.raycastTarget = false;
        }

        private void ApplySettingsFonts()
        {
            if (_settingsRowButtons == null)
            {
                return;
            }

            for (int index = 0; index < _settingsRowButtons.Length; index++)
            {
                Button row = _settingsRowButtons[index];
                if (row == null)
                {
                    continue;
                }

                Text[] labels = row.GetComponentsInChildren<Text>(true);
                for (int labelIndex = 0; labelIndex < labels.Length; labelIndex++)
                {
                    Text rowLabel = labels[labelIndex];
                    if (rowLabel == null)
                    {
                        continue;
                    }

                    rowLabel.fontSize = SettingsScroll.RowFont;
                    rowLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                    rowLabel.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }

            if (_settingsControlsTitle != null)
            {
                _settingsControlsTitle.fontSize = SettingsScroll.RowFont;
            }

            if (_settingsControlsBody != null)
            {
                _settingsControlsBody.fontSize = SettingsScroll.RowFont;
            }
        }

        private void ApplySettingsScroll()
        {
            if (_settingsRowButtons == null)
            {
                return;
            }

            int focus = SettingsRows.ClampIndex(_settingsIndex);
            int count = SettingsRows.Count;
            for (int index = 0; index < count; index++)
            {
                float y0;
                float y1;
                SettingsScroll.ViewportBandAt(index, _settingsShift, out y0, out y1);
                SettingsRowId rowId = SettingsRows.At(index);
                if (rowId == SettingsRowId.Controls)
                {
                    PlaceSettingsControls(y0, y1);
                    continue;
                }

                if (index >= _settingsRowButtons.Length)
                {
                    continue;
                }

                Button rowButton = _settingsRowButtons[index];
                if (rowButton == null)
                {
                    continue;
                }

                float x0;
                float x1;
                SettingsScroll.RowX(rowId, out x0, out x1);
                Stretch(rowButton.GetComponent<RectTransform>(), new Vector2(x0, y0), new Vector2(x1, y1));
                bool rowShown = SettingsScroll.IntersectsViewport(y0, y1);
                bool rowFull = SettingsScroll.ContainsBand(y0, y1);
                SetSettingsRowRays(rowButton, rowShown, rowFull);
            }

            SyncSettingsScrollbar();
        }

        private void ApplyPreviewAspect()
        {
            if (_previewViewport == null)
            {
                return;
            }

            float frameMaxY = ShipPreviewMax.y;
            if (_session != null && _session.Phase == GamePhase.Failed)
            {
                frameMaxY = FailedPreviewMaxY;
            }

            float frameWidth = (ShipPreviewMax.x - ShipPreviewMin.x) * Mathf.Max(1, Screen.width);
            float frameHeight = (frameMaxY - ShipPreviewMin.y) * Mathf.Max(1, Screen.height);
            float wellWidth = frameWidth * (HangarPreviewRig.WellMaxX - HangarPreviewRig.WellMinX);
            float wellHeight = frameHeight * (HangarPreviewRig.WellMaxY - HangarPreviewRig.WellMinY);
            float uvX;
            float uvY;
            float uvW;
            float uvH;
            float fitMinX;
            float fitMinY;
            float fitMaxX;
            float fitMaxY;
            HangarPreviewRig.Contain(
                wellWidth,
                wellHeight,
                HangarPreviewRig.ViewportWidth,
                HangarPreviewRig.ViewportHeight,
                out uvX,
                out uvY,
                out uvW,
                out uvH,
                out fitMinX,
                out fitMinY,
                out fitMaxX,
                out fitMaxY);
            _previewViewport.uvRect = new Rect(uvX, uvY, uvW, uvH);
            Stretch(_previewViewport.rectTransform, new Vector2(fitMinX, fitMinY), new Vector2(fitMaxX, fitMaxY));
        }

        private void BuildSettingsScrollbar()
        {
            _settingsScrollTrack = new GameObject("SettingsScrollTrack");
            _settingsScrollTrack.transform.SetParent(_settingsPanel.transform, false);
            Stretch(
                _settingsScrollTrack.AddComponent<RectTransform>(),
                new Vector2(0.962f, SettingsScroll.ViewportBottom),
                new Vector2(0.988f, SettingsScroll.ViewportTop));
            Image trackImage = _settingsScrollTrack.AddComponent<Image>();
            trackImage.color = UiTheme.WithAlpha(UiTheme.Void, 0.92f);
            trackImage.raycastTarget = true;

            GameObject thumb = new GameObject("SettingsScrollThumb");
            thumb.transform.SetParent(_settingsScrollTrack.transform, false);
            RectTransform thumbRect = thumb.AddComponent<RectTransform>();
            Stretch(thumbRect, Vector2.zero, Vector2.one);
            Image thumbImage = thumb.AddComponent<Image>();
            thumbImage.color = UiTheme.Primary;
            thumbImage.raycastTarget = true;

            _settingsScrollbar = _settingsScrollTrack.AddComponent<Scrollbar>();
            _settingsScrollbar.handleRect = thumbRect;
            _settingsScrollbar.targetGraphic = thumbImage;
            _settingsScrollbar.direction = Scrollbar.Direction.BottomToTop;
            Navigation scrollNav = _settingsScrollbar.navigation;
            scrollNav.mode = Navigation.Mode.None;
            _settingsScrollbar.navigation = scrollNav;
            _settingsScrollbar.onValueChanged.AddListener(OnSettingsScrollChanged);
        }

        private void SyncSettingsScrollbar()
        {
            if (_settingsScrollbar == null)
            {
                return;
            }

            _settingsScrollMute = true;
            _settingsScrollbar.size = SettingsScroll.ScrollbarSize();
            _settingsScrollbar.value = SettingsScroll.ScrollbarValue(_settingsShift);
            _settingsScrollMute = false;
            if (_settingsScrollTrack != null)
            {
                _settingsScrollTrack.SetActive(SettingsScroll.MaxShift() > 0.0001f);
            }
        }

        private void OnSettingsScrollChanged(float value)
        {
            if (_settingsScrollMute)
            {
                return;
            }

            _settingsShift = SettingsScroll.ShiftFromScrollbar(value);
            ApplySettingsScroll();
        }

        private void TickSettingsWheel()
        {
            float scrollWheel = Input.mouseScrollDelta.y;
            if (scrollWheel == 0f)
            {
                return;
            }

            _settingsShift = SettingsScroll.Wheel(_settingsShift, scrollWheel);
            ApplySettingsScroll();
        }

        private void SetSettingsRowRays(Button rowButton, bool shown, bool full)
        {
            if (rowButton == null)
            {
                return;
            }

            rowButton.interactable = shown;
            Image plate = rowButton.GetComponent<Image>();
            if (plate != null)
            {
                plate.raycastTarget = shown;
            }

            Selectable[] nested = rowButton.GetComponentsInChildren<Selectable>(true);
            for (int nestedIndex = 0; nestedIndex < nested.Length; nestedIndex++)
            {
                Selectable part = nested[nestedIndex];
                if (part == null || part == rowButton)
                {
                    continue;
                }

                part.interactable = full;
                if (part.targetGraphic != null)
                {
                    part.targetGraphic.raycastTarget = full;
                }
            }
        }

        private void BindSettingsClick(Button row, int rowIndex, UnityEngine.Events.UnityAction action)
        {
            int captured = rowIndex;
            row.onClick.AddListener(() =>
            {
                if (!AllowSettingsActivate(captured))
                {
                    return;
                }

                if (action != null)
                {
                    action();
                }
            });
        }

        private bool AllowSettingsActivate(int rowIndex)
        {
            float bandBottom;
            float bandTop;
            SettingsScroll.ViewportBandAt(rowIndex, _settingsShift, out bandBottom, out bandTop);
            int kind = SettingsScroll.ClickKind(bandBottom, bandTop);
            if (kind == SettingsScroll.ClickActivate)
            {
                return true;
            }

            if (kind == SettingsScroll.ClickFocus)
            {
                _settingsIndex = rowIndex;
                RefreshSettingsFocus();
                return false;
            }

            return false;
        }

        private void PlaceSettingsControls(float y0, float y1)
        {
            float header = 0.06f;
            if (y1 - header < y0)
            {
                header = (y1 - y0) * 0.22f;
            }

            if (_settingsControlsTitle != null)
            {
                Stretch(
                    _settingsControlsTitle.rectTransform,
                    new Vector2(SettingsMeasure.BodyMinX, y1 - header),
                    new Vector2(SettingsMeasure.BodyMaxX, y1));
            }

            if (_settingsControlsBody != null)
            {
                Stretch(
                    _settingsControlsBody.rectTransform,
                    new Vector2(SettingsMeasure.BodyMinX, y0),
                    new Vector2(SettingsMeasure.BodyMaxX, y1 - header));
            }
        }

        private void BuildSettingsLanguageRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsLanguage",
                _settingsRows,
                body,
                new Vector2(0.06f, y0),
                new Vector2(0.94f, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, CycleSettingsLanguage);
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
                _settingsRows,
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
                _settingsRows,
                body,
                SettingsScroll.RowFont,
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
            PaintPrompt(_settingsControlsBody, FullControlHint());
        }

        private void BuildSettingsCloseRow(int rowIndex, Font display, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsClose",
                _settingsRows,
                display,
                new Vector2(0.22f, y0),
                new Vector2(0.78f, y1));
            _settingsRowButtons[rowIndex] = row;
            Text closeLabel = row.GetComponentInChildren<Text>();
            closeLabel.fontSize = UiTheme.BodyMin;
            closeLabel.text = Loc.T("ui.settings.close", "Close");
            BindSettingsClick(row, rowIndex, CloseSettings);
            UiTheme.ApplyButton(row, true, false, false);
        }

        private static string FullControlHint()
        {
            string playHint = Loc.T("ui.hint_play", HintPlay);
            string hangarHint = Loc.Tf(
                "ui.hint_hangar",
                "{move} move · {0}",
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
                current = AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : MixCurve.DefaultMusicSlider;
            }
            else
            {
                current = AudioCues.Instance != null ? AudioCues.Instance.SfxVolume : MixCurve.DefaultSfxSlider;
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

        private void EnsureSettings()
        {
            if (_settings == null)
            {
                _settings = SettingsState.Load();
            }
        }

        private void CycleHintMode()
        {
            StepHintMode(1);
        }

        private void CycleHintSize()
        {
            StepHintSize(1);
        }

        private void StepHintMode(int direction)
        {
            EnsureSettings();
            _settings.HintMode = SettingsState.StepHintMode(_settings.HintMode, direction);
            _settings.Save();
            RefreshSettingsHint();
            ApplyBottomHint(_session != null && _session.Phase == GamePhase.Playing);
        }

        private void StepHintSize(int direction)
        {
            EnsureSettings();
            _settings.HintSizeStep = SettingsState.StepHintSize(_settings.HintSizeStep, direction, Screen.width);
            _settings.Save();
            RefreshSettingsHint();
            ApplyBottomHint(_session != null && _session.Phase == GamePhase.Playing);
        }

        private static string HintModeLabel(HintMode mode)
        {
            if (mode == HintMode.HangarFooter)
            {
                return Loc.T("ui.settings.hint.hangar", "Hangar only");
            }

            if (mode == HintMode.On)
            {
                return Loc.T("ui.settings.on", "On");
            }

            if (mode == HintMode.SettingsOnly)
            {
                return Loc.T("ui.settings.hint.panel", "Settings only");
            }

            return Loc.T("ui.settings.off", "Off");
        }

        private void RefreshSettingsHint()
        {
            if (_settingsHintModeLabel != null)
            {
                _settingsHintModeLabel.text = Loc.T("ui.settings.hint", "Hint line");
            }

            if (_settingsHintSizeLabel != null)
            {
                _settingsHintSizeLabel.text = Loc.T("ui.settings.hint_size", "Hint text size");
            }

            HintMode mode = _settings != null ? _settings.HintMode : HintMode.HangarFooter;
            int step = _settings != null ? _settings.HintSizeStep : SettingsState.DefaultHintSizeStep;
            if (_settingsHintModeValue != null)
            {
                _settingsHintModeValue.text = HintModeLabel(mode);
            }

            if (_settingsHintSizeValue != null)
            {
                int shown = SettingsState.VisibleHintStep(step, Screen.width);
                _settingsHintSizeValue.text = Loc.Tf("ui.settings.hint.px", "{0}", SettingsState.HintPx(shown));
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

        private void ToggleReduceEffects()
        {
            EnsureSettings();
            _settings.ReduceEffects = !_settings.ReduceEffects;
            _settings.Save();
            RefreshSettingsReduce();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsReduce()
        {
            if (_settingsReduceLabel != null)
            {
                _settingsReduceLabel.text = Loc.T(
                    "ui.settings.reduce",
                    "Reduce effects: less screen shake, flashes and particles");
            }

            if (_settingsReduceValue == null)
            {
                return;
            }

            bool enabled = _settings != null && _settings.ReduceEffects;
            _settingsReduceValue.text = enabled
                ? Loc.T("ui.settings.on", "On")
                : Loc.T("ui.settings.off", "Off");
        }

        private void ToggleAssistMode()
        {
            EnsureSettings();
            _settings.AssistMode = !_settings.AssistMode;
            _settings.Save();
            if (_settings.AssistMode && _game != null && _session != null && _session.Phase == GamePhase.Playing)
            {
                _game.NoteAssistUsed();
            }

            RefreshSettingsAssist();
            Refresh();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsAssist()
        {
            if (_settingsAssistLabel != null)
            {
                _settingsAssistLabel.text = Loc.T(
                    "ui.settings.assist",
                    "Assist mode: extra shield, less enemy damage");
            }

            if (_settingsAssistValue == null)
            {
                return;
            }

            bool enabled = _settings != null && _settings.AssistMode;
            _settingsAssistValue.text = enabled
                ? Loc.T("ui.settings.on", "On")
                : Loc.T("ui.settings.off", "Off");
        }

        private void ToggleConfirmAbort()
        {
            EnsureSettings();
            _settings.ConfirmRestartInPlay = !_settings.ConfirmRestartInPlay;
            _settings.Save();
            RefreshSettingsConfirm();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void ToggleConfirmNewRun()
        {
            EnsureSettings();
            _settings.ConfirmRestartNewRun = !_settings.ConfirmRestartNewRun;
            _settings.Save();
            RefreshSettingsConfirm();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsConfirm()
        {
            if (_settingsConfirmAbortLabel != null)
            {
                _settingsConfirmAbortLabel.text = PromptText.Flatten(
                    Loc.T("ui.settings.confirm_abort", "Confirm abort ({pause}) during wave"),
                    InputSchemeDriver.Current);
            }

            if (_settingsConfirmNewRunLabel != null)
            {
                _settingsConfirmNewRunLabel.text = Loc.T("ui.settings.confirm_new_run", "Confirm New Run");
            }

            bool confirmAbort = _settings == null || _settings.ConfirmRestartInPlay;
            bool confirmNewRun = _settings != null && _settings.ConfirmRestartNewRun;
            if (_settingsConfirmAbortValue != null)
            {
                _settingsConfirmAbortValue.text = confirmAbort
                    ? Loc.T("ui.settings.on", "On")
                    : Loc.T("ui.settings.off", "Off");
            }

            if (_settingsConfirmNewRunValue != null)
            {
                _settingsConfirmNewRunValue.text = confirmNewRun
                    ? Loc.T("ui.settings.on", "On")
                    : Loc.T("ui.settings.off", "Off");
            }
        }

        private void BuildSettingsPromptRow(int rowIndex, Font body, float y0, float y1)
        {
            BuildSettingsChoiceRow(
                rowIndex,
                "SettingsPromptScheme",
                body,
                y0,
                y1,
                CyclePromptScheme,
                out _settingsPromptLabel,
                out _settingsPromptValue);
            RefreshSettingsPrompt();
        }

        private void BuildSettingsRebindRow(int rowIndex, Font body, float y0, float y1)
        {
            Text rebindValue;
            BuildSettingsChoiceRow(
                rowIndex,
                "SettingsRebind",
                body,
                y0,
                y1,
                OpenRebind,
                out _settingsRebindLabel,
                out rebindValue);
            if (_settingsRebindLabel != null)
            {
                _settingsRebindLabel.text = Loc.T("ui.settings.rebind", "Change controls");
            }

            if (rebindValue != null)
            {
                rebindValue.text = string.Empty;
            }
        }

        private void OpenRebind()
        {
            if (_rebindOverlay == null)
            {
                return;
            }

            _rebindOverlay.Open();
        }

        private void OnBindingsChanged()
        {
            EnsureSettings();
            _settings.Bindings = BindingStore.Serialize(BindingMap.ActiveOrDefault());
            _settings.Save();
            RestylePrompts();
        }

        private bool RebindConsumesInput()
        {
            return _rebindOverlay != null && _rebindOverlay.IsOpen;
        }

        private void TickRebind()
        {
            if (_rebindOverlay == null)
            {
                return;
            }

            _rebindOverlay.Tick(Time.unscaledTime, Time.frameCount);
            if (_settingsOpen)
            {
                SetSettingsNavigationLock(true);
            }
        }

        private void CyclePromptScheme()
        {
            StepPromptScheme(1);
        }

        private void StepPromptScheme(int direction)
        {
            EnsureSettings();
            InputSchemePreference current = InputSchemeRules.Normalize(_settings.PromptScheme);
            _settings.PromptScheme = (int)InputSchemeRules.Step(current, direction);
            _settings.Save();
            InputSchemeDriver.Poll(SettingsState.PromptPreference);
            RefreshSettingsPrompt();
            RestylePrompts();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private static string PromptSchemeLabel(InputSchemePreference preference)
        {
            InputSchemePreference normalized = InputSchemeRules.Normalize((int)preference);
            if (normalized == InputSchemePreference.Xbox)
            {
                return Loc.T("ui.settings.scheme.xbox", "Xbox");
            }

            if (normalized == InputSchemePreference.PlayStation)
            {
                return Loc.T("ui.settings.scheme.playstation", "PlayStation");
            }

            if (normalized == InputSchemePreference.Deck)
            {
                return Loc.T("ui.settings.scheme.deck", "Deck");
            }

            if (normalized == InputSchemePreference.Keyboard)
            {
                return Loc.T("ui.settings.scheme.keyboard", "Keyboard");
            }

            return Loc.T("ui.settings.scheme.auto", "Auto");
        }

        private void RefreshSettingsPrompt()
        {
            if (_settingsPromptLabel != null)
            {
                _settingsPromptLabel.text = Loc.T("ui.settings.prompt_scheme", "Button icons");
            }

            if (_settingsPromptValue == null)
            {
                return;
            }

            int stored = _settings != null ? _settings.PromptScheme : (int)InputSchemePreference.Auto;
            _settingsPromptValue.text = PromptSchemeLabel(InputSchemeRules.Normalize(stored));
        }

        private void CyclePadNav()
        {
            StepPadNav(1);
        }

        private void StepPadNav(int direction)
        {
            EnsureSettings();
            _settings.PadNavSource = SettingsState.StepPadNav(_settings.PadNavSource, direction);
            _settings.Save();
            RefreshSettingsPadNav();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private static string PadNavLabel(PadNavSource source)
        {
            if (source == PadNavSource.DPad)
            {
                return Loc.T("ui.settings.pad.dpad", "D-pad");
            }

            if (source == PadNavSource.Analog)
            {
                return Loc.T("ui.settings.pad.analog", "Analog");
            }

            return Loc.T("ui.settings.pad.both", "Both");
        }

        private void RefreshSettingsPadNav()
        {
            if (_settingsPadNavLabel != null)
            {
                _settingsPadNavLabel.text = Loc.T("ui.settings.pad_nav", "Pad navigation");
            }

            if (_settingsPadNavValue == null)
            {
                return;
            }

            PadNavSource source = PadNavSource.Both;
            if (_settings != null)
            {
                source = _settings.PadNavSource;
            }

            _settingsPadNavValue.text = PadNavLabel(source);
        }

        private void CycleWindowMode()
        {
            StepWindowMode(1);
        }

        private void CycleResolution()
        {
            StepResolution(1);
        }

        private void CycleFpsCap()
        {
            StepFpsCap(1);
        }

        private void StepWindowMode(int direction)
        {
            EnsureSettings();
            _settings.WindowMode = DisplaySettings.StepWindow(_settings.WindowMode, direction);
            _settings.Save();
            DisplayRuntime.Apply(_settings);
            RefreshSettingsDisplay();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void StepResolution(int direction)
        {
            EnsureSettings();
            int nextWidth;
            int nextHeight;
            DisplaySettings.StepResolution(_settings.ResolutionWidth, _settings.ResolutionHeight, direction, out nextWidth, out nextHeight);
            _settings.ResolutionWidth = nextWidth;
            _settings.ResolutionHeight = nextHeight;
            _settings.Save();
            DisplayRuntime.Apply(_settings);
            RefreshSettingsDisplay();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void ToggleVSync()
        {
            EnsureSettings();
            _settings.VSync = !_settings.VSync;
            _settings.Save();
            DisplayRuntime.Apply(_settings);
            RefreshSettingsDisplay();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void StepFpsCap(int direction)
        {
            EnsureSettings();
            _settings.FpsCap = DisplaySettings.StepFps(_settings.FpsCap, direction);
            _settings.Save();
            DisplayRuntime.Apply(_settings);
            RefreshSettingsDisplay();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private static string WindowModeLabel(int mode)
        {
            int normalized = DisplaySettings.NormalizeWindow(mode);
            if (normalized == (int)WindowModeId.Windowed)
            {
                return Loc.T("ui.settings.window.windowed", "Windowed");
            }

            if (normalized == (int)WindowModeId.Fullscreen)
            {
                return Loc.T("ui.settings.window.fullscreen", "Fullscreen");
            }

            return Loc.T("ui.settings.window.borderless", "Borderless");
        }

        private static string FpsCapLabel(int cap, bool vsync)
        {
            int normalized = DisplaySettings.NormalizeFps(cap);
            if (normalized == DisplaySettings.Uncapped)
            {
                if (vsync)
                {
                    return Loc.T("ui.settings.fps.uncapped_held", "Uncapped  ·  ignored");
                }

                return Loc.T("ui.settings.fps.uncapped", "Uncapped");
            }

            if (vsync)
            {
                return Loc.Tf("ui.settings.fps.held", "{0}  ·  ignored", normalized);
            }

            return Loc.Tf("ui.settings.fps.value", "{0}", normalized);
        }

        private void RefreshSettingsDisplay()
        {
            if (_settingsWindowLabel != null)
            {
                _settingsWindowLabel.text = Loc.T("ui.settings.window", "Window");
            }

            if (_settingsResolutionLabel != null)
            {
                _settingsResolutionLabel.text = Loc.T("ui.settings.resolution", "Resolution");
            }

            if (_settingsVSyncLabel != null)
            {
                _settingsVSyncLabel.text = Loc.T("ui.settings.vsync", "VSync");
            }

            if (_settingsFpsLabel != null)
            {
                _settingsFpsLabel.text = Loc.T("ui.settings.fps", "FPS cap");
            }

            int mode = DisplaySettings.DefaultWindowMode;
            int width = DisplaySettings.DefaultWidth;
            int height = DisplaySettings.DefaultHeight;
            bool vsync = DisplaySettings.DefaultVSync != 0;
            int cap = DisplaySettings.DefaultFpsCap;
            if (_settings != null)
            {
                mode = _settings.WindowMode;
                width = _settings.ResolutionWidth;
                height = _settings.ResolutionHeight;
                vsync = _settings.VSync;
                cap = _settings.FpsCap;
            }

            if (_settingsWindowValue != null)
            {
                _settingsWindowValue.text = WindowModeLabel(mode);
            }

            if (_settingsResolutionValue != null)
            {
                _settingsResolutionValue.text = Loc.Tf("ui.settings.resolution.value", "{0} x {1}", width, height);
            }

            if (_settingsVSyncValue != null)
            {
                _settingsVSyncValue.text = vsync
                    ? Loc.T("ui.settings.vsync.on", "On  ·  cap ignored")
                    : Loc.T("ui.settings.off", "Off");
            }

            if (_settingsFpsValue != null)
            {
                _settingsFpsValue.text = FpsCapLabel(cap, vsync);
            }
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

            float musicVolume = AudioCues.Instance != null ? AudioCues.Instance.MusicVolume : MixCurve.DefaultMusicSlider;
            float sfxVolume = AudioCues.Instance != null ? AudioCues.Instance.SfxVolume : MixCurve.DefaultSfxSlider;
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

        private void BuildConfirmDialog(Font display, Font body)
        {
            _confirmRoot = new GameObject("ConfirmRoot");
            _confirmRoot.transform.SetParent(transform, false);
            Stretch(_confirmRoot.AddComponent<RectTransform>(), Vector2.zero, Vector2.one);
            RaiseCanvas(_confirmRoot, CanvasOrder.Overlay);

            GameObject scrim = CreateFill(
                "ConfirmScrim",
                _confirmRoot.transform,
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
            scrimButton.onClick.AddListener(OnConfirmScrim);
            Navigation scrimNav = scrimButton.navigation;
            scrimNav.mode = Navigation.Mode.None;
            scrimButton.navigation = scrimNav;

            GameObject panel = UiTheme.BuildPanel(
                "ConfirmPanel",
                _confirmRoot.transform,
                new Vector2(ConfirmDialogLayout.PanelMinX, ConfirmDialogLayout.PanelMinY),
                new Vector2(ConfirmDialogLayout.PanelMaxX, ConfirmDialogLayout.PanelMaxY),
                0.72f);
            Image panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.raycastTarget = true;
            }

            _confirmTitle = CreateText(
                "ConfirmTitle",
                panel.transform,
                display,
                22,
                TextAnchor.MiddleCenter,
                FontStyle.Bold);
            Stretch(_confirmTitle.rectTransform, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.96f));
            _confirmTitle.color = UiTheme.Primary;
            AddReadability(_confirmTitle, true);

            _confirmBody = CreateText(
                "ConfirmBody",
                panel.transform,
                body,
                18,
                TextAnchor.MiddleCenter,
                FontStyle.Normal);
            Stretch(_confirmBody.rectTransform, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.70f));
            _confirmBody.color = UiTheme.Accent;
            _confirmBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _confirmBody.verticalOverflow = VerticalWrapMode.Truncate;
            _confirmBody.lineSpacing = UiTheme.ShopLineSpacing;

            _confirmYes = CreateButton(
                "ConfirmYes",
                panel.transform,
                body,
                new Vector2(ConfirmDialogLayout.YesMinX, ConfirmDialogLayout.YesMinY),
                new Vector2(ConfirmDialogLayout.YesMaxX, ConfirmDialogLayout.YesMaxY));
            _confirmYesLabel = _confirmYes.GetComponentInChildren<Text>();
            _confirmYesLabel.fontSize = 18;
            _confirmYes.onClick.AddListener(OnConfirmYesClick);
            UiTheme.ApplyButton(_confirmYes, false, false, false);
            LockButtonNavigation(_confirmYes);

            _confirmNo = CreateButton(
                "ConfirmNo",
                panel.transform,
                body,
                new Vector2(ConfirmDialogLayout.NoMinX, ConfirmDialogLayout.NoMinY),
                new Vector2(ConfirmDialogLayout.NoMaxX, ConfirmDialogLayout.NoMaxY));
            _confirmNoLabel = _confirmNo.GetComponentInChildren<Text>();
            _confirmNoLabel.fontSize = 18;
            _confirmNo.onClick.AddListener(OnConfirmNoClick);
            UiTheme.ApplyButton(_confirmNo, true, false, false);
            LockButtonNavigation(_confirmNo);

            _confirmRoot.SetActive(false);
        }

        private static void LockButtonNavigation(Button button)
        {
            if (button == null)
            {
                return;
            }

            Navigation locked = button.navigation;
            locked.mode = Navigation.Mode.None;
            button.navigation = locked;
        }

        private ConfirmRequest ReadConfirmRequest()
        {
            ConfirmRequest request = new ConfirmRequest();
            request.DialogOpen = _confirmOpen;
            request.Focus = _confirmFocus;
            request.SettingsOpen = _settingsOpen;
            request.CreditsVisible = _creditsVisible;
            if (_settingsOpen || _creditsVisible)
            {
                request.RestartScreen = false;
                request.Playing = false;
            }
            else
            {
                request.Playing = _session != null && _session.Phase == GamePhase.Playing;
                request.RestartScreen = _session != null && GameSession.PrimaryRestartsRun(_session.Phase);
            }
            EnsureSettings();
            request.ConfirmInPlay = _settings.ConfirmRestartInPlay;
            bool anyPurchase = _loadout != null && _loadout.State != null && _loadout.State.HasPurchase();
            int progressWave = _session != null ? _session.WaveIndex : 1;
            int progressScore = _session != null ? _session.Score : 0;
            int progressCredits = _session != null ? _session.Credits : 0;
            bool hasProgress = GameSession.HasRunProgress(progressWave, progressScore, progressCredits, anyPurchase);
            request.ConfirmNewRun = GameSession.ShouldConfirmNewRun(_settings.ConfirmRestartNewRun, hasProgress);
            request.Escape = Input.GetKeyDown(KeyCode.Escape);
            request.Start = GamepadInput.PausePressed() && !request.Escape;
            request.Cancel = GamepadInput.CancelPressed() && !request.Escape;
            request.Submit = GamepadInput.ConfirmPressed();
            Vector2 confirmStick = GamepadInput.UiNavCombined();
            request.FocusDelta = HangarPadNav.DominantStep(confirmStick.x, confirmStick.y, HangarPadNav.Flick);
            return request;
        }

        private void OnConfirmScrim()
        {
            ConfirmRequest request = ReadConfirmRequest();
            request.Scrim = true;
            request.Submit = false;
            request.Escape = false;
            request.Start = false;
            request.Cancel = false;
            request.FocusDelta = 0;
            ApplyConfirmRoute(ConfirmDialogRouter.Route(request), request);
            ApplyConfirmClock();
        }

        private void OnConfirmYesClick()
        {
            ConfirmRequest request = ReadConfirmRequest();
            request.Submit = true;
            request.Focus = ConfirmDialogRouter.FocusYes;
            request.Escape = false;
            request.Start = false;
            request.Cancel = false;
            request.Scrim = false;
            request.FocusDelta = 0;
            ApplyConfirmRoute(ConfirmDialogRouter.Route(request), request);
            ApplyConfirmClock();
        }

        private void OnConfirmNoClick()
        {
            ConfirmRequest request = ReadConfirmRequest();
            request.Submit = true;
            request.Focus = ConfirmDialogRouter.FocusNo;
            request.Escape = false;
            request.Start = false;
            request.Cancel = false;
            request.Scrim = false;
            request.FocusDelta = 0;
            ApplyConfirmRoute(ConfirmDialogRouter.Route(request), request);
            ApplyConfirmClock();
        }

        private void ApplyConfirmRoute(ConfirmAction action, ConfirmRequest request)
        {
            if (action == ConfirmAction.Blocked)
            {
                if (request.FocusDelta == 0)
                {
                    _confirmNavHeld = false;
                }

                return;
            }

            if (action == ConfirmAction.None)
            {
                return;
            }

            if (action == ConfirmAction.MoveFocus)
            {
                float now = Time.unscaledTime;
                if (_confirmNavHeld && now < _confirmNavRepeatAt)
                {
                    return;
                }

                _confirmFocus = ConfirmDialogRouter.MoveFocus(_confirmFocus, request.FocusDelta);
                RefreshConfirmFocus();
                _confirmNavRepeatAt = now + (_confirmNavHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
                _confirmNavHeld = true;
                return;
            }

            if (action == ConfirmAction.Open)
            {
                OpenConfirm(KindForRequest(request));
                return;
            }

            if (action == ConfirmAction.No)
            {
                CloseConfirm();
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayUiClick();
                }

                return;
            }

            if (action == ConfirmAction.Yes)
            {
                ConfirmKind chosen = _confirmOpen ? _confirmKind : KindForRequest(request);
                CloseConfirm();
                if (chosen == ConfirmKind.NewRun)
                {
                    if (_game != null && _game.HasContinueOffer)
                    {
                        OpenRunSetup(RunSetupKind.Abandon);
                    }
                    else
                    {
                        OnPrimary();
                    }
                }
                else
                {
                    OnAbort();
                }
            }
        }

        private static ConfirmKind KindForRequest(ConfirmRequest request)
        {
            if (request.AbandonClick)
            {
                return ConfirmKind.NewRun;
            }

            bool abort = request.Playing && (request.Escape || request.Start || request.AbortClick);
            if (abort)
            {
                return ConfirmKind.AbortWave;
            }

            return ConfirmKind.NewRun;
        }

        private void OpenConfirm(ConfirmKind kind)
        {
            if (_confirmOpen)
            {
                return;
            }

            _confirmOpen = true;
            _confirmKind = kind;
            _confirmFocus = ConfirmDialogRouter.DefaultFocus();
            _confirmNavHeld = false;
            _confirmOpenedFrame = Time.frameCount;
            if (_confirmRoot != null)
            {
                _confirmRoot.SetActive(true);
                _confirmRoot.transform.SetAsLastSibling();
            }

            RefreshConfirmCopy();
            RefreshConfirmFocus();
            SetConfirmNavigationLock(true);
            ApplyConfirmClock();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void CloseConfirm()
        {
            _confirmOpen = false;
            _confirmKind = ConfirmKind.None;
            _confirmNavHeld = false;
            if (_confirmRoot != null)
            {
                _confirmRoot.SetActive(false);
            }

            SetConfirmNavigationLock(false);
            ApplyConfirmClock();
        }

        private void ApplyConfirmClock()
        {
            bool waveLive = _session != null && _session.Phase == GamePhase.Playing;
            bool abortDialog = _confirmOpen && _confirmKind == ConfirmKind.AbortWave;
            if (ConfirmPause.DismissAbort(abortDialog, waveLive))
            {
                _confirmOpen = false;
                _confirmKind = ConfirmKind.None;
                _confirmNavHeld = false;
                if (_confirmRoot != null)
                {
                    _confirmRoot.SetActive(false);
                }

                SetConfirmNavigationLock(false);
                abortDialog = false;
            }

            Time.timeScale = ConfirmPause.TimeScale(abortDialog, waveLive);
            bool silenceNow = ConfirmPause.SilenceShip(abortDialog, waveLive);
            if (silenceNow)
            {
                if (!_confirmSilencedShip && _ship != null)
                {
                    _ship.SetInputPaused(true);
                    _confirmSilencedShip = true;
                }

                return;
            }

            if (!_confirmSilencedShip)
            {
                return;
            }

            _confirmSilencedShip = false;
            if (ConfirmPause.RestoreShipInput(true, false, waveLive) && _ship != null)
            {
                _ship.SetInputPaused(false);
            }
        }

        private void SetConfirmNavigationLock(bool locked)
        {
            if (_settingsOpen)
            {
                return;
            }

            SetSettingsNavigationLock(locked);
        }

        private void RefreshConfirmCopy()
        {
            bool newRun = _confirmKind == ConfirmKind.NewRun;
            if (_confirmTitle != null)
            {
                _confirmTitle.text = newRun
                    ? Loc.T("ui.confirm.new_run_title", "New Run?")
                    : Loc.T("ui.confirm.abort_title", "Abort wave?");
            }

            if (_confirmBody != null)
            {
                if (newRun)
                {
                    int wave = _session != null ? _session.WaveIndex : 1;
                    int score = _session != null ? _session.Score : 0;
                    int credits = _session != null ? _session.Credits : 0;
                    if (_game != null && _game.HasContinueOffer && _game.PendingContinue != null)
                    {
                        RunSaveData pendingSave = _game.PendingContinue;
                        wave = pendingSave.WaveIndex;
                        score = pendingSave.Score;
                        credits = pendingSave.Credits;
                    }

                    _confirmBody.text = Loc.Tf(
                        "ui.confirm.new_run_body",
                        "Start over from wave 1? You reach wave {0}, score {1}, credits {2} are lost.",
                        wave,
                        score,
                        credits);
                }
                else
                {
                    _confirmBody.text = Loc.T("ui.confirm.abort_body", "Return to the hangar?");
                }
            }

            if (_confirmYesLabel != null)
            {
                _confirmYesLabel.text = Loc.T("ui.confirm.yes", "Yes");
            }

            if (_confirmNoLabel != null)
            {
                _confirmNoLabel.text = Loc.T("ui.confirm.no", "No");
            }
        }

        private void RefreshConfirmFocus()
        {
            if (_confirmYes != null)
            {
                UiTheme.SetPadFocus(_confirmYes.gameObject, _confirmFocus == ConfirmDialogRouter.FocusYes, false);
            }

            if (_confirmNo != null)
            {
                UiTheme.SetPadFocus(_confirmNo.gameObject, _confirmFocus == ConfirmDialogRouter.FocusNo, false);
            }
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
            // The panel always accepts d-pad and stick so a filtered source cannot trap the player.
            Vector2 settingsStick = GamepadInput.UiNavCombined(PadNavSource.Both);
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
            if (_rebindOverlay != null)
            {
                _rebindOverlay.Close();
            }

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

            if (_rebindOverlay != null)
            {
                _rebindOverlay.Close();
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
                return;
            }

            if (rowId == SettingsRowId.ReduceEffects)
            {
                ToggleReduceEffects();
                return;
            }

            if (rowId == SettingsRowId.AssistMode)
            {
                ToggleAssistMode();
                return;
            }

            if (rowId == SettingsRowId.HintMode)
            {
                StepHintMode(direction);
                return;
            }

            if (rowId == SettingsRowId.HintSize)
            {
                StepHintSize(direction);
                return;
            }

            if (rowId == SettingsRowId.ConfirmAbort)
            {
                ToggleConfirmAbort();
                return;
            }

            if (rowId == SettingsRowId.ConfirmNewRun)
            {
                ToggleConfirmNewRun();
                return;
            }

            if (rowId == SettingsRowId.PadNav)
            {
                StepPadNav(direction);
                return;
            }

            if (rowId == SettingsRowId.WindowMode)
            {
                StepWindowMode(direction);
                return;
            }

            if (rowId == SettingsRowId.Resolution)
            {
                StepResolution(direction);
                return;
            }

            if (rowId == SettingsRowId.VSync)
            {
                ToggleVSync();
                return;
            }

            if (rowId == SettingsRowId.FpsCap)
            {
                StepFpsCap(direction);
                return;
            }

            if (rowId == SettingsRowId.PromptScheme)
            {
                StepPromptScheme(direction);
                return;
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

            if (rowId == SettingsRowId.ReduceEffects)
            {
                ToggleReduceEffects();
                return;
            }

            if (rowId == SettingsRowId.AssistMode)
            {
                ToggleAssistMode();
                return;
            }

            if (rowId == SettingsRowId.HintMode)
            {
                CycleHintMode();
                return;
            }

            if (rowId == SettingsRowId.HintSize)
            {
                CycleHintSize();
                return;
            }

            if (rowId == SettingsRowId.ConfirmAbort)
            {
                ToggleConfirmAbort();
                return;
            }

            if (rowId == SettingsRowId.ConfirmNewRun)
            {
                ToggleConfirmNewRun();
                return;
            }

            if (rowId == SettingsRowId.PadNav)
            {
                CyclePadNav();
                return;
            }

            if (rowId == SettingsRowId.WindowMode)
            {
                CycleWindowMode();
                return;
            }

            if (rowId == SettingsRowId.Resolution)
            {
                CycleResolution();
                return;
            }

            if (rowId == SettingsRowId.VSync)
            {
                ToggleVSync();
                return;
            }

            if (rowId == SettingsRowId.FpsCap)
            {
                CycleFpsCap();
                return;
            }

            if (rowId == SettingsRowId.PromptScheme)
            {
                CyclePromptScheme();
                return;
            }

            if (rowId == SettingsRowId.ChangeControls)
            {
                OpenRebind();
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

            _settingsShift = SettingsScroll.ShiftForFocus(SettingsRows.ClampIndex(_settingsIndex));
            ApplySettingsScroll();
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

        public void FlashRetry()
        {
            FlashHit(0.20f, new Color(1f, 0.96f, 0.92f, 1f), 0.18f);
        }

        private void BuildFirstStart(Font display, Font body)
        {
            _firstStartRoot = new GameObject("FirstStart");
            _firstStartRoot.transform.SetParent(transform, false);
            Stretch(_firstStartRoot.AddComponent<RectTransform>(), Vector2.zero, Vector2.one);
            RaiseCanvas(_firstStartRoot, CanvasOrder.Overlay);

            GameObject firstScrim = CreateFill(
                "FirstStartScrim",
                _firstStartRoot.transform,
                UiTheme.WithAlpha(UiTheme.Void, 0.82f),
                Vector2.zero,
                Vector2.one);
            Image firstScrimImage = firstScrim.GetComponent<Image>();
            if (firstScrimImage != null)
            {
                firstScrimImage.raycastTarget = true;
            }

            GameObject firstCard = UiTheme.BuildPanel(
                "FirstStartCard",
                _firstStartRoot.transform,
                new Vector2(0.18f, 0.16f),
                new Vector2(0.82f, 0.84f),
                0.94f);
            _firstStartTitle = CreateText("FirstStartTitle", firstCard.transform, display, 26, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_firstStartTitle.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.97f));
            _firstStartTitle.color = UiTheme.Primary;
            _firstStartTitle.text = FirstRunRules.FirstDifficultyTitle();

            _firstEasy = CreateButton("FirstEasy", firstCard.transform, body, new Vector2(0.04f, 0.40f), new Vector2(0.34f, 0.82f));
            _firstNormal = CreateButton("FirstNormal", firstCard.transform, body, new Vector2(0.35f, 0.40f), new Vector2(0.65f, 0.82f));
            _firstHard = CreateButton("FirstHard", firstCard.transform, body, new Vector2(0.66f, 0.40f), new Vector2(0.96f, 0.82f));
            _firstEasyLabel = _firstEasy.GetComponentInChildren<Text>();
            _firstNormalLabel = _firstNormal.GetComponentInChildren<Text>();
            _firstHardLabel = _firstHard.GetComponentInChildren<Text>();
            FitChoiceLabel(_firstEasyLabel);
            FitChoiceLabel(_firstNormalLabel);
            FitChoiceLabel(_firstHardLabel);
            _firstEasy.onClick.AddListener(OnFirstEasy);
            _firstNormal.onClick.AddListener(OnFirstNormal);
            _firstHard.onClick.AddListener(OnFirstHard);
            LockButtonNavigation(_firstEasy);
            LockButtonNavigation(_firstNormal);
            LockButtonNavigation(_firstHard);

            _firstGo = CreateButton("FirstGo", firstCard.transform, display, new Vector2(0.04f, 0.08f), new Vector2(0.48f, 0.32f));
            _firstSkip = CreateButton("FirstSkip", firstCard.transform, display, new Vector2(0.52f, 0.08f), new Vector2(0.96f, 0.32f));
            _firstGoLabel = _firstGo.GetComponentInChildren<Text>();
            _firstSkipLabel = _firstSkip.GetComponentInChildren<Text>();
            _firstGoLabel.fontSize = 22;
            _firstSkipLabel.fontSize = 18;
            _firstGo.onClick.AddListener(OnFirstGo);
            _firstSkip.onClick.AddListener(OnFirstSkip);
            LockButtonNavigation(_firstGo);
            LockButtonNavigation(_firstSkip);
            UiTheme.ApplyButton(_firstGo, true, false, false);
            _firstStartRoot.SetActive(false);
        }

        private static void FitChoiceLabel(Text label)
        {
            if (label == null)
            {
                return;
            }

            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.lineSpacing = 1.05f;
        }

        private void BuildTutorialPlayChrome(Font display)
        {
            _tutorialBanner = CreateText("TutorialBanner", transform, display, 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_tutorialBanner.rectTransform, new Vector2(0.16f, 0.18f), new Vector2(0.62f, 0.30f));
            _tutorialBanner.color = UiTheme.Primary;
            _tutorialBanner.horizontalOverflow = HorizontalWrapMode.Wrap;
            _tutorialBanner.verticalOverflow = VerticalWrapMode.Truncate;
            _tutorialBanner.gameObject.SetActive(false);

            _tutorialSkipPlay = CreateButton(
                "TutorialSkipPlay",
                transform,
                display,
                new Vector2(0.64f, 0.18f),
                new Vector2(0.90f, 0.30f));
            Text skipPlayLabel = _tutorialSkipPlay.GetComponentInChildren<Text>();
            if (skipPlayLabel != null)
            {
                skipPlayLabel.fontSize = 18;
                skipPlayLabel.text = FirstRunRules.SkipTutorialLabel();
            }

            _tutorialSkipPlay.onClick.AddListener(OnTutorialSkipPlay);
            LockButtonNavigation(_tutorialSkipPlay);
            _tutorialSkipPlay.gameObject.SetActive(false);
        }

        private void OnTutorialSkipPlay()
        {
            if (_game != null)
            {
                _game.SkipTutorial();
            }
        }

        private bool FirstStartOpen()
        {
            return _game != null
                && _game.ShowDifficultyChooser
                && _session != null
                && _session.Phase != GamePhase.Playing;
        }

        private bool FirstStartSlotLive(int slot)
        {
            if (slot != HangarPadNav.FirstEasySlot
                && slot != HangarPadNav.FirstNormalSlot
                && slot != HangarPadNav.FirstHardSlot
                && slot != HangarPadNav.FirstGoSlot
                && slot != HangarPadNav.FirstSkipSlot)
            {
                return false;
            }

            Button choice = ButtonFromSlot(slot);
            return choice != null && choice.gameObject.activeInHierarchy && choice.IsInteractable();
        }

        private void RefreshFirstStart()
        {
            bool open = FirstStartOpen();
            if (_firstStartRoot != null)
            {
                _firstStartRoot.SetActive(open);
            }

            if (!open)
            {
                _firstStartArmed = false;
                return;
            }

            _firstStartRoot.transform.SetAsLastSibling();
            if (_firstSkip != null)
            {
                bool showSkip = _game != null && _game.TutorialPending;
                _firstSkip.gameObject.SetActive(showSkip);
            }

            PaintFirstStartChoices();
            if (_firstStartArmed)
            {
                return;
            }

            _firstStartArmed = true;
            _firstStartPick = DifficultyGrade.Normal;
            PaintFirstStartChoices();
            FocusHangarSlot(HangarPadNav.FirstNormalSlot);
        }

        private void PaintFirstStartChoices()
        {
            if (_firstStartTitle != null)
            {
                _firstStartTitle.text = FirstRunRules.FirstDifficultyTitle();
            }

            if (_firstEasyLabel != null)
            {
                _firstEasyLabel.text = FirstRunRules.EasyChoiceLabel();
            }

            if (_firstNormalLabel != null)
            {
                _firstNormalLabel.text = FirstRunRules.NormalChoiceLabel();
            }

            if (_firstHardLabel != null)
            {
                _firstHardLabel.text = FirstRunRules.HardChoiceLabel();
            }

            if (_firstGoLabel != null)
            {
                _firstGoLabel.text = FirstRunRules.FirstStartLabel();
            }

            if (_firstSkipLabel != null)
            {
                _firstSkipLabel.text = FirstRunRules.SkipTutorialLabel();
            }

            UiTheme.ApplyButton(_firstEasy, _firstStartPick == DifficultyGrade.Easy, false, false);
            UiTheme.ApplyButton(_firstNormal, _firstStartPick == DifficultyGrade.Normal, false, false);
            UiTheme.ApplyButton(_firstHard, _firstStartPick == DifficultyGrade.Hard, false, false);
            UiTheme.ApplyButton(_firstGo, true, false, false);
        }

        private void OnFirstEasy()
        {
            _firstStartPick = DifficultyGrade.Easy;
            PaintFirstStartChoices();
        }

        private void OnFirstNormal()
        {
            _firstStartPick = DifficultyGrade.Normal;
            PaintFirstStartChoices();
        }

        private void OnFirstHard()
        {
            _firstStartPick = DifficultyGrade.Hard;
            PaintFirstStartChoices();
        }

        private void OnFirstGo()
        {
            ConfirmFirstStartFromUi();
        }

        private void OnFirstSkip()
        {
            if (_game != null)
            {
                _game.SkipFirstStart(_firstStartPick);
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void ConfirmFirstStartFromUi()
        {
            if (_game == null || !FirstStartOpen())
            {
                return;
            }

            _game.ConfirmFirstStart(_firstStartPick);
        }

        private void SyncFirstStartPick()
        {
            if (!FirstStartOpen())
            {
                return;
            }

            if (_padSlot == HangarPadNav.FirstEasySlot)
            {
                _firstStartPick = DifficultyGrade.Easy;
            }
            else if (_padSlot == HangarPadNav.FirstNormalSlot)
            {
                _firstStartPick = DifficultyGrade.Normal;
            }
            else if (_padSlot == HangarPadNav.FirstHardSlot)
            {
                _firstStartPick = DifficultyGrade.Hard;
            }

            PaintFirstStartChoices();
        }

        private bool ConsumeFirstStartInput()
        {
            if (!FirstStartOpen())
            {
                return false;
            }

            bool escapeDown = Input.GetKeyDown(KeyCode.Escape);
            bool startDown = GamepadInput.PausePressed() && !escapeDown;
            if (startDown)
            {
                ConfirmFirstStartFromUi();
                return true;
            }

            bool keyboardStart = Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Space);
            if (keyboardStart)
            {
                EventSystem keys = EventSystem.current;
                int liveSlot = keys != null
                    ? SlotFromSelected(keys.currentSelectedGameObject)
                    : HangarPadNav.PrimarySlot;
                if (!HangarPadNav.IsFirstStartSlot(liveSlot))
                {
                    FocusHangarSlot(HangarPadNav.OverlayHome(PadSelectableMask()));
                }

                return true;
            }

            if (!escapeDown)
            {
                return false;
            }

            if (_game != null && _game.TutorialPending)
            {
                _game.SkipFirstStart(_firstStartPick);
            }

            return true;
        }

        private bool ConsumeTutorialSkip()
        {
            if (_game == null || !_game.TutorialActive || !GamepadInput.PausePressed())
            {
                return false;
            }

            _game.SkipTutorial();
            return true;
        }

        private void ApplyHangarSkipLabel()
        {
            if (_gotItLabel == null)
            {
                return;
            }

            if (_game != null && _game.TutorialPending)
            {
                _gotItLabel.text = FirstRunRules.SkipTutorialLabel();
                return;
            }

            _gotItLabel.text = Loc.T("ui.got_it", "Got it");
        }

        private void RefreshTutorialChrome()
        {
            bool live = _game != null && _game.TutorialActive;
            if (_tutorialBanner != null)
            {
                _tutorialBanner.gameObject.SetActive(live);
                if (live)
                {
                    PaintPrompt(_tutorialBanner, FirstRunRules.PromptLine(_game.TutorialPrompt, ControlLabels.PreferPad));
                }
            }

            if (_tutorialSkipPlay != null)
            {
                _tutorialSkipPlay.gameObject.SetActive(live);
                Text skipPlayLabel = _tutorialSkipPlay.GetComponentInChildren<Text>();
                if (skipPlayLabel != null)
                {
                    skipPlayLabel.text = FirstRunRules.SkipTutorialLabel();
                }
            }
        }

        private void NoteControlDevice()
        {
            InputSchemeDriver.Poll(SettingsState.PromptPreference);
            if (InputSchemeDriver.Changed)
            {
                RestylePrompts();
            }

            Vector2 padFly = GamepadInput.PadMoveStick();
            Vector2 aimFly = GamepadInput.AimStick();
            bool padFire = Input.GetButton(GamepadInput.FirePad);
            if (padFly.sqrMagnitude > 0.02f || aimFly.sqrMagnitude > 0.02f || padFire)
            {
                ControlLabels.NotePad();
                return;
            }

            bool keyFly = Input.GetKey(KeyCode.W)
                || Input.GetKey(KeyCode.A)
                || Input.GetKey(KeyCode.S)
                || Input.GetKey(KeyCode.D)
                || Input.GetKey(KeyCode.Space)
                || Input.GetMouseButton(0);
            if (keyFly)
            {
                ControlLabels.NoteKeyboard();
            }
        }

        private void TickTutorialPrompts()
        {
            if (_game == null || !_game.TutorialActive)
            {
                return;
            }

            Vector2 keyFly = GamepadInput.MoveStick();
            Vector2 stickFly = GamepadInput.PadMoveStick();
            bool moved = keyFly.sqrMagnitude > 0.02f || stickFly.sqrMagnitude > 0.02f;
            _game.TickTutorial(moved, GamepadInput.FireHeld());
            if (_tutorialBanner != null)
            {
                PaintPrompt(_tutorialBanner, FirstRunRules.PromptLine(_game.TutorialPrompt, ControlLabels.PreferPad));
            }
        }

        private void RememberRecommendedUpgrade()
        {
            _pulseShopIndex = -1;
            if (_game == null || _session == null || _loadout == null || _loadout.State == null)
            {
                return;
            }

            _pulseShopIndex = RunSummary.RecommendedShopIndex(
                _session.Credits,
                _loadout.State,
                _session.LastResolvedWave,
                _session.Phase);
        }

        private void NotePhaseFocus()
        {
            if (_session == null)
            {
                return;
            }

            GamePhase phaseNow = _session.Phase;
            bool enteredFail = phaseNow == GamePhase.Failed && _failFocusPhase != GamePhase.Failed;
            _failFocusPhase = phaseNow;
            if (!enteredFail || (_game != null && _game.HasContinueOffer))
            {
                return;
            }

            FocusHangarSlot(HangarPadNav.PrimarySlot);
        }

        private void PulseRecommendedUpgrade()
        {
            if (_pulseShopIndex < 0 || _buyButtons == null || _pulseShopIndex >= _buyButtons.Length)
            {
                return;
            }

            Button shopButton = _buyButtons[_pulseShopIndex];
            if (shopButton == null || !shopButton.gameObject.activeInHierarchy)
            {
                return;
            }

            Outline shopRing = shopButton.GetComponent<Outline>();
            if (shopRing == null)
            {
                return;
            }

            float shopPulse = EffectScale.UiPulse(
                SettingsState.ReduceEffectsEnabled,
                Mathf.PingPong(Time.unscaledTime * 2.2f, 1f));
            if (EffectScale.FreezePulse(SettingsState.ReduceEffectsEnabled))
            {
                shopPulse = 0f;
            }
            shopRing.effectColor = Color.Lerp(UiTheme.Secondary, UiTheme.Focus, shopPulse);
            shopRing.enabled = true;
        }
    }
}
