using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using NatNetML;

using SkinTouch.Devices;
using SkinTouch.Models;
using SkinTouch.SkinTouch3D;

namespace SkinTouch.Entities
{
    #region Class 'Arm'
    public class Arm
    {
        #region Class Members
        #region Tracking Class Members
        private Point3D m_forearmPosition;

        private Quaternion m_forearmOrientation;

        private Point3D m_wristPosition;

        private Quaternion m_wristOrientation;
        #endregion

        #region Physical Class Members
        private double m_armCircumference = Constants.Entities.DefaultArmCircumference;

        private double m_wristWidth = Constants.Entities.DefaultWristWidth;

        private double m_wristHeight = Constants.Entities.DefaultWristHeight;

        private Point3D m_forearmCenter;

        private Point3D m_wristCenter;
        #endregion

        #region Transform Class Members
        private Transform3D m_forearmTransform;

        private Transform3D m_wristTransform;
        #endregion

        #region Model Class Members
        private Dictionary<ModelType, LinearModel> m_displayedModels;

        private ModelType m_activeModel = ModelType.None;
        #endregion

        #region Offline Class Members
        private double m_forearmAngle = 225.0;

        private double m_wristAngle = 180.0;
        #endregion

        #region Visual Class Members
        private ArmModel m_model;
        #endregion
        #endregion

        #region Events
        internal event SkinEventHandler SkinTouchDown;

        internal event SkinEventHandler SkinTouchMove;

        internal event SkinEventHandler SkinTouchUp;

        internal event SkinEventHandler SkinHover;

        internal event SkinEventHandler SkinInactive;

        internal event RawDataEventHandler RawData;
        #endregion

        #region Constructors
        public Arm()
            : this(Constants.Entities.DefaultArmCircumference,
                  Constants.Entities.DefaultWristWidth, 
                  Constants.Entities.DefaultWristHeight)
        { }

        public Arm(double armCircumference, double wristWidth, double wristHeight)
        {
            m_armCircumference = armCircumference;
            m_wristWidth = wristWidth;
            m_wristHeight = wristHeight;

            m_displayedModels = new Dictionary<ModelType, LinearModel>();

            InitializeVisuals();
            InitializeOfflinePosition();

            Update();
        }
        #endregion

        #region Initialization
        private void InitializeVisuals()
        {
            m_model = new ArmModel(this);
        }

        private void InitializeOfflinePosition()
        {
            // NOTE: arm is centered in origin, along z-axis (forward)
            double armRadius = Radius;

            // create forearm position & orientation
            ForearmOrientation = new Quaternion(new Vector3D(0.0, 0.0, 1.0), m_forearmAngle);

            Transform3DGroup forearmTransform = new Transform3DGroup();
            forearmTransform.Children.Add(new TranslateTransform3D(
                new Vector3D(0.0, armRadius, 0.0)));
            forearmTransform.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(ForearmOrientation)));
            forearmTransform.Children.Add(new TranslateTransform3D(
                new Vector3D(0.0, 1250.0, 0.0)));

            ForearmPosition = forearmTransform.Transform(new Point3D(0.0, 0.0, 0.0));

            // create wrist position & orientation
            WristOrientation = new Quaternion(new Vector3D(0.0, 0.0, 1.0), m_wristAngle);

            Transform3DGroup wristTransform = new Transform3DGroup();
            wristTransform.Children.Add(new TranslateTransform3D(
                new Vector3D(WristWidth / 2.0, -WristHeight / 2.0, Constants.Entities.DefaultArmLength)));
            wristTransform.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(WristOrientation)));
            wristTransform.Children.Add(new TranslateTransform3D(
                new Vector3D(0.0, 1250.0, 0.0)));

            WristPosition = wristTransform.Transform(new Point3D(0.0, 0.0, 0.0));
        }
        #endregion

        #region Properties
        #region Tracking Properties
        public Point3D ForearmPosition
        {
            get { return m_forearmPosition; }
            internal set { m_forearmPosition = value; }
        }

        public Quaternion ForearmOrientation
        {
            get { return m_forearmOrientation; }
            internal set { m_forearmOrientation = value; }
        }

        public Point3D WristPosition
        {
            get { return m_wristPosition; }
            internal set { m_wristPosition = value; }
        }

        public Quaternion WristOrientation
        {
            get { return m_wristOrientation; }
            internal set { m_wristOrientation = value; }
        }

        public Point3D ForearmCenter
        {
            get { return m_forearmCenter; }
        }

        public Point3D WristCenter
        {
            get { return m_wristCenter; }
        }
        #endregion

        #region Transform Properties
        public Transform3D ForearmTransform
        {
            get { return m_forearmTransform; }
        }

        public Transform3D WristTransform
        {
            get { return m_wristTransform; }
        }
        #endregion

        #region Model Prperties
        public ModelType ActiveModel
        {
            get { return m_activeModel; }
            set { m_activeModel = value; }
        }
        #endregion

        #region Physical Properties
        public double WristWidth
        {
            get { return m_wristWidth; }
            set
            {
                m_wristWidth = value;

                InitializeOfflinePosition();
                m_model.UpdateVisuals();
                Update();
            }
        }

        public double WristHeight
        {
            get { return m_wristHeight; }
            set
            {
                m_wristHeight = value;

                InitializeOfflinePosition();
                m_model.UpdateVisuals();
                Update();
            }
        }

        public double Circumference
        {
            get { return m_armCircumference; }
            set
            {
                m_armCircumference = value;

                InitializeOfflinePosition();
                m_model.UpdateVisuals();
                Update();
            }
        }

        public double Radius
        {
            get { return Circumference / (2.0 * Math.PI); }
        }
        #endregion

        #region Visual Properties
        public ArmModel VisualModel
        {
            get { return m_model; }
        }
        #endregion
        #endregion

        #region Model/Touch Methods
        internal void AddModel(LinearModel model, ModelType type, Color color)
        {
            lock (this)
            {
                if (!(m_displayedModels.ContainsKey(type)))
                {
                    m_displayedModels.Add(type, model);
                    m_model.AddGrid(model.GridPoints, color);
                }
            }
        }

        internal void RemoveModel(LinearModel model, ModelType type)
        {
            lock (this)
            {
                if (m_displayedModels.ContainsKey(type))
                {
                    m_displayedModels.Remove(type);
                    m_model.RemoveGrid(model.GridPoints);
                }
            }
        }

        internal void SetProjectedTouchPoint(Point? projectedPoint, 
            RawDataEventType state = RawDataEventType.Outside)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate ()
                {
                    if (projectedPoint != null)
                    {
                        Color colorToSet = Constants.Colors.InactiveTouchColor;
                        switch (state)
                        {
                            default:
                            case RawDataEventType.Outside:
                            case RawDataEventType.WithinArm:
                                colorToSet = Constants.Colors.InactiveTouchColor;
                                break;
                            case RawDataEventType.Hover:
                                colorToSet = Constants.Colors.HoverTouchColor;
                                break;
                            case RawDataEventType.Touch:
                                colorToSet = Constants.Colors.ActiveTouchColor;
                                break;
                        }

                        m_model.SetProjectedTouchPoint(projectedPoint.Value,
                            new SolidColorBrush(colorToSet));
                    }
                    else
                    {
                        m_model.ClearProjectedTouchPoint();
                    }
                }));
        }
        #endregion

        #region Update Methods
        public void Update()
        {
            // change the coordinate systems
            Transform3DGroup forearmMarkerTransformGroup = new Transform3DGroup();
            forearmMarkerTransformGroup.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(m_forearmOrientation)));
            forearmMarkerTransformGroup.Children.Add(new TranslateTransform3D(
                m_forearmPosition.X, m_forearmPosition.Y, m_forearmPosition.Z));

            Transform3DGroup wristMarkerTransformGroup = new Transform3DGroup();
            wristMarkerTransformGroup.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(m_wristOrientation)));
            wristMarkerTransformGroup.Children.Add(new TranslateTransform3D(
                m_wristPosition.X, m_wristPosition.Y, m_wristPosition.Z));

            m_forearmTransform = forearmMarkerTransformGroup;
            m_wristTransform = wristMarkerTransformGroup;

            // calculate the local transformations
            double forearmRadius = m_armCircumference / (2.0 * Math.PI);

            Vector3D forearmOffsetVector = new Vector3D(0.0, -forearmRadius, 0.0);
            Vector3D wristOffsetVector = new Vector3D(-(m_wristWidth / 2.0), m_wristHeight / 2.0, 0.0);

            Transform3DGroup forearmTransformGroup = new Transform3DGroup();
            forearmTransformGroup.Children.Add(new TranslateTransform3D(
                forearmOffsetVector.X, forearmOffsetVector.Y, forearmOffsetVector.Z));
            forearmTransformGroup.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(m_forearmOrientation)));
            forearmTransformGroup.Children.Add(new TranslateTransform3D(
                m_forearmPosition.X, m_forearmPosition.Y, m_forearmPosition.Z));
            MatrixTransform3D forearmTransform = new MatrixTransform3D(forearmTransformGroup.Value);

            Transform3DGroup wristTransformGroup = new Transform3DGroup();
            wristTransformGroup.Children.Add(new TranslateTransform3D(
                wristOffsetVector.X, wristOffsetVector.Y, wristOffsetVector.Z));
            wristTransformGroup.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(m_wristOrientation)));
            wristTransformGroup.Children.Add(new TranslateTransform3D(
                m_wristPosition.X, m_wristPosition.Y, m_wristPosition.Z));
            MatrixTransform3D wristTransform = new MatrixTransform3D(wristTransformGroup.Value);

            m_forearmCenter = forearmTransform.Transform(new Point3D(0.0, 0.0, 0.0));  // now, it is in world coordinates
            m_wristCenter = wristTransform.Transform(new Point3D(0.0, 0.0, 0.0));      // now, it is in world coordinates

            if (Constants.DebugPrint)
            {
                Debug.WriteLine("Forearm [CENTER]:  " + m_forearmCenter);
                Debug.WriteLine("Forearm [QUAT.]:   " + m_forearmOrientation.Axis + " (" + m_forearmOrientation.Angle + ")");
                Debug.WriteLine("Wrist [CENTER]:    " + m_wristCenter);
            }

            m_model.Update();
        }

        internal bool UpdateModel(OptiTrackTrackingEventArgs e)
        {
            bool armUpdated = false;

            RigidBodyData forearmData = e[Constants.ForearmRigidBodyName];
            RigidBodyData wristData = e[Constants.WristRigidBodyName];

            if (forearmData != null
                && wristData != null)
            {
                double millimeterFactor = 1000.0;

                m_forearmPosition = new Point3D(forearmData.x * millimeterFactor,
                    forearmData.y * millimeterFactor, forearmData.z * millimeterFactor);
                m_forearmOrientation = new Quaternion(forearmData.qx,
                    forearmData.qy, forearmData.qz, forearmData.qw);

                m_wristPosition = new Point3D(wristData.x * millimeterFactor,
                    wristData.y * millimeterFactor, wristData.z * millimeterFactor);
                m_wristOrientation = new Quaternion(wristData.qx,
                    wristData.qy, wristData.qz, wristData.qw);

                Application.Current.Dispatcher.Invoke(
                    new Action(delegate ()
                    {
                        Update();
                    }));

                armUpdated = true;
            }

            return armUpdated;
        }
        #endregion

        #region Event Handling
        private Point TransformPoint(Point rawPt)
        {
            if (m_displayedModels.ContainsKey(m_activeModel))
            {
                Point? transformedPt = m_displayedModels[m_activeModel].Transform(rawPt);
                if (transformedPt != null)
                {
                    return (transformedPt.Value);
                }
                else
                {
                    return new Point(-1.0, -1.0);
                }
            }
            else
            {
                return (new Point(rawPt.X, rawPt.Y));
            }
        }

        internal void OnSkinTouchDown(Point location, double distanceToArmCenter, double hoverDistance)
        {
            SkinEventArgs e = new SkinEventArgs(location, TransformPoint(location),
                distanceToArmCenter, hoverDistance, SkinEventType.Down);
            SkinTouchDown?.Invoke(this, e);
        }

        internal void OnSkinTouchMove(Point location, double distanceToArmCenter, double hoverDistance)
        {
            SkinEventArgs e = new SkinEventArgs(location, TransformPoint(location), 
                distanceToArmCenter, hoverDistance, SkinEventType.Move);
            SkinTouchMove?.Invoke(this, e);
        }

        internal void OnSkinTouchUp(Point location, double distanceToArmCenter, double hoverDistance)
        {
            SkinEventArgs e = new SkinEventArgs(location, TransformPoint(location), 
                distanceToArmCenter, hoverDistance, SkinEventType.Up);
            SkinTouchUp?.Invoke(this, e);
        }

        internal void OnSkinHover(Point location, double distanceToArmCenter, double hoverDistance)
        {
            SkinEventArgs e = new SkinEventArgs(location, TransformPoint(location), 
                distanceToArmCenter, hoverDistance, SkinEventType.Hover);
            SkinHover?.Invoke(this, e);
        }

        internal void OnSkinInactive(Point location, double distanceToArmCenter, double hoverDistance)
        {
            SkinEventArgs e = new SkinEventArgs(location, location,
                distanceToArmCenter, hoverDistance, SkinEventType.None);
            SkinInactive?.Invoke(this, e);
        }

        internal void OnRawData(Tuple<Point3D, Quaternion> armData,
            Tuple<Point3D, Quaternion> wristData, Tuple<Point3D, Quaternion> fingerData,
            Point location, double distanceToArmCenter, double hoverDistance,
            SkinEventType skinEventType, RawDataEventType rawEventType)
        {
            RawDataEventArgs e = new RawDataEventArgs(
                armData.Item1, armData.Item2, wristData.Item1, wristData.Item2,
                fingerData.Item1, fingerData.Item2, location, distanceToArmCenter,
                hoverDistance, skinEventType, rawEventType);
            RawData?.Invoke(this, e);
        }
        #endregion
    }
    #endregion
}
