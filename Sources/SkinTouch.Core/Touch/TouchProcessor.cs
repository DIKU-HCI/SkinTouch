using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

using SkinTouch.Entities;
using SkinTouch.Geometry;

namespace SkinTouch.Touch
{
    #region Class 'TouchProcessor'
    internal class TouchProcessor
    {
        #region Class Members
        private Arm m_arm;

        private Finger m_finger;

        private Point m_prevTouchLocation;

        private bool m_touching = false;
        #endregion

        #region Constructors
        internal TouchProcessor(Arm arm, Finger finger)
        {
            m_arm = arm;
            m_finger = finger;
        }
        #endregion

        #region Activation/Deactivation Methods
        private SkinEventType SetTouchInactive(double distanceToCenter, double hoverDistance)
        {
            SkinEventType eventType = SkinEventType.None;
            if (m_touching)
            {
                // there was a touch before -> fire touch up
                m_arm.OnSkinTouchUp(m_prevTouchLocation, distanceToCenter, hoverDistance);
                eventType = SkinEventType.Up;
            }
            m_touching = false;

            return eventType;
        }

        private SkinEventType SetTouchActive(double distanceToCenter, double hoverDistance)
        {
            SkinEventType eventType = SkinEventType.None;
            if (!(m_touching))
            {
                // there was no touch before -> fire touch down
                m_arm.OnSkinTouchDown(m_prevTouchLocation, distanceToCenter, hoverDistance);
                eventType = SkinEventType.Down;
            }
            else
            {
                m_arm.OnSkinTouchMove(m_prevTouchLocation, distanceToCenter, hoverDistance);
                eventType = SkinEventType.Move;
            }
            m_touching = true;

            return eventType;
        }
        #endregion

        #region Process Methods
        private Point GetProjectedLocationOnArmSurface(double angle,
            double minorAxis, double majorAxis, double normalizedX)
        {
            double transformedAngle = angle - Math.PI;
            transformedAngle += (2.0 * Math.PI);
            transformedAngle %= (2.0 * Math.PI);

            double circumference = Calculations.GetEllipseCircumference(majorAxis, minorAxis);
            double arcLength = Calculations.GetEllipseArcLength(majorAxis, minorAxis, transformedAngle);
            double normalizedY = arcLength / circumference;

            return (new Point(normalizedX, normalizedY));
        }

        internal void DetectTouch()
        {
            Point? projectedTouchPoint = null;

            // gather data (for RAW event)
            Tuple<Point3D, Quaternion> armData = new Tuple<Point3D, Quaternion>(
                m_arm.ForearmPosition, m_arm.ForearmOrientation);
            Tuple<Point3D, Quaternion> wristData = new Tuple<Point3D, Quaternion>(
                m_arm.WristPosition, m_arm.WristOrientation);
            Tuple<Point3D, Quaternion> fingerData = new Tuple<Point3D, Quaternion>(
                m_finger.FingertipPosition, m_finger.FingertipOrientation);

            // set initial values
            RawDataEventType rawEventType = RawDataEventType.Outside;
            SkinEventType skinEventType = SkinEventType.None;
            Point touchLocation = new Point(-1.0, -1.0);
            double distanceToCenter = double.MaxValue;
            double distanceToArmSurface = double.MaxValue;

            // do the detection
            Point3D forearmCenter = m_arm.ForearmCenter;
            Point3D wristCenter = m_arm.WristCenter;
            Point3D fingertipPosition = m_finger.FingertipPosition;

            // get closest point
            Vector3D u = wristCenter - forearmCenter;
            Vector3D pq = fingertipPosition - forearmCenter;
            Vector3D w2 = pq - Vector3D.Multiply(u, Vector3D.DotProduct(pq, u) / u.LengthSquared);
            Point3D closestPoint = fingertipPosition - w2;

            // test, if closestPoint is between forearmCenter and wristCenter
            Vector3D centerVect = wristCenter - forearmCenter;
            double armLength = centerVect.Length;

            Vector3D closestToForearm = closestPoint - forearmCenter;
            double lengthToForearm = closestToForearm.Length;

            Vector3D closestToWrist = closestPoint - wristCenter;
            double lengthToWrist = closestToWrist.Length;

            // distance between center of arm to tracked point
            distanceToCenter = (closestPoint - fingertipPosition).Length;
            bool withinArm = true;

            if (lengthToForearm / armLength > 1.0
                || lengthToWrist / armLength > 1.0
                || distanceToCenter > m_arm.Radius * 3.0)
            {
                rawEventType = RawDataEventType.Outside;

                m_arm.OnSkinInactive(new Point(-1.0, -1.0), double.MaxValue, double.MaxValue);

                withinArm = false;
            }

            if (Constants.DebugPrint)
            {
                // sample
                Debug.WriteLine(closestPoint + " [d = " + distanceToCenter + " | WITHIN: " + withinArm + "]");
            }

            // if we're within the arm, we should calculate the closest ellipse
            if (withinArm)
            {
                rawEventType = RawDataEventType.WithinArm;

                double radius = m_arm.Radius;

                // first, get the x-coordinate (normalized between 0 and 1)
                double normalizedX = lengthToForearm / armLength;

                // now, create generic ellipse data (major and minor axis)
                double minorAxis = normalizedX * m_arm.WristHeight / 2.0 + (1.0 - normalizedX) * radius;
                double majorAxis = normalizedX * m_arm.WristWidth / 2.0 + (1.0 - normalizedX) * radius;
                
                // transform finger position locally
                Point3D center = forearmCenter + normalizedX * (wristCenter - forearmCenter);

                // Quaternion adjustedForearm = new Quaternion(m_arm.ForearmOrientation.Axis, m_arm.ForearmOrientation.Angle - 90.0);
                Quaternion adjustedForearm = m_arm.ForearmOrientation
                    * (new Quaternion(new Vector3D(0.0, 0.0, 1.0), -90.0));

                Quaternion orientation = Quaternion.Slerp(adjustedForearm, m_arm.WristOrientation, normalizedX);

                Transform3DGroup localTransformGroup = new Transform3DGroup();
                localTransformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(orientation)));
                localTransformGroup.Children.Add(new TranslateTransform3D(center.X, center.Y, center.Z));

                Matrix3D localTransform = localTransformGroup.Value;
                Matrix3D localTransformInv = localTransform;
                localTransformInv.Invert();

                Point3D localFingerPosition = localTransformInv.Transform(m_finger.FingertipPosition);
                double angle = Math.Atan2(localFingerPosition.Y, localFingerPosition.X);

                Point3D pointOnArm = new Point3D(majorAxis * Math.Cos(angle),
                    minorAxis * Math.Sin(angle), localFingerPosition.Z);

                // distance between tracked finger and arm surface
                distanceToArmSurface = (localFingerPosition - pointOnArm).Length;

                // distance between center of arm and surface point
                double distanceFromCenterToSurface = (pointOnArm - new Point3D(0.0, 0.0, 0.0)).Length;
                distanceToArmSurface = distanceToCenter - distanceFromCenterToSurface;

                bool insideArm = (distanceFromCenterToSurface > distanceToCenter);

                if (Constants.DebugPrint)
                {
                    Debug.WriteLine(distanceFromCenterToSurface + " :: " + distanceToCenter);
                }

                if (distanceToArmSurface >= Constants.Touch.MinimumTouchDistance
                    && !(insideArm))
                {
                    if (Constants.DebugPrint)
                    {
                        Debug.WriteLine("NO TOUCH: d = " + distanceToArmSurface);
                    }

                    skinEventType = SetTouchInactive(distanceToCenter, distanceToArmSurface);

                    if (distanceToArmSurface <= Constants.Touch.MaximumHoverDistance)
                    {
                        Point hoverLocation = GetProjectedLocationOnArmSurface(angle, minorAxis, majorAxis, normalizedX);
                        m_arm.OnSkinHover(hoverLocation, distanceToCenter, distanceToArmSurface);

                        rawEventType = RawDataEventType.Hover;
                        skinEventType = SkinEventType.Hover;

                        touchLocation = hoverLocation;
                        projectedTouchPoint = touchLocation;
                    }
                    else
                    {
                        Point hoverLocation = GetProjectedLocationOnArmSurface(angle, minorAxis, majorAxis, normalizedX);
                        m_arm.OnSkinInactive(hoverLocation, distanceToCenter, distanceToArmSurface);
                    }
                }
                else
                {
                    if (Constants.DebugPrint)
                    {
                        Debug.WriteLine(localFingerPosition + " [d = "
                            + (localFingerPosition - new Point3D(0.0, 0.0, 0.0)).Length
                            + ", angle = " + (angle / Math.PI * 180.0) + "]");
                        Debug.WriteLine(pointOnArm + " [d = " + distanceToArmSurface + "]");
                    }

                    m_prevTouchLocation = GetProjectedLocationOnArmSurface(angle, minorAxis, majorAxis, normalizedX);

                    skinEventType = SetTouchActive(distanceToCenter, distanceToArmSurface);
                    touchLocation = m_prevTouchLocation;
                    rawEventType = RawDataEventType.Touch;

                    projectedTouchPoint = touchLocation;
                }
            }
            else
            {
                skinEventType = SetTouchInactive(-1.0, -1.0);
                projectedTouchPoint = null;
            }

            // update finger
            m_finger.State = rawEventType;
            m_arm.SetProjectedTouchPoint(projectedTouchPoint, rawEventType);

            // fire RAW event
            m_arm.OnRawData(armData, wristData, fingerData, touchLocation,
                distanceToCenter, distanceToArmSurface, skinEventType, rawEventType);
        }
        #endregion
    }
    #endregion
}
