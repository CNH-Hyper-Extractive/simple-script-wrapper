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
using System.Text;
using KansasState.Ssw.Extras;

namespace Sc.Smw
{
    public class LogFile
    {
        private readonly String _path;
        private bool _isEnabled = false;

        public LogFile(String path)
        {
            if (_isEnabled == false)
            {
                return;
            }

            _path = path;
            File.Delete(_path);
            var writer = File.CreateText(_path);
            writer.Close();
        }

        public void Append(String s)
        {
            if (_isEnabled == false)
            {
                return;
            }

            Console.WriteLine(s);
            var writer = new StreamWriter(_path, true);
            writer.Write(DateTimeToString(DateTime.Now) + ":" + s + "\r\n");
            writer.Close();
        }

        public static string DateTimeToString(DateTime dt)
        {
            var s = new StringBuilder();
            s.Append(String.Format("{0:0000}", dt.Year));
            s.Append("/");
            s.Append(String.Format("{0:00}", dt.Month));
            s.Append("/");
            s.Append(String.Format("{0:00}", dt.Day));
            s.Append("T");
            s.Append(String.Format("{0:00}", dt.Hour));
            s.Append(":");
            s.Append(String.Format("{0:00}", dt.Minute));
            s.Append(":");
            s.Append(String.Format("{0:00}", dt.Second));
            return s.ToString();
        }

        public static string ArrayToString(double[] array)
        {
            var s = new StringBuilder();
            for (var i = 0; i < array.Length; i++)
            {
                if (s.Length > 0)
                    s.Append(",");
                s.Append(String.Format("{0:0.00}", array[i]));
            }
            return s.ToString();
        }

        public void AppendNoEOL(String s)
        {
            if (_isEnabled == false)
            {
                return;
            }

            var writer = new StreamWriter(_path, true);
            writer.Write(s);
            writer.Close();
        }

        public void Append(string dataName, double[] data)
        {
            if (_isEnabled == false)
            {
                return;
            }

            var s = new StringBuilder();
            s.Append(dataName);
            if (Utils.IsEmpty(data) == true)
            {
                s.Append("(empty)");
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    s.Append(String.Format("[{0}]={1:0.00},", i, data[i]));
                }
            }
            Append(s.ToString());
        }
    }
}