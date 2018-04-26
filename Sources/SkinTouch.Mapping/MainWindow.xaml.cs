using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SkinTouch;
using SkinTouch.Visuals;

using SkinTouch.Mapping.UI;

namespace SkinTouch.Mapping
{
    #region Partial Class 'MainWindow'
    public partial class MainWindow : Window
    {
        #region Static Class Members
        internal static readonly Color InactiveTouchColor = Color.FromArgb(128, 200, 0, 0);

        internal static readonly Color ActiveTouchColor = Color.FromArgb(255, 0, 200, 0);

        internal static readonly Color HoverTouchColor = Color.FromArgb(192, 200, 200, 0);
        #endregion

        #region Class Members
        private SkinTouchBase m_ctrl;

        private DisplaySettings m_settings = new DisplaySettings();

        private Renderer m_renderer;

        private CoordinateSystem m_coordinateSystem;

        private Floor m_floor;

        private Stroke m_stroke;

        private Ellipse m_fingerCursor;

        private SkinTouchPoint m_currSkinPt = null;
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            InitializeTracking();
            InitializeVisuals();

            UpdateTexture();

            InitializeEvents();
        }
        #endregion

        #region Initialization
        private void InitializeTracking()
        {
            m_ctrl = new SkinTouchBase();
        }

        private void InitializeVisuals()
        {
            m_renderer = new Renderer(MainViewport);

            ModelVisual3D floorModel = new ModelVisual3D();
            m_floor = new Floor(new Size(1500.0, 1500.0), 4, 5, 0.5);
            floorModel.Content = m_floor.Model;

            m_renderer.AddElement(floorModel);

            ModelVisual3D coordinateSystemModel = new ModelVisual3D();
            m_coordinateSystem = new CoordinateSystem();
            coordinateSystemModel.Content = m_coordinateSystem.Model;

            m_renderer.AddElement(coordinateSystemModel);

            // add arm and finger
            m_renderer.AddElement(m_ctrl.Arm.VisualModel);
            m_renderer.AddElement(m_ctrl.Finger.VisualModel);

            m_ctrl.Arm.ActiveModel = ModelType.None;
            m_ctrl.Arm.VisualModel.DisplaySettings = m_settings;

            // cursor
            Color fingerCursorColor = Colors.Red;
            m_fingerCursor = new Ellipse();
            m_fingerCursor.Fill = new SolidColorBrush(Color.FromArgb(128,
                fingerCursorColor.R, fingerCursorColor.G, fingerCursorColor.B));
            m_fingerCursor.Stroke = new SolidColorBrush(fingerCursorColor);
            m_fingerCursor.StrokeThickness = 2.0;
            m_fingerCursor.Width = 30.0;
            m_fingerCursor.Height = 30.0;
            m_fingerCursor.Visibility = Visibility.Hidden;

            DemoCanvas.Children.Add(m_fingerCursor);
        }

        private void InitializeEvents()
        {
            m_ctrl.SkinTouchDown += OnSkinTouchDown;
            m_ctrl.SkinTouchMove += OnSkinTouchMove;
            m_ctrl.SkinTouchUp += OnSkinTouchUp;
            m_ctrl.SkinHover += OnSkinHover;
            m_ctrl.SkinInactive += OnSkinInactive;
        }
        #endregion

        #region Update Methods
        private void UpdateUI()
        {
            bool connected = m_ctrl.IsLive;

            IPAddressLabel.IsEnabled = !connected;
            IPAddressTextBox.IsEnabled = !connected;
            ConnectButton.Content = (connected ? "Disconnect" : "Connect");

            CircumferenceLabel.IsEnabled = !connected;
            CircumferenceTextBox.IsEnabled = !connected;
            WristWidthLabel.IsEnabled = !connected;
            WristWidthTextBox.IsEnabled = !connected;
            WristHeightLabel.IsEnabled = !connected;
            WristHeightTextBox.IsEnabled = !connected;
            UpdateButton.IsEnabled = !connected;
        }

        private void UpdateSettings()
        {
            if (m_settings != null)
            {
                m_settings.ShowUlna = UlnaDisplayCheckBox.IsChecked.Value;
                m_settings.ShowGuides = GuidesDisplayCheckBox.IsChecked.Value;
                m_settings.ShowHorizontalModel = HorizontalModelDisplayCheckBox.IsChecked.Value;
                m_settings.ShowVerticalModel = VerticalModelDisplayCheckBox.IsChecked.Value;
                m_settings.ShowTracingModel = TracingModelDisplayCheckBox.IsChecked.Value;
            }
        }

        private void ToggleControlPanel()
        {
            bool isVisible = (ControlPanel.Visibility == Visibility.Visible);
            if (isVisible)
            {
                ControlPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                ControlPanel.Visibility = Visibility.Visible;
            }
        }

        private void ToggleDemo()
        {
            bool isVisible = (DemoCanvas.Visibility == Visibility.Visible);
            if (isVisible)
            {
                DemoCanvas.Visibility = Visibility.Hidden;
                ControlPanel.Visibility = Visibility.Visible;
            }
            else
            {
                DemoCanvas.Visibility = Visibility.Visible;
                ControlPanel.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateTexture()
        {
            // Texture
            int textureWidth = (int)Texture.Width;
            int textureHeight = (int)Texture.Height;
            double touchPointSize = 10.0;

            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            Pen borderPen = new Pen(Brushes.White, 2.0);
            Brush backgroundBrush = new SolidColorBrush(Color.FromArgb(153, 0, 0, 0));

            context.DrawRectangle(backgroundBrush, borderPen,
                new Rect(0.0, 0.0, textureWidth, textureHeight));

            // draw grid
            int rows = 5;
            int cols = 5;

            if (GuidesDisplayCheckBox != null
                && GuidesDisplayCheckBox.IsChecked.Value)
            {
                Pen dashedPen = new Pen(Brushes.DarkGray, 0.5);
                DashStyle dashStyle = new DashStyle(new double[] { 4.0, 8.0 }, 0.0);
                dashedPen.DashStyle = dashStyle;

                for (int i = 1; i < rows - 1; i++)
                {
                    context.DrawLine(dashedPen, new Point(0.0, i / (double)(rows - 1) * textureHeight),
                        new Point(textureWidth, i / (double)(rows - 1) * textureHeight));
                }

                for (int i = 1; i < cols - 1; i++)
                {
                    context.DrawLine(dashedPen, new Point(i / (double)(cols - 1) * textureWidth, 0.0),
                           new Point(i / (double)(cols - 1) * textureWidth, textureHeight));
                }
            }

            if (m_currSkinPt != null
                && m_currSkinPt.EventType != SkinEventType.None)
            {
                // first, transform the point
                Point correctPt = new Point(m_currSkinPt.TransformedLocation.X * textureWidth,
                    m_currSkinPt.TransformedLocation.Y * textureHeight);

                Color pointColor = InactiveTouchColor;
                switch(m_currSkinPt.EventType)
                {
                    default:
                    case SkinEventType.None:
                    case SkinEventType.Up:
                        // do nothing here!
                        break;
                    case SkinEventType.Down:
                    case SkinEventType.Move:
                        pointColor = ActiveTouchColor;
                        break;
                    case SkinEventType.Hover:
                        pointColor = HoverTouchColor;
                        break;
                }
                
                context.DrawEllipse(new SolidColorBrush(Color.FromArgb(128, pointColor.R, pointColor.G, pointColor.B)),
                    new Pen(new SolidColorBrush(pointColor), 2.0), correctPt, 
                    touchPointSize / 2.0, touchPointSize / 2.0);
            }

            context.Close();

            ImageSource texture = new RenderTargetBitmap(
                textureWidth, textureHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            ((RenderTargetBitmap)texture).Render(visual);

            Texture.Source = texture;
        }
        #endregion

        #region Cursor Methods
        private void UpdateCursor()
        {
            if (m_currSkinPt != null)
            {
                double width = m_fingerCursor.Width;
                double height = m_fingerCursor.Height;

                switch (m_currSkinPt.EventType)
                {
                    default:
                    case SkinEventType.None:
                        m_fingerCursor.Visibility = Visibility.Hidden;
                        break;
                    case SkinEventType.Down:
                    case SkinEventType.Move:
                    case SkinEventType.Up:
                    case SkinEventType.Hover:
                        m_fingerCursor.Visibility = Visibility.Visible;
                        Canvas.SetLeft(m_fingerCursor, DemoCanvas.ActualWidth * m_currSkinPt.TransformedLocation.X - width / 2.0);
                        Canvas.SetTop(m_fingerCursor, DemoCanvas.ActualHeight * m_currSkinPt.TransformedLocation.Y - height / 2.0);
                        break;
                }
            }
        }
        #endregion

        #region Stroke Methods
        private Point ScalePointToCanvas(Point point)
        {
            return (new Point(DemoCanvas.ActualWidth * m_currSkinPt.TransformedLocation.X,
                DemoCanvas.ActualHeight * m_currSkinPt.TransformedLocation.Y));
        }

        public void BeginStroke(Point startPoint)
        {
            if (m_stroke != null)
            {
                DemoCanvas.Children.Remove(m_stroke.Path);
            }

            m_stroke = new Stroke(ScalePointToCanvas(startPoint));
            DemoCanvas.Children.Add(m_stroke.Path);
        }

        public void AddPointToStroke(Point newPoint)
        {
            if (m_stroke != null)
            {
                m_stroke.AddPoint(ScalePointToCanvas(newPoint));
            }
        }
        #endregion

        #region Event Handler
        #region Window Event Handler
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (m_ctrl != null)
            {
                m_ctrl.Stop();
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.H)
            {
                ToggleControlPanel();
            }
            else if (e.Key == Key.D)
            {
                ToggleDemo();
            }
            else
            {
                if (m_ctrl == null
                    || (!(m_ctrl.IsLive)))
                {
                    // left decreases angle, right increases
                    // with modifier = wrist, without modifier = arm

                    bool hasCtrlModifier = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                    bool hasShiftModifier = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                    int xDirection = 0;
                    int yDirection = 0;
                    int zDirection = 0;

                    switch (e.Key)
                    {
                        default:
                            // do nothing!
                            break;
                        case Key.NumPad1:
                            xDirection = -1;
                            break;
                        case Key.NumPad3:
                            xDirection = 1;
                            break;
                        case Key.NumPad2:
                            yDirection = -1;
                            break;
                        case Key.NumPad5:
                            yDirection = 1;
                            break;
                        case Key.NumPad4:
                            zDirection = -1;
                            break;
                        case Key.NumPad6:
                            zDirection = 1;
                            break;
                    }

                    Vector3D motion = new Vector3D(xDirection * (hasCtrlModifier ? 10.0 : 1.0),
                        yDirection * (hasCtrlModifier ? 10.0 : 1.0), zDirection * (hasCtrlModifier ? 10.0 : 1.0));
                    m_ctrl.Finger.Move(motion);

                    m_ctrl.Update();
                    UpdateTexture();
                }
            }
        }
        #endregion

        #region UI Event Handler
        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                bool controlWasLiveActive = m_ctrl.IsLive;
                if (controlWasLiveActive)
                {
                    m_ctrl.Stop();
                }
                else
                {
                    string ipAddress = IPAddressTextBox.Text;
                    m_ctrl.Start(ipAddress);
                }

                UpdateUI();
            }
        }

        private void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsLoaded
                && m_ctrl != null
                && (!(m_ctrl.IsLive)))
            {
                double circumference = Convert.ToDouble(CircumferenceTextBox.Text);
                double wristWidth = Convert.ToDouble(WristWidthTextBox.Text);
                double wristHeight = Convert.ToDouble(WristHeightTextBox.Text);

                m_ctrl.Arm.Circumference = circumference;
                m_ctrl.Arm.WristWidth = wristWidth;
                m_ctrl.Arm.WristHeight = wristHeight;
            }
        }

        private void DisplayCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                UpdateSettings();
                UpdateTexture();
            }
        }

        private void UsageRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                if (sender is RadioButton
                    && ((RadioButton)sender).Tag != null
                    && ((RadioButton)sender).Tag is string)
                {
                    string tag = ((RadioButton)sender).Tag as string;
                    ModelType type = (ModelType)Enum.Parse(
                        typeof(ModelType), tag);

                    m_ctrl.Arm.ActiveModel = type;
                    m_ctrl.Update();

                    UpdateTexture();
                }
            }
        }
        #endregion

        #region SkinSkinTouch Event Handler
        private void OnSkinTouchDown(object sender, SkinEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_currSkinPt = e.Point;

                    UpdateCursor();
                    UpdateTexture();

                    BeginStroke(m_currSkinPt.TransformedLocation);
                }));
        }

        private void OnSkinTouchMove(object sender, SkinEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_currSkinPt = e.Point;

                    UpdateCursor();
                    UpdateTexture();

                    AddPointToStroke(m_currSkinPt.TransformedLocation);
                }));
        }

        private void OnSkinTouchUp(object sender, SkinEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_currSkinPt = e.Point;

                    UpdateCursor();
                    UpdateTexture();

                    AddPointToStroke(m_currSkinPt.TransformedLocation);
                }));
        }

        private void OnSkinHover(object sender, SkinEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_currSkinPt = e.Point;

                    UpdateCursor();
                    UpdateTexture();
                }));
        }

        private void OnSkinInactive(object sender, SkinEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_currSkinPt = e.Point;

                    UpdateCursor();
                    UpdateTexture();
                }));
        }
        #endregion
        #endregion
    }
    #endregion
}
