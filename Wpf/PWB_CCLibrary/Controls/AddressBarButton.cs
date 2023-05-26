using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using GF = PWB_CCLibrary.Common.GenFuncts;

namespace PWB_CCLibrary.Controls;

internal enum Modes { Path, Image, ImageSource }

public class AddressBarButton : Button {
    static AddressBarButton() {
        DefaultStyleKeyProperty.OverrideMetadata( typeof( AddressBarButton ), new FrameworkPropertyMetadata( typeof( AddressBarButton ) ) );
    }

    #region Dependency Properties
    public String ImageSource {
        get { return (String)GetValue( ImageSourceProperty ); }
        set {
            SetValue( ImageSourceProperty, value );
            PrepareImages();
            //SetImage();
        }
    }

    // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageSourceProperty =
                           DependencyProperty.Register(nameof( ImageSource ), typeof(String), typeof(AddressBarButton), new PropertyMetadata(""));

    public String ImageKey {
        get { return (String)GetValue( ImageKeyProperty ); }
        set {
            SetValue( ImageKeyProperty, value );
            PrepareImages();
            //SetImage();
        }
    }

    // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageKeyProperty =
                           DependencyProperty.Register(nameof( ImageKey ), typeof(String), typeof(AddressBarButton), new PropertyMetadata(""));

    public int RotateAngle {
        get { return (int)GetValue( RotateAngleProperty ); }
        set { SetValue( RotateAngleProperty, value ); }
    }

    // Using a DependencyProperty as the backing store for RotateAngle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RotateAngleProperty =
    DependencyProperty.Register(nameof( RotateAngle ), typeof(int), typeof(AddressBarButton), new PropertyMetadata(0));
    #endregion

    #region Members and Properties
    Path? _myPath;
    Image? _myImage;

    Modes _viewMode;
    #endregion

    public AddressBarButton() {
        Loaded += AddressBarButton_Loaded;
    }

    private void AddressBarButton_Loaded( object sender, RoutedEventArgs e ) {
        if (!string.IsNullOrEmpty( ImageSource ) && !string.IsNullOrEmpty( ImageKey ))
            throw new Exception( "Ambiguous reference definition detected. Use only one of ImageSource or ImageKey. Simutaneous definition is not alloed" );

        _viewMode = Modes.Image;
        if (!string.IsNullOrEmpty( ImageKey )) {
            if (ImageKey.StartsWith( "geo" ))
                _viewMode = Modes.Path;
        } else
            _viewMode = Modes.ImageSource;

        if (_viewMode == Modes.Path)
            _myPath!.Visibility = Visibility.Visible;
        else
            _myImage!.Visibility = Visibility.Visible;

        PrepareImages();
    }

    private void PrepareImages() {
        switch (_viewMode) {
            case Modes.Path:
                var geo = Application.Current.TryFindResource(ImageKey) as Geometry;
                _myPath!.Data = geo;
                _myPath.Fill = Brushes.Black;
                if (RotateAngle != 0)
                    _myPath.LayoutTransform = new RotateTransform( RotateAngle );

                break;
            case Modes.Image:
                var drw = Application.Current.TryFindResource(ImageKey) as DrawingImage;
                _myImage!.Source = drw;
                if (RotateAngle != 0)
                    _myImage.LayoutTransform = new RotateTransform( RotateAngle );

                break;
            case Modes.ImageSource:
                _myImage!.Source = GF.GetImage( ImageSource ) ?? new BitmapImage();
                break;
        }
    }

    public override void OnApplyTemplate() {

        _myPath = Template.FindName( "PART_Path", this ) as Path;
        _myImage = Template.FindName( "PART_Image", this ) as Image;

        if (_myPath is null || _myImage is null) {
            throw new Exception( "Applied template does not contain either of PART_Image and/or PART_Path" );
        }

        base.OnApplyTemplate();
    }

}