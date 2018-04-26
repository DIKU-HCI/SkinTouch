using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Accord.Imaging;

using SkinTouch.Geometry;

namespace SkinTouch.Models
{
    #region Class 'LinearModel'
    internal class LinearModel
    {
        #region Class Members
        private int m_rows;

        private int m_cols;

        private ModelDirection m_direction = ModelDirection.Landscape;

        private List<Line2D> m_rowLines;

        private List<Line2D> m_columnLines;

        private Point[,] m_gridPoints;

        private Polygon2D[,] m_polygons;

        private Dictionary<Polygon2D, MatrixH> m_homographies = new Dictionary<Polygon2D, MatrixH>();
        #endregion

        #region Constructors
        public LinearModel(int rows, int columns, ModelDirection direction)
        {
            m_rows = rows;
            m_cols = columns;
            m_direction = direction;

            m_rowLines = new List<Line2D>();
            m_columnLines = new List<Line2D>();

            m_gridPoints = new Point[m_rows, m_cols];
            m_polygons = new Polygon2D[m_rows - 1, m_cols - 1];
        }
        #endregion

        #region Properties
        public int Rows
        {
            get { return m_rows; }
        }

        public int Columns
        {
            get { return m_cols; }
        }

        public Point[,] GridPoints
        {
            get { return m_gridPoints; }
        }

        public Point this[int row, int column]
        {
            get
            {
                try
                {
                    return m_gridPoints[row, column];
                }
                catch { return new Point(-1.0, -1.0); }
            }
        }
        #endregion

        #region Transform Methods
        public Point? Transform(Point point)
        {
            Polygon2D container = null;
            int polygonRow = -1;
            int polygonCol = -1;

            for (int i = 0; i < m_rows - 1; i++)
            {
                bool done = false;
                for (int j = 0; j < m_cols - 1; j++)
                {
                    if (m_polygons[i, j].IsInside(point))
                    {
                        container = m_polygons[i, j];

                        polygonRow = i;
                        polygonCol = j;

                        done = true;
                        break;
                    }
                }

                if (done)
                {
                    break;
                }
            }

            if (container != null)
            {
                MatrixH homography = m_homographies[container];
                if (homography != null)
                {
                    System.Drawing.PointF originalPt = new System.Drawing.PointF((float)point.X, (float)point.Y);
                    System.Drawing.PointF tempPt = homography.TransformPoints(originalPt)[0];

                    Point transformedPt = new Point(tempPt.X, tempPt.Y);

                    if (Constants.DebugPrint)
                    {
                        Debug.WriteLine(point + " -> " + transformedPt);
                    }

                    if (m_direction == ModelDirection.Portrait)
                    {
                        transformedPt = new Point(transformedPt.Y, 1.0 - transformedPt.X);
                    }

                    return transformedPt;
                }
            }

            if (Constants.DebugPrint)
            {
                Debug.WriteLine(point + ": (" + polygonRow + ", " + polygonCol + ")");
            }

            return null;
        }
        #endregion

        #region Read Methods
        public void Load(string fileName)
        {
            FileStream strm = new FileStream(fileName, FileMode.Open);
            StreamReader reader = new StreamReader(strm);

            // read header
            string rawLine = reader.ReadLine();
            int counter = 0;

            while ((rawLine = reader.ReadLine()) != null)
            {
                string[] elements = rawLine.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                double parameter1 = Convert.ToDouble(elements[1]);
                double parameter2 = Convert.ToDouble(elements[2]);

                Line2D line = new Line2D(parameter1, parameter2, counter < m_rows);
                if (counter < m_rows)
                {
                    m_rowLines.Add(line);
                }
                else
                {
                    m_columnLines.Add(line);
                }

                counter++;
            }

            // now do the intersections
            for (int i = 0; i < m_rows; i++)
            {
                Line2D row = m_rowLines[i];
                for (int j = 0; j < m_cols; j++)
                {
                    Line2D column = m_columnLines[j];
                    Point? intersection = row.IntersectWith(column);

                    if (intersection != null)
                    {
                        m_gridPoints[i, j] = intersection.Value;
                    }
                }
            }

            // now fill the polygons
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            for (int i = 0; i < m_rows - 1; i++)
            {
                for (int j = 0; j < m_cols - 1; j++)
                {
                    m_polygons[i, j] = new Polygon2D();
                    m_polygons[i, j].Points.Add(m_gridPoints[i, j]);
                    m_polygons[i, j].Points.Add(m_gridPoints[i, j + 1]);
                    m_polygons[i, j].Points.Add(m_gridPoints[i + 1, j + 1]);
                    m_polygons[i, j].Points.Add(m_gridPoints[i + 1, j]);

                    if (Constants.DebugPrint)
                    {
                        Debug.WriteLine("\tPolygon:");
                        Debug.WriteLine("\t\t" + m_polygons[i, j].Points[0]);
                        Debug.WriteLine("\t\t" + m_polygons[i, j].Points[1]);
                        Debug.WriteLine("\t\t" + m_polygons[i, j].Points[2]);
                        Debug.WriteLine("\t\t" + m_polygons[i, j].Points[3]);
                    }

                    Accord.Point[] templatePts = new Accord.Point[4];
                    templatePts[0] = new Accord.Point(j / (float)(m_cols - 1), i / (float)(m_rows - 1));
                    templatePts[1] = new Accord.Point((j + 1) / (float)(m_cols - 1), i / (float)(m_rows - 1));
                    templatePts[2] = new Accord.Point((j + 1) / (float)(m_cols - 1), (i + 1) / (float)(m_rows - 1));
                    templatePts[3] = new Accord.Point(j / (float)(m_cols - 1), (i + 1) / (float)(m_rows - 1));

                    Accord.Point[] realPoints = new Accord.Point[4];
                    for (int k = 0; k < 4; k++)
                    {
                        realPoints[k] = new Accord.Point((float)m_polygons[i, j].Points[k].X, (float)m_polygons[i, j].Points[k].Y);
                    }

                    Accord.Point[][] matchedPoints = new Accord.Point[2][];
                    matchedPoints[1] = templatePts;
                    matchedPoints[0] = realPoints;

                    MatrixH homography = ransac.Estimate(matchedPoints);
                    m_homographies.Add(m_polygons[i, j], homography);
                }
            }

            reader.Close();
            reader.Dispose();
            reader = null;

            strm.Close();
            strm.Dispose();
            strm = null;
        }
        #endregion
    }
    #endregion
}
