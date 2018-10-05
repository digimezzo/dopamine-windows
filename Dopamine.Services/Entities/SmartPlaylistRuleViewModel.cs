using Digimezzo.Utilities.Utils;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistRuleViewModel : BindableBase
    {
        private ObservableCollection<SmartPlaylistRuleFieldViewModel> fields = new ObservableCollection<SmartPlaylistRuleFieldViewModel>();
        private SmartPlaylistRuleFieldViewModel selectedField;
        private string @operator;
        private string value;

        public SmartPlaylistRuleViewModel()
        {
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Artist"), "artist", SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Album_Artist"), "albumartist", SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Genre"), "genre", SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Title"), "title", SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Album"), "albumtitle", SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Bitrate"), "bitrate", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Track_Number"), "tracknumber", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Track_Count"), "trackcount", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Disc_Number"), "discnumber", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Disc_Count"), "disccount", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Year"), "year", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Rating"), "rating", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Love"), "love", SmartPlaylistRuleFieldDataType.Boolean));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Plays"), "playcount", SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Skips"), "skipcount", SmartPlaylistRuleFieldDataType.Numeric));
            this.selectedField = this.fields.First();
        }

        public ObservableCollection<SmartPlaylistRuleFieldViewModel> Fields
        {
            get { return this.fields; }
            set { SetProperty<ObservableCollection<SmartPlaylistRuleFieldViewModel>>(ref this.fields, value); }
        }

        public SmartPlaylistRuleFieldViewModel SelectedField
        {
            get { return this.selectedField; }
            set { SetProperty<SmartPlaylistRuleFieldViewModel>(ref this.selectedField, value); }
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
