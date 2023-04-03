﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color48.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 48-bit RGB color.
    /// Implements <see cref="IEquatable{T}"/> because used in a <see cref="HashSet{T}"/> in <see cref="BitmapDataExtensions.GetColorCount{T}"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal readonly struct Color48 : IEquatable<Color48>
    {
        #region Fields

        [FieldOffset(0)]internal readonly ushort B;
        [FieldOffset(2)]internal readonly ushort G;
        [FieldOffset(4)]internal readonly ushort R;

        #endregion

        #region Constructors

        internal Color48(Color32 c)
        {
            B = ColorSpaceHelper.ToUInt16(c.B);
            G = ColorSpaceHelper.ToUInt16(c.G);
            R = ColorSpaceHelper.ToUInt16(c.R);
        }

        internal Color48(Color64 c)
        {
            B = c.B;
            G = c.G;
            R = c.R;
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => new Color64(R, G, B).GetHashCode();
        public bool Equals(Color48 other) => R == other.R && G == other.G && B == other.B;
        public override bool Equals(object? obj) => obj is Color48 other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(R), ColorSpaceHelper.ToByte(G), ColorSpaceHelper.ToByte(B));
        internal Color64 ToColor64() => new Color64(R, G, B);

        #endregion

        #endregion
    }
}