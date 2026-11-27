using System.Collections.Generic;

public static class TableChecks
{
    private const char HangulStart = '가';
    private const char HangulEnd = '힣';
    private const int FinalConsonantCount = 28;

    public static void CheckSequential(ICollection<int> numbers, int rowCount, string tag, string unitName)
    {
        if (!CheckNotEmpty(numbers.Count, rowCount, tag, unitName)) { return; }

        ReportGaps(numbers, tag, unitName);
    }

    public static bool CheckNotEmpty(int count, int rowCount, string tag, string unitName)
    {
        if (count > 0) { return true; }

        DebugUtility.LogError($"[{tag}] 사용할 수 있는 {unitName}{SubjectParticle(unitName)} 하나도 없습니다. (읽은 행 {rowCount}개)");
        return false;
    }

    private static void ReportGaps(ICollection<int> numbers, string tag, string unitName)
    {
        List<int> sorted = new List<int>(numbers);
        sorted.Sort();

        int last = sorted[sorted.Count - 1];
        int expected = 1;

        foreach (int number in sorted)
        {
            if (number > expected)
            {
                ReportGap(expected, number - 1, last, tag, unitName);
            }

            expected = number + 1;
        }
    }

    private static void ReportGap(int gapStart, int gapEnd, int last, string tag, string unitName)
    {
        string range = gapStart == gapEnd ? $"{gapStart}" : $"{gapStart}~{gapEnd}";

        DebugUtility.LogError($"[{tag}] {range}{unitName}{SubjectParticle(unitName)} 없습니다. ({last}{unitName}까지 있음)");
    }

    private static string SubjectParticle(string word)
    {
        if (string.IsNullOrEmpty(word)) { return "가"; }

        char last = word[word.Length - 1];
        
        if (last < HangulStart || last > HangulEnd) { return "가"; }

        bool hasFinalConsonant = (last - HangulStart) % FinalConsonantCount != 0;

        return hasFinalConsonant ? "이" : "가";
    }
}
