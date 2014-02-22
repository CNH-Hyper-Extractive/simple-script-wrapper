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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using KansasState.Ssw.InterfaceCore;
using Oatc.OpenMI.Sdk.Backbone;
using OpenMI.Standard;

namespace KansasState.Ssw.ScilabCore
{
    /// <summary>
    ///     Note that this does not work with .NET 4.0 on x64 (apparently due to
    ///     the combination of an issue in the x64 build of Scilab and additional
    ///     memory checking added in .NET 4.0. So either use an x86 build of the
    ///     Configuration Editor to force the use of the x86 version of Scilab or
    ///     use a .NET 2.0 version of the Configuration Editor to allow the use
    ///     of the x64 version of Scilab. This applies to Windows, not checked on
    ///     Unix.
    /// </summary>
    public class ScilabAdapter : ILanguageAdapter
    {
        private static readonly Random Random = new Random((int)DateTime.Now.Ticks);
        private readonly string _binPath;
        private readonly string _prefix = RandomPrefix();
        private readonly string _scriptPath;
        private IScilab _scilab;

        public ScilabAdapter(String scriptPath, String installationFolder)
        {
            Console.WriteLine("Using prefix: " + _prefix);

            // check the path specified in the config file (if there was one)
            _binPath = CheckPathForInstallation(installationFolder);

            // check the default program files folder
            if (_binPath == null)
            {
                _binPath = CheckPathForInstallation(@"C:\Program Files\");
            }

            // check the 32 bit program files folder
            if (_binPath == null)
            {
                _binPath = CheckPathForInstallation(@"C:\Program Files (x86)\");
            }

            // check the beocat folder
            if (_binPath == null)
            {
                _binPath = CheckPathForInstallation(@"/opt/beocat/scilab");
            }

            // make sure we found it
            if (_binPath == null)
            {
                throw new Exception("Unable to find Scilab installation folder");
            }

            Console.WriteLine("Using Scilab at " + _binPath);

            // make sure we use an absolute path
            _scriptPath = Path.GetFullPath(scriptPath);
        }

        public void Start(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs, DateTime startTime, double timeStepInSeconds)
        {
            // generate our dynamic functions
            var autoFilename = GenerateFunctions(inputs, outputs);

            // create the scripting environment
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _scilab = new ScilabUnix(_binPath);
            }
            else
            {
                SetDllDirectory(_binPath);
                _scilab = new ScilabWin();
            }

            // declare globals to hold the time variables
            _scilab.SendScilabJob("global " + _prefix + "sswStartTime;");
            _scilab.SendScilabJob("global " + _prefix + "sswTimeStep;");
            _scilab.SendScilabJob("global " + _prefix + "sswCurrentTime;");

            // set the values of the time variables
            _scilab.SendScilabJob(GetSetTimeCommand(_prefix + "sswStartTime", startTime));
            _scilab.SendScilabJob(GetSetTimeCommand(_prefix + "sswCurrentTime", startTime));
            _scilab.SendScilabJob(_prefix + "sswTimeStep = " + timeStepInSeconds + ";");

            // declare the globals for the inputs
            foreach (IExchangeItem item in inputs)
            {
                if (GetIdArrayName(item).Length > 24 || GetValueArrayName(item).Length > 24)
                {
                    throw new Exception("Input item " + item.Quantity.ID + " name is too long (limit to 20)");
                }

                _scilab.SendScilabJob("global " + GetIdArrayName(item) + ";");
                _scilab.SendScilabJob("global " + GetValueArrayName(item) + ";");
            }

            foreach (IExchangeItem item in outputs)
            {
                if (GetIdArrayName(item).Length > 24 || GetValueArrayName(item).Length > 24)
                {
                    throw new Exception("Output item " + item.Quantity.ID + " name is too long (limit to 20)");
                }

                // declare the globals for the outputs
                _scilab.SendScilabJob("global " + GetIdArrayName(item) + ";");
                _scilab.SendScilabJob("global " + GetValueArrayName(item) + ";");

                // declare the ids array for the item
                SetIdArray(item);

                // initialize the output values
                InitializeValuesArray(item);
            }

            _scilab.SendScilabJob(@"exec('" + autoFilename + "')");
            File.Delete(autoFilename);

            //Console.ReadKey();

            SendScilabScriptWithPrefix("sswInitialize.sce");
            SendScilabScriptWithPrefix("sswPerformTimeStep.sce");
            SendScilabScriptWithPrefix("sswFinish.sce");

            _scilab.SendScilabJob(_prefix + "sswInitialize();");
        }

        public void SetValues(InputExchangeItem input, double[] values)
        {
            SetIdArray(input);
            _scilab.CreateNamedMatrixOfDouble(GetValueArrayName(input), 1, values.Length, values);
        }

        public double[] GetValues(OutputExchangeItem output)
        {
            return _scilab.ReadNamedMatrixOfDouble(GetValueArrayName(output));
        }

        public void PerformTimeStep(DateTime currentTime)
        {
            _scilab.SendScilabJob(GetSetTimeCommand(_prefix + "sswCurrentTime", currentTime));
            _scilab.SendScilabJob(_prefix + "sswPerformTimeStep();");
        }

        public void Stop()
        {
            _scilab.SendScilabJob(_prefix + "sswFinish();");

            // don't stop the scilab interpreter, since we don't know if there are
            // multiple components using it that sswFinish hasn't been called on yet
            //_scilab.StopScilab();
        }

        private void SendScilabScriptWithPrefix(string scriptFilename)
        {
            var s = ReadScriptFile(scriptFilename);
            s = s.Replace("sswInitialize", _prefix + "sswInitialize");
            s = s.Replace("sswPerformTimeStep", _prefix + "sswPerformTimeStep");
            s = s.Replace("sswFinish", _prefix + "sswFinish");
            s = s.Replace("sswGetStartTime", _prefix + "sswGetStartTime");
            s = s.Replace("sswGetTimeStep", _prefix + "sswGetTimeStep");
            s = s.Replace("sswGetCurrentTime", _prefix + "sswGetCurrentTime");
            s = s.Replace("sswGetScriptFolder", _prefix + "sswGetScriptFolder");

            s = s.Replace("sswGetInput", _prefix + "sswGetInput");
            s = s.Replace("sswSetInput", _prefix + "sswSetInput");
            s = s.Replace("sswGetOutput", _prefix + "sswGetOutput");
            s = s.Replace("sswSetOutput", _prefix + "sswSetOutput");


            var tempFilename = Path.GetRandomFileName();
            var writer = File.CreateText(tempFilename);
            writer.Write(s);
            writer.Close();
            _scilab.SendScilabJob(@"exec('" + _scriptPath + tempFilename + "')");
            File.Delete(tempFilename);
        }

        private static string RandomPrefix()
        {
            var builder = new StringBuilder();
            char ch;
            for (var i = 0; i < 2; i++)
            {
                ch = (char)Random.Next('A', 'Z' + 1);
                builder.Append(ch);
            }

            return builder.ToString();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        ///     Checks if the given path exists, and if so, searches for a MATLAB
        ///     installation.
        /// </summary>
        private static string CheckPathForInstallation(string path)
        {
            // validate the input argument
            if (path == null || Directory.Exists(path) == false)
            {
                return null;
            }

            // get the list of all folders to see what (if any) version of
            // scilab is installed (e.g. scilab-5.4.1)
            var folders = Directory.GetDirectories(path);

            string binPath = null;
            foreach (var nextFolder in folders)
            {
                // GetFileName actually will give us the directory name in this case
                if (Path.GetFileName(nextFolder).StartsWith("scilab-") == false)
                {
                    continue;
                }

                if (Directory.Exists(Path.Combine(nextFolder, @"bin")) == true)
                {
                    binPath = Path.Combine(nextFolder, @"bin");
                }
            }

            return binPath;
        }

        private void GenerateArrayAccessors(StreamWriter writer, String functionName, IExchangeItem[] items, String op)
        {
            if (op == "Get")
                writer.WriteLine(@"function[values] = " + functionName + "(quantityName)");
            else
                writer.WriteLine(@"function[] = " + functionName + "(quantityName, values)");

            foreach (var item in items)
            {
                writer.WriteLine(@"  global " + GetIdArrayName(item) + ";");
                writer.WriteLine(@"  global " + GetValueArrayName(item) + ";");
            }

            if (op == "Get")
                writer.WriteLine(@"  values = [];");

            // step through each item in the list
            foreach (var item in items)
            {
                writer.WriteLine(@"  if(quantityName == '" + item.Quantity.ID + "')");

                if (op == "Get")
                    writer.WriteLine(@"        values = " + GetValueArrayName(item) + ";");
                else
                    writer.WriteLine(@"        " + GetValueArrayName(item) + " = values;");

                writer.WriteLine(@"  end");
            }

            writer.WriteLine(@"endfunction");
        }

        private static void GenerateTimeAccessor(TextWriter writer, String functionName, String variableName)
        {
            writer.WriteLine(@"function[value] = " + functionName + "()");
            writer.WriteLine(@"  global " + variableName + ";");
            writer.WriteLine(@"  value = " + variableName + ";");
            writer.WriteLine(@"endfunction");
        }

        private void GenerateFolderAccessor(TextWriter writer)
        {
            writer.WriteLine(@"function[path] = " + _prefix + "sswGetScriptFolder()");
            writer.WriteLine(@"  path = '" + _scriptPath.Replace(@"\", @"\\") + "';");
            writer.WriteLine(@"endfunction");
        }

        private String GenerateFunctions(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs)
        {
            var autoFilename = Path.GetRandomFileName();
            var writer = File.CreateText(autoFilename);

            GenerateArrayAccessors(writer, _prefix + "sswGetInput", inputs.ToArray(), "Get");
            GenerateArrayAccessors(writer, _prefix + "sswSetInput", inputs.ToArray(), "Set");
            GenerateArrayAccessors(writer, _prefix + "sswGetOutput", outputs.ToArray(), "Get");
            GenerateArrayAccessors(writer, _prefix + "sswSetOutput", outputs.ToArray(), "Set");

            GenerateTimeAccessor(writer, _prefix + "sswGetStartTime", _prefix + "sswStartTime");
            GenerateTimeAccessor(writer, _prefix + "sswGetTimeStep", _prefix + "sswTimeStep");
            GenerateTimeAccessor(writer, _prefix + "sswGetCurrentTime", _prefix + "sswCurrentTime");

            GenerateFolderAccessor(writer);

            writer.Close();

            return autoFilename;
        }

        private String GetIdArrayName(IExchangeItem item)
        {
            // figure out the prefix
            var prefix = "";
            if (item is InputExchangeItem)
                prefix = "IN";
            if (item is OutputExchangeItem)
                prefix = "OT";

            return _prefix + prefix + "ID" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }

        private String GetValueArrayName(IExchangeItem item)
        {
            // figure out the prefix
            var prefix = "";
            if (item is InputExchangeItem)
                prefix = "IN";
            if (item is OutputExchangeItem)
                prefix = "OT";

            return _prefix + prefix + "VL" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }

        private void SetIdArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            var ids = new String[item.ElementSet.ElementCount];
            for (var i = 0; i < item.ElementSet.ElementCount; i++)
            {
                ids[i] = item.ElementSet.GetElementID(i);
            }

            var s = new StringBuilder();
            s.Append(GetIdArrayName(item));
            s.Append("=[");
            for (var i = 0; i < ids.Length; i++)
            {
                if (i > 0)
                {
                    s.Append(",");
                }
                s.Append("'" + ids[i] + "'");
            }
            s.Append("]");
            _scilab.SendScilabJob(s.ToString());
        }

        private void InitializeValuesArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            var values = new double[item.ElementSet.ElementCount];
            for (var i = 0; i < item.ElementSet.ElementCount; i++)
                values[i] = -999;

            Console.WriteLine("Initializing Values Array: " + GetValueArrayName(item));

            // set the arrays
            _scilab.CreateNamedMatrixOfDouble(GetValueArrayName(item), 1, values.Length, values);

            var v = _scilab.ReadNamedMatrixOfDouble(GetValueArrayName(item));
            if (v == null)
            {
                throw new Exception("Initialization failed for " + item.Quantity.ID);
            }

            Console.WriteLine(v);
        }

        // this is called once for each input exchange item, and it takes
        // the values and element ids and stores them in two 1d arrays in the
        // scilab environment. the arrays have a name that is unique to
        // this input exchange item.

        private static String GetSetTimeCommand(String variableName, DateTime dt)
        {
            return variableName + " = [" + dt.Year + "," + dt.Month + "," + dt.Day + "," + dt.Hour + "," + dt.Minute + "," + dt.Second + "," + dt.Millisecond + "];";
        }

        private String ReadScriptFile(String scriptName)
        {
            var reader = new StreamReader(_scriptPath + scriptName);
            var s = reader.ReadToEnd();
            reader.Close();

            return s;
        }
    }
}