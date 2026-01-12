using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace TimeTray
{
    public sealed class TrayService : IDisposable
    {
        public static TrayService Instance { get; } = new TrayService();
        private NotifyIcon? _icon;
        private MainWindow? _window;
        private Icon _idleIcon;
        private Icon _runningIcon;

        private TrayService() { }

        public void Init(MainWindow window)
        {
            if (_icon != null) return;

            _window = window;

            _icon = new NotifyIcon
            {
                Visible = true,
                Text = "TimeTray (Alt+T)",
                Icon = SystemIcons.Application, // minimal: Standard-Icon
                ContextMenuStrip = BuildMenu()
            };
            _idleIcon = new Icon("tray_idle.ico");
            _runningIcon = new Icon("tray_running.ico");

            UpdateIcon();


            _icon.DoubleClick += (_, __) => _window?.ShowAndRefresh();
        }
        public void UpdateIcon()
        {
            if (_icon == null) return;

            _icon.Icon = TimeTracker.Instance.IsRunning
                ? _runningIcon
                : _idleIcon;
            UpdateTooltip();

        }
        public void UpdateTooltip()
        {
            if (_icon == null) return;

            if (TimeTracker.Instance.IsRunning)
                _icon.Text = "Time läuft";
            else
                _icon.Text = "Time gestoppt";
        }


        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();

            var show = new ToolStripMenuItem("Öffnen");
            show.Click += (_, __) => _window?.ShowAndRefresh();

            var start = new ToolStripMenuItem("Start");
            start.Click += (_, __) => { TimeTracker.Instance.Start(); _window?.ShowAndRefresh(); };

            var stop = new ToolStripMenuItem("Stop");
            stop.Click += (_, __) => { TimeTracker.Instance.Stop(); _window?.ShowAndRefresh(); };

            var exit = new ToolStripMenuItem("Beenden");
            exit.Click += (_, __) =>
            {
                TimeTracker.Instance.TryAutoStopOnExit();
                _icon!.Visible = false;
                System.Windows.Application.Current.Shutdown();
            };

            menu.Items.Add(show);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(start);
            menu.Items.Add(stop);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exit);

            return menu;
        }

        public void Dispose()
        {
            if (_icon != null)
            {
                _icon.Visible = false;
                _icon.Dispose();
                _icon = null;
            }
        }
    }
}
