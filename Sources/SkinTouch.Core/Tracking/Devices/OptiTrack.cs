using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using NatNetML;

namespace SkinTouch.Devices
{
    #region Class 'OptiTrack'
    internal class OptiTrack
    {
        #region Class Members
        private NatNetClientML m_natNetClient;

        private bool m_connected = false;

        private IPAddress m_localAddr;

        private List<RigidBody> m_rigidBodies = new List<RigidBody>();
        #endregion

        #region Events
        public event OptiTrackTrackingEventHandler TrackingUpdated;
        #endregion

        #region Constructors
        public OptiTrack()
        {
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            m_localAddr = IPAddress.Any;
            if (m_localAddr != null)
            {
                if (m_natNetClient != null)
                {
                    m_natNetClient.Uninitialize();
                }

                m_natNetClient = new NatNetClientML(Constants.MulticastConnectionType);
                m_natNetClient.OnFrameReady += new FrameReadyEventHandler(OnOptiTrackFrameReady);
            }
        }
        #endregion

        #region Properties
        public List<RigidBody> RigidBodies
        {
            get { return m_rigidBodies; }
        }

        public bool IsConnected
        {
            get { return m_connected; }
            protected internal set { m_connected = value; }
        }
        #endregion

        #region Connection Methods
        public bool Connect(string ipAddress)
        {
            int returnCode = m_natNetClient.Initialize(
                m_localAddr.ToString(), ipAddress);

            if (returnCode == 0)
            {
                if (Constants.DebugPrint)
                {
                    Debug.WriteLine("Initialization succeeded ...");
                }

                m_rigidBodies.Clear();

                List<DataDescriptor> descs = new List<DataDescriptor>();
                bool success = m_natNetClient.GetDataDescriptions(out descs);
                if (success)
                {
                    foreach (DataDescriptor d in descs)
                    {
                        if (d.type == (int)DataDescriptorType.eRigidbodyData)
                        {
                            RigidBody rigidBody = (RigidBody)d;
                            m_rigidBodies.Add(rigidBody);
                        }
                    }

                    IsConnected = true;
                    return true;
                }
                else
                {
                    if (Constants.DebugPrint)
                    {
                        Debug.WriteLine("Could not get data descriptors ...");
                    }

                    m_natNetClient.Uninitialize();
                    return false;
                }
            }
            else
            {
                if (Constants.DebugPrint)
                {
                    Debug.WriteLine("Error Initializing!");
                }

                IsConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
            if (m_natNetClient != null)
            {
                m_natNetClient.Uninitialize();
            }
        }
        #endregion

        #region RigidBody Methods
        private RigidBody GetRigidBody(int bodyId)
        {
            foreach (RigidBody rigidBody in m_rigidBodies)
            {
                if (rigidBody.ID == bodyId)
                {
                    return rigidBody;
                }
            }
            return null;
        }

        internal RigidBody GetRigidBody(string rigidBodyName)
        {
            foreach (RigidBody rigidBody in m_rigidBodies)
            {
                if (rigidBody.Name == rigidBodyName)
                {
                    return rigidBody;
                }
            }
            return null;
        }
        #endregion

        #region Event Handler
        private void OnOptiTrackFrameReady(FrameOfMocapData data, NatNetClientML client)
        {
            if (data != null)
            {
                List<Tuple<RigidBody, RigidBodyData>> rigidBodyInfo = new List<Tuple<RigidBody, RigidBodyData>>();
                for (int i = 0; i < data.nRigidBodies; i++)
                {
                    RigidBodyData rigidBodyData = data.RigidBodies[i];
                    int id = rigidBodyData.ID;

                    RigidBody rigidBody = GetRigidBody(id);
                    if (rigidBody != null)
                    {
                        rigidBodyInfo.Add(new Tuple<RigidBody, RigidBodyData>(rigidBody, rigidBodyData));
                    }
                }

                List<Marker> markerInfo = new List<Marker>();
                for (int i = 0; i < data.nOtherMarkers; i++)
                {
                    if (data.OtherMarkers[i].ID == -1)
                    {
                        markerInfo.Add(data.OtherMarkers[i]);
                    }
                }

                TrackingUpdated?.Invoke(this, new OptiTrackTrackingEventArgs(
                    rigidBodyInfo, markerInfo, m_natNetClient, data.fTimestamp));
            }
        }
        #endregion
    }
    #endregion
}
