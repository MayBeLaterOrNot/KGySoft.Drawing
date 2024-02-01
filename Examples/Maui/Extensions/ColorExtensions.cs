﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
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

using Microsoft.Maui.Graphics;

#endregion

namespace KGySoft.Drawing.Examples.Maui.Extensions
{
    internal static class ColorExtensions
    {
        #region Methods

        internal static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.ToInt());

        #endregion
    }
}