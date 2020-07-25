﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow32Argb.cs
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

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapDataRow32Argb : ManagedBitmapDataRowBase<Color32, ManagedBitmapDataRow32Argb>
    {
        #region Methods

        public override Color32 DoGetColor32(int x) => Row[x];

        public override void DoSetColor32(int x, Color32 c) => Row[x] = c;

        #endregion
    }
}