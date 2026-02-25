namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMIO
    {
        public static partial class Data
        {
            public struct Save
            {
                public object value;

                public Save(object value)
                {
                    this.value = value;
                }
            }

            public struct Load { }
            public struct Delete { }
            public struct Copy { }
            public struct Asset { }
            public struct Remove
            {
                public string path;
                
                public Remove(string path)
                {
                    this.path = path;
                }
            }
            public struct Persistant
            {
                public string path;
                public Save save;
                public Load load;

                public Persistant(string path, Save saveData, Load loadData)
                {
                    this.path = path;
                    save = saveData;
                    load = loadData;
                }
            }

            public struct Custom
            {
                public string path;
                public Save save;
                public Load load;

                public Custom(string path, Save saveData, Load loadData)
                {
                    this.path = path;
                    save = saveData;
                    load = loadData;
                }
            }

            public struct And { }

            public struct Ensure
            {
                public string path;

                public Ensure(string path)
                {
                    this.path = path;
                }
            }

            public struct Legal { }
        }
    }
}
