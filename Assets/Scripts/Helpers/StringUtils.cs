public static class StringUtils {
    public static string FormatMultiple(int count, string word) {
        return count == 1 ? $"{count} {word}" : $"{count} {word}s";
    }

    public static string FormatMultiple(float number, string word) {
        return FormatMultiple((int) number, word);
    }

    public static string Standing(int place) {
        return place switch {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            _ => $"{place}th"
        };
    }
}