using System;
using System.Text.RegularExpressions;

namespace Unity.VisualScripting.Community.Libraries.Humility {
    public static partial class HUMString_Children
    {
        /// <summary>
        /// Adds or changes text when a space occurs after a lowercase letter, and the next option.
        /// </summary>
        public static HUMString.Data.SpaceBetweenLowercaseAnd And(this HUMString.Data.SpaceBetweenLowercase lowercase)
        {
            return new HUMString.Data.SpaceBetweenLowercaseAnd(lowercase);
        }

        /// <summary>
        /// Adds or changes when a space is between text.
        /// </summary>
        public static HUMString.Data.SpaceBetween Between(this HUMString.Data.Space space)
        {
            return new HUMString.Data.SpaceBetween(space);
        }

        /// <summary>
        /// Adds or changes something when a space occurs before some text.
        /// </summary>
        public static HUMString.Data.SpaceBefore Before(this HUMString.Data.Space space)
        {
            return new HUMString.Data.SpaceBefore(space);
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Letter(this HUMString.Data.CapitalizeFirst capitalizeFirst)
        {
            return capitalizeFirst.capitalize.text[0].ToString().ToUpper() + capitalizeFirst.capitalize.text.Remove(0, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public static HUMString.Data.Space Space(this HUMString.Data.Add add)
        {
            return new HUMString.Data.Space(add);
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Text(this HUMString.Data.SpaceBefore before, int amount = 1)
        {
            var output = string.Empty;

            for (int i = 0; i < amount; i++)
            {
                output += " ";
            }

            return output + before.space.add.text;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HUMString.Data.SpaceBetweenLowercase Lowercase(this HUMString.Data.SpaceBetween between)
        {
            return new HUMString.Data.SpaceBetweenLowercase(between);
        }

        /// <summary>
        /// Capitalizes the first of some text or character.
        /// </summary>
        public static HUMString.Data.CapitalizeFirst First(this HUMString.Data.Capitalize capitalize)
        {
            return new HUMString.Data.CapitalizeFirst(capitalize);
        }

        /// <summary>
        /// Adds a space between the condition where a lowercase letter neighbors an upper case letter. Ex. 'aA' would be 'a A'
        /// </summary>
        public static string Uppercase(this HUMString.Data.SpaceBetweenLowercaseAnd and)
        {
            string last = string.Empty;
            string newString = string.Empty;
            var text = and.lowercase.between.space.add.text;

            for (int i = 0; i < text.Length; i++)
            {
                bool addedSpace = false;

                if (!string.IsNullOrEmpty(last))
                {
                    if (text[i].ToString() == text[i].ToString().ToUpper())
                    {
                        if (last == last.ToString().ToLower())
                        {
                            addedSpace = true;
                        }
                    }
                }

                if (addedSpace) newString += " ";

                newString += text[i];

                last = text[i].ToString();
            }
            return newString;
        }

        /// <summary>
        /// Adds a space between the condition where a lowercase letter neighbors an upper case letter. Ex. 'aA' would be 'a A'
        /// </summary>
        public static string Uppercase(this HUMString.Data.SpaceBetweenLowercaseAnd and, int fromIndex, int toIndex)
        {
            string last = string.Empty;
            string newString = string.Empty;
            var text = and.lowercase.between.space.add.text;

            for (int i = fromIndex; i < toIndex; i++)
            {
                bool addedSpace = false;

                if (!string.IsNullOrEmpty(last))
                {
                    if (text[i].ToString() == text[i].ToString().ToUpper())
                    {
                        if (last == last.ToString().ToLower())
                        {
                            addedSpace = true;
                        }
                    }
                }

                if (addedSpace) newString += " ";

                newString += text[i];

                last = text[i].ToString();
            }

            return newString;
        }

        /// <summary>
        /// Finds and removes the last occurence of any string.
        /// </summary>
        /// <param name="remove"></param>
        /// <param name="removedString"></param>
        /// <returns></returns>
        public static string Last(this HUMString.Data.Remove remove)
        {
            return remove.text.Remove(remove.text.LastIndexOf(remove.remove), remove.text.Length - 1);
        }
    }
}
