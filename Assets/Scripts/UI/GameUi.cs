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

        private Text _title;
        private Text _hud;
        private Text _status;
        private Text _credits;
        private Text _hint;
        private GameObject _menuRoot;
        private Button _primary;
        private Text _primaryLabel;
        private readonly Button[] _buyButtons = new Button[3];
        private readonly Text[] _buyLabels = new Text[3];

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
                    _status.text = "Ship lost  ·  Score " + _session.Score + "  ·  Retry the wave";
                    _primaryLabel.text = "Retry Wave";
                    break;
                default:
                    _status.text = "Hangar  ·  Wave " + _session.WaveIndex + " ready";
                    _primaryLabel.text = "Start Wave";
                    break;
            }

            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                RefreshBuyButton(i, ShopCatalog.Items[i]);
            }
        }

        private void Construct(string productTitle)
        {
            Font font = ResolveFont();
            CreateFill("Scrim", transform, new Color(0.02f, 0.03f, 0.05f, 0.18f), new Vector2(0f, 0f), new Vector2(1f, 1f));

            _title = CreateText("Title", transform, font, 54, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_title.rectTransform, new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.98f));
            _title.text = productTitle;
            _title.color = new Color(1f, 0.78f, 0.32f);

            _hud = CreateText("Hud", transform, font, 26, TextAnchor.UpperLeft, FontStyle.Normal);
            Stretch(_hud.rectTransform, new Vector2(0.03f, 0.72f), new Vector2(0.4f, 0.86f));
            _hud.color = Color.white;

            _hint = CreateText("Hint", transform, font, 20, TextAnchor.LowerCenter, FontStyle.Normal);
            Stretch(_hint.rectTransform, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.08f));
            _hint.color = new Color(0.75f, 0.8f, 0.85f);
            _hint.text = "WASD move  ·  Mouse aim  ·  Left mouse / Space fire";

            _menuRoot = CreatePanel("HangarPanel", transform, new Color(0.04f, 0.06f, 0.09f, 0.82f),
                new Vector2(0.22f, 0.16f), new Vector2(0.78f, 0.7f));

            _status = CreateText("Status", _menuRoot.transform, font, 28, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(_status.rectTransform, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.96f));
            _status.color = new Color(0.95f, 0.95f, 0.9f);

            _credits = CreateText("Credits", _menuRoot.transform, font, 24, TextAnchor.UpperCenter, FontStyle.Normal);
            Stretch(_credits.rectTransform, new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.82f));
            _credits.color = new Color(0.7f, 0.9f, 1f);

            _primary = CreateButton("Primary", _menuRoot.transform, font, new Vector2(0.3f, 0.58f), new Vector2(0.7f, 0.7f));
            _primaryLabel = _primary.GetComponentInChildren<Text>();
            _primary.onClick.AddListener(OnPrimary);

            float y = 0.42f;
            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                Button button = CreateButton("Buy_" + item.Id, _menuRoot.transform, font,
                    new Vector2(0.08f, y - 0.12f), new Vector2(0.92f, y));
                int captured = i;
                button.onClick.AddListener(() => OnBuy(ShopCatalog.Items[captured].Id));
                _buyButtons[i] = button;
                _buyLabels[i] = button.GetComponentInChildren<Text>();
                y -= 0.15f;
            }
        }

        private void OnPrimary()
        {
            if (_game == null)
            {
                return;
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

        private void RefreshBuyButton(int index, ShopItem item)
        {
            bool owned = _loadout.State.Owns(item.Id);
            bool canBuy = _session.ShopOpen && _loadout.State.CanApply(item.Id) && _session.Credits >= item.Cost;
            _buyButtons[index].interactable = canBuy;
            string suffix = owned ? "  [owned]" : "  —  " + item.Cost + " cr";
            _buyLabels[index].text = item.Title + suffix + "\n" + item.Description;
        }

        private string BuildHud(bool playing)
        {
            int hull = _ship != null && _ship.Health != null ? _ship.Health.Hull : LoadoutState.HullHitPoints;
            int shield = _ship != null && _ship.Health != null ? _ship.Health.Shield : _loadout.State.ShieldCharges;
            string remaining = playing && _game != null
                ? "   ·   Remaining " + FindRemaining()
                : string.Empty;
            return "Wave " + _session.WaveIndex
                + "   ·   Score " + _session.Score
                + "\nHull " + hull + "   ·   Shield " + shield
                + remaining;
        }

        private int FindRemaining()
        {
            WaveManager waves = _game.GetComponent<WaveManager>();
            return waves != null ? waves.RemainingThreats : 0;
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

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
