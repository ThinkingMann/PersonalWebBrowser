using PWB_CCLibrary.Controls;

namespace PWB_CCLibrary.Interfaces;

public interface IBrowserPanel {
    public int Id { get; set; }

    string Url { get; set; }

    PWB_TabItem TabItem { get; set; }

    void Goto( string newAddress );

    void Refresh();
}
