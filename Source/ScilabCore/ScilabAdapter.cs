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
        private readonly string _binPath;
        private readonly string _scriptPath;
        private IScilab _scilab;

        public ScilabAdapter(String scriptPath, String installationFolder)
        {
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
            _scilab.SendScilabJob("global sswStartTime;");
            _scilab.SendScilabJob("global sswTimeStep;");
            _scilab.SendScilabJob("global sswCurrentTime;");

            // set the values of the time variables
            _scilab.SendScilabJob(GetSetTimeCommand("sswStartTime", startTime));
            _scilab.SendScilabJob(GetSetTimeCommand("sswCurrentTime", startTime));
            _scilab.SendScilabJob("sswTimeStep = " + timeStepInSeconds + ";");

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

            _scilab.SendScilabJob(@"exec('" + _scriptPath + "sswInitialize.sce')");
            _scilab.SendScilabJob(@"exec('" + _scriptPath + "sswPerformTimeStep.sce')");
            _scilab.SendScilabJob(@"exec('" + _scriptPath + "sswFinish.sce')");

            _scilab.SendScilabJob("sswInitialize();");
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
            _scilab.SendScilabJob(GetSetTimeCommand("sswCurrentTime", currentTime));
            _scilab.SendScilabJob("sswPerformTimeStep();");
        }

        public void Stop()
        {
            _scilab.SendScilabJob("sswFinish();");
            _scilab.StopScilab();
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
            writer.WriteLine(@"function[path] = sswGetScriptFolder()");
            writer.WriteLine(@"  path = '" + _scriptPath.Replace(@"\", @"\\") + "';");
            writer.WriteLine(@"endfunction");
        }

        private String GenerateFunctions(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs)
        {
            var autoFilename = Path.GetRandomFileName();
            var writer = File.CreateText(autoFilename);

            GenerateArrayAccessors(writer, "sswGetInput", inputs.ToArray(), "Get");
            GenerateArrayAccessors(writer, "sswSetInput", inputs.ToArray(), "Set");
            GenerateArrayAccessors(writer, "sswGetOutput", outputs.ToArray(), "Get");
            GenerateArrayAccessors(writer, "sswSetOutput", outputs.ToArray(), "Set");

            GenerateTimeAccessor(writer, "sswGetStartTime", "sswStartTime");
            GenerateTimeAccessor(writer, "sswGetTimeStep", "sswTimeStep");
            GenerateTimeAccessor(writer, "sswGetCurrentTime", "sswCurrentTime");

            GenerateFolderAccessor(writer);

            writer.Close();

            return autoFilename;
        }

        private static String GetIdArrayName(IExchangeItem item)
        {
            // figure out the prefix
            var prefix = "";
            if (item is InputExchangeItem)
                prefix = "IN";
            if (item is OutputExchangeItem)
                prefix = "OT";

            return prefix + "ID" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }

        private static String GetValueArrayName(IExchangeItem item)
        {
            // figure out the prefix
            var prefix = "";
            if (item is InputExchangeItem)
                prefix = "IN";
            if (item is OutputExchangeItem)
                prefix = "OT";

            return prefix + "VL" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }

        private void SetIdArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            var ids = new String[item.ElementSet.ElementCount];
            for (var i = 0; i < item.ElementSet.ElementCount; i++)
                ids[i] = item.ElementSet.GetElementID(i);

            // set the arrays
            _scilab.CreateNamedMatrixOfString(GetIdArrayName(item), 1, ids.Length, ids);
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
                throw new Exception("Initializationi failed for " + item.Quantity.ID);
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