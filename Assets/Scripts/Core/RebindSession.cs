namespace AsteroidsGoneRogue
{
    public static class RebindTick
    {
        public const int None = 0;
        public const int Changed = 1;
        public const int Abort = 2;
        public const int Timeout = 3;
        public const int Reserved = 4;
        public const int Fixed = 5;
    }

    /// <summary>
    /// Controls list: ten actions, reset, and back. Listening captures the
    /// next key, mouse button, or joystick button for five seconds.
    /// Escape or cancel aborts. A timeout returns to the list.
    /// </summary>
    public sealed class RebindSession
    {
        public const int ResetRow = BindAction.Count;
        public const int BackRow = BindAction.Count + 1;
        public const int RowCount = BindAction.Count + 2;
        public const float ListenSeconds = 5f;

        public int Index;
        public bool Listening;
        public string Note;
        public string NoteOther;

        private float _listenStart;
        private int _armStamp;

        public RebindSession()
        {
            Index = 0;
            Listening = false;
            Note = string.Empty;
            NoteOther = string.Empty;
            _listenStart = 0f;
            _armStamp = -1;
        }

        public float ListenLeft(float now)
        {
            if (!Listening)
            {
                return 0f;
            }

            float left = ListenSeconds - (now - _listenStart);
            if (left < 0f)
            {
                return 0f;
            }

            return left;
        }

        public void Nudge(int delta)
        {
            if (Listening || delta == 0)
            {
                return;
            }

            int step = delta > 0 ? 1 : -1;
            int next = Index + step;
            if (next < 0)
            {
                next = 0;
            }

            if (next >= RowCount)
            {
                next = RowCount - 1;
            }

            Index = next;
        }

        public bool Activate(float now, int stamp)
        {
            return Click(Index, now, stamp);
        }

        /// <summary>
        /// True when the click should close the screen (the Back row).
        /// </summary>
        public bool Click(int index, float now, int stamp)
        {
            if (index < 0)
            {
                index = 0;
            }

            if (index >= RowCount)
            {
                index = RowCount - 1;
            }

            Listening = false;
            Index = index;
            if (index == BackRow)
            {
                Note = string.Empty;
                NoteOther = string.Empty;
                return true;
            }

            if (index == ResetRow)
            {
                Note = "reset";
                NoteOther = string.Empty;
                return false;
            }

            BeginListen(now, stamp);
            return false;
        }

        /// <summary>
        /// True when the screen should close. A listen in progress only aborts.
        /// </summary>
        public bool Cancel()
        {
            if (Listening)
            {
                Listening = false;
                Note = string.Empty;
                NoteOther = string.Empty;
                return false;
            }

            return true;
        }

        public int Tick(float now, int stamp, IBindSource source, BindingMap map)
        {
            if (!Listening)
            {
                return RebindTick.None;
            }

            if (stamp == _armStamp)
            {
                return RebindTick.None;
            }

            if (ListenLeft(now) <= 0f)
            {
                Listening = false;
                Note = "timeout";
                NoteOther = string.Empty;
                return RebindTick.Timeout;
            }

            int kind;
            int code;
            bool abort;
            BoundInput.FillCapture(source, map, out kind, out code, out abort);
            if (abort)
            {
                Listening = false;
                Note = string.Empty;
                NoteOther = string.Empty;
                return RebindTick.Abort;
            }

            if (kind == BindKind.None)
            {
                return RebindTick.None;
            }

            int otherAction;
            int status = map.TryAssign(Index, kind, code, out otherAction);
            Listening = false;
            if (status == BindingMap.AssignReserved)
            {
                Note = "reserved";
                NoteOther = string.Empty;
                return RebindTick.Reserved;
            }

            if (status == BindingMap.AssignFixed)
            {
                Note = "fixed";
                NoteOther = string.Empty;
                return RebindTick.Fixed;
            }

            if (status == BindingMap.AssignSwap)
            {
                Note = "swap";
                NoteOther = BindAction.Id(otherAction);
                return RebindTick.Changed;
            }

            if (status == BindingMap.AssignOk)
            {
                Note = string.Empty;
                NoteOther = string.Empty;
                return RebindTick.Changed;
            }

            Note = string.Empty;
            NoteOther = string.Empty;
            return RebindTick.None;
        }

        private void BeginListen(float now, int stamp)
        {
            Listening = true;
            _listenStart = now;
            _armStamp = stamp;
            Note = "listen";
            NoteOther = string.Empty;
        }
    }

    /// <summary>
    /// Panel fractions for the controls list. The list is taller than the
    /// mask, so focus shifts a row fully inside the viewport.
    /// </summary>
    public static class RebindList
    {
        public const float ViewportTop = 0.83f;
        public const float ViewportBottom = 0.05f;
        public const float RowHeight = 0.062f;
        public const float RowGap = 0.006f;
        public const int RowFont = 18;
        public const float TextMinX = 0.04f;
        public const float TextMaxX = 0.96f;
        public const float PanelMinX = 0.22f;
        public const float PanelMaxX = 0.78f;
        public const float PanelMinY = 0.08f;
        public const float PanelMaxY = 0.92f;

        public static float ViewportSpan
        {
            get { return ViewportTop - ViewportBottom; }
        }

        public static void ContentBand(int index, out float bottom, out float top)
        {
            int count = RebindSession.RowCount;
            int clamped = index;
            if (clamped < 0)
            {
                clamped = 0;
            }

            if (clamped >= count)
            {
                clamped = count - 1;
            }

            float cursor = ViewportTop;
            for (int row = 0; row < count; row++)
            {
                float rowTop = cursor;
                float rowBottom = cursor - RowHeight;
                if (row == clamped)
                {
                    bottom = rowBottom;
                    top = rowTop;
                    return;
                }

                cursor = rowBottom - RowGap;
            }

            bottom = ViewportBottom;
            top = ViewportTop;
        }

        public static float ShiftForFocus(int focusIndex)
        {
            float bottom;
            float top;
            ContentBand(focusIndex, out bottom, out top);
            float shift = 0f;
            if (bottom < ViewportBottom)
            {
                shift = ViewportBottom - bottom;
            }

            if (top + shift > ViewportTop)
            {
                shift = ViewportTop - top;
            }

            if (shift < 0f)
            {
                shift = 0f;
            }

            return shift;
        }

        public static void ViewportBand(int index, int focusIndex, out float bottom, out float top)
        {
            float panelBottom;
            float panelTop;
            ContentBand(index, out panelBottom, out panelTop);
            float shift = ShiftForFocus(focusIndex);
            panelBottom += shift;
            panelTop += shift;
            float span = ViewportSpan;
            if (span < 0.0001f)
            {
                span = 0.0001f;
            }

            bottom = (panelBottom - ViewportBottom) / span;
            top = (panelTop - ViewportBottom) / span;
        }

        public static bool ContainsBand(float bottom, float top)
        {
            return bottom >= -0.001f && top <= 1.001f && top > bottom;
        }
    }
}
