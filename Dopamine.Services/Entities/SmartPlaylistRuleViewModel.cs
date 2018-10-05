using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistRuleViewModel : BindableBase
    {
        private string field;
        private string @operator;
        private string value;

        public string Field
        {
            get { return this.field; }
            set { SetProperty<string>(ref this.field, value); }
        }

        public string Operator
        {
            get { return this.@operator; }
            set { SetProperty<string>(ref this.@operator, value); }
        }

        public string Value
        {
            get { return this.value; }
            set { SetProperty<string>(ref this.value, value); }
        }
    }
}
