using PWB_CCLibrary.Controls;

namespace PWB_CCLibrary.Delegates;

internal delegate void CloseRequestedEventHandler( object sender, CloseRequestedEventArgs e );

internal class CloseRequestedEventArgs {
    public TabGroupButton Source;

    public CloseRequestedEventArgs( TabGroupButton source ) {
        Source = source;
    }
}
