using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinTouch
{
    #region Partial Class 'Constants'
    internal partial class Constants
    {
        #region Nested Class 'Models'
        internal class Models
        {
            #region Static Matrix Class Members
            internal const int HorizontalRows = 4;

            internal const int HorizontalColumns = 6;

            internal const int VerticalRows = 4;

            internal const int VerticalColumns = 6;

            internal const int TracingRows = 4;

            internal const int TracingColumns = 6;
            #endregion

            #region Static File Class Members
            internal static readonly string ModelDirectory = "./Models/";

            internal static readonly string HorizontalModelFileName = ModelDirectory + "coefficients_horizontal.txt";

            internal static readonly string VerticalModelFileName = ModelDirectory + "coefficients_vertical.txt";

            internal static readonly string TracingModelFileName = ModelDirectory + "coefficients_swipes.txt";
            #endregion
        }
        #endregion
    }
    #endregion
}
