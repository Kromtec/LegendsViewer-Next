using System.Drawing;
using System.Text;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Various;

namespace LegendsViewer.Backend.Utilities;

public static class Formatting
{
    public static string GetInitials(string name)
    {
        ReadOnlySpan<char> nameSpan = name.AsSpan();
        Span<char> initials = stackalloc char[10]; // Allocating space for initials on the stack
        int count = 0;

        string[] fillers = { "the", "of", "and", "a", "an", "in" }; // Filler words to exclude

        int i = 0;
        while (i < nameSpan.Length && count < initials.Length)
        {
            // Skip leading whitespace
            while (i < nameSpan.Length && char.IsWhiteSpace(nameSpan[i]))
                i++;

            // Identify the start of a word
            int start = i;
            while (i < nameSpan.Length && !char.IsWhiteSpace(nameSpan[i]))
                i++;

            // Extract the word as a span and check if it's a filler
            ReadOnlySpan<char> word = nameSpan.Slice(start, i - start);
            if (word.Length > 0 && !IsFillerWord(word, fillers))
            {
                initials[count++] = char.ToUpperInvariant(word[0]);
            }
        }

        return new string(initials[..count]);
    }

    // Function to check if a word is a filler word
    private static bool IsFillerWord(ReadOnlySpan<char> word, string[] fillers)
    {
        foreach (var filler in fillers)
        {
            if (word.Equals(filler, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public static string InitCaps(ReadOnlySpan<char> text)
    {
        // Create a span for the result with the same length as the input text
        Span<char> newText = stackalloc char[text.Length];

        // Track the previous character to determine word boundaries
        bool isNewWord = true;

        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];
            if (i == 0)
            {
                newText[i] = char.ToUpper(currentChar);
                isNewWord = false;
                continue;
            }
            // Check if we are starting a new word
            if (isNewWord)
            {
                if (!(currentChar == 't' && i + 2 < text.Length && (text[i + 1] == 'h' || text[i + 1] == 'H') && (text[i + 2] == 'e' || text[i + 2] == 'E') && (i + 3 >= text.Length || (text[i + 3] == ' ' || text[i + 3] == '-')))
                    && !(currentChar == 'o' && i + 1 < text.Length && (text[i + 1] == 'f' || text[i + 1] == 'F') && (i + 2 >= text.Length || (text[i + 2] == ' ' || text[i + 2] == '-'))))
                {
                    newText[i] = char.ToUpper(currentChar);
                }
                else
                {
                    newText[i] = currentChar;
                }
            }
            else
            {
                // Convert underscores to spaces and handle regular lowercasing
                newText[i] = currentChar == '_' ? ' ' : char.ToLower(currentChar);
            }

            // Determine if the next character should start a new word
            isNewWord = newText[i] == ' ' || newText[i] == '-';
        }

        // Return the result as a string
        return new string(newText);
    }

    public static string ToUpperFirstLetter(this string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }
        // convert to char array of the string
        char[] letters = source.ToCharArray();
        // upper case the first char
        letters[0] = char.ToUpper(letters[0]);
        // return the array made of the new char array
        return new string(letters);
    }

    public static string MakePopulationPlural(string population)
    {
        string ending = "";

        if (population.Contains(" of"))
        {
            ending = population.Substring(population.IndexOf(" of"), population.Length - population.IndexOf(" of"));
            population = population.Substring(0, population.IndexOf(" of"));
        }

        if (population.EndsWith("Men") || population.EndsWith("men") || population == "Humans")
        {
            return population + ending;
        }

        if (population.EndsWith('s') && !population.EndsWith("ss"))
        {
            return population + ending;
        }

        if (population == "Human")
        {
            population = "Humans";
        }
        else if (population.EndsWith("Man"))
        {
            population = population.Replace("Man", "Men");
        }
        else if (population.EndsWith("man") && !population.Contains("Human"))
        {
            population = population.Replace("man", "men");
        }
        else if (population.EndsWith("Woman"))
        {
            population = population.Replace("Woman", "Women");
        }
        else if (population.EndsWith("woman"))
        {
            population = population.Replace("woman", "women");
        }
        else if (population.EndsWith("f"))
        {
            population = population.Substring(0, population.Length - 1) + "ves";
        }
        else if (population.EndsWith("x") || population.EndsWith("ch") || population.EndsWith("sh") || population.EndsWith("s"))
        {
            population += "es";
        }
        else if (population.EndsWith("y") && !population.EndsWith("ay") && !population.EndsWith("ey") && !population.EndsWith("iy") && !population.EndsWith("oy") && !population.EndsWith("uy"))
        {
            population = population.Substring(0, population.Length - 1) + "ies";
        }
        else if (!population.EndsWith("i") && !population.EndsWith("le"))
        {
            population += "s";
        }

        if (ending != "")
        {
            population += ending;
        }

        return population;
    }

    public static string FormatRace(string race)
    {
        if (string.IsNullOrWhiteSpace(race))
        {
            return string.Empty;
        }

        if (race.Contains("FORGOTTEN", StringComparison.OrdinalIgnoreCase))
        {
            return "Forgotten Beast";
        }

        if (race.StartsWith("HFEXP", StringComparison.OrdinalIgnoreCase))
        {
            if (race.Contains("E_HUM", StringComparison.OrdinalIgnoreCase))
            {
                return "Human";
            }
            if (race.Contains("E_BEAST", StringComparison.OrdinalIgnoreCase))
            {
                return "Beast";
            }
            if (race.Contains("E_FS", StringComparison.OrdinalIgnoreCase) || race.Contains("E_FL", StringComparison.OrdinalIgnoreCase))
            {
                return "Local Defender";
            }
        }

        return InitCaps(race);
    }

    public static string RemoveSpecialCharacters(string str)
    {
        StringBuilder sb = new();
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] >= '0' && str[i] <= '9' || str[i] >= 'A' && str[i] <= 'z' || str[i] == '.' || str[i] == '_')
            {
                sb.Append(str[i]);
            }
        }

        return sb.ToString();
    }

    public static string AddArticle(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }
        return text[0] == 'a' || text[0] == 'A' ||
            text[0] == 'e' || text[0] == 'E' ||
            text[0] == 'i' || text[0] == 'I' ||
            text[0] == 'o' || text[0] == 'O' ||
            text[0] == 'u' || text[0] == 'U'
            ? $"an {text}"
            : $"a {text}";
    }

    public static string ReplaceNonAscii(string name)
    {
        name = name.Replace("\u017D", "a")
            .Replace("\u017E", "a")
            .Replace("\u201E", "a")
            .Replace("\u0192", "a")
            .Replace("\u008F", "a")
            .Replace("\u2020", "a")
            .Replace("\u00A0", "a")
            .Replace("\u2026", "a")
            .Replace("\u02C6", "e")
            .Replace("\u2030", "e")
            .Replace("\u201A", "e")
            .Replace("\u0220", "e")
            .Replace("\u0160", "e")
            .Replace("\u0090", "e")
            .Replace("\u2039", "i")
            .Replace("\u00A1", "i")
            .Replace("\u008D", "i")
            .Replace("\u0152", "i")
            .Replace("\u00A4", "n")
            .Replace("\u00A5", "n")
            .Replace("\u201D", "o")
            .Replace("\u00A2", "o")
            .Replace("\u2022", "o")
            .Replace("\u201C", "o")
            .Replace("\u2122", "o")
            .Replace("\u2014", "u")
            .Replace("\u2013", "u")
            .Replace("\u00A3", "u")
            .Replace("\u02DC", "y");
        return name;
    }

    public static Location ConvertToLocation(string coordinates, IWorld world)
    {
        var indexOfComma = coordinates.IndexOf(',');
        int x = int.Parse(coordinates.Substring(0, indexOfComma));
        int y = int.Parse(coordinates.Substring(indexOfComma + 1, coordinates.Length - indexOfComma - 1));
        if (x > world.Width)
        {
            world.Width = x;
        }
        if (y > world.Height)
        {
            world.Height = y;
        }
        return new Location(x, y);
    }

    // After processing all coordinates adjust the world size
    public static void AdjustWorldSizeToPowerOfTwo(IWorld world)
    {
        world.Width = NextPowerOfTwo(world.Width) + 1;
        world.Height = NextPowerOfTwo(world.Height) + 1;
    }

    // Helper function to find the next power of two
    private static int NextPowerOfTwo(int n)
    {
        if (n <= 0) return 1;
        n--;
        n |= n >> 1;
        n |= n >> 2;
        n |= n >> 4;
        n |= n >> 8;
        n |= n >> 16;
        return n + 1;
    }

    public static Color HsvToColor(double h, double s, double v)
    {
        int red, green, blue;
        HsvToRgb(h, s, v, out red, out green, out blue);
        return Color.FromArgb(red, green, blue);
    }

    public static void HsvToRgb(double hue, double saturation, double value, out int red, out int green, out int blue)
    {
        if (saturation == 0)
        {
            red = (int)(value * 255);
            green = (int)(value * 255);
            blue = (int)(value * 255);
            return;
        }

        hue /= 60;
        int i = (int)Math.Floor(hue);
        double f = hue - i;
        double p = value * (1 - saturation);
        double q = value * (1 - saturation * f);
        double t = value * (1 - saturation * (1 - f));

        switch (i)
        {
            case 0:
                red = (int)(value * 255);
                green = (int)(t * 255);
                blue = (int)(p * 255);
                break;
            case 1:
                red = (int)(q * 255);
                green = (int)(value * 255);
                blue = (int)(p * 255);
                break;
            case 2:
                red = (int)(p * 255);
                green = (int)(value * 255);
                blue = (int)(t * 255);
                break;
            case 3:
                red = (int)(p * 255);
                green = (int)(q * 255);
                blue = (int)(value * 255);
                break;
            case 4:
                red = (int)(t * 255);
                green = (int)(p * 255);
                blue = (int)(value * 255);
                break;
            default:
                red = (int)(value * 255);
                green = (int)(p * 255);
                blue = (int)(q * 255);
                break;
        }
    }

    public static string GetReadableForegroundColor(Color bgColor)
    {
        // Calculate the relative luminance of the background color
        double luminance = ((0.299 * bgColor.R) + (0.587 * bgColor.G) + (0.114 * bgColor.B)) / 255;

        // If luminance is greater than 0.5, use black as the foreground color; otherwise, use white
        return luminance > 0.5 ? "#000000" : "#FFFFFF";
    }

    public static string YearPlusSeconds72ToTimestamp(int year, int seconds72)
    {
        var month = 1 + seconds72 / (28 * 1200);
        var day = 1 + seconds72 % (28 * 1200) / 1200;
        return year < 0 ? "In a time before time" : $"{year:0000}-{month:00}-{day:00}";
    }

    private static readonly string[] MonthNames = { "Granite", "Slate", "Felsite", "Hematite", "Malachite", "Galena", "Limestone", "Sandstone", "Timber", "Moonstone", "Opal", "Obsidian" };
    public static string YearPlusSeconds72ToProsa(int year, int seconds72)
    {
        var month = 1 + seconds72 / (28 * 1200);
        var day = 1 + seconds72 % (28 * 1200) / 1200;
        if (year == -1)
        {
            return "In a time before time, ";
        }

        string yearTime = $"In {year}, ";
        if (seconds72 == -1)
        {
            return yearTime;
        }

        int partOfMonth = seconds72 % 100800;
        string partOfMonthString = "";
        if (partOfMonth <= 33600)
        {
            partOfMonthString = "early ";
        }
        else if (partOfMonth <= 67200)
        {
            partOfMonthString = "mid";
        }
        else if (partOfMonth <= 100800)
        {
            partOfMonthString = "late ";
        }

        int season = seconds72 % 403200;
        string seasonString = "";
        if (season < 100800)
        {
            seasonString = "spring, ";
        }
        else if (season < 201600)
        {
            seasonString = "summer, ";
        }
        else if (season < 302400)
        {
            seasonString = "autumn, ";
        }
        else if (season < 403200)
        {
            seasonString = "winter, ";
        }

        string ordinal = "";
        int num = day;
        if (num > 0)
        {
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    ordinal = "th";
                    break;
            }
            if (ordinal?.Length == 0)
            {
                switch (num % 10)
                {
                    case 1:
                        ordinal = "st";
                        break;
                    case 2:
                        ordinal = "nd";
                        break;
                    case 3:
                        ordinal = "rd";
                        break;
                    default:
                        ordinal = "th";
                        break;
                }
            }
        }
        var monthName = MonthNames[month - 1];
        return $"{yearTime}{partOfMonthString}{seasonString} ({day}{ordinal} of {monthName}) ";
    }

    public static string TimeCountToSeason(int count)
    {
        string seasonString = string.Empty;
        int month = count % 100800;
        if (month <= 33600)
        {
            seasonString += "early ";
        }
        else if (month <= 67200)
        {
            seasonString += "mid";
        }
        else if (month <= 100800)
        {
            seasonString += "late ";
        }

        int season = count % 403200;
        if (season < 100800)
        {
            seasonString += "spring";
        }
        else if (season < 201600)
        {
            seasonString += "summer";
        }
        else if (season < 302400)
        {
            seasonString += "autumn";
        }
        else if (season < 403200)
        {
            seasonString += "winter";
        }

        return seasonString;
    }

    public static string AddOrdinal(int num)
    {
        var numString = num.ToString();
        if (num <= 0)
        {
            return numString;
        }

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return $"{numString}th";
        }

        switch (num % 10)
        {
            case 1:
                return $"{numString}st";
            case 2:
                return $"{numString}nd";
            case 3:
                return $"{numString}rd";
            default:
                return $"{numString}th";
        }
    }

    public static string IntegerToWords(long inputNum)
    {
        int level = 0;

        string retval = "";
        string[] ones ={
            "zero",
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine",
            "ten",
            "eleven",
            "twelve",
            "thirteen",
            "fourteen",
            "fifteen",
            "sixteen",
            "seventeen",
            "eighteen",
            "nineteen"
          };
        string[] tens ={
            "zero",
            "ten",
            "twenty",
            "thirty",
            "forty",
            "fifty",
            "sixty",
            "seventy",
            "eighty",
            "ninety"
          };
        string[] thou ={
            "",
            "thousand",
            "million",
            "billion",
            "trillion",
            "quadrillion",
            "quintillion"
          };

        bool isNegative = false;
        if (inputNum < 0)
        {
            isNegative = true;
            inputNum *= -1;
        }

        if (inputNum == 0)
        {
            return "zero";
        }

        string s = inputNum.ToString();

        while (s.Length > 0)
        {
            // Get the three rightmost characters
            var x = s.Length < 3 ? s : s.Substring(s.Length - 3, 3);

            // Separate the three digits
            var threeDigits = int.Parse(x);
            var lasttwo = threeDigits % 100;
            var dig1 = threeDigits / 100;
            var dig2 = lasttwo / 10;
            var dig3 = threeDigits % 10;

            // append a "thousand" where appropriate
            if (level > 0 && dig1 + dig2 + dig3 > 0)
            {
                retval = $"{thou[level]} {retval}";
                retval = retval.Trim();
            }

            // check that the last two digits is not a zero
            if (lasttwo > 0)
            {
                retval = lasttwo < 20 ? $"{ones[lasttwo]} {retval}" : $"{tens[dig2]} {ones[dig3]} {retval}";
            }

            // if a hundreds part is there, translate it
            if (dig1 > 0)
            {
                retval = ones[dig1] + " hundred " + retval;
            }

            s = s.Length - 3 > 0 ? s.Substring(0, s.Length - 3) : "";
            level++;
        }

        while (retval.IndexOf("  ", StringComparison.Ordinal) > 0)
        {
            retval = retval.Replace("  ", " ");
        }

        retval = retval.Trim();

        if (isNegative)
        {
            retval = "negative " + retval;
        }

        return retval;
    }

    public static double ScaleValue(double value, double minValue, double maxValue, double minSize, double maxSize)
    {
        return minSize + ((value - minValue) * (maxSize - minSize) / (maxValue - minValue));
    }
}
