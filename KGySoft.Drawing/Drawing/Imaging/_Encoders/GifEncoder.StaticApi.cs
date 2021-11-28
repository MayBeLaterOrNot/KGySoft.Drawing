﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.Encode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using KGySoft.Collections;
#if !NET35
using System.Threading.Tasks;
#endif
using KGySoft.CoreLibraries;

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Methods

        #region Public Methods

        #region EncodeImage

        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a GIF image and writes it into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the <see cref="GlobalPalette"/>, or, if that is not set,
        /// by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/> using no dithering.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="imageData"/> is not an indexed image or the image contains different alpha pixels,
        /// then <see cref="OptimizedPaletteQuantizer.Wu"/> quantizer will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginEncodeImage">BeginEncodeImage</see>
        /// or <see cref="EncodeImageAsync">EncodeImageAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>To encode an <see cref="Image"/> you can use also the <see cref="O:KGySoft.Drawing.ImageExtensions.SaveAsGif">ImageExtensions.SaveAsGif</see>
        /// methods that provide a higher level access.</para>
        /// <para>To create a GIF completely manually you can create a <see cref="GifEncoder"/> instance that provides a lower level access.</para>
        /// <para>If <paramref name="quantizer"/> is specified, then it will be used even for already indexed images.</para>
        /// <para>If <paramref name="quantizer"/> is an <see cref="OptimizedPaletteQuantizer"/>, then the palette of the result image will be adjusted for the actual image content.</para>
        /// </remarks>
        public static void EncodeImage(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(imageData, stream);
            DoEncodeImage(AsyncContext.Null, imageData, stream, quantizer, ditherer);
        }

        /// <summary>
        /// Begins to encode the specified <paramref name="imageData"/> as a GIF image and to write it into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the <see cref="GlobalPalette"/>, or, if that is not set,
        /// by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/> using no dithering.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="imageData"/> is not an indexed image or the image contains different alpha pixels,
        /// then <see cref="OptimizedPaletteQuantizer.Wu"/> quantizer will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="EncodeImageAsync">EncodeImageAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndEncodeImage">EndEncodeImage</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.
        /// The encoding itself cannot be parallelized. The <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> setting affects only the quantizing session
        /// if <paramref name="imageData"/> has a non-indexed pixel format, or when <paramref name="quantizer"/> is set.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeImage">EncodeImage</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginEncodeImage(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncContext.BeginOperation(ctx => DoEncodeImage(ctx, imageData, stream, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginEncodeImage">BeginEncodeImage</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="EncodeImageAsync">EncodeImageAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndEncodeImage(IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginEncodeImage));

#if !NET35
        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a GIF image asynchronously, and writes it into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the <see cref="GlobalPalette"/>, or, if that is not set,
        /// by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/> using no dithering.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="imageData"/> is not an indexed image or the image contains different alpha pixels,
        /// then <see cref="OptimizedPaletteQuantizer.Wu"/> quantizer will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.
        /// The encoding itself cannot be parallelized. The <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> setting affects only the quantizing session
        /// if <paramref name="imageData"/> has a non-indexed pixel format, or when <paramref name="quantizer"/> is set.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeImage">EncodeImage</see> method for more details.</note>
        /// </remarks>
        public static Task EncodeImageAsync(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncContext.DoOperationAsync(ctx => DoEncodeImage(ctx, imageData, stream, quantizer, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region EncodeAnimation

        /// <summary>
        /// Encodes the frames of the specified <paramref name="configuration"/> as an animated GIF image and writes it into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="configuration">An <see cref="AnimatedGifConfiguration"/> instance describing the configuration of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="configuration"/> is invalid.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginEncodeAnimation">BeginEncodeAnimation</see>
        /// or <see cref="EncodeAnimationAsync">EncodeAnimationAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>To encode <see cref="Image"/> instances with default configuration you can use the <see cref="O:KGySoft.Drawing.ImageExtensions.SaveAsAnimatedGif">ImageExtensions.SaveAsAnimatedGif</see>
        /// methods that provide a higher level access.</para>
        /// <para>To create an animation completely manually you can create a <see cref="GifEncoder"/> instance that provides a lower level access.</para>
        /// </remarks>
        public static void EncodeAnimation(AnimatedGifConfiguration configuration, Stream stream)
        {
            ValidateArguments(configuration, stream);
            DoEncodeAnimation(AsyncContext.Null, configuration, stream);
        }

        /// <summary>
        /// Begins to encode the frames of the specified <paramref name="configuration"/> as an animated GIF image and to write it into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="configuration">An <see cref="AnimatedGifConfiguration"/> instance describing the configuration of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="configuration"/> is invalid.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="EncodeAnimationAsync">EncodeAnimationAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndEncodeAnimation">EndEncodeAnimation</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeAnimation">EncodeAnimation</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginEncodeAnimation(AnimatedGifConfiguration configuration, Stream stream, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(configuration, stream);
            return AsyncContext.BeginOperation(ctx => DoEncodeAnimation(ctx, configuration, stream), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginEncodeAnimation">BeginEncodeAnimation</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="EncodeAnimationAsync">EncodeAnimationAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndEncodeAnimation(IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginEncodeAnimation));

#if !NET35
        /// <summary>
        /// Encodes the frames of the specified <paramref name="configuration"/> as an animated GIF image asynchronously, and writes it into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="configuration">An <see cref="AnimatedGifConfiguration"/> instance describing the configuration of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="configuration"/> is invalid.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeAnimation">EncodeAnimation</see> method for more details.</note>
        /// </remarks>
        public static Task EncodeAnimationAsync(AnimatedGifConfiguration configuration, Stream stream, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(configuration, stream);
            return AsyncContext.DoOperationAsync(ctx => DoEncodeAnimation(ctx, configuration, stream), asyncConfig);
        }
#endif

        #endregion

        #region EncodeHighColorImage

        public static void EncodeHighColorImage(IReadableBitmapData imageData, Stream stream, bool allowFullScan = false, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(imageData, stream);
            DoEncodeHighColorImage(AsyncContext.Null, imageData, stream, backColor, alphaThreshold, allowFullScan);
        }

        public static IAsyncResult BeginEncodeHighColorImage(IReadableBitmapData imageData, Stream stream, bool allowFullScan = false, Color32 backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncContext.BeginOperation(ctx => DoEncodeHighColorImage(ctx, imageData, stream, backColor, alphaThreshold, allowFullScan), asyncConfig);
        }

        public static void EndEncodeHighColorImage(IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginEncodeHighColorImage));

        public static Task EncodeHighColorImageAsync(IReadableBitmapData imageData, Stream stream, bool allowFullScan = false, Color32 backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncContext.DoOperationAsync(ctx => DoEncodeHighColorImage(ctx, imageData, stream, backColor, alphaThreshold, allowFullScan), asyncConfig);
        }

        #endregion

        #endregion

        #region Private Methods

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it's called Validate")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static void ValidateArguments(AnimatedGifConfiguration configuration, Stream stream)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (configuration.AnimationMode < AnimationMode.PingPong || (int)configuration.AnimationMode > UInt16.MaxValue)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(configuration.AnimationMode), PublicResources.ArgumentOutOfRange), nameof(configuration));
            if (!configuration.SizeHandling.IsDefined())
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(configuration.SizeHandling), PublicResources.EnumOutOfRange(configuration.SizeHandling)), nameof(configuration));
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it's called Validate")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static void ValidateArguments(IReadableBitmapData imageData, Stream stream)
        {
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (imageData.Width < 1 || imageData.Height < 1)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(imageData));
        }

        private static void DoEncodeImage(IAsyncContext context, IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer, IDitherer? ditherer)
        {
            IReadableBitmapData? source = quantizer == null && imageData.PixelFormat.IsIndexed() && !HasMultipleTransparentIndices(context, imageData)
                ? imageData
                : imageData.DoClone(context, PixelFormat.Format8bppIndexed, quantizer
                    ?? (imageData.PixelFormat == PixelFormat.Format16bppGrayScale
                        ? PredefinedColorsQuantizer.Grayscale()
                        : OptimizedPaletteQuantizer.Wu()), ditherer);

            // cancel occurred
            if (source == null)
                return;

            try
            {
                context.Progress?.New(DrawingOperation.Saving);
                Palette palette = source.Palette!;
                new GifEncoder(stream, imageData.GetSize())
                    {
                        GlobalPalette = palette,
                        BackColorIndex = (byte)(palette.HasAlpha ? palette.TransparentIndex : 0),
#if DEBUG
                        AddMetaInfo = true,
#endif
                    }
                    .AddImage(source)
                    .FinalizeEncoding();
            }
            finally
            {
                if (!ReferenceEquals(source, imageData))
                    source.Dispose();
            }
        }

        private static void DoEncodeAnimation(IAsyncContext context, AnimatedGifConfiguration configuration, Stream stream)
        {
            using var enumerator = new FramesEnumerator(configuration, context);
            using GifEncoder? encoder = enumerator.CreateEncoder(stream);
            if (encoder == null)
                return;

            while (enumerator.MoveNext())
            {
                encoder.AddImage(enumerator.Frame!, enumerator.Location, enumerator.Delay, enumerator.DisposalMethod);
                enumerator.ReportProgress();
            }
        }

        private static void DoEncodeHighColorImage(IAsyncContext context, IReadableBitmapData imageData, Stream stream, Color32 backColor, byte alphaThreshold, bool fullScan)
        {
            #region Local Methods

            static Color32 GetColor(Color32 color, Color32 backColor, byte alphaThreshold) => color.A == Byte.MaxValue ? color
                : color.A >= alphaThreshold ? color.BlendWithBackground(backColor)
                : default;

            #endregion

            // redirecting for an already indexed image
            if (imageData.PixelFormat.ToBitsPerPixel() <= 8 && imageData.Palette != null)
            {
                DoEncodeImage(context, imageData, stream, PredefinedColorsQuantizer.FromCustomPalette(new Palette(imageData.Palette, backColor, alphaThreshold)), null);
                return;
            }

            // TODO: use array for colors if faster
            // TODO: parallelize
            // TODO: progress
            // TODO: reduce complexity if possible
            // TODO: refactoring to not use additional colors?
            Size size = imageData.GetSize();
            var currentColors = new HashSet<Color32>(256);
            var additionalColors = new HashSet<Color32>();
            var sourceRows = new IReadableBitmapDataRow[16];
            var maskRows = new IReadableBitmapDataRow[16];
            Point currentOrigin = Point.Empty;
            using IReadWriteBitmapData mask = BitmapDataFactory.CreateBitmapData(size, PixelFormat.Format1bppIndexed);

            using GifEncoder encoder = new GifEncoder(stream, size)
            {
#if DEBUG
                AddMetaInfo = true
#endif
            };

            // while not reaching the bottom-right corner
            while (currentOrigin.Y < size.Height)
            {
                int x, y;
                Color32 color;
                currentColors.Clear();

                // 1.) Collecting the colors of an up to 16x16 block
                Rectangle currentRegion = new Rectangle(currentOrigin, new Size(Math.Min(16, size.Width - currentOrigin.X), Math.Min(16, size.Height - currentOrigin.Y)));
                for (y = 0; y < currentRegion.Height; y++)
                {
                    IReadableBitmapDataRow row = sourceRows[y] = imageData[currentRegion.Top + y];
                    IReadableBitmapDataRow maskRow = maskRows[y] = mask[currentRegion.Top + y];
                    for (x = currentRegion.Left; x < currentRegion.Right; x++)
                        currentColors.Add(maskRow.GetColorIndex(x) == 0 ? GetColor(row[x], backColor, alphaThreshold) : default);
                }

                // 2.) Expanding the region to the right
                int additionalLimit;
                while (currentRegion.Right < size.Width)
                {
                    Debug.Assert(currentRegion.Height <= 16);
                    additionalColors.Clear();
                    additionalLimit = 256 - currentColors.Count;

                    for (y = 0; y < currentRegion.Height && additionalColors.Count <= additionalLimit; y++)
                    {
                        if (maskRows[y].GetColorIndex(currentRegion.Right) != 0)
                        {
                            additionalColors.Add(default);
                            continue;
                        }

                        color = GetColor(sourceRows[y][currentRegion.Right], backColor, alphaThreshold);
                        if (!currentColors.Contains(color))
                            additionalColors.Add(color);
                    }

                    // could not complete the new column
                    if (y != currentRegion.Height || additionalColors.Count > additionalLimit)
                    {
                        // adding as many colors as can but not expanding region any more
                        if (additionalLimit > 0)
                        {
                            currentColors.Add(default);
                            foreach (Color32 c in additionalColors)
                            {
                                if (currentColors.Count == 256)
                                    break;
                                currentColors.Add(c);
                            }
                        }

                        break;
                    }

                    // the region can be expanded
                    currentColors.AddRange(additionalColors);
                    currentRegion.Width += 1;
                }

                currentOrigin.X += currentRegion.Width;

                // 3.) Expanding the region to the bottom
                while (currentRegion.Bottom < size.Height)
                {
                    additionalColors.Clear();
                    additionalLimit = 256 - currentColors.Count;
                    IReadableBitmapDataRow row = imageData[currentRegion.Bottom];
                    IReadableBitmapDataRow maskRow = mask[currentRegion.Bottom];
                    for (x = 0; x < currentRegion.Width && additionalColors.Count <= additionalLimit; x++)
                    {
                        if (maskRow.GetColorIndex(x + currentRegion.Left) != 0)
                        {
                            additionalColors.Add(default);
                            continue;
                        }

                        color = GetColor(row[x + currentRegion.Left], backColor, alphaThreshold);
                        if (!currentColors.Contains(color))
                            additionalColors.Add(color);
                    }

                    // could not complete the new row
                    if (x != currentRegion.Width || additionalColors.Count > additionalLimit)
                    {
                        // adding as many colors as can but not expanding region any more
                        if (additionalLimit > 0)
                        {
                            currentColors.Add(default);
                            foreach (Color32 c in additionalColors)
                            {
                                if (currentColors.Count == 256)
                                    break;
                                currentColors.Add(c);
                            }
                        }

                        break;
                    }

                    // the region can be expanded
                    currentColors.AddRange(additionalColors);
                    currentRegion.Height += 1;
                }

                if (currentOrigin.X == size.Width)
                    currentOrigin.Y += currentRegion.Left == 0 ? currentRegion.Height : Math.Min(16, currentRegion.Height);

                // 4.) Expanding the palette while can
                if (fullScan && currentColors.Contains(default))
                {
                    for (y = currentRegion.Right == size.Width ? currentRegion.Bottom : currentRegion.Top; y < size.Height && currentColors.Count < 256; y++)
                    {
                        IReadableBitmapDataRow row = imageData[y];
                        IReadableBitmapDataRow maskRow = mask[y];
                        for (x = y < currentRegion.Bottom ? currentRegion.Right : 0; x < size.Width && currentColors.Count < 256; x++)
                        {
                            if (maskRow.GetColorIndex(x) == 0)
                                currentColors.Add(GetColor(row[x], backColor, alphaThreshold));
                        }
                    }
                }

                // 5.) Adding the layer
                Palette palette = new Palette(currentColors.ToArray(), backColor, alphaThreshold);
                if (palette.HasAlpha)
                {
                    Rectangle layerRegion;
                    if (fullScan)
                        layerRegion = new Rectangle(0, currentRegion.Top, size.Width, size.Height - currentRegion.Top);
                    else
                    {
                        layerRegion = currentRegion;
                        if (currentRegion.Width < size.Width)
                            layerRegion.Width += 1;
                        else
                            layerRegion.Height += 1;
                        layerRegion.Intersect(new Rectangle(Point.Empty, size));
                    }

                    using IReadWriteBitmapData layer = BitmapDataFactory.CreateBitmapData(layerRegion.Size, PixelFormat.Format8bppIndexed, palette);
                    if (palette.TransparentIndex != 0)
                        layer.DoClear(context, default);

                    // filling up colors in the whole remaining image
                    // TODO: parallel
                    for (y = 0; y < layerRegion.Height; y++)
                    {
                        IReadableBitmapDataRow rowSource = imageData[layerRegion.Top + y];
                        IReadWriteBitmapDataRow rowMask = mask[layerRegion.Top + y];
                        IReadWriteBitmapDataRow rowLayer = layer[y];

                        for (x = 0; x < layerRegion.Width; x++)
                        {
                            // already masked out
                            if (rowMask.GetColorIndex(x + layerRegion.Left) != 0)
                                continue;
                            color = GetColor(rowSource[x + layerRegion.Left], backColor, alphaThreshold);

                            // cannot include yet
                            if (!currentColors.Contains(color))
                                continue;

                            // can include, masking out
                            rowMask.SetColorIndex(x + layerRegion.Left, 1);
                            if (color.A != 0)
                                rowLayer[x] = color;
                        }
                    }

                    layerRegion = GetContentArea(layer, true);
                    IReadWriteBitmapData clipped = layer.Clip(layerRegion);
                    if (!layerRegion.IsEmpty)
                    {
                        if (fullScan)
                            layerRegion.Y += currentRegion.Top;
                        else
                            layerRegion.Location += new Size(currentRegion.Location);
                        encoder.AddImage(clipped, layerRegion.Location, 10);
                    }
                }
                else
                {
                    // No transparent color: just adding the current region (not using the currentLayer here because a clipped image is slower in AddImage)
                    using IReadWriteBitmapData layer = imageData.DoClone(context, currentRegion, PixelFormat.Format8bppIndexed, new Palette(currentColors.ToArray(), backColor, alphaThreshold));
                    encoder.AddImage(layer, currentRegion.Location, 10);
                }

                // 6.) Adjusting origin for the next session
                if (currentOrigin.X != size.Width)
                    continue;
                
                // trying to skip complete rows
                currentOrigin.X = 0;
                while (currentOrigin.Y < size.Height)
                {
                    IReadableBitmapDataRow rowSource = imageData[currentOrigin.Y];
                    IReadWriteBitmapDataRow rowMask = mask[currentOrigin.Y];
                    for (x = 0; x < size.Width; x++)
                    {
                        if (rowMask.GetColorIndex(x) == 0 && GetColor(rowSource[x], backColor, alphaThreshold).A != 0)
                            break;
                    }

                    if (x != size.Width)
                        break;

                    // a complete row can be skipped
                    currentOrigin.Y += 1;
                }
            }
        }

        private static bool HasMultipleTransparentIndices(IAsyncContext context, IReadableBitmapData imageData)
        {
            Debug.Assert(imageData.PixelFormat.IsIndexed());
            Palette? palette = imageData.Palette;

            // There is no palette or it is too large: returning true to force a quantization
            if (palette == null || palette.Count > 256)
                return true;

            // no transparency: we are done
            if (!palette.HasAlpha)
                return false;

            // we need to check whether the palette has multiple transparent entries (or entries with partial transparency)
            bool multiAlpha = false;
            int transparentIndex = palette.TransparentIndex;
            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i].A < Byte.MaxValue && i != transparentIndex)
                {
                    multiAlpha = true;
                    break;
                }
            }

            // no multiple transparent entries
            if (!multiAlpha)
                return false;

            // we need to scan the image to check whether alpha pixels other than transparent index is in use
            int width = imageData.Width;

            // sequential processing
            if (width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, imageData.Height);
                IReadableBitmapDataRow row = imageData.FirstRow;
                do
                {
                    if (context.IsCancellationRequested)
                        return false;
                    for (int x = 0; x < imageData.Width; x++)
                    {
                        int index = row.GetColorIndex(x);
                        if (index != transparentIndex && palette[index].A < Byte.MaxValue)
                            return true;
                    }

                    context.Progress?.Increment();
                } while (row.MoveNextRow());

                return false;
            }

            // parallel processing
            bool result = false;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, imageData.Height, y =>
            {
                if (Volatile.Read(ref result))
                    return;
                IReadableBitmapDataRow row = imageData[y];
                int w = width;
                int ti = transparentIndex;
                Color32[] paletteEntries = palette.Entries;
                for (int x = 0; x < w; x++)
                {
                    int index = row.GetColorIndex(x);
                    if (index != ti && paletteEntries[index].A < Byte.MaxValue)
                    {
                        Volatile.Write(ref result, true);
                        return;
                    }
                }
            });

            return result;
        }

        private static Rectangle GetContentArea(IReadableBitmapData imageData, bool allowEmpty = false)
        {
            Rectangle result = new Rectangle(0, 0, imageData.Width, imageData.Height);
            if (!imageData.HasAlpha())
                return result;

            IReadableBitmapDataRow row = imageData.FirstRow;
            do
            {
                for (int x = 0; x < result.Width; x++)
                {
                    if (row[x].A != 0)
                        goto continueBottom;
                }

                result.Y += 1;
                result.Height -= 1;
            } while (row.MoveNextRow());

        continueBottom:
            // fully transparent image: returning 1x1 at the center
            if (result.Height == 0)
                return allowEmpty ? Rectangle.Empty : new Rectangle(imageData.Width >> 1, imageData.Height >> 1, 1, 1);

            for (int y = result.Bottom - 1; y >= result.Top; y--)
            {
                row = imageData[y];
                for (int x = 0; x < result.Width; x++)
                {
                    if (row[x].A != 0)
                        goto continueLeft;
                }

                result.Height -= 1;
            }

        continueLeft:
            Debug.Assert(result.Height > 0);
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = result.Top; y < result.Bottom; y++)
                {
                    if (imageData[y][x].A != 0)
                        goto continueRight;
                }

                result.X += 1;
                result.Width -= 1;
            }

        continueRight:
            Debug.Assert(result.Width > 0);
            for (int x = result.Right - 1; x >= result.Left; x--)
            {
                for (int y = result.Top; y < result.Bottom; y++)
                {
                    if (imageData[y][x].A != 0)
                        return result;
                }

                result.Width -= 1;
            }

            throw new InvalidOperationException(Res.InternalError("Empty result is not expected at this point"));
        }

        #endregion

        #endregion
    }
}