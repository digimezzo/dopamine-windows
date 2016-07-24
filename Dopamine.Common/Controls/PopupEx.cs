using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.Windows;

namespace Dopamine.Common.Controls
{
    public class PopupEx : Popup
    {
        #region Variables
        private static FieldInfo fi = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
        #endregion

        #region Public
        public void Open()
        {
            if (SystemParameters.MenuDropAlignment)
            {
                fi.SetValue(null, false);
                this.IsOpen = true;
                fi.SetValue(null, true);
            }
            else
            {
                this.IsOpen = true;
            }
        }

        public void Close()
        {
            this.IsOpen = false;
        }
        #endregion
    }
}
