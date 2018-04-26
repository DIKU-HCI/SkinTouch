using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinTouch.Geometry
{
    #region Class 'Calculations'
    internal class Calculations
    {
        #region Distance Methods
        internal static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
        }
        #endregion

        #region Ellipse Methods
        internal static double GetEllipseCircumference(double majorRadius, double minorRaidus, double stepping = 0.001)
        {
            return GetEllipseArcLength(majorRadius, minorRaidus, 2.0 * Math.PI, stepping);
        }

        internal static double GetEllipseArcLength(double majorRadius, double minorRaidus, double stopAngle, double stepping = 0.001)
        {
            if (stopAngle <= 0.0)
            {
                stopAngle += (2.0 * Math.PI);
                stopAngle %= (2.0 * Math.PI);
            }

            double angle = 0.0;
            double fullAngle = stopAngle;
            double angleStep = stepping;

            double a = majorRadius;
            double b = minorRaidus;

            double x0 = a;
            double y0 = 0.0;
            double d = 0.0;

            while (angle <= fullAngle)
            {
                double x = a * Math.Cos(angle);
                double y = b * Math.Sin(angle);

                d += GetDistance(x0, y0, x, y);
                x0 = x;
                y0 = y;

                angle += angleStep;
            }

            return d;
        }
        #endregion
    }
    #endregion
}
