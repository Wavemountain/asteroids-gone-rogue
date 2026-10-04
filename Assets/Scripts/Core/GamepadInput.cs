using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Legacy Input Manager Xbox / pad map. Keyboard and mouse stay live (hot-plug).
    /// Left stick flies. RT fires primary (axes read separately so they cannot cancel).
    /// LT holds utility (FireTrigger3 / UtilityTrigger). LB/RB cycle primary.
    /// Right stick aims; otherwise face the left-stick fly vector.
    /// </summary>
    public static class GamepadInput
    {
        public const float StickDead = 0.22f;
        public const float TriggerFire = 0.45f;
        public const string MoveX = "Horizontal";
        public const string MoveY = "Vertical";
        public const string PadMoveX = "PadMoveX";
        public const string PadMoveY = "PadMoveY";
        public const string AimX = "AimX";
        public const string AimY = "AimY";
        public const string FireTrigger = "FireTrigger";
        public const string FireTrigger3 = "FireTrigger3";
        public const string FireTrigger6 = "FireTrigger6";
        public const string UtilityTrigger = "UtilityTrigger";
        public const string FirePad = "FirePad";
        public const string CycleFire = "CycleFire";
        public const string Pause = "Pause";
        public const string PadDpadX = "PadDpadX";
        public const string PadDpadY = "PadDpadY";
        public const string DpadUp = "DpadUp";
        public const string DpadDown = "DpadDown";
        public const string DpadLeft = "DpadLeft";
        public const string DpadRight = "DpadRight";

        public static IBindSource Live
        {
            get { return LiveBindSource.Instance; }
        }

        public static Vector2 MoveStick()
        {
            BindVec2 raw = BoundInput.MoveVector(Live);
            return Deadzone(new Vector2(raw.X, raw.Y));
        }

        public static Vector2 PadMoveStick()
        {
            return Deadzone(new Vector2(Axis(PadMoveX), Axis(PadMoveY)));
        }

        public static Vector2 AimStick()
        {
            return Deadzone(new Vector2(Axis(AimX), Axis(AimY)));
        }

        public static bool FireHeld()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyFire())
            {
                if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space))
                {
                    return true;
                }

                if (Input.GetButton(FirePad))
                {
                    return true;
                }
            }
            else if (BoundInput.Held(BindAction.Fire, Live) || BoundInput.Held(BindAction.FireAlt, Live))
            {
                return true;
            }

            return TriggerHeld(FireTrigger) || TriggerHeld(FireTrigger6);
        }

        public static bool UtilityHeld()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyUtility())
            {
                if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(1))
                {
                    return true;
                }
            }
            else if (BoundInput.Held(BindAction.Utility, Live))
            {
                return true;
            }

            if (TriggerHeld(UtilityTrigger))
            {
                return true;
            }

            if (Axis(FireTrigger3) <= -TriggerFire)
            {
                return true;
            }

            return TriggerHeld(FireTrigger3)
                && !TriggerHeld(FireTrigger)
                && !TriggerHeld(FireTrigger6);
        }

        public static bool CyclePressed()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyCycle())
            {
                return Input.GetKeyDown(KeyCode.Q)
                    || Input.GetKeyDown(KeyCode.JoystickButton4)
                    || ButtonDown(CycleFire);
            }

            return BoundInput.Down(BindAction.Cycle, Live) || BoundInput.Down(BindAction.CycleAlt, Live);
        }

        public static bool CyclePrevPressed()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyCyclePrev())
            {
                return Input.GetKeyDown(KeyCode.JoystickButton5);
            }

            return BoundInput.Down(BindAction.CyclePrev, Live);
        }

        public static bool PausePressed()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyPause())
            {
                return Input.GetKeyDown(KeyCode.Escape) || ButtonDown(Pause);
            }

            return BoundInput.Down(BindAction.Pause, Live);
        }

        public static bool ConfirmPressed()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyConfirm())
            {
                return Input.GetButtonDown("Submit") || ButtonDown(FirePad);
            }

            return BoundInput.Down(BindAction.Confirm, Live);
        }

        public static bool CancelPressed()
        {
            BindingMap map = BindingMap.ActiveOrDefault();
            if (map.UsesLegacyCancel())
            {
                return Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape);
            }

            return BoundInput.Down(BindAction.Cancel, Live);
        }

        public static Vector2 UiNavStick()
        {
            Vector2 stick = PadMoveStick();
            if (stick.sqrMagnitude >= StickDead * StickDead)
            {
                return stick;
            }

            return Deadzone(new Vector2(Axis(MoveX), Axis(MoveY)));
        }

        /// <summary>
        /// Hangar D-pad only. Not aliased onto Horizontal / move (that mapped RT
        /// into fly on some backends). Axes 7/8 + joystick buttons 11–14.
        /// </summary>
        public static Vector2 UiNavDpad()
        {
            float x = 0f;
            float y = 0f;
            if (ButtonHeld(DpadRight) || Input.GetKey(KeyCode.JoystickButton12))
            {
                x += 1f;
            }

            if (ButtonHeld(DpadLeft) || Input.GetKey(KeyCode.JoystickButton11))
            {
                x -= 1f;
            }

            if (ButtonHeld(DpadUp) || Input.GetKey(KeyCode.JoystickButton13))
            {
                y += 1f;
            }

            if (ButtonHeld(DpadDown) || Input.GetKey(KeyCode.JoystickButton14))
            {
                y -= 1f;
            }

            x += Axis(PadDpadX);
            y += Axis(PadDpadY);
            return Deadzone(new Vector2(x, y));
        }

        public static Vector2 UiNavCombined()
        {
            return UiNavCombined(SettingsState.MenuPadNav);
        }

        /// <summary>
        /// Menu highlight only. Pass <see cref="PadNavSource.Both"/> to ignore the
        /// saved filter (the settings panel does this so a bad choice cannot trap
        /// the player). DPad reads only PadDpad and the d-pad buttons. Analog reads
        /// the left stick (PadMove, then Horizontal/Vertical) and never the d-pad.
        /// Both keeps the previous mix, including the keyboard fallback.
        /// Gameplay fly input uses <see cref="MoveStick"/> and does not call this.
        /// </summary>
        public static Vector2 UiNavCombined(PadNavSource source)
        {
            Vector2 dpad = UiNavDpad();
            if (source == PadNavSource.DPad)
            {
                float onlyX;
                float onlyY;
                PadNavSourceRules.Select(PadNavSource.DPad, dpad.x, dpad.y, 0f, 0f, 0f, 0f, StickDead, out onlyX, out onlyY);
                return Deadzone(new Vector2(onlyX, onlyY));
            }

            Vector2 padStick = PadMoveStick();
            float pickedX;
            float pickedY;
            PadNavSourceRules.Select(
                source,
                dpad.x,
                dpad.y,
                padStick.x,
                padStick.y,
                Axis(MoveX),
                Axis(MoveY),
                StickDead,
                out pickedX,
                out pickedY);
            return Deadzone(new Vector2(pickedX, pickedY));
        }

        public static Vector2 MouseDelta()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        private static bool TriggerHeld(string name)
        {
            return Axis(name) >= TriggerFire;
        }

        private static float Axis(string name)
        {
            try
            {
                return Input.GetAxisRaw(name);
            }
            catch (System.ArgumentException)
            {
                return 0f;
            }
        }

        private static bool ButtonHeld(string name)
        {
            try
            {
                return Input.GetButton(name);
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }

        private static bool ButtonDown(string name)
        {
            try
            {
                return Input.GetButtonDown(name);
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }

        private static Vector2 Deadzone(Vector2 stick)
        {
            if (stick.sqrMagnitude < StickDead * StickDead)
            {
                return Vector2.zero;
            }

            if (stick.sqrMagnitude > 1f)
            {
                stick.Normalize();
            }

            return stick;
        }

        private sealed class LiveBindSource : IBindSource
        {
            public static readonly LiveBindSource Instance = new LiveBindSource();

            private LiveBindSource()
            {
            }

            public bool KeyHeld(int keyCode)
            {
                if (keyCode == (int)KeyCode.E)
                {
                    return Input.GetKey(KeyCode.E);
                }

                if (keyCode <= 0)
                {
                    return false;
                }

                return Input.GetKey((KeyCode)keyCode);
            }

            public bool KeyDown(int keyCode)
            {
                if (keyCode <= 0)
                {
                    return false;
                }

                return Input.GetKeyDown((KeyCode)keyCode);
            }

            public bool MouseHeld(int button)
            {
                if (button == 1)
                {
                    return Input.GetMouseButton(1);
                }

                if (button < 0 || button > 6)
                {
                    return false;
                }

                return Input.GetMouseButton(button);
            }

            public bool MouseDown(int button)
            {
                if (button < 0 || button > 6)
                {
                    return false;
                }

                return Input.GetMouseButtonDown(button);
            }

            public bool PadHeld(int button)
            {
                if (button < 0 || button > 19)
                {
                    return false;
                }

                return Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + button));
            }

            public bool PadDown(int button)
            {
                if (button == 4)
                {
                    return Input.GetKeyDown(KeyCode.JoystickButton4);
                }

                if (button == 5)
                {
                    return Input.GetKeyDown(KeyCode.JoystickButton5);
                }

                if (button < 0 || button > 19)
                {
                    return false;
                }

                return Input.GetKeyDown((KeyCode)((int)KeyCode.JoystickButton0 + button));
            }

            public float Axis(string name)
            {
                return GamepadInput.Axis(name);
            }

            public int NextKeyDown()
            {
                for (int code = 8; code <= 319; code++)
                {
                    if (code == (int)KeyCode.Escape)
                    {
                        continue;
                    }

                    if (Input.GetKeyDown((KeyCode)code))
                    {
                        return code;
                    }
                }

                return 0;
            }

            public int NextMouseDown()
            {
                for (int button = 0; button <= 6; button++)
                {
                    if (MouseDown(button))
                    {
                        return button;
                    }
                }

                return -1;
            }

            public int NextPadDown()
            {
                for (int button = 0; button <= 19; button++)
                {
                    if (PadDown(button))
                    {
                        return button;
                    }
                }

                return -1;
            }
        }
    }
}
