using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SkinTouch.Visuals
{
    #region Class 'Cylinder'
    public class Cylinder
    {
        #region Class Members
        private GeometryModel3D m_model;

        private MeshGeometry3D m_mesh;

        private Material m_material;

        private double m_thickness;

        private Point3D m_start;

        private Point3D m_end;

        private Color m_color;
        #endregion

        #region Constructors
        public Cylinder(Point3D start, Point3D end,
            double thickness, Color color)
        {
            m_thickness = thickness;
            m_start = start;
            m_end = end;
            m_color = color;

            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            m_mesh = new MeshGeometry3D();

            Vector3D axis = m_end - m_start;
            CreateCylinder(m_mesh, m_start, axis, m_thickness, 12);

            m_model = new GeometryModel3D();
            m_model.Geometry = m_mesh;

            m_material = new DiffuseMaterial(new SolidColorBrush(m_color));

            m_model.Material = m_material;
            m_model.BackMaterial = m_model.Material;
        }

        private void CreateCylinder(MeshGeometry3D mesh, Point3D endPoint, Vector3D axis, double radius, int numSides)
        {
            // Get two vectors perpendicular to the axis.
            Vector3D v1;
            if ((axis.Z < -0.01) || (axis.Z > 0.01))
            {
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            }
            else
            {
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
            }

            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

            // Make the vectors have length radius.
            v1 *= (radius / v1.Length);
            v2 *= (radius / v2.Length);

            // Make the top end cap.
            // Make the end point.
            int pt0 = mesh.Positions.Count; // Index of end_point.
            mesh.Positions.Add(endPoint);

            // Make the top points.
            double theta = 0;
            double dtheta = 2 * Math.PI / numSides;
            for (int i = 0; i < numSides; i++)
            {
                mesh.Positions.Add(endPoint +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2);
                theta += dtheta;
            }

            // Make the top triangles.
            int pt1 = mesh.Positions.Count - 1; // Index of last point.
            int pt2 = pt0 + 1;                  // Index of first point in this cap.
            for (int i = 0; i < numSides; i++)
            {
                mesh.TriangleIndices.Add(pt0);
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                pt1 = pt2++;
            }

            // Make the bottom end cap.
            // Make the end point.
            pt0 = mesh.Positions.Count; // Index of end_point2.
            Point3D end_point2 = endPoint + axis;
            mesh.Positions.Add(end_point2);

            // Make the bottom points.
            theta = 0;
            for (int i = 0; i < numSides; i++)
            {
                mesh.Positions.Add(end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2);
                theta += dtheta;
            }

            // Make the bottom triangles.
            theta = 0;
            pt1 = mesh.Positions.Count - 1; // Index of last point.
            pt2 = pt0 + 1;                  // Index of first point in this cap.
            for (int i = 0; i < numSides; i++)
            {
                mesh.TriangleIndices.Add(numSides + 1);    // end_point2
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt1);
                pt1 = pt2++;
            }

            // Make the sides.
            // Add the points to the mesh.
            int first_side_point = mesh.Positions.Count;
            theta = 0;
            for (int i = 0; i < numSides; i++)
            {
                Point3D p1 = endPoint +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                mesh.Positions.Add(p1);
                Point3D p2 = p1 + axis;
                mesh.Positions.Add(p2);
                theta += dtheta;
            }

            // Make the side triangles.
            pt1 = mesh.Positions.Count - 2;
            pt2 = pt1 + 1;
            int pt3 = first_side_point;
            int pt4 = pt3 + 1;
            for (int i = 0; i < numSides; i++)
            {
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt4);

                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt4);
                mesh.TriangleIndices.Add(pt3);

                pt1 = pt3;
                pt3 += 2;
                pt2 = pt4;
                pt4 += 2;
            }
        }
        #endregion

        #region Properties
        public GeometryModel3D Model
        {
            get { return m_model; }
        }
        #endregion
    }
    #endregion
}
