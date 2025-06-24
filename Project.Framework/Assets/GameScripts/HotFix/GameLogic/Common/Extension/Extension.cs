namespace GameLogic
{
    public static class Extension
    {
        public static string ToFirstCharLower(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string GetLastPathSegment(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            int lastSlash = str.LastIndexOf('/');
            if (lastSlash < 0)
                return str;
            if (lastSlash == str.Length - 1)
                return ""; // 末尾是/，无内容

            return str.Substring(lastSlash + 1);
        }
    }
}