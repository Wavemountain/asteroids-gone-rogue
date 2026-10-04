namespace AsteroidsGoneRogue
{
    public struct BindVec2
    {
        public float X;
        public float Y;
    }

    /// <summary>
    /// The only input surface BoundInput reads. GamepadInput supplies the
    /// live legacy Input Manager. Tests supply a fake.
    /// </summary>
    public interface IBindSource
    {
        bool KeyHeld(int keyCode);
        bool KeyDown(int keyCode);
        bool MouseHeld(int button);
        bool MouseDown(int button);
        bool PadHeld(int button);
        bool PadDown(int button);
        float Axis(string name);
        int NextKeyDown();
        int NextMouseDown();
        int NextPadDown();
    }

    /// <summary>
    /// Held, Down, movement, and rebind capture through one facade.
    /// Default move reads Horizontal/Vertical so WASD and the arrows stay
    /// identical to the Input Manager axes. A rebound move uses the four
    /// stored keys plus the pad stick (the stick is not rebindable).
    /// </summary>
    public static class BoundInput
    {
        public static bool Held(int action, IBindSource source)
        {
            return Held(action, source, BindingMap.ActiveOrDefault());
        }

        public static bool Held(int action, IBindSource source, BindingMap map)
        {
            if (source == null || map == null)
            {
                return false;
            }

            if ((action == BindAction.Pause || action == BindAction.Cancel) && source.KeyHeld(BindCodes.Escape))
            {
                return true;
            }

            return map.IsHeld(action, source, false);
        }

        public static bool Down(int action, IBindSource source)
        {
            return Down(action, source, BindingMap.ActiveOrDefault());
        }

        public static bool Down(int action, IBindSource source, BindingMap map)
        {
            if (source == null || map == null)
            {
                return false;
            }

            if ((action == BindAction.Pause || action == BindAction.Cancel) && source.KeyDown(BindCodes.Escape))
            {
                return true;
            }

            return map.IsHeld(action, source, true);
        }

        public static BindVec2 MoveVector(IBindSource source)
        {
            return MoveVector(source, BindingMap.ActiveOrDefault());
        }

        public static BindVec2 MoveVector(IBindSource source, BindingMap map)
        {
            BindVec2 raw = new BindVec2();
            if (source == null)
            {
                return raw;
            }

            BindingMap used = map ?? BindingMap.ActiveOrDefault();
            if (used.MoveIsDefault())
            {
                raw.X = source.Axis("Horizontal");
                raw.Y = source.Axis("Vertical");
                return raw;
            }

            float x = 0f;
            float y = 0f;
            if (HeldKey(used.GetKey(BindAction.Move, 1), source))
            {
                x -= 1f;
            }

            if (HeldKey(used.GetKey(BindAction.Move, 3), source))
            {
                x += 1f;
            }

            if (HeldKey(used.GetKey(BindAction.Move, 2), source))
            {
                y -= 1f;
            }

            if (HeldKey(used.GetKey(BindAction.Move, 0), source))
            {
                y += 1f;
            }

            x += source.Axis("PadMoveX");
            y += source.Axis("PadMoveY");
            raw.X = ClampAxis(x);
            raw.Y = ClampAxis(y);
            return raw;
        }

        /// <summary>
        /// Next key, mouse button, or joystick button. Escape and the bound
        /// cancel button set abort and are not assignments.
        /// </summary>
        public static void FillCapture(IBindSource source, BindingMap map, out int kind, out int code, out bool abort)
        {
            kind = BindKind.None;
            code = 0;
            abort = false;
            if (source == null)
            {
                return;
            }

            if (source.KeyDown(BindCodes.Escape))
            {
                abort = true;
                kind = BindKind.Key;
                code = BindCodes.Escape;
                return;
            }

            int pad = source.NextPadDown();
            int cancelPad = -1;
            if (map != null && map.Pad0 != null && map.Pad0[BindAction.Cancel].Kind == BindKind.Pad)
            {
                cancelPad = map.Pad0[BindAction.Cancel].Code;
            }

            if (pad >= 0 && pad == cancelPad)
            {
                abort = true;
                kind = BindKind.Pad;
                code = pad;
                return;
            }

            int key = source.NextKeyDown();
            if (key > 0)
            {
                kind = BindKind.Key;
                code = key;
                return;
            }

            int mouse = source.NextMouseDown();
            if (mouse >= 0)
            {
                kind = BindKind.Mouse;
                code = mouse;
                return;
            }

            if (pad >= 0)
            {
                kind = BindKind.Pad;
                code = pad;
            }
        }

        private static bool HeldKey(BindControl control, IBindSource source)
        {
            if (control.Kind == BindKind.Key)
            {
                return source.KeyHeld(control.Code);
            }

            if (control.Kind == BindKind.Mouse)
            {
                return source.MouseHeld(control.Code);
            }

            return false;
        }

        private static float ClampAxis(float value)
        {
            if (value > 1f)
            {
                return 1f;
            }

            if (value < -1f)
            {
                return -1f;
            }

            return value;
        }
    }
}
