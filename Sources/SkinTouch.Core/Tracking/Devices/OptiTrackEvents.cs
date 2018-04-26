using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NatNetML;

namespace SkinTouch.Devices
{
    #region Delegates
    public delegate void OptiTrackTrackingEventHandler(object sender, OptiTrackTrackingEventArgs e);
    #endregion

    #region Class 'OptiTrackTrackingEventArgs'
    public class OptiTrackTrackingEventArgs : TimedEventArgs
    {
        #region Static Class Members
        public const int PitchIndex = 0;

        public const int YawIndex = 1;

        public const int RollIndex = 2;
        #endregion

        #region Class Members
        private List<Tuple<RigidBody, RigidBodyData>> m_rigidBodyData;

        private List<Marker> m_markers;

        private NatNetClientML m_client;
        #endregion

        #region Constructors
        public OptiTrackTrackingEventArgs(List<Tuple<RigidBody, RigidBodyData>> rigidBodyData,
            List<Marker> marekers, NatNetClientML client, double timestamp)
            : base(timestamp)
        {
            m_rigidBodyData = rigidBodyData;
            m_markers = marekers;
            m_client = client;
        }
        #endregion

        #region Properties
        public List<Tuple<RigidBody, RigidBodyData>> RigidBodyData
        {
            get { return m_rigidBodyData; }
        }

        public List<Marker> Markers
        {
            get { return m_markers; }
        }

        public RigidBodyData this[RigidBody rigidBody]
        {
            get
            {
                foreach (Tuple<RigidBody, RigidBodyData> rigidBodies in m_rigidBodyData)
                {
                    if (rigidBodies.Item1 != null
                        && rigidBodies.Item1.Equals(rigidBody))
                    {
                        return rigidBodies.Item2;
                    }
                }
                return null;
            }
        }

        public RigidBodyData this[string rigidBodyName]
        {
            get
            {
                foreach (Tuple<RigidBody, RigidBodyData> rigidBodies in m_rigidBodyData)
                {
                    if (rigidBodies.Item1 != null
                        && rigidBodies.Item1.Name != null
                        && rigidBodies.Item1.Name.Equals(rigidBodyName))
                    {
                        return rigidBodies.Item2;
                    }
                }
                return null;
            }
        }
        #endregion
    }
    #endregion
}
