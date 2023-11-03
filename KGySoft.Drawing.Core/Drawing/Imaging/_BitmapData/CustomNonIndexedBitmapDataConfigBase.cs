﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomNonIndexedBitmapDataConfigBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    public abstract class CustomNonIndexedBitmapDataConfigBase : CustomBitmapDataConfigBase
    {
        #region Properties


        public Color32 BackColor { get; set; }

        public byte AlphaThreshold { get; set; }

        public WorkingColorSpace WorkingColorSpace { get; set; }

        #endregion
    }
}