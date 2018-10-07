using Digimezzo.Utilities.Utils;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistRuleViewModel : BindableBase
    {
        private ObservableCollection<SmartPlaylistRuleFieldViewModel> fields = new ObservableCollection<SmartPlaylistRuleFieldViewModel>();
        private ObservableCollection<SmartPlaylistRuleOperatorViewModel> operators = new ObservableCollection<SmartPlaylistRuleOperatorViewModel>();
        private SmartPlaylistRuleFieldViewModel selectedField;
        private SmartPlaylistRuleOperatorViewModel selectedOperator;
        private string @operator;
        private string value;
        private bool isTextInputSelected;
        private bool isLoveSelected;
        private bool isRatingSelected;

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
            this.GetOperators();
            this.GetField();
        }

        public ObservableCollection<SmartPlaylistRuleFieldViewModel> Fields
        {
            get { return this.fields; }
            set { SetProperty<ObservableCollection<SmartPlaylistRuleFieldViewModel>>(ref this.fields, value); }
        }

        public SmartPlaylistRuleFieldViewModel SelectedField
        {
            get { return this.selectedField; }
            set
            {
                SetProperty<SmartPlaylistRuleFieldViewModel>(ref this.selectedField, value);
                this.GetOperators();
                this.GetField();
            }
        }

        public ObservableCollection<SmartPlaylistRuleOperatorViewModel> Operators
        {
            get { return this.operators; }
            set { SetProperty<ObservableCollection<SmartPlaylistRuleOperatorViewModel>>(ref this.operators, value); }
        }

        public SmartPlaylistRuleOperatorViewModel SelectedOperator
        {
            get { return this.selectedOperator; }
            set { SetProperty<SmartPlaylistRuleOperatorViewModel>(ref this.selectedOperator, value); }
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

        public bool IsTextInputSelected
        {
            get { return this.isTextInputSelected; }
            set { SetProperty<bool>(ref this.isTextInputSelected, value); }
        }

        public bool IsLoveSelected
        {
            get { return this.isLoveSelected; }
            set { SetProperty<bool>(ref this.isLoveSelected, value); }
        }

        public bool IsRatingSelected
        {
            get { return this.isRatingSelected; }
            set { SetProperty<bool>(ref this.isRatingSelected, value); }
        }

        private void GetField()
        {
            // Some defaults
            this.isTextInputSelected = true;
            this.isLoveSelected = false;
            this.isRatingSelected = false;

            if (this.selectedField != null)
            {
                this.isLoveSelected = this.selectedField.Name.Equals("love");
                this.isRatingSelected = this.selectedField.Name.Equals("rating");
                this.isTextInputSelected = !this.isLoveSelected & !this.isRatingSelected;
            }

            // Notify the UI of the changes
            RaisePropertyChanged(nameof(this.IsTextInputSelected));
            RaisePropertyChanged(nameof(this.IsLoveSelected));
            RaisePropertyChanged(nameof(this.IsRatingSelected));
        }

        private void GetOperators()
        {
            this.operators = new ObservableCollection<SmartPlaylistRuleOperatorViewModel>();

            if (this.selectedField != null)
            {
                if (this.SelectedField.DataType.Equals(SmartPlaylistRuleFieldDataType.Boolean))
                {
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is"), "is"));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is_Not"), "isnot"));
                }
                else if (this.SelectedField.DataType.Equals(SmartPlaylistRuleFieldDataType.Numeric))
                {
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is"), "is"));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is_Not"), "isnot"));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Greater_Than"), "greaterthan"));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Less_Than"), "lessthan"));
                }
                else if (this.SelectedField.DataType.Equals(SmartPlaylistRuleFieldDataType.Text))
                {
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is"), "is"));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is_Not"), "isnot"));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Contains"), "contains"));
                }
            }

            if(this.operators.Count > 0)
            {
                if (this.selectedOperator == null || !this.operators.Contains(this.selectedOperator))
                {
                    this.selectedOperator = this.operators.First();
                }
                else
                {
                    // this.selectedOperator remains the same
                }
            }
            else
            {
                this.selectedOperator = null;
            }

            RaisePropertyChanged(nameof(this.SelectedOperator));
            RaisePropertyChanged(nameof(this.Operators));
        }
    }
}
