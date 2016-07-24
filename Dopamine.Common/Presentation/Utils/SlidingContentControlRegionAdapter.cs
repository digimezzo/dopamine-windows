using Digimezzo.WPFControls;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.Utils
{
    public class SlidingContentControlRegionAdapter : RegionAdapterBase<SlidingContentControl>
    {
        public SlidingContentControlRegionAdapter(IRegionBehaviorFactory factory) : base(factory)
        {
        }

        protected override IRegion CreateRegion()
        {
            return new SlidingSingleActiveRegion();
        }

        protected override void Adapt(IRegion region, SlidingContentControl regionTarget)
        {
            if (regionTarget == null) throw new ArgumentNullException("regionTarget");

            bool contentIsSet = regionTarget.Content != null;
            contentIsSet = contentIsSet | (BindingOperations.GetBinding(regionTarget, ContentControl.ContentProperty) != null);

            if (contentIsSet) throw new InvalidOperationException();
           
            region.ActiveViews.CollectionChanged += (sender, e) => { regionTarget.Content = region.ActiveViews.FirstOrDefault(); };
            region.Views.CollectionChanged += (sender, e) =>
            {
                if (e.Action.Equals(NotifyCollectionChangedAction.Add) & region.ActiveViews.Count() == 0)
                {
                    region.Activate(e.NewItems[0]);
                }
            };
        }
    }

    public class SlidingSingleActiveRegion : Region
    {
        public override void Activate(object view)
        {
            object currentActiveView = this.ActiveViews.FirstOrDefault();

            base.Activate(view);

            if (currentActiveView != null && !currentActiveView.Equals(view) & this.Views.Contains(currentActiveView))
            {
                base.Deactivate(currentActiveView);
            }
        }
    }
}
