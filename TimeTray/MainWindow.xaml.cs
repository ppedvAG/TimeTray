using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TimeTray
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Tray + globaler Hotkey
            TrayService.Instance.Init(this);
            HotkeyService.Instance.Init(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initial UI/Icons setzen
            RefreshUi();
            UpdateTaskbarOverlay();
            TrayService.Instance.UpdateIcon();

            // Start: im Hintergrund (Tray)
            Hide();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // X versteckt nur (App läuft weiter)
            e.Cancel = true;
            Hide();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            TimeTracker.Instance.Start(); // ignoriert, wenn schon läuft
            AfterStateChange();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            TimeTracker.Instance.Stop(); // ignoriert, wenn nicht läuft
            AfterStateChange();
        }

        /// <summary>
        /// Wird von Hotkey/Tray aufgerufen: Fenster nach vorne + alles aktualisieren
        /// </summary>
        public void ShowAndRefresh()
        {
            AfterStateChange();

            if (!IsVisible)
                Show();

            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();

            // "nach vorne" Trick
            Topmost = true;
            Topmost = false;

            Focus();
        }

        private void AfterStateChange()
        {
            RefreshUi();
            TrayService.Instance.UpdateIcon();
            UpdateTaskbarOverlay();
        }

        private void RefreshUi()
        {
            // Falls du die tabellarische Variante nutzt:
            if (WeeksGrid != null)
                WeeksGrid.ItemsSource = TimeTracker.Instance.GetWeeklyTotals(maxWeeks: 30);

            // Buttons passend setzen
            if (StartBtn != null) StartBtn.IsEnabled = !TimeTracker.Instance.IsRunning;
            if (StopBtn != null) StopBtn.IsEnabled = TimeTracker.Instance.IsRunning;

            // Falls du zusätzlich noch StatusText o.ä. hast:
            // if (StatusText != null) StatusText.Text = TimeTracker.Instance.GetStatusText();
        }

        private void UpdateTaskbarOverlay()
        {
            // Overlay sieht man nur, wenn ein Taskbar-Button existiert.
            // Wenn Fenster komplett versteckt ist, gibt’s ggf. keinen Button.
            // Trotzdem setzen wir es immer, damit es stimmt sobald es sichtbar ist.

            if (TaskbarInfo == null) return;

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                var file = TimeTracker.Instance.IsRunning
                    ? Path.Combine(baseDir, "tray_running.ico")
                    : Path.Combine(baseDir, "tray_idle.ico");

                if (!File.Exists(file))
                {
                    TaskbarInfo.Overlay = null;
                    return;
                }

                // BitmapFrame kann ICO direkt lesen
                TaskbarInfo.Overlay = BitmapFrame.Create(new Uri(file, UriKind.Absolute));
            }
            catch
            {
                // Wenn irgendwas schiefgeht: Overlay weg, App soll weiterlaufen
                TaskbarInfo.Overlay = null;
            }
        }
    }
}
