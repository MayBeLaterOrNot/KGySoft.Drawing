﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba64.cs
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
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorRgba64
    {
        #region Fields

        [FieldOffset(0)]
        private readonly ushort r;
        [FieldOffset(2)]
        private readonly ushort g;
        [FieldOffset(4)]
        private readonly ushort b;
        [FieldOffset(6)]
        private readonly ushort a;

        #endregion

        #region Constructors

        #region Internal Constructors
        
        internal ColorRgba64(Color32 c)
        {
            r = (ushort)((c.R << 8) | c.R);
            g = (ushort)((c.G << 8) | c.G);
            b = (ushort)((c.B << 8) | c.B);
            a = (ushort)((c.A << 8) | c.A);
        }

        #endregion

        #region Private Constructors

        private ColorRgba64(ushort a, ushort r, ushort g, ushort b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        #endregion

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32((byte)(a >> 8), (byte)(r >> 8), (byte)(g >> 8), (byte)(b >> 8));

        internal ColorRgba64 ToStraight() => a switch
        {
            UInt16.MaxValue => this,
            0 => default,
            _ => new ColorRgba64(a,
                (ushort)((uint)r * UInt16.MaxValue / a),
                (ushort)((uint)g * UInt16.MaxValue / a),
                (ushort)((uint)b * UInt16.MaxValue / a))
        };

        internal ColorRgba64 ToPremultiplied() => a switch
        {
            UInt16.MaxValue => this,
            0 => default,
            _ => new ColorRgba64(a,
                (ushort)((uint)r * a / UInt16.MaxValue),
                (ushort)((uint)g * a / UInt16.MaxValue),
                (ushort)((uint)b * a / UInt16.MaxValue))
        };

        #endregion
    }
}