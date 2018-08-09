using Prism.Commands;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.Services.Entities
{
    public interface ISemanticZoomViewModel
    {
        ObservableCollection<ISemanticZoomable> SemanticZoomables { get; set; }

        ObservableCollection<ISemanticZoomSelector> SemanticZoomSelectors { get; set; }

        DelegateCommand<string> SemanticJumpCommand { get; set; }

        Task ShowSemanticZoomAsync();

        void HideSemanticZoom();

        void UpdateSemanticZoomHeaders();
    }
}
