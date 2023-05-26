using System.Collections.ObjectModel;

using PWB_CCLibrary.Delegates;

namespace PWB_CCLibrary.Classes {
    public interface ITabber {
        event SelectedTabItemChangedEventHandler? SelectionChanged;
        event SelectedTabGroupChangedEventHandler? SelectedGroupChanged;

        ObservableCollection<TabGroup> TabGroups { get; }
        TabGroup? SelectedTabGroup { get; }

        void SetSelectedTabGroup( TabGroup tabGroup );

        TabGroup AddNewTabGroup();

        void RemoveTabGroup( TabGroup tabGroup, TabGroup? newTabGroupToSelect );
    }
}
