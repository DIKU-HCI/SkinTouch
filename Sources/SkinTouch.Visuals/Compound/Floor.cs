using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SkinTouch.Visuals
{
    #region Class 'Floor'
    public class Floor
    {
        #region Class Members
        private Model3DGroup m_model;

        private List<Cylinder> m_majorTickLines;

        private List<Cylinder> m_minorTickLines;
        
        private double m_lineThickness = 0.001 * 1000.0;

        private Size m_size;

        private int m_majorTicks;

        private int m_minorTicks;
        #endregion

        #region Constructors
        public Floor(Size size, int majorTicks, int minorTicks, double lineThickness = 1.0)
        {
            m_size = size;
            m_majorTicks = majorTicks;
            m_minorTicks = minorTicks;
            m_lineThickness = lineThickness;

            if (m_majorTicks % 2 != 0)
            {
                m_majorTicks++;
            }

            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            Color minorColor = Color.FromArgb(255, 64, 64, 64);
            Color majorColor = Color.FromArgb(255, 128, 128, 128);

            double width = m_size.Width;
            double height = m_size.Height;
            
            m_model = new Model3DGroup();

            m_majorTickLines = new List<Cylinder>();
            m_minorTickLines = new List<Cylinder>();

            for (int i = 0; i <= m_majorTicks / 2; i++)
            {
                // majors
                Cylinder line1 = new Cylinder(new Point3D(-width / 2.0, 0.0, i * 1.0 * height / m_majorTicks),
                    new Point3D(width / 2.0, 0.0, i * 1.0 * height / m_majorTicks), m_lineThickness, majorColor);
                m_majorTickLines.Add(line1);

                if (i != 0)
                {
                    Cylinder line2 = new Cylinder(new Point3D(-width / 2.0, 0.0, -i * 1.0 * height / m_majorTicks),
                        new Point3D(width / 2.0, 0.0, -i * 1.0 * height / m_majorTicks), m_lineThickness, majorColor);
                    m_majorTickLines.Add(line2);
                }

                Cylinder line3 = new Cylinder(new Point3D(i * 1.0 * width / m_majorTicks, 0.0, height / 2.0),
                    new Point3D(i * 1.0 * width / m_majorTicks, 0.0, -height / 2.0), m_lineThickness, majorColor);
                m_majorTickLines.Add(line3);

                if (i != 0)
                {
                    Cylinder line4 = new Cylinder(new Point3D(-i * 1.0 * width / m_majorTicks, 0.0, height / 2.0),
                        new Point3D(-i * 1.0 * width / m_majorTicks, 0.0, -height / 2.0), m_lineThickness, majorColor);
                    m_majorTickLines.Add(line4);
                }

                // minors
                if (i != 0)
                {
                    double wSpacing = width / m_majorTicks;
                    double hSpacing = height / m_majorTicks;

                    for (int j = 1; j <= m_minorTicks; j++)
                    {
                        Cylinder line5 = new Cylinder(new Point3D(-width / 2.0, 0.0, (i - 1) * 1.0 * height / m_majorTicks + (double)j / m_minorTicks * hSpacing),
                            new Point3D(width / 2.0, 0.0, (i - 1) * 1.0 * height / m_majorTicks + (double)j / m_minorTicks * hSpacing), m_lineThickness, minorColor);
                        m_minorTickLines.Add(line5);

                        Cylinder line6 = new Cylinder(new Point3D(-width / 2.0, 0.0, -(i - 1) * 1.0 * height / m_majorTicks - (double)j / m_minorTicks * hSpacing),
                            new Point3D(width / 2.0, 0.0, -(i - 1) * 1.0 * height / m_majorTicks - (double)j / m_minorTicks * hSpacing), m_lineThickness, minorColor);
                        m_minorTickLines.Add(line6);

                        Cylinder line7 = new Cylinder(new Point3D((i - 1) * 1.0 * width / m_majorTicks + (double)j / m_minorTicks * wSpacing, 0.0, height / 2.0),
                            new Point3D((i - 1) * 1.0 * width / m_majorTicks + (double)j / m_minorTicks * wSpacing, 0.0, -height / 2.0), m_lineThickness, minorColor);
                        m_minorTickLines.Add(line7);

                        Cylinder line8 = new Cylinder(new Point3D(-(i - 1) * 1.0 * width / m_majorTicks - (double)j / m_minorTicks * wSpacing, 0.0, height / 2.0),
                            new Point3D(-(i - 1) * 1.0 * width / m_majorTicks - (double)j / m_minorTicks * wSpacing, 0.0, -height / 2.0), m_lineThickness, minorColor);
                        m_minorTickLines.Add(line8);
                    }
                }
            }

            foreach (Cylinder minorLine in m_minorTickLines)
            {
                m_model.Children.Add(minorLine.Model);
            }

            foreach (Cylinder majorLine in m_majorTickLines)
            {
                m_model.Children.Add(majorLine.Model);
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
