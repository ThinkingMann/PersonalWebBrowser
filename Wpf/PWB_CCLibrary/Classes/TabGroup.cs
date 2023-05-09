using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

using PWB_CCLibrary.Controls;
using PWB_CCLibrary.Delegates;

namespace PWB_CCLibrary.Classes {
    public class TabGroup : INotifyPropertyChanged {
        #region INotifyPropertyChanged interface implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged( string propertyName ) {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion

        #region members and properties
        private string _name;

        public string Name {
            get => _name;
            set {
                _name = value;
                RaisePropertyChanged( nameof( Name ) );
            }
        }

        private ObservableCollection<PWB_TabItem> pinnedItems = new ObservableCollection<PWB_TabItem>();

        public ObservableCollection<PWB_TabItem> PinnedItems {
            get => pinnedItems;
            set => pinnedItems = value;
        }

        private ObservableCollection<PWB_TabItem> normalItems = new ObservableCollection<PWB_TabItem>();

        public ObservableCollection<PWB_TabItem> NormalItems {
            get => normalItems;
            set => normalItems = value;
        }
        #endregion

        #region Injected Event Handlers
        private Func<bool> _isPinOk;
        private Action<object, TabItemEventArgs> _tabItemSelected;
        private Action<object, RoutedEventArgs> _pinnedChanged;
        private Action<object, RoutedEventArgs> _closeButtonPressed;

        private void Item_TabItemSelected( object sender, TabItemEventArgs e ) => _tabItemSelected( sender, e );

        private void Item_PinnedChanged( object sender, RoutedEventArgs e ) => _pinnedChanged( sender, e );

        private void Item_CloseButtonPressed( object sender, RoutedEventArgs e ) => _closeButtonPressed( sender, e );
        #endregion

        public TabGroup( String name, Func<bool> isPinOk,
                         Action<object, TabItemEventArgs> tabItemSelected,
                         Action<object, RoutedEventArgs> pinnedChanged,
                         Action<object, RoutedEventArgs> closeButtonPressed ) {
            _name = name ?? "New Tab Group";
            _isPinOk = isPinOk;
            _tabItemSelected = tabItemSelected;
            _pinnedChanged = pinnedChanged;
            _closeButtonPressed = closeButtonPressed;
        }

        #region CreateNewTabItem method
        public PWB_TabItem CreateNewTabItem( bool isPinned = false ) {
            PWB_TabItem item = new( _isPinOk ) {
                IsPinned = isPinned,
                Margin = isPinned ? new Thickness( 2, 0, 0, 0 ) : new Thickness( 1, 0, 0, 0 ),
                VerticalAlignment = VerticalAlignment.Top,

                //Address = "https://www.google.com",
                //Title = $"({NormalItems.Count + 1}) Google Search"
            };

            item.PinnedChanged += Item_PinnedChanged;
            item.CloseButtonPressed += Item_CloseButtonPressed;
            item.TabItemSelected += Item_TabItemSelected;

            NormalItems.Add( item );

            return item;
        }

        private void İtem_TabItemSelected( object sender, TabItemEventArgs e ) {
            throw new NotImplementedException();
        }
        #endregion

    }
}
