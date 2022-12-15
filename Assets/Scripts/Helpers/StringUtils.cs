public static class StringUtils {
    public static string FormatMultiple(int count, string word) {
        return count == 1 ? $"{count} {word}" : $"{count} {word}s";
    }

    public static string FormatMultiple(float number, string word) {
        return FormatMultiple((int) number, word);
    }
}