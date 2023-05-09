using PWB_CCLibrary.Controls;

namespace PWB_CCLibrary.Delegates;

public delegate void TabItemSelectedHandler( object sender, TabItemEventArgs e );
public delegate void TabItemUnselectedHandler( object sender, TabItemEventArgs e );

public class TabItemEventArgs {
    public PWB_TabItem? TabItem { get; set; }

    public TabItemEventArgs( PWB_TabItem? tabItem ) {
        TabItem = tabItem;
    }
}
