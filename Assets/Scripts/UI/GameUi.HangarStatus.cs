using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    public sealed partial class GameUi
    {
        private GameObject _hangarHudRoot;
        private Text _hudCreditsCaption;
        private Image _hudCreditsIcon;
        private Text[] _hudCreditDigits;
        private Text _hudCreditsDelta;
        private Text[] _hudLifeCaptions;
        private Text[] _hudValueDigits;
        private Text[] _hudGhost;
        private Image[] _hudLifeIcons;
        private Image _hudLifeGhost;
        private Text _hudLifeTimes;
        private Image[] _hudHullSegs;
        private Image[] _hudShieldSegs;
        private Image _hudHullFill;
        private Image _hudShieldFill;
        private Image _hudHullDanger;
        private RectTransform _hudHullContent;
        private RectTransform _hudShieldContent;
        private readonly UiCountUp _hudCredits = new UiCountUp();
        private bool _hudCreditsReady;
        private int _hudCreditTarget = -1;
        private float _denyUntil = -1f;

        private GameObject _stripRoot;
        private CanvasGroup[] _stripGroups;
        private Text[] _stripCaptions;
        private Image[] _stripCaptionIcons;
        private Text[][] _stripDigits;
        private Image _stripHullIcon;
        private Image _stripShieldIcon;
        private Text[] _stripHullDigits;
        private Text[] _stripShieldDigits;
        private Image _stripUnderline;
        private GameObject _stripBadge;
        private CanvasGroup _stripBadgeGroup;
        private Image _stripStar;
        private float _stripStart = -1f;
        private bool _stripHold;
        private bool _stripSnap;
        private bool _recordPlayed;
        private bool _poseFreeze;
        private float _poseElapsed = 1.2f;
        private bool _poseBest;
        private bool _poseTimeBest;
        private bool _poseComboBest;

        private const int StripDigitCells = 6;
        private const int HudLifeIcons = 6;
        private const int BarSegments = 12;

        private void BuildHangarHud(Font display, Font body)
        {
            _hangarHudRoot = UiTheme.BuildPanel(
                "HangarHud",
                transform,
                HangarHudRectMin,
                HangarHudRectMax,
                0.985f,
                Color.clear,
                UiTheme.HeaderRule,
                UiTheme.PanelPlate);
            Image plate = _hangarHudRoot.GetComponent<Image>();
            if (plate != null)
            {
                plate.raycastTarget = false;
            }

            _hudCreditsCaption = CreateText("CreditsCaption", _hangarHudRoot.transform, body, 16, TextAnchor.MiddleLeft, FontStyle.Normal);
            _hudCreditsCaption.color = UiTheme.Secondary;
            PinTopLeft(_hudCreditsCaption.rectTransform, 16f, 12f, 260f, 18f);
            ClampOneLine(_hudCreditsCaption);

            _hudCreditsIcon = MakeIcon("CreditsIcon", _hangarHudRoot.transform, "hud_coin", UiTheme.Primary);
            PinTopLeft(_hudCreditsIcon.rectTransform, 16f, 38f, 36f, 36f);

            GameObject creditBox = new GameObject("CreditsValue", typeof(RectTransform));
            creditBox.transform.SetParent(_hangarHudRoot.transform, false);
            int creditCell = UiDigits.CellWidth(UiDigits.CreditSize);
            PinTopLeft(creditBox.GetComponent<RectTransform>(), 60f, 34f, creditCell * UiDigits.CreditCells, 44f);
            _hudCreditDigits = MakeDigits("CreditDigit", creditBox.transform, display, UiDigits.CreditSize, UiDigits.CreditCells, creditCell, UiTheme.Primary);

            _hudCreditsDelta = CreateText("CreditsDelta", _hangarHudRoot.transform, body, 16, TextAnchor.MiddleRight, FontStyle.Normal);
            _hudCreditsDelta.color = UiTheme.Accent;
            PinTopLeft(_hudCreditsDelta.rectTransform, 60f, 84f, 170f, 20f);
            _hudCreditsDelta.horizontalOverflow = HorizontalWrapMode.Wrap;
            _hudCreditsDelta.verticalOverflow = VerticalWrapMode.Truncate;

            Image divider = MakeIcon("Divider", _hangarHudRoot.transform, null, UiTheme.WithAlpha(UiTheme.Secondary, 0.35f));
            RectTransform dividerRt = divider.rectTransform;
            dividerRt.anchorMin = new Vector2(0f, 1f);
            dividerRt.anchorMax = new Vector2(0f, 1f);
            dividerRt.pivot = new Vector2(0.5f, 1f);
            dividerRt.sizeDelta = new Vector2(1f, 93f);
            dividerRt.anchoredPosition = new Vector2(292f, -16f);

            _hudLifeCaptions = new Text[3];
            _hudGhost = new Text[3];
            _hudValueDigits = new Text[3 * 7];
            string[] captionNames = new string[] { "LifeCaption", "HullCaption", "ShieldCaption" };
            for (int row = 0; row < 3; row++)
            {
                RectTransform rowRt = MakeRow(row);
                _hudLifeCaptions[row] = CreateText(captionNames[row], rowRt, body, 16, TextAnchor.MiddleLeft, FontStyle.Normal);
                _hudLifeCaptions[row].color = UiTheme.Secondary;
                StretchSide(_hudLifeCaptions[row].rectTransform, 0f, 80f, true);
                ClampOneLine(_hudLifeCaptions[row]);

                int valueCell = UiDigits.GlyphWidth(UiDigits.ValueSize);
                GameObject valueBox = new GameObject("Value", typeof(RectTransform));
                valueBox.transform.SetParent(rowRt, false);
                RectTransform valueRt = valueBox.GetComponent<RectTransform>();
                valueRt.anchorMin = new Vector2(1f, 0f);
                valueRt.anchorMax = new Vector2(1f, 1f);
                valueRt.pivot = new Vector2(1f, 0.5f);
                valueRt.sizeDelta = new Vector2(valueCell * 7, 0f);
                valueRt.anchoredPosition = Vector2.zero;
                Text[] digits = MakeDigits("Xy", valueBox.transform, body, UiDigits.ValueSize, 7, valueCell, UiTheme.Accent);
                for (int digit = 0; digit < 7; digit++)
                {
                    _hudValueDigits[(row * 7) + digit] = digits[digit];
                }

                _hudGhost[row] = CreateText("Ghost", rowRt, body, 16, TextAnchor.MiddleRight, FontStyle.Normal);
                _hudGhost[row].color = UiTheme.Primary;
                RectTransform ghostRt = _hudGhost[row].rectTransform;
                ghostRt.anchorMin = new Vector2(1f, 0.5f);
                ghostRt.anchorMax = new Vector2(1f, 0.5f);
                ghostRt.pivot = new Vector2(1f, 0.5f);
                ghostRt.sizeDelta = new Vector2(36f, 20f);
                ghostRt.anchoredPosition = new Vector2(-(112f + 8f), 0f);
                _hudGhost[row].gameObject.SetActive(false);
            }

            RectTransform lifeRow = _hangarHudRoot.transform.Find("HudRow0").GetComponent<RectTransform>();
            RectTransform hullRow = _hangarHudRoot.transform.Find("HudRow1").GetComponent<RectTransform>();
            RectTransform shieldRow = _hangarHudRoot.transform.Find("HudRow2").GetComponent<RectTransform>();
            _hudHullContent = MakeContent(hullRow);
            _hudShieldContent = MakeContent(shieldRow);
            GameObject lifeContent = new GameObject("Content", typeof(RectTransform));
            lifeContent.transform.SetParent(lifeRow, false);
            StretchContent(lifeContent.GetComponent<RectTransform>());
            _hudLifeIcons = new Image[HudLifeIcons];
            for (int icon = 0; icon < HudLifeIcons; icon++)
            {
                _hudLifeIcons[icon] = MakeIcon("Life" + icon, lifeContent.transform, "hud_ship", UiTheme.Accent);
                RectTransform iconRt = _hudLifeIcons[icon].rectTransform;
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.sizeDelta = new Vector2(22f, 22f);
                iconRt.anchoredPosition = new Vector2(icon * 28f, 0f);
            }

            _hudLifeGhost = MakeIcon("LifeGhost", lifeContent.transform, "hud_ship", UiTheme.WithAlpha(UiTheme.Primary, 0.60f));
            RectTransform ghostIconRt = _hudLifeGhost.rectTransform;
            ghostIconRt.anchorMin = new Vector2(0f, 0.5f);
            ghostIconRt.anchorMax = new Vector2(0f, 0.5f);
            ghostIconRt.pivot = new Vector2(0f, 0.5f);
            ghostIconRt.sizeDelta = new Vector2(22f, 22f);
            _hudLifeGhost.gameObject.SetActive(false);
            FontStyle lifeStyle;
            Font lifeFont = UiFonts.ForDigits(display, 22, out lifeStyle);
            _hudLifeTimes = CreateText("LifeTimes", lifeContent.transform, lifeFont, 22, TextAnchor.MiddleLeft, lifeStyle);
            _hudLifeTimes.color = UiTheme.Accent;
            RectTransform timesRt = _hudLifeTimes.rectTransform;
            timesRt.anchorMin = new Vector2(0f, 0.5f);
            timesRt.anchorMax = new Vector2(0f, 0.5f);
            timesRt.pivot = new Vector2(0f, 0.5f);
            timesRt.sizeDelta = new Vector2(64f, 28f);
            timesRt.anchoredPosition = new Vector2(28f, 0f);
            _hudLifeTimes.gameObject.SetActive(false);

            _hudHullSegs = MakeSegments(_hudHullContent, "HullSeg");
            _hudShieldSegs = MakeSegments(_hudShieldContent, "ShieldSeg");
            _hudHullFill = MakeFill(_hudHullContent, "HullFill");
            _hudShieldFill = MakeFill(_hudShieldContent, "ShieldFill");
            _hudHullDanger = MakeIcon("Danger", hullRow, "hud_warning", UiTheme.Danger);
            RectTransform dangerRt = _hudHullDanger.rectTransform;
            dangerRt.anchorMin = new Vector2(1f, 0.5f);
            dangerRt.anchorMax = new Vector2(1f, 0.5f);
            dangerRt.pivot = new Vector2(1f, 0.5f);
            dangerRt.sizeDelta = new Vector2(16f, 16f);
            dangerRt.anchoredPosition = new Vector2(-(112f + 8f), 0f);
            _hudHullDanger.gameObject.SetActive(false);
            _hangarHudRoot.SetActive(false);
        }

        private void BuildWaveStrip(Font display, Font body)
        {
            if (_summaryRoot == null)
            {
                return;
            }

            GameObject strip = new GameObject("WaveStatsStrip", typeof(RectTransform));
            strip.transform.SetParent(_summaryRoot.transform, false);
            Stretch(strip.GetComponent<RectTransform>(), new Vector2(0.03f, HangarHudLayout.StripMinY), new Vector2(HangarHudLayout.StripMaxX, HangarHudLayout.StripMaxY));
            _stripRoot = strip;
            _stripGroups = new CanvasGroup[4];
            _stripCaptions = new Text[4];
            _stripCaptionIcons = new Image[4];
            _stripDigits = new Text[3][];
            string[] captions = new string[]
            {
                Loc.T("summary.kills", "KILLS"),
                Loc.T("summary.credits", "CREDITS"),
                Loc.T("summary.time", "TIME"),
                Loc.T("summary.damage", "DAMAGE"),
            };
            string[] icons = new string[] { "hud_target", "hud_coin", "hud_hourglass", null };
            for (int cell = 0; cell < 4; cell++)
            {
                GameObject cellGo = new GameObject("StripCell" + cell, typeof(RectTransform));
                cellGo.transform.SetParent(strip.transform, false);
                Stretch(cellGo.GetComponent<RectTransform>(), new Vector2(cell * 0.25f, 0f), new Vector2((cell + 1) * 0.25f, 1f));
                CanvasGroup group = cellGo.AddComponent<CanvasGroup>();
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
                _stripGroups[cell] = group;
                if (cell >= 1)
                {
                    Image rule = MakeIcon("Divider", cellGo.transform, null, UiTheme.WithAlpha(UiTheme.Secondary, 0.35f));
                    Stretch(rule.rectTransform, new Vector2(0f, 0.08f), new Vector2(0f, 0.92f));
                    rule.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rule.rectTransform.sizeDelta = new Vector2(1f, 0f);
                }

                GameObject inner = new GameObject("Inner", typeof(RectTransform));
                inner.transform.SetParent(cellGo.transform, false);
                RectTransform innerRt = inner.GetComponent<RectTransform>();
                Stretch(innerRt, Vector2.zero, Vector2.one);
                innerRt.offsetMin = new Vector2(8f, 3f);
                innerRt.offsetMax = new Vector2(-8f, -3f);

                GameObject caption = new GameObject("Caption", typeof(RectTransform));
                caption.transform.SetParent(inner.transform, false);
                RectTransform captionRt = caption.GetComponent<RectTransform>();
                captionRt.anchorMin = new Vector2(0f, 1f);
                captionRt.anchorMax = new Vector2(1f, 1f);
                captionRt.pivot = new Vector2(0f, 1f);
                captionRt.sizeDelta = new Vector2(0f, 18f);
                captionRt.anchoredPosition = Vector2.zero;
                if (icons[cell] != null)
                {
                    _stripCaptionIcons[cell] = MakeIcon("Icon", caption.transform, icons[cell], UiTheme.Secondary);
                    RectTransform iconRt = _stripCaptionIcons[cell].rectTransform;
                    iconRt.anchorMin = new Vector2(0f, 0.5f);
                    iconRt.anchorMax = new Vector2(0f, 0.5f);
                    iconRt.pivot = new Vector2(0f, 0.5f);
                    iconRt.sizeDelta = new Vector2(16f, 16f);
                    iconRt.anchoredPosition = Vector2.zero;
                }

                _stripCaptions[cell] = CreateText("Label", caption.transform, body, 16, TextAnchor.MiddleLeft, FontStyle.Normal);
                _stripCaptions[cell].color = UiTheme.Secondary;
                _stripCaptions[cell].text = captions[cell];
                Stretch(_stripCaptions[cell].rectTransform, Vector2.zero, Vector2.one);
                _stripCaptions[cell].rectTransform.offsetMin = new Vector2(icons[cell] != null ? 22f : 0f, 0f);
                ClampOneLine(_stripCaptions[cell]);

                GameObject value = new GameObject("Value", typeof(RectTransform));
                value.transform.SetParent(inner.transform, false);
                RectTransform valueRt = value.GetComponent<RectTransform>();
                valueRt.anchorMin = new Vector2(0f, 0f);
                valueRt.anchorMax = new Vector2(1f, 0f);
                valueRt.pivot = new Vector2(0f, 0f);
                valueRt.sizeDelta = new Vector2(0f, 30f);
                valueRt.anchoredPosition = Vector2.zero;
                if (cell < 3)
                {
                    int cellPx = UiDigits.CellWidth(UiDigits.StripSize);
                    _stripDigits[cell] = MakeDigits("Digit", value.transform, display, UiDigits.StripSize, StripDigitCells, cellPx, cell == 1 ? UiTheme.Primary : UiTheme.Accent);
                    for (int digit = 0; digit < _stripDigits[cell].Length; digit++)
                    {
                        RectTransform digitRt = _stripDigits[cell][digit].rectTransform;
                        digitRt.anchorMin = new Vector2(0f, 0f);
                        digitRt.anchorMax = new Vector2(0f, 1f);
                        digitRt.pivot = new Vector2(0f, 0.5f);
                        digitRt.anchoredPosition = new Vector2((StripDigitCells - 1 - digit) * cellPx, 0f);
                    }
                }
                else
                {
                    BuildDamageValue(value.transform, display);
                }
            }

            _stripUnderline = MakeIcon("RecordLine", _stripRoot.transform.Find("StripCell2/Inner/Value"), null, UiTheme.Primary);
            RectTransform lineRt = _stripUnderline.rectTransform;
            lineRt.anchorMin = new Vector2(0f, 0f);
            lineRt.anchorMax = new Vector2(0f, 0f);
            lineRt.pivot = new Vector2(0f, 1f);
            lineRt.sizeDelta = new Vector2(48f, 2f);
            lineRt.anchoredPosition = new Vector2(0f, -1f);
            _stripUnderline.gameObject.SetActive(false);

            _stripBadge = new GameObject("RecordBadge", typeof(RectTransform));
            _stripBadge.transform.SetParent(_summaryRoot.transform, false);
            _stripBadgeGroup = _stripBadge.AddComponent<CanvasGroup>();
            _stripBadgeGroup.alpha = 0f;
            _stripBadgeGroup.blocksRaycasts = false;
            _stripBadgeGroup.interactable = false;
            Image badgePlate = _stripBadge.AddComponent<Image>();
            badgePlate.color = UiTheme.HeaderWash;
            badgePlate.raycastTarget = false;
            _stripStar = MakeIcon("Star", _stripBadge.transform, "hud_star", UiTheme.Primary);
            RectTransform starRt = _stripStar.rectTransform;
            starRt.anchorMin = new Vector2(0f, 0.5f);
            starRt.anchorMax = new Vector2(0f, 0.5f);
            starRt.pivot = new Vector2(0f, 0.5f);
            starRt.sizeDelta = new Vector2(16f, 16f);
            starRt.anchoredPosition = new Vector2(8f, 0f);
            Text badgeText = CreateText("Label", _stripBadge.transform, body, 16, TextAnchor.MiddleLeft, FontStyle.Normal);
            badgeText.color = UiTheme.Focus;
            badgeText.text = Loc.T("ui.new_best", "NEW BEST");
            RectTransform badgeTextRt = badgeText.rectTransform;
            badgeTextRt.anchorMin = new Vector2(0f, 0f);
            badgeTextRt.anchorMax = new Vector2(1f, 1f);
            badgeTextRt.offsetMin = new Vector2(30f, 0f);
            badgeTextRt.offsetMax = new Vector2(-8f, 0f);
            ClampOneLine(badgeText);
            _stripBadge.SetActive(false);
            _stripRoot.SetActive(false);
        }

        private void BuildDamageValue(Transform parent, Font display)
        {
            _stripHullIcon = MakeIcon("Hull", parent, "hud_wrench", UiTheme.Accent);
            PlaceDamageIcon(_stripHullIcon.rectTransform, 0f);
            int cell = UiDigits.CellWidth(UiDigits.StripSize);
            GameObject hullBox = new GameObject("HullDigits", typeof(RectTransform));
            hullBox.transform.SetParent(parent, false);
            RectTransform hullRt = hullBox.GetComponent<RectTransform>();
            hullRt.anchorMin = new Vector2(0f, 0f);
            hullRt.anchorMax = new Vector2(0f, 1f);
            hullRt.pivot = new Vector2(0f, 0.5f);
            hullRt.sizeDelta = new Vector2(cell * 3f, 0f);
            hullRt.anchoredPosition = new Vector2(26f, 0f);
            _stripHullDigits = MakeDigits("H", hullBox.transform, display, UiDigits.StripSize, 3, cell, UiTheme.Accent);
            LeftDigits(_stripHullDigits, cell);
            _stripShieldIcon = MakeIcon("Shield", parent, "hud_shield", UiTheme.Secondary);
            PlaceDamageIcon(_stripShieldIcon.rectTransform, 26f + (cell * 3f) + 14f);
            GameObject shieldBox = new GameObject("ShieldDigits", typeof(RectTransform));
            shieldBox.transform.SetParent(parent, false);
            RectTransform shieldRt = shieldBox.GetComponent<RectTransform>();
            shieldRt.anchorMin = new Vector2(0f, 0f);
            shieldRt.anchorMax = new Vector2(0f, 1f);
            shieldRt.pivot = new Vector2(0f, 0.5f);
            shieldRt.sizeDelta = new Vector2(cell * 3f, 0f);
            shieldRt.anchoredPosition = new Vector2(26f + (cell * 3f) + 14f + 26f, 0f);
            _stripShieldDigits = MakeDigits("S", shieldBox.transform, display, UiDigits.StripSize, 3, cell, UiTheme.Accent);
            LeftDigits(_stripShieldDigits, cell);
        }

        private void FinishWaveStrip()
        {
            bool wave = _session != null && _session.Phase == GamePhase.WaveClear;
            if (_summaryBody != null)
            {
                _summaryBody.gameObject.SetActive(!wave);
            }

            if (_continueHint != null && wave)
            {
                _continueHint.gameObject.SetActive(false);
            }

            if (_stripRoot != null)
            {
                _stripRoot.SetActive(wave);
            }

            if (!wave)
            {
                _stripStart = -1f;
                _stripHold = false;
                _stripSnap = false;
                _recordPlayed = false;
                if (_stripBadge != null)
                {
                    _stripBadge.SetActive(false);
                }

                return;
            }

            LoadoutState loadout = _loadout != null ? _loadout.State : null;
            if (RunSummary.ShowContinueHint(_session.LastResolvedWave, _session.Phase))
            {
                string continueLine = RunSummary.ContinueHint(_session.LastResolvedWave, _session.Credits, loadout);
                _statusBase = string.IsNullOrEmpty(continueLine) ? string.Empty : continueLine.Replace("\n", "  ·  ");
            }

            bool medal = RunSummary.ShowWaveMedal(_session.LastResolvedWave, _session.Phase);
            bool timeBest = _poseFreeze ? _poseTimeBest : (_game != null && _game.WaveTimeBest);
            bool comboBest = _poseFreeze ? _poseComboBest : (_game != null && _game.WaveComboBest);
            bool badge = _poseFreeze ? _poseBest : (timeBest || comboBest);
            if (_summaryTitle != null && medal && badge)
            {
                Stretch(_summaryTitle.rectTransform, new Vector2(0.03f, SummaryTitleMinY), new Vector2(0.64f, SummaryTitleMaxY));
            }

            PaintWaveStrip(timeBest, badge, medal);
        }

        private void PaintWaveStrip(bool timeBest, bool badge, bool medal)
        {
            if (_stripRoot == null || _session == null || _session.Wave == null)
            {
                return;
            }

            WaveStats stats = _session.Wave;
            bool daily = _game != null && _game.ActiveDailyDate != 0;
            int mutator = _game != null ? MutatorRules.CreditPercent(_game.ActiveMutatorMask) : 100;
            string suffix = WaveStats.CreditSuffix(daily, 100, mutator);
            bool finals = _poseFreeze ? _poseElapsed >= 1f : (BoonModalOpen() || _stripSnap);
            float content = 144f;
            if (_stripDigits != null && _stripDigits.Length > 0 && _stripDigits[0] != null && _stripDigits[0].Length > 0 && _stripDigits[0][0] != null)
            {
                RectTransform box = _stripDigits[0][0].rectTransform.parent as RectTransform;
                if (box != null && box.rect.width >= 8f)
                {
                    content = box.rect.width;
                }
            }
            float elapsed = StripElapsed(finals);
            if (BoonModalOpen() && !_poseFreeze)
            {
                finals = true;
                elapsed = 10f;
            }

            int kills = finals || elapsed >= CountEnd(stats.Kills, stats.Credits) ? stats.Kills : ShownCount(stats.Kills, 0, elapsed);
            int credits = finals || elapsed >= CountEnd(stats.Kills, stats.Credits) ? stats.Credits : ShownCount(stats.Credits, 1, elapsed);
            string[] values = new string[]
            {
                kills.ToString(),
                WaveStats.SignedCredits(credits),
                WaveStats.FormatTime(stats.Seconds)
            };
            for (int cell = 0; cell < 3; cell++)
            {
                int size = UiDigits.StripSizeFor(values[cell].Length, content);
                int cellPx = UiDigits.CellWidth(size);
                PaintLeftDigits(_stripDigits[cell], values[cell], cellPx, size);
            }

            PaintLeftDigits(_stripHullDigits, WaveStats.ClampDamage(stats.HullDamage).ToString(), UiDigits.CellWidth(UiDigits.StripSize), UiDigits.StripSize);
            PaintLeftDigits(_stripShieldDigits, WaveStats.ClampDamage(stats.ShieldDamage).ToString(), UiDigits.CellWidth(UiDigits.StripSize), UiDigits.StripSize);
            if (_stripCaptions[1] != null)
            {
                _stripCaptions[1].text = Loc.T("summary.credits", "CREDITS") + suffix;
            }

            if (_stripCaptions[2] != null)
            {
                _stripCaptions[2].color = timeBest ? UiTheme.Primary : UiTheme.Secondary;
            }

            float badgeStart = CountEnd(stats.Kills, stats.Credits);
            float badgeAlpha = !badge ? 0f : (finals || elapsed >= badgeStart + UiTheme.MotionFast ? 1f : Fade((elapsed - badgeStart) / UiTheme.MotionFast));
            for (int cell = 0; cell < 4; cell++)
            {
                if (_stripGroups[cell] != null)
                {
                    _stripGroups[cell].alpha = finals ? 1f : Fade((elapsed - (cell * UiTransition.CardStaggerSeconds)) / UiTheme.MotionFast);
                }
            }

            if (_stripUnderline != null)
            {
                bool showLine = timeBest && badgeAlpha > 0.01f;
                _stripUnderline.gameObject.SetActive(showLine);
                if (showLine)
                {
                    int width = values[2].Length * UiDigits.CellWidth(UiDigits.StripSize);
                    _stripUnderline.rectTransform.sizeDelta = new Vector2(width, 2f);
                }
            }

            PlaceBadge(badge, medal, badgeAlpha);
            if (badge && badgeAlpha > 0.01f && !_recordPlayed && !_poseFreeze && AudioCues.Instance != null)
            {
                float earliest = AudioCues.Instance.StingerAt + 0.5f;
                if (AudioCues.Instance.StingerAt <= 0f || Time.unscaledTime >= earliest)
                {
                    AudioCues.Instance.PlayRecord();
                    _recordPlayed = true;
                }
            }
        }

        private void PlaceBadge(bool badge, bool medal, float alpha)
        {
            if (_stripBadge == null)
            {
                return;
            }

            _stripBadge.SetActive(badge);
            if (!badge)
            {
                return;
            }

            _stripBadgeGroup.alpha = alpha;
            RectTransform rt = _stripBadge.GetComponent<RectTransform>();
            Text label = _stripBadge.GetComponentInChildren<Text>();
            if (medal)
            {
                rt.anchorMin = new Vector2(0.655f, 0.76f);
                rt.anchorMax = new Vector2(0.655f, 0.76f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(20f, 20f);
                rt.anchoredPosition = Vector2.zero;
                Image plate = _stripBadge.GetComponent<Image>();
                if (plate != null)
                {
                    plate.enabled = false;
                }

                if (label != null)
                {
                    label.gameObject.SetActive(false);
                }

                if (_stripStar != null)
                {
                    _stripStar.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    _stripStar.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    _stripStar.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    _stripStar.rectTransform.sizeDelta = new Vector2(20f, 20f);
                    _stripStar.rectTransform.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                rt.anchorMin = new Vector2(0.97f, 0.76f);
                rt.anchorMax = new Vector2(0.97f, 0.76f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.sizeDelta = new Vector2(151f, 24f);
                rt.anchoredPosition = Vector2.zero;
                Image plate = _stripBadge.GetComponent<Image>();
                if (plate != null)
                {
                    plate.enabled = true;
                    plate.color = UiTheme.HeaderWash;
                }

                if (label != null)
                {
                    label.gameObject.SetActive(true);
                }

                if (_stripStar != null)
                {
                    _stripStar.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    _stripStar.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                    _stripStar.rectTransform.pivot = new Vector2(0f, 0.5f);
                    _stripStar.rectTransform.sizeDelta = new Vector2(16f, 16f);
                    _stripStar.rectTransform.anchoredPosition = new Vector2(8f, 0f);
                }
            }
        }

        private void RefreshHangarHud()
        {
            if (_hangarHudRoot == null)
            {
                return;
            }

            bool show = _session != null && HangarHudLayout.Shows(_session.Phase) && !_creditsVisible && !_settingsOpen;
            _hangarHudRoot.SetActive(show);
            if (_healthRoot != null && show)
            {
                _healthRoot.SetActive(false);
            }

            if (!show)
            {
                _hudCreditsReady = false;
                return;
            }

            int credits = _session.Credits;
            if (!_hudCreditsReady)
            {
                _hudCredits.Snap(credits);
                _hudCreditsReady = true;
                _hudCreditTarget = credits;
            }
            else if (credits != _hudCreditTarget)
            {
                float shown = _hudCredits.Display();
                _hudCredits.From = shown;
                _hudCredits.To = credits;
                _hudCredits.Elapsed = 0f;
                _hudCredits.Seconds = UiCountUp.Duration(credits - shown);
                _hudCreditTarget = credits;
            }

            int shownCredits = WaveStats.VisualCredits(_hudCredits.Rounded());
            bool deny = Time.unscaledTime < _denyUntil;
            PaintDigits(_hudCreditDigits, shownCredits.ToString(), UiDigits.CellWidth(UiDigits.CreditSize), true);
            Color creditColor = deny ? UiTheme.ShopPoorText : UiTheme.Primary;
            TintDigits(_hudCreditDigits, creditColor);
            if (_hudCreditsCaption != null)
            {
                _hudCreditsCaption.text = Loc.T("hud.credits", "CREDITS");
            }

            if (_hudLifeCaptions != null)
            {
                _hudLifeCaptions[0].text = Loc.T("ui.lives", "LIVES");
                _hudLifeCaptions[1].text = Loc.T("ui.hull_label", "HULL");
                _hudLifeCaptions[2].text = Loc.T("ui.shield_label", "SHIELD");
            }

            int hull = _ship != null && _ship.Health != null ? _ship.Health.Hull : LoadoutState.HullHitPoints;
            int maxHull = _ship != null && _ship.Health != null ? _ship.Health.MaxHull : LoadoutState.HullHitPoints;
            int shield = _ship != null && _ship.Health != null ? _ship.Health.Shield : 0;
            int maxShield = _ship != null && _ship.Health != null ? _ship.Health.MaxShield : LoadoutState.MaxShieldCharges;
            if (maxHull < 1)
            {
                maxHull = 1;
            }

            int lives = _session.Lives;
            int maxLives = _session.MaxLives;
            HangarFocus focus = ReadHangarFocus(hull, maxHull, shield, maxShield, credits);
            PaintRatio(0, lives, maxLives);
            PaintRatio(1, hull, maxHull);
            PaintRatio(2, shield, maxShield);
            PaintLives(lives, maxLives, focus.LifeGhost);
            bool danger = WaveStats.HullDanger(hull, maxHull);
            bool hullGhostWins = focus.HullGhost > 0;
            PaintBar(_hudHullSegs, _hudHullFill, _hudHullContent, hull, maxHull, focus.HullGhost, danger ? UiTheme.Danger : UiTheme.Accent);
            PaintBar(_hudShieldSegs, _hudShieldFill, _hudShieldContent, shield, maxShield, focus.ShieldGhost, UiTheme.Secondary);
            if (_hudHullDanger != null)
            {
                _hudHullDanger.gameObject.SetActive(danger && !hullGhostWins);
            }

            PaintGhost(0, focus.LifeGhost ? 1 : 0);
            PaintGhost(1, hullGhostWins ? focus.HullGhost : 0);
            PaintGhost(2, focus.ShieldGhost);
            if (_hudCreditsDelta != null)
            {
                bool showCost = focus.HasCost;
                _hudCreditsDelta.gameObject.SetActive(showCost);
                if (showCost)
                {
                    _hudCreditsDelta.text = "-" + focus.Cost.ToString();
                    _hudCreditsDelta.color = focus.CanAfford ? UiTheme.Accent : UiTheme.ShopPoorText;
                }
            }
        }

        private void TickHangarStatus()
        {
            if (_session != null && _session.Phase == GamePhase.WaveClear && !_poseFreeze)
            {
                if (BoonModalOpen())
                {
                    _stripHold = true;
                }
                else if (_stripHold || _stripStart < 0f)
                {
                    _stripStart = Time.unscaledTime;
                    _stripHold = false;
                    _stripSnap = false;
                }

                if (!_stripSnap && StripBusy())
                {
                    bool confirm = GamepadInput.ConfirmPressed();
                    Vector2 nav = GamepadInput.UiNavCombined();
                    bool moved = HangarPadNav.DominantStep(nav.x, nav.y, HangarPadNav.Flick) != 0;
                    if (confirm || moved || _hoveredItem != null)
                    {
                        _stripSnap = true;
                    }
                }
            }

            if (_hudCreditsReady && _hangarHudRoot != null && _hangarHudRoot.activeSelf && !HangarCreditsHeld())
            {
                _hudCredits.Advance(Time.unscaledDeltaTime, false);
            }

            if (_session != null && _session.Phase == GamePhase.WaveClear && _stripRoot != null && _stripRoot.activeSelf)
            {
                bool medal = RunSummary.ShowWaveMedal(_session.LastResolvedWave, _session.Phase);
                bool timeBest = _poseFreeze ? _poseTimeBest : (_game != null && _game.WaveTimeBest);
                bool comboBest = _poseFreeze ? _poseComboBest : (_game != null && _game.WaveComboBest);
                bool badge = _poseFreeze ? _poseBest : (timeBest || comboBest);
                PaintWaveStrip(timeBest, badge, medal);
            }
        }

        public void CaptureHangarPose(string pose)
        {
            if (_session == null || pose == null)
            {
                return;
            }

            if (pose != HangarHudLayout.PoseDailySv)
            {
                Loc.SetLanguage(GameLanguage.English);
            }

            _poseFreeze = true;
            _poseElapsed = pose == HangarHudLayout.PoseClearMid ? 0.30f : 1.2f;
            _poseBest = false;
            _poseTimeBest = false;
            _poseComboBest = false;
            _hoveredItem = null;
            _tutorialDismissed = true;
            _doctrineIntroDismissed = true;
            if (_tutorialRoot != null)
            {
                _tutorialRoot.SetActive(false);
            }

            if (_game != null && _game.ShowDifficultyChooser)
            {
                _game.SkipFirstStart(DifficultyGrade.Normal);
            }

            if (_game != null)
            {
                _game.PresentDailyForCapture(0, 0);
            }

            int credits = 1250;
            int lives = DifficultySettings.MaxLives;
            int maxLives = DifficultySettings.MaxLives;
            int wave = 3;
            int resolved = 2;
            GamePhase phase = GamePhase.Hangar;
            if (pose == HangarHudLayout.PoseClearDone || pose == HangarHudLayout.PoseClearMid)
            {
                phase = GamePhase.WaveClear;
                resolved = 7;
                wave = 8;
                _session.Wave.Present(42, 1250, 102f, 2, 1, 4);
            }
            else if (pose == HangarHudLayout.PoseRecordMedal)
            {
                phase = GamePhase.WaveClear;
                resolved = 3;
                wave = 4;
                _poseBest = true;
                _poseTimeBest = true;
                _session.Wave.Present(18, 400, 54f, 1, 0, 3);
            }
            else if (pose == HangarHudLayout.PoseRecordPill)
            {
                phase = GamePhase.WaveClear;
                resolved = 1;
                wave = 2;
                _poseBest = true;
                _poseTimeBest = true;
                _session.Wave.Present(11, 200, 48f, 0, 1, 2);
            }
            else if (pose == HangarHudLayout.PoseDailySv)
            {
                phase = GamePhase.WaveClear;
                resolved = 4;
                wave = 5;
                Loc.SetLanguage(GameLanguage.Swedish);
                if (_game != null)
                {
                    _game.PresentDailyForCapture(20261017, 17);
                }

                _session.Wave.Present(30, 900, 80f, 1, 2, 5);
            }
            else if (pose == HangarHudLayout.PoseLives7)
            {
                credits = 99999;
                lives = 7;
                maxLives = 7;
            }
            else if (pose == HangarHudLayout.PoseMax)
            {
                credits = 99999;
            }
            else if (pose == HangarHudLayout.PoseDanger)
            {
                credits = 400;
            }
            else if (pose == HangarHudLayout.PoseRepair)
            {
                credits = 500;
            }
            else if (pose == HangarHudLayout.PosePoor)
            {
                credits = 10;
            }
            else if (pose == HangarHudLayout.PoseFailed)
            {
                phase = GamePhase.Failed;
                resolved = 4;
                credits = 80;
            }

            _session.PresentForCapture(phase, wave, resolved, credits, lives, maxLives);
            if (_ship != null && _ship.Health != null)
            {
                if (pose == HangarHudLayout.PoseDanger)
                {
                    _ship.Health.SetHull(1);
                    _ship.Health.SetShield(0);
                }
                else if (pose == HangarHudLayout.PoseRepair)
                {
                    _ship.Health.SetHull(2);
                    _ship.Health.SetShield(1);
                }
                else if (pose == HangarHudLayout.PoseFailed)
                {
                    _ship.Health.SetHull(1);
                    _ship.Health.SetShield(1);
                }
                else
                {
                    _ship.Health.SetHull(_ship.Health.MaxHull);
                    _ship.Health.SetShield(_ship.Health.MaxShield);
                }
            }

            if (pose == HangarHudLayout.PoseRepair)
            {
                _padSlot = HangarPadNav.HullRepairSlot;
            }
            else if (pose == HangarHudLayout.PosePoor)
            {
                ShopItem poor = FirstUnaffordable(credits);
                _hoveredItem = poor;
                _padSlot = poor != null ? HangarPadNav.ShopSlot(ShopIndex(poor)) : HangarPadNav.PrimarySlot;
            }
            else
            {
                _padSlot = HangarPadNav.PrimarySlot;
            }

            _hudCreditsReady = false;
            Refresh();
            if (_hudCreditsReady)
            {
                _hudCredits.Snap(_session.Credits);
            }
        }

        private void NoteShopDeny(UpgradeId id)
        {
            if (_session == null || _loadout == null || _loadout.State == null)
            {
                return;
            }

            ShopItem item = HangarShop.FindItem(id);
            if (item == null)
            {
                return;
            }

            int world = WorldCatalog.NumberForWave(_session.WaveIndex);
            bool mk2 = _loadout.State.Owns(id) && _loadout.State.CanBuyMk2(id);
            int price = mk2 ? _loadout.State.Mk2Price(item, world) : _loadout.State.EffectiveCost(item, world);
            if (_session.Credits < price)
            {
                _denyUntil = Time.unscaledTime + 0.30f;
            }
        }

        private void NoteServiceDeny(int baseCost)
        {
            if (_session == null)
            {
                return;
            }

            int world = WorldCatalog.NumberForWave(_session.WaveIndex);
            int price = ShopPrices.ApplyWorld(baseCost, world);
            if (_session.Credits < price)
            {
                _denyUntil = Time.unscaledTime + 0.30f;
            }
        }

        private HangarFocus ReadHangarFocus(int hull, int maxHull, int shield, int maxShield, int credits)
        {
            HangarFocus focus = new HangarFocus();
            int world = _session != null ? WorldCatalog.NumberForWave(_session.WaveIndex) : 1;
            if (_padSlot == HangarPadNav.HullRepairSlot)
            {
                focus.HasCost = true;
                focus.Cost = ShopPrices.ApplyWorld(ShopPrices.HullRepairCost, world);
                focus.CanAfford = credits >= focus.Cost && hull > 0 && hull < maxHull;
                if (focus.CanAfford)
                {
                    focus.HullGhost = maxHull - hull;
                }
            }
            else if (_padSlot == HangarPadNav.ShieldRefillSlot)
            {
                focus.HasCost = true;
                focus.Cost = ShopPrices.ApplyWorld(ShopPrices.ShieldRefillCost, world);
                focus.CanAfford = credits >= focus.Cost && maxShield > 0 && shield < maxShield;
                if (focus.CanAfford)
                {
                    focus.ShieldGhost = maxShield - shield;
                }
            }
            else if (_padSlot == HangarPadNav.ExtraLifeSlot)
            {
                focus.HasCost = true;
                focus.Cost = ShopPrices.ApplyWorld(ShopPrices.ExtraLifeCost, world);
                int cap = DifficultySettings.MaxLives + 1;
                if (_session != null && _session.MaxLives > cap)
                {
                    cap = _session.MaxLives;
                }

                focus.CanAfford = credits >= focus.Cost && _session != null && _session.Lives < cap;
                focus.LifeGhost = focus.CanAfford;
            }
            else if (_hoveredItem != null && _loadout != null && _loadout.State != null)
            {
                focus.HasCost = true;
                bool mk2 = _loadout.State.Owns(_hoveredItem.Id) && _loadout.State.CanBuyMk2(_hoveredItem.Id);
                focus.Cost = mk2 ? _loadout.State.Mk2Price(_hoveredItem, world) : _loadout.State.EffectiveCost(_hoveredItem, world);
                focus.CanAfford = credits >= focus.Cost && (_loadout.State.CanApply(_hoveredItem.Id) || mk2);
            }

            return focus;
        }

        private ShopItem FirstUnaffordable(int credits)
        {
            if (ShopCatalog.Items == null)
            {
                return null;
            }

            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                if (item != null && item.Cost > credits)
                {
                    return item;
                }
            }

            return ShopCatalog.Items.Length > 0 ? ShopCatalog.Items[0] : null;
        }

        private static int ShopIndex(ShopItem item)
        {
            if (item == null || ShopCatalog.Items == null)
            {
                return 0;
            }

            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                if (ShopCatalog.Items[i].Id == item.Id)
                {
                    return i;
                }
            }

            return 0;
        }

        private float StripElapsed(bool finals)
        {
            if (_poseFreeze)
            {
                return _poseElapsed;
            }

            if (finals && _stripSnap)
            {
                return 10f;
            }

            if (_stripStart < 0f)
            {
                return 0f;
            }

            return Time.unscaledTime - _stripStart;
        }

        private bool StripBusy()
        {
            if (_session == null || _session.Wave == null)
            {
                return false;
            }

            float elapsed = _stripStart < 0f ? 0f : Time.unscaledTime - _stripStart;
            return elapsed < CountEnd(_session.Wave.Kills, _session.Wave.Credits) + UiTheme.MotionFast;
        }

        private static float CountEnd(int kills, int credits)
        {
            float killsEnd = UiCountUp.Duration(kills);
            float creditEnd = UiTransition.CardStaggerSeconds + UiCountUp.Duration(credits);
            return killsEnd > creditEnd ? killsEnd : creditEnd;
        }

        private static int ShownCount(int target, int cell, float elapsed)
        {
            float start = cell * UiTransition.CardStaggerSeconds;
            float span = UiCountUp.Duration(target);
            float u = span <= 0.0001f ? 1f : (elapsed - start) / span;
            if (u <= 0f)
            {
                return 0;
            }

            if (u >= 1f)
            {
                return target;
            }

            return (int)System.Math.Round(target * (double)UiTheme.EaseOut(u));
        }

        private static float Fade(float u)
        {
            if (u <= 0f)
            {
                return 0f;
            }

            if (u >= 1f)
            {
                return 1f;
            }

            return UiTheme.EaseOut(u);
        }

        private void PaintLives(int lives, int maxLives, bool ghost)
        {
            if (maxLives < 1)
            {
                maxLives = 1;
            }

            if (lives < 0)
            {
                lives = 0;
            }

            if (lives > maxLives)
            {
                lives = maxLives;
            }

            bool icons = maxLives <= HudLifeIcons;
            for (int i = 0; i < HudLifeIcons; i++)
            {
                if (_hudLifeIcons[i] == null)
                {
                    continue;
                }

                bool on = icons && i < maxLives;
                _hudLifeIcons[i].gameObject.SetActive(on);
                if (on)
                {
                    _hudLifeIcons[i].color = i < lives ? UiTheme.Accent : UiTheme.WithAlpha(UiTheme.Accent, 0.45f);
                }
            }

            if (_hudLifeTimes != null)
            {
                _hudLifeTimes.gameObject.SetActive(!icons);
                if (!icons)
                {
                    _hudLifeTimes.text = "\u00d7" + lives.ToString();
                }
            }

            if (_hudLifeGhost != null)
            {
                int slot = icons ? maxLives : -1;
                bool show = ghost && icons && slot >= 0 && slot <= HudLifeIcons;
                _hudLifeGhost.gameObject.SetActive(show);
                if (show)
                {
                    float x = slot < HudLifeIcons ? slot * 28f : HudLifeIcons * 28f;
                    _hudLifeGhost.rectTransform.anchoredPosition = new Vector2(x, 0f);
                    _hudLifeGhost.color = UiTheme.WithAlpha(UiTheme.Primary, 0.60f);
                }
            }
        }

        private void PaintRatio(int row, int current, int max)
        {
            if (current < 0)
            {
                current = 0;
            }

            if (max < 0)
            {
                max = 0;
            }

            string text = current.ToString() + " / " + max.ToString();
            Text[] digits = new Text[7];
            for (int i = 0; i < 7; i++)
            {
                digits[i] = _hudValueDigits[(row * 7) + i];
            }

            PaintDigits(digits, text, UiDigits.GlyphWidth(UiDigits.ValueSize), true);
            TintDigits(digits, UiTheme.Accent);
        }

        private void PaintGhost(int row, int amount)
        {
            if (_hudGhost == null || _hudGhost[row] == null)
            {
                return;
            }

            bool show = amount > 0;
            _hudGhost[row].gameObject.SetActive(show);
            if (show)
            {
                _hudGhost[row].text = "+" + amount.ToString();
                _hudGhost[row].color = UiTheme.Primary;
            }
        }

        private void PaintBar(Image[] segs, Image fill, RectTransform content, int value, int max, int ghost, Color color)
        {
            if (segs == null || content == null)
            {
                return;
            }

            if (max < 1)
            {
                max = 1;
            }

            if (value < 0)
            {
                value = 0;
            }

            if (value > max)
            {
                value = max;
            }

            if (ghost < 0)
            {
                ghost = 0;
            }

            float width = content.rect.width;
            if (width < 8f)
            {
                width = 238f;
            }

            bool continuous = max > BarSegments;
            if (fill != null)
            {
                fill.gameObject.SetActive(continuous);
                if (continuous)
                {
                    fill.color = color;
                    fill.fillAmount = value / (float)max;
                }
            }

            for (int i = 0; i < segs.Length; i++)
            {
                if (segs[i] == null)
                {
                    continue;
                }

                bool on = !continuous && i < max;
                segs[i].gameObject.SetActive(on);
                if (!on)
                {
                    continue;
                }

                float seg = (width - (2f * (max - 1))) / max;
                if (seg < 1f)
                {
                    seg = 1f;
                }

                RectTransform rt = segs[i].rectTransform;
                rt.sizeDelta = new Vector2(seg, 12f);
                rt.anchoredPosition = new Vector2(i * (seg + 2f), 0f);
                if (i < value)
                {
                    segs[i].color = color;
                }
                else if (i < value + ghost)
                {
                    segs[i].color = UiTheme.WithAlpha(UiTheme.Primary, 0.60f);
                }
                else
                {
                    segs[i].color = UiTheme.WithAlpha(UiTheme.Void, 0.96f);
                }
            }

            PlaceTicks(content, continuous, width);
        }

        private static void PlaceTicks(RectTransform content, bool continuous, float width)
        {
            if (content == null)
            {
                return;
            }

            for (int tick = 0; tick < 4; tick++)
            {
                Transform mark = content.Find("Tick" + tick);
                if (mark == null)
                {
                    continue;
                }

                mark.gameObject.SetActive(continuous);
                if (!continuous)
                {
                    continue;
                }

                RectTransform rt = mark as RectTransform;
                if (rt == null)
                {
                    continue;
                }

                rt.anchoredPosition = new Vector2(width * (tick + 1) * 0.2f, 0f);
                rt.sizeDelta = new Vector2(1f, 12f);
            }
        }

        private static void PaintDigits(Text[] cells, string text, int cellPx, bool fromRight)
        {
            if (cells == null)
            {
                return;
            }

            int length = string.IsNullOrEmpty(text) ? 0 : text.Length;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                {
                    continue;
                }

                int index = fromRight ? length - 1 - i : i;
                bool on = index >= 0 && index < length;
                cells[i].gameObject.SetActive(on);
                if (!on)
                {
                    continue;
                }

                cells[i].text = text.Substring(index, 1);
                cells[i].rectTransform.sizeDelta = new Vector2(cellPx, cells[i].rectTransform.sizeDelta.y);
            }
        }

        private static void PaintLeftDigits(Text[] cells, string text, int cellPx, int size)
        {
            if (cells == null)
            {
                return;
            }

            int length = string.IsNullOrEmpty(text) ? 0 : text.Length;
            int used = length < cells.Length ? length : cells.Length;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                {
                    continue;
                }

                cells[i].fontSize = size;
                bool on = i < used;
                cells[i].gameObject.SetActive(on);
                if (!on)
                {
                    continue;
                }

                int fromRight = used - 1 - i;
                int index = length - 1 - fromRight;
                cells[i].text = text.Substring(index, 1);
                cells[i].rectTransform.sizeDelta = new Vector2(cellPx, 0f);
                cells[i].rectTransform.anchoredPosition = new Vector2((used - 1 - fromRight) * cellPx, 0f);
            }
        }

        private static void TintDigits(Text[] cells, Color color)
        {
            if (cells == null)
            {
                return;
            }

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                {
                    cells[i].color = color;
                }
            }
        }

        private static void LeftDigits(Text[] cells, int cellPx)
        {
            if (cells == null)
            {
                return;
            }

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                {
                    continue;
                }

                RectTransform rt = cells[i].rectTransform;
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(cellPx, 0f);
                rt.anchoredPosition = new Vector2((cells.Length - 1 - i) * cellPx, 0f);
            }
        }

        private Text[] MakeDigits(string name, Transform parent, Font font, int size, int count, int cell, Color color)
        {
            Text[] cells = new Text[count];
            for (int i = 0; i < count; i++)
            {
                FontStyle digitStyle;
                Font digitFont = UiFonts.ForDigits(font, size, out digitStyle);
                Text text = CreateText(name + i, parent, digitFont, size, TextAnchor.MiddleCenter, digitStyle);
                text.color = color;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                RectTransform rt = text.rectTransform;
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.sizeDelta = new Vector2(cell, UiDigits.GlyphHeight(size));
                rt.anchoredPosition = new Vector2(-i * cell, 0f);
                cells[i] = text;
            }

            return cells;
        }

        private RectTransform MakeRow(int row)
        {
            GameObject go = new GameObject("HudRow" + row, typeof(RectTransform));
            go.transform.SetParent(_hangarHudRoot.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            float top = 14f + (row * 34f);
            rt.offsetMin = new Vector2(308f, -(top + 30f));
            rt.offsetMax = new Vector2(-16f, -top);
            return rt;
        }

        private static RectTransform MakeContent(RectTransform row)
        {
            GameObject go = new GameObject("Content", typeof(RectTransform));
            go.transform.SetParent(row, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            StretchContent(rt);
            Image track = go.AddComponent<Image>();
            track.color = UiTheme.WithAlpha(UiTheme.Void, 0.96f);
            track.raycastTarget = false;
            Outline bezel = go.AddComponent<Outline>();
            bezel.effectColor = UiTheme.WithAlpha(UiTheme.Accent, 0.35f);
            bezel.effectDistance = new Vector2(1f, -1f);
            return rt;
        }

        private static void StretchContent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(88f, 9f);
            rt.offsetMax = new Vector2(-164f, -9f);
        }

        private Image[] MakeSegments(RectTransform content, string name)
        {
            Image[] segs = new Image[BarSegments];
            for (int i = 0; i < BarSegments; i++)
            {
                segs[i] = MakeIcon(name + i, content, null, UiTheme.Accent);
                segs[i].sprite = BarFillSprite();
                RectTransform rt = segs[i].rectTransform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(8f, 12f);
            }

            return segs;
        }

        private Image MakeFill(RectTransform content, string name)
        {
            Image fill = MakeIcon(name, content, null, UiTheme.Accent);
            fill.sprite = BarFillSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            Stretch(fill.rectTransform, Vector2.zero, Vector2.one);
            fill.gameObject.SetActive(false);
            for (int tick = 0; tick < 4; tick++)
            {
                Image mark = MakeIcon("Tick" + tick, content, null, UiTheme.WithAlpha(UiTheme.Accent, 0.55f));
                mark.sprite = BarFillSprite();
                RectTransform tickRt = mark.rectTransform;
                tickRt.anchorMin = new Vector2(0f, 0.5f);
                tickRt.anchorMax = new Vector2(0f, 0.5f);
                tickRt.pivot = new Vector2(0.5f, 0.5f);
                tickRt.sizeDelta = new Vector2(1f, 12f);
                mark.gameObject.SetActive(false);
            }

            return fill;
        }

        private static Image MakeIcon(string name, Transform parent, string spriteName, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = true;
            if (!string.IsNullOrEmpty(spriteName))
            {
                image.sprite = Resources.Load<Sprite>("UI/Icons/" + spriteName);
            }
            else
            {
                image.sprite = BarFillSprite();
            }

            return image;
        }

        private static void PinTopLeft(RectTransform rt, float x, float yDown, float width, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(x, -yDown);
        }

        private static void StretchSide(RectTransform rt, float x, float width, bool left)
        {
            if (left)
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(width, 0f);
                rt.anchoredPosition = new Vector2(x, 0f);
            }
        }

        private static void PlaceDamageIcon(RectTransform rt, float x)
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(20f, 20f);
            rt.anchoredPosition = new Vector2(x, 0f);
        }

        private struct HangarFocus
        {
            public bool HasCost;
            public int Cost;
            public bool CanAfford;
            public int HullGhost;
            public int ShieldGhost;
            public bool LifeGhost;
        }
    }
}
