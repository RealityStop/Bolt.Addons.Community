using Unity.VisualScripting.ReorderableList;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    public class ModernReorderableListControl : ReorderableListControl
    {
        protected override void AddItemsToMenu(GenericMenu menu, int itemIndex, IReorderableListAdaptor adaptor)
        {
            base.AddItemsToMenu(menu, itemIndex, adaptor);
            if (menu.GetItemCount() > 0)
            {
                menu.AddSeparator("");
            }

            menu.AddItem(CommandRemove, false, DefaultContextHandler, CommandRemove);
            menu.AddSeparator("");
            menu.AddItem(CommandClearAll, false, DefaultContextHandler, CommandClearAll);
        }
    }
}
