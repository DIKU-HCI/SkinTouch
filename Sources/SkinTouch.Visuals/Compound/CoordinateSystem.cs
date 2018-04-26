using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SkinTouch.Visuals
{
    #region Class 'CoordinateSystem'
    public class CoordinateSystem
    {
        #region Class Members
        private Model3DGroup m_model;

        private Sphere m_origin;

        private Cylinder m_xAxis;

        private Cone m_xAxisArrow;

        private Cylinder m_yAxis;

        private Cone m_yAxisArrow;

        private Cylinder m_zAxis;

        private Cone m_zAxisArrow;

        private double m_lineThickness = 0.001 * 1000.0;

        private double m_axisLength;

        private bool m_colored;

        private bool m_showArrows;
        #endregion

        #region Constructors
        public CoordinateSystem(double axisLength = 500.0, bool showCenter = true, 
            bool colored = true, bool showArrows = true, double lineThickness = 1.0)
        {
            m_axisLength = axisLength;
            m_colored = colored;
            m_showArrows = showArrows;
            m_lineThickness = lineThickness;

            Initialize(showCenter);
        }
        #endregion

        #region Initialization
        private void Initialize(bool showCenter)
        {
            Point3D origin = new Point3D(0.0, 0.0, 0.0);
            Color color = Color.FromArgb(192, 64, 64, 64);

            if (m_colored)
            {
                m_xAxis = new Cylinder(origin, origin + new Vector3D(m_axisLength, 0.0, 0.0),
                    m_lineThickness, Colors.Red);
                m_yAxis = new Cylinder(origin, origin + new Vector3D(0.0, m_axisLength, 0.0),
                    m_lineThickness, Colors.Green);
                m_zAxis = new Cylinder(origin, origin + new Vector3D(0.0, 0.0, m_axisLength),
                    m_lineThickness, Colors.Blue);
            }
            else
            {
                m_xAxis = new Cylinder(origin, origin + new Vector3D(m_axisLength, 0.0, 0.0),
                    m_lineThickness, color);
                m_yAxis = new Cylinder(origin, origin + new Vector3D(0.0, m_axisLength, 0.0),
                    m_lineThickness, color);
                m_zAxis = new Cylinder(origin, origin + new Vector3D(0.0, 0.0, m_axisLength),
                    m_lineThickness, color);
            }

            m_model = new Model3DGroup();
            m_model.Children.Add(m_xAxis.Model);
            m_model.Children.Add(m_yAxis.Model);
            m_model.Children.Add(m_zAxis.Model);

            if (m_showArrows)
            {
                double coneThickness = 5.0 * m_lineThickness;
                double coneLength = 15.0 * m_lineThickness;

                Point3D xAxisEnd = origin + new Vector3D(m_axisLength, 0.0, 0.0);
                Point3D yAxisEnd = origin + new Vector3D(0.0, m_axisLength, 0.0);
                Point3D zAxisEnd = origin + new Vector3D(0.0, 0.0, m_axisLength);

                m_xAxisArrow = new Cone(xAxisEnd, xAxisEnd + new Vector3D(coneLength, 0.0, 0.0),
                    coneThickness, 0.0, m_colored ? Colors.Red : color);
                m_yAxisArrow = new Cone(yAxisEnd, yAxisEnd + new Vector3D(0.0, coneLength, 0.0),
                    coneThickness, 0.0, m_colored ? Colors.Green : color);
                m_zAxisArrow = new Cone(zAxisEnd, zAxisEnd + new Vector3D(0.0, 0.0, coneLength),
                    coneThickness, 0.0, m_colored ? Colors.Blue : color);

                m_model.Children.Add(m_xAxisArrow.Model);
                m_model.Children.Add(m_yAxisArrow.Model);
                m_model.Children.Add(m_zAxisArrow.Model);
            }

            if (showCenter)
            {
                m_origin = new Sphere(Color.FromArgb(255, 64, 64, 64), m_lineThickness * 7.5);
                m_model.Children.Add(m_origin.Model);
            }
        }
        #endregion

        #region Properties
        public Model3DGroup Model
        {
            get { return m_model; }
        }
        #endregion
    }
    #endregion
}
