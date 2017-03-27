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

using System.ComponentModel;

namespace Dopamine.Common.Audio
{
    /// <summary>
    /// Provides access to sound player functionality needed to
    /// render a spectrum analyzer.
    /// </summary>
    public interface ISpectrumPlayer : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets whether the sound player is currently playing audio.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Assigns current FFT data to a buffer.
        /// </summary>
        /// <remarks>
        /// The FFT data in the buffer should consist only of the real number intensity values. This means that if your FFT algorithm returns
        /// complex numbers (as many do), you'd run an algorithm similar to:
        /// for(int i = 0; i &lt; complexNumbers.Length / 2; i++)
        ///     fftResult[i] = Math.Sqrt(complexNumbers[i].Real * complexNumbers[i].Real + complexNumbers[i].Imaginary * complexNumbers[i].Imaginary);
        /// </remarks>
        /// <param name="fftDataBuffer">The buffer to copy FFT data. The buffer should consist of only non-imaginary numbers.</param>
        /// <returns>True if data was written to the buffer, otherwise false.</returns>
        bool GetFFTData(ref float[] fftDataBuffer);

        /// <summary>
        /// Gets the index in the FFT data buffer for a given frequency.
        /// </summary>
        /// <param name="frequency">The frequency for which to obtain a buffer index</param>
        /// <returns>An index in the FFT data buffer</returns>
        int GetFFTFrequencyIndex(int frequency);
    }
}