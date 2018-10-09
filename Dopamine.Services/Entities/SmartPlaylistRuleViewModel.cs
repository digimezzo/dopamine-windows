using Digimezzo.Utilities.Utils;
using Dopamine.Core.IO;
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
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Artist"), SmartPlaylistDecoder.FieldArtist, SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Album_Artist"), SmartPlaylistDecoder.FieldAlbumArtist, SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Genre"), SmartPlaylistDecoder.FieldGenre, SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Title"), SmartPlaylistDecoder.FieldTitle, SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Album"), SmartPlaylistDecoder.FieldAlbumTitle, SmartPlaylistRuleFieldDataType.Text));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Bitrate"), SmartPlaylistDecoder.FieldBitrate, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Track_Number"), SmartPlaylistDecoder.FieldTrackNumber, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Track_Count"), SmartPlaylistDecoder.FieldTrackCount, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Disc_Number"), SmartPlaylistDecoder.FieldDiscNumber, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Disc_Count"), SmartPlaylistDecoder.FieldDiscCount, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Year"), SmartPlaylistDecoder.FieldYear, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Rating"), SmartPlaylistDecoder.FieldRating, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Love"), SmartPlaylistDecoder.FieldLove, SmartPlaylistRuleFieldDataType.Boolean));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Plays"), SmartPlaylistDecoder.FieldPlayCount, SmartPlaylistRuleFieldDataType.Numeric));
            this.fields.Add(new SmartPlaylistRuleFieldViewModel(ResourceUtils.GetString("Language_Skips"), SmartPlaylistDecoder.FieldSkipCount, SmartPlaylistRuleFieldDataType.Numeric));
            this.selectedField = this.fields.First();
            this.GetOperators();
            this.GetValueSelector();
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
                this.GetValueSelector();
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

        private void GetValueSelector()
        {
            // Some defaults
            this.isTextInputSelected = true;
            this.isLoveSelected = false;
            this.isRatingSelected = false;
            this.value = null;

            if (this.selectedField != null)
            {
                this.isLoveSelected = this.selectedField.Name.Equals(SmartPlaylistDecoder.FieldLove);
                this.isRatingSelected = this.selectedField.Name.Equals(SmartPlaylistDecoder.FieldRating);
                this.isTextInputSelected = !this.isLoveSelected & !this.isRatingSelected;

                if (this.isLoveSelected)
                {
                    this.value = "1";
                }
                else if (this.isRatingSelected)
                {
                    this.value = "3";
                }
            }

            // Notify the UI of the changes
            RaisePropertyChanged(nameof(this.IsTextInputSelected));
            RaisePropertyChanged(nameof(this.IsLoveSelected));
            RaisePropertyChanged(nameof(this.IsRatingSelected));
            RaisePropertyChanged(nameof(this.Value));
        }

        private void GetOperators()
        {
            this.operators = new ObservableCollection<SmartPlaylistRuleOperatorViewModel>();

            if (this.selectedField != null)
            {
                if (this.SelectedField.DataType.Equals(SmartPlaylistRuleFieldDataType.Boolean))
                {
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is"), SmartPlaylistDecoder.OperatorIs));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is_Not"), SmartPlaylistDecoder.OperatorIsNot));
                }
                else if (this.SelectedField.DataType.Equals(SmartPlaylistRuleFieldDataType.Numeric))
                {
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is"), SmartPlaylistDecoder.OperatorIs));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is_Not"), SmartPlaylistDecoder.OperatorIsNot));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Greater_Than"), SmartPlaylistDecoder.OperatorGreaterThan));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Less_Than"), SmartPlaylistDecoder.OperatorLessThan));
                }
                else if (this.SelectedField.DataType.Equals(SmartPlaylistRuleFieldDataType.Text))
                {
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is"), SmartPlaylistDecoder.OperatorIs));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Is_Not"), SmartPlaylistDecoder.OperatorIsNot));
                    this.operators.Add(new SmartPlaylistRuleOperatorViewModel(ResourceUtils.GetString("Language_Smart_Playlist_Contains"), SmartPlaylistDecoder.OperatorContains));
                }
            }

            if (this.operators.Count > 0)
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

        public SmartPlaylistRule ToSmartPlaylistRule()
        {
            return new SmartPlaylistRule(this.selectedField.Name, this.selectedOperator.Name, this.value);
        }
    }
}
