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
    #region Class 'Sphere'
    public class Sphere
    {
        #region Class Members
        private GeometryModel3D m_model;

        private MeshGeometry3D m_mesh;

        private Material m_material;

        private Color m_color;

        private double m_radius;

        private Vector3D m_position = new Vector3D(0.0, 0.0, 0.0);
        #endregion

        #region Constructors
        public Sphere(Color color, double radius = 0.005)
        {
            m_color = color;
            m_radius = radius;

            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            m_mesh = new MeshGeometry3D();

            CreateSphere(m_mesh, new Point3D(0.0, 0.0, 0.0), m_radius, 20, 20);

            m_model = new GeometryModel3D();
            m_model.Geometry = m_mesh;

            m_material = new DiffuseMaterial(new SolidColorBrush(m_color));
            m_model.Material = m_material;
        }

        private void CreateSphere(MeshGeometry3D mesh, Point3D center, double radius, int numPhi, int numTheta)
        {
            Dictionary<Point3D, int> dict = new Dictionary<Point3D, int>();

            double phi0, theta0;
            double dphi = Math.PI / numPhi;
            double dtheta = 2 * Math.PI / numTheta;

            phi0 = 0;
            double y0 = radius * Math.Cos(phi0);
            double r0 = radius * Math.Sin(phi0);

            for (int i = 0; i < numPhi; i++)
            {
                double phi1 = phi0 + dphi;
                double y1 = radius * Math.Cos(phi1);
                double r1 = radius * Math.Sin(phi1);

                theta0 = 0;
                Point3D pt00 = new Point3D(
                    center.X + r0 * Math.Cos(theta0),
                    center.Y + y0,
                    center.Z + r0 * Math.Sin(theta0));
                Point3D pt10 = new Point3D(
                    center.X + r1 * Math.Cos(theta0),
                    center.Y + y1,
                    center.Z + r1 * Math.Sin(theta0));

                for (int j = 0; j < numTheta; j++)
                {
                    double theta1 = theta0 + dtheta;
                    Point3D pt01 = new Point3D(
                        center.X + r0 * Math.Cos(theta1),
                        center.Y + y0,
                        center.Z + r0 * Math.Sin(theta1));
                    Point3D pt11 = new Point3D(
                        center.X + r1 * Math.Cos(theta1),
                        center.Y + y1,
                        center.Z + r1 * Math.Sin(theta1));

                    CreateTriangle(mesh, dict, pt00, pt11, pt10);
                    CreateTriangle(mesh, dict, pt00, pt01, pt11);

                    theta0 = theta1;
                    pt00 = pt01;
                    pt10 = pt11;
                }

                phi0 = phi1;
                y0 = y1;
                r0 = r1;
            }
        }

        private void CreateTriangle(MeshGeometry3D mesh, Dictionary<Point3D, int> dict, Point3D point1, Point3D point2, Point3D point3)
        {
            int index1, index2, index3;
            if (dict.ContainsKey(point1))
            {
                index1 = dict[point1];
            }
            else
            {
                index1 = mesh.Positions.Count;
                mesh.Positions.Add(point1);
                dict.Add(point1, index1);
            }

            if (dict.ContainsKey(point2))
            {
                index2 = dict[point2];
            }
            else
            {
                index2 = mesh.Positions.Count;
                mesh.Positions.Add(point2);
                dict.Add(point2, index2);
            }

            if (dict.ContainsKey(point3))
            {
                index3 = dict[point3];
            }
            else
            {
                index3 = mesh.Positions.Count;
                mesh.Positions.Add(point3);
                dict.Add(point3, index3);
            }

            if ((index1 == index2) ||
                (index2 == index3) ||
                (index3 == index1))
            {
                return;
            }

            mesh.TriangleIndices.Add(index1);
            mesh.TriangleIndices.Add(index2);
            mesh.TriangleIndices.Add(index3);
        }
        #endregion

        #region Properties
        public GeometryModel3D Model
        {
            get { return m_model; }
        }

        public Color Color
        {
            get
            {
                Color color = new Color();
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        delegate ()
                        {
                            color = ((SolidColorBrush)((DiffuseMaterial)m_material).Brush).Color;
                        }));
                return color;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        delegate ()
                        {
                            m_material = new DiffuseMaterial(new SolidColorBrush(value));
                            m_model.Material = m_material;
                        }));
            }
        }
        #endregion
    }
    #endregion
}
