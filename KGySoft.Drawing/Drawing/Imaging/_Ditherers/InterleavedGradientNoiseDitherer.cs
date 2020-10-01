﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: InterleavedGradientNoiseDitherer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an <see cref="IDitherer"/> implementation for applying an interleaved gradient noise pattern to the dithered result. For other noise-like
    /// ditherers see the <see cref="OrderedDitherer.BlueNoise">OrderedDitherer.BlueNoise</see> property and the <see cref="RandomNoiseDitherer"/> class.
    /// <br/>See the <strong>Remarks</strong> section for details and some examples.
    /// </summary>
    /// <remarks>
    /// <note>The noise generated by the <see cref="InterleavedGradientNoiseDitherer"/> is not random but based on a formula so using the
    /// same source image, quantizer and strength produces always the same result (similarly to the <see cref="OrderedDitherer.BlueNoise"/> ditherer,
    /// which is based on <see cref="OrderedDitherer">ordered dithering</see>).
    /// To dither images with real random noise use the <see cref="RandomNoiseDitherer"/>, which applies white noise to the quantized source.</note>
    /// <para>The following table demonstrates the effect of the dithering:
    /// <list type="table">
    /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilver.gif" alt="Color hues with system default 8 BPP palette and silver background"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredIGN.gif" alt="Color hues with system default 8 BPP palette, using silver background and interleaved gradient noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and interleaved gradient noise dithering</para></div></term>
    /// </item>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredIGN.gif" alt="Grayscale color shades with black and white palette using interleaved gradient noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and interleaved gradient noise dithering</para></div></term>
    /// </item>
    /// </list></para>
    /// </remarks>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="OrderedDitherer" />
    /// <seealso cref="ErrorDiffusionDitherer" />
    /// <seealso cref="InterleavedGradientNoiseDitherer" />
    public sealed class InterleavedGradientNoiseDitherer : IDitherer
    {
        #region InterleavedGradientNoiseDitheringSession class

        private sealed class InterleavedGradientNoiseDitheringSession : VariableStrengthDitheringSessionBase
        {
            #region Properties

            public override bool IsSequential => false;

            #endregion

            #region Constructors

            internal InterleavedGradientNoiseDitheringSession(IQuantizingSession quantizingSession, InterleavedGradientNoiseDitherer ditherer)
                : base(quantizingSession)
            {
                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                CalibrateStrength(-127, 127);
            }

            #endregion

            #region Methods

            protected override sbyte GetOffset(int x, int y)
            {
                static double Frac(double value) => value - Math.Floor(value);

                // Generating values between -127 and 127 so completely white/black pixels will not change
                // The formula is taken from here: https://bartwronski.com/2016/10/30/dithering-part-three-real-world-2d-quantization-dithering/
                return (sbyte)(Frac(52.9829189 * Frac(0.06711056 * x + 0.00583715 * y)) * 256 - 128);
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Instance Fields

        private readonly float strength;

        #endregion

        #endregion

        #region Properties

        bool IDitherer.InitializeReliesOnContent => false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterleavedGradientNoiseDitherer"/> class.
        /// <br/>See the <strong>Examples</strong> section for some examples.
        /// </summary>
        /// <param name="strength">The strength of the dithering effect between 0 and 1 (inclusive bounds).
        /// Specify 0 to use an auto value for each dithering session based on the used quantizer.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and some examples regarding dithering strength.
        /// The same applies also for the <see cref="InterleavedGradientNoiseDitherer"/> class. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <example>
        /// The following example demonstrates how to use the <see cref="InterleavedGradientNoiseDitherer"/> class.
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToDitheredInterleavedGradientNoise(Bitmap source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = new InterleavedGradientNoiseDitherer();
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized and dithered image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredIGN.gif" alt="Color hues with system default 8 BPP palette, using silver background and interleaved gradient noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredIGN.gif" alt="Grayscale color shades with black and white palette using interleaved gradient noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public InterleavedGradientNoiseDitherer(float strength = 0f)
        {
            if (Single.IsNaN(strength) || strength < 0f || strength > 1f)
                throw new ArgumentOutOfRangeException(nameof(strength), PublicResources.ArgumentMustBeBetween(0, 1));
            this.strength = strength;
        }

        #endregion

        #region Methods

        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizer, IAsyncContext context)
            => new InterleavedGradientNoiseDitheringSession(quantizer, this);

        #endregion
    }
}
