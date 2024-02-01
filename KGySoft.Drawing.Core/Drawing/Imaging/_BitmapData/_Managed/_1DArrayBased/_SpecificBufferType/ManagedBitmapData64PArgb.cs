﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData64PArgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData64PArgb : ManagedBitmapData1DArrayBase<PColor64, ManagedBitmapData64PArgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<PColor64>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = new PColor64(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetPColor32(int x) => Row[x].ToPColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor32(int x, PColor32 c) => Row[x] = new PColor64(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => Row[x].ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => Row[x] = new PColor64(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => Row[x];

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => Row[x] = c;

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => Row[x].ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => Row[x] = c.ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => Row[x].ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => Row[x] = c.ToPColor64();

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData64PArgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData64PArgb(Array2D<PColor64> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => Buffer[y, x] = new PColor64(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColor32 DoGetPColor32(int x, int y) => Buffer[y, x].ToPColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor32(int x, int y, PColor32 c) => Buffer[y, x] = new PColor64(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color64 DoGetColor64(int x, int y) => Buffer[y, x].ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor64(int x, int y, Color64 c) => Buffer[y, x] = new PColor64(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColor64 DoGetPColor64(int x, int y) => Buffer[y, x];

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => Buffer[y, x] = c;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override ColorF DoGetColorF(int x, int y) => Buffer[y, x].ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorF(int x, int y, ColorF c) => Buffer[y, x] = c.ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColorF DoGetPColorF(int x, int y) => Buffer[y, x].ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => Buffer[y, x] = c.ToPColor64();

        #endregion
    }
}
