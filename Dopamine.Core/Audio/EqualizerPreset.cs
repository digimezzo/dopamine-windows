using Dopamine.Core.Helpers;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dopamine.Core.Audio
{
    public delegate void BandValueChangedEventHandler(int bandIndex, double newValue); 

    public class EqualizerPreset : BindableBase
    {
        #region Variables
        private NotifiableCollection<EqualizerBand> bands;
        private string name;
        private bool isRemovable;
        #endregion

        #region Properties
        public NotifiableCollection<EqualizerBand> Bands
        {
            get { return this.bands; }
            set
            {
                SetProperty<NotifiableCollection<EqualizerBand>>(ref this.bands, value);
            }
        }

        public bool IsRemovable
        {
            get { return this.isRemovable; }
        }

        public string Name
        {
            get { return this.name; }
        }
        #endregion

        #region Construction
        public EqualizerPreset(string name, bool isRemovable)
        {
            this.name = name;
            this.isRemovable = isRemovable;
            this.Initialize();
        }
        #endregion

        #region Events
        public event BandValueChangedEventHandler BandValueChanged = delegate { }; 
        #endregion

        #region Private
        private void Initialize()
        {
            var localBands = new NotifiableCollection<EqualizerBand>();

            // Add 10 default bands (all at 0.0)
            localBands.Add(new EqualizerBand("60"));
            localBands.Add(new EqualizerBand("170"));
            localBands.Add(new EqualizerBand("310"));
            localBands.Add(new EqualizerBand("600"));
            localBands.Add(new EqualizerBand("1K"));
            localBands.Add(new EqualizerBand("3K"));
            localBands.Add(new EqualizerBand("6K"));
            localBands.Add(new EqualizerBand("12K"));
            localBands.Add(new EqualizerBand("14K"));
            localBands.Add(new EqualizerBand("16K"));

            this.Bands = localBands;
            this.Bands.ItemChanged -= Bands_ItemChanged; 
            this.Bands.ItemChanged += Bands_ItemChanged; 
        }
        #endregion

        #region Public
        public void Load(double[] bandValues)
        {
            if (bandValues.Count() != this.Bands.Count()) return;

            for (int i = 0; i < bandValues.Count(); i++)
            {
                this.Bands[i].Value = bandValues[i];
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return this.name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.name.Equals(((EqualizerPreset)obj).name);
        }

        public override int GetHashCode()
        {
            return new { this.name }.GetHashCode();
        }
        #endregion

        #region Event Handlers
        private void Bands_ItemChanged(object sender, NotifyCollectionChangeEventArgs e)
        {
            this.BandValueChanged(e.Index, this.Bands[e.Index].Value);
        }
        #endregion
    }
}