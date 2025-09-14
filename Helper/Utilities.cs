using System.Drawing;
// using OfficeOpenXml;
// using OfficeOpenXml.Style;
using System.Globalization;
using System.Text;

namespace Dashboard.Helper
{
    public static class Utilities
    {
        public static DateTime? ParseDate(string date)
        {
            return date == null ? null : DateTime.TryParse(date, out var d) ? d : null;
        }

        public static DateTime? ParseODate(string date)
        {
            try
            {
                long dateNum = long.Parse(date);
                DateTime result = DateTime.FromOADate(dateNum);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public static DateTime ParseDateTime(string dateValue)
        {
            if (dateValue != null)
            {
                var dateFormat = "dd/MM/yyyy";
                var parsedDate = ParseODate(dateValue) ?? DateTime.ParseExact(dateValue, dateFormat, CultureInfo.InvariantCulture);
                return parsedDate;
            }
            return DateTime.MinValue;
        }

        public static DateTime? ParseDateTimeCheckFormat(string dateValue)
        {
            try
            {// Các định dạng có thể gặp

                if (dateValue != null)
                {
                    var parsedDate = ParseODate(dateValue.Trim()) ?? ParseDate(dateValue.Trim());
                    return parsedDate;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static DateTime ParseDateTimeV1(string dateValue)
        {
            if (string.IsNullOrWhiteSpace(dateValue))
                return DateTime.MinValue;

            // Các định dạng có thể gặp
            var formats = new[]
            {
                "dd/MM/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy hh:mm:ss tt", // hỗ trợ AM/PM hoặc SA/CH
                "M/d/yyyy h:mm:ss tt",
                "yyyy-MM-ddTHH:mm:ss", // ISO format
                "dd-MM-yyyy",
            };

            // Cho phép văn hóa tiếng Việt để hiểu "SA", "CH"
            var culture = new CultureInfo("en-US");

            // Normalize SA/CH nếu cần chuyển thủ công
            dateValue = dateValue.Replace(" SA", " AM").Replace(" CH", " PM");

            var parsedDate = ParseODate(dateValue) ?? DateTime.ParseExact(dateValue, formats, CultureInfo.InvariantCulture);

            return parsedDate;
        }

        public static T GetRandomItem<T>(List<T> list)
        {
            Random random = new Random();
            int index = random.Next(list.Count); // Get a random index
            return list[index]; // Return the item at the random index
        }

        /// <summary>
        /// Replace At
        /// </summary>
        /// <param name="str">the source string</param>
        /// <param name="index">the start location to replace at (0-based)</param>
        /// <param name="length">the number of characters to be removed before inserting</param>
        /// <param name="replace">the string that is replacing characters</param>
        /// <returns></returns>
        public static string ReplaceAt(string str, int index, int length, string replace)
        {
            return str.Remove(index, Math.Min(length, str.Length - index))
                    .Insert(index, replace);
        }
        /// <summary>
        /// Draw Border for Excel File when Export
        /// </summary>
        

        public static string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);
            return filename;
        }

        public record FileServerConfig
        {
            public string? Path { set; get; }
        }

        public static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string[] words = str.Split(new char[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (i == 0)
                    words[i] = words[i].ToLower(); // First word should be lowercase
                else
                    words[i] = textInfo.ToTitleCase(words[i].ToLower()); // Capitalize subsequent words
            }

            return string.Join(string.Empty, words);
        }

        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Tách từ dựa trên khoảng trắng, gạch dưới, gạch ngang
            var words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (word.Length > 0)
                {
                    result.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                        result.Append(word.Substring(1).ToLower());
                }
            }

            return result.ToString();
        }


        public static string ConvertToVNĐ(double number)
        {
            if (number < 0)
            {
                return "Âm " + ConvertToVNĐ(-number);
            }
            if (number == 0)
            {
                return "Không";
            }

            string[] units = { "", "nghìn", "triệu", "tỷ" };
            string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

            string result = "";
            int unitIndex = 0;

            while (number > 0)
            {
                int part = (int)(number % 1000);
                if (part > 0)
                {
                    string partInWords = ConvertPartToWords(part, digits);
                    result = partInWords + " " + units[unitIndex] + " " + result;
                }
                number /= 1000;
                unitIndex++;
            }

            return result.Trim() + " VNĐ";
        }

        private static string ConvertPartToWords(int part, string[] digits)
        {
            string result = "";
            int hundreds = part / 100;
            int tens = (part % 100) / 10;
            int ones = part % 10;

            if (hundreds > 0)
            {
                result += digits[hundreds] + " trăm ";
            }

            if (tens > 1)
            {
                result += digits[tens] + " mươi ";
                if (ones == 1)
                {
                    result += "một ";
                }
            }
            else if (tens == 1)
            {
                result += "mười ";
            }

            if (ones > 0)
            {
                result += digits[ones] + " ";
            }

            return result.Trim();
        }

    }
}
