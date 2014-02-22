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
    ///     This class is based on the code sample included with Scilab.
    /// </summary>
    public class ScilabWin : IScilab
    {
        public ScilabWin()
        {
            ScilabWinInterop.StartScilab(null, null, null);
        }

        public int SendScilabJob(string command)
        {
            return ScilabWinInterop.SendScilabJob(command);
        }

        public int CreateNamedMatrixOfDouble(string matrixName, int iRows, int iCols, double[] matrixDouble)
        {
            var ptrEmpty = new IntPtr();
            var sciErr = ScilabWinInterop.createNamedMatrixOfDouble(ptrEmpty, matrixName, iRows, iCols, matrixDouble);
            return sciErr.iErr;
        }

        public unsafe double[] ReadNamedMatrixOfDouble(string matrixName)
        {
            var iRows = 0;
            var iCols = 0;

            var ptrEmpty = new IntPtr();
            ScilabWinInterop.readNamedMatrixOfDouble(ptrEmpty, matrixName, &iRows, &iCols, null);

            if (iRows*iCols > 0)
            {
                var matrixDouble = new double[iRows*iCols];
                var sciErr = ScilabWinInterop.readNamedMatrixOfDouble(ptrEmpty, matrixName, &iRows, &iCols, matrixDouble);
                if (sciErr.iErr != 0) return null;
                return matrixDouble;
            }
            return null;
        }

        public void StopScilab()
        {
            ScilabWinInterop.TerminateScilab(null);
        }
    }
}