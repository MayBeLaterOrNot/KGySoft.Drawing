﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Gray.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Collections.Generic;

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 16-bit grayscale color.
    /// Implements <see cref="IEquatable{T}"/> because used in a <see cref="HashSet{T}"/> in <see cref="BitmapDataExtensions.GetColorCount{T}"/>
    /// </summary>
    internal readonly struct Color16Gray : IEquatable<Color16Gray>
    {
        #region Fields

        internal readonly ushort Value;

        #endregion

        #region Constructors

        internal Color16Gray(Color32 c)
        {
            var c64 = new Color64(c);
            Value = (ushort)(c64.R * ColorExtensions.RLum
                + c64.G * ColorExtensions.GLum
                + c64.B * ColorExtensions.BLum);
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value;

        public bool Equals(Color16Gray other) => Value == other.Value;

        public override bool Equals(object obj) => obj is Color16Gray other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => Color32.FromGray((byte)(Value >> 8));

        #endregion

        #endregion
    }
}