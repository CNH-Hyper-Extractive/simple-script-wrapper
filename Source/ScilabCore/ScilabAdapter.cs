
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using OpenMI.Standard;
using Oatc.OpenMI.Sdk.Backbone;
using KansasState.Ssw.InterfaceCore;
using DotNetScilab;

namespace KansasState.Ssw.ScilabCore
{
    /// <summary>
    /// Note that this does not work with .NET 4.0 on x64 (apparently due to
    /// the combination of an issue in the x64 build of Scilab and additional
    /// memory checking added in .NET 4.0. So either use an x86 build of the
    /// Configuration Editor to force the use of the x86 version of Scilab or
    /// use a .NET 2.0 version of the Configuration Editor to allow the use
    /// of the x64 version of Scilab.
    /// </summary>
    public class ScilabAdapter : ILanguageAdapter
    {
        private readonly String _scriptPath;
        private Scilab _scilab;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// Checks if the given path exists, and if so, searches for a MATLAB
        /// installation.
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

        public ScilabAdapter(String scriptPath, String installationFolder) : base()
        {
            string binPath = null;

            // check the path specified in the config file (if there was one)
            binPath = CheckPathForInstallation(installationFolder);

            // check the default program files folder
            if (binPath == null)
            {
                binPath = CheckPathForInstallation(@"C:\Program Files\");
            }

            // check the 32 bit program files folder
            if (binPath == null)
            {
                binPath = CheckPathForInstallation(@"C:\Program Files (x86)\");
            }

            // make sure we found it
            if (binPath == null)
            {
                throw new Exception("Unable to find Scilab installation folder");
            }
            
            // make sure we use an absolute path
            _scriptPath = Path.GetFullPath(scriptPath);
        }

        private void GenerateArrayAccessors(StreamWriter writer, String functionName, IExchangeItem[] items, String op)
        {
            if (op == "Get")
                writer.WriteLine(@"function[values] = " + functionName + "(quantityName)");
            else
                writer.WriteLine(@"function[] = " + functionName + "(quantityName, values)");

            foreach (IExchangeItem item in items)
            {
                writer.WriteLine(@"  global " + GetIDArrayName(item) + ";");
                writer.WriteLine(@"  global " + GetValueArrayName(item) + ";");
            }

            if (op == "Get")
                writer.WriteLine(@"  values = [];");

            // step through each item in the list
            foreach (IExchangeItem item in items)
            {
                writer.WriteLine(@"  if(quantityName == '" + item.Quantity.ID + "')");

                //writer.WriteLine(@"    ids = " + GetIDArrayName(item) + ";");

                //writer.WriteLine(@"    for i=1:size(ids,'c')");
                //writer.WriteLine(@"      if(ids(i) == elementID)");

                if (op == "Get")
                    writer.WriteLine(@"        values = " + GetValueArrayName(item) + ";");
                else
                    writer.WriteLine(@"        " + GetValueArrayName(item) + " = values;");

                //writer.WriteLine(@"        break;");
                //writer.WriteLine(@"      end");
                //writer.WriteLine(@"    end");

                writer.WriteLine(@"  end");
            }

            writer.WriteLine(@"endfunction");
        }

        private void GenerateTimeAccessor(StreamWriter writer, String functionName, String variableName)
        {
            writer.WriteLine(@"function[value] = " + functionName + "()");
            writer.WriteLine(@"  global " + variableName + ";");
            writer.WriteLine(@"  value = " + variableName + ";");
            writer.WriteLine(@"endfunction");
        }

        private void GenerateFolderAccessor(StreamWriter writer)
        {
            writer.WriteLine(@"function[path] = sswGetScriptFolder()");
            writer.WriteLine(@"  path = '" + _scriptPath.Replace(@"\", @"\\") + "';");
            writer.WriteLine(@"endfunction");
        }

        private String GenerateFunctions(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs)
        {
            String autoFilename = Path.GetRandomFileName();
            StreamWriter writer = File.CreateText(autoFilename);

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

        public void Start(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs, DateTime startTime, double timeStepInSeconds)
        {
            // generate our dynamic functions
            String autoFilename = GenerateFunctions(inputs, outputs);

            // create the scripting environment
            //if (System.Environment.OSVersion.Platform != PlatformID.Unix)
            {
            //    if (_dllDirectory != null)
                
                    SetDllDirectory(@"C:\Program Files (x86)\scilab-5.4.1\bin");

                // on windows the dll directory must be null
               _scilab = new Scilab(null);
            }
            //else
            {
                // on unix we need to pass in the dll directory
                //mScilab = new Scilab(_dllDirectory);
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
                _scilab.SendScilabJob("global " + GetIDArrayName(item) + ";");
                _scilab.SendScilabJob("global " + GetValueArrayName(item) + ";");
            }

            foreach (IExchangeItem item in outputs)
            {
                // declare the globals for the outputs
                _scilab.SendScilabJob("global " + GetIDArrayName(item) + ";");
                _scilab.SendScilabJob("global " + GetValueArrayName(item) + ";");

                // declare the ids array for the item
                SetIDArray(item);

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


        private String GetIDArrayName(IExchangeItem item)
        {
            // figure out the prefix
            String prefix = "";
            if (item is InputExchangeItem)
                prefix = "In";
            if (item is OutputExchangeItem)
                prefix = "Out";

            return prefix + "IDs" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }


        private String GetValueArrayName(IExchangeItem item)
        {
            // figure out the prefix
            String prefix = "";
            if (item is InputExchangeItem)
                prefix = "In";
            if (item is OutputExchangeItem)
                prefix = "Out";

            return prefix + "Values" + item.Quantity.ID.Replace(" ", "").ToUpper();
        }


        private void SetIDArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            String[] ids = new String[item.ElementSet.ElementCount];
            for (int i = 0; i < item.ElementSet.ElementCount; i++)
                ids[i] = item.ElementSet.GetElementID(i);

            // set the arrays
            _scilab.createNamedMatrixOfString(GetIDArrayName(item), 1, ids.Length, ids);
        }


        private void InitializeValuesArray(IExchangeItem item)
        {
            // build the 1d array to hold the id's
            double[] values = new double[item.ElementSet.ElementCount];
            for (int i = 0; i < item.ElementSet.ElementCount; i++)
                values[i] = -999;

            // set the arrays
            _scilab.createNamedMatrixOfDouble(GetValueArrayName(item), 1, values.Length, values);
        }


        // this is called once for each input exchange item, and it takes
        // the values and element ids and stores them in two 1d arrays in the
        // scilab environment. the arrays have a name that is unique to
        // this input exchange item.
        public void SetValues(InputExchangeItem input, double[] values)
        {
            SetIDArray(input);
            _scilab.createNamedMatrixOfDouble(GetValueArrayName(input), 1, values.Length, values);
        }


        public double[] GetValues(OutputExchangeItem output)
        {
            return _scilab.readNamedMatrixOfDouble(GetValueArrayName(output));
        }


        public void PerformTimeStep(DateTime currentTime)
        {
            _scilab.SendScilabJob(GetSetTimeCommand("sswCurrentTime", currentTime));
            _scilab.SendScilabJob("sswPerformTimeStep();");
        }


        public void Stop()
        {
            _scilab.SendScilabJob("sswFinish();");
        }

        private String GetSetTimeCommand(String variableName, DateTime dt)
        {
            return variableName + " = [" + dt.Year + "," + dt.Month + "," + dt.Day + "," + dt.Hour + "," + dt.Minute + "," + dt.Second + "," + dt.Millisecond + "];";
        }

        private String ReadScriptFile(String scriptName)
        {
            StreamReader reader = new StreamReader(_scriptPath + scriptName);
            String s = reader.ReadToEnd();
            reader.Close();

            return s;
        }

    }
}
