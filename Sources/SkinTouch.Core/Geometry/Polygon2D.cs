using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SkinTouch.Geometry
{
    #region Class 'Polygon2D'
    internal class Polygon2D
    {
        #region Class Members
        private List<Point> m_points;
        #endregion

        #region Constructors
        public Polygon2D()
        {
            m_points = new List<Point>();
        }
        #endregion

        #region Properties
        public List<Point> Points
        {
            get { return m_points; }
        }
        #endregion

        #region Geometrical Methods
        public bool IsInside(Point point)
        {
            Point p1, p2;
            bool inside = false;

            if (m_points.Count < 3)
            {
                return inside;
            }

            Point oldPoint = new Point(m_points[m_points.Count - 1].X,
                m_points[m_points.Count - 1].Y);

            for (int i = 0; i < m_points.Count; i++)
            {
                Point newPoint = new Point(m_points[i].X, m_points[i].Y);
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < point.X) == (point.X <= oldPoint.X)
                    && (point.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (point.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }

            return inside;
        }
        #endregion
    }
    #endregion
}
