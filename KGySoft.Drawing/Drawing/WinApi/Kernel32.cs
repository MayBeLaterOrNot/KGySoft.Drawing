﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Kernel32.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [SecurityCritical]
    internal static class Kernel32
    {
        #region NativeMethods class

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// Copies a block of memory from one location to another.
            /// </summary>
            /// <param name="dest">A pointer to the starting address of the copied block's destination.</param>
            /// <param name="src">A pointer to the starting address of the block of memory to copy.</param>
            /// <param name="length">The size of the block of memory to copy, in bytes.</param>
            [DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory")]
            internal static extern void CopyMemory(IntPtr dest, IntPtr src, int length);

            #endregion
        }

        #endregion

        #region Methods

        internal static void CopyMemory(IntPtr dest, IntPtr src, int length) => NativeMethods.CopyMemory(dest, src, length);

        #endregion
    }
}
