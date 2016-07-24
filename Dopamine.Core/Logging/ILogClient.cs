namespace Dopamine.Core.Logging
{
    public interface ILogClient
    {
        #region Properties
        string LogFile { get; set; }
        NLog.Logger Logger { get; set; }
        #endregion
    }
}
