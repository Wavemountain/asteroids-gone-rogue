using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Controls list above settings. Pad up/down, confirm, and cancel.
    /// Mouse clicks the same rows. No pulse, so Reduce effects stays still.
    /// </summary>
    public sealed class RebindOverlay
    {
        private readonly GameObject _root;
        private readonly Text _title;
        private readonly Text _note;
        private readonly Button[] _rows;
        private readonly Text[] _names;
        private readonly Text[] _values;
        private readonly Text[] _icons;
        private readonly RebindSession _session;
        private UnityEngine.Events.UnityAction _changed;
        private bool _navHeld;
        private float _navRepeatAt;

        private RebindOverlay(
            GameObject root,
            Text title,
            Text note,
            Button[] rows,
            Text[] names,
            Text[] values,
            Text[] icons)
        {
            _root = root;
            _title = title;
            _note = note;
            _rows = rows;
            _names = names;
            _values = values;
            _icons = icons;
            _session = new RebindSession();
        }

        public bool IsOpen
        {
            get { return _root != null && _root.activeSelf; }
        }

        public void SetChanged(UnityEngine.Events.UnityAction changed)
        {
            _changed = changed;
        }

        public static RebindOverlay Create(Transform parent)
        {
            Font display = UiFonts.Display();
            Font body = UiFonts.Body();
            GameObject root = new GameObject("RebindRoot");
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            UiTheme.Stretch(rootRect, Vector2.zero, Vector2.one);
            Raise(root);

            GameObject scrim = UiTheme.Fill(
                "RebindScrim",
                root.transform,
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
            Navigation scrimNav = scrimButton.navigation;
            scrimNav.mode = Navigation.Mode.None;
            scrimButton.navigation = scrimNav;

            GameObject panel = UiTheme.BuildPanel(
                "RebindPanel",
                root.transform,
                new Vector2(RebindList.PanelMinX, RebindList.PanelMinY),
                new Vector2(RebindList.PanelMaxX, RebindList.PanelMaxY),
                0.90f);
            Image panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.raycastTarget = true;
            }

            Text title = MakeText("RebindTitle", panel.transform, display, 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiTheme.Stretch(title.rectTransform, new Vector2(0.06f, 0.915f), new Vector2(0.94f, 0.985f));
            title.color = UiTheme.Primary;

            Text note = MakeText("RebindNote", panel.transform, body, RebindList.RowFont, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiTheme.Stretch(note.rectTransform, new Vector2(0.06f, 0.845f), new Vector2(0.94f, 0.91f));
            note.color = UiTheme.Accent;
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            note.verticalOverflow = VerticalWrapMode.Truncate;

            GameObject viewport = new GameObject("RebindViewport");
            viewport.transform.SetParent(panel.transform, false);
            RectTransform viewRect = viewport.AddComponent<RectTransform>();
            UiTheme.Stretch(
                viewRect,
                new Vector2(0f, RebindList.ViewportBottom),
                new Vector2(1f, RebindList.ViewportTop));
            viewport.AddComponent<RectMask2D>();

            int count = RebindSession.RowCount;
            Button[] rows = new Button[count];
            Text[] names = new Text[count];
            Text[] values = new Text[count];
            Text[] icons = new Text[count];
            RebindOverlay overlay = new RebindOverlay(root, title, note, rows, names, values, icons);
            scrimButton.onClick.AddListener(overlay.Close);

            for (int rowIndex = 0; rowIndex < count; rowIndex++)
            {
                int picked = rowIndex;
                Button row = MakeButton("RebindRow" + rowIndex.ToString(), viewport.transform, body);
                rows[rowIndex] = row;
                row.onClick.AddListener(() => overlay.ClickRow(picked));
                icons[rowIndex] = MakeText("Icon", row.transform, body, RebindList.RowFont, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiTheme.Stretch(icons[rowIndex].rectTransform, new Vector2(0.02f, 0.08f), new Vector2(0.12f, 0.92f));
                icons[rowIndex].color = UiTheme.Accent;
                names[rowIndex] = MakeText("Name", row.transform, body, RebindList.RowFont, TextAnchor.MiddleLeft, FontStyle.Bold);
                UiTheme.Stretch(names[rowIndex].rectTransform, new Vector2(0.14f, 0.08f), new Vector2(0.52f, 0.92f));
                names[rowIndex].color = UiTheme.Accent;
                names[rowIndex].horizontalOverflow = HorizontalWrapMode.Wrap;
                names[rowIndex].verticalOverflow = VerticalWrapMode.Truncate;
                values[rowIndex] = MakeText("Value", row.transform, body, RebindList.RowFont, TextAnchor.MiddleRight, FontStyle.Bold);
                UiTheme.Stretch(values[rowIndex].rectTransform, new Vector2(0.52f, 0.08f), new Vector2(0.96f, 0.92f));
                values[rowIndex].color = UiTheme.Primary;
                values[rowIndex].horizontalOverflow = HorizontalWrapMode.Wrap;
                values[rowIndex].verticalOverflow = VerticalWrapMode.Truncate;
            }

            overlay.Close();
            return overlay;
        }

        public void Open()
        {
            if (_root == null)
            {
                return;
            }

            _session.Index = 0;
            _session.Listening = false;
            _session.Note = string.Empty;
            _session.NoteOther = string.Empty;
            _navHeld = false;
            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
            Refresh();
        }

        public void Close()
        {
            _session.Listening = false;
            _navHeld = false;
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        public void Tick(float now, int stamp)
        {
            if (!IsOpen)
            {
                return;
            }

            if (_session.Listening)
            {
                int result = _session.Tick(now, stamp, GamepadInput.Live, BindingMap.ActiveOrDefault());
                if (result == RebindTick.Changed)
                {
                    Notify();
                }

                Refresh();
                return;
            }

            bool escape = Input.GetKeyDown(KeyCode.Escape);
            bool cancel = GamepadInput.CancelPressed();
            bool start = GamepadInput.PausePressed();
            if (escape || cancel || start)
            {
                if (_session.Cancel())
                {
                    Close();
                }

                return;
            }

            if (GamepadInput.ConfirmPressed())
            {
                bool close = _session.Activate(now, stamp);
                if (_session.Note == "reset")
                {
                    BindingMap.ActiveOrDefault().ResetToDefaults();
                    Notify();
                }

                if (close)
                {
                    Close();
                    return;
                }

                Refresh();
                return;
            }

            Vector2 stick = GamepadInput.UiNavCombined(PadNavSource.Both);
            int step = SettingsInputRouter.ScreenStepY(stick.x, stick.y, HangarPadNav.Flick);
            if (step == 0)
            {
                _navHeld = false;
                return;
            }

            if (_navHeld && now < _navRepeatAt)
            {
                return;
            }

            int delta = step > 0 ? -1 : 1;
            _session.Nudge(delta);
            _navRepeatAt = now + (_navHeld ? HangarPadNav.RepeatNextSeconds : HangarPadNav.RepeatFirstSeconds);
            _navHeld = true;
            Refresh();
        }

        public void Refresh()
        {
            if (_title != null)
            {
                _title.text = Loc.T("ui.settings.controls", "Controls");
            }

            if (_note != null)
            {
                _note.text = NoteLine();
            }

            InputScheme scheme = InputSchemeDriver.Current;
            float scale = SettingsMeasure.CanvasScale(Screen.width, Screen.height);
            BindingMap map = BindingMap.ActiveOrDefault();
            int focus = _session.Index;
            for (int rowIndex = 0; rowIndex < _rows.Length; rowIndex++)
            {
                Button row = _rows[rowIndex];
                if (row == null)
                {
                    continue;
                }

                float y0;
                float y1;
                RebindList.ViewportBand(rowIndex, focus, out y0, out y1);
                UiTheme.Stretch(row.GetComponent<RectTransform>(), new Vector2(0.04f, y0), new Vector2(0.96f, y1));
                bool inside = RebindList.ContainsBand(y0, y1);
                row.interactable = inside;
                UiTheme.SetPadFocus(row.gameObject, rowIndex == focus, false);
                PaintRow(rowIndex, scheme, scale, map);
            }
        }

        private void ClickRow(int index)
        {
            bool close = _session.Click(index, Time.unscaledTime, Time.frameCount);
            if (_session.Note == "reset")
            {
                BindingMap.ActiveOrDefault().ResetToDefaults();
                Notify();
            }

            if (close)
            {
                Close();
                return;
            }

            Refresh();
        }

        private void Notify()
        {
            if (_changed != null)
            {
                _changed.Invoke();
            }
        }

        private void PaintRow(int rowIndex, InputScheme scheme, float scale, BindingMap map)
        {
            if (rowIndex < BindAction.Count)
            {
                string token = "{" + BindAction.Id(rowIndex) + "}";
                if (_icons[rowIndex] != null)
                {
                    PromptLineView.Paint(_icons[rowIndex], token, scheme, scale);
                }

                if (_names[rowIndex] != null)
                {
                    _names[rowIndex].text = ActionName(rowIndex);
                }

                if (_values[rowIndex] != null)
                {
                    _values[rowIndex].text = BindingGlyph.BindingLine(rowIndex, scheme, map);
                }

                return;
            }

            if (_icons[rowIndex] != null)
            {
                _icons[rowIndex].text = string.Empty;
            }

            if (rowIndex == RebindSession.ResetRow)
            {
                if (_names[rowIndex] != null)
                {
                    _names[rowIndex].text = Loc.T("ui.rebind.reset", "Reset to defaults");
                }

                if (_values[rowIndex] != null)
                {
                    _values[rowIndex].text = string.Empty;
                }

                return;
            }

            if (_names[rowIndex] != null)
            {
                _names[rowIndex].text = Loc.T("ui.rebind.back", "Back");
            }

            if (_values[rowIndex] != null)
            {
                _values[rowIndex].text = string.Empty;
            }
        }

        private string NoteLine()
        {
            if (_session.Note == "listen")
            {
                return Loc.T("ui.rebind.listen", "Press a key or button...");
            }

            if (_session.Note == "timeout")
            {
                return Loc.T("ui.rebind.timeout", "Timed out");
            }

            if (_session.Note == "reserved")
            {
                return Loc.T("ui.rebind.reserved", "Escape is reserved");
            }

            if (_session.Note == "fixed")
            {
                return Loc.T("ui.rebind.fixed", "Stick stays on the axis");
            }

            if (_session.Note == "swap")
            {
                int other = BindAction.Find(_session.NoteOther);
                string otherName = other >= 0 ? ActionName(other) : string.Empty;
                return Loc.Tf("ui.rebind.swap", "Swapped with {0}", otherName);
            }

            if (_session.Note == "reset")
            {
                return Loc.T("ui.rebind.reset", "Reset to defaults");
            }

            return string.Empty;
        }

        private static string ActionName(int action)
        {
            if (action == BindAction.Move)
            {
                return Loc.T("ui.rebind.move", "Move");
            }

            if (action == BindAction.Fire)
            {
                return Loc.T("ui.rebind.fire", "Fire");
            }

            if (action == BindAction.FireAlt)
            {
                return Loc.T("ui.rebind.fire_alt", "Alt fire");
            }

            if (action == BindAction.Utility)
            {
                return Loc.T("ui.rebind.utility", "Utility");
            }

            if (action == BindAction.Cycle)
            {
                return Loc.T("ui.rebind.cycle", "Cycle");
            }

            if (action == BindAction.CyclePrev)
            {
                return Loc.T("ui.rebind.cycle_prev", "Cycle back");
            }

            if (action == BindAction.CycleAlt)
            {
                return Loc.T("ui.rebind.cycle_alt", "Alt cycle");
            }

            if (action == BindAction.Pause)
            {
                return Loc.T("ui.rebind.pause", "Pause");
            }

            if (action == BindAction.Confirm)
            {
                return Loc.T("ui.rebind.confirm", "Confirm");
            }

            if (action == BindAction.Cancel)
            {
                return Loc.T("ui.rebind.cancel", "Cancel");
            }

            return string.Empty;
        }

        private static void Raise(GameObject root)
        {
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = root.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = CanvasOrder.Rebind;
            if (root.GetComponent<GraphicRaycaster>() == null)
            {
                root.AddComponent<GraphicRaycaster>();
            }
        }

        private static Text MakeText(string name, Transform parent, Font font, int size, TextAnchor anchor, FontStyle style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            UiTheme.Stretch(rect, Vector2.zero, Vector2.one);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = UiTheme.Accent;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private static Button MakeButton(string name, Transform parent, Font font)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = UiTheme.Surface2;
            Button button = go.AddComponent<Button>();
            UiTheme.ApplyButton(button, false, false, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            UiTheme.Stretch(rect, new Vector2(0.04f, 0f), new Vector2(0.96f, 0.06f));
            Text label = MakeText("Label", go.transform, font, RebindList.RowFont, TextAnchor.MiddleCenter, FontStyle.Normal);
            label.text = string.Empty;
            return button;
        }
    }
}
