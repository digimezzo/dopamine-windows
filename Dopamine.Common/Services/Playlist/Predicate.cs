namespace Dopamine.Common.Services.Playlist
{
    public class Predicate
    {
        #region Properties
        public PredicateField Field { get; set; }
        public PredicateOperator Operator { get; set; }
        public string Value { get; set; }
        #endregion
    }
}
