using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PWB_CCLibrary.Controls;

public class AddressBarButton : Button {

    #region Dependendy Properties (ImageSource)
    public String ImageSource {
        get { return (String)GetValue( ImageSourceProperty ); }
        set {
            SetValue( ImageSourceProperty, value );
            PrepareImage();
        }
    }

    // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageSourceProperty =
                           DependencyProperty.Register(nameof( ImageSource ), typeof(String), typeof(AddressBarButton), new PropertyMetadata(""));
    #endregion

    #region protected controle (myViewbox and myImage)
    protected Viewbox myViewbox { get; set; }

    protected Image? myImage { get; set; }
    #endregion

    #region Class ctor..
    public AddressBarButton() : base() {
        this.BorderThickness = new Thickness( 0 );
        this.Background = Brushes.Transparent;
        this.BorderBrush = Brushes.Transparent;

        this.Padding = new Thickness( 2 );

        myViewbox = new Viewbox();
        myViewbox.HorizontalAlignment = HorizontalAlignment.Stretch;
        myViewbox.VerticalAlignment = VerticalAlignment.Stretch;

        this.AddChild( myViewbox );

        this.Loaded += AddressBarButton_Loaded;
    }
    #endregion

    private void AddressBarButton_Loaded( object sender, RoutedEventArgs e ) {
        PrepareImage();
        this.myViewbox.Child = myImage;
    }

    #region PrepareImage() method
    private void PrepareImage() {
        try {
            #region Definition of image
            myImage ??= new Image();
            if (String.IsNullOrEmpty( ImageSource )) {
                myImage.Source = null;
                return;
            }
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            var imgsrc = ImageSource;
            if (!imgsrc.StartsWith( "pack://application:,,," ))
                imgsrc = "pack://application:,,," + imgsrc;
            bi.UriSource = new Uri( imgsrc, UriKind.RelativeOrAbsolute );
            bi.EndInit();
            myImage.Source = bi;
            #endregion
        } catch {
        }
    }
    #endregion

}
