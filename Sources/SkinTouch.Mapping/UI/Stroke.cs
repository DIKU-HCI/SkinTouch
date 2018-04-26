using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SkinTouch.Mapping.UI
{
    #region Class 'Stroke'
    public class Stroke
    {
        #region Class Members
        private Path m_currentStroke;

        private PathGeometry m_currentGeometry;

        private PolyQuadraticBezierSegment m_currentLine;

        private Point[] m_previousPoints = new Point[2];

        private Point m_currentPoint;
        #endregion

        #region Constructors
        internal Stroke(Point point)
        {
            m_previousPoints[0] = point;
            m_previousPoints[1] = point;
            m_currentPoint = point;

            m_currentStroke = new Path();
            m_currentGeometry = new PathGeometry();
            m_currentStroke.Data = m_currentGeometry;

            m_currentStroke.Stroke = new SolidColorBrush(Colors.Yellow);
            m_currentStroke.StrokeThickness = 15.0;

            PathFigure figure = new PathFigure();
            figure.StartPoint = m_currentPoint;

            m_currentLine = new PolyQuadraticBezierSegment();
            m_currentLine.IsSmoothJoin = true;

            figure.Segments.Add(m_currentLine);
            m_currentGeometry.Figures.Add(figure);
        }
        #endregion

        #region Properties
        internal Path Path
        {
            get { return m_currentStroke; }
        }
        #endregion

        #region Update Methods
        internal void AddPoint(Point point)
        {
            if (point.X < 0.0
                || point.Y < 0.0)
            {
                return;
            }

            m_previousPoints[1] = m_previousPoints[0];
            m_previousPoints[0] = m_currentPoint;
            m_currentPoint = point;

            Point centerPt1 = new Point((m_previousPoints[0].X + m_previousPoints[1].X) / 2.0,
                (m_previousPoints[0].Y + m_previousPoints[1].Y) / 2.0);
            Point centerPt2 = new Point((m_currentPoint.X + m_previousPoints[0].X) / 2.0,
                (m_currentPoint.Y + m_previousPoints[0].Y) / 2.0);

            m_currentLine.Points.Add(m_previousPoints[0]);
            m_currentLine.Points.Add(centerPt2);
        }
        #endregion
    }
    #endregion
}
