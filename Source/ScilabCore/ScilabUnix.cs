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

namespace KansasState.Ssw.ScilabCore
{
    /// <summary>
    ///     This class is based on the code sample included with Scilab. The SCI and
    ///     LD_LIBRARY_PATH environment variables must be set, for example:
    ///     export SCI=/opt/beocat/scilab/scilab-5.4.0/share/scilab
    ///     export
    ///     LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/opt/beocat/scilab/scilab-5.4.0/lib/scilab:/opt/beocat/scilab/scilab-5.4.0/lib/thirdparty
    /// </summary>
    public class ScilabUnix : IScilab
    {
        public ScilabUnix(string sciPath)
        {
            ScilabUnixInterop.StartScilab(sciPath, null, null);
        }

        public int SendScilabJob(string command)
        {
            return ScilabUnixInterop.SendScilabJob(command);
        }

        public int CreateNamedMatrixOfDouble(string matrixName, int iRows, int iCols, double[] matrixDouble)
        {
            var ptrEmpty = new IntPtr();
            var sciErr = ScilabUnixInterop.createNamedMatrixOfDouble(ptrEmpty, matrixName, iRows, iCols, matrixDouble);
            return sciErr.iErr;
        }

        public int CreateNamedMatrixOfString(string matrixName, int iRows, int iCols, string[] matrixString)
        {
            Console.WriteLine("CreateNamedMatrixOfString"); // need console out for unix to work?
            var ptrEmpty = new IntPtr();
            var sciErr = ScilabUnixInterop.createNamedMatrixOfString(ptrEmpty, matrixName, iRows, iCols, matrixString);
            return sciErr.iErr;
        }

        public unsafe double[] ReadNamedMatrixOfDouble(string matrixName)
        {
            var iRows = 0;
            var iCols = 0;

            var ptrEmpty = new IntPtr();
            ScilabUnixInterop.readNamedMatrixOfDouble(ptrEmpty, matrixName, &iRows, &iCols, null);

            if (iRows*iCols > 0)
            {
                var matrixDouble = new double[iRows*iCols];
                var sciErr = ScilabUnixInterop.readNamedMatrixOfDouble(ptrEmpty, matrixName, &iRows, &iCols, matrixDouble);
                if (sciErr.iErr != 0) return null;
                return matrixDouble;
            }
            return null;
        }

        public void StopScilab()
        {
            ScilabUnixInterop.TerminateScilab(null);
        }
    }
}