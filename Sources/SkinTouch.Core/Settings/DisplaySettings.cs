using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinTouch
{
    #region Class 'DisplaySettings'
    public class DisplaySettings
    {
        #region Class Members
        private bool m_showUlna = true;

        private bool m_showGuides = true;

        private bool m_showHorizontalModel = false;

        private bool m_showVerticalModel = false;

        private bool m_showTracingModel = false;
        #endregion

        #region Events
        internal event EventHandler ValueChanged;
        #endregion

        #region Constructors
        public DisplaySettings()
            : this(true, true, false, false, false)
        { }

        public DisplaySettings(bool showUlna, bool showGuides,
            bool showHorizontalModel, bool showVerticalModel, bool showTracingModel)
        {
            m_showUlna = showUlna;
            m_showGuides = showGuides;

            m_showHorizontalModel = showHorizontalModel;
            m_showVerticalModel = showVerticalModel;
            m_showTracingModel = showTracingModel;
        }
        #endregion

        #region Properties
        public bool ShowUlna
        {
            get { return m_showUlna; }
            set
            {
                bool changed = (m_showUlna != value);
                m_showUlna = value;

                if (changed)
                {
                    OnValueChanged();
                }
            }
        }

        public bool ShowGuides
        {
            get { return m_showGuides; }
            set
            {
                bool changed = (m_showGuides != value);
                m_showGuides = value;

                if (changed)
                {
                    OnValueChanged();
                }
            }
        }

        public bool ShowHorizontalModel
        {
            get { return m_showHorizontalModel; }
            set
            {
                bool changed = (m_showHorizontalModel != value);
                m_showHorizontalModel = value;

                if (changed)
                {
                    OnValueChanged();
                }
            }
        }

        public bool ShowVerticalModel
        {
            get { return m_showVerticalModel; }
            set
            {
                bool changed = (m_showVerticalModel != value);
                m_showVerticalModel = value;

                if (changed)
                {
                    OnValueChanged();
                }
            }
        }

        public bool ShowTracingModel
        {
            get { return m_showTracingModel; }
            set
            {
                bool changed = (m_showTracingModel != value);
                m_showTracingModel = value;

                if (changed)
                {
                    OnValueChanged();
                }
            }
        }
        #endregion

        #region Event Handling
        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
    #endregion
}
