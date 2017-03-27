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


namespace Dopamine.Common.Audio
{
    /// <summary>
    /// The different ways that the bar height can be scaled by the spectrum analyzer.
    /// </summary>
    public enum BarHeightScalingStyles
    {
        /// <summary>
        /// A decibel scale. Formula: 20 * Log10(FFTValue). Total bar height
        /// is scaled from -90 to 0 dB.
        /// </summary>
        Decibel,

        /// <summary>
        /// A non-linear squareroot scale. Formula: Sqrt(FFTValue) * 2 * BarHeight.
        /// </summary>
        Sqrt,

        /// <summary>
        /// A linear scale. Formula: 9 * FFTValue * BarHeight.
        /// </summary>
        Linear
    }
}