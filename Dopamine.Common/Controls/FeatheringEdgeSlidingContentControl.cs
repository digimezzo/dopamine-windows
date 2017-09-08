using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Digimezzo.WPFControls;

namespace Dopamine.Common.Controls
{
    public class FeatheringEdgeSlidingContentControl : SlidingContentControl
    {
        #region Variables
        private Shape dummy;
        #endregion

        #region Constructor
        static FeatheringEdgeSlidingContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FeatheringEdgeSlidingContentControl), new FrameworkPropertyMetadata(typeof(FeatheringEdgeSlidingContentControl)));
        }
        #endregion

        #region Override
        public override void OnApplyTemplate()
        {
            this.dummy = (Shape)GetTemplateChild("Dummy");
            base.OnApplyTemplate();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            this.dummy.Fill = Brushes.Transparent;
            base.OnContentChanged(oldContent, newContent);
        }
        #endregion
    }
}