namespace Unity.VisualScripting.Community 
{
    public class GateLogic
    {
        private bool isInitial;
        private bool isOpen;
        public bool IsOpen(bool initialValue)
        {
            if (isInitial)
            {
                isOpen = initialValue;
                isInitial = false;
            }
            return isOpen;
        }
    
        public void Open()
        {
            isInitial = false;
            isOpen = true;
        }
        public void Close()
        {
            isInitial = false;
            isOpen = false;
        }
        public void Toggle()
        {
            isInitial = false;
            isOpen = !isOpen;
        }
    } 
}