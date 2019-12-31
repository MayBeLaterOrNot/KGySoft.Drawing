﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow16Rgb555.cs
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
    internal sealed class BitmapDataRow16Rgb555 : BitmapDataRowNonIndexedBase
    {
        #region Methods

        internal override unsafe Color32 DoGetColor32(int x) => ((Color16Rgb555*)Address)[x].ToColor32();

        internal override unsafe void DoSetColor32(int x, Color32 c) => ((Color16Rgb555*)Address)[x] = new Color16Rgb555(c);

        #endregion
    }
}