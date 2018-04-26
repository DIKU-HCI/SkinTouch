using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

using SkinTouch.Entities;
using SkinTouch.Geometry;
using SkinTouch.Visuals;

namespace SkinTouch.SkinTouch3D
{
    #region Class 'ArmModel'
    public class ArmModel : ModelVisual3D
    {
        #region Class Members
        #region Control Class Members
        private Arm m_arm;

        private DisplaySettings m_settings = new DisplaySettings();
        #endregion

        #region Visual Class Members
        private bool m_useTexture = true;

        private Model3DGroup m_model;

        private GeometryModel3D m_armModel;

        private MeshGeometry3D m_mesh;

        private CoordinateSystem m_forearmSystem;

        private CoordinateSystem m_wristSystem;
        #endregion

        #region Vertex Class Members
        private List<Point3D[]> m_ellipsePts = new List<Point3D[]>();
        #endregion

        #region Texture Class Members
        private Material m_outsideMaterial;

        private Material m_insideMaterial;

        private ImageSource m_texture;
        #endregion

        #region Model/Touch Class Members
        private List<Tuple<Brush, Point[,]>> m_grids = null;

        private Tuple<Brush, Point> m_projectedPoint = null;
        #endregion
        #endregion

        #region Constructors
        internal ArmModel(Arm arm)
            : base()
        {
            m_arm = arm;
            m_grids = new List<Tuple<Brush, Point[,]>>();

            m_settings.ValueChanged += SettingsValueChanged;

            InitializeVisuals();
            InitializeTexture();
        }
        #endregion

        #region Initialization
        private void InitializeVisuals()
        {
            m_mesh = new MeshGeometry3D();
            m_ellipsePts.Clear();

            double armRadius = m_arm.Radius;
            double diffMajor = armRadius - m_arm.WristWidth / 2.0;
            double diffMinor = armRadius - m_arm.WristHeight / 2.0;

            // create the first and last points
            List<Point3D> forearmPts = VertexGenerator.GenerateCirclePoints(
                armRadius, Constants.NumberOfArmShapeSegments, true);
            List<Point3D> wristPts = VertexGenerator.GenerateEllipsePoints(
                m_arm.WristWidth / 2.0, m_arm.WristHeight / 2.0, 
                Constants.NumberOfArmShapeSegments, true);

            // create the remaining points (in between)
            m_ellipsePts.Add(forearmPts.ToArray());
            for (int i = 1; i < Constants.NumberOfArmLengthSegments; i++)
            {
                List<Point3D> ellipsePoints = new List<Point3D>();
                for (int j = 0; j < forearmPts.Count; j++)
                {
                    double correctX = forearmPts[j].X - i
                        * (forearmPts[j].X - wristPts[j].X) / Constants.NumberOfArmLengthSegments;
                    double correctY = forearmPts[j].Y - i
                        * (forearmPts[j].Y - wristPts[j].Y) / Constants.NumberOfArmLengthSegments;

                    ellipsePoints.Add(new Point3D(correctX, correctY, 0.0));
                }

                m_ellipsePts.Add(ellipsePoints.ToArray());
            }
            m_ellipsePts.Add(wristPts.ToArray());

            // add positions and triangle indices
            for (int i = 0; i <= Constants.NumberOfArmLengthSegments; i++)
            {
                for (int j = 0; j <= Constants.NumberOfArmShapeSegments; j++)
                {
                    m_mesh.Positions.Add(m_ellipsePts[i][j]);

                    double textureX = i / (double)Constants.NumberOfArmLengthSegments;
                    double textureY = j / (double)Constants.NumberOfArmShapeSegments;

                    m_mesh.TextureCoordinates.Add(new Point(textureX, textureY));
                }
            }

            for (int i = 0; i < Constants.NumberOfArmLengthSegments; i++)
            {
                for (int j = 0; j < Constants.NumberOfArmShapeSegments; j++)
                {
                    m_mesh.TriangleIndices.Add(j + i * (Constants.NumberOfArmShapeSegments + 1));
                    m_mesh.TriangleIndices.Add(j + 1 + (i + 1) * (Constants.NumberOfArmShapeSegments + 1));
                    m_mesh.TriangleIndices.Add(j + (i + 1) * (Constants.NumberOfArmShapeSegments + 1));

                    m_mesh.TriangleIndices.Add(j + i * (Constants.NumberOfArmShapeSegments + 1));
                    m_mesh.TriangleIndices.Add(j + 1 + i * (Constants.NumberOfArmShapeSegments + 1));
                    m_mesh.TriangleIndices.Add(j + 1 + (i + 1) * (Constants.NumberOfArmShapeSegments + 1));
                }
            }

            // create the model
            if (m_armModel == null)
            {
                m_armModel = new GeometryModel3D();
            }
            m_armModel.Geometry = m_mesh;

            if (m_model == null)
            {
                m_model = new Model3DGroup();
            }

            m_model.Children.Clear();
            m_model.Children.Add(m_armModel);

            // add the two marker coordinate systems
            m_forearmSystem = new CoordinateSystem(0.05 * 1000.0, true, true, false, 0.5);
            m_wristSystem = new CoordinateSystem(0.05 * 1000.0, true, true, false, 0.5);

            m_model.Children.Add(m_forearmSystem.Model);
            m_model.Children.Add(m_wristSystem.Model);

            // add all models
            Content = m_model;
        }

        private void InitializeTexture()
        {
            if (!(UseTexture))
            {
                m_outsideMaterial = new DiffuseMaterial(Brushes.DarkGray);
                m_armModel.Material = m_outsideMaterial;
            }
            else
            {
                m_texture = CreateGenericTexture();

                m_outsideMaterial = new DiffuseMaterial(new ImageBrush(m_texture));
                m_armModel.Material = m_outsideMaterial;
            }

            m_insideMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 96, 96, 96)));
            m_armModel.BackMaterial = m_insideMaterial;
        }

        private ImageSource CreateGenericTexture(bool colored = false)
        {
            bool showUlna = (m_settings != null && m_settings.ShowUlna);
            bool showGuides = (m_settings != null && m_settings.ShowGuides);

            int textureWidth = 1024;
            int textureHeight = 1024;

            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            context.DrawRectangle(new SolidColorBrush(Constants.Colors.SkinTextureColor), null,
                new Rect(0.0, 0.0, textureWidth, textureHeight));

            double lineThickness = 10.0;

            for (int i = 0; i <= 4; i++)
            {
                Pen dashedPen = new Pen(Brushes.DarkGray, lineThickness / 2.0);
                DashStyle dashStyle = new DashStyle(new double[] { 4.0, 8.0 }, 0.0);

                if (i % 4 == 0 && m_settings.ShowUlna)
                {
                    dashedPen.Brush = Brushes.DarkRed;
                }
                else
                {
                    dashedPen.DashStyle = dashStyle;
                }

                if (colored)
                {
                    switch (i)
                    {
                        default:
                        case 0:
                            dashedPen.Brush = Brushes.Red;
                            break;
                        case 1:
                            dashedPen.Brush = Brushes.Green;
                            break;
                        case 2:
                            dashedPen.Brush = Brushes.Blue;
                            break;
                        case 3:
                            dashedPen.Brush = Brushes.Yellow;
                            break;
                    }
                }

                if ((i % 4 == 0 && showUlna)
                    || (i % 4 != 0 && showGuides))
                {
                    context.DrawLine(dashedPen, new Point(0.0, i / 4.0 * textureHeight + 0.0 * textureHeight),
                       new Point(textureWidth, i / 4.0 * textureHeight + 0.0 * textureHeight));
                    context.DrawLine(dashedPen, new Point(0.0, i / 4.0 * textureHeight + 0.0 * textureHeight - textureHeight),
                        new Point(textureWidth, i / 4.0 * textureHeight + 0.0 * textureHeight - textureHeight));
                    context.DrawLine(dashedPen, new Point(0.0, i / 4.0 * textureHeight + 0.0 * textureHeight + textureHeight),
                        new Point(textureWidth, i / 4.0 * textureHeight + 0.0 * textureHeight + textureHeight));
                }
            }

            context.Close();

            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                textureWidth, textureHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            return bitmap;
        }
        #endregion

        #region Properties
        public bool UseTexture
        {
            get { return m_useTexture; }
        }

        public DisplaySettings DisplaySettings
        {
            get { return m_settings; }
            set
            {
                if (m_settings != null)
                {
                    m_settings.ValueChanged -= SettingsValueChanged;
                }

                m_settings = value;
                if (m_settings != null)
                {
                    m_settings.ValueChanged += SettingsValueChanged;
                    UpdateTexture();
                }
            }
        }
        #endregion

        #region Update Methods
        internal void UpdateVisuals()
        {
            InitializeVisuals();
            UpdateTexture();
            Update();
        }
        #endregion

        #region Texture Methods
        private void UpdateTexture()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    delegate ()
                    {
                        bool showUlna = (m_settings != null && m_settings.ShowUlna);
                        bool showGuides = (m_settings != null && m_settings.ShowGuides);

                        int textureWidth = 1024;
                        int textureHeight = 1024;

                        DrawingVisual visual = new DrawingVisual();
                        DrawingContext context = visual.RenderOpen();

                        context.DrawRectangle(new SolidColorBrush(Constants.Colors.SkinTextureColor),
                            null, new Rect(0.0, 0.0, textureWidth, textureHeight));

                        // draw the guides
                        double lineThickness = 10.0;
                        for (int i = 0; i <= 4; i++)
                        {
                            Pen dashedPen = new Pen(Brushes.DarkGray, lineThickness / 4.0);
                            DashStyle dashStyle = new DashStyle(new double[] { 4.0, 8.0 }, 0.0);

                            if (i % 4 == 0 && showUlna)
                            {
                                dashedPen.Brush = Brushes.DarkRed;
                            }
                            else
                            {
                                dashedPen.DashStyle = dashStyle;
                            }

                            if ((i % 4 == 0 && showUlna)
                                || (i % 4 != 0 && showGuides))
                            {
                                context.DrawLine(dashedPen, new Point(0.0, i / 4.0 * textureHeight + 0.0 * textureHeight),
                                        new Point(textureWidth, i / 4.0 * textureHeight + 0.0 * textureHeight));
                                context.DrawLine(dashedPen, new Point(0.0, i / 4.0 * textureHeight + 0.0 * textureHeight - textureHeight),
                                    new Point(textureWidth, i / 4.0 * textureHeight + 0.0 * textureHeight - textureHeight));
                                context.DrawLine(dashedPen, new Point(0.0, i / 4.0 * textureHeight + 0.0 * textureHeight + textureHeight),
                                    new Point(textureWidth, i / 4.0 * textureHeight + 0.0 * textureHeight + textureHeight));
                            }
                        }

                        // let's draw the grid
                        if (m_grids != null)
                        {
                            bool[] showModels = new bool[3];
                            showModels[0] = (m_settings != null && m_settings.ShowHorizontalModel);
                            showModels[1] = (m_settings != null && m_settings.ShowVerticalModel);
                            showModels[2] = (m_settings != null && m_settings.ShowTracingModel);
                            int counter = 0;

                            foreach (Tuple<Brush, Point[,]> grid in m_grids)
                            {
                                if (showModels[counter % 3])
                                {
                                    Pen linePen = new Pen(grid.Item1, 7.5);
                                    linePen.DashCap = PenLineCap.Round;

                                    int rows = grid.Item2.GetLength(0);
                                    int cols = grid.Item2.GetLength(1);

                                    // get the four edges
                                    List<Tuple<Point, Point>> edges = new List<Tuple<Point, Point>>();
                                    edges.Add(new Tuple<Point, Point>(grid.Item2[0, 0], grid.Item2[0, cols - 1]));
                                    edges.Add(new Tuple<Point, Point>(grid.Item2[0, cols - 1], grid.Item2[rows - 1, cols - 1]));
                                    edges.Add(new Tuple<Point, Point>(grid.Item2[rows - 1, cols - 1], grid.Item2[rows - 1, 0]));
                                    edges.Add(new Tuple<Point, Point>(grid.Item2[rows - 1, 0], grid.Item2[0, 0]));

                                    List<Point> polygonPts = new List<Point>();
                                    foreach (Tuple<Point, Point> edge in edges)
                                    {
                                        Point correctPt = new Point(edge.Item1.X * textureWidth, edge.Item1.Y * textureHeight);
                                        polygonPts.Add(correctPt);
                                    }

                                    PathFigure polygonFigure = new PathFigure();
                                    polygonFigure.StartPoint = polygonPts[0];
                                    polygonFigure.Segments.Add(new LineSegment(polygonPts[1], true));
                                    polygonFigure.Segments.Add(new LineSegment(polygonPts[2], true));
                                    polygonFigure.Segments.Add(new LineSegment(polygonPts[3], true));
                                    polygonFigure.Segments.Add(new LineSegment(polygonPts[0], true));
                                    polygonFigure.IsClosed = true;

                                    PathGeometry polygon = new PathGeometry();
                                    polygon.Figures.Add(polygonFigure);

                                    context.DrawGeometry(new SolidColorBrush(
                                        Color.FromArgb(32, ((SolidColorBrush)grid.Item1).Color.R,
                                            ((SolidColorBrush)grid.Item1).Color.G,
                                            ((SolidColorBrush)grid.Item1).Color.B)), linePen, polygon);

                                    // add the grid lines
                                    Pen gridPen = new Pen(grid.Item1, 4.0);
                                    gridPen.DashCap = PenLineCap.Round;
                                    DashStyle dashStyle = new DashStyle(new double[] { 2.0, 4.0 }, 0.0);
                                    gridPen.DashStyle = dashStyle;

                                    // horizontal first
                                    for (int i = 1; i < cols - 1; i++)
                                    {
                                        Point startPt = new Point(grid.Item2[0, i].X * textureWidth, grid.Item2[0, i].Y * textureHeight);
                                        Point endPt = new Point(grid.Item2[rows - 1, i].X * textureWidth, grid.Item2[rows - 1, i].Y * textureHeight);

                                        context.DrawLine(gridPen, startPt, endPt);
                                    }

                                    // now vertical
                                    for (int i = 1; i < rows - 1; i++)
                                    {
                                        Point startPt = new Point(grid.Item2[i, 0].X * textureWidth, grid.Item2[i, 0].Y * textureHeight);
                                        Point endPt = new Point(grid.Item2[i, cols - 1].X * textureWidth, grid.Item2[i, cols - 1].Y * textureHeight);

                                        context.DrawLine(gridPen, startPt, endPt);
                                    }
                                }

                                counter++;
                            }
                        }

                        if (m_projectedPoint != null)
                        {
                            double size = 30.0;

                            Point correctPoint = new Point(m_projectedPoint.Item2.X * textureWidth,
                                m_projectedPoint.Item2.Y * textureHeight);

                            Brush pointBrush = new SolidColorBrush(Color.FromArgb(
                                128, ((SolidColorBrush)m_projectedPoint.Item1).Color.R,
                                    ((SolidColorBrush)m_projectedPoint.Item1).Color.G,
                                    ((SolidColorBrush)m_projectedPoint.Item1).Color.B));
                            Pen pointPen = new Pen(m_projectedPoint.Item1, 5.0);

                            context.DrawEllipse(pointBrush, pointPen, correctPoint, size / 2.0, size / 2.0);
                        }

                        context.Close();

                        m_texture = new RenderTargetBitmap(
                            textureWidth, textureHeight, 96.0, 96.0, PixelFormats.Pbgra32);
                        ((RenderTargetBitmap)m_texture).Render(visual);

                        m_outsideMaterial = new DiffuseMaterial(new ImageBrush(m_texture));
                        m_armModel.Material = m_outsideMaterial;
                    }));
        }
        #endregion

        #region Model/Touch Methods
        private int GetGridIndex(Point[,] points)
        {
            int index = -1;
            for (int i = 0; i < m_grids.Count; i++)
            {
                if (m_grids[i] != null
                    && m_grids[i].Item2 != null
                    && m_grids[i].Item2.Equals(points))
                {
                    return i;
                }
            }

            return index;
        }

        public void AddGrid(Point[,] grid, Color color)
        {
            AddGrid(grid, new SolidColorBrush(color));
        }

        public void AddGrid(Point[,] grid, Brush brush)
        {
            lock (this)
            {
                if (GetGridIndex(grid) == -1)
                {
                    m_grids.Add(new Tuple<Brush, Point[,]>(brush, grid));
                    UpdateTexture();
                }
            }
        }

        public void RemoveGrid(Point[,] grid)
        {
            lock (this)
            {
                int index = GetGridIndex(grid);
                if (index != -1)
                {
                    m_grids.RemoveAt(index);
                    UpdateTexture();
                }
            }
        }

        public void SetProjectedTouchPoint(Point projectedTouchPoint, Brush brush)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_projectedPoint = new Tuple<Brush, Point>(brush, projectedTouchPoint);
                    UpdateTexture();
                }));
        }

        public void ClearProjectedTouchPoint()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_projectedPoint = null;
                    UpdateTexture();
                }));
        }
        #endregion

        #region Update Methods
        internal void Update()
        {
            m_forearmSystem.Model.Transform = m_arm.ForearmTransform;
            m_wristSystem.Model.Transform = m_arm.WristTransform;

            // now, distribute the circle/ellipses
            Quaternion adjustedForearm = m_arm.ForearmOrientation
                * (new Quaternion(new Vector3D(0.0, 0.0, 1.0), -90.0));

            for (int i = 0; i <= Constants.NumberOfArmLengthSegments; i++)
            {
                double factor = (double)i / Constants.NumberOfArmLengthSegments;

                Point3D center = m_arm.ForearmCenter + factor * (m_arm.WristCenter - m_arm.ForearmCenter);
                Quaternion orientation = Quaternion.Slerp(adjustedForearm, m_arm.WristOrientation, factor);

                Transform3DGroup transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(orientation)));
                transformGroup.Children.Add(new TranslateTransform3D(center.X, center.Y, center.Z));

                MatrixTransform3D transform = new MatrixTransform3D(transformGroup.Value);
                for (int j = 0; j <= Constants.NumberOfArmShapeSegments; j++)
                {
                    Point3D transformedPt = transform.Transform(m_ellipsePts[i][j]);
                    m_mesh.Positions[j + i * (Constants.NumberOfArmShapeSegments + 1)] = transformedPt;
                }
            }
        }
        #endregion

        #region Event Handler
        private void SettingsValueChanged(object sender, EventArgs e)
        {
            UpdateTexture();
        }
        #endregion
    }
    #endregion
}
