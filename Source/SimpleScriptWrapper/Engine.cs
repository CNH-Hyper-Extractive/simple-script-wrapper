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

using System.Collections;
using System.IO;
using KansasState.Ssw.Extras;
using KansasState.Ssw.InterfaceCore;
using KansasState.Ssw.MatlabCore;
using KansasState.Ssw.PythonCore;
using KansasState.Ssw.ScilabCore;
using Oatc.OpenMI.Sdk.Backbone;
using Oatc.OpenMI.Sdk.DevelopmentSupport;
using Sc.Smw;

namespace KansasState.Ssw.SimpleScriptWrapper
{
    public class Engine : Wrapper
    {
        private ILanguageAdapter _adapter;

        public Engine()
        {
            _adapter = null;
        }

        /// <summary>
        ///     This method is used to perform actions prior to model simulation
        /// </summary>
        public override void Initialize(Hashtable properties)
        {
            // get config file path defined in the omi file
            var configFile = (string)properties["ConfigFile"];

            // set various variables from the config file
            SetVariablesFromConfigFile(configFile);

            // assumes that there is only one component in this folder (could add GetModelID
            // to make it unique)
            _logFile = new LogFile(Path.Combine(Directory.GetCurrentDirectory(), "Log.txt"));

            for (var i = 0; i < GetInputExchangeItemCount(); i++)
            {
                var item = GetInputExchangeItem(i);
                _logFile.Append("Input Item: " + item.Quantity.ID + " has " + item.ElementSet.ElementCount + " elements");
            }

            // get the name of the folder where the scripts are located (relative path)
            string scriptPath;
            if (properties["ScriptPath"] != null)
            {
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), (string)properties["ScriptPath"]);
            }
            else
            {
                scriptPath = Directory.GetCurrentDirectory();
            }

            // make sure the script path ends with a slash
            scriptPath = Utils.AddTrailingSeparatorIfNecessary(scriptPath);
            _logFile.Append("ScriptPath: " + scriptPath);

            // get the name of the folder where the runtime dll's are located
            var installationFolder = (string)properties["InstallationPath"];
            _logFile.Append("InstallationPath: " + installationFolder);

            _logFile.Append("Creating adapter..");

            // instantiate the appropriate interpreter adapter
            switch (_adapterName.ToLower())
            {
                case "python":
                    _adapter = new PythonAdapter(scriptPath, installationFolder);
                    break;
                case "matlab":
                    _adapter = new MatlabAdapter(scriptPath, installationFolder);
                    break;
                case "scilab":
                    _adapter = new ScilabAdapter(scriptPath, installationFolder);
                    break;
            }

            // initialize a data structure to hold results
            SetValuesTableFields();

            // start the adapter
            var startTime = Utils.ITimeToDateTime(GetTimeHorizon().Start);
            _adapter.Start(Inputs, Outputs, startTime, GetTimeStepLength());

            // update the output exchange item values in this component to
            // reflect the current (start) time of the script.
            var outputCount = GetOutputExchangeItemCount();
            if (outputCount > 0)
            {
                // for each output
                for (var i = 0; i < outputCount; i++)
                {
                    // get the next item
                    var item = GetOutputExchangeItem(i);

                    // extract the values from the adapter
                    var data = _adapter.GetValues(item);

                    _logFile.Append("Adapter.GetValues [" + item.Quantity.ID + "]:", data, true);

                    // store the values
                    SetValues(item.Quantity.ID, item.ElementSet.ID, new ScalarSet(data));
                }
            }

            var currentDt = CalendarConverter.ModifiedJulian2Gregorian(((TimeStamp)GetCurrentTime()).ModifiedJulianDay);
            _logFile.Append("Time: " + currentDt);

            _logFile.Append("Initialization complete");
        }

        public override bool PerformTimeStep()
        {
            _logFile.Append("PerformTimeStep()");

            var currentDt = CalendarConverter.ModifiedJulian2Gregorian(((TimeStamp)GetCurrentTime()).ModifiedJulianDay);
            _logFile.Append("Time: " + currentDt);

            // get any input values from other components and pass them into
            // the interpreter adapter
            var inputCount = GetInputExchangeItemCount();
            if (inputCount > 0)
            {
                // for each input
                for (var i = 0; i < inputCount; i++)
                {
                    // get the next item
                    var item = GetInputExchangeItem(i);

                    // get the values that have already been retrieved from
                    // the other component
                    var values = (ScalarSet)GetValues(item.Quantity.ID, item.ElementSet.ID);
                    var data = values.data;

                    _logFile.Append("Adapter.SetValues [" + item.Quantity.ID + "]:", data, true);

                    // give the values to the interpreter adapter
                    _adapter.SetValues(item, data);
                }
            }

            // tell the interpreter adapter to perform a time step
            var currentTime = Utils.ITimeToDateTime(GetCurrentTime());
            _adapter.PerformTimeStep(currentTime);

            // extract the outputs from the interpreter adapter for each
            // output exchange item
            var outputCount = GetOutputExchangeItemCount();
            if (outputCount > 0)
            {
                // for each output
                for (var i = 0; i < outputCount; i++)
                {
                    // get the next item
                    var item = GetOutputExchangeItem(i);

                    // extract the values from the adapter
                    var data = _adapter.GetValues(item);

                    _logFile.Append("Adapter.GetValues [" + item.Quantity.ID + "]:", data, true);

                    // store the values
                    SetValues(item.Quantity.ID, item.ElementSet.ID, new ScalarSet(data));
                }
            }

            // advance the component clock
            AdvanceTime();

            currentDt = CalendarConverter.ModifiedJulian2Gregorian(((TimeStamp)GetCurrentTime()).ModifiedJulianDay);
            _logFile.Append("Time: " + currentDt);

            // IMPORTANT: the values in the data table must reflect the
            // quantities at the time that we just advanced to

            return true;
        }


        /// <summary>
        ///     This method is used to perform actions after model simulation has ended
        /// </summary>
        public override void Finish()
        {
            _logFile.Append("Finish()");

            _adapter.Stop();
        }
    }
}