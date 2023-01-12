﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Gray.cs
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
using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData16Gray : ManagedBitmapData1DArrayBase<Color16Gray, ManagedBitmapData16Gray.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<Color16Gray>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => Row[x] = new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.PrefersLinearBlending));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Gray(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData16Gray(Array2D<Color16Gray> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
            => Buffer[y, x] = new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, PrefersLinearBlending));

        #endregion
    }
}
