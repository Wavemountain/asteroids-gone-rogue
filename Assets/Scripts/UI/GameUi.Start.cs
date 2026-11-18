using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    public sealed partial class GameUi
    {
        private bool _startOpen;
        private bool _startResolved;
        private bool _startDecided;
        private bool _startConfirmOpen;
        private bool _startNavHeld;
        private float _startNavRepeatAt;
        private int _startFocus = StartChoice.FocusNew;
        private int _startConfirmFocus = StartChoice.FocusNo;
        private StartChoiceView _startView;
        private GameObject _startRoot;
        private Text _startTitle;
        private Text _startQuestion;
        private Button _startContinue;
        private Button _startNew;
        private Button _startYes;
        private Button _startNo;
        private Text _startContinueLabel;
        private Text _startNewLabel;
        private Text _startYesLabel;
        private Text _startNoLabel;

        private void BuildStartPanel(Font display, Font body)
        {
            GameObject canvasGo = new GameObject("StartChoiceCanvas");
            UseUiLayer(canvasGo);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = CanvasOrder.Overlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            _startRoot = canvasGo;

            GameObject scrim = CreateFill(
                "StartChoiceScrim",
                canvasGo.transform,
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
            scrimButton.onClick.AddListener(OnStartScrim);
            LockButtonNavigation(scrimButton);

            GameObject plate = CreateFill(
                "StartChoicePlate",
                canvasGo.transform,
                UiTheme.PanelPlate,
                new Vector2(StartChoiceLayout.PlateMinX, StartChoiceLayout.PlateMinY),
                new Vector2(StartChoiceLayout.PlateMaxX, StartChoiceLayout.PlateMaxY));
            Image plateImage = plate.GetComponent<Image>();
            if (plateImage != null)
            {
                plateImage.raycastTarget = true;
            }

            CreateFill(
                "StartChoiceHeader",
                plate.transform,
                UiTheme.HeaderWash,
                new Vector2(0f, 0.88f),
                Vector2.one);
            CreateFill(
                "StartChoiceRule",
                plate.transform,
                UiTheme.HeaderRule,
                new Vector2(0.04f, 0.868f),
                new Vector2(0.96f, 0.88f));

            _startTitle = CreateText(
                "StartTitle",
                canvasGo.transform,
                display,
                UiTheme.TypeTitle,
                TextAnchor.MiddleCenter,
                FontStyle.Bold);
            Stretch(
                _startTitle.rectTransform,
                new Vector2(StartChoiceLayout.TitleMinX, StartChoiceLayout.TitleMinY),
                new Vector2(StartChoiceLayout.TitleMaxX, StartChoiceLayout.TitleMaxY));
            _startTitle.color = UiTheme.Primary;
            _startTitle.lineSpacing = UiTheme.TitleLine;
            _startTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            _startTitle.verticalOverflow = VerticalWrapMode.Truncate;
            _startTitle.raycastTarget = false;
            AddReadability(_startTitle, true);

            _startContinue = CreateButton(
                "StartContinue",
                canvasGo.transform,
                body,
                new Vector2(StartChoiceLayout.ContinueMinX, StartChoiceLayout.ContinueMinY),
                new Vector2(StartChoiceLayout.ContinueMaxX, StartChoiceLayout.ContinueMaxY));
            _startContinueLabel = _startContinue.GetComponentInChildren<Text>();
            FitStartLabel(_startContinueLabel);
            _startContinue.onClick.AddListener(OnStartContinueClick);
            LockButtonNavigation(_startContinue);

            _startNew = CreateButton(
                "StartNew",
                canvasGo.transform,
                body,
                new Vector2(StartChoiceLayout.NewMinX, StartChoiceLayout.NewMinY),
                new Vector2(StartChoiceLayout.NewMaxX, StartChoiceLayout.NewMaxY));
            _startNewLabel = _startNew.GetComponentInChildren<Text>();
            FitStartLabel(_startNewLabel);
            _startNew.onClick.AddListener(OnStartNewClick);
            LockButtonNavigation(_startNew);

            _startQuestion = CreateText(
                "StartQuestion",
                canvasGo.transform,
                body,
                UiTheme.TypeBody,
                TextAnchor.MiddleCenter,
                FontStyle.Bold);
            Stretch(
                _startQuestion.rectTransform,
                new Vector2(StartChoiceLayout.QuestionMinX, StartChoiceLayout.QuestionMinY),
                new Vector2(StartChoiceLayout.QuestionMaxX, StartChoiceLayout.QuestionMaxY));
            _startQuestion.color = UiTheme.Primary;
            _startQuestion.lineSpacing = UiTheme.BodyLine;
            _startQuestion.horizontalOverflow = HorizontalWrapMode.Wrap;
            _startQuestion.verticalOverflow = VerticalWrapMode.Truncate;
            _startQuestion.raycastTarget = false;
            AddReadability(_startQuestion, true);

            _startYes = CreateButton(
                "StartYes",
                canvasGo.transform,
                body,
                new Vector2(StartChoiceLayout.YesMinX, StartChoiceLayout.YesMinY),
                new Vector2(StartChoiceLayout.YesMaxX, StartChoiceLayout.YesMaxY));
            _startYesLabel = _startYes.GetComponentInChildren<Text>();
            FitStartLabel(_startYesLabel);
            _startYes.onClick.AddListener(OnStartYesClick);
            LockButtonNavigation(_startYes);
            UiTheme.ApplyButton(_startYes, false, false, false);

            _startNo = CreateButton(
                "StartNo",
                canvasGo.transform,
                body,
                new Vector2(StartChoiceLayout.NoMinX, StartChoiceLayout.NoMinY),
                new Vector2(StartChoiceLayout.NoMaxX, StartChoiceLayout.NoMaxY));
            _startNoLabel = _startNo.GetComponentInChildren<Text>();
            FitStartLabel(_startNoLabel);
            _startNo.onClick.AddListener(OnStartNoClick);
            LockButtonNavigation(_startNo);
            UiTheme.ApplyButton(_startNo, true, false, false);

            SetStartConfirmVisible(false);
            _startRoot.SetActive(false);
        }

        private static void FitStartLabel(Text label)
        {
            if (label == null)
            {
                return;
            }

            label.fontSize = UiTheme.TypeBody;
            label.resizeTextForBestFit = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
        }

        public void PresentStartChoice()
        {
            if (_startDecided)
            {
                return;
            }

            _startDecided = true;
            string[] args = System.Environment.GetCommandLineArgs();
            if (StartChoice.ReadsSkipArg(args))
            {
                StartChoice.SkipPanel = true;
            }

            bool capture = StartChoice.CaptureRunning || StartChoice.CaptureCommand(args);
            bool hasSave = _game != null && _game.HasContinueOffer;
            if (StartChoice.ShouldSkip(Application.isBatchMode, capture))
            {
                _startResolved = true;
                HideStartPanel();
                StartSkip kind = StartChoice.WhenSkipped(hasSave);
                if (kind == StartSkip.KeepSave)
                {
                    return;
                }

                Refresh();
                return;
            }

            ShowStart(false);
        }

        /// <summary>
        /// Capture pose. 0 hides the panel. 1 is no save. 2 is a save.
        /// 3 leaves the overwrite confirm open.
        /// </summary>
        public void CaptureStartPanel(int mode)
        {
            if (_startRoot == null)
            {
                return;
            }

            if (mode <= 0)
            {
                _startResolved = true;
                HideStartPanel();
                return;
            }

            ShowStart(mode >= 3);
        }

        private bool StartPanelOpen()
        {
            return _startOpen && !_startResolved;
        }

        private void ShowStart(bool confirm)
        {
            if (_startRoot == null)
            {
                return;
            }

            bool hasSave = _game != null && _game.HasContinueOffer;
            int world = 1;
            int wave = 1;
            if (hasSave && _game.PendingContinue != null)
            {
                wave = _game.PendingContinue.WaveIndex;
                if (wave < 1)
                {
                    wave = 1;
                }

                world = WorldCatalog.NumberForWave(wave);
            }

            _startView = StartChoice.Decide(hasSave, world, wave);
            _startFocus = _startView.Focus;
            _startConfirmFocus = StartChoice.FocusNo;
            _startConfirmOpen = confirm && _startView.ConfirmOverwrite;
            _startOpen = true;
            _startResolved = false;
            _startNavHeld = false;
            _startRoot.SetActive(true);
            RefreshStartCopy();
            Refresh();
            RefreshStartCopy();
        }

        private void HideStartPanel()
        {
            _startOpen = false;
            _startConfirmOpen = false;
            _startNavHeld = false;
            if (_startRoot != null)
            {
                _startRoot.SetActive(false);
            }
        }

        private void RefreshStartCopy()
        {
            if (_startTitle != null)
            {
                _startTitle.text = Loc.T("ui.start.title", "Choose a run");
            }

            if (_startQuestion != null)
            {
                _startQuestion.text = Loc.Tf(
                    "ui.start.overwrite",
                    "Overwrite saved run - World {0} Wave {1}?",
                    _startView.WorldNumber,
                    _startView.WaveIndex);
            }

            if (_startContinueLabel != null)
            {
                _startContinueLabel.text = _startView.ContinueEnabled
                    ? RunSummary.ContinueRunLabel(_startView.WorldNumber, _startView.WaveIndex)
                    : Loc.T("ui.continue", "Continue");
            }

            if (_startNewLabel != null)
            {
                _startNewLabel.text = Loc.T("ui.start.new", "Start new");
            }

            if (_startYesLabel != null)
            {
                _startYesLabel.text = Loc.T("ui.confirm.yes", "Yes");
            }

            if (_startNoLabel != null)
            {
                _startNoLabel.text = Loc.T("ui.confirm.no", "No");
            }

            SetStartChoiceVisible(!_startConfirmOpen);
            SetStartConfirmVisible(_startConfirmOpen);
            PaintStartFocus();
        }

        private void SetStartChoiceVisible(bool visible)
        {
            if (_startTitle != null)
            {
                _startTitle.gameObject.SetActive(visible);
            }

            if (_startContinue != null)
            {
                _startContinue.gameObject.SetActive(visible);
            }

            if (_startNew != null)
            {
                _startNew.gameObject.SetActive(visible);
            }
        }

        private void SetStartConfirmVisible(bool visible)
        {
            if (_startQuestion != null)
            {
                _startQuestion.gameObject.SetActive(visible);
            }

            if (_startYes != null)
            {
                _startYes.gameObject.SetActive(visible);
            }

            if (_startNo != null)
            {
                _startNo.gameObject.SetActive(visible);
            }
        }

        private void PaintStartFocus()
        {
            bool continueFocus = !_startConfirmOpen
                && _startView.ContinueEnabled
                && _startFocus == StartChoice.FocusContinue;
            bool newFocus = !_startConfirmOpen && _startFocus == StartChoice.FocusNew;
            if (_startContinue != null)
            {
                bool enabled = _startView.ContinueEnabled;
                _startContinue.interactable = enabled;
                UiTheme.ApplyButton(_startContinue, enabled, false, false);
                if (!enabled)
                {
                    ColorBlock colors = _startContinue.colors;
                    colors.disabledColor = Color.white;
                    _startContinue.colors = colors;
                    Image plate = _startContinue.targetGraphic as Image;
                    if (plate != null)
                    {
                        plate.color = UiTheme.Disabled;
                    }

                    if (_startContinueLabel != null)
                    {
                        _startContinueLabel.color = UiTheme.DisabledRgb;
                    }
                }
                else if (_startContinueLabel != null)
                {
                    _startContinueLabel.color = UiTheme.Accent;
                }

                UiTheme.SetPadFocus(_startContinue.gameObject, continueFocus, false);
            }

            if (_startNew != null)
            {
                UiTheme.ApplyButton(_startNew, newFocus && !_startView.ContinueEnabled, false, false);
                if (_startNewLabel != null)
                {
                    _startNewLabel.color = UiTheme.Accent;
                }

                UiTheme.SetPadFocus(_startNew.gameObject, newFocus, false);
            }

            if (_startYes != null)
            {
                UiTheme.SetPadFocus(_startYes.gameObject, _startConfirmOpen && _startConfirmFocus == StartChoice.FocusYes, false);
            }

            if (_startNo != null)
            {
                UiTheme.SetPadFocus(_startNo.gameObject, _startConfirmOpen && _startConfirmFocus == StartChoice.FocusNo, false);
            }
        }

        private void TickStartPanel()
        {
            if (!StartPanelOpen())
            {
                return;
            }

            StartRequest request = ReadStartRequest();
            ApplyStartAction(StartChoice.Route(request), request);
        }

        private StartRequest ReadStartRequest()
        {
            StartRequest request = new StartRequest();
            request.PanelOpen = _startOpen;
            request.ConfirmOpen = _startConfirmOpen;
            request.Focus = _startFocus;
            request.ConfirmFocus = _startConfirmFocus;
            request.ContinueEnabled = _startView.ContinueEnabled;
            request.HasSave = _startView.ConfirmOverwrite;
            request.Escape = Input.GetKeyDown(KeyCode.Escape);
            request.Start = GamepadInput.PausePressed() && !request.Escape;
            request.Cancel = GamepadInput.CancelPressed() && !request.Escape;
            request.Submit = GamepadInput.ConfirmPressed()
                || Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Space);
            Vector2 stick = GamepadInput.UiNavCombined(PadNavSource.Both);
            int delta = HangarPadNav.DominantStep(stick.x, stick.y, HangarPadNav.Flick);
            if (delta == 0)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    delta = -1;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    delta = 1;
                }
            }

            request.FocusDelta = delta;
            return request;
        }

        private StartRequest ClickRequest()
        {
            StartRequest request = new StartRequest();
            request.PanelOpen = _startOpen;
            request.ConfirmOpen = _startConfirmOpen;
            request.Focus = _startFocus;
            request.ConfirmFocus = _startConfirmFocus;
            request.ContinueEnabled = _startView.ContinueEnabled;
            request.HasSave = _startView.ConfirmOverwrite;
            return request;
        }

        private void OnStartContinueClick()
        {
            if (!_startView.ContinueEnabled || _startConfirmOpen)
            {
                return;
            }

            StartRequest request = ClickRequest();
            request.ContinueClick = true;
            request.Focus = StartChoice.FocusContinue;
            ApplyStartAction(StartChoice.Route(request), request);
        }

        private void OnStartNewClick()
        {
            if (_startConfirmOpen)
            {
                return;
            }

            StartRequest request = ClickRequest();
            request.NewClick = true;
            request.Focus = StartChoice.FocusNew;
            ApplyStartAction(StartChoice.Route(request), request);
        }

        private void OnStartYesClick()
        {
            if (!_startConfirmOpen)
            {
                return;
            }

            StartRequest request = ClickRequest();
            request.YesClick = true;
            request.ConfirmFocus = StartChoice.FocusYes;
            ApplyStartAction(StartChoice.Route(request), request);
        }

        private void OnStartNoClick()
        {
            if (!_startConfirmOpen)
            {
                return;
            }

            StartRequest request = ClickRequest();
            request.NoClick = true;
            request.ConfirmFocus = StartChoice.FocusNo;
            ApplyStartAction(StartChoice.Route(request), request);
        }

        private void OnStartScrim()
        {
            if (!StartPanelOpen())
            {
                return;
            }

            StartRequest request = ClickRequest();
            request.Scrim = true;
            ApplyStartAction(StartChoice.Route(request), request);
        }

        private void ApplyStartAction(StartAction action, StartRequest request)
        {
            if (action == StartAction.None || action == StartAction.Blocked)
            {
                if (request.FocusDelta == 0)
                {
                    _startNavHeld = false;
                }

                return;
            }

            if (action == StartAction.MoveFocus)
            {
                float now = Time.unscaledTime;
                if (_startNavHeld && now < _startNavRepeatAt)
                {
                    return;
                }

                if (_startConfirmOpen)
                {
                    _startConfirmFocus = StartChoice.MoveOverwrite(_startConfirmFocus, request.FocusDelta);
                }
                else
                {
                    _startFocus = StartChoice.MoveChoice(_startFocus, request.FocusDelta, _startView.ContinueEnabled);
                }

                PaintStartFocus();
                _startNavRepeatAt = now + (_startNavHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
                _startNavHeld = true;
                return;
            }

            if (action == StartAction.Continue)
            {
                _startResolved = true;
                HideStartPanel();
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayUiConfirm();
                }

                if (_game != null)
                {
                    _game.AcceptContinue();
                }

                return;
            }

            if (action == StartAction.AskOverwrite)
            {
                _startConfirmOpen = true;
                _startConfirmFocus = StartChoice.FocusNo;
                _startNavHeld = false;
                RefreshStartCopy();
                return;
            }

            if (action == StartAction.BeginNewRun)
            {
                _startResolved = true;
                HideStartPanel();
                Refresh();
                return;
            }

            if (action == StartAction.OverwriteNo)
            {
                _startConfirmOpen = false;
                _startNavHeld = false;
                RefreshStartCopy();
                return;
            }

            if (action == StartAction.OverwriteYes)
            {
                _startResolved = true;
                HideStartPanel();
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayUiConfirm();
                }

                if (_game != null)
                {
                    _game.AbandonSavedRun();
                }
            }
        }
    }
}
