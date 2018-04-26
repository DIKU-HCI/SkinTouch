using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SkinTouch.Mapping.UI
{
    #region Class 'Renderer'
    public class Renderer
    {
        #region Static Class Members
        private static readonly bool PrintMouseState = false;
        #endregion

        #region Class Members
        #region Visual Class Members
        private Canvas m_viewportParent;

        private Viewport3D m_viewport;
        #endregion

        #region Camera Class Members
        private PerspectiveCamera m_camera;

        private Transform3D m_cameraTransform;

        private Vector3D m_cameraTargetPosition;

        private double m_cameraDistance = Math.Sqrt(2.0) * 1000.0;

        private double m_yaw = 0.0;

        private double m_pitch = 0.0;

        private double m_roll = 0.0;
        #endregion

        #region Mouse Class Members
        private bool m_leftMouseButtonDown = false;

        private bool m_rightMouseButtonDown = false;

        private Point m_lastPoint = new Point(0.0, 0.0);
        #endregion
        #endregion

        #region Constructors
        public Renderer(Viewport3D viewport)
        {
            m_viewport = viewport;
            m_viewportParent = (Canvas)m_viewport.Parent;

            InitializeVisuals();
            InitializeEvents();
        }
        #endregion

        #region Initialization
        private void InitializeVisuals()
        {
            ResetCamera();

            // light
            AmbientLight light = new AmbientLight(Colors.White);

            ModelVisual3D lightModel = new ModelVisual3D();
            lightModel.Content = light;

            m_viewport.Children.Add(lightModel);
        }

        private void InitializeEvents()
        {
            m_viewportParent.MouseDown += ViewportMouseDown;
            m_viewportParent.MouseMove += ViewportMouseMove;
            m_viewportParent.MouseUp += ViewportMouseUp;
            m_viewportParent.MouseWheel += ViewportParentMouseWheel;
        }
        #endregion

        #region Add/Remove Methods
        public void AddElement(ModelVisual3D element)
        {
            if (!(m_viewport.Children.Contains(element)))
            {
                m_viewport.Children.Add(element);
            }
        }

        public void RemoveElement(ModelVisual3D element)
        {
            if (m_viewport.Children.Contains(element))
            {
                m_viewport.Children.Remove(element);
            }
        }
        #endregion

        #region Camera Methods
        public void ResetCamera()
        {
            // do some math
            m_pitch = 40.0;
            m_yaw = 35.0;
            m_roll = 0.0;

            m_cameraTargetPosition = new Vector3D(0.0, 1250.0, 0.0);
            m_cameraDistance += 1000.0;

            // now transform
            TransformCamera();
        }
        #endregion

        #region Transformation Methods
        private void TransformCamera()
        {
            Vector3D cameraLookVect = new Vector3D(0.0, 0.0, 1.0);
            Vector3D cameraUpVect = new Vector3D(0.0, 1.0, 0.0);

            Transform3DGroup cameraTransform = new Transform3DGroup();
            cameraTransform.Children.Add(new TranslateTransform3D(new Vector3D(0.0, 0.0, -(m_cameraDistance))));
            cameraTransform.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(new Quaternion(new Vector3D(1.0, 0.0, 0.0), m_pitch))));
            cameraTransform.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(new Quaternion(new Vector3D(0.0, 1.0, 0.0), m_yaw))));
            cameraTransform.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(new Quaternion(new Vector3D(0.0, 0.0, 1.0), m_roll))));

            // TODO: fix this
            cameraTransform.Children.Add(new TranslateTransform3D(m_cameraTargetPosition));

            m_cameraTransform = cameraTransform;

            Point3D cameraPosition = cameraTransform.Transform(new Point3D(0.0, 0.0, 0.0));
            Vector3D lookDirection = cameraTransform.Transform(cameraLookVect);
            Vector3D upDirection = cameraTransform.Transform(cameraUpVect);

            if (m_camera == null)
            {
                Debug.WriteLine("Camera:");
                Debug.WriteLine("\tPosition:     " + cameraPosition);
                Debug.WriteLine("\tLook Vector:  " + lookDirection);
                Debug.WriteLine("\tUp: Vector:   " + upDirection);

                m_camera = new PerspectiveCamera(
                    cameraPosition, lookDirection, upDirection, 45.0);
                m_camera.NearPlaneDistance = 0.01;
                m_camera.FarPlaneDistance = 10000.0;

                m_viewport.Camera = m_camera;
            }
            else
            {
                m_camera.Position = cameraPosition;
                m_camera.LookDirection = lookDirection;
                m_camera.UpDirection = upDirection;
            }
        }
        #endregion

        #region Event Handler
        #region Mouse Event Handler
        // left rotates (x -> around y, y -> around x), right moves
        private void ViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(m_leftMouseButtonDown)
                && !(m_rightMouseButtonDown))
            {
                switch (e.ChangedButton)
                {
                    default:
                    case MouseButton.Left:
                        m_leftMouseButtonDown = true;
                        break;
                    case MouseButton.Right:
                        m_rightMouseButtonDown = true;
                        break;
                }

                m_lastPoint = e.GetPosition(m_viewportParent);

                if (PrintMouseState)
                {
                    Debug.WriteLine("Mouse Down ["
                        + (m_leftMouseButtonDown ? "LEFT" : "RIGHT") + "]");
                }
            }
        }

        private void ViewportMouseMove(object sender, MouseEventArgs e)
        {
            if (m_leftMouseButtonDown)
            {
                Point currPoint = e.GetPosition(m_viewportParent);
                Vector movementVect = currPoint - m_lastPoint;

                // transform the movement vector
                Vector3D transformedMovementVect = m_cameraTransform.Transform(new Vector3D(movementVect.X, movementVect.Y, 0.0));
                m_cameraTargetPosition += transformedMovementVect;

                TransformCamera();

                m_lastPoint = currPoint;

                if (PrintMouseState)
                {
                    Debug.WriteLine("Mouse Move [LEFT]: " + movementVect);
                }
            }
            else if (m_rightMouseButtonDown)
            {
                Point currPoint = e.GetPosition(m_viewportParent);
                Vector movementVect = currPoint - m_lastPoint;

                m_pitch += movementVect.Y / 2.0;
                m_yaw -= movementVect.X / 2.0;

                if (m_pitch > 360.0)
                {
                    m_pitch -= 360.0;
                }
                else if (m_pitch < 0.0)
                {
                    m_pitch += 360.0;
                }

                if (m_yaw > 360.0)
                {
                    m_yaw -= 360.0;
                }
                else if (m_yaw < 0.0)
                {
                    m_yaw += 360.0;
                }

                TransformCamera();

                m_lastPoint = currPoint;

                if (PrintMouseState)
                {
                    Debug.WriteLine("Mouse Move [RIGHT]: " + movementVect);
                }
            }
        }

        private void ViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_leftMouseButtonDown)
            {
                m_leftMouseButtonDown = false;

                if (PrintMouseState)
                {
                    Debug.WriteLine("Mouse Up [LEFT]");
                }
            }
            else if (m_rightMouseButtonDown)
            {
                m_rightMouseButtonDown = false;

                if (PrintMouseState)
                {
                    Debug.WriteLine("Mouse Up [RIGHT]");
                }
            }
        }

        private void ViewportParentMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(m_leftMouseButtonDown)
                && (!m_rightMouseButtonDown))
            {
                int sign = Math.Sign(e.Delta);

                m_cameraDistance -= sign * 50.0;
                m_cameraDistance = Math.Max(100.0, m_cameraDistance);

                TransformCamera();

                if (PrintMouseState)
                {
                    Debug.WriteLine("Mouse [WHEEL]: " + e.Delta);
                }
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
