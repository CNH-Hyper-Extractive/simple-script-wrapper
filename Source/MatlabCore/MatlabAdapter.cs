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
using System.Reflection;
using System.Runtime.InteropServices;
using KansasState.Ssw.Extras;
using KansasState.Ssw.InterfaceCore;
using Oatc.OpenMI.Sdk.Backbone;
using OpenMI.Standard;

namespace KansasState.Ssw.MatlabCore
{
    public class MatlabAdapter : ILanguageAdapter
    {
        private readonly String mScriptsPath;
        private readonly String mTempScriptsPath;
        private DynamicMLAppClass matlab;

        public MatlabAdapter(String scriptPath, String installationFolder)
        {
            string typeLibPath = null;

            // check the path specified in the config file (if there was one)
            typeLibPath = CheckPathForInstallation(installationFolder);

            // check the default program files folder
            if (typeLibPath == null)
            {
                typeLibPath = CheckPathForInstallation(@"C:\Program Files\MATLAB");
            }

            // check the 32 bit program files folder
            if (typeLibPath == null)
            {
                typeLibPath = CheckPathForInstallation(@"C:\Program Files (x86)\MATLAB");
            }

            // make sure we found it
            if (typeLibPath == null)
            {
                throw new Exception("Unable to find MATLAB installation folder");
            }

            // this is necessary so that the generated assembly ends up in the
            // same folder as this assembly
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // generate an assembly for the COM type library
            MyTlbImpApp.generateAssembly(typeLibPath);

            // make sure we use an absolute path
            scriptPath = Path.GetFullPath(scriptPath);

            mScriptsPath = scriptPath;

            // create a temporary folder for our generated scripts
            mTempScriptsPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(mTempScriptsPath);

            // make sure the paths end with a path separator
            mScriptsPath = Utils.AddTrailingSeparatorIfNecessary(mScriptsPath);
            mTempScriptsPath = Utils.AddTrailingSeparatorIfNecessary(mTempScriptsPath);
        }

        public void Start(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs, DateTime startTime, double timeStepInSeconds)
        {
            // generate our dynamic functions
            GenerateFunctions(inputs, outputs);

            matlab = new DynamicMLAppClass();

            // clear the state of the Matlab environment
            matlab.Execute("clear all");

            // add our temporary scripts folder
            matlab.Execute(@"addpath('" + mTempScriptsPath.Replace(@"\", @"\\") + "')");

            // run our dynamic support functions (which effectively just
            // loads them into memory so that we can call them later)
            matlab.Execute(@"run sswGetInput.m");
            matlab.Execute(@"run sswSetInput.m");
            matlab.Execute(@"run sswGetOutput.m");
            matlab.Execute(@"run sswSetOutput.m");

            matlab.Execute(@"run sswGetStartTime.m");
            matlab.Execute(@"run sswGetTimeStep.m");
            matlab.Execute(@"run sswGetCurrentTime.m");

            matlab.Execute(@"run sswGetCurrentFolder.m");

            // declare globals to hold the time variables
            matlab.Execute("global sswStartTime;");
            matlab.Execute("global sswTimeStep;");
            matlab.Execute("global sswCurrentTime;");

            // set the values of the time variables
            matlab.Execute(GetSetTimeCommand("sswStartTime", startTime));
            matlab.Execute(GetSetTimeCommand("sswCurrentTime", startTime));
            matlab.Execute("sswTimeStep = " + timeStepInSeconds + ";");

            // declare the globals for the inputs
            foreach (IExchangeItem item in inputs)
            {
                matlab.Execute("global " + GetIDArrayName(item) + ";");
                matlab.Execute("global " + GetValueArrayName(item) + ";");
            }

            foreach (IExchangeItem item in outputs)
            {
                // declare the globals for the outputs
                matlab.Execute("global " + GetIDArrayName(item) + ";");
                matlab.Execute("global " + GetValueArrayName(item) + ";");

                // declare the ids array for the item
                SetIDArray(item);

                // initialize the output values
                InitializeValuesArray(item);
            }

            // call the initialize script
            matlab.Execute(@"run '" + mScriptsPath + "sswInitialize.m'");
        }

        public void SetValues(InputExchangeItem input, double[] values)
        {
            SetIDArray(input);
            Array a = values;
            Array i = new double[a.Length];
            matlab.PutFullMatrix(GetValueArrayName(input), "base", a, i);
            matlab.Execute(GetValueArrayName(input) + " = " + GetValueArrayName(input) + "'");
        }

        public double[] GetValues(OutputExchangeItem output)
        {
            Array r = new double[output.ElementSet.ElementCount];
            Array i = new double[output.ElementSet.ElementCount];
            matlab.GetFullMatrix(GetValueArrayName(output), "base", ref r, ref i);
            return (double[])r;
        }

        public void PerformTimeStep(DateTime currentTime)
        {
            matlab.Execute(GetSetTimeCommand("sswCurrentTime", currentTime));
            matlab.Execute(@"run '" + mScriptsPath + "sswPerformTimeStep.m'");
        }

        public void Stop()
        {
            matlab.Execute(@"run '" + mScriptsPath + "sswFinish.m'");

            matlab.Quit();

            try
            {
                File.Delete(mTempScriptsPath + "sswSetInput.m");
                File.Delete(mTempScriptsPath + "sswSetOutput.m");
                File.Delete(mTempScriptsPath + "sswGetInput.m");
                File.Delete(mTempScriptsPath + "sswGetOutput.m");

                File.Delete(mTempScriptsPath + "sswGetStartTime.m");
                File.Delete(mTempScriptsPath + "sswGetCurrentTime.m");
                File.Delete(mTempScriptsPath + "sswGetTimeStep.m");

                File.Delete(mTempScriptsPath + "sswGetScriptFolder.m");

                Directory.Delete(mTempScriptsPath);
            }
            catch (Exception)
            {
                // these are temp files that will eventually be deleted anyway
            }
        }

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

            // get the list of matlab version folders (e.g. R2012a) in the
            // given installation folder
            var folders = Directory.GetDirectories(path);

            string typeLibPath = null;
            foreach (var nextFolder in folders)
            {
                if (Directory.Exists(Path.Combine(nextFolder, @"bin\win32")) == true)
                {
                    if (File.Exists(Path.Combine(nextFolder, @"bin\win32\mlapp.tlb")) == true)
                    {
                        typeLibPath = Path.Combine(nextFolder, @"bin\win32\mlapp.tlb");
                    }
                }
                else if (Directory.Exists(Path.Combine(nextFolder, @"bin\win64")) == true)
                {
                    if (File.Exists(Path.Combine(nextFolder, @"bin\win64\mlapp.tlb")) == true)
                    {
                        typeLibPath = Path.Combine(nextFolder, @"bin\win64\mlapp.tlb");
                    }
                }
            }

            return typeLibPath;
        }

        private void GenerateArrayAccessorsByElement(StreamWriter writer, String functionName, IExchangeItem[] items, String op)
        {
            if (op == "Get")
                writer.WriteLine(@"function[value] = " + functionName + "(quantityName, elementID)");
            else
                writer.WriteLine(@"function[] = " + functionName + "(quantityName, elementID, value)");

            foreach (var item in items)
            {
                writer.WriteLine(@"  global " + GetIDArrayName(item) + ";");
                writer.WriteLine(@"  global " + GetValueArrayName(item) + ";");
            }

            if (op == "Get")
                writer.WriteLine(@"  value = -999;");

            // step through each item in the list
            foreach (var item in items)
            {
                writer.WriteLine(@"  if(strcmp(quantityName, '" + item.Quantity.ID + "') == 1)");

                writer.WriteLine(@"    ids = " + GetIDArrayName(item) + ";");

                writer.WriteLine(@"    for i=1:size(ids, 1)");
                writer.WriteLine(@"      if(strcmp(elementID, strtrim(char(ids(i,:)))) == 1)");

                if (op == "Get")
                    writer.WriteLine(@"        value = " + GetValueArrayName(item) + "(i);");
                else
                    writer.WriteLine(@"        " + GetValueArrayName(item) + "(i) = value;");

                writer.WriteLine(@"        break;");
                writer.WriteLine(@"      end");
                writer.WriteLine(@"    end");

                writer.WriteLine(@"  end");
            }

            writer.WriteLine(@"return");
        }

        private void GenerateArrayAccessors(StreamWriter writer, String functionName, IExchangeItem[] items, String op)
        {
            if (op == "Get")
                writer.WriteLine(@"function[values] = " + functionName + "(quantityName)");
            else
                writer.WriteLine(@"function[] = " + functionName + "(quantityName, values)");

            foreach (var item in items)
            {
                writer.WriteLine(@"  global " + GetIDArrayName(item) + ";");
                writer.WriteLine(@"  global " + GetValueArrayName(item) + ";");
            }

            if (op == "Get")
                writer.WriteLine(@"  values = [];");

            // step through each item in the list
            foreach (var item in items)
            {
                writer.WriteLine(@"  if(strcmp(quantityName, '" + item.Quantity.ID + "') == 1)");

                //writer.WriteLine(@"    ids = " + GetIDArrayName(item) + ";");

                //writer.WriteLine(@"    for i=1:size(ids, 1)");
                //writer.WriteLine(@"      if(strcmp(elementID, strtrim(ids(i,:))) == 1)");

                if (op == "Get")
                    writer.WriteLine(@"        values = " + GetValueArrayName(item));
                else
                    writer.WriteLine(@"        " + GetValueArrayName(item) + " = values;");

                //writer.WriteLine(@"        break;");
                //writer.WriteLine(@"      end");
                //writer.WriteLine(@"    end");

                writer.WriteLine(@"  end");
            }

            writer.WriteLine(@"return");
        }

        private void GenerateTimeAccessor(StreamWriter writer, String functionName, String variableName)
        {
            writer.WriteLine(@"function[value] = " + functionName + "()");
            writer.WriteLine(@"  global " + variableName + ";");
            writer.WriteLine(@"  value = " + variableName + ";");
            writer.WriteLine(@"return");
        }

        private void GenerateFolderAccessor(StreamWriter writer)
        {
            writer.WriteLine(@"function[path] = sswGetScriptFolder()");
            writer.WriteLine(@"  path = '" + mScriptsPath.Replace(@"\", @"\\") + "';");
            writer.WriteLine(@"return");
        }

        private void GenerateFunctions(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs)
        {
            var writer = File.CreateText(mTempScriptsPath + "sswGetInput.m");
            GenerateArrayAccessors(writer, "sswGetInput", inputs.ToArray(), "Get");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswSetInput.m");
            GenerateArrayAccessors(writer, "sswSetInput", inputs.ToArray(), "Set");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswGetOutput.m");
            GenerateArrayAccessors(writer, "sswGetOutput", outputs.ToArray(), "Get");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswSetOutput.m");
            GenerateArrayAccessors(writer, "sswSetOutput", outputs.ToArray(), "Set");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswGetStartTime.m");
            GenerateTimeAccessor(writer, "sswGetStartTime", "sswStartTime");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswGetTimeStep.m");
            GenerateTimeAccessor(writer, "sswGetTimeStep", "sswTimeStep");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswGetCurrentTime.m");
            GenerateTimeAccessor(writer, "sswGetCurrentTime", "sswCurrentTime");
            writer.Close();

            writer = File.CreateText(mTempScriptsPath + "sswGetScriptFolder.m");
            GenerateFolderAccessor(writer);
            writer.Close();
        }

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode)]
        private static extern void LoadTypeLibEx(string strTypeLibName, REGKIND regKind, out UCOMITypeLib TypeLib);

        private String GetIDArrayName(IExchangeItem item)
        {
            // figure out the prefix
            var prefix = "";
            if (item is InputExchangeItem)
                prefix = "In";
            if (item is OutputExchangeItem)
                prefix = "Out";

            return prefix + "IDs" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }


        private String GetValueArrayName(IExchangeItem item)
        {
            // figure out the prefix
            var prefix = "";
            if (item is InputExchangeItem)
                prefix = "In";
            if (item is OutputExchangeItem)
                prefix = "Out";

            return prefix + "Values" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }


        /// <summary>
        ///     Creates the array of element IDs in the Matlab environment. Note
        ///     that it pads the IDs to be uniform length since string arrays
        ///     are actually 2d character arrays so all the IDs must be of equal
        ///     length. Note that the IDs have to be trimmed anytime they're
        ///     used.
        /// </summary>
        /// <param name="item"></param>
        private void SetIDArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            Array ids = new double[item.ElementSet.ElementCount, 20];
            for (var i = 0; i < item.ElementSet.ElementCount; i++)
            {
                var c = item.ElementSet.GetElementID(i).PadLeft(8, ' ').ToCharArray();

                for (var j = 0; j < c.Length; j++)

                    ids.SetValue(c[j], i, j);
            }

            // set the array
            matlab.PutFullMatrix(GetIDArrayName(item), "base", ids, new double[item.ElementSet.ElementCount, 20]);
        }


        private void InitializeValuesArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            Array values = new double[item.ElementSet.ElementCount];
            Array valuesi = new double[item.ElementSet.ElementCount];
            for (var i = 0; i < item.ElementSet.ElementCount; i++)
            {
                values.SetValue(-999, i);
            }

            // set the arrays
            matlab.PutFullMatrix(GetValueArrayName(item), "base", values, valuesi);
            matlab.Execute(GetValueArrayName(item) + " = " + GetValueArrayName(item) + "'");
        }


        private String GetSetTimeCommand(String variableName, DateTime dt)
        {
            return variableName + " = [" + dt.Year + " " + dt.Month + " " + dt.Day + " " + dt.Hour + " " + dt.Minute + " " + dt.Second + "]";
        }

        internal enum REGKIND
        {
            REGKIND_DEFAULT = 0,
            REGKIND_REGISTER = 1,
            REGKIND_NONE = 2
        }
    }
}