namespace Unity.VisualScripting.Community
{
    public abstract class GlobalProcess
    {
        public abstract void Process();
        public abstract void OnBind();
        public abstract void OnUnbind();
        public abstract void OnInitialize();
    }
}