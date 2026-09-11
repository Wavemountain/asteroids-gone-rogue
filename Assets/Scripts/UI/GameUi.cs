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
        private GameObject _audioPanel;
        private GameObject _tutorialRoot;
        private GameObject _summaryRoot;
        private Text _summaryTitle;
        private Text _summaryBody;
        private Text _waveMedal;
        private Text _continueHint;
        private Text _badgeRow;
        private Image _hitFlash;
        private GameObject _hudPlate;
        private bool _tutorialDismissed;
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
        private bool _creditsVisible;
        private float _creditsScroll;

        public const string FirstHangarHintKey = "agr.ui.firstHangarHint";
        public const string HangarControlsHint =
            "Abort (Esc)  ·  Q / RMB fire modes (discover Spread / Pierce when owned)";
        public const string MedalLadderPrefix = "MEDALS";
        public const string HangarHintBody =
            "WASD move · mouse aim\nLMB / Space shoot\nAbort (Esc) leaves the wave\n"
            + "Q / RMB fire modes\n(discover Spread / Pierce when owned)\n\n"
            + "Clear a wave to earn credits and upgrades.\n"
            + "Medal ladder (top-left): ★ Scout Wing at wave 3.\n\n"
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

            _hud.gameObject.SetActive(true);
            _hud.text = BuildHud(playing);
            if (_audioPanel != null)
            {
                _audioPanel.SetActive(!playing);
            }

            _hint.text = playing
                ? "WASD move  ·  Mouse aim  ·  LMB / Space fire  ·  Q / RMB fire mode  ·  Esc abort"
                : "WASD move  ·  Mouse aim  ·  LMB / Space fire  ·  " + HangarControlsHint;
            RefreshWorldBadge();
            RefreshBadgeRow(playing);
            RefreshFirstHangarHint();

            if (playing)
            {
                return;
            }

            _credits.text = "Credits: " + _session.Credits;
            switch (_session.Phase)
            {
                case GamePhase.WaveClear:
                    _statusBase = HangarControlsHint;
                    _primaryLabel.text = "Next Wave";
                    break;
                case GamePhase.Failed:
                    _statusBase = DamageCauseText.PlayerFaultLine(FailReasonText());
                    _primaryLabel.text = "Retry Wave";
                    break;
                default:
                    _statusBase = HangarReadyStatus();
                    _primaryLabel.text = "Start Wave";
                    break;
            }

            RefreshRunSummary(playing);
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
                    + "  ·  World " + ContentFactory.WorldIndexForWave(_session.WaveIndex)
                    + " layout: " + ArenaLayout.Title(ArenaLayout.ForWave(_session.WaveIndex));
            string tease = RunSummary.MonsterTeaser(_session.WaveIndex);
            string hook = RunSummary.NextMedalHook(_session.WaveIndex);
            string extra = string.Empty;
            if (!string.IsNullOrEmpty(tease))
            {
                extra += "\n" + tease;
            }

            if (!string.IsNullOrEmpty(hook))
            {
                extra += "\n" + hook;
            }

            return waveLine + extra + "\n" + HangarControlsHint;
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
            Font display = UiFonts.Display();
            Font body = UiFonts.Body();
            CreateFill("Scrim", transform, new Color(0.015f, 0.02f, 0.04f, 0.22f), new Vector2(0f, 0f), new Vector2(1f, 1f));
            _hitFlash = CreateFill("ScreenFlash", transform, new Color(1f, 0.88f, 0.72f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 1f)).GetComponent<Image>();

            _hudPlate = CreatePanel("HudPlate", transform, new Color(0.02f, 0.035f, 0.06f, 0.72f),
                new Vector2(0.012f, 0.605f), new Vector2(0.395f, 0.875f));
            CreateFill("HudPlateRule", _hudPlate.transform, new Color(1f, 0.72f, 0.28f, 0.55f),
                new Vector2(0.04f, 0.0f), new Vector2(0.96f, 0.018f));

            _title = CreateText("Title", transform, display, 46, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_title.rectTransform, new Vector2(0.16f, 0.875f), new Vector2(0.84f, 0.985f));
            _title.text = productTitle;
            _title.color = new Color(1f, 0.82f, 0.38f);
            AddReadability(_title, true);

            _world = CreateText("WorldBadge", transform, display, 30, TextAnchor.UpperRight, FontStyle.Bold);
            Stretch(_world.rectTransform, new Vector2(0.62f, 0.86f), new Vector2(0.97f, 0.98f));
            _world.color = new Color(1f, 0.84f, 0.32f);
            AddReadability(_world, true);

            _hud = CreateText("Hud", transform, body, 22, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(_hud.rectTransform, new Vector2(0.03f, 0.62f), new Vector2(0.5f, 0.775f));
            _hud.color = new Color(0.96f, 0.97f, 0.94f);
            AddReadability(_hud, false);

            _badgeRow = CreateText("BadgeRow", transform, display, 16, TextAnchor.UpperLeft, FontStyle.Bold);
            Stretch(_badgeRow.rectTransform, new Vector2(0.03f, 0.775f), new Vector2(0.62f, 0.86f));
            _badgeRow.color = new Color(1f, 0.86f, 0.42f);
            AddReadability(_badgeRow, false);

            _hint = CreateText("Hint", transform, body, 18, TextAnchor.LowerCenter, FontStyle.Normal);
            Stretch(_hint.rectTransform, new Vector2(0.1f, 0.018f), new Vector2(0.9f, 0.078f));
            _hint.color = new Color(0.82f, 0.88f, 0.92f);
            _hint.text = "WASD move  ·  Mouse aim  ·  Left mouse / Space fire";
            AddReadability(_hint, false);

            _menuRoot = CreatePanel("HangarPanel", transform, new Color(0.025f, 0.038f, 0.06f, 0.94f),
                new Vector2(0.185f, 0.035f), new Vector2(0.815f, 0.725f));
            CreateFill("HangarHeader", _menuRoot.transform, new Color(1f, 0.58f, 0.16f, 0.28f),
                new Vector2(0f, 0.962f), new Vector2(1f, 1f));
            CreateFill("HangarRule", _menuRoot.transform, new Color(1f, 0.78f, 0.34f, 0.88f),
                new Vector2(0.04f, 0.955f), new Vector2(0.96f, 0.962f));
            CreateFill("HangarInner", _menuRoot.transform, new Color(0.04f, 0.07f, 0.1f, 0.35f),
                new Vector2(0.012f, 0.018f), new Vector2(0.988f, 0.948f));

            BuildRunSummary(display, body);

            _status = CreateText("Status", _menuRoot.transform, body, 17, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_status.rectTransform, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.95f));
            _status.color = new Color(0.97f, 0.95f, 0.88f);

            _credits = CreateText("Credits", _menuRoot.transform, body, 20, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_credits.rectTransform, new Vector2(0.06f, 0.785f), new Vector2(0.94f, 0.84f));
            _credits.color = new Color(0.45f, 0.92f, 1f);

            _primary = CreateButton("Primary", _menuRoot.transform, display, new Vector2(0.28f, 0.675f), new Vector2(0.72f, 0.75f));
            _primaryLabel = _primary.GetComponentInChildren<Text>();
            _primary.onClick.AddListener(OnPrimary);
            Image primaryPlate = _primary.targetGraphic as Image;
            if (primaryPlate != null)
            {
                primaryPlate.color = new Color(0.42f, 0.26f, 0.08f, 0.98f);
            }

            _abortButton = CreateButton("AbortWave", transform, body, new Vector2(0.78f, 0.09f), new Vector2(0.97f, 0.155f));
            _abortLabel = _abortButton.GetComponentInChildren<Text>();
            _abortLabel.text = "Abort → Hangar";
            _abortLabel.fontSize = 16;
            _abortButton.onClick.AddListener(OnAbort);
            _abortButton.gameObject.SetActive(false);

            _creditsButton = CreateButton("OpenCredits", transform, body, new Vector2(0.78f, 0.09f), new Vector2(0.97f, 0.155f));
            Text creditsLabel = _creditsButton.GetComponentInChildren<Text>();
            creditsLabel.text = "Credits";
            creditsLabel.fontSize = 16;
            _creditsButton.onClick.AddListener(ShowEndCredits);

            BuildShop(display, body);
            BuildAudioControls(body);
            BuildFirstHangarHint(display, body);
            BuildEndCredits(display, body);
        }

        private void BuildShop(Font display, Font body)
        {
            BuildGroupHeader(display, ShopCatalog.HullHeader, new Vector2(0.03f, 0.605f), new Vector2(0.60f, 0.66f));
            BuildGroupHeader(display, ShopCatalog.WeaponsHeader, new Vector2(0.615f, 0.605f), new Vector2(0.80f, 0.66f));
            BuildGroupHeader(display, ShopCatalog.DefenseHeader, new Vector2(0.815f, 0.605f), new Vector2(0.97f, 0.66f));

            int shopCount = ShopCatalog.Items.Length;
            _buyButtons = new Button[shopCount];
            _buyLabels = new Text[shopCount];
            int hullIndex = 0;
            int weaponIndex = 0;
            int defenseIndex = 0;
            for (int i = 0; i < shopCount; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                Vector2 min;
                Vector2 max;
                ShopButtonRect(item.Group, ref hullIndex, ref weaponIndex, ref defenseIndex, out min, out max);
                Button button = CreateButton("Buy_" + item.Id, _menuRoot.transform, body, min, max);
                int captured = i;
                button.onClick.AddListener(() => OnBuy(ShopCatalog.Items[captured].Id));
                BindShopHover(button, item);
                _buyButtons[i] = button;
                _buyLabels[i] = button.GetComponentInChildren<Text>();
                _buyLabels[i].fontSize = 13;
                _buyLabels[i].fontStyle = FontStyle.Bold;
            }
        }

        private static void ShopButtonRect(
            ShopGroup group,
            ref int hullIndex,
            ref int weaponIndex,
            ref int defenseIndex,
            out Vector2 min,
            out Vector2 max)
        {
            const float ButtonHeight = 0.068f;
            const float RowStep = 0.078f;
            if (group == ShopGroup.Weapons)
            {
                float top = 0.59f - weaponIndex * RowStep;
                min = new Vector2(0.615f, top - ButtonHeight);
                max = new Vector2(0.80f, top);
                weaponIndex++;
                return;
            }

            if (group == ShopGroup.Defense)
            {
                float top = 0.59f - defenseIndex * RowStep;
                min = new Vector2(0.815f, top - ButtonHeight);
                max = new Vector2(0.97f, top);
                defenseIndex++;
                return;
            }

            int col = hullIndex % 4;
            int row = hullIndex / 4;
            float x0 = 0.03f + col * 0.145f;
            min = new Vector2(x0, 0.59f - row * RowStep - ButtonHeight);
            max = new Vector2(x0 + 0.138f, 0.59f - row * RowStep);
            hullIndex++;
        }

        private void BuildGroupHeader(Font font, string label, Vector2 min, Vector2 max)
        {
            Text header = CreateText("Group_" + label, _menuRoot.transform, font, 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(header.rectTransform, min, max);
            header.color = new Color(1f, 0.84f, 0.46f);
            header.text = label;
            CreateFill("Rule_" + label, _menuRoot.transform, new Color(1f, 0.7f, 0.28f, 0.7f),
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

        private void BuildRunSummary(Font display, Font body)
        {
            _summaryRoot = CreatePanel("RunSummaryCard", _menuRoot.transform, new Color(0.04f, 0.07f, 0.11f, 0.97f),
                new Vector2(0.04f, 0.81f), new Vector2(0.96f, 0.965f));
            CreateFill("SummaryHeader", _summaryRoot.transform, new Color(1f, 0.58f, 0.16f, 0.3f),
                new Vector2(0f, 0.78f), new Vector2(1f, 1f));
            CreateFill("SummaryRule", _summaryRoot.transform, new Color(1f, 0.78f, 0.34f, 0.85f),
                new Vector2(0.06f, 0.77f), new Vector2(0.94f, 0.79f));

            _summaryTitle = CreateText("SummaryTitle", _summaryRoot.transform, display, 17, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_summaryTitle.rectTransform, new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.97f));
            _summaryTitle.color = new Color(1f, 0.86f, 0.44f);

            _summaryBody = CreateText("SummaryBody", _summaryRoot.transform, body, 16, TextAnchor.UpperCenter, FontStyle.Normal);
            Stretch(_summaryBody.rectTransform, new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.76f));
            _summaryBody.color = new Color(0.95f, 0.94f, 0.88f);

            _waveMedal = CreateText("WaveMedal", _summaryRoot.transform, display, 14, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(_waveMedal.rectTransform, new Vector2(0.04f, 0.155f), new Vector2(0.96f, 0.28f));
            _waveMedal.color = new Color(1f, 0.86f, 0.42f);

            _continueHint = CreateText("ContinueHint", _summaryRoot.transform, body, 15, TextAnchor.LowerCenter, FontStyle.Bold);
            Stretch(_continueHint.rectTransform, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.155f));
            _continueHint.color = new Color(0.5f, 0.92f, 1f);
            _summaryRoot.SetActive(false);
        }

        private void RefreshRunSummary(bool playing)
        {
            if (_summaryRoot == null)
            {
                return;
            }

            bool show = !playing
                && (_session.Phase == GamePhase.WaveClear || _session.Phase == GamePhase.Failed);
            _summaryRoot.SetActive(show);
            if (_credits != null)
            {
                _credits.gameObject.SetActive(!show);
            }

            if (_status != null)
            {
                if (show)
                {
                    Stretch(_status.rectTransform, new Vector2(0.04f, 0.755f), new Vector2(0.96f, 0.805f));
                }
                else
                {
                    Stretch(_status.rectTransform, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.95f));
                }
            }

            if (!show)
            {
                return;
            }

            int wave = _session.LastResolvedWave > 0 ? _session.LastResolvedWave : _session.WaveIndex;
            int world = ContentFactory.WorldIndexForWave(wave);
            LoadoutState loadout = _loadout != null ? _loadout.State : null;
            _summaryTitle.text = RunSummary.Title(_session.Phase, FailReasonText());
            string body = RunSummary.StatsLine(_session.Score, wave, world)
                + "\n" + RunSummary.CreditsLine(_session.Credits, _session.LastCreditsAwarded)
                + "\n" + RunSummary.UpgradesLine(loadout);
            if (_game != null && _game.LastRunWasNewBest)
            {
                body += "\nNEW BEST";
            }

            _summaryBody.text = body;
            bool medal = RunSummary.ShowWaveMedal(_session.LastResolvedWave, _session.Phase);
            if (_waveMedal != null)
            {
                _waveMedal.gameObject.SetActive(medal);
                if (medal)
                {
                    _waveMedal.text = RunSummary.WaveMedal(_session.LastResolvedWave);
                    _waveMedal.color = RunSummary.IsWorld3EntryLine(_session.LastResolvedWave)
                        ? new Color(0.55f, 0.9f, 1f)
                        : new Color(1f, 0.84f, 0.38f);
                }
            }

            bool hint = RunSummary.ShowContinueHint(_session.LastResolvedWave, _session.Phase)
                || RunSummary.ShowFailContinue(_session.Phase);
            if (medal)
            {
                Stretch(_summaryBody.rectTransform, new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.76f));
                Stretch(_continueHint.rectTransform, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.155f));
            }
            else
            {
                Stretch(_summaryBody.rectTransform, new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.76f));
                Stretch(_continueHint.rectTransform, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.24f));
            }

            _continueHint.gameObject.SetActive(hint);
            if (hint)
            {
                _continueHint.text = _session.Phase == GamePhase.Failed
                    ? RunSummary.FailContinueHint(FailReasonText(), _session.WaveIndex)
                    : RunSummary.ContinueHint(_session.LastResolvedWave, _session.Credits, loadout);
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
                _hitFlash.color = new Color(1f, 0.88f, 0.72f, 0f);
                _hitFlashStrength = 0f;
                return;
            }

            float pulse = Mathf.Clamp01((_hitFlashUntil - Time.unscaledTime) / 0.12f);
            _hitFlash.color = new Color(1f, 0.82f, 0.62f, _hitFlashStrength * pulse);
        }

        private void BuildFirstHangarHint(Font display, Font body)
        {
            _tutorialDismissed = PlayerPrefs.GetInt(FirstHangarHintKey, 0) == 1;
            _tutorialRoot = CreatePanel("FirstHangarHint", transform, new Color(0.03f, 0.045f, 0.07f, 0.96f),
                new Vector2(0.008f, 0.08f), new Vector2(0.185f, 0.74f));
            CreateFill("HintHeader", _tutorialRoot.transform, new Color(1f, 0.58f, 0.16f, 0.3f),
                new Vector2(0f, 0.94f), new Vector2(1f, 1f));
            CreateFill("HintRule", _tutorialRoot.transform, new Color(1f, 0.78f, 0.34f, 0.85f),
                new Vector2(0.08f, 0.932f), new Vector2(0.92f, 0.94f));

            Text title = CreateText("HintTitle", _tutorialRoot.transform, display, 16, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(title.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.97f));
            title.color = new Color(1f, 0.86f, 0.44f);
            title.text = "First flight";

            Text bodyText = CreateText("HintBody", _tutorialRoot.transform, body, 13, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(bodyText.rectTransform, new Vector2(0.07f, 0.2f), new Vector2(0.93f, 0.85f));
            bodyText.color = new Color(0.92f, 0.92f, 0.88f);
            bodyText.text = HangarHintBody;

            Button gotIt = CreateButton("DismissHint", _tutorialRoot.transform, display,
                new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.18f));
            gotIt.GetComponentInChildren<Text>().text = "Got it";
            gotIt.GetComponentInChildren<Text>().fontSize = 15;
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
            if (_hudPlate != null)
            {
                _hudPlate.SetActive(!firstHangar);
            }
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

        private void BuildEndCredits(Font display, Font body)
        {
            _endCreditsRoot = CreatePanel("EndCredits", transform, new Color(0.012f, 0.018f, 0.04f, 0.96f),
                new Vector2(0f, 0f), new Vector2(1f, 1f));
            Image scrim = _endCreditsRoot.GetComponent<Image>();
            if (scrim != null)
            {
                scrim.raycastTarget = true;
            }

            CreateFill("CreditsHeader", _endCreditsRoot.transform, new Color(1f, 0.58f, 0.16f, 0.32f),
                new Vector2(0.18f, 0.86f), new Vector2(0.82f, 0.94f));
            CreateFill("CreditsRule", _endCreditsRoot.transform, new Color(1f, 0.82f, 0.44f, 0.9f),
                new Vector2(0.22f, 0.852f), new Vector2(0.78f, 0.86f));
            CreateFill("CreditsPlate", _endCreditsRoot.transform, new Color(0.03f, 0.05f, 0.08f, 0.72f),
                new Vector2(0.2f, 0.16f), new Vector2(0.8f, 0.84f));

            Text title = CreateText("CreditsTitle", _endCreditsRoot.transform, display, EndCredits.TitleSize,
                TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(title.rectTransform, new Vector2(0.22f, 0.78f), new Vector2(0.78f, 0.92f));
            title.color = EndCredits.TitleColor;
            title.text = EndCredits.Title();
            AddReadability(title, true);

            GameObject window = CreatePanel("CreditsWindow", _endCreditsRoot.transform, new Color(0.02f, 0.04f, 0.07f, 0.35f),
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

            Button cont = CreateButton("CreditsContinue", _endCreditsRoot.transform, display,
                new Vector2(0.34f, 0.045f), new Vector2(0.66f, 0.13f));
            Text contLabel = cont.GetComponentInChildren<Text>();
            contLabel.text = "Continue";
            contLabel.fontSize = 20;
            Image contPlate = cont.targetGraphic as Image;
            if (contPlate != null)
            {
                contPlate.color = new Color(1f, 0.82f, 0.44f, 0.98f);
            }

            cont.onClick.AddListener(() => HideEndCredits(true));
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
                _endCreditsBody.rectTransform.anchoredPosition = Vector2.zero;
            }

            _endCreditsRoot.SetActive(true);
            _endCreditsRoot.transform.SetAsLastSibling();
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

        private void BuildAudioControls(Font font)
        {
            _audioPanel = CreatePanel("AudioPanel", transform, new Color(0.03f, 0.05f, 0.08f, 0.88f),
                new Vector2(0.68f, 0.72f), new Vector2(0.98f, 0.86f));
            GameObject panel = _audioPanel;

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

            RefreshAudioControls();
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
            _flashedLayout = ArenaLayout.Title(ArenaLayout.ForWorld(world));
            _flashedBadge = ArenaLayout.Badge(ArenaLayout.ForWorld(world));
            _worldFlashUntil = Time.unscaledTime + 2.2f;
            Stretch(_world.rectTransform, new Vector2(0.48f, 0.72f), new Vector2(0.97f, 0.98f));
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
            Stretch(_world.rectTransform, new Vector2(0.52f, 0.76f), new Vector2(0.97f, 0.98f));
            RefreshWorldBadge();
        }

        private void Update()
        {
            if (_creditsVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                HideEndCredits(true);
                return;
            }

            if (_session != null && _session.Phase == GamePhase.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                OnAbort();
            }

            if (_creditsVisible && _endCreditsBody != null)
            {
                _creditsScroll += EndCredits.ScrollSpeed * Time.unscaledDeltaTime;
                _endCreditsBody.rectTransform.anchoredPosition = new Vector2(0f, _creditsScroll);
            }

            if (_session != null && _session.Phase == GamePhase.Playing && _hud != null)
            {
                _hud.text = BuildHud(true);
            }

            ApplyHitFlash();

            if (_world == null)
            {
                return;
            }

            if (Time.unscaledTime < _worldFlashUntil)
            {
                float pulse = Mathf.PingPong(Time.unscaledTime * 3.2f, 1f);
                _world.fontSize = 30 + (int)(4f * pulse);
                bool world3 = _flashedWorld == MedalCatalog.World3EntryWorld;
                _world.color = world3
                    ? Color.Lerp(new Color(0.82f, 0.94f, 1f), new Color(0.32f, 0.68f, 0.95f), pulse)
                    : Color.Lerp(new Color(1f, 0.92f, 0.62f), new Color(1f, 0.58f, 0.18f), pulse);
                string flash = "LAYOUT SWAP\nWORLD " + _flashedWorld + "  ONLINE";
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

            if (_world.fontSize != 30 || !string.IsNullOrEmpty(_medalBeat))
            {
                _world.fontSize = 30;
                _medalBeat = string.Empty;
                Stretch(_world.rectTransform, new Vector2(0.62f, 0.86f), new Vector2(0.97f, 0.98f));
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
            _world.text = "WORLD " + world + "  ·  " + ArenaLayout.Badge(ArenaLayout.ForWorld(world));
            _world.color = new Color(1f, 0.82f, 0.28f);
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
                _badgeRow.text = MedalLadderPrefix + "\n" + row;
                _badgeRow.fontSize = playing ? 14 : 16;
                _badgeRow.color = playing
                    ? new Color(1f, 0.84f, 0.38f, 0.92f)
                    : new Color(1f, 0.84f, 0.38f);
            }
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
                && _loadout.State.HasAltFire)
            {
                fireMode = "\nFire " + _ship.Shooter.Mode;
            }

            string scoreLine = "Wave " + _session.WaveIndex
                + "   ·   Score " + _session.Score;
            if (playing)
            {
                scoreLine += PlayBestCompare();
            }

            string hangarBest = playing ? string.Empty : "\n" + BestCardLine();
            return scoreLine
                + "\nHull " + hull + "   ·   Shield " + shield
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

        private static void AddReadability(Text text, bool strong)
        {
            if (text == null)
            {
                return;
            }

            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.02f, 0.03f, 0.05f, strong ? 0.92f : 0.78f);
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
            image.color = new Color(0.16f, 0.2f, 0.28f, 0.96f);
            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 0.55f, 1f);
            colors.pressedColor = new Color(0.92f, 0.62f, 0.22f, 1f);
            colors.disabledColor = new Color(0.78f, 0.78f, 0.8f, 1f);
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
            fillImage.color = new Color(0.95f, 0.68f, 0.22f, 1f);
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
