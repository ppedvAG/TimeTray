using System;
using System.Globalization;
using System.IO;

namespace TimeTray
{

    public sealed class TimeTracker
    {
        public static TimeTracker Instance { get; } = new TimeTracker();

        private DateTime? _runningSince;

        private TimeTracker() { }

        public List<WeekRow> GetWeeklyTotals(int maxWeeks = 20)
        {
            // Key: (ISOYear, ISOWeek)
            var totals = new Dictionary<(int year, int week), TimeSpan>();

            // 1) aus Datei lesen
            if (File.Exists(DataFilePath))
            {
                foreach (var line in File.ReadLines(DataFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';');
                    if (parts.Length != 2) continue;

                    if (!DateTime.TryParse(parts[0], null, DateTimeStyles.RoundtripKind, out var s)) continue;
                    if (!DateTime.TryParse(parts[1], null, DateTimeStyles.RoundtripKind, out var e)) continue;
                    if (e <= s) continue;

                    AddIntervalByIsoWeeks(totals, s, e);
                }
            }

            // 2) laufendes Intervall bis jetzt dazurechnen
            if (IsRunning)
            {
                AddIntervalByIsoWeeks(totals, _runningSince!.Value, DateTime.Now);
            }

            // 3) in Rows umwandeln + sortieren (aktuelle Woche oben)
            var rows = totals
                .Select(kvp =>
                {
                    var (y, w) = kvp.Key;
                    var ts = kvp.Value;
                    return new WeekRow
                    {
                        Year = y,
                        Kw = w,
                        DurationText = FormatHhMm(ts),
                        SortKey = ((long)y * 100) + w
                    };
                })
                .OrderByDescending(r => r.SortKey)
                .Take(maxWeeks)
                .ToList();

            return rows;
        }

        private static string FormatHhMm(TimeSpan ts)
        {
            // angezeigt als HH:MM (mit Stunden > 24 möglich)
            var hours = (int)ts.TotalHours;
            return $"{hours:00}:{ts.Minutes:00}";
        }

        private static void AddIntervalByIsoWeeks(Dictionary<(int year, int week), TimeSpan> totals, DateTime start, DateTime end)
        {
            // Wir splitten das Intervall an Tagesgrenzen; das ist simpel und für Arbeitszeiten völlig ok.
            var cursor = start;
            while (cursor < end)
            {
                var next = cursor.Date.AddDays(1);
                if (next > end) next = end;

                var dayPart = next - cursor;

                var isoYear = ISOWeek.GetYear(cursor);
                var isoWeek = ISOWeek.GetWeekOfYear(cursor);

                var key = (isoYear, isoWeek);
                totals[key] = totals.TryGetValue(key, out var current) ? current + dayPart : dayPart;

                cursor = next;
            }
        }
        public bool IsRunning => _runningSince.HasValue;

        private static string DataFilePath
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TimeTray");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "zeiten.txt");
            }
        }

        public void Start()
        {
            if (IsRunning) return; // 1) ignorieren
            _runningSince = DateTime.Now;
        }

        public void Stop()
        {
            if (!IsRunning) return; // 2) ignorieren

            var start = _runningSince!.Value;
            var end = DateTime.Now;
            _runningSince = null;

            // Eine Zeile pro Intervall: start;end (ISO)
            File.AppendAllText(DataFilePath,
                $"{start:O};{end:O}{Environment.NewLine}");
        }

        public void TryAutoStopOnExit()
        {
            if (IsRunning) Stop();
        }

        public string GetStatusText()
        {
            if (!IsRunning) return "Gestoppt";
            return $"Läuft seit {_runningSince!.Value:dd.MM.yyyy HH:mm:ss}";
        }

        public TimeSpan GetCurrentIsoWeekSum()
        {
            var weekStart = GetIsoWeekStart(DateTime.Now);
            var weekEnd = weekStart.AddDays(7);

            var sum = TimeSpan.Zero;

            if (File.Exists(DataFilePath))
            {
                foreach (var line in File.ReadLines(DataFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';');
                    if (parts.Length != 2) continue;

                    if (!DateTime.TryParse(parts[0], null, DateTimeStyles.RoundtripKind, out var s)) continue;
                    if (!DateTime.TryParse(parts[1], null, DateTimeStyles.RoundtripKind, out var e)) continue;
                    if (e <= s) continue;

                    // Intervall auf aktuelle Woche schneiden
                    var a = s < weekStart ? weekStart : s;
                    var b = e > weekEnd ? weekEnd : e;
                    if (b > a) sum += (b - a);
                }
            }

            // falls gerade läuft: bis jetzt dazu
            if (IsRunning)
            {
                var s = _runningSince!.Value;
                var e = DateTime.Now;

                var a = s < weekStart ? weekStart : s;
                var b = e > weekEnd ? weekEnd : e;
                if (b > a) sum += (b - a);
            }

            return sum;
        }

        private static DateTime GetIsoWeekStart(DateTime dt)
        {
            // ISO: Woche startet Montag
            var date = dt.Date;
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.AddDays(-diff);
        }
    }
}
