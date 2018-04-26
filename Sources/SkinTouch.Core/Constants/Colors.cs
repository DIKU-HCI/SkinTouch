using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SkinTouch
{
    #region Partial Class 'Constants'
    internal partial class Constants
    {
        #region Nested Class 'Colors'
        internal class Colors
        {
            #region Static Class Members
            internal static readonly Color InactiveTouchColor = Color.FromArgb(128, 200, 0, 0);

            internal static readonly Color ActiveTouchColor = Color.FromArgb(255, 0, 200, 0);

            internal static readonly Color HoverTouchColor = Color.FromArgb(192, 200, 200, 0);

            internal static readonly Color SkinTextureColor = Color.FromArgb(255, 80, 80, 80);

            internal static readonly Color HorizontalGridModelColor = System.Windows.Media.Colors.OrangeRed;

            internal static readonly Color VerticalGridModelColor = System.Windows.Media.Colors.DarkBlue;

            internal static readonly Color TracingGridModelColor = System.Windows.Media.Colors.Green;
            #endregion
        }
        #endregion
    }
    #endregion
}
