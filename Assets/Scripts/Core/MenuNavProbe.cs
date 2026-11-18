using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One-frame nav injection for the editor start-choice test.
    /// Unarmed, every read is the real Input manager.
    /// </summary>
    internal static class MenuNavProbe
    {
        private static bool _pending;
        private static int _latched = -2;
        private static int[] _keys = new int[8];
        private static int _keyCount;
        private static float _dpadX;
        private static bool _submit;
        private static bool _cancel;

        internal static void Clear()
        {
            _pending = false;
            _latched = -2;
            _keyCount = 0;
            _dpadX = 0f;
            _submit = false;
            _cancel = false;
        }

        internal static void Pulse(int[] keyCodes, int keyCount, float dpadX, bool submit, bool cancel)
        {
            _pending = true;
            _latched = -2;
            _keyCount = 0;
            _dpadX = dpadX;
            _submit = submit;
            _cancel = cancel;
            if (keyCodes == null || keyCount < 1)
            {
                return;
            }

            int count = keyCount;
            if (count > _keys.Length)
            {
                count = _keys.Length;
            }

            for (int index = 0; index < count; index++)
            {
                _keys[index] = keyCodes[index];
            }

            _keyCount = count;
        }

        internal static bool KeyDown(KeyCode key)
        {
            bool down;
            if (!TryKey(key, out down))
            {
                return Input.GetKeyDown(key);
            }

            return down;
        }

        internal static bool TryKey(KeyCode key, out bool down)
        {
            down = false;
            if (!Active())
            {
                return false;
            }

            int code = (int)key;
            for (int index = 0; index < _keyCount; index++)
            {
                if (_keys[index] == code)
                {
                    down = true;
                    return true;
                }
            }

            return true;
        }

        internal static bool TryAxis(string name, out float value)
        {
            value = 0f;
            if (!Active())
            {
                return false;
            }

            if (name == GamepadInput.PadDpadX)
            {
                value = _dpadX;
            }

            return true;
        }

        internal static bool TryButton(string name, bool downEdge, out bool value)
        {
            value = false;
            if (!Active())
            {
                return false;
            }

            if (name == GamepadInput.DpadRight)
            {
                value = _dpadX > 0.5f;
            }
            else if (name == GamepadInput.DpadLeft)
            {
                value = _dpadX < -0.5f;
            }
            else if (name == GamepadInput.FirePad || name == "Submit")
            {
                value = _submit;
            }
            else if (name == "Cancel")
            {
                value = _cancel;
            }

            if (downEdge)
            {
                return true;
            }

            return true;
        }

        internal static bool TryConfirm(out bool pressed)
        {
            pressed = false;
            if (!Active())
            {
                return false;
            }

            pressed = _submit;
            return true;
        }

        internal static bool TryCancel(out bool pressed)
        {
            pressed = false;
            if (!Active())
            {
                return false;
            }

            pressed = _cancel;
            return true;
        }

        private static bool Active()
        {
            if (!_pending)
            {
                return false;
            }

            int frame = Time.frameCount;
            if (_latched < 0)
            {
                _latched = frame;
            }

            if (frame != _latched)
            {
                _pending = false;
                _latched = -2;
                return false;
            }

            return true;
        }
    }
}
