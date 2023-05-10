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
            PrepareImages();
            SetImage();
        }
    }

    // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageSourceProperty =
                           DependencyProperty.Register(nameof( ImageSource ), typeof(String), typeof(AddressBarButton), new PropertyMetadata(""));

    public string DisabledImageSource {
        get { return (string)GetValue( DisabledImageSourceProperty ); }
        set {
            SetValue( DisabledImageSourceProperty, value );
            PrepareImages();
            SetImage();
        }
    }

    // Using a DependencyProperty as the backing store for DisabledImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DisabledImageSourceProperty =
    DependencyProperty.Register("DisabledImageSource", typeof(string), typeof(AddressBarButton), new PropertyMetadata(""));
    #endregion

    BitmapImage? enabledImage, disabledImage;

    #region protected controls (myViewbox and myImage)
    protected Viewbox myViewbox { get; set; }

    protected Image myImage { get; set; }
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
        myImage = new Image();
        myViewbox.Child = myImage;

        this.AddChild( myViewbox );

        this.Loaded += AddressBarButton_Loaded;
        this.IsEnabledChanged += AddressBarButton_IsEnabledChanged;

    }
    #endregion

    private void AddressBarButton_Loaded( object sender, RoutedEventArgs e ) {
        PrepareImages();
        SetImage();
        this.myViewbox.Child = myImage;
    }

    #region Image related methods
    private void PrepareImages() {
        enabledImage = GetImage( ImageSource ) ?? new BitmapImage();
        disabledImage = GetImage( DisabledImageSource ) ?? enabledImage;
    }

    private void SetImage() => myImage.Source = IsEnabled ? enabledImage : disabledImage;

    private void AddressBarButton_IsEnabledChanged( object sender, DependencyPropertyChangedEventArgs e ) {
        SetImage();
    }

    private static BitmapImage? GetImage( string url ) {
        try {
            if (!string.IsNullOrEmpty( url )) {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                var imgsrc = url;
                if (!imgsrc.StartsWith( "pack://application:,,," ))
                    imgsrc = "pack://application:,,," + imgsrc;
                bi.UriSource = new Uri( imgsrc, UriKind.RelativeOrAbsolute );
                bi.EndInit();
                return bi;
            }
        } catch {
        }
        return null;
    }
    #endregion

}
