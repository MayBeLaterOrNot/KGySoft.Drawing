﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKImageExtensions.cs
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
    /// <summary>
    /// Contains extension methods for the <see cref="SKImage"/> type.
    /// </summary>
    public static class SKImageExtensions
    {
        #region Methods

        #region Public Methods

        public static IReadableBitmapData GetReadableBitmapData(this SKImage image) => image.GetBitmapDataInternal();

        #endregion

        #region Internal Methods

        internal static IReadableBitmapData GetBitmapDataInternal(this SKImage image, Action? disposeCallback = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            SKPixmap? pixels = image.PeekPixels();

            // Raster-based image: We can simply get a bitmap data for its pixels
            if (pixels != null)
                return pixels.GetBitmapDataInternal(true, disposeCallback: disposeCallback);

            // Other image: converting it to a bitmap
            // TODO: test if this works for GPU/vector images
            SKImageInfo imageInfo = image.Info;
            var bitmap = new SKBitmap(imageInfo);
            if (!image.ReadPixels(imageInfo, bitmap.GetPixels()))
            {
                bitmap.Dispose();
                disposeCallback?.Invoke();
                throw new ArgumentException(PublicResources.ArgumentInvalid, nameof(image));
            }

            Action disposeBitmap = disposeCallback == null
                ? bitmap.Dispose
                : () =>
                {
                    bitmap.Dispose();
                    disposeCallback();
                };

            return bitmap.GetBitmapDataInternal(true, disposeCallback: disposeBitmap);
        }

        #endregion

        #endregion
    }
}
