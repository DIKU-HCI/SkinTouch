using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SkinTouch
{
    #region Partial Class 'Constants'
    internal partial class Constants
    {
        #region Static Debug Class Members
#if DEBUG
        internal static readonly bool DebugPrint = false;
#else
        internal static readonly bool DebugPrint = false;
#endif
        #endregion

        #region Static 3D Model Class Members
        internal static readonly int NumberOfArmLengthSegments = 12;

        internal static readonly int NumberOfArmShapeSegments = 24;
        #endregion

        #region Static OptiTrack Class Members
        internal const int MulticastConnectionType = 0;

        internal const int UnicastConnectionType = 1;
        #endregion

        #region Static RigidBody Class Members
        internal const string ForearmRigidBodyName = "Forearm";

        internal const string WristRigidBodyName = "Wrist";

        internal const string FingerRigidBodyName = "Finger";
        #endregion
    }
    #endregion
}
