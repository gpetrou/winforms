﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class User32
    {
        [DllImport(Libraries.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern unsafe int GetClassNameW(IntPtr hwnd, char* lpClassName, int nMaxCount);

        public static unsafe int GetClassNameW(HandleRef hwnd, char* lpClassName, int nMaxCount)
        {
            int result = GetClassNameW(hwnd.Handle, lpClassName, nMaxCount);
            GC.KeepAlive(hwnd.Wrapper);
            return result;
        }
    }
}
