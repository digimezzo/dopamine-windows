using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.Interfaces
{
    public interface ISemanticZoomViewModel
    {
        ObservableCollection<ISemanticZoomable> SemanticZoomables { get; set; }
        ObservableCollection<ISemanticZoomSelector> SemanticZoomSelectors { get; set; }

        DelegateCommand SemanticJumpCommand { get; set; }
        Task ShowSemanticZoomAsync();

        void HideSemanticZoom();
        void UpdateSemanticZoomHeaders();
    }
}
