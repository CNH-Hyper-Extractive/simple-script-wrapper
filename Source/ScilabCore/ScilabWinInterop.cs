//  -----------------------------------------------------------------------
//   Copyright (c) 2014 Tom Bulatewicz, Kansas State University
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//   SOFTWARE.
//  -----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace KansasState.Ssw.ScilabCore
{
    internal class ScilabWinInterop
    {
        private const string CALL_SCILAB_DLL = "call_scilab.dll";
        private const string API_SCILAB_DLL = "api_scilab.dll";

        [DllImport(CALL_SCILAB_DLL, CharSet = CharSet.Ansi)]
        public static extern int StartScilab
            ([In] String SCIpath,
                [In] String ScilabStartup,
                [In] Int32[] Stacksize);

        [DllImport(CALL_SCILAB_DLL, CharSet = CharSet.Ansi)]
        public static extern int SendScilabJob([In] String job);

        [DllImport(API_SCILAB_DLL, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern ApiErr createNamedMatrixOfDouble
            ([In] IntPtr pvApiCtx, [In] String _pstName,
                [In] int _iRows, [In] int _iCols,
                [In] double[] _pdblReal);

        [DllImport(API_SCILAB_DLL, CharSet = CharSet.Ansi)]
        public static extern ApiErr createNamedMatrixOfString
            ([In] IntPtr pvApiCtx, [In] String _pstName,
                [In] int _iRows, [In] int _iCols,
                [In] String[] _pstStrings);

        [DllImport(API_SCILAB_DLL, CharSet = CharSet.Ansi)]
        public static extern unsafe ApiErr readNamedMatrixOfDouble
            ([In] IntPtr pvApiCtx, [In] String _pstName,
                [Out] Int32* _piRows, [Out] Int32* _piCols,
                [In, Out] Double[] _pdblReal);

        [DllImport(CALL_SCILAB_DLL, CharSet = CharSet.Ansi)]
        public static extern int TerminateScilab([In] String ScilabQuit);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ApiCtx
        {
            public String pstName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct ApiErr
        {
            public int iErr;
            public int iMsgCount;
            public fixed int pstructMsg [5];
        }
    }
}