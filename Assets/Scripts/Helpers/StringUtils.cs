public static class StringUtils {
    public static string Format(int count, string word) {
        return count == 1 ? $"{count} {word}" : $"{count} {word}s";
    }

    public static string Format(float count, string word) {
        return Format((int) count, word);
    }
}