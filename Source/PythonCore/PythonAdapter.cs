﻿//  -----------------------------------------------------------------------
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IronPython.Hosting;
using IronPython.Runtime;
using KansasState.Ssw.InterfaceCore;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using Oatc.OpenMI.Sdk.Backbone;

namespace KansasState.Ssw.PythonCore
{
    public class PythonAdapter : ILanguageAdapter
    {
        private readonly String _dllDirectory;
        private readonly String mScriptPath;
        private Func<String, IList> GetValuesFunc;
        private Action PerformTimestepFunc;
        private Func<String, IList, int> SetValuesFunc;
        private object mClassInstance;

        private ScriptEngine pyEngine;

        public PythonAdapter(String scriptPath, String dllDirectory)
        {
            // if the dll directory was not specified, then look for Scilab in the
            // usual place
            if (dllDirectory == null)
            {
                if (Directory.Exists(@"C:\Python26") == true)
                    dllDirectory = @"C:\Python26";
                else
                    throw new Exception("Failed to locate Python install folder");
            }

            // make sure we use an absolute path
            scriptPath = Path.GetFullPath(scriptPath);

            mScriptPath = scriptPath;
            _dllDirectory = dllDirectory;
        }

        public void Start(List<InputExchangeItem> inputs, List<OutputExchangeItem> outputs, DateTime startTime, double timeStepInSeconds)
        {
            pyEngine = Python.CreateEngine();
            var pyScope = pyEngine.CreateScope();

            var headerCode = "import sys\r\nsys.path.append(r\"" + _dllDirectory + "\\Lib\")\r\n";
            var code = GetFromResources("KansasState.Ssw.PythonCore.Core.py");
            var modelCode = ReadScriptFile("Model.py");
            //String initializeCode = ReadScriptFile("sswInitialize.py");
            //String performTimeStepCode = ReadScriptFile("sswPerformTimeStep.py");
            //String finishCode = ReadScriptFile("sswFinish.py");
            var completeCode = (headerCode + code + modelCode);

            var source = pyEngine.CreateScriptSourceFromString(completeCode);
            var compiled = source.Compile();
            compiled.Execute(pyScope);

            mClassInstance = pyEngine.Operations.Invoke(pyScope.GetVariable("ModelClass"));

            // put all the output exchange items into a dictionary that we'll
            // give to the runtime so the script knows what it must be able to
            // provide
            var exchangeOut = new PythonDictionary();
            foreach (var item in outputs)
            {
                // make a dictionary of element ids in this exchange item
                //IronPython.Runtime.PythonDictionary elements = new IronPython.Runtime.PythonDictionary();

                // add each element id to the list
                //for (int i = 0; i < item.ElementSet.ElementCount; i++)
                //    elements[item.ElementSet.GetElementID(i)] = -999.99;

                var elements = new double[item.ElementSet.ElementCount];
                for (var i = 0; i < item.ElementSet.ElementCount; i++)
                    elements[i] = -999.99;

                // add each element id to the list
                //for (int i = 0; i < item.ElementSet.ElementCount; i++)
                //    elements[item.ElementSet.GetElementID(i)] = -999.99;

                // store the list in the dictionary keyed by the quantity name
                exchangeOut[item.Quantity.ID] = elements;
            }
            pyEngine.Operations.SetMember(mClassInstance, "ExchangeOut", exchangeOut);

            // set the values of the time variables
            pyEngine.Operations.SetMember(mClassInstance, "sswStartTime", startTime, false);
            pyEngine.Operations.SetMember(mClassInstance, "sswCurrentTime", startTime, false);
            pyEngine.Operations.SetMember(mClassInstance, "sswTimeStep", timeStepInSeconds, false);

            // set the script path
            pyEngine.Operations.SetMember(mClassInstance, "sswScriptPath", mScriptPath, false);

            // put all the input exchange items into a dictionary that we'll
            // give to the runtime so the script knows what it will receive
            var exchangeIn = new PythonDictionary();
            foreach (var item in inputs)
            {
                // make a dictionary of element ids in this exchange item
                //IronPython.Runtime.PythonDictionary elements = new IronPython.Runtime.PythonDictionary();

                // add each element id to the list
                //for (int i = 0; i < item.ElementSet.ElementCount; i++)
                //    elements[item.ElementSet.GetElementID(i)] = -999.99;

                var elements = new double[item.ElementSet.ElementCount];
                for (var i = 0; i < item.ElementSet.ElementCount; i++)
                    elements[i] = -999.99;

                // store the list in the dictionary keyed by the quantity name
                exchangeIn[item.Quantity.ID] = elements;
            }
            pyEngine.Operations.SetMember(mClassInstance, "ExchangeIn", exchangeIn);

            // create callable functions
            SetValuesFunc = pyEngine.Operations.GetMember<Func<String, IList, int>>(mClassInstance, "putdata");
            GetValuesFunc = pyEngine.Operations.GetMember<Func<String, IList>>(mClassInstance, "getdata");
            PerformTimestepFunc = pyEngine.Operations.GetMember<Action>(mClassInstance, "sswPerformTimeStep");

            // call the initialize function
            pyEngine.Operations.InvokeMember(mClassInstance, "sswInitialize", new object[0]);
        }


        public void SetValues(InputExchangeItem input, double[] values)
        {
            //IronPython.Runtime.PythonDictionary dict = new IronPython.Runtime.PythonDictionary();

            //for (int i = 0; i < values.Length; i++)
            //    dict.Add(input.ElementSet.GetElementID(i), values[i]);

            SetValuesFunc(input.Quantity.ID, values);
        }


        public double[] GetValues(OutputExchangeItem output)
        {
            //IronPython.Runtime.PythonDictionary dict = GetValuesFunc(output.Quantity.ID);

            /*double[] values = new double[dict.Count];
            int v = 0;
            foreach (double value in dict.Values)
            {
                values[v] = value;
                v++;
            }*/

            //return values;
            var list = GetValuesFunc(output.Quantity.ID);
            var values = new double[list.Count];
            for (var i = 0; i < values.Length; i++)
                values[i] = (double)list[i];
            return values;
        }


        public void PerformTimeStep(DateTime currentTime)
        {
            // this assigns a .NET DateTime instance to the currentTime variable inside Python
            pyEngine.Operations.SetMember(mClassInstance, "sswCurrentTime", currentTime, false);
            PerformTimestepFunc();
        }


        public void Stop()
        {
            pyEngine.Operations.InvokeMember(mClassInstance, "sswFinish", new object[0]);
        }

        internal string GetFromResources(string resourceName)
        {
            var assem = GetType().Assembly;
            using (var stream = assem.GetManifestResourceStream(resourceName))
            {
                try
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error retrieving from Resources. Tried '"
                                        + resourceName + "'\r\n" + e);
                }
            }
        }


        private String ReadScriptFile(String scriptName)
        {
            string script = null;
            var reader = new StreamReader(mScriptPath + scriptName);

            // skip to the class definition
            while (true)
            {
                // get the next line
                var s = reader.ReadLine();

                // if we got to the end without finding a class, then return null
                if (s == null)
                    return null;

                // if this is the class definition we're looking for, stop skipping lines
                if (s.Contains("class ModelClass(ModelBaseClass):") == true)
                {
                    script = s + "\r\n";
                    break;
                }
            }

            script += reader.ReadToEnd();
            reader.Close();

            return script;
        }
    }
}