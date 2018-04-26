using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using SkinTouch.Entities;
using SkinTouch.Visuals;

namespace SkinTouch.SkinTouch3D
{
    #region Class 'FingerModel'
    public class FingerModel : ModelVisual3D
    {
        #region Class Members
        #region Control Class Members
        private Finger m_finger;
        #endregion

        #region Visual Class Members
        private Model3DGroup m_model;

        private Sphere m_fingerModel;

        private CoordinateSystem m_fingertipSystem;
        #endregion
        #endregion

        #region Constructors
        public FingerModel(Finger finger)
        {
            m_finger = finger;

            InitializeVisuals();
        }
        #endregion

        #region Initialization
        private void InitializeVisuals()
        {
            // create the model
            m_fingerModel = new Sphere(Constants.Colors.InactiveTouchColor, 0.005 * 1000.0);

            m_fingertipSystem = new CoordinateSystem(0.05 * 1000.0, false, true, false, 0.5);

            m_model = new Model3DGroup();
            m_model.Children.Add(m_fingerModel.Model);
            m_model.Children.Add(m_fingertipSystem.Model);

            Content = m_model;
        }
        #endregion

        #region Properties
        public Color Color
        {
            get { return m_fingerModel.Color; }
            set { m_fingerModel.Color = value; }
        }
        #endregion

        #region Update Methods
        internal void Update()
        {
            m_fingerModel.Model.Transform = m_finger.FingertipTransform;
            m_fingertipSystem.Model.Transform = m_finger.FingertipTransform;
        }
        #endregion
    }
    #endregion
}
