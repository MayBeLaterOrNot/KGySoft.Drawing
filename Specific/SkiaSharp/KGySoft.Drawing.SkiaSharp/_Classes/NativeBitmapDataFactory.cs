﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal static class NativeBitmapDataFactory
    {
        #region Methods

        #region Internal Methods
        
        internal static bool TryCreateBitmapData(IntPtr buffer, SKImageInfo info, int stride, SKColor backColor, byte alphaThreshold,
            WorkingColorSpace workingColorSpace, Action? disposeCallback, [MaybeNullWhen(false)]out IReadWriteBitmapData bitmapData)
        {
            info.GetDirectlySupportedColorSpace(out bool srgb, out bool linear);
            bitmapData = srgb ? CreateBitmapDataSrgb(buffer, info, stride, workingColorSpace, backColor, alphaThreshold, disposeCallback)
                : linear ? CreateBitmapDataLinear(buffer, info, stride, workingColorSpace, backColor, alphaThreshold, disposeCallback)
                : null;
            return bitmapData != null;
        }

        #endregion

        #region Private Methods
        
        private static IReadWriteBitmapData CreateBitmapDataSrgb(IntPtr buffer, SKImageInfo info, int stride,
            WorkingColorSpace workingColorSpace, SKColor backColor, byte alphaThreshold, Action? disposeCallback = null)
        {
            Debug.Assert(info.IsDirectlySupported());

            var size = new Size(info.Width, info.Height);
            KnownPixelFormat knownPixelFormat = info.AsKnownPixelFormat();
            Color32 backColor32 = backColor.ToColor32();

            // Natively supported formats
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownPixelFormat, workingColorSpace, backColor32, alphaThreshold, disposeCallback);

            // Custom formats
            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            return info switch
            {
                // Rgba8888/Unpremul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) = new ColorRgba8888Srgb(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Premul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPrgba8888Srgb>(x) = new ColorPrgba8888Srgb(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Opaque, Rgb888x
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb888x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) =
                            new ColorRgba8888Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Gray8
                { ColorType: SKColorType.Gray8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorGray8Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorGray8Srgb>(x) =
                        new ColorGray8Srgb((c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace))
                            .GetBrightness(row.BitmapData.WorkingColorSpace)),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba16161616/Unpremul
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x) = new ColorRgba16161616Srgb(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba16161616/Premul
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPrgba16161616Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPrgba16161616Srgb>(x) = new ColorPrgba16161616Srgb(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba16161616/Opaque
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x) =
                        new ColorRgba16161616Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Bgra1010102/Unpremul
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x) = new ColorBgra1010102Srgb(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Bgra1010102/Premul
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPbgra1010102Srgb>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPbgra1010102Srgb>(x) = new ColorPbgra1010102Srgb(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Bgra1010102/Opaque, Bgr101010x // TODO: from Color64
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Bgr101010x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x) =
                            new ColorBgra1010102Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba1010102/Unpremul
                //{ ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba1010102/Premul
                //{ ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba1010102/Opaque, Rgb101010x
                //{ ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb101010x }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToColor32().ToOpaque(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x)
                //            = new ColorRgba1010102(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Argb4444/Unpremul
                //{ ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorArgb4444>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444>(x) = new ColorArgb4444(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Argb4444/Premul
                //// NOTE: Skia premultiplies the color _before_ converting the pixel format, which is optimal only for black background.
                ////       The KGySoft version premultiplies it _after_ converting, which removes finer gradients for black background but is better generally.
                //{ ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorArgb4444>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444>(x) = new ColorArgb4444(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Argb4444/Opaque
                //{ ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorArgb4444>(x).ToColor32().ToOpaque(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444>(x) =
                //        new ColorArgb4444(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF32/Unpremul
                //{ ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgbaF32>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32>(x) = new ColorRgbaF32(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF32/Premul
                //{ ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgbaF32>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32>(x) = new ColorRgbaF32(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF32/Opaque
                //{ ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgbaF32>(x).ToColor32().ToOpaque(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32>(x) =
                //        new ColorRgbaF32(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF16/Unpremul, RgbaF16Clamped/Unpremul
                //{ ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Unpremul }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16>(x).ToColor32(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16>(x) = new ColorRgbaF16(c),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF16/Premul, RgbaF16Clamped/Premul
                //{ ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Premul }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16>(x).ToStraight().ToColor32(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16>(x) = new ColorRgbaF16(c).ToPremultiplied(),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF16/Opaque, RgbaF16Clamped/Opaque
                //{ ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Opaque }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16>(x).ToColor32().ToOpaque(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16>(x) =
                //            new ColorRgbaF16(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Alpha8
                //{ ColorType: SKColorType.Alpha8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => new Color32(row.UnsafeGetRefAs<byte>(x), 0, 0, 0),
                //    (row, x, c) => row.UnsafeGetRefAs<byte>(x) = c.A,
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Alpha16
                //{ ColorType: SKColorType.Alpha16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => new Color32((byte)(row.UnsafeGetRefAs<ushort>(x) >> 8), 0, 0, 0),
                //    (row, x, c) => row.UnsafeGetRefAs<ushort>(x) = (ushort)(c.A | (c.A << 8)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// AlphaF16
                //{ ColorType: SKColorType.AlphaF16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => new Color32(ColorSpaceHelper.ToByte((float)row.UnsafeGetRefAs<Half>(x)), 0, 0, 0),
                //    (row, x, c) => row.UnsafeGetRefAs<Half>(x) = (Half)(c.A / 255f),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rg88
                //{ ColorType: SKColorType.Rg88 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRg88>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRg88>(x) =
                //        new ColorRg88(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rg1616
                //{ ColorType: SKColorType.Rg1616 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRg1616>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRg1616>(x) =
                //        new ColorRg1616(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rg16F
                //{ ColorType: SKColorType.RgF16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgF16>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgF16>(x) =
                //        new ColorRgF16(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                _ => throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly in the sRGB color space. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."))
            };
        }

        private static IReadWriteBitmapData CreateBitmapDataLinear(IntPtr buffer, SKImageInfo info, int stride,
            WorkingColorSpace workingColorSpace, SKColor backColor, byte alphaThreshold, Action? disposeCallback = null)
        {
            Debug.Assert(info.IsDirectlySupported() && info.AsKnownPixelFormat() == KnownPixelFormat.Undefined);

            var size = new Size(info.Width, info.Height);
            Color32 backColor32 = backColor.ToColor32();

            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            return info switch
            {
                // Bgra8888/Unpremul
                { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = new ColorBgra8888Linear(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Bgra8888/Premul
                { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x) = new ColorPbgra8888Linear(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Bgra8888/Opaque
                { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) =
                        c.A == Byte.MaxValue
                            ? new ColorBgra8888Linear(c)
                            : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                                ? new ColorBgra8888Linear(c.Blend(row.BitmapData.BackColor))
                                : new ColorBgra8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Unpremul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = new ColorRgba8888Linear(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Premul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x) = new ColorPrgba8888Linear(c),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Opaque, Rgb888x
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb888x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) =
                            c.A == Byte.MaxValue
                                ? new ColorRgba8888Linear(c)
                                : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                                    ? new ColorRgba8888Linear(c.Blend(row.BitmapData.BackColor))
                                    : new ColorRgba8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                // Gray8
                { ColorType: SKColorType.Gray8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorGray8Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorGray8Linear>(x) =
                        new ColorGray8Linear((c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.GetPreferredColorSpace()))
                            .GetBrightness(row.BitmapData.WorkingColorSpace)),
                    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba16161616/Unpremul
                //{ ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616>(x) = new ColorRgba16161616(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba16161616/Premul
                //{ ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616>(x) = new ColorRgba16161616(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba16161616/Opaque
                //{ ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616>(x).ToColor32().ToOpaque(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616>(x) = new ColorRgba16161616(c.Blend(row.BitmapData.BackColor)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Bgra1010102/Unpremul
                //{ ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorBgra1010102>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102>(x) = new ColorBgra1010102(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Bgra1010102/Premul
                //{ ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorBgra1010102>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102>(x) = new ColorBgra1010102(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Bgra1010102/Opaque, Bgr101010x
                //{ ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Bgr101010x }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorBgra1010102>(x).ToColor32().ToOpaque(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102>(x) = new ColorBgra1010102(c.Blend(row.BitmapData.BackColor)),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba1010102/Unpremul
                //{ ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba1010102/Premul
                //{ ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rgba1010102/Opaque, Rgb101010x
                //{ ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb101010x }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToColor32().ToOpaque(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c.Blend(row.BitmapData.BackColor)),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Argb4444/Unpremul
                //{ ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorArgb4444>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444>(x) = new ColorArgb4444(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Argb4444/Premul
                //// NOTE: Skia premultiplies the color _before_ converting the pixel format, which is optimal only for black background.
                ////       The KGySoft version premultiplies it _after_ converting, which removes finer gradients for black background but is better generally.
                //{ ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorArgb4444>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444>(x) = new ColorArgb4444(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Argb4444/Opaque
                //{ ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorArgb4444>(x).ToColor32().ToOpaque(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444>(x) = new ColorArgb4444(c.Blend(row.BitmapData.BackColor)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF32/Unpremul
                //{ ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgbaF32>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32>(x) = new ColorRgbaF32(c),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF32/Premul
                //{ ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgbaF32>(x).ToStraight().ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32>(x) = new ColorRgbaF32(c).ToPremultiplied(),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF32/Opaque
                //{ ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgbaF32>(x).ToColor32().ToOpaque(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32>(x) = new ColorRgbaF32(c.Blend(row.BitmapData.BackColor)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF16/Unpremul, RgbaF16Clamped/Unpremul
                //{ ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Unpremul }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16>(x).ToColor32(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16>(x) = new ColorRgbaF16(c),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF16/Premul, RgbaF16Clamped/Premul
                //{ ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Premul }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16>(x).ToStraight().ToColor32(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16>(x) = new ColorRgbaF16(c).ToPremultiplied(),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// RgbaF16/Opaque, RgbaF16Clamped/Opaque
                //{ ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Opaque }
                //    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16>(x).ToColor32().ToOpaque(),
                //        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16>(x) = new ColorRgbaF16(c.Blend(row.BitmapData.BackColor)),
                //        workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Alpha8
                //{ ColorType: SKColorType.Alpha8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => new Color32(row.UnsafeGetRefAs<byte>(x), 0, 0, 0),
                //    (row, x, c) => row.UnsafeGetRefAs<byte>(x) = c.A,
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Alpha16
                //{ ColorType: SKColorType.Alpha16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => new Color32((byte)(row.UnsafeGetRefAs<ushort>(x) >> 8), 0, 0, 0),
                //    (row, x, c) => row.UnsafeGetRefAs<ushort>(x) = (ushort)(c.A | (c.A << 8)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// AlphaF16
                //{ ColorType: SKColorType.AlphaF16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => new Color32(((float)row.UnsafeGetRefAs<Half>(x)).To8Bit(), 0, 0, 0),
                //    (row, x, c) => row.UnsafeGetRefAs<Half>(x) = (Half)(c.A / 255f),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rg88
                //{ ColorType: SKColorType.Rg88 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRg88>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRg88>(x) = new ColorRg88(c.Blend(row.BitmapData.BackColor)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rg1616
                //{ ColorType: SKColorType.Rg1616 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRg1616>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRg1616>(x) = new ColorRg1616(c.Blend(row.BitmapData.BackColor)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                //// Rg16F
                //{ ColorType: SKColorType.RgF16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //    (row, x) => row.UnsafeGetRefAs<ColorRgF16>(x).ToColor32(),
                //    (row, x, c) => row.UnsafeGetRefAs<ColorRgF16>(x) = new ColorRgF16(c.Blend(row.BitmapData.BackColor)),
                //    workingColorSpace, backColor32, alphaThreshold, disposeCallback),

                _ => throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly in the linear color space. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."))
            };
        }

        #endregion

        #endregion
    }
}