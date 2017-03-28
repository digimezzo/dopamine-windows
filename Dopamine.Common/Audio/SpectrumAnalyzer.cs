// Copyright (C) 2011 - 2012, Jacob Johnston 
//
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions: 
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software. 
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Dopamine.Common.Audio
{
    public class SpectrumAnalyzer : Control
    {
        #region Constants
        private const double minDBValue = -90;
        private const double maxDBValue = 0;
        private const double dbScale = (maxDBValue - minDBValue);
        private const int defaultRefreshInterval = 25;
        #endregion

        #region Variables
        private readonly DispatcherTimer animationTimer;
        private Canvas spectrumCanvas;
        private ISpectrumPlayer soundPlayer;
        private readonly List<Shape> barShapes = new List<Shape>();
        private double[] barHeights;
        private float[] channelData = new float[2048];
        private float[] channelPeakData;
        private double bandWidth = 1.0;
        private int maximumFrequency = 20000;
        private int maximumFrequencyIndex = 2047;
        private int minimumFrequency = 20;
        private int minimumFrequencyIndex;
        private int[] barIndexMax;
        private int[] barLogScaleIndexMax;
        private int peakFallDelay = 10;
        #endregion

        #region Dependency Properties
        #region BarWidth
        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register("BarWidth", typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(1.0, OnBarWidthChanged, OnCoerceBarWidth));

        private static object OnCoerceBarWidth(DependencyObject o, object value)
        {
            return value;
        }

        private static void OnBarWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        public double BarWidth
        {
            get
            {
                return (double)GetValue(BarWidthProperty);
            }
            set
            {
                SetValue(BarWidthProperty, value);
            }
        }
        #endregion

        #region BarCount
        public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register("BarCount", typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(32, OnBarCountChanged, OnCoerceBarCount));

        private static object OnCoerceBarCount(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null) return spectrumAnalyzer.OnCoerceBarCount((int)value);
            return value;
        }

        private static void OnBarCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null) spectrumAnalyzer.OnBarCountChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual int OnCoerceBarCount(int value)
        {
            value = Math.Max(value, 1);
            return value;
        }

        protected virtual void OnBarCountChanged(int oldValue, int newValue)
        {
            this.UpdateBarLayout();
        }

        public int BarCount
        {
            get
            {
                return (int)GetValue(BarCountProperty);
            }
            set
            {
                SetValue(BarCountProperty, value);
            }
        }
        #endregion

        #region BarSpacing
        public static readonly DependencyProperty BarSpacingProperty = DependencyProperty.Register("BarSpacing", typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(5.0d, OnBarSpacingChanged, OnCoerceBarSpacing));

        private static object OnCoerceBarSpacing(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer != null) return spectrumAnalyzer.OnCoerceBarSpacing((double)value);
            return value;
        }

        private static void OnBarSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null) spectrumAnalyzer.OnBarSpacingChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceBarSpacing(double value)
        {
            value = Math.Max(value, 0);
            return value;
        }

        protected virtual void OnBarSpacingChanged(double oldValue, double newValue)
        {
            this.UpdateBarLayout();
        }

        public double BarSpacing
        {
            get
            {
                return (double)GetValue(BarSpacingProperty);
            }
            set
            {
                SetValue(BarSpacingProperty, value);
            }
        }
        #endregion

        #region BarStyle
        public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register("BarStyle", typeof(Style), typeof(SpectrumAnalyzer), new UIPropertyMetadata(null, OnBarStyleChanged, OnCoerceBarStyle));

        private static object OnCoerceBarStyle(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceBarStyle((Style)value);
            else
                return value;
        }

        private static void OnBarStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnBarStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        protected virtual Style OnCoerceBarStyle(Style value)
        {
            return value;
        }

        protected virtual void OnBarStyleChanged(Style oldValue, Style newValue)
        {
            UpdateBarLayout();
        }

        public Style BarStyle
        {
            get
            {
                return (Style)GetValue(BarStyleProperty);
            }
            set
            {
                SetValue(BarStyleProperty, value);
            }
        }
        #endregion

        #region RefreshInterval
        public static readonly DependencyProperty RefreshIntervalProperty = DependencyProperty.Register("RefreshInterval", typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(defaultRefreshInterval, OnRefreshIntervalChanged, OnCoerceRefreshInterval));

        private static object OnCoerceRefreshInterval(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null) return spectrumAnalyzer.OnCoerceRefreshInterval((int)value);
            return value;
        }

        private static void OnRefreshIntervalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null) spectrumAnalyzer.OnRefreshIntervalChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual int OnCoerceRefreshInterval(int value)
        {
            value = Math.Min(1000, Math.Max(10, value));
            return value;
        }

        protected virtual void OnRefreshIntervalChanged(int oldValue, int newValue)
        {
            animationTimer.Interval = TimeSpan.FromMilliseconds(newValue);
        }

        public int RefreshInterval
        {
            get
            {
                return (int)GetValue(RefreshIntervalProperty);
            }
            set
            {
                SetValue(RefreshIntervalProperty, value);
            }
        }
        #endregion
        #endregion

        #region Construction
        static SpectrumAnalyzer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumAnalyzer), new FrameworkPropertyMetadata(typeof(SpectrumAnalyzer)));
        }

        public SpectrumAnalyzer()
        {
            this.animationTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = TimeSpan.FromMilliseconds(defaultRefreshInterval)
            };

            this.animationTimer.Tick += animationTimer_Tick;
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            this.spectrumCanvas = (Canvas)GetTemplateChild("PART_SpectrumCanvas");
            this.spectrumCanvas.SizeChanged += spectrumCanvas_SizeChanged;
            this.UpdateBarLayout();
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);
            if (this.spectrumCanvas != null) this.spectrumCanvas.SizeChanged -= spectrumCanvas_SizeChanged;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            this.UpdateBarLayout();
            this.UpdateSpectrum();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.UpdateBarLayout();
            this.UpdateSpectrum();
        }
        #endregion

        #region Public
        public void RegisterSoundPlayer(ISpectrumPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
            this.soundPlayer.PropertyChanged += soundPlayer_PropertyChanged;
            this.UpdateBarLayout();
            this.animationTimer.Start();
        }
        #endregion

        #region Private
        private void UpdateSpectrum()
        {
            if (this.soundPlayer == null || this.spectrumCanvas == null || this.spectrumCanvas.RenderSize.Width < 1 || this.spectrumCanvas.RenderSize.Height < 1) return;
            if (this.soundPlayer.IsPlaying && !this.soundPlayer.GetFFTData(ref this.channelData)) return;
            this.UpdateSpectrumShapes();
        }

        private void UpdateSpectrumShapes()
        {
            bool allZero = true;
            double fftBucketHeight = 0f;
            double barHeight = 0f;
            double lastPeakHeight = 0f;
            double peakYPos = 0f;
            double height = spectrumCanvas.RenderSize.Height;
            int barIndex = 0;

            for (int i = this.minimumFrequencyIndex; i <= this.maximumFrequencyIndex; i++)
            {
                // If we're paused, keep drawing, but set the current height to 0 so the peaks fall.
                if (!this.soundPlayer.IsPlaying)
                {
                    barHeight = 0f;
                }
                else // Draw the maximum value for the bar's band
                {
                    double dbValue = 20 * Math.Log10((double)channelData[i]);
                    fftBucketHeight = ((dbValue - minDBValue) / dbScale) * height;

                    if (barHeight < fftBucketHeight) barHeight = fftBucketHeight;
                    if (barHeight < 0f) barHeight = 0f;
                }

                // If this is the last FFT bucket in the bar's group, draw the bar.
                if (i == this.barIndexMax[barIndex])
                {
                    // Peaks can't surpass the height of the control.
                    if (barHeight > height) barHeight = height;

                    peakYPos = barHeight;

                    if (this.channelPeakData[barIndex] < peakYPos)
                    {
                        this.channelPeakData[barIndex] = (float)peakYPos;
                    }
                    else
                    {
                        this.channelPeakData[barIndex] = (float)(peakYPos + (this.peakFallDelay * this.channelPeakData[barIndex])) / ((float)(this.peakFallDelay + 1));
                    }

                    double xCoord = this.BarSpacing + (this.BarWidth * barIndex) + (this.BarSpacing * barIndex) + 1;

                    //this.barShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - barHeight, 0, 0);
                    //this.barShapes[barIndex].Height = barHeight;
                    this.barShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - this.channelPeakData[barIndex], 0, 0);
                    this.barShapes[barIndex].Height = this.channelPeakData[barIndex];

                    if (this.channelPeakData[barIndex] > 0.05) allZero = false;

                    lastPeakHeight = barHeight;
                    barHeight = 0f;
                    barIndex++;
                }
            }

            if (allZero && !this.soundPlayer.IsPlaying) this.animationTimer.Stop();
        }

        private void UpdateBarLayout()
        {
            if (this.soundPlayer == null || this.spectrumCanvas == null) return;

            this.BarWidth = Math.Max(((double)(this.spectrumCanvas.RenderSize.Width - (this.BarSpacing * (this.BarCount + 1))) / (double)this.BarCount), 1);
            this.maximumFrequencyIndex = Math.Min(this.soundPlayer.GetFFTFrequencyIndex(this.maximumFrequency) + 1, 2047);
            this.minimumFrequencyIndex = Math.Min(this.soundPlayer.GetFFTFrequencyIndex(this.minimumFrequency), 2047);
            this.bandWidth = Math.Max(((double)(this.maximumFrequencyIndex - this.minimumFrequencyIndex)) / this.spectrumCanvas.RenderSize.Width, 1.0);

            int actualBarCount;

            if (this.BarWidth >= 1.0d)
            {
                actualBarCount = this.BarCount;
            }
            else
            {
                actualBarCount = Math.Max((int)((this.spectrumCanvas.RenderSize.Width - this.BarSpacing) / (this.BarWidth + this.BarSpacing)), 1);
            }

            this.channelPeakData = new float[actualBarCount];

            int indexCount = this.maximumFrequencyIndex - this.minimumFrequencyIndex;
            int linearIndexBucketSize = (int)Math.Round((double)indexCount / (double)actualBarCount, 0);
            var maxIndexList = new List<int>();
            var maxLogScaleIndexList = new List<int>();
            double maxLog = Math.Log(actualBarCount, actualBarCount);

            for (int i = 1; i < actualBarCount; i++)
            {
                maxIndexList.Add(this.minimumFrequencyIndex + (i * linearIndexBucketSize));
                int logIndex = (int)((maxLog - Math.Log((actualBarCount + 1) - i, (actualBarCount + 1))) * indexCount) + this.minimumFrequencyIndex;
                maxLogScaleIndexList.Add(logIndex);
            }

            maxIndexList.Add(this.maximumFrequencyIndex);
            maxLogScaleIndexList.Add(this.maximumFrequencyIndex);
            this.barIndexMax = maxIndexList.ToArray();
            this.barLogScaleIndexMax = maxLogScaleIndexList.ToArray();

            this.barHeights = new double[actualBarCount];

            this.spectrumCanvas.Children.Clear();
            this.barShapes.Clear();

            double height = this.spectrumCanvas.RenderSize.Height;

            for (int i = 0; i < actualBarCount; i++)
            {
                double xCoord = this.BarSpacing + (this.BarWidth * i) + (this.BarSpacing * i) + 1;
                Rectangle barRectangle = new Rectangle()
                {
                    Margin = new Thickness(xCoord, height, 0, 0),
                    Width = this.BarWidth,
                    Height = 0,
                    Style = this.BarStyle
                };

                this.barShapes.Add(barRectangle);
            }

            foreach (Shape shape in barShapes) this.spectrumCanvas.Children.Add(shape);
        }
        #endregion

        #region Event Handlers
        private void soundPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPlaying":
                    if (this.soundPlayer.IsPlaying && !this.animationTimer.IsEnabled) this.animationTimer.Start();
                    break;
            }
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateSpectrum();
        }

        private void spectrumCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateBarLayout();
        }
        #endregion
    }
}