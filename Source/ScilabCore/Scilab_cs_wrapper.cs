/*
 * Scilab ( http://www.scilab.org/ ) - This file is part of Scilab
 * Copyright (C) 2009 - DIGITEO - Allan CORNET
 * 
 * This file must be used under the terms of the CeCILL.
 * This source file is licensed as described in the file COPYING, which
 * you should have received as part of this distribution.  The terms
 * are also available at    
 * http://www.cecill.info/licences/Licence_CeCILL_V2-en.txt
 *
 */
//=============================================================================
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//=============================================================================
namespace DotNetScilab
{
    class Scilab_cs_wrapper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct api_Ctx
        {
            public String pstName; /**< Function name */
        }
        //=============================================================================
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct api_Err
        {
            public int iErr;
            public int iMsgCount;
            public fixed int pstructMsg[5];
        }

        private static bool isWindows = (System.Environment.OSVersion.Platform != PlatformID.Unix);

        public static int SendScilabJob([In]String job)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.SendScilabJob(job);
            else
                return Scilab_cs_wrapper_unix.SendScilabJob(job);
        }

        public static int StartScilab([In] String SCIpath,
                                              [In] String ScilabStartup,
                                              [In] Int32[] Stacksize)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.StartScilab(SCIpath,
                                              ScilabStartup,
                                              Stacksize);
            else
                return Scilab_cs_wrapper_unix.StartScilab(SCIpath,
                                              ScilabStartup,
                                              Stacksize);
        }

        public static int TerminateScilab([In] String ScilabQuit)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.TerminateScilab(ScilabQuit);
            else
                return Scilab_cs_wrapper_unix.TerminateScilab(ScilabQuit);
        }

        public static void DisableInteractiveMode()
        {
            if (isWindows == true)
                Scilab_cs_wrapper_win.DisableInteractiveMode();
            else
                Scilab_cs_wrapper_unix.DisableInteractiveMode();
        }

        public static Scilab_cs_wrapper.api_Err createNamedMatrixOfString([In]IntPtr pvApiCtx, [In] String _pstName,
                                                            [In] int _iRows, [In] int _iCols,
                                                            [In] String[] _pstStrings)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.createNamedMatrixOfString(pvApiCtx, _pstName,
                                                            _iRows, _iCols,
                                                            _pstStrings);
            else
                return Scilab_cs_wrapper_unix.createNamedMatrixOfString(pvApiCtx, _pstName,
                                                            _iRows, _iCols,
                                                             _pstStrings);
        }

        public static Scilab_cs_wrapper.api_Err createNamedMatrixOfWideString([In]IntPtr pvApiCtx,
                                                            [In] String _pstName,
                                                            [In] int _iRows, [In] int _iCols,
                                                            [In] String[] _pstStrings)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.createNamedMatrixOfWideString(pvApiCtx,
                                                            _pstName,
                                                            _iRows, _iCols,
                                                            _pstStrings);
            else
                return Scilab_cs_wrapper_unix.createNamedMatrixOfWideString(pvApiCtx,
                                                            _pstName,
                                                             _iRows, _iCols,
                                                             _pstStrings);
        }

        public static Scilab_cs_wrapper.api_Err createNamedMatrixOfDouble([In]IntPtr pvApiCtx, [In] String _pstName,
                                                            [In] int _iRows, [In] int _iCols,
                                                            [In] double[] _pdblReal)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.createNamedMatrixOfDouble(pvApiCtx, _pstName,
                                                            _iRows, _iCols,
                                                            _pdblReal);
            else
                return Scilab_cs_wrapper_unix.createNamedMatrixOfDouble(pvApiCtx, _pstName,
                                                             _iRows, _iCols,
                                                             _pdblReal);
        }

        public static Scilab_cs_wrapper.api_Err createNamedMatrixOfBoolean([In]IntPtr pvApiCtx, [In] String _pstName,
                                                            [In] int _iRows, [In] int _iCols,
                                                            [In] int[] _piBool)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.createNamedMatrixOfBoolean(pvApiCtx, _pstName,
                                                             _iRows, _iCols,
                                                             _piBool);
            else
                return Scilab_cs_wrapper_unix.createNamedMatrixOfBoolean(pvApiCtx, _pstName,
                                                           _iRows, _iCols,
                                                            _piBool);
        }

        public unsafe static Scilab_cs_wrapper.api_Err createNamedMatrixOfInteger32([In]IntPtr pvApiCtx, [In] String _pstName,
                                                           [In] int _iRows, [In] int _iCols,
                                                           [In] int[] _piData)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.createNamedMatrixOfInteger32(pvApiCtx, _pstName,
                                                           _iRows, _iCols,
                                                            _piData);
            else
                return Scilab_cs_wrapper_unix.createNamedMatrixOfInteger32(pvApiCtx, _pstName,
                                                            _iRows, _iCols,
                                                           _piData);
        }

        public unsafe static Scilab_cs_wrapper.api_Err createNamedComplexMatrixOfDouble([In]IntPtr pvApiCtx, [In] String _pstName,
                                                            [In] int _iRows, [In] int _iCols,
                                                            [In] double[] _pdblReal,
                                                            [In] double[] _pdblImg)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.createNamedComplexMatrixOfDouble(pvApiCtx, _pstName,
                                                            _iRows, _iCols,
                                                             _pdblReal,
                                                             _pdblImg);
            else
                return Scilab_cs_wrapper_unix.createNamedComplexMatrixOfDouble(pvApiCtx, _pstName,
                                                            _iRows, _iCols,
                                                             _pdblReal,
                                                             _pdblImg);
        }

        public unsafe static Scilab_cs_wrapper.api_Err readNamedMatrixOfString([In]IntPtr pvApiCtx, [In] String _pstName,
                                                          [Out]  Int32* _piRows, [Out]  Int32* _piCols,
                                                          [In, Out] int[] _piLength,
                                                          [In, Out] String[] _pstStrings)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.readNamedMatrixOfString(pvApiCtx, _pstName,
                                                          _piRows, _piCols,
                                                           _piLength,
                                                           _pstStrings);
            else
                return Scilab_cs_wrapper_unix.readNamedMatrixOfString(pvApiCtx, _pstName,
                                                           _piRows, _piCols,
                                                           _piLength,
                                                          _pstStrings);
        }

        public unsafe static Scilab_cs_wrapper.api_Err readNamedMatrixOfWideString([In]IntPtr pvApiCtx, [In] String _pstName,
                                                          [Out]  Int32* _piRows, [Out]  Int32* _piCols,
                                                          [In, Out] int[] _piLength,
                                                          [In, Out] String[] _pstStrings)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.readNamedMatrixOfWideString(pvApiCtx, _pstName,
                                                          _piRows, _piCols,
                                                           _piLength,
                                                           _pstStrings);
            else
                return Scilab_cs_wrapper_unix.readNamedMatrixOfWideString(pvApiCtx, _pstName,
                                                           _piRows, _piCols,
                                                           _piLength,
                                                           _pstStrings);
        }

        public unsafe static Scilab_cs_wrapper.api_Err readNamedMatrixOfDouble([In]IntPtr pvApiCtx, [In] String _pstName,
                                                          [Out] Int32* _piRows, [Out] Int32* _piCols,
                                                          [In, Out] Double[] _pdblReal)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.readNamedMatrixOfDouble(pvApiCtx, _pstName,
                                                          _piRows, _piCols,
                                                           _pdblReal);
            else
                return Scilab_cs_wrapper_unix.readNamedMatrixOfDouble(pvApiCtx, _pstName,
                                                           _piRows, _piCols,
                                                           _pdblReal);
        }

        public unsafe static Scilab_cs_wrapper.api_Err readNamedMatrixOfBoolean([In]IntPtr pvApiCtx, [In] String _pstName,
                                                          [Out] Int32* _piRows, [Out] Int32* _piCols,
                                                          [In, Out] int[] _piBool)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.readNamedMatrixOfBoolean(pvApiCtx, _pstName,
                                                          _piRows, _piCols,
                                                           _piBool);
            else
                return Scilab_cs_wrapper_unix.readNamedMatrixOfBoolean(pvApiCtx, _pstName,
                                                           _piRows, _piCols,
                                                          _piBool);
        }

        public unsafe static Scilab_cs_wrapper.api_Err readNamedMatrixOfInteger32([In]IntPtr pvApiCtx, [In] String _pstName,
                                                          [Out] Int32* _piRows, [Out] Int32* _piCols,
                                                          [In, Out] int[] _piData)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.readNamedMatrixOfInteger32(pvApiCtx, _pstName,
                                                           _piRows, _piCols,
                                                          _piData);
            else
                return Scilab_cs_wrapper_unix.readNamedMatrixOfInteger32(pvApiCtx, _pstName,
                                                           _piRows, _piCols,
                                                          _piData);
        }

        public unsafe static Scilab_cs_wrapper.api_Err readNamedComplexMatrixOfDouble([In]IntPtr pvApiCtx, [In] String _pstName,
                                                        [Out] Int32* _piRows, [Out] Int32* _piCols,
                                                        [In, Out] double[] _pdblReal,
                                                        [In, Out] double[] _pdblImg)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.readNamedComplexMatrixOfDouble(pvApiCtx, _pstName,
                                                        _piRows, _piCols,
                                                        _pdblReal,
                                                         _pdblImg);
            else
                return Scilab_cs_wrapper_unix.readNamedComplexMatrixOfDouble(pvApiCtx, _pstName,
                                                        _piRows, _piCols,
                                                        _pdblReal,
                                                         _pdblImg);
        }

        public unsafe static Scilab_cs_wrapper.api_Err getVarAddressFromName([In]IntPtr pvApiCtx, [In] String _pstName,
                                                               [Out] Int32** _piAddress)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.getVarAddressFromName(pvApiCtx, _pstName,
                                                               _piAddress);
            else
                return Scilab_cs_wrapper_unix.getVarAddressFromName(pvApiCtx, _pstName,
                                                               _piAddress);
        }

        public unsafe static Scilab_cs_wrapper.api_Err getNamedVarType([In]IntPtr pvApiCtx, [In] String _pstName, [Out]Int32* _piType)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.getNamedVarType(pvApiCtx, _pstName, _piType);
            else
                return Scilab_cs_wrapper_unix.getNamedVarType(pvApiCtx, _pstName, _piType);
        }

        public unsafe static Scilab_cs_wrapper.api_Err getVarType([In]IntPtr pvApiCtx, [In] Int32* _piAddress, [Out]Int32* _piType)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.getVarType(pvApiCtx, _piAddress, _piType);
            else
                return Scilab_cs_wrapper_unix.getVarType(pvApiCtx, _piAddress, _piType);
        }

        public unsafe static int sciHasFigures()
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.sciHasFigures();
            else
                return Scilab_cs_wrapper_unix.sciHasFigures();
        }

        public unsafe static int GetLastErrorCode()
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.GetLastErrorCode();
            else
                return Scilab_cs_wrapper_unix.GetLastErrorCode();
        }

        public unsafe static Scilab_cs_wrapper.api_Err getNamedVarDimension([In]IntPtr pvApiCtx, [In] String _pstName,
                                   [Out] Int32* _piRows, [Out] Int32* _piCols)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.getNamedVarDimension(pvApiCtx, _pstName, _piRows, _piCols);
            else
                return Scilab_cs_wrapper_unix.getNamedVarDimension(pvApiCtx, _pstName,
                                   _piRows, _piCols);
        }

        public unsafe static int isNamedVarComplex([In]IntPtr pvApiCtx, [In] String _pstName)
        {
            if (isWindows == true)
                return Scilab_cs_wrapper_win.isNamedVarComplex(pvApiCtx, _pstName);
            else
                return Scilab_cs_wrapper_unix.isNamedVarComplex(pvApiCtx, _pstName);
        }

    }
}
