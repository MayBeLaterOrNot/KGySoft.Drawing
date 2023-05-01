﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKSurfaceExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKSurface"/> type.
    /// </summary>
    public static class SKSurfaceExtensions
    {
        #region Methods

        /// <summary>
        /// Gets a managed read-only accessor for an <see cref="SKSurface"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="surface">An <see cref="SKSurface"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result. As SkiaSharp does not support indexed formats
        /// with palette anymore the <paramref name="backColor"/> for the read-only result bitmap data is relevant in very rare cases only, such as cloning by
        /// the <see cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IDitherer?)"/> method or obtaining a quantizer by
        /// the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// in very rare cases such as cloning the result or obtaining a matching quantizer from it. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="surface"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetReadableBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKSurface surface, SKColor backColor = default, byte alphaThreshold = 128)
            => surface.GetReadableBitmapData(WorkingColorSpace.Default, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed read-only accessor for an <see cref="SKSurface"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="surface">An <see cref="SKSurface"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="surface"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result. As SkiaSharp does not support indexed formats
        /// with palette anymore the <paramref name="backColor"/> for the read-only result bitmap data is relevant in very rare cases only, such as cloning by
        /// the <see cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IDitherer?)"/> method or obtaining a quantizer by
        /// the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// in very rare cases such as cloning the result or obtaining a matching quantizer from it. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="surface"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetReadableBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKSurface surface, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface), PublicResources.ArgumentNull);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            // Raster-based surface: We can simply get a bitmap data for its pixels
            SKPixmap? pixels = surface.PeekPixels();
            if (pixels != null)
                return pixels.GetReadableBitmapData(workingColorSpace);

            // fallback: taking a snapshot as an SKImage, and obtaining the bitmap data for that
            // TODO: This will use SKImage.ReadPixels internally, which is another allocation.
            //       Instead, use surface.ReadPixels directly if there will be a surface.Info or surface.Canvas.Info so no Snapshot will be needed: https://github.com/mono/SkiaSharp/issues/2281
            SKImage skImage = surface.Snapshot();
            return skImage.GetBitmapDataInternal(workingColorSpace, backColor.ToColor32(), alphaThreshold, skImage.Dispose);
        }

        /// <summary>
        /// Gets a managed write-only accessor for an <see cref="SKSurface"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="surface">An <see cref="SKSurface"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// As SkiaSharp does not support indexed pixel formats with palette anymore, this parameter is relevant in very rare cases only, such as
        /// obtaining a quantizer by the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="surface"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetWritableBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetWritableBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this SKSurface surface, SKColor backColor = default, byte alphaThreshold = 128)
            => GetReadWriteBitmapData(surface, WorkingColorSpace.Default, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed write-only accessor for an <see cref="SKSurface"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="surface">An <see cref="SKSurface"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="surface"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// As SkiaSharp does not support indexed pixel formats with palette anymore, this parameter is relevant in very rare cases only, such as
        /// obtaining a quantizer by the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="surface"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetWritableBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetWritableBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this SKSurface surface, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => GetReadWriteBitmapData(surface, workingColorSpace, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed read-write accessor for an <see cref="SKSurface"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="surface">An <see cref="SKSurface"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant in some operations such as when drawing another <see cref="IReadableBitmapData"/> instance with alpha into the returned bitmap data
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods and the specified <paramref name="surface"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="surface"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadWriteBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetReadWriteBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKSurface surface, SKColor backColor = default, byte alphaThreshold = 128)
            => GetReadWriteBitmapData(surface, WorkingColorSpace.Default, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed read-write accessor for an <see cref="SKSurface"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="surface">An <see cref="SKSurface"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="surface"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant in some operations such as when drawing another <see cref="IReadableBitmapData"/> instance with alpha into the returned bitmap data
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods and the specified <paramref name="surface"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="surface"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadWriteBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetReadWriteBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKSurface surface, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface), PublicResources.ArgumentNull);

            Action disposeCallback;
            SKPixmap? pixels = surface.PeekPixels();

            // Raster-based surface: getting the pixels directly, and on dispose drawing it back to the surface
            if (pixels != null)
            {
                disposeCallback = () =>
                {
                    using (var bitmap = new SKBitmap())
                    {
                        bitmap.InstallPixels(pixels);
                        surface.Canvas.Clear();
                        surface.Canvas.DrawBitmap(bitmap, SKPoint.Empty, SKBitmapExtensions.CopySourcePaint);
                    }

                    pixels.Dispose();
                };

                return pixels.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold, disposeCallback);
            }

            // Not a raster-based surface: taking a snapshot as an image, converting it to bitmap and doing the same as above
            // TODO: use surface.ReadPixels directly if there will be a surface.Info or surface.Canvas.Info so no Snapshot will be needed: https://github.com/mono/SkiaSharp/issues/2281
            SKBitmap bitmap;
            using (SKImage snapshot = surface.Snapshot())
            {
                SKImageInfo info = snapshot.Info;
                bitmap = new SKBitmap(info);
                if (!snapshot.ReadPixels(info, bitmap.GetPixels()))
                {
                    bitmap.Dispose();
                    throw new ArgumentException(PublicResources.ArgumentInvalid, nameof(surface));
                }
            }

            disposeCallback = () =>
            {
                surface.Canvas.Clear();
                surface.Canvas.DrawBitmap(bitmap, SKPoint.Empty, SKBitmapExtensions.CopySourcePaint);
                bitmap.Dispose();
            };

            return bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold, disposeCallback);
        }

        #endregion
    }
}
