﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKSurfaceExtensions.cs
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

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    public static class SKSurfaceExtensions
    {
        #region Methods

        public static IReadableBitmapData GetReadableBitmapData(this SKSurface surface)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface), PublicResources.ArgumentNull);

            // Raster-based surface: We can simply get a bitmap data for its pixels
            SKPixmap? pixels = surface.PeekPixels();
            if (pixels != null)
                return pixels.GetReadableBitmapData();

            // fallback: taking a snapshot as an SKImage, and obtaining the bitmap data for that
            SKImage skImage = surface.Snapshot();
            return skImage.GetBitmapDataInternal(disposeCallback: skImage.Dispose);
        }

        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKSurface surface, SKColor backColor = default, byte alphaThreshold = 128)
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
                        surface.Canvas.DrawBitmap(bitmap, SKPoint.Empty);
                    }

                    pixels.Dispose();
                };

                return pixels.GetBitmapDataInternal(backColor, alphaThreshold, disposeCallback);
            }

            // Not a raster-based surface: taking a snapshot as an image, converting it to bitmap and doing the same as above
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
                surface.Canvas.DrawBitmap(bitmap, SKPoint.Empty);
                bitmap.Dispose();
            };

            return bitmap.GetBitmapDataInternal(backColor, alphaThreshold, disposeCallback);
        }

        #endregion
    }
}
