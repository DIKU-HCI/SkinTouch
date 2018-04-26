using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SkinTouch.Geometry
{
    #region Class 'VertexGenerator'
    internal class VertexGenerator
    {
        #region Static Circle & Ellipse Methods
        internal static List<Point3D> GenerateCirclePoints(double radius, int numPoints, bool includeFull = false)
        {
            double angleStep = 2.0 * Math.PI / numPoints;

            List<Point3D> circlePoints = new List<Point3D>();
            for (int i = 0; i < numPoints; i++)
            {
                double angle = i * angleStep;

                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);

                circlePoints.Add(new Point3D(-x, -y, 0.0));
            }

            if (includeFull)
            {
                circlePoints.Add(new Point3D(-radius, 0.0, 0.0));
            }

            return circlePoints;
        }

        internal static List<Point3D> GenerateEllipsePoints(double majorRadius, double minorRaidus, int numPoints, bool useFull = false)
        {
            double stepping = 0.0001;
            double angle = 0.0;

            double a = majorRadius;
            double b = minorRaidus;

            double x0 = a;
            double y0 = 0.0;
            double d = Calculations.GetEllipseCircumference(a, b, stepping);

            double subLength = d / numPoints;
            angle = 0.0;
            x0 = a;
            y0 = 0.0;

            List<Point3D> ellipsePoints = new List<Point3D>();
            ellipsePoints.Add(new Point3D(-x0, -y0, 0.0));

            for (int i = 0; i < numPoints - 1; i++)
            {
                double distance = 0.0;
                while (distance < subLength)
                {
                    angle += stepping;

                    double x = a * Math.Cos(angle);
                    double y = b * Math.Sin(angle);

                    distance += Calculations.GetDistance(x0, y0, x, y);
                    x0 = x;
                    y0 = y;
                }

                ellipsePoints.Add(new Point3D(-x0, -y0, 0.0));
            }

            if (useFull)
            {
                ellipsePoints.Add(new Point3D(-a, 0.0, 0.0));
            }

            return ellipsePoints;
        }
        #endregion
    }
    #endregion
}
