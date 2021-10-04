using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    public static class Patcher
    {
        public static string LegalVariableName(string name, bool underscoreMiddleSpaces)
        {
            var output = name;
            output = RemoveStartingSpaces(output);
            output = RemoveIllegalVariableCharacters(output, underscoreMiddleSpaces);
            return output;
        }

        private static string RemoveStartingSpaces(string text)
        {
            var output = text;
            var length = output.Length;
            int finalCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (output[i].ToString() != " ")
                {
                    break;
                }

                finalCount++;
            }

            output = output.Remove(0, finalCount);

            return output;
        }

        private static string RemoveIllegalVariableCharacters(string name, bool underscoreSpaces)
        {
            var output = string.Empty;
            var length = name.Length;

            for (int i = 0; i < length; i++)
            {
                if (name[i].ToString() == null) break;
                if (char.IsLetter(name[i]))
                {
                    output += name[i].ToString();
                }
                else
                {
                    if (char.IsWhiteSpace(name[i]))
                    {
                        if (underscoreSpaces)
                            output += "_";
                    }
                }
            }

            return output;
        }

        public static string PeriodsToSlashes(this string member)
        {
            return string.IsNullOrEmpty(member) ? string.Empty : member.Replace(".", "/");
        }

        public static string SlashesToPeriods(this string member)
        {
            return string.IsNullOrEmpty(member) ? string.Empty : member.Replace("/", ".").Replace(@"\", ".");
        }

        public static string RemoveIllegalFirstLetters(this string text)
        {
            if (text.Length > 0)
            {
                var textReplacement = text[0].ToString().RemoveAllButLettersorDigitsExcept(new string[] { "_" });
                var newText = text.Remove(0, 1);
                newText = newText.Insert(0, textReplacement);
                return newText;
            }

            return text;
        }

        private static string[] _constructNames;
        public static string[] constructNames => _constructNames = _constructNames ?? new string[]
        {
            "object",
            "var",
            "func",
            "delegate",
            "class",
            "struct",
            "enum",
            "interface",
            "void",
            "float",
            "int",
            "string",
            "bool",
            "in",
            "out",
            "as",
            "is",
            "ref",
            "return",
            "public",
            "private",
            "protected",
            "internal",
            "static",
            "where",
            "from",
            "select"
        };

        public static string EnsureNonConstructName(this string text)
        {
            for(int i = 0; i < constructNames.Length; i++)
            {
                if (constructNames[i] == text) return "@" + text;
            }

            return text;
        }
    }
}