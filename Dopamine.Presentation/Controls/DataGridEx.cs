using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Presentation.Controls
{
    public class DataGridEx : DataGrid
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (this.DataContext != null && this.Columns.Count > 0)
            {
                foreach (DataGridColumn col in this.Columns)
                {
                    col.SetValue(FrameworkElement.DataContextProperty, this.DataContext);
                }
            }
        }
    }
}
