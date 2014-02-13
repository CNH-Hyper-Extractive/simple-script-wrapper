//
// Copyright (c) 2012 Tom Bulatewicz, Kansas State University
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using OpenMI.Standard;
using Oatc.OpenMI.Sdk.Backbone;
using KansasState.Ssw.InterfaceCore;
using KansasState.Ssw.MatlabCore;
using KansasState.Ssw.ScilabCore;
using KansasState.Ssw.PythonCore;
using KansasState.Ssw.FileElementSet;

namespace KansasState.Ssw.AdapterApp
{
    public partial class FormHome : Form
    {
        public FormHome()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Runs a test of basic functionality on a language adapter.
        /// </summary>
        /// <param name="adapter"></param>
        private void RunTest(ILanguageAdapter adapter)
        {
            // create holders for our exchange items
            List<InputExchangeItem> _inputs = new List<InputExchangeItem>();
            List<OutputExchangeItem> _outputs = new List<OutputExchangeItem>();

            // read the element sets from the xml file
            List<XmlElementSet> elementSets = XmlElementSet.Read(@"..\..\Data\ElementSets.xml");

            // create an input exchange item
            InputExchangeItem inItem = new InputExchangeItem();
            inItem.Quantity = new Quantity("initem");
            inItem.ElementSet = elementSets[0];
            _inputs.Add(inItem);

            // create some fake input data
            double[] values = new double[inItem.ElementSet.ElementCount];
            for (int i = 0; i < values.Length; i++)
                values[i] = 1.0;

            // create an output exchange item
            OutputExchangeItem outItem = new OutputExchangeItem();
            outItem.Quantity = new Quantity("outitem");
            outItem.ElementSet = elementSets[0];
            _outputs.Add(outItem);

            // fake some times
            DateTime startTime = new DateTime(1982, 01, 01, 00, 00, 00);
            double timeStep = 3600;
            DateTime currentTime = startTime;

            // call initialize
            adapter.Start(_inputs, _outputs, startTime, timeStep);

            // set some values and perform a time step
            adapter.SetValues(inItem, values);
            adapter.PerformTimeStep(currentTime);
            currentTime = currentTime.AddSeconds(timeStep);

            // get some values and change them
            values = adapter.GetValues(outItem);
            for (int i = 0; i < values.Length; i++)
                values[i] += 0.1;

            // set some values and perform a time step
            adapter.SetValues(inItem, values);
            adapter.PerformTimeStep(currentTime);
            currentTime = currentTime.AddSeconds(timeStep);

            // get some values and change them
            values = adapter.GetValues(outItem);
            for (int i = 0; i < values.Length; i++)
                values[i] += 0.1;

            // set some values and perform a time step
            adapter.SetValues(inItem, values);
            adapter.PerformTimeStep(currentTime);
            currentTime = currentTime.AddSeconds(timeStep);

            // call finish
            adapter.Stop();
        }

        private void buttonTestMatlabClick(object sender, EventArgs e)
        {
            // create an adapter to matlab
            MatlabAdapter adapter = new MatlabAdapter(@"..\..\Data\Matlab Scripts\", null);

            // run the test
            RunTest(adapter);
        }

        private void buttonTestPythonClick(object sender, EventArgs e)
        {
            // create an adapter to python
            PythonAdapter adapter = new PythonAdapter(@"..\..\Data\Python Scripts\", null);

            // run the test
            RunTest(adapter);
        }

        private void buttonTestScilabClick(object sender, EventArgs e)
        {
            // create an adapter to scilab
            ScilabAdapter adapter = new ScilabAdapter(@"..\..\Data\Scilab Scripts\", null);

            // run the test
            RunTest(adapter);
        }
    }
}
