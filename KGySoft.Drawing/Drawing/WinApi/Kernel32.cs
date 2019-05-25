﻿using System;
using System.Runtime.InteropServices;
using System.Security;

namespace KGySoft.Drawing.WinApi
{
    internal static class Kernel32
    {
        [SecurityCritical]
        private static class NativeMethods
        {
            /// <summary>
            /// Copies a block of memory from one location to another.
            /// </summary>
            /// <param name="dest">A pointer to the starting address of the copied block's destination.</param>
            /// <param name="src">A pointer to the starting address of the block of memory to copy.</param>
            /// <param name="length">The size of the block of memory to copy, in bytes.</param>
            [DllImport("kernel32.dll")]
            internal static extern void CopyMemory(IntPtr dest, IntPtr src, int length);
        }

        internal static void CopyMemory(IntPtr dest, IntPtr src, int length) => NativeMethods.CopyMemory(dest, src, length);
    }
}
