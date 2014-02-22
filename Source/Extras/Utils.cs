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
using System.IO;
using Oatc.OpenMI.Sdk.Backbone;
using Oatc.OpenMI.Sdk.DevelopmentSupport;
using OpenMI.Standard;

namespace KansasState.Ssw.Extras
{
    public class Utils
    {
        public static char[] Delimiters
        {
            get
            {
                var d = new char[4];
                d = new char[3];
                d[0] = ' ';
                d[1] = '\t';
                d[2] = ',';
                return d;
            }
        }

        public static void WriteArrayToFile(string path, string dataName, string header, DateTime dateTime, double[] data, IElementSet elementSet)
        {
            // don't write out empty data sets
            if (IsEmpty(data))
                return;

            var dt = string.Format("{0:0000}{1:00}{2:00}", dateTime.Year, dateTime.Month, dateTime.Day);
            dataName = dataName.Replace(' ', '_');
            var filename = dataName + dt + ".txt";
            var writer = new StreamWriter(Path.Combine(path, filename), false);
            writer.WriteLine(header);
            for (var i = 0; i < data.Length; i++)
            {
                var wellID = elementSet.GetElementID(i);
                writer.Write(wellID.PadLeft(8, ' '));

                var value = String.Format("{0:0.0000}", data[i]);
                writer.Write(value.PadLeft(12, ' '));

                writer.WriteLine();
            }
            writer.Close();
        }

        // remove empty strings from the string array
        public static string[] Compact(string[] inStrings)
        {
            // first see how many strings there are in the array
            var count = 0;
            for (var i = 0; i < inStrings.Length; i++)
            {
                if (inStrings[i] != "")
                    count++;
            }

            var outStrings = new string[count];
            var c = 0;
            for (var i = 0; i < inStrings.Length; i++)
            {
                if (inStrings[i] != "")
                {
                    outStrings[c] = inStrings[i];
                    c++;
                }
            }

            return outStrings;
        }

        public static bool IsEmpty(double[] data)
        {
            var isEmpty = true;
            for (var i = 0; i < data.Length; i++)
            {
                if ((int)data[i] != -999)
                {
                    isEmpty = false;
                    break;
                }
            }
            return isEmpty;
        }

        public static DateTime ITimeToDateTime(ITime iTime)
        {
            if (iTime is TimeStamp)
            {
                var ts = (TimeStamp)iTime;
                var d = ts.ModifiedJulianDay;
                var dt = CalendarConverter.ModifiedJulian2Gregorian(d);
                return dt;
            }

            throw new Exception("Unable to convert ITime to DateTime");
        }

        public static String AddTrailingSeparatorIfNecessary(String path)
        {
            if (path.EndsWith("" + Path.DirectorySeparatorChar) == false)
                path += Path.DirectorySeparatorChar;
            return path;
        }
    }
}