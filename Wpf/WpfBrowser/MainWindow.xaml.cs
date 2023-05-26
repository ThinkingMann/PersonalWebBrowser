using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfBrowser;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    private static IntPtr WindowProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled ) {
        switch (msg) {
            case 0x0024:
                WmGetMinMaxInfo( hwnd, lParam );
                handled = true;
                break;
            default:
                break;
        }
        return (IntPtr)0;
    }

    public MainWindow() {
        InitializeComponent();

        SourceInitialized += ( s, e ) => {
            IntPtr handle = (new WindowInteropHelper( this )).Handle;
            HwndSource.FromHwnd( handle ).AddHook( new HwndSourceHook( WindowProc ) );
        };

        // Windows 11 Rounded Corners
        IntPtr hWnd = new WindowInteropHelper( GetWindow( this ) ).EnsureHandle();
        var attribute = DWWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
        var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;

        DwmSetWindowAttribute( hWnd, attribute, ref preference, sizeof( uint ) );

        StateChanged += MainWindow_StateChanged;

        tgcTabGroups.MainContent = MainContent;
        tgcTabGroups.Tabber = MainContent.Tabber;
        //tgcTabGroups.TabGroups = MainContent.Tabber.TabGroups;
    }

    private void MainWindow_StateChanged( object? sender, EventArgs e ) {
        IntPtr hWnd = new WindowInteropHelper( GetWindow( this ) ).EnsureHandle();
        MainWindow Current = this;
        if (Current.WindowState == WindowState.Maximized) {
            var attribute = DWWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute( hWnd, attribute, ref preference, sizeof( uint ) );
        } else {
            var attribute = DWWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute( hWnd, attribute, ref preference, sizeof( uint ) );
        }
    }


    #region User32 hooks for monitors

    [DllImport( "dwmapi.dll" )]
    private static extern long DwmSetWindowAttribute( IntPtr hwnd,
        DWWINDOWATTRIBUTE attribute,
        ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
        uint cbAttribute );

    public enum DWWINDOWATTRIBUTE {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    public enum DWM_WINDOW_CORNER_PREFERENCE {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3,
    }

    private static void WmGetMinMaxInfo( IntPtr hwnd, IntPtr lParam ) {
        MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
        int MONITIR_DEFAULTTONEAREST = 0x00000002;
        IntPtr monitor = MonitorFromWindow(hwnd, MONITIR_DEFAULTTONEAREST);
        if (monitor != IntPtr.Zero) {
            MONITORINFO monitorinfo = new MONITORINFO();
            GetMonitorInfo( monitor, monitorinfo );
            RECT rcWorkArea = monitorinfo.rcWork;
            RECT rcMonitorArea = monitorinfo.rcMonitor;
            mmi.ptMaxPosition.x = Math.Abs( rcWorkArea.left - rcMonitorArea.left );
            mmi.ptMaxPosition.y = Math.Abs( rcWorkArea.top - rcMonitorArea.top );
            mmi.ptMaxSize.x = Math.Abs( rcWorkArea.right - rcMonitorArea.left );
            mmi.ptMaxSize.y = Math.Abs( rcWorkArea.bottom - rcMonitorArea.top );
        }
        Marshal.StructureToPtr( mmi, lParam, true );
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct POINT {
        public int x;
        public int y;

        public POINT( int x, int y ) {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct MINMAXINFO {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
    public class MONITORINFO {
        public int cbSize = Marshal.SizeOf(typeof( MONITORINFO ) );
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }

    public struct RECT {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public static readonly RECT Empty = new RECT();

        public int Width => Math.Abs( right - left );
        public int Height => Math.Abs( bottom - top );

        public RECT() {
            this.left = this.top = this.right = this.bottom = 0;
        }

        public RECT( int left, int top, int right, int bottom ) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT( RECT rcSrc ) : this( rcSrc.left, rcSrc.top, rcSrc.right, rcSrc.bottom ) { }

        public bool IsEmpty => left >= right || top >= bottom;

        public override string ToString() {
            if (this == Empty) return "RECT (Empty)";
            return $"RECT (Empty) {left}/{top}/{right}/{bottom}";
        }

        public override bool Equals( [NotNullWhen( true )] object? obj ) {
            if (obj is null || obj is not RECT rect) return false;
            return left == rect.left && right == rect.right && top == rect.top && bottom == rect.bottom;
        }

        public override int GetHashCode() => HashCode.Combine( left, top, right, bottom );

        public static bool operator ==( RECT left, RECT right ) => left.Equals( right );
        public static bool operator !=( RECT left, RECT right ) => !left.Equals( right );
    }

    [DllImport( "user32" )]
    internal static extern bool GetMonitorInfo( IntPtr hMonitor, MONITORINFO lpmi );

    [DllImport( "user32" )]
    internal static extern IntPtr MonitorFromWindow( IntPtr handle, int flags );
    #endregion
}
