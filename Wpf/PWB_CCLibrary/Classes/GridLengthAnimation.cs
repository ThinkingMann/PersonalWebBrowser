using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace PWB_CCLibrary.Classes;

// Copied from:
// https://social.msdn.microsoft.com/Forums/vstudio/en-US/da47a4b8-4d39-4d6e-a570-7dbe51a842e4/gridlengthanimation?forum=wpf
public class GridLengthAnimation : AnimationTimeline {
    /// <summary>
    /// Returns the type of object to animate
    /// </summary>
    public override Type TargetPropertyType {
        get {
            return typeof( GridLength );
        }
    }

    /// <summary>
    /// Creates an instance of the animation object
    /// </summary>
    /// <returns>Returns the instance of the GridLengthAnimation</returns>
    protected override System.Windows.Freezable CreateInstanceCore() {
        return new GridLengthAnimation();
    }

    /// <summary>
    /// Dependency property for the From property
    /// </summary>
    public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength),
            typeof(GridLengthAnimation));

    /// <summary>
    /// CLR Wrapper for the From depenendency property
    /// </summary>
    public GridLength From {
        get {
            return (GridLength)GetValue( GridLengthAnimation.FromProperty );
        }
        set {
            SetValue( GridLengthAnimation.FromProperty, value );
        }
    }

    /// <summary>
    /// Dependency property for the To property
    /// </summary>
    public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength),
            typeof(GridLengthAnimation));

    /// <summary>
    /// CLR Wrapper for the To property
    /// </summary>
    public GridLength To {
        get {
            return (GridLength)GetValue( GridLengthAnimation.ToProperty );
        }
        set {
            SetValue( GridLengthAnimation.ToProperty, value );
        }
    }

    /// <summary>
    /// Animates the grid let set
    /// </summary>
    /// <param name="defaultOriginValue">The original value to animate</param>
    /// <param name="defaultDestinationValue">The final value</param>
    /// <param name="animationClock">The animation clock (timer)</param>
    /// <returns>Returns the new grid length to set</returns>
    public override object GetCurrentValue( object defaultOriginValue,
        object defaultDestinationValue, AnimationClock animationClock ) {
        double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
        //check that from was set from the caller
        if (fromVal == 1)
            //set the from as the actual value
            fromVal = ((GridLength)defaultDestinationValue).Value;

        double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

        if (fromVal > toVal)
            return new GridLength( (1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, GridUnitType.Star );
        else
            return new GridLength( animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, GridUnitType.Star );
    }
}
