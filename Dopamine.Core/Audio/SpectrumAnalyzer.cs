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

namespace Dopamine.Core.Audio
{
    public enum SpectrumAnimationStyle
    {
        Nervous = 1,
        Gentle
    }

    public class SpectrumAnalyzer : Control
    {
        private const double minDBValue = -90;
        private const double maxDBValue = 0;
        private const double dbScale = (maxDBValue - minDBValue);
        private const int defaultRefreshInterval = 25;

        private readonly DispatcherTimer animationTimer;
        private Canvas spectrumCanvas;
        private ISpectrumPlayer soundPlayer;
        private readonly List<Shape> barShapes = new List<Shape>();
        private double[] barHeights;
        private float[] channelData = new float[1024];
        private float[] channelPeakData;
        private double bandWidth = 1.0;
        private int maximumFrequency = 20000;
        private int maximumFrequencyIndex = 2047;
        private int minimumFrequency = 20;
        private int minimumFrequencyIndex;
        private int[] barIndexMax;
        private int[] barLogScaleIndexMax;
        private int peakFallDelay = 10;

        public static readonly DependencyProperty AnimationStyleProperty = DependencyProperty.Register("AnimationStyle", typeof(SpectrumAnimationStyle), typeof(SpectrumAnalyzer), new PropertyMetadata(SpectrumAnimationStyle.Nervous, null));

        public SpectrumAnimationStyle AnimationStyle
        {
            get
            {
                return (SpectrumAnimationStyle)GetValue(AnimationStyleProperty);
            }
            set
            {
                SetValue(AnimationStyleProperty, value);
            }
        }

        public static readonly DependencyProperty BarBackgroundProperty = DependencyProperty.Register("BarBackground", typeof(Brush), typeof(SpectrumAnalyzer), new PropertyMetadata(Brushes.White, OnBarBackgroundChanged));

        private static void OnBarBackgroundChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer.barShapes != null && spectrumAnalyzer.barShapes.Count > 0)
            {
                foreach (Shape bar in spectrumAnalyzer.barShapes)
                {
                    bar.Fill = spectrumAnalyzer.BarBackground;
                }
            }
        }

        public Brush BarBackground
        {
            get
            {
                return (Brush)GetValue(BarBackgroundProperty);
            }
            set
            {
                SetValue(BarBackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register("BarWidth", typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(1.0, OnBarWidthChanged, OnCoerceBarWidth));

        private static object OnCoerceBarWidth(DependencyObject o, object value)
        {
            return value;
        }

        private static void OnBarWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer.barShapes != null && spectrumAnalyzer.barShapes.Count > 0)
            {
                foreach (Shape bar in spectrumAnalyzer.barShapes)
                {
                    bar.Width = spectrumAnalyzer.BarWidth;
                }
            }
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

        public void RegisterSoundPlayer(ISpectrumPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
            this.soundPlayer.PropertyChanged += soundPlayer_PropertyChanged;
            this.UpdateBarLayout();
            this.animationTimer.Start();
        }

        private void UpdateSpectrum()
        {
            if (this.soundPlayer == null || this.spectrumCanvas == null || this.spectrumCanvas.RenderSize.Width < 1 || this.spectrumCanvas.RenderSize.Height < 1) return;
            if (this.soundPlayer.IsPlaying && !this.soundPlayer.GetFFTData(ref this.channelData)) return;
            this.UpdateSpectrumShapes();
        }

        private void UpdateSpectrumShapes()
        {
            bool allZero = true;
            double height = spectrumCanvas.RenderSize.Height, dbValue = 0d, barHeight = 0d, xCoord = 0d;
            float tPeek = 0f;
            double barWidth = this.BarWidth, barSpacing = this.BarSpacing;

            for (var barIndex = 0; barIndex < barIndexMax.Length; barIndex++)
            {
                var i = barIndexMax[barIndex];
                barHeight = 0d;

                // If we're paused, keep drawing, but set the current height to 0 so the peaks fall.
                if (this.soundPlayer.IsPlaying)
                // Draw the maximum value for the bar's band
                {
                    switch (this.AnimationStyle)
                    {
                        case SpectrumAnimationStyle.Gentle:
                            this.channelData[i] -= 0.003f;
                            break;
                        // Do nothing
                        default:
                            break;
                    }

                    dbValue = 20 * Math.Log10(this.channelData[i]);

                    var fftBucketHeight = ((dbValue - minDBValue) / dbScale) * height;

                    if (barHeight < fftBucketHeight) barHeight = fftBucketHeight;
                    if (barHeight < 0f) barHeight = 0f;
                }

                // Peaks can't surpass the height of the control.
                if (barHeight > height) barHeight = height;

                tPeek = this.channelPeakData[barIndex];
                if (tPeek < barHeight)
                {
                    tPeek = (float)barHeight;
                }
                else
                {
                    tPeek = (float)(barHeight + (this.peakFallDelay * tPeek)) / ((float)(this.peakFallDelay + 1));
                }

                xCoord = barSpacing + (barWidth * barIndex) + (barSpacing * barIndex) + 1;

                switch (this.AnimationStyle)
                {
                    case SpectrumAnimationStyle.Nervous:
                        this.barShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - barHeight, 0, 0);
                        this.barShapes[barIndex].Height = barHeight;
                        break;
                    case SpectrumAnimationStyle.Gentle:
                        this.barShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - tPeek, 0, 0);
                        this.barShapes[barIndex].Height = tPeek;
                        break;
                    default:
                        break;
                }

                if (tPeek > 0.05) allZero = false;
                this.channelPeakData[barIndex] = tPeek;
            }

            if (!allZero || this.soundPlayer.IsPlaying)
                return;
            this.animationTimer.Stop();
        }

        private void UpdateBarLayout()
        {
            if (this.soundPlayer == null || this.spectrumCanvas == null) return;

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
                    Fill = this.BarBackground
                };

                this.barShapes.Add(barRectangle);
            }

            foreach (Shape shape in barShapes) this.spectrumCanvas.Children.Add(shape);
        }

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
    }
}