﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [DebuggerDisplay("{" + nameof(Width) + "}x{" + nameof(Height) + "} {KGySoft.Drawing." + nameof(PixelFormatExtensions) + "." + nameof(PixelFormatExtensions.ToBitsPerPixel) + "(" + nameof(PixelFormat) + ")}bpp")]
    internal abstract class BitmapDataBase : IBitmapDataInternal
    {
        #region Fields

        private Action? disposeCallback;
        private Func<Palette, bool>? trySetPaletteCallback;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height { get; protected set; }
        public int Width { get; protected set; }
        public PixelFormat PixelFormat { get; }
        public Color32 BackColor { get; }
        public byte AlphaThreshold { get; }
        public Palette? Palette { get; private set; }
        public int RowSize { get; protected set; }
        public bool IsDisposed { get; private set; }
        public bool CanSetPalette => PixelFormat.IsIndexed() && Palette != null && AllowSetPalette;
        public virtual bool IsCustomPixelFormat => !PixelFormat.IsValidFormat();

        #endregion

        #region Protected Properties

        protected virtual bool AllowSetPalette => true;

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => GetFirstRow();
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => GetFirstRow();
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => GetFirstRow();

        #endregion

        #endregion

        #region Indexers

        #region Public Indexers

        public IReadWriteBitmapDataRow this[int y]
        {
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get
            {
                if (IsDisposed)
                    ThrowDisposed();
                if ((uint)y >= Height)
                    ThrowYOutOfRange();
                return DoGetRow(y);
            }
        }

        #endregion

        #region Explicitly Implemented Interface Indexers

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => this[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => this[y];

        #endregion

        #endregion

        #endregion

        #region Construction and Destruction
        
        #region Constructors

        protected BitmapDataBase(Size size, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
        {
            #region Local Methods

            static Palette ExpandPalette(Palette palette, int bpp)
            {
                var entries = new Color32[1 << bpp];
                palette.Entries.CopyTo(entries, 0);
                return new Palette(entries, palette.BackColor, palette.AlphaThreshold);
            }

            #endregion

            Debug.Assert(size.Width > 0 && size.Height > 0, "Non-empty size expected");
            Debug.Assert(pixelFormat.ToBitsPerPixel() is > 0 and <= 128);
            Debug.Assert(palette == null || palette.BackColor == backColor.ToOpaque() && palette.AlphaThreshold == alphaThreshold);

            this.disposeCallback = disposeCallback;
            this.trySetPaletteCallback = trySetPaletteCallback;
            Width = size.Width;
            Height = size.Height;
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            PixelFormat = pixelFormat;
            if (!pixelFormat.IsIndexed())
                return;

            int bpp = pixelFormat.ToBitsPerPixel();
            if (palette != null)
            {
                if (palette.Count > 1 << bpp)
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(1 << bpp, bpp), nameof(palette));
                Palette = palette;
                return;
            }

            Palette = palette ?? bpp switch
            {
                > 8 => ExpandPalette(Palette.SystemDefault8BppPalette(backColor, alphaThreshold), bpp),
                8 => Palette.SystemDefault8BppPalette(backColor, alphaThreshold),
                > 4 => ExpandPalette(Palette.SystemDefault4BppPalette(backColor), bpp),
                4 => Palette.SystemDefault4BppPalette(backColor),
                > 1 => ExpandPalette(Palette.SystemDefault1BppPalette(backColor), bpp),
                _ => Palette.SystemDefault1BppPalette(backColor)
            };

            AlphaThreshold = Palette.AlphaThreshold;
        }


        #endregion

        #region Destructor

        ~BitmapDataBase() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region Protected Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected static void ThrowDisposed() => throw new ObjectDisposedException(null, PublicResources.ObjectDisposed);

        #endregion

        #region Private Methods
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowYOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("y", PublicResources.ArgumentOutOfRange);
        }

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color GetPixel(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return DoGetRow(y).GetColor(x);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            DoGetRow(y).SetColor(x, color);
        }

        public abstract IBitmapDataRowInternal DoGetRow(int y);

        public bool TrySetPalette(Palette? palette)
        {
            if (!CanSetPalette || palette == null || palette.Count < Palette!.Count || palette.Count > 1 << PixelFormat.ToBitsPerPixel())
                return false;

            if (trySetPaletteCallback?.Invoke(palette) == false)
                return false;

            if (palette.BackColor == BackColor && palette.AlphaThreshold == AlphaThreshold)
                Palette = palette;
            else
                Palette = new Palette(palette, BackColor, AlphaThreshold);

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                // may happen if the constructor failed and the call comes from the finalizer
                disposeCallback?.Invoke();
            }
            catch (Exception)
            {
                // From explicit dispose we throw it further but we ignore it from destructor.
                if (disposing)
                    throw;
            }
            finally
            {
                disposeCallback = null;
                trySetPaletteCallback = null;
                IsDisposed = true;
            }
        }

        #endregion

        #region Private Methods

        private IReadWriteBitmapDataRow GetFirstRow()
        {
            if (IsDisposed)
                ThrowDisposed();
            return DoGetRow(0);
        }

        #endregion

        #endregion

        #endregion
    }
}
