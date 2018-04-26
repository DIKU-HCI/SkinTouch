using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SkinTouch
{
    #region Class 'SkinTouchPoint'
    public class SkinTouchPoint
    {
        #region Class Members
        private Point m_rawLocation;

        private Point m_transformedLocation;

        private double m_distanceToArmCenter;

        private double m_hoverDistance;

        private SkinEventType m_eventType;
        #endregion

        #region Constructors
        internal SkinTouchPoint(Point rawLocation, Point transformedLocation,
            double distanceToArmCenter, double hoverDistance, SkinEventType eventType)
        {
            m_rawLocation = rawLocation;
            m_transformedLocation = transformedLocation;

            m_eventType = eventType;
            m_distanceToArmCenter = distanceToArmCenter;
            m_hoverDistance = hoverDistance;
        }
        #endregion

        #region Properties
        public SkinEventType EventType
        {
            get { return m_eventType; }
        }

        public Point RawLocation
        {
            get { return m_rawLocation; }
        }

        public Point TransformedLocation
        {
            get { return m_transformedLocation; }
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
    }
    #endregion
}
