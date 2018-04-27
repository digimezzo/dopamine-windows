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

        //private Canvas spectrumCanvas;
        private Path spectrumPath;
        private ISpectrumPlayer soundPlayer;
        //private readonly List<Shape> barShapes = new List<Shape>();
        private float[] channelData = new float[1024];
        private float[] preBarHeight;
        private double bandWidth = 1.0;
        private int maximumFrequency = 20000;
        private int maximumFrequencyIndex = 2047;
        private int minimumFrequency = 20;
        private int minimumFrequencyIndex;
        private int[] barIndexMax;
        //private int[] barLogScaleIndexMax;
        private int peakFallDelay = 10;
        private bool Running;
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
            var path = spectrumAnalyzer.spectrumPath;
            if (path == null) return;
            var brush = e.NewValue as Brush;
            path.Fill = brush;
            path.Stroke = brush;
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

        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register("BarWidth", typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(1.0,OnBarWidthChanged));

        private static void OnBarWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            SpectrumAnalyzer spectrumAnalyzer = d as SpectrumAnalyzer;
            var path = spectrumAnalyzer.spectrumPath;
            if (path == null) return;
            path.StrokeThickness = (double)e.NewValue;
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
            this.spectrumPath.SizeChanged += spectrumCanvas_SizeChanged;
            this.UpdateBarLayout();
            CompositionTarget.Rendering += UpdateSpectrum;
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);
            if (this.spectrumPath != null) this.spectrumPath.SizeChanged -= spectrumCanvas_SizeChanged;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.UpdateBarLayout();
            //this.UpdateSpectrum();
        }

        public void RegisterSoundPlayer(ISpectrumPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
            this.soundPlayer.PropertyChanged += soundPlayer_PropertyChanged;
            this.UpdateBarLayout();
            Running = true;
        }

        private void UpdateSpectrum(object sender, EventArgs e)
        {
            if (!Running) return;
            if(this.soundPlayer == null || this.spectrumPath == null || this.spectrumPath.RenderSize.Width < 1 || this.spectrumPath.RenderSize.Height < 1) return;
            if (this.soundPlayer.IsPlaying && !this.soundPlayer.GetFFTData(ref this.channelData)) return;
            this.UpdateSpectrumShapes();
        }
        private double GetCurrentBarHeight(int barIndex, out bool isZero)
        {
            var i = barIndexMax[barIndex];
            var controlHeight = spectrumPath.RenderSize.Height;
            var barHeight = 0d;


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
                    default: break;
                }
                var dbValue = 20 * Math.Log10(this.channelData[i]);

                var fftBucketHeight = ((dbValue - minDBValue) / dbScale) * controlHeight;
                barHeight = Math.Max(0f, fftBucketHeight);
            }
            // Peaks can't surpass the height of the control.
            if (barHeight > controlHeight) barHeight = controlHeight;

            var preData = this.preBarHeight[barIndex];
             
            if (preData < barHeight)
            {
                preData = (float)barHeight;
            }
            else
            {
                preData = (float)(barHeight + (this.peakFallDelay * preData)) / ((float)(this.peakFallDelay + 1));
            }

            isZero = !(preData > 0.05);
            this.preBarHeight[barIndex] = preData;
            switch (this.AnimationStyle)
            {
                case SpectrumAnimationStyle.Nervous:return barHeight;
                case SpectrumAnimationStyle.Gentle:return preData;
                default:return 0;
            }

        }
        private void UpdateSpectrumShapes()
        {
            bool allZero = true;
            var barSpacing = BarSpacing;
            var barWidth = BarWidth;
            var geo = new StreamGeometry();
            var controlHeight = spectrumPath.RenderSize.Height;
            using (var context = geo.Open())
            {
                for (var barIndex = 0; barIndex < barIndexMax.Length; barIndex++)
                {
                    var x = barSpacing + (barWidth * barIndex) + (barSpacing * barIndex) + 1;
                    var y = GetCurrentBarHeight(barIndex, out bool isZero);

                    context.BeginFigure(new Point(x, controlHeight), false, false) ;
                    context.LineTo(new Point(x, controlHeight - y - 1), true, false);
                    allZero &= isZero;
                }
            }
            geo.Freeze();
            spectrumPath.Data = geo;
            if (allZero && !this.soundPlayer.IsPlaying)
            {
                Running = false;
            }
        }

        private void UpdateBarLayout()
        {
            if (this.soundPlayer == null || this.spectrumPath == null) return;

            this.maximumFrequencyIndex = Math.Min(this.soundPlayer.GetFFTFrequencyIndex(this.maximumFrequency) + 1, 2047);
            this.minimumFrequencyIndex = Math.Min(this.soundPlayer.GetFFTFrequencyIndex(this.minimumFrequency), 2047);
            this.bandWidth = Math.Max(((double)(this.maximumFrequencyIndex - this.minimumFrequencyIndex)) / this.spectrumPath.RenderSize.Width, 1.0);

            int actualBarCount;

            if (this.BarWidth >= 1.0d)
            {
                actualBarCount = this.BarCount;
            }
            else
            {
                actualBarCount = Math.Max((int)((this.spectrumPath.RenderSize.Width - this.BarSpacing) / (this.BarWidth + this.BarSpacing)), 1);
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
            //this.barLogScaleIndexMax = maxLogScaleIndexList.ToArray();
 }

        private void soundPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPlaying":
                    if (this.soundPlayer.IsPlaying)
                    {
                        Running = true;
                    }
                    break;
            }
        }


        private void spectrumCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateBarLayout();
        }
    }
}