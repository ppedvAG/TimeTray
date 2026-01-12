namespace TimeTray
{
    public sealed class WeekRow
    {
        public int Year { get; init; }
        public int Kw { get; init; }              // ISO Kalenderwoche
        public string DurationText { get; init; } = "00:00";
        public long SortKey { get; init; }        // zum Sortieren (Jahr+KW)
    }
}
