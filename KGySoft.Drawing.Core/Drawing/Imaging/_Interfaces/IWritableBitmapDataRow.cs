﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IWritableBitmapDataRow.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides a fast write-only access to a single row of an <see cref="IWritableBitmapData"/>.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a>method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapDataRow"/>
    /// <seealso cref="IWritableBitmapData"/>
    public interface IWritableBitmapDataRow : IBitmapDataRow
    {
        #region Indexers

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <value>A <see cref="Color32"/> instance that represents the color of the specified pixel.</value>
        /// <remarks>
        /// <para>To set the color from a <see cref="Color"/> structure you can use also the <see cref="SetColor">SetColor</see> method but this member has a slightly better performance.</para>
        /// <para>The color value represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="KnownPixelFormat"/>. To access the actual <see cref="KnownPixelFormat"/>-dependent raw data
        /// use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set is not supported by owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="SetColor"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="WriteRaw{T}"/>
        Color32 this[int x] { set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color"/> instance that represents the color of the specified pixel.</param>
        /// <remarks>
        /// <para>If you don't really need to set the pixel color from a 20 byte wide <see cref="Color"/> structure (16 bytes on 32-bit targets), then you can use the
        /// <see cref="this">indexer</see> for a slightly better performance, which uses the more compact 4-byte <see cref="Color32"/> structure.</para>
        /// <para>The specified <paramref name="color"/> represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="KnownPixelFormat"/>. To access the actual <see cref="KnownPixelFormat"/>-dependent raw data
        /// use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set is not supported by owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColor(int x, Color color);

        /// <summary>
        /// If the owner <see cref="IWritableBitmapData"/> has an indexed pixel format, then sets the color index of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the color index to set.</param>
        /// <param name="colorIndex">A palette index that represents the color to be set.</param>
        /// <remarks>
        /// <para>This method can be used only if the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IWritableBitmapData"/> returns an indexed format
        /// (which are <see cref="KnownPixelFormat.Format8bppIndexed"/>, <see cref="KnownPixelFormat.Format4bppIndexed"/> and <see cref="KnownPixelFormat.Format1bppIndexed"/>).
        /// Otherwise, this method throws an <see cref="InvalidOperationException"/>.</para>
        /// <para>To set the actual color of the pixel at the <paramref name="x"/> coordinate you can use the <see cref="SetColor">SetColor</see> method or
        /// the <see cref="this">indexer</see>.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="IWritableBitmapDataRow"/> does not belong to a row of an indexed <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColor"/>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColorIndex(int x, int colorIndex);

        /// <summary>
        /// Sets the underlying raw value within the current <see cref="IWritableBitmapDataRow"/> at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <typeparam name="T">The type of the value to write. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to write. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <param name="data">The raw value to write.</param>
        /// <remarks>
        /// <para>This method writes the actual raw underlying data. <typeparamref name="T"/> can have any size so you by using this method you can write multiple pixels as well as individual color channels.</para>
        /// <para>To determine the row width in bytes use the <see cref="IBitmapData.RowSize"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
        /// <para>To determine the actual pixel size use the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IWritableBitmapData"/> instance.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to write multiple pixels by a single <see cref="WriteRaw{T}">WriteRaw</see> call:
        /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
        /// <code lang="C#"><![CDATA[
        /// using (Bitmap bmp4bppIndexed = new Bitmap(8, 1, PixelFormat.Format4bppIndexed))
        /// using (IReadWriteBitmapData bitmapData = bmp4bppIndexed.GetReadWriteBitmapData())
        /// {
        ///     IReadWriteBitmapDataRow row = bitmapData[0];
        ///
        ///     // Writing as uint writes 8 pixels at once in case of a 4 BPP indexed bitmap:
        ///     row.WriteRaw<uint>(0, 0x12345678);
        ///
        ///     // because of little endianness and 4 BPP pixel order the color indices will be printed
        ///     // in the following order: 7, 8, 5, 6, 3, 4, 1, 2
        ///     for (int x = 0; x < bitmapData.Width; x++)
        ///         Console.WriteLine(row.GetColorIndex(x));
        /// }]]></code>
        /// <note type="tip">See also the example at the <strong>Examples</strong> section of the <see cref="IReadableBitmapDataRow.ReadRaw{T}">IReadableBitmapDataRow.ReadRaw</see> method.</note>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or the memory location of the value (considering the size of <typeparamref name="T"/>)
        /// at least partially exceeds the bounds of the current row.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColor"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="IReadableBitmapDataRow.ReadRaw{T}"/>
        void WriteRaw<T>(int x, T data) where T : unmanaged;

        #endregion
    }
}