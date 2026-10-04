namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Unity KeyCode values used by the legacy Input Manager. Kept as ints so
    /// the map can round-trip without a UnityEngine reference.
    /// </summary>
    public static class BindCodes
    {
        public const int Return = 13;
        public const int Escape = 27;
        public const int Space = 32;
        public const int A = 97;
        public const int D = 100;
        public const int E = 101;
        public const int Q = 113;
        public const int S = 115;
        public const int W = 119;
        public const int LeftControl = 306;
        public const int SubmitPad = 0;
    }

    public static class BindKind
    {
        public const int None = 0;
        public const int Key = 1;
        public const int Mouse = 2;
        public const int Pad = 3;
    }

    public static class BindAction
    {
        public const int Move = 0;
        public const int Fire = 1;
        public const int FireAlt = 2;
        public const int Utility = 3;
        public const int Cycle = 4;
        public const int CyclePrev = 5;
        public const int CycleAlt = 6;
        public const int Pause = 7;
        public const int Confirm = 8;
        public const int Cancel = 9;
        public const int Count = 10;

        public static string Id(int action)
        {
            if (action == Move)
            {
                return "move";
            }

            if (action == Fire)
            {
                return "fire";
            }

            if (action == FireAlt)
            {
                return "fire_alt";
            }

            if (action == Utility)
            {
                return "utility";
            }

            if (action == Cycle)
            {
                return "cycle";
            }

            if (action == CyclePrev)
            {
                return "cycle_prev";
            }

            if (action == CycleAlt)
            {
                return "cycle_alt";
            }

            if (action == Pause)
            {
                return "pause";
            }

            if (action == Confirm)
            {
                return "confirm";
            }

            if (action == Cancel)
            {
                return "cancel";
            }

            return string.Empty;
        }

        public static int Find(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return -1;
            }

            for (int index = 0; index < Count; index++)
            {
                if (Id(index) == action)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// One key, mouse button, or joystick button. Empty slots are Kind None.
    /// </summary>
    public struct BindControl
    {
        public int Kind;
        public int Code;

        public static BindControl Clear()
        {
            BindControl slot = new BindControl();
            slot.Kind = BindKind.None;
            slot.Code = 0;
            return slot;
        }

        public static BindControl Key(int code)
        {
            BindControl slot = new BindControl();
            slot.Kind = BindKind.Key;
            slot.Code = code;
            return slot;
        }

        public static BindControl Mouse(int code)
        {
            BindControl slot = new BindControl();
            slot.Kind = BindKind.Mouse;
            slot.Code = code;
            return slot;
        }

        public static BindControl Pad(int code)
        {
            BindControl slot = new BindControl();
            slot.Kind = BindKind.Pad;
            slot.Code = code;
            return slot;
        }

        public bool Empty()
        {
            return Kind == BindKind.None;
        }

        public bool IsEscape()
        {
            return Kind == BindKind.Key && Code == BindCodes.Escape;
        }

        public bool Equal(BindControl other)
        {
            return Kind == other.Kind && Code == other.Code;
        }
    }

    /// <summary>
    /// Keyboard/mouse and gamepad-button bindings. Sticks and triggers are
    /// Input Manager axes and are not stored. Escape stays pause and cancel.
    /// Assigning a control that another action already uses swaps the two.
    /// </summary>
    public sealed class BindingMap
    {
        public const int AssignIgnore = 0;
        public const int AssignOk = 1;
        public const int AssignSwap = 2;
        public const int AssignReserved = 3;
        public const int AssignFixed = 4;

        public static int Revision;

        public BindControl[] Key0;
        public BindControl[] Key1;
        public BindControl[] Key2;
        public BindControl[] Key3;
        public BindControl[] Pad0;

        private int _moveCursor;
        private static BindingMap _active;
        private static BindingMap _defaults;

        public static BindingMap ActiveOrDefault()
        {
            if (_active == null)
            {
                _active = CreateDefault();
            }

            return _active;
        }

        public static void Publish(BindingMap map)
        {
            _active = map ?? CreateDefault();
            Revision += 1;
        }

        public static BindingMap Defaults()
        {
            if (_defaults == null)
            {
                _defaults = CreateDefault();
            }

            return _defaults;
        }

        public static BindingMap CreateDefault()
        {
            BindingMap map = new BindingMap();
            map.Key0 = Blank();
            map.Key1 = Blank();
            map.Key2 = Blank();
            map.Key3 = Blank();
            map.Pad0 = Blank();
            map._moveCursor = 0;

            map.Key0[BindAction.Move] = BindControl.Key(BindCodes.W);
            map.Key1[BindAction.Move] = BindControl.Key(BindCodes.A);
            map.Key2[BindAction.Move] = BindControl.Key(BindCodes.S);
            map.Key3[BindAction.Move] = BindControl.Key(BindCodes.D);

            map.Key0[BindAction.Fire] = BindControl.Mouse(0);
            map.Key1[BindAction.Fire] = BindControl.Key(BindCodes.LeftControl);

            map.Key0[BindAction.FireAlt] = BindControl.Key(BindCodes.Space);
            map.Pad0[BindAction.FireAlt] = BindControl.Pad(0);

            map.Key0[BindAction.Utility] = BindControl.Mouse(1);
            map.Key1[BindAction.Utility] = BindControl.Key(BindCodes.E);

            map.Key0[BindAction.Cycle] = BindControl.Key(BindCodes.Q);
            map.Pad0[BindAction.Cycle] = BindControl.Pad(4);

            map.Pad0[BindAction.CyclePrev] = BindControl.Pad(5);
            map.Pad0[BindAction.CycleAlt] = BindControl.Pad(2);

            map.Key0[BindAction.Pause] = BindControl.Key(BindCodes.Escape);
            map.Pad0[BindAction.Pause] = BindControl.Pad(7);

            map.Key0[BindAction.Confirm] = BindControl.Key(BindCodes.Return);
            map.Pad0[BindAction.Confirm] = BindControl.Pad(0);

            map.Key0[BindAction.Cancel] = BindControl.Key(BindCodes.Escape);
            map.Pad0[BindAction.Cancel] = BindControl.Pad(1);
            return map;
        }

        public void ResetToDefaults()
        {
            CopyFrom(Defaults());
            _moveCursor = 0;
            Revision += 1;
        }

        public bool MoveIsDefault()
        {
            return KeyboardIsDefault(BindAction.Move);
        }

        public bool KeyboardIsDefault(int action)
        {
            BindingMap stock = Defaults();
            if (action < 0 || action >= BindAction.Count)
            {
                return true;
            }

            return GetKey(action, 0).Equal(stock.GetKey(action, 0))
                && GetKey(action, 1).Equal(stock.GetKey(action, 1))
                && GetKey(action, 2).Equal(stock.GetKey(action, 2))
                && GetKey(action, 3).Equal(stock.GetKey(action, 3));
        }

        public bool PadIsDefault(int action)
        {
            BindingMap stock = Defaults();
            if (action < 0 || action >= BindAction.Count)
            {
                return true;
            }

            return Pad0[action].Equal(stock.Pad0[action]);
        }

        public bool UsesLegacyFire()
        {
            return KeyboardIsDefault(BindAction.Fire)
                && KeyboardIsDefault(BindAction.FireAlt)
                && PadIsDefault(BindAction.Fire)
                && PadIsDefault(BindAction.FireAlt);
        }

        public bool UsesLegacyUtility()
        {
            return KeyboardIsDefault(BindAction.Utility) && PadIsDefault(BindAction.Utility);
        }

        public bool UsesLegacyCycle()
        {
            return KeyboardIsDefault(BindAction.Cycle)
                && PadIsDefault(BindAction.Cycle)
                && KeyboardIsDefault(BindAction.CycleAlt)
                && PadIsDefault(BindAction.CycleAlt);
        }

        public bool UsesLegacyCyclePrev()
        {
            return KeyboardIsDefault(BindAction.CyclePrev) && PadIsDefault(BindAction.CyclePrev);
        }

        public bool UsesLegacyPause()
        {
            return KeyboardIsDefault(BindAction.Pause) && PadIsDefault(BindAction.Pause);
        }

        public bool UsesLegacyConfirm()
        {
            return KeyboardIsDefault(BindAction.Confirm) && PadIsDefault(BindAction.Confirm);
        }

        public bool UsesLegacyCancel()
        {
            return KeyboardIsDefault(BindAction.Cancel) && PadIsDefault(BindAction.Cancel);
        }

        public BindControl GetKey(int action, int slot)
        {
            if (slot == 1)
            {
                return Key1[action];
            }

            if (slot == 2)
            {
                return Key2[action];
            }

            if (slot == 3)
            {
                return Key3[action];
            }

            return Key0[action];
        }

        public void ClearAll()
        {
            for (int action = 0; action < BindAction.Count; action++)
            {
                Key0[action] = BindControl.Clear();
                Key1[action] = BindControl.Clear();
                Key2[action] = BindControl.Clear();
                Key3[action] = BindControl.Clear();
                Pad0[action] = BindControl.Clear();
            }
        }

        /// <summary>
        /// Writes a captured key, mouse button, or joystick button.
        /// Escape cannot move onto any action except pause and cancel.
        /// A pad button on move is rejected (the stick is an axis).
        /// </summary>
        public int TryAssign(int action, int kind, int code, out int otherAction)
        {
            otherAction = -1;
            if (action < 0 || action >= BindAction.Count)
            {
                return AssignFixed;
            }

            if (kind == BindKind.None)
            {
                return AssignIgnore;
            }

            BindControl next = BindControl.Clear();
            if (kind == BindKind.Key)
            {
                next = BindControl.Key(code);
            }
            else if (kind == BindKind.Mouse)
            {
                next = BindControl.Mouse(code);
            }
            else if (kind == BindKind.Pad)
            {
                next = BindControl.Pad(code);
            }

            if (next.Empty() || code < 0)
            {
                return AssignIgnore;
            }

            if (kind == BindKind.Key && code == 0)
            {
                return AssignIgnore;
            }

            if (next.IsEscape() && action != BindAction.Pause && action != BindAction.Cancel)
            {
                return AssignReserved;
            }

            bool pad = kind == BindKind.Pad;
            if (pad && action == BindAction.Move)
            {
                return AssignFixed;
            }

            if (Owns(action, next))
            {
                return AssignIgnore;
            }

            int foundAction;
            int foundSlot;
            bool foundPad;
            bool hit = FindOwner(next, action, out foundAction, out foundSlot, out foundPad);
            int destSlot = 0;
            if (!pad && action == BindAction.Move)
            {
                destSlot = _moveCursor;
                if (destSlot < 0 || destSlot > 3)
                {
                    destSlot = 0;
                }
            }

            BindControl previous = pad ? Pad0[action] : GetKey(action, destSlot);
            if (hit && previous.IsEscape())
            {
                SetSlot(action, pad, destSlot, next);
                SetSlot(foundAction, foundPad, foundSlot, BindControl.Clear());
                otherAction = foundAction;
                AdvanceMove(action, pad);
                Revision += 1;
                return AssignSwap;
            }

            SetSlot(action, pad, destSlot, next);
            AdvanceMove(action, pad);
            Revision += 1;
            if (!hit)
            {
                return AssignOk;
            }

            SetSlot(foundAction, foundPad, foundSlot, previous);
            otherAction = foundAction;
            return AssignSwap;
        }

        /// <summary>
        /// Escape may sit on pause and cancel only. Pad A (submit) may be shared,
        /// because it is the confirm fallback and the default alt-fire. Every
        /// other control has one owner. A pad button on move is rejected.
        /// </summary>
        public bool OwnersAreValid()
        {
            if (Key0 == null || Key1 == null || Key2 == null || Key3 == null || Pad0 == null)
            {
                return false;
            }

            for (int action = 0; action < BindAction.Count; action++)
            {
                for (int slot = 0; slot < 5; slot++)
                {
                    BindControl control = SlotControl(action, slot);
                    if (control.Empty())
                    {
                        continue;
                    }

                    if (control.IsEscape())
                    {
                        if (action != BindAction.Pause && action != BindAction.Cancel)
                        {
                            return false;
                        }

                        continue;
                    }

                    if (control.Kind == BindKind.Pad && control.Code == BindCodes.SubmitPad)
                    {
                        continue;
                    }

                    if (control.Kind == BindKind.Pad && action == BindAction.Move)
                    {
                        return false;
                    }

                    if (CountOwners(control) > 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsHeld(int action, IBindSource source, bool edge)
        {
            if (source == null || action < 0 || action >= BindAction.Count)
            {
                return false;
            }

            if (Hit(GetKey(action, 0), source, edge))
            {
                return true;
            }

            if (Hit(GetKey(action, 1), source, edge))
            {
                return true;
            }

            if (Hit(GetKey(action, 2), source, edge))
            {
                return true;
            }

            if (Hit(GetKey(action, 3), source, edge))
            {
                return true;
            }

            if (Pad0[action].Kind == BindKind.Pad)
            {
                int button = Pad0[action].Code;
                if (edge)
                {
                    return source.PadDown(button);
                }

                return source.PadHeld(button);
            }

            return false;
        }

        private void AdvanceMove(int action, bool pad)
        {
            if (pad || action != BindAction.Move)
            {
                return;
            }

            _moveCursor += 1;
            if (_moveCursor > 3)
            {
                _moveCursor = 0;
            }
        }

        private BindControl SlotControl(int action, int slot)
        {
            if (slot >= 4)
            {
                return Pad0[action];
            }

            return GetKey(action, slot);
        }

        private int CountOwners(BindControl control)
        {
            int count = 0;
            for (int action = 0; action < BindAction.Count; action++)
            {
                for (int slot = 0; slot < 5; slot++)
                {
                    BindControl stored = SlotControl(action, slot);
                    if (!stored.Empty() && stored.Equal(control))
                    {
                        count += 1;
                    }
                }
            }

            return count;
        }

        private bool Owns(int action, BindControl control)
        {
            if (control.Kind == BindKind.Pad)
            {
                return Pad0[action].Equal(control);
            }

            return GetKey(action, 0).Equal(control)
                || GetKey(action, 1).Equal(control)
                || GetKey(action, 2).Equal(control)
                || GetKey(action, 3).Equal(control);
        }

        private bool FindOwner(BindControl control, int skipAction, out int other, out int slot, out bool pad)
        {
            other = -1;
            slot = -1;
            pad = false;
            bool wantPad = control.Kind == BindKind.Pad;
            for (int action = 0; action < BindAction.Count; action++)
            {
                if (action == skipAction)
                {
                    continue;
                }

                if (wantPad)
                {
                    if (Pad0[action].Equal(control) && !Pad0[action].Empty())
                    {
                        other = action;
                        slot = 0;
                        pad = true;
                        return true;
                    }

                    continue;
                }

                for (int keySlot = 0; keySlot < 4; keySlot++)
                {
                    BindControl stored = GetKey(action, keySlot);
                    if (!stored.Empty() && stored.Equal(control))
                    {
                        other = action;
                        slot = keySlot;
                        pad = false;
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetSlot(int action, bool pad, int slot, BindControl value)
        {
            if (pad)
            {
                Pad0[action] = value;
                return;
            }

            if (slot == 1)
            {
                Key1[action] = value;
                return;
            }

            if (slot == 2)
            {
                Key2[action] = value;
                return;
            }

            if (slot == 3)
            {
                Key3[action] = value;
                return;
            }

            Key0[action] = value;
        }

        private static bool Hit(BindControl control, IBindSource source, bool edge)
        {
            if (control.Kind == BindKind.Key)
            {
                if (edge)
                {
                    return source.KeyDown(control.Code);
                }

                return source.KeyHeld(control.Code);
            }

            if (control.Kind == BindKind.Mouse)
            {
                if (edge)
                {
                    return source.MouseDown(control.Code);
                }

                return source.MouseHeld(control.Code);
            }

            return false;
        }

        private void CopyFrom(BindingMap source)
        {
            Key0 = Clone(source.Key0);
            Key1 = Clone(source.Key1);
            Key2 = Clone(source.Key2);
            Key3 = Clone(source.Key3);
            Pad0 = Clone(source.Pad0);
        }

        private static BindControl[] Blank()
        {
            BindControl[] slots = new BindControl[BindAction.Count];
            for (int index = 0; index < BindAction.Count; index++)
            {
                slots[index] = BindControl.Clear();
            }

            return slots;
        }

        private static BindControl[] Clone(BindControl[] source)
        {
            BindControl[] copy = new BindControl[BindAction.Count];
            for (int index = 0; index < BindAction.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }
}
