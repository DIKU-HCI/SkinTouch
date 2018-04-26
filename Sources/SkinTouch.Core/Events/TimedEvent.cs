using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinTouch
{
    #region Class 'TimedEventArgs'
    public class TimedEventArgs : EventArgs
    {
        #region Class Members
        private double m_timestamp;
        #endregion

        #region Constructors
        protected TimedEventArgs(double timestamp)
        {
            m_timestamp = timestamp;
        }
        #endregion

        #region Properties
        public double Timestamp
        {
            get { return m_timestamp; }
        }
        #endregion
    }
    #endregion
}
