using System;
using System.Windows;

namespace TimeTray
{
    public partial class App : System.Windows.Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            // falls Timer läuft → automatisch stoppen & speichern
            TimeTracker.Instance.TryAutoStopOnExit();

            TrayService.Instance.Dispose();

            base.OnExit(e);
        }
    }
}
