using PWB_CCLibrary.Classes;

namespace PWB_CCLibrary.Delegates;

public delegate void SelectedTabGroupChangedEventHandler( object sender, SelectedTabGroupChangedEventArgs e );

public class SelectedTabGroupChangedEventArgs {
    public TabGroup? OldTabGroup { get; }
    public TabGroup? NewTabGroup { get; }

    public SelectedTabGroupChangedEventArgs( TabGroup? oldTabGroup, TabGroup? newTabGroup ) {
        OldTabGroup = oldTabGroup;
        NewTabGroup = newTabGroup;
    }
}
