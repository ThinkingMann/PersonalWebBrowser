using System;
using System.Windows.Media.Imaging;

namespace PWB_CCLibrary.Common;

public static class GenFuncts {
    public static BitmapImage? GetImage( string url ) {
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
}
