﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow8I.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class ManagedBitmapDataRow8I : ManagedBitmapDataRowIndexedBase<ManagedBitmapDataRow8I>
    {
        #region Properties

        protected override uint MaxIndex => 255;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x) => Row[x];

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int colorIndex) => Row[x] = (byte)colorIndex;

        #endregion
    }
}