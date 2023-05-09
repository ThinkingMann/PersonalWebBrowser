using PWB_CCLibrary.Classes;
using PWB_CCLibrary.Controls;

namespace PWB_CCLibrary.Delegates;

public delegate void SelectedTabItemChangedEventHandler( object sender, SelectedTabItemChangedEventArgs e );

public class SelectedTabItemChangedEventArgs {
    public TabGroup? OldTabGroup { get; }
    public PWB_TabItem? OldTabItem { get; set; }
    public TabGroup? NewTabGroup { get; }
    public PWB_TabItem? NewTabItem { get; set; }

    public SelectedTabItemChangedEventArgs( TabGroup? oldTabGroup, PWB_TabItem? oldTabItem, TabGroup? newTabGroup, PWB_TabItem? newTabItem ) {
        OldTabGroup = oldTabGroup;
        OldTabItem = oldTabItem;
        NewTabGroup = newTabGroup;
        NewTabItem = newTabItem;
    }
}
