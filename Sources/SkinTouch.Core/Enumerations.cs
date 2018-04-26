using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinTouch
{
    #region Enumerations
    [Flags]
    public enum RawDataEventType : int
    {
        Outside = 1,
        WithinArm = 2,
        Hover = 4,
        Touch = 8
    }

    [Flags]
    public enum SkinEventType : int
    {
        None = 1,
        Hover = 2,
        Down = 4,
        Move = 8,
        Up = 16
    }

    [Flags]
    public enum ModelType : int
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Tracing = 3
    }

    [Flags]
    public enum ModelDirection : int
    {
        Landscape = 1,
        Portrait = 2
    }
    #endregion
}
