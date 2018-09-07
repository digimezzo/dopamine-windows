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

using Dopamine.Core.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dopamine.Controls
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

        private Path spectrumPath;
        private ISpectrumPlayer soundPlayer;
        private float[] channelData = new float[1024];
        private float[] preBarHeight;
        private double bandWidth = 1.0;
        private int maximumFrequency = 20000;
        private int maximumFrequencyIndex = 2047;
        private int minimumFrequency = 20;
        private int minimumFrequencyIndex;
        private int[] barIndexMax;
        private int preBarHeightDelay = 10;
        private bool Running;
        public static readonly DependencyProperty AnimationStyleProperty = 
            DependencyProperty.Register(nameof(AnimationStyle), typeof(SpectrumAnimationStyle), typeof(SpectrumAnalyzer), new PropertyMetadata(SpectrumAnimationStyle.Nervous, null));

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

        public static readonly DependencyProperty BarBackgroundProperty = 
            DependencyProperty.Register(nameof(BarBackground), typeof(Brush), typeof(SpectrumAnalyzer), new PropertyMetadata(Brushes.White));

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

        public static readonly DependencyProperty BarWidthProperty = 
            DependencyProperty.Register(nameof(BarWidth), typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(1.0));

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

        public static readonly DependencyProperty BarCountProperty = 
            DependencyProperty.Register(nameof(BarCount), typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(32, OnBarCountChanged, OnCoerceBarCount));

        private static object OnCoerceBarCount(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer != null)
            {
                return spectrumAnalyzer.OnCoerceBarCount((int)value);
            }

            return value;
        }

        private static void OnBarCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer != null)
            {
                spectrumAnalyzer.OnBarCountChanged((int)e.OldValue, (int)e.NewValue);
            }
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

        public static readonly DependencyProperty BarSpacingProperty = 
            DependencyProperty.Register(nameof(BarSpacing), typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(5.0d, OnBarSpacingChanged, OnCoerceBarSpacing));

        private static object OnCoerceBarSpacing(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer != null)
            {
                return spectrumAnalyzer.OnCoerceBarSpacing((double)value);
            }

            return value;
        }

        private static void OnBarSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;

            if (spectrumAnalyzer != null)
            {
                spectrumAnalyzer.OnBarSpacingChanged((double)e.OldValue, (double)e.NewValue);
            }
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

        static SpectrumAnalyzer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumAnalyzer), new FrameworkPropertyMetadata(typeof(SpectrumAnalyzer)));
        }

        public SpectrumAnalyzer()
        {
        }

        public override void OnApplyTemplate()
        {
            this.spectrumPath = (Path)GetTemplateChild("PART_SpectrumPath");
            this.spectrumPath.SizeChanged += spectrumPath_SizeChanged;
            this.UpdateBarLayout();
            CompositionTarget.Rendering += UpdateSpectrum;
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);

            if (this.spectrumPath != null)
            {
                this.spectrumPath.SizeChanged -= spectrumPath_SizeChanged;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.UpdateBarLayout();
        }

        public void RegisterSoundPlayer(ISpectrumPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
            this.soundPlayer.PropertyChanged += soundPlayer_PropertyChanged;
            this.UpdateBarLayout();
            this.Running = true;
        }

        private void UpdateSpectrum(object sender, EventArgs e)
        {
            if (!Running)
            {
                return;
            }

            if (this.soundPlayer == null || this.spectrumPath == null || this.RenderSize.Width < 1 || this.RenderSize.Height < 1)
            {
                return;
            }

            if (this.soundPlayer.IsPlaying && !this.soundPlayer.GetFFTData(ref this.channelData))
            {
                return;
            }

            this.UpdateSpectrumShapes();
        }
        private double GetCurrentBarHeight(int barIndex, out bool isZero)
        {
            try
            {
                int i = barIndexMax[barIndex];
                double controlHeight = this.RenderSize.Height;
                double barHeight = 0d;

                if (this.soundPlayer.IsPlaying)
                {
                    switch (this.AnimationStyle)
                    {
                        case SpectrumAnimationStyle.Gentle:
                            this.channelData[i] = Math.Max(0, this.channelData[i] - 0.003f);
                            break;
                        // Do nothing
                        default:
                            break;
                    }

                    double dbValue = 20 * Math.Log10(this.channelData[i]);
                    double fftBucketHeight = ((dbValue - minDBValue) / dbScale) * controlHeight;
                    barHeight = Math.Max(0f, fftBucketHeight);
                }

                // Bars can never surpass the height of the control
                if (barHeight > controlHeight)
                {
                    barHeight = controlHeight;
                }

                float preData = this.preBarHeight[barIndex];

                if (preData < barHeight)
                {
                    preData = (float)barHeight;
                }
                else
                {
                    preData = (float)(barHeight + (this.preBarHeightDelay * preData)) / ((float)(this.preBarHeightDelay + 1));
                }

                isZero = !(preData > 0.05);
                this.preBarHeight[barIndex] = preData;

                switch (this.AnimationStyle)
                {
                    case SpectrumAnimationStyle.Nervous:
                        return barHeight;
                    case SpectrumAnimationStyle.Gentle:
                        return preData;
                    default:
                        return 0;
                }
            }
            catch (Exception)
            {
                // HACK: this block sporadically causes a System.IndexOutOfRangeException.
                // The exception was reported by 2 users. For 1 user, it fixed itself by
                // re-presenting his Bose Bluetooth speaker. I wasn't able to reproduce, 
                // nor debug this, so exceptions in the whole block are now suppressed.
                isZero = true;
                return 0;
            }
        }

        private void UpdateSpectrumShapes()
        {
            bool allZero = true;
            double barSpacing = BarSpacing;
            double barWidth = BarWidth;
            var geo = new StreamGeometry();
            double controlHeight = spectrumPath.RenderSize.Height;

            using (StreamGeometryContext context = geo.Open())
            {
                for (int barIndex = 0; barIndex < barIndexMax.Length; barIndex++)
                {
                    double x = barSpacing + barWidth / 2 + (barWidth * barIndex) + (barSpacing * barIndex) + 1;
                    double y = GetCurrentBarHeight(barIndex, out bool isZero);

                    context.BeginFigure(new Point(x, controlHeight), false, false);
                    context.LineTo(new Point(x, controlHeight - y - 1), true, false);
                    allZero &= isZero;
                }
            }

            geo.Freeze();
            spectrumPath.Data = geo;

            if (allZero && !this.soundPlayer.IsPlaying)
            {
                this.Running = false;
                spectrumPath.Data = null;
            }
        }

        private void UpdateBarLayout()
        {
            if (this.soundPlayer == null || this.spectrumPath == null)
            {
                return;
            }

            this.maximumFrequencyIndex = Math.Min(this.soundPlayer.GetFFTFrequencyIndex(this.maximumFrequency) + 1, 2047);
            this.minimumFrequencyIndex = Math.Min(this.soundPlayer.GetFFTFrequencyIndex(this.minimumFrequency), 2047);
            this.bandWidth = Math.Max(((double)(this.maximumFrequencyIndex - this.minimumFrequencyIndex)) / this.RenderSize.Width, 1.0);

            int actualBarCount;

            if (this.BarWidth >= 1.0d)
            {
                actualBarCount = this.BarCount;
            }
            else
            {
                actualBarCount = Math.Max((int)((this.RenderSize.Width - this.BarSpacing) / (this.BarWidth + this.BarSpacing)), 1);
            }

            this.preBarHeight = new float[actualBarCount];

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
        }

        private void soundPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPlaying":
                    if (this.soundPlayer.IsPlaying)
                    {
                        this.Running = true;
                    }

                    break;
            }
        }

        private void spectrumPath_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateBarLayout();
            this.spectrumPath.Data = null;
        }
    }
}