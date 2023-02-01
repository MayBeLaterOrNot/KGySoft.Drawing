﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataBase.cs
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
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [DebuggerDisplay("{" + nameof(Width) + "}x{" + nameof(Height) + "} {" + nameof(PixelFormat) + "." + nameof(PixelFormatInfo.BitsPerPixel) + "}bpp")]
    internal abstract class BitmapDataBase : IBitmapDataInternal
    {
        #region Fields

        private Action? disposeCallback;
        private Func<Palette, bool>? trySetPaletteCallback;

        // This cache is exposed only for the indexers, which return interface types without MoveNext/MoveToRow methods
        // Non-volatile field because it's even better if the threads see their lastly set instance
        private IBitmapDataRowInternal? cachedRowByIndex;

        // This cache is not exposed to public access, only to the internal GetCachedRow method.
        // Its consumers must always use the result in a local scope where no context switch is possible between threads.
        private volatile StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>?[]? cachedRowByThreadId;
        private int hashMask; // non-volatile because always the volatile cachedRowByThreadId is accessed first

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height { get; protected set; }
        public int Width { get; protected set; }
        public Size Size => new Size(Width, Height);
        public PixelFormatInfo PixelFormat { get; }
        public Color32 BackColor { get; }
        public byte AlphaThreshold { get; }
        public Palette? Palette { get; private set; }
        public int RowSize { get; protected set; }
        public bool IsDisposed { get; private set; }
        public bool CanSetPalette => PixelFormat.Indexed && Palette != null && AllowSetPalette;
        public virtual bool IsCustomPixelFormat => PixelFormat.IsCustomFormat;
        public BlendingMode BlendingMode { get; }

        #endregion

        #region Internal Properties

        internal bool LinearBlending { get; }

        #endregion

        #region Protected Properties

        protected virtual bool AllowSetPalette => true;

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRowMovable IReadableBitmapData.FirstRow => GetFirstRow();
        IWritableBitmapDataRowMovable IWritableBitmapData.FirstRow => GetFirstRow();
        IReadWriteBitmapDataRowMovable IReadWriteBitmapData.FirstRow => GetFirstRow();

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
                return GetCachedRowByIndex(y);
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

        protected BitmapDataBase(in BitmapDataConfig cfg)
        {
            #region Local Methods

            static Palette ExpandPalette(Palette palette, int bpp)
            {
                var entries = new Color32[1 << bpp];
                palette.Entries.CopyTo(entries, 0);
                return new Palette(entries, palette.BackColor, palette.AlphaThreshold, palette.PrefersLinearColorSpace, null);
            }

            #endregion

            Debug.Assert(cfg.Size.Width > 0 && cfg.Size.Height > 0, "Non-empty size expected");
            Debug.Assert(cfg.PixelFormat.BitsPerPixel is > 0 and <= 128);
            Debug.Assert(cfg.Palette == null || cfg.Palette.BackColor == cfg.BackColor.ToOpaque()
                && cfg.Palette.AlphaThreshold == cfg.AlphaThreshold && (cfg.Palette.BlendingMode == cfg.BlendingMode || cfg.BlendingMode == BlendingMode.Default));

            disposeCallback = cfg.DisposeCallback;
            trySetPaletteCallback = cfg.TrySetPaletteCallback;
            Width = cfg.Size.Width;
            Height = cfg.Size.Height;
            BackColor = cfg.BackColor.ToOpaque();
            AlphaThreshold = cfg.AlphaThreshold;
            PixelFormat = cfg.PixelFormat;
            BlendingMode = cfg.BlendingMode;
            LinearBlending = BlendingMode == BlendingMode.Linear || BlendingMode == BlendingMode.Default && PixelFormat.LinearGamma;
            if (!cfg.PixelFormat.Indexed)
                return;

            int bpp = cfg.PixelFormat.BitsPerPixel;
            if (cfg.Palette != null)
            {
                if (cfg.Palette.Count > 1 << bpp)
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(1 << bpp, bpp), "palette");
                Palette = cfg.Palette;
                LinearBlending = Palette.PrefersLinearColorSpace;
                return;
            }

            Palette = cfg.Palette ?? bpp switch
            {
                > 8 => ExpandPalette(Palette.SystemDefault8BppPalette(LinearBlending, cfg.BackColor, cfg.AlphaThreshold), bpp),
                8 => Palette.SystemDefault8BppPalette(LinearBlending, cfg.BackColor, cfg.AlphaThreshold),
                > 4 => ExpandPalette(Palette.SystemDefault4BppPalette(LinearBlending, cfg.BackColor), bpp),
                4 => Palette.SystemDefault4BppPalette(LinearBlending, cfg.BackColor),
                > 1 => ExpandPalette(Palette.SystemDefault1BppPalette(LinearBlending, cfg.BackColor), bpp),
                _ => Palette.SystemDefault1BppPalette(LinearBlending, cfg.BackColor)
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowXOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("x", PublicResources.ArgumentOutOfRange);
        }

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color GetPixel(int x, int y) => GetColor32(x, y).ToColor();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color) => SetColor32(x, y, new Color32(color));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 GetColor32(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetPixel(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetColor32(int x, int y, Color32 color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetPixel(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public IBitmapDataRowInternal GetRowUncached(int y) => DoGetRow(y);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public IBitmapDataRowInternal GetRowCached(int y) => GetCachedRowByThreadId(y);

        public bool TrySetPalette(Palette? palette)
        {
            if (!CanSetPalette || palette == null || palette.Count < Palette!.Count || palette.Count > 1 << PixelFormat.BitsPerPixel)
                return false;

            if (trySetPaletteCallback?.Invoke(palette) == false)
                return false;

            // Inheriting only the color entries from the palette because back color, alpha and blending mode are read-only
            if (palette.BackColor == BackColor && palette.AlphaThreshold == AlphaThreshold && palette.PrefersLinearColorSpace == LinearBlending)
                Palette = palette;
            else
                Palette = new Palette(palette, LinearBlending, BackColor, AlphaThreshold);

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        protected abstract IBitmapDataRowInternal DoGetRow(int y);
        protected abstract Color32 DoGetPixel(int x, int y);
        protected abstract void DoSetPixel(int x, int y, Color32 c);

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
                cachedRowByIndex = null;
                cachedRowByThreadId = null;
                IsDisposed = true;
            }
        }

        #endregion

        #region Private Methods

        private IBitmapDataRowInternal GetFirstRow()
        {
            if (IsDisposed)
                ThrowDisposed();
            return DoGetRow(0);
        }

        private IBitmapDataRowInternal GetMovableRow(int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return DoGetRow(y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private IBitmapDataRowInternal GetCachedRowByIndex(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            // Note: this caching is exposed only to the indexer, which returns an immutable interface where Index cannot be changed
            IBitmapDataRowInternal? cached = cachedRowByIndex;
            if (cached?.Index == y)
                return cached;

            // Otherwise, we create and cache the result.
            return cachedRowByIndex = DoGetRow(y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private IBitmapDataRowInternal GetCachedRowByThreadId(int y)
        {
            if (cachedRowByThreadId == null)
                InitThreadIdCache();
            int threadId = EnvironmentHelper.CurrentThreadId;
            var hash = threadId & hashMask;
            StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>? cached = cachedRowByThreadId![hash];
            if (cached?.Value.ThreadId == threadId)
                cached.Value.Row.DoMoveToRow(y);
            else
                cachedRowByThreadId[hash] = cached = new StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>((threadId, DoGetRow(y)));
            return cached.Value.Row;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitThreadIdCache()
        {
            var result = new StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>[Math.Max(8, ((uint)Environment.ProcessorCount << 1).RoundUpToPowerOf2())];
            hashMask = result.Length - 1;
            cachedRowByThreadId = result;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        IReadableBitmapDataRowMovable IReadableBitmapData.GetMovableRow(int y) => GetMovableRow(y);
        IWritableBitmapDataRowMovable IWritableBitmapData.GetMovableRow(int y) => GetMovableRow(y);
        IReadWriteBitmapDataRowMovable IReadWriteBitmapData.GetMovableRow(int y) => GetMovableRow(y);

        #endregion

        #endregion

        #endregion
    }

}
