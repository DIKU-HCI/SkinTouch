using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SkinTouch
{
    #region Delegates
    public delegate void SkinEventHandler(object sender, SkinEventArgs e);

    public delegate void RawDataEventHandler(object sender, RawDataEventArgs e);
    #endregion

    #region Class 'SkinEventArgs'
    public class SkinEventArgs : TimedEventArgs
    {
        #region Class Members
        private SkinTouchPoint m_point;
        #endregion

        #region Constructors
        internal SkinEventArgs(Point rawLocation, Point transformedLocation,
            double distanceToArmCenter, double hoverDistance, SkinEventType eventType)
            : base(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds)
        {
            m_point = new SkinTouchPoint(rawLocation, transformedLocation,
                distanceToArmCenter, hoverDistance, eventType);
        }
        #endregion

        #region Properties
        public SkinTouchPoint Point
        {
            get { return m_point; }
        }
        #endregion
    }
    #endregion

    #region Class 'RawDataEventArgs'
    public class RawDataEventArgs : TimedEventArgs
    {
        #region Static Class Members
        public static readonly new RawDataEventArgs Empty = new RawDataEventArgs(
            new Point3D(0.0, 0.0, 0.0), new Quaternion(0.0, 0.0, 0.0, 1.0),
            new Point3D(0.0, 0.0, 0.0), new Quaternion(0.0, 0.0, 0.0, 1.0),
            new Point3D(0.0, 0.0, 0.0), new Quaternion(0.0, 0.0, 0.0, 1.0),
            new Point(0.0, 0.0), 0.0, 0.0, SkinEventType.None, RawDataEventType.Outside);
        #endregion

        #region Class Members
        #region OptiTrack Class Members
        private Point3D m_armPosition;

        private Quaternion m_armOrientation;

        private Point3D m_wristPosition;

        private Quaternion m_wristOrientation;

        private Point3D m_fingerPosition;

        private Quaternion m_fingerOrientation;
        #endregion

        #region Touch Class Members
        private Point m_location;

        private double m_distanceToArmCenter;

        private double m_hoverDistance;

        private SkinEventType m_skinEventType;

        private RawDataEventType m_rawEventType;
        #endregion
        #endregion

        #region Constructors
        internal RawDataEventArgs(Point3D armPosition, Quaternion armOrientation,
            Point3D wristPosition, Quaternion wristOrientation, Point3D fingerPosition,
            Quaternion fingerOrientation, Point location, double distanceToArmCenter,
            double hoverDistance, SkinEventType skinEventType, RawDataEventType rawEventType)
            : base(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds)
        {
            m_armPosition = armPosition;
            m_armOrientation = armOrientation;
            m_wristPosition = wristPosition;
            m_wristOrientation = wristOrientation;
            m_fingerPosition = fingerPosition;
            m_fingerOrientation = fingerOrientation;

            m_location = location;
            m_skinEventType = skinEventType;
            m_rawEventType = rawEventType;
            m_distanceToArmCenter = distanceToArmCenter;
            m_hoverDistance = hoverDistance;
        }
        #endregion

        #region Properties
        #region OptiTrack Properties
        public Point3D ArmLocation
        {
            get { return m_armPosition; }
        }

        public Quaternion ArmOrientation
        {
            get { return m_armOrientation; }
        }

        public Point3D WristLocation
        {
            get { return m_wristPosition; }
        }

        public Quaternion WristOrientation
        {
            get { return m_wristOrientation; }
        }

        public Point3D FingerLocation
        {
            get { return m_fingerPosition; }
        }

        public Quaternion FingerOrientation
        {
            get { return m_fingerOrientation; }
        }
        #endregion

        #region Touch Properties
        public SkinEventType SkinEventType
        {
            get { return m_skinEventType; }
        }

        public RawDataEventType RawEventType
        {
            get { return m_rawEventType; }
        }

        public Point Location
        {
            get { return m_location; }
        }

        public double DistanceToArmCenter
        {
            get { return m_distanceToArmCenter; }
        }

        public double HoverDistance
        {
            get { return m_hoverDistance; }
        }
        #endregion
        #endregion
    }
    #endregion
}
