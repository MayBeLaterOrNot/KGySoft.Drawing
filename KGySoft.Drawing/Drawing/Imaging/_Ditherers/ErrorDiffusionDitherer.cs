﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorDiffusionDitherer.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
#pragma warning disable CA1814 // arrays in this class are better to be matrices than jagged arrays as they are always rectangular

    public sealed class ErrorDiffusionDitherer : IDitherer
    {
        #region ErrorDiffusionDitheringSession class

        private class ErrorDiffusionDitheringSession : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly ErrorDiffusionDitherer ditherer;
            private readonly int imageWidth;
            private readonly int imageHeight;
            private readonly bool byBrightness;
            private readonly CircularList<(float R, float G, float B)[]> errorsBuffer;

            private int lastRow;

            #endregion

            #region Properties

            public bool IsSequential => true;

            #endregion

            #region Constructors

            internal ErrorDiffusionDitheringSession(IQuantizingSession quantizer, ErrorDiffusionDitherer ditherer, IBitmapData source)
            {
                this.quantizer = quantizer;
                this.ditherer = ditherer;
                imageWidth = source.Width;
                imageHeight = source.Height;
                byBrightness = ditherer.byBrightness ?? quantizer.Palette?.IsGrayscale ?? false;

                // Initializing a circular buffer for the diffused errors.
                // This helps to minimize used memory because it needs only a few lines to be stored.
                // Another solution could be to store resulting colors instead of just the errors but then the color
                // entries would be clipped not just in the end but in every iteration, and small errors would be lost
                // that could stack up otherwise.
                // See also the ErrorDiffusionDitherer constructor for more comments on why using floats.
                errorsBuffer = new CircularList<(float, float, float)[]>(ditherer.matrixHeight);
                for (int i = 0; i < ditherer.matrixHeight; i++)
                    errorsBuffer.Add(new (float, float, float)[imageWidth]);
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                // new line
                if (y != lastRow)
                {
                    errorsBuffer.RemoveFirst();

                    if (y + ditherer.matrixHeight <= imageHeight)
                        errorsBuffer.AddLast(new (float, float, float)[imageWidth]);

                    lastRow = y;
                }

                Color32 currentColor;

                // handling alpha
                if (origColor.A != Byte.MaxValue)
                {
                    currentColor = quantizer.BlendOrMakeTransparent(origColor);
                    if (currentColor.A == 0)
                        return currentColor;
                }
                else
                    currentColor = origColor;

                // applying propagated errors to the current pixel
                ref var error = ref errorsBuffer[0][x];
                currentColor = new Color32((currentColor.R + (int)error.R).ClipToByte(),
                    (currentColor.G + (int)error.G).ClipToByte(),
                    (currentColor.B + (int)error.B).ClipToByte());

                // getting the quantized result for the current pixel + errors
                Color32 quantizedColor = quantizer.GetQuantizedColor(currentColor);

                // determining the quantization error for the current pixel
                int errR;
                int errG;
                int errB;

                if (byBrightness)
                    errR = errG = errB = currentColor.GetBrightness() - quantizedColor.GetBrightness();
                else
                {
                    errR = currentColor.R - quantizedColor.R;
                    errG = currentColor.G - quantizedColor.G;
                    errB = currentColor.B - quantizedColor.B;
                }

                // no error, nothing to propagate further
                if (errR == 0 && errG == 0 && errB == 0)
                    return quantizedColor;

                // TODO: parallel if possible
                // processing the whole matrix and propagating the current error to neighbors
                for (int my = 0; my < ditherer.matrixHeight; my++)
                {
                    // beyond last row
                    if (y + my >= imageHeight)
                        continue;

                    for (int mx = 0; mx < ditherer.matrixWidth; mx++)
                    {
                        int targetX = x + mx - ditherer.matrixFirstPixelIndex + 1;

                        // ignored coefficient or beyond first / last column
                        if (my == 0 && mx < ditherer.matrixFirstPixelIndex || targetX <= 0 || targetX >= imageWidth)
                            continue;

                        float coefficient = ditherer.coefficientsMatrix[my, mx];

                        // ReSharper disable once CompareOfFloatsByEqualityOperator - this is intended
                        if (coefficient == 0f)
                            continue;

                        // applying the error in our buffer
                        error = ref errorsBuffer[my][targetX];
                        error.R += errR * coefficient;
                        error.G += errG * coefficient;
                        error.B += errB * coefficient;
                    }
                }

                return quantizedColor;
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Static Fields

        private static byte[,] floydSteinbergMatrix =
        {
            { 0, 0, 7 },
            { 3, 5, 1 },
        };

        private static byte[,] jarvisJudiceNinkeMatrix =
        {
            { 0, 0, 0, 7, 5 },
            { 3, 5, 7, 5, 3 },
            { 1, 3, 5, 3, 1 },
        };

        private static byte[,] stuckiMatrix =
        {
            { 0, 0, 0, 8, 4 },
            { 2, 4, 8, 4, 2 },
            { 1, 2, 4, 2, 1 },
        };

        private static byte[,] burkesMatrix =
        {
            { 0, 0, 0, 8, 4 },
            { 2, 4, 8, 4, 2 },
        };

        private static byte[,] sierra3Matrix =
        {
            { 0, 0, 0, 5, 3 },
            { 2, 4, 5, 4, 2 },
            { 0, 2, 3, 2, 0 },
        };

        private static byte[,] sierra2Matrix =
        {
            { 0, 0, 0, 4, 3 },
            { 1, 2, 3, 2, 1 },
        };

        private static byte[,] sierraLiteMatrix =
        {
            { 0, 0, 2 },
            { 1, 1, 0 },
        };

        private static byte[,] stevensonArceMatrix =
        {
            { 0, 0, 0, 0, 0, 32, 0 },
            { 12, 0, 26, 0, 30, 0, 16 },
            { 0, 12, 0, 26, 0, 12, 0 },
            { 5, 0, 12, 0, 12, 0, 5 },
        };

        #endregion

        #region Instance Fields

        private readonly float[,] coefficientsMatrix;
        private readonly int matrixWidth;
        private readonly int matrixHeight;
        private readonly int matrixFirstPixelIndex;
        private readonly bool? byBrightness;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDiffusionDitherer" /> class using the specified <paramref name="matrix"/>, <paramref name="divisor"/> and <paramref name="matrixFirstPixelIndex"/>.
        /// <br/>For some well known error diffusion ditherers see the static properties.
        /// </summary>
        /// <param name="matrix">A matrix to be used as the coefficients for the quantization errors to be propagated to the neighboring pixels.</param>
        /// <param name="divisor">Each elements in the <paramref name="matrix"/> will be divided by this value. If less than the sum of the elements
        /// in the <paramref name="matrix"/>, then only a fraction of the error will be propagated.</param>
        /// <param name="matrixFirstPixelIndex">Specifies the first effective index in the first row of the matrix. If larger than zero, then the error will be propagated also to the bottom-left direction.
        /// Must be between 0 and <paramref name="matrix"/> width, excluding upper bound.</param>
        /// <param name="byBrightness"><see langword="true"/>&#160;to apply the same quantization error on every color channel determined by brightness difference;
        /// <see langword="false"/>&#160;to apply handle quantization errors on each color channels independently; <see langword="null"/>&#160;to auto select strategy.
        /// Deciding by brightness can produce a better result when fully saturated colors are mapped to a grayscale palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="OrderedDitherer"/> instance using the specified <paramref name="matrix"/>, <paramref name="divisor"/> and <paramref name="matrixFirstPixelIndex"/>.</returns>
        public ErrorDiffusionDitherer(byte[,] matrix, int divisor, int matrixFirstPixelIndex, bool? byBrightness)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), PublicResources.ArgumentNull);
            if (matrix.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(matrix));
            if (divisor <= 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), PublicResources.ArgumentMustBeGreaterThan(0));

            matrixWidth = matrix.GetUpperBound(1) + 1;
            matrixHeight = matrix.GetUpperBound(0) + 1;
            if (matrixFirstPixelIndex >= matrixWidth)
                throw new ArgumentOutOfRangeException(nameof(matrixFirstPixelIndex), PublicResources.ArgumentMustBeBetween(0, matrixWidth - 1));

            this.matrixFirstPixelIndex = matrixFirstPixelIndex;
            this.byBrightness = byBrightness;

            // Applying divisor to the provided matrix elements into a new float matrix. This has two benefits:
            // 1. Applying the error will be a simple multiplication, which alone is faster even for a float than an
            //    int multiplication combined with bit shifting (or division if divisor is not power of 2)
            // 2. By not losing the fractions after bit shifting, small errors can stack up that would be lost otherwise,
            //    which prevents some artifacts and provides better results for almost completely black/white/saturated areas.
            coefficientsMatrix = new float[matrixHeight, matrixWidth];
            for (int y = 0; y < matrixHeight; y++)
            {
                for (int x = 0; x < matrixWidth; x++)
                    coefficientsMatrix[y, x] = matrix[y, x] / (float)divisor;
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the original filter proposed by Floyd and Steinberg in 1975 when they came out with the idea of error diffusion dithering.
        /// Uses a small, 3x2 matrix so the processing is somewhat faster than the other alternatives.
        /// </summary>
        public static ErrorDiffusionDitherer FloydSteinberg(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(floydSteinbergMatrix, 16, 2, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by Jarvis, Judice and Ninke in 1976.
        /// Uses a 5x3 matrix so the processing is slower than by the original Floyd-Steinberg filter but distributes errors in a wider range.
        /// </summary>
        public static ErrorDiffusionDitherer JarvisJudiceNinke(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(jarvisJudiceNinkeMatrix, 48, 3, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by P. Stucki in 1981.
        /// Uses a 5x3 matrix so the processing is slower than by the original Floyd-Steinberg filter  but distributes errors in a wider range.
        /// </summary>
        public static ErrorDiffusionDitherer Stucki(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(stuckiMatrix, 42, 3, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by D. Burkes in 1988.
        /// Uses a 5x2 matrix, which is actually the same as the first two lines of the matrix used by the Stucki filter.
        /// </summary>
        public static ErrorDiffusionDitherer Burkes(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(burkesMatrix, 32, 3, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the three-line filter proposed by Frankie Sierra in 1989.
        /// Uses a 5x3 matrix so this is the slowest Sierra filter but this produces the best result among them.
        /// </summary>
        public static ErrorDiffusionDitherer Sierra3(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(sierra3Matrix, 32, 3, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the two-line filter proposed by Frankie Sierra in 1990.
        /// Uses a 5x2 matrix so this somewhat faster than the three-line version and still provides a similar quality.
        /// </summary>
        public static ErrorDiffusionDitherer Sierra2(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(sierra2Matrix, 16, 3, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using a small two-line filter proposed by Frankie Sierra.
        /// Uses a 3x2 matrix so it has the same performance as the Floyd-Steinberg algorithm and also produces a quite similar result.
        /// </summary>
        public static ErrorDiffusionDitherer SierraLite(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(sierraLiteMatrix, 4, 2, byBrightness);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the hexagonal filter proposed by Stevenson and Arce in 1985.
        /// Uses a fairly large, 7x4 matrix, but due to the hexagonal arrangement of the coefficients the processing performance is comparable to a rectangular 5x3 matrix.
        /// </summary>
        public static ErrorDiffusionDitherer StevensonArce(bool? byBrightness = null)
            => new ErrorDiffusionDitherer(stevensonArceMatrix, 200, 4, byBrightness);

        #endregion


        #region Instance Methods

        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizer)
            => new ErrorDiffusionDitheringSession(quantizer, this, source);

        #endregion

        #endregion
    }
}
