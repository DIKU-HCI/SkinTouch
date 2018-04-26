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
        #region Nested Class 'Entities'
        internal class Entities
        {
            #region Static Class Members
            public const double DefaultArmCircumference = 260.0;

            public const double DefaultWristWidth = 52.0;

            public const double DefaultWristHeight = 44.0;

            public static Size DefaultWristSize = new Size(DefaultWristWidth, DefaultWristHeight);

            public const double DefaultArmLength = 220.0;

            public static Point3D DefaultElbowLocation = new Point3D(32.0180580019951, 1279.02913093567, 53.4249693155289);

            public static Quaternion DefaultElbowOrientation = new Quaternion(-0.280943930149078, 0.183404937386513, 0.721736133098602, -0.605417430400848);

            public static Point3D DefaultWristLocation = new Point3D(-153.89096736908, 1250.92041492462, 187.19470500946);

            public static Quaternion DefaultWristOrientation = new Quaternion(0.335674852132797, 0.0391782335937023, -0.927251935005188, 0.161218479275703);

            public static Point3D DefaultFingerLocation = new Point3D(0.0, 1400.0, 110.0);

            public static Quaternion DefaultFingerOrientation = new Quaternion(new Vector3D(0.0, 0.0, 1.0), 0.0);
            #endregion
        }
        #endregion
    }
    #endregion
}
