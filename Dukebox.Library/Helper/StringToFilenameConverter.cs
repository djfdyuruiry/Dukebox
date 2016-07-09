using System.IO;
using System.Text.RegularExpressions;

namespace Dukebox.Library.Helper
{
    public static class StringToFilenameConverter
    {
        public static string ConvertStringToValidFileName(string input)
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars());
            var escapedInvalidChars = Regex.Escape(invalidChars);
            var invalidRegex = string.Format(@"([{0}]*\.+$)|([{0}]+)", escapedInvalidChars);

            return Regex.Replace(input, invalidRegex, "_");
        }
    }
}
