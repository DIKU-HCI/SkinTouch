using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using NatNetML;

using SkinTouch.Devices;
using SkinTouch.SkinTouch3D;

namespace SkinTouch.Entities
{
    #region Class 'Finger'
    public class Finger
    {
        #region Class Members
        #region Geometrical Class Members
        private Point3D m_fingertipPosition;

        private Quaternion m_fingertipOrientation;
        #endregion

        #region Transform Class Members
        private Transform3D m_fingertipTransform;
        #endregion

        #region Visual Class Members
        private FingerModel m_model;
        #endregion

        #region State Class Members
        private RawDataEventType m_state = RawDataEventType.Outside;
        #endregion
        #endregion

        #region Constructors
        public Finger()
        {
            InitializeVisuals();
            InitializeOfflinePosition();

            Update();
        }
        #endregion

        #region Initialization
        private void InitializeVisuals()
        {
            m_model = new FingerModel(this);
        }

        private void InitializeOfflinePosition()
        {
            m_fingertipPosition = Constants.Entities.DefaultFingerLocation;
            m_fingertipOrientation = Constants.Entities.DefaultFingerOrientation;
        }
        #endregion

        #region Properties
        #region Geometrical Properties
        public Point3D FingertipPosition
        {
            get { return m_fingertipPosition; }
        }

        public Quaternion FingertipOrientation
        {
            get { return m_fingertipOrientation; }
        }
        #endregion

        #region Transform Properties
        public Transform3D FingertipTransform
        {
            get { return m_fingertipTransform; }
        }
        #endregion

        #region Visual Properties
        public ModelVisual3D VisualModel
        {
            get { return m_model; }
        }
        #endregion

        #region State Properties
        internal RawDataEventType State
        {
            get { return m_state; }
            set
            {
                bool changed = (m_state != value);
                m_state = value;

                if (changed)
                {
                    Application.Current.Dispatcher.Invoke(
                        new Action(
                            delegate ()
                            {
                                Color colorToSet = m_model.Color;
                                switch (m_state)
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

                                m_model.Color = colorToSet;
                            }));
                }
            }
        }
        #endregion
        #endregion

        #region Motion Methods
        public void Move(Vector3D movementVector)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate ()
                {
                    m_fingertipPosition += movementVector;
                    Update();
                }));
        }
        #endregion

        #region Update Methods
        private void Update()
        {
            Transform3DGroup fingertipTransformGroup = new Transform3DGroup();
            fingertipTransformGroup.Children.Add(new RotateTransform3D(
                new QuaternionRotation3D(m_fingertipOrientation)));
            fingertipTransformGroup.Children.Add(new TranslateTransform3D(
                m_fingertipPosition.X, m_fingertipPosition.Y, m_fingertipPosition.Z));

            m_fingertipTransform = fingertipTransformGroup;

            m_model.Update();
        }

        internal bool UpdateModel(OptiTrackTrackingEventArgs e)
        {
            bool fingerUpdated = false;
            RigidBodyData fingertipData = e[Constants.FingerRigidBodyName];

            if (fingertipData != null)
            {
                double millimeterFactor = 1000.0;

                m_fingertipPosition = new Point3D(fingertipData.x * millimeterFactor,
                    fingertipData.y * millimeterFactor, fingertipData.z * millimeterFactor);
                m_fingertipOrientation = new Quaternion(fingertipData.qx,
                    fingertipData.qy, fingertipData.qz, fingertipData.qw);

                Application.Current.Dispatcher.Invoke(
                    new Action(delegate ()
                    {
                        Update();
                    }));

                fingerUpdated = true;
            }

            return fingerUpdated;
        }
        #endregion
    }
    #endregion
}
