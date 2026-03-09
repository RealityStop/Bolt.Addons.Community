using System;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility {
    public static partial class HUMString
    {
        /// <summary>
        /// Structs for passing data down a string operation.
        /// </summary>
        public static partial class Data
        {
            /// <summary>
            /// String operation data for adding text.
            /// </summary>
            public struct Add
            {
                public string text;

                public Add(string text)
                {
                    this.text = text;
                }
            }

            /// <summary>
            /// String operation data for removing text.
            /// </summary>
            public struct Remove
            {
                public string text;
                public string remove;

                public Remove(string text, string remove)
                {
                    this.text = text;
                    this.remove = remove;
                }
            }

            /// <summary>
            /// String operation data for adding a space between a lowercase letter and something else.
            /// </summary>
            public struct SpaceBetweenLowercaseAnd
            {
                public SpaceBetweenLowercase lowercase;

                public SpaceBetweenLowercaseAnd(SpaceBetweenLowercase lowercase)
                {
                    this.lowercase = lowercase;
                }
            }

            /// <summary>
            /// String operation data for adding a space.
            /// </summary>
            public struct Space
            {
                public Add add;

                public Space(Add add)
                {
                    this.add = add;
                }
            }

            /// <summary>
            /// String operation data for adding a space between two strings with characteristics.
            /// </summary>
            public struct SpaceBetween
            {
                public Space space;

                public SpaceBetween(Space space)
                {
                    
                    this.space = space;
                }
            }

            /// <summary>
            /// String operation data for adding a space before something else occurs.
            /// </summary>
            public struct SpaceBefore
            {
                public Space space;

                public SpaceBefore(Space space)
                {
                    this.space = space;
                }
            }

            /// <summary>
            /// String operation data for adding text after something occurs.
            /// </summary>
            public struct After
            {

            }

            /// <summary>
            /// String operation data for capitalizing text.
            /// </summary>
            public struct Capitalize
            {
                public string text;

                public Capitalize(string text)
                {
                    this.text = text;
                }
            }

            /// <summary>
            /// String operation data for capitalizing the first of some text.
            /// </summary>
            public struct CapitalizeFirst
            {
                public Capitalize capitalize;

                public CapitalizeFirst(Capitalize capitalize)
                {
                    this.capitalize = capitalize;
                }
            }

            /// <summary>
            /// String operation data for adding a space between a lowercase letter and some other text/character.
            /// </summary>
            public struct SpaceBetweenLowercase
            {
                public SpaceBetween between;

                public SpaceBetweenLowercase(SpaceBetween between)
                {
                    this.between = between;
                }
            }

            /// <summary>
            /// String operation data for text conversion to something else.
            /// </summary>
            public struct As
            {
                public string text;

                public As(string text)
                {
                    this.text = text;
                }
            }
        }
    }
}
