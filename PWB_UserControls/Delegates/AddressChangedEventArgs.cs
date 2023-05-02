using System;

namespace PWB_UserControls.Delegates;

public delegate void AddressChangedEventHandler( object sender, AddressChangedEventArgs e );

public class AddressChangedEventArgs : EventArgs {
    public string? OldAddress { get; set; }
    public string? NewAddress { get; set; }
}
