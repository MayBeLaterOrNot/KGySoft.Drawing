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
using System.Drawing.Imaging;
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class NativeBitmapDataFactory
    {
        #region Methods

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> from a <see cref="Bitmap"/>.
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "Very straightforward switch with many conditions. Would be OK without the libgdiplus special handling for 16bpp RGB555/565 formats.")]
        [SuppressMessage("VisualStudio.Style", "IDE0039: Use local function instead of lambda", Justification = "False alarm, it would be converted to a delegate anyway.")]
        internal static IReadWriteBitmapData CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128, Palette? palette = null)
        {
            PixelFormat pixelFormat = bitmap.PixelFormat;

            // On Linux with libgdiplus 16bpp formats can be accessed only via 24bpp bitmap data
            PixelFormat bitmapDataPixelFormat = OSUtils.IsWindows
                ? pixelFormat
                : pixelFormat is PixelFormat.Format16bppRgb565 or PixelFormat.Format16bppRgb555
                    ? PixelFormat.Format24bppRgb
                    : pixelFormat;

            Size size = bitmap.Size;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, size), lockMode, bitmapDataPixelFormat);
            Action dispose = () => bitmap.UnlockBits(bitmapData);
            KnownPixelFormat knownPixelFormat = pixelFormat.ToKnownPixelFormatInternal();

            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose);

                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    Debug.Assert(palette == null || palette.Equals(bitmap.Palette.Entries), "Non-null palette entries must match actual palette. Expected to be passed to re-use its cache only.");
                    palette ??= new Palette(bitmap.Palette.Entries, backColor.ToColor(), alphaThreshold);
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, palette, bitmap.TrySetPalette, dispose);

                case PixelFormat.Format64bppArgb:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(64) { HasAlpha = true },
                        (row, x) => row.UnsafeGetRefAs<GdiPColor64>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPColor64>(x) = new GdiPColor64(c),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format64bppPArgb:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(64) { HasPremultipliedAlpha = true },
                        (row, x) => row.UnsafeGetRefAs<GdiPColor64>(x).ToStraight().ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPColor64>(x) = new GdiPColor64(c).ToPremultiplied(),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format48bppRgb:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(48),
                        (row, x) => row.UnsafeGetRefAs<GdiPColor48>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPColor48>(x) = new GdiPColor48(c.Blend(row.BitmapData.BackColor)),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb565:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => row.UnsafeGetRefAs<Color16As24>(x).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color16As24>(x) = new Color16As24(c.Blend(row.BitmapData.BackColor), true),
                            backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb555:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => row.UnsafeGetRefAs<Color16As24>(x).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color16As24>(x) = new Color16As24(c.Blend(row.BitmapData.BackColor), false),
                            backColor, alphaThreshold, dispose);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        #endregion
    }
}
