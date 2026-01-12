using System;
using System.ComponentModel;
using System.Windows;

namespace TimeTray
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Tray + Hotkey beim Start initialisieren
            TrayService.Instance.Init(this);
            HotkeyService.Instance.Init(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUi();
            // direkt beim Start ins Tray minimieren (optional)
            this.Hide();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            TimeTracker.Instance.Start(); // wenn schon läuft -> ignoriert
            TimeTracker.Instance.Start();
            TrayService.Instance.UpdateIcon();
            RefreshUi();

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            TimeTracker.Instance.Stop(); // wenn nicht läuft -> ignoriert
            TimeTracker.Instance.Start();
            TrayService.Instance.UpdateIcon();
            RefreshUi();

        }

        public void ShowAndRefresh()
        {
            RefreshUi();
            if (!IsVisible) Show();
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate();
            Topmost = true;  // kleiner Trick um sicher nach vorne zu kommen
            Topmost = false;
            Focus();
        }

        private void RefreshUi()
        {
            WeeksGrid.ItemsSource = TimeTracker.Instance.GetWeeklyTotals(maxWeeks: 30);

            StartBtn.IsEnabled = !TimeTracker.Instance.IsRunning;
            StopBtn.IsEnabled = TimeTracker.Instance.IsRunning;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // X versteckt nur
            e.Cancel = true;
            Hide();
        }
    }
}
