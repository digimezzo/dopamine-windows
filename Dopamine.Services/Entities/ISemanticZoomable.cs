namespace Dopamine.Services.Entities
{
    public interface ISemanticZoomable
    {
        string Header { get; }

        bool IsHeader { get; set; }
    }
}
