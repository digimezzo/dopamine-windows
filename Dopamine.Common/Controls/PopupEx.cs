using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dopamine.Common.Controls
{
    public class PopupEx : Popup
    {
        private static FieldInfo fi = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);

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
    }
}
