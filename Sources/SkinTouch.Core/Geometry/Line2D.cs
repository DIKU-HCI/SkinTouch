using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SkinTouch.Geometry
{
    #region Class 'Line2D'
    public class Line2D
    {
        #region Class Members
        private Point m_origin;

        private Vector m_direction;
        #endregion

        #region Constructors
        public Line2D(Point origin, Vector direction)
        {
            m_origin = origin;
            m_direction = direction;
        }

        public Line2D(double a, double b, bool xMajor = true)
        {
            // now we need to calculate
            Point p1, p2;
            if (xMajor)
            {
                p1 = new Point(0.0, a * 0.0 + b);
                p2 = new Point(1.0, a * 1.0 + b);
            }
            else
            {
                p1 = new Point(a * 0.0 + b, 0.0);
                p2 = new Point(a * 1.0 + b, 1.0);
            }

            m_origin = p1;

            m_direction = (p2 - p1);
            m_direction.Normalize();
        }
        #endregion

        #region Properties
        public Point Origin
        {
            get { return m_origin; }
        }

        public Vector Direction
        {
            get { return m_direction; }
        }
        #endregion

        #region Geometrical Methods
        public Point? IntersectWith(Line2D line)
        {
            Point p1 = m_origin;
            Point p2 = m_origin + 1.0 * m_direction;
            Point p3 = line.Origin;
            Point p4 = line.Origin + 1.0 * line.Direction;

            double determinant = (p1.X - p2.X) * (p3.Y - p4.Y)
                - (p1.Y - p2.Y) * (p3.X - p4.X);
            if (Math.Abs(determinant) <= 0.0001)
            {
                return null;
            }

            return (new Point(
                ((p1.X * p2.Y - p1.Y * p2.X) * (p3.X - p4.X) - (p1.X - p2.X) * (p3.X * p4.Y - p3.Y * p4.X)) / determinant,
                ((p1.X * p2.Y - p1.Y * p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X * p4.Y - p3.Y * p4.X)) / determinant));
        }
        #endregion
    }
    #endregion
}
