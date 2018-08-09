namespace Dopamine.Services.Entities
{
    public interface ISemanticZoomSelector
    {
        string Header { get; set; }

        bool CanZoom { get; set; }
    }
}
