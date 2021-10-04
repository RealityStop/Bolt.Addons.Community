namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(StuffHappens))]
    public class StuffHappensDescriptor : UnitDescriptor<StuffHappens>
    {
        public StuffHappensDescriptor(StuffHappens unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("weather_clouds", CommunityEditorPath.Fundamentals);
        }
    }
}