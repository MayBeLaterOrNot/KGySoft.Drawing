﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData32PArgb.cs
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedBitmapData32PArgb : UnmanagedBitmapDataBase<UnmanagedBitmapData32PArgb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((PColor32*)Row)[x].ToStraight();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c) => ((PColor32*)Row)[x] = c.ToPremultiplied();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor32 DoGetColor32Premultiplied(int x) => ((PColor32*)Row)[x];

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32Premultiplied(int x, PColor32 c) => ((PColor32*)Row)[x] = c;

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData32PArgb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetPixel(int x, int y) => GetPixelAddress<PColor32>(y, x)->ToStraight();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetPixel(int x, int y, Color32 c) => *GetPixelAddress<PColor32>(y, x) = c.ToPremultiplied();

        #endregion
    }
}
