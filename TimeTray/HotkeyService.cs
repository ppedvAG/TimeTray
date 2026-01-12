using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TimeTray
{
    public sealed class HotkeyService
    {
        public static HotkeyService Instance { get; } = new HotkeyService();

        private const int HOTKEY_ID = 1;
        private const uint MOD_ALT = 0x0001;
        private const int WM_HOTKEY = 0x0312;

        private HwndSource? _source;
        private MainWindow? _window;

        private HotkeyService() { }

        public void Init(MainWindow window)
        {
            _window = window;

            var helper = new WindowInteropHelper(window);
            window.SourceInitialized += (_, __) =>
            {
                _source = HwndSource.FromHwnd(helper.Handle);
                _source.AddHook(HwndHook);

                // ALT+T registrieren
                RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT, (uint)KeyToVk('T'));
            };

            window.Closed += (_, __) =>
            {
                if (_source != null)
                {
                    _source.RemoveHook(HwndHook);
                    _source = null;
                }
                if (helper.Handle != IntPtr.Zero)
                {
                    UnregisterHotKey(helper.Handle, HOTKEY_ID);
                }
            };
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _window?.ShowAndRefresh();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private static int KeyToVk(char c) => char.ToUpperInvariant(c);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
