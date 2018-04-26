using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SkinTouch.Devices;
using SkinTouch.Entities;
using SkinTouch.Models;
using SkinTouch.Touch;

namespace SkinTouch
{
    #region Class 'SkinTouchBase'
    public class SkinTouchBase
    {
        #region Static Class Members
        private static bool InstanceCreated = false;
        #endregion

        #region Class Members
        #region Device Class Members
        private OptiTrack m_optiTrack;

        private bool m_optiTrackBusy = false;
        #endregion

        #region Model Class Members
        private Arm m_arm;

        private Finger m_finger;
        #endregion

        #region Touch Class Members
        private TouchProcessor m_processor;
        #endregion

        #region Model Class Members
        private Dictionary<ModelType, LinearModel> m_models;
        #endregion

        #region Synchronization Class Members
        private object m_syncObj = new object();
        #endregion
        #endregion

        #region Contructors
        public SkinTouchBase()
        {
            if (InstanceCreated)
            {
                throw new InvalidOperationException("An instance is already created!");
            }

            InstanceCreated = true;
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            // entities
            m_arm = new Arm();
            m_finger = new Finger();

            // processing & tracking
            m_processor = new TouchProcessor(m_arm, m_finger);
            m_optiTrack = new OptiTrack();

            // models
            m_models = new Dictionary<ModelType, LinearModel>();

            LinearModel horizontalModel = new LinearModel(Constants.Models.HorizontalRows, 
                Constants.Models.HorizontalColumns, ModelDirection.Landscape);
            horizontalModel.Load(Constants.Models.HorizontalModelFileName);
            m_models.Add(ModelType.Horizontal, horizontalModel);

            LinearModel verticalModel = new LinearModel(Constants.Models.VerticalRows, 
                Constants.Models.VerticalColumns, ModelDirection.Portrait);
            verticalModel.Load(Constants.Models.VerticalModelFileName);
            m_models.Add(ModelType.Vertical, verticalModel);

            LinearModel tracingModel = new LinearModel(Constants.Models.TracingRows, 
                Constants.Models.TracingColumns, ModelDirection.Landscape);
            tracingModel.Load(Constants.Models.TracingModelFileName);
            m_models.Add(ModelType.Tracing, tracingModel);

            m_arm.AddModel(horizontalModel, ModelType.Horizontal, Constants.Colors.HorizontalGridModelColor);
            m_arm.AddModel(verticalModel, ModelType.Vertical, Constants.Colors.VerticalGridModelColor);
            m_arm.AddModel(tracingModel, ModelType.Tracing, Constants.Colors.TracingGridModelColor);
        }
        #endregion

        #region Events
        public event SkinEventHandler SkinTouchDown
        {
            add
            {
                if (m_arm != null)
                {
                    m_arm.SkinTouchDown += value;
                    m_processor.DetectTouch();
                }
            }
            remove
            {
                if (m_arm != null)
                {
                    m_arm.SkinTouchDown -= value;
                }
            }
        }

        public event SkinEventHandler SkinTouchMove
        {
            add
            {
                if (m_arm != null)
                {
                    m_arm.SkinTouchMove += value;
                    m_processor.DetectTouch();
                }
            }
            remove
            {
                if (m_arm != null)
                {
                    m_arm.SkinTouchMove -= value;
                }
            }
        }

        public event SkinEventHandler SkinTouchUp
        {
            add
            {
                if (m_arm != null)
                {
                    m_arm.SkinTouchUp += value;
                    m_processor.DetectTouch();
                }
            }
            remove
            {
                if (m_arm != null)
                {
                    m_arm.SkinTouchUp -= value;
                }
            }
        }

        public event SkinEventHandler SkinHover
        {
            add
            {
                if (m_arm != null)
                {
                    m_arm.SkinHover += value;
                    m_processor.DetectTouch();
                }
            }
            remove
            {
                if (m_arm != null)
                {
                    m_arm.SkinHover -= value;
                }
            }
        }

        public event SkinEventHandler SkinInactive
        {
            add
            {
                if (m_arm != null)
                {
                    m_arm.SkinInactive += value;
                    m_processor.DetectTouch();
                }
            }
            remove
            {
                if (m_arm != null)
                {
                    m_arm.SkinInactive -= value;
                }
            }
        }

        public event RawDataEventHandler RawData
        {
            add
            {
                if (m_arm != null)
                {
                    m_arm.RawData += value;
                }
            }
            remove
            {
                if (m_arm != null)
                {
                    m_arm.RawData -= value;
                }
            }
        }
        #endregion

        #region Properties
        public Arm Arm
        {
            get { return m_arm; }
        }

        public Finger Finger
        {
            get { return m_finger; }
        }

        public bool IsLive
        {
            get
            {
                return (m_optiTrack != null
                    && m_optiTrack.IsConnected);
            }
        }
        #endregion

        #region Start/Stop Methods
        public void Start(string ipAddress = "127.0.0.1")
        {
            if (m_arm == null
                || m_finger == null
                || m_optiTrack == null)
            {
                throw new InvalidOperationException("This instance is not initialized! You must call Initialize(...) first!");
            }

            ConnectOptiTrack(ipAddress);
            m_optiTrack.TrackingUpdated += OnOptiTrackTrackingUpdated;
        }

        public void Stop()
        {
            if (m_optiTrack != null)
            {
                m_optiTrack.TrackingUpdated -= OnOptiTrackTrackingUpdated;
            }
            DisconnectOptiTrack();
        }
        #endregion

        #region Update Methods
        public void Update()
        {
            if (m_processor != null)
            {
                m_processor.DetectTouch();
            }
        }
        #endregion

        #region Connect Methods
        internal void ConnectOptiTrack(string ipAddress)
        {
            if (m_optiTrack != null
                && !(m_optiTrack.IsConnected))
            {
                m_optiTrack.Connect(ipAddress);
            }
        }

        internal void DisconnectOptiTrack()
        {
            if (m_optiTrack != null
                && m_optiTrack.IsConnected)
            {
                m_optiTrack.Disconnect();
            }
        }
        #endregion

        #region Event Handler
        #region OptiTrack Event Handler
        private void OnOptiTrackTrackingUpdated(object sender, OptiTrackTrackingEventArgs e)
        {
            lock (m_syncObj)
            {
                if (m_arm == null
                    || m_finger == null
                    || m_optiTrackBusy)
                {
                    return;
                }
                m_optiTrackBusy = true;
            }

            // now we just update all models
            // the models will notify clients directly
            bool armUpdated = m_arm.UpdateModel(e);
            bool fingerUpdated = m_finger.UpdateModel(e);

            if (Constants.DebugPrint)
            {
                Debug.WriteLine("Arm: " + armUpdated + ", Finger: " + fingerUpdated);
            }

            // do calculations
            if (armUpdated && fingerUpdated)
            {
                m_processor.DetectTouch();
            }

            lock (m_syncObj)
            {
                m_optiTrackBusy = false;
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
