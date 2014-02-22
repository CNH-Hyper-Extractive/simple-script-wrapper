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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Xml;
using Oatc.OpenMI.Sdk.Backbone;
using Oatc.OpenMI.Sdk.DevelopmentSupport;
using Oatc.OpenMI.Sdk.Wrapper;
using OpenMI.Standard;
using TimeSpan = Oatc.OpenMI.Sdk.Backbone.TimeSpan;
using ValueType = OpenMI.Standard.ValueType;

namespace Sc.Smw
{
    //
    // This is a derivative work of the Simple Model Wrapper library (MIT license)
    // available at http://code.google.com/p/smw/
    //
    public abstract class Wrapper : IEngine
    {
        private const int SECONDS_IN_DAY = 86400;

        private const int STEP_UNIT_SECOND = 1;
        private const int STEP_UNIT_DAY = 2;
        private const int STEP_UNIT_YEAR = 3;

        #region Global Objects

        private readonly Dictionary<string, ElementSet> _elementSets = new Dictionary<string, ElementSet>();
        private readonly Dictionary<string, Quantity> _quantities = new Dictionary<string, Quantity>();
        private readonly DataTable _values = new DataTable();
        protected string _adapterName;
        private string _componentDescription = "Interpreter Model Component";
        private string _componentID = "Interpreter_Model_Component";
        private double _currentTime; // in julian days
        private List<InputExchangeItem> _inputs = new List<InputExchangeItem>();
        protected LogFile _logFile;
        private string _modelDescription;
        private string _modelID;
        private Unit _omiUnits;
        private List<OutputExchangeItem> _outputs = new List<OutputExchangeItem>();
        private string _shapefilepath;
        private double _simulationEndTime;
        private double _simulationStartTime;
        private int _stepInputOffset;
        //private double _timeStepLength;     // in days
        //private double _inputTimeOffset;    // + or - days from current time at which to get the inputs
        private int _stepLength; // number of steps of the appropriate unit
        private int _stepUnit; // time step units
        private string _xmlFilePath;

        #endregion

        #region Abstract Methods

        /// <summary>
        ///     Used to Initialize the component.  Performs routines that must be completed prior to simulation start.
        /// </summary>
        /// <param name="properties">properties extracted from the components *.omi file</param>
        public abstract void Initialize(Hashtable properties);

        public abstract bool PerformTimeStep();
        public abstract void Finish();

        #endregion

        protected List<InputExchangeItem> Inputs
        {
            get { return _inputs; }
            set { _inputs = value; }
        }

        protected List<OutputExchangeItem> Outputs
        {
            get { return _outputs; }
            set { _outputs = value; }
        }

        public string GetComponentID()
        {
            return _componentID;
        }

        public string GetComponentDescription()
        {
            return _modelDescription;
        }

        public string GetModelID()
        {
            return _modelID;
        }

        public string GetModelDescription()
        {
            return _modelDescription;
        }

        public InputExchangeItem GetInputExchangeItem(int exchangeItemIndex)
        {
            return _inputs[exchangeItemIndex];
        }

        public OutputExchangeItem GetOutputExchangeItem(int exchangeItemIndex)
        {
            return _outputs[exchangeItemIndex];
        }

        public int GetInputExchangeItemCount()
        {
            if (_inputs == null) return 0;
            return _inputs.Count;
        }

        public int GetOutputExchangeItemCount()
        {
            if (_outputs == null) return 0;
            return _outputs.Count;
        }

        public ITimeSpan GetTimeHorizon()
        {
            return new TimeSpan(
                new TimeStamp(_simulationStartTime), new TimeStamp(_simulationEndTime));
        }

        public void Dispose()
        {
        }

        public ITime GetCurrentTime()
        {
            if (_currentTime == 0)
                _currentTime = _simulationStartTime;
            return new TimeStamp(_currentTime);
        }

        public ITimeStamp GetEarliestNeededTime()
        {
            return new TimeStamp(_simulationStartTime);
        }

        /// <summary>
        ///     Returns the time at which the the inputs from other components is needed.
        /// </summary>
        public ITime GetInputTime(string QuantityID, string ElementSetID)
        {
            //return new TimeStamp(_currentTime); // original one where providing models run their time steps after this one
            //return new TimeStamp(_currentTime + (_timeStep / 86400)); // alternate one where providing models run their time steps before this one

            // use the GetCurrentTime() method so that it gets initialized if necessary
            var ct = (TimeStamp)GetCurrentTime();

            // add the input time offset to the current time
            //double inputTime = ct.ModifiedJulianDay + _inputTimeOffset;

            var dt = CalendarConverter.ModifiedJulian2Gregorian(ct.ModifiedJulianDay);
            switch (_stepUnit)
            {
                case STEP_UNIT_SECOND:
                    dt = dt.AddSeconds(_stepInputOffset*_stepLength);
                    break;
                case STEP_UNIT_DAY:
                    dt = dt.AddDays(_stepInputOffset*_stepLength);
                    break;
                case STEP_UNIT_YEAR:
                    dt = dt.AddYears(_stepInputOffset*_stepLength);
                    break;
            }
            var inputTime = CalendarConverter.Gregorian2ModifiedJulian(dt);

            // log it
            _logFile.Append("InputTime:" + CalendarConverter.ModifiedJulian2Gregorian(inputTime) + " (current:" + CalendarConverter.ModifiedJulian2Gregorian(_currentTime) + ")");

            // return the input time
            return new TimeStamp(inputTime);
        }

        public double GetMissingValueDefinition()
        {
            return -999;
        }

        /// <summary>
        ///     This method is used to extract values from an upstream component.
        /// </summary>
        /// <param name="QuantityID">The input Quantity ID</param>
        /// <param name="ElementSetID">The input Element Set ID</param>
        /// <returns>the values saved under the matching QuantityID and ElementSetID, from an upstream component</returns>
        public IValueSet GetValues(string QuantityID, string ElementSetID)
        {
            var rows = _values.Select("QuantityID = '" + QuantityID + "' AND ElementSetID = '" + ElementSetID + "'");
            if (rows.Length == 1)
                return (IValueSet)rows[0].ItemArray[2];
            var es = _elementSets[ElementSetID];
            var ss = new ScalarSet();
            ss.data = new double[es.ElementCount];
            return ss;
        }

        public void SetValues(string QuantityID, string ElementSetID, IValueSet values)
        {
            //_logFile.Append("SetValues: " + QuantityID + " (" + values.Count + ") table: " + _values.Rows.Count);

            // TODO: change this to store all values with a time stamp and change
            // getvlaues to accept a parameter to get an aggregated set of values

            // -- check to see if QuantityID and ElementSetID already exits in Values.  If so, delete that row before adding.
            var rows = _values.Select("QuantityID = '" + QuantityID + "' AND ElementSetID = '" + ElementSetID + "'");

            if (rows.Length == 1)
            {
                _values.Rows.Remove(rows[0]);
            }
            _values.BeginLoadData();
            var dr = _values.LoadDataRow(new object[] {QuantityID, ElementSetID, values}, true);
            _values.EndLoadData();
        }

        #region Auxilary Methods

        /// <summary>
        ///     This method will advance the components in time, by a single timestep.
        /// </summary>
        /// <remarks>
        ///     This should be called at the end of Perform Time Step.
        /// </remarks>
        public void AdvanceTime()
        {
            //TimeStamp ct = (TimeStamp)GetCurrentTime();
            //_currentTime = ct.ModifiedJulianDay + _timeStep / 86400;

            // use the GetCurrentTime() method so that it gets initialized if necessary
            var ct = (TimeStamp)GetCurrentTime();
            //_currentTime = ct.ModifiedJulianDay + _timeStepLength;

            var dt = CalendarConverter.ModifiedJulian2Gregorian(ct.ModifiedJulianDay);
            switch (_stepUnit)
            {
                case STEP_UNIT_SECOND:
                    dt = dt.AddSeconds(_stepLength);
                    break;
                case STEP_UNIT_DAY:
                    dt = dt.AddDays(_stepLength);
                    break;
                case STEP_UNIT_YEAR:
                    dt = dt.AddYears(_stepLength);
                    break;
            }
            _currentTime = CalendarConverter.Gregorian2ModifiedJulian(dt);
        }

        /// <summary>
        ///     Reads the Configuration file, and creates OpenMI exchange items
        /// </summary>
        /// <param name="configFile">path pointing to the components comfiguration (XML) file</param>
        public void SetVariablesFromConfigFile(string configFile)
        {
            //Read config file
            var doc = new XmlDocument();
            doc.Load(configFile);

            var root = doc.DocumentElement;

            var ID = root.SelectSingleNode("ModelInfo//ID");
            _modelID = ID.InnerText;

            var Desc = root.SelectSingleNode("ModelInfo//Description");
            if (Desc != null)
                _componentDescription = Desc.InnerText;

            var AdapterName = root.SelectSingleNode("ModelInfo//ScriptingLanguage");
            if (AdapterName != null)
                _adapterName = AdapterName.InnerText;

            var outputExchangeItems = root.SelectNodes("//OutputExchangeItem");
            foreach (XmlNode outputExchangeItem in outputExchangeItems)
            {
                CreateExchangeItemsFromXMLNode(outputExchangeItem, "OutputExchangeItem");
            }

            var inputExchangeItems = root.SelectNodes("//InputExchangeItem");
            foreach (XmlNode inputExchangeItem in inputExchangeItems)
            {
                CreateExchangeItemsFromXMLNode(inputExchangeItem, "InputExchangeItem");
            }

            var timeHorizon = root.SelectSingleNode("//TimeHorizon");

            //Set IEngine properties
            _simulationStartTime = CalendarConverter.Gregorian2ModifiedJulian(Convert.ToDateTime(timeHorizon["StartDateTime"].InnerText));
            _simulationEndTime = CalendarConverter.Gregorian2ModifiedJulian(Convert.ToDateTime(timeHorizon["EndDateTime"].InnerText));

            // get the units
            var timeStepUnit = timeHorizon["TimeStepUnit"].InnerText;
            switch (timeStepUnit)
            {
                case "second":
                    _stepUnit = STEP_UNIT_SECOND;
                    break;
                case "day":
                    _stepUnit = STEP_UNIT_DAY;
                    break;
                case "year":
                    _stepUnit = STEP_UNIT_YEAR;
                    break;
            }

            // get the step length
            _stepLength = Convert.ToInt32(timeHorizon["TimeStep"].InnerText);

            // get the input time offset
            _stepInputOffset = Convert.ToInt32(timeHorizon["InputTimeOffset"].InnerText);
        }

        private void CreateExchangeItemsFromXMLNode(XmlNode ExchangeItem, string Identifier)
        {
            //Create Dimensions
            var omiDimensions = new Dimension();
            var dimensions = ExchangeItem.SelectNodes("//Dimensions/Dimension"); // You can filter elements here using XPath
            if (dimensions != null)
            {
                foreach (XmlNode dimension in dimensions)
                {
                    if (dimension["Base"].InnerText == "Length")
                    {
                        omiDimensions.SetPower(DimensionBase.Length, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "Time")
                    {
                        omiDimensions.SetPower(DimensionBase.Time, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "AmountOfSubstance")
                    {
                        omiDimensions.SetPower(DimensionBase.AmountOfSubstance, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "Currency")
                    {
                        omiDimensions.SetPower(DimensionBase.Currency, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "ElectricCurrent")
                    {
                        omiDimensions.SetPower(DimensionBase.ElectricCurrent, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "LuminousIntensity")
                    {
                        omiDimensions.SetPower(DimensionBase.LuminousIntensity, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "Mass")
                    {
                        omiDimensions.SetPower(DimensionBase.Mass, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                    else if (dimension["Base"].InnerText == "Temperature")
                    {
                        omiDimensions.SetPower(DimensionBase.Temperature, Convert.ToDouble(dimension["Power"].InnerText));
                    }
                }
            }

            //Create Units
            _omiUnits = new Unit();
            var units = ExchangeItem.SelectSingleNode("Quantity/Unit");
            if (units != null)
            {
                _omiUnits.ID = units["ID"].InnerText;
                if (units["Description"] != null) _omiUnits.Description = units["Description"].InnerText;
                if (units["ConversionFactorToSI"] != null) _omiUnits.ConversionFactorToSI = Convert.ToDouble(units["ConversionFactorToSI"].InnerText);
                if (units["OffSetToSI"] != null) _omiUnits.OffSetToSI = Convert.ToDouble(units["OffSetToSI"].InnerText);
            }

            //Create Quantity
            var omiQuantity = new Quantity();
            var quantity = ExchangeItem.SelectSingleNode("Quantity");
            omiQuantity.ID = quantity["ID"].InnerText;
            if (quantity["Description"] != null) omiQuantity.Description = quantity["Description"].InnerText;
            omiQuantity.Dimension = omiDimensions;
            omiQuantity.Unit = _omiUnits;
            omiQuantity.ValueType = ValueType.Scalar;
            if (quantity["ValueType"] != null)
            {
                if (quantity["ValueType"].InnerText == "Scalar")
                {
                    omiQuantity.ValueType = ValueType.Scalar;
                }
                else if (quantity["ValueType"].InnerText == "Vector")
                {
                    omiQuantity.ValueType = ValueType.Vector;
                }
            }

            //Create Element Set
            var omiElementSet = new ElementSet();
            var elementSet = ExchangeItem.SelectSingleNode("ElementSet");
            omiElementSet.ID = elementSet["ID"].InnerText;
            if (elementSet["Description"] != null) omiElementSet.Description = elementSet["Description"].InnerText;

            try
            {
                //add elements from shapefile to element set
                var utils = new Utilities();
                _shapefilepath = elementSet["ShapefilePath"].InnerText;
                omiElementSet = utils.AddElementsFromShapefile(omiElementSet, _shapefilepath);
            }
            catch (Exception)
            {
                Debug.WriteLine("An Element Set has not been declared using AddElementsFromShapefile");
            }

            try
            {
                // add elements from xml file to element set
                var utils = new Utilities();
                _xmlFilePath = elementSet["XmlFilePath"].InnerText;
                omiElementSet = utils.AddElementsFromXmlFile(omiElementSet, _xmlFilePath);
            }
            catch (Exception)
            {
                Debug.WriteLine("An Element Set has not been declared using AddElementsFromXmlFile");
            }


            if (Identifier == "OutputExchangeItem")
            {
                //create exchange item
                var omiOutputExchangeItem = new OutputExchangeItem();
                omiOutputExchangeItem.Quantity = omiQuantity;
                omiOutputExchangeItem.ElementSet = omiElementSet;

                //add the output exchange item to the list of output exchange items for the component
                _outputs.Add(omiOutputExchangeItem);
                if (!_quantities.ContainsKey(omiQuantity.ID)) _quantities.Add(omiQuantity.ID, omiQuantity);
                if (!_elementSets.ContainsKey(omiElementSet.ID)) _elementSets.Add(omiElementSet.ID, omiElementSet);
            }
            else if (Identifier == "InputExchangeItem")
            {
                //create exchange item
                var omiInputExchangeItem = new InputExchangeItem();
                omiInputExchangeItem.Quantity = omiQuantity;
                omiInputExchangeItem.ElementSet = omiElementSet;


                //add the output exchange item to the list of output exchange items for the component
                _inputs.Add(omiInputExchangeItem);
                if (!_quantities.ContainsKey(omiQuantity.ID)) _quantities.Add(omiQuantity.ID, omiQuantity);
                if (!_elementSets.ContainsKey(omiElementSet.ID)) _elementSets.Add(omiElementSet.ID, omiElementSet);
            }
        }

        /// <summary>
        ///     Adds columns to hold the components input and output the SMW's global data structure to
        /// </summary>
        public void SetValuesTableFields()
        {
            _values.Columns.Add("QuantityID", typeof (string));
            _values.Columns.Add("ElementSetID", typeof (string));
            _values.Columns.Add("ValueSet", typeof (IValueSet));
        }

        #endregion

        /// <summary>
        ///     Returns the model's constant time step length in whatever the units are.
        /// </summary>
        public double GetTimeStepLength()
        {
            return _stepLength;
        }

        /// <summary>
        ///     Use to get the shapefile path stored in config.xml
        /// </summary>
        /// <returns>the absolute path to the elementset shapefile</returns>
        public string GetShapefilePath()
        {
            return _shapefilepath;
        }

        /// <summary>
        ///     Use to get the xml file path stored in config.xml
        /// </summary>
        /// <returns>the absolute path to the elementset xml file</returns>
        public string GetElementSetXmlFilePath()
        {
            return _xmlFilePath;
        }

        /// <summary>
        ///     Gets the unit value that the component is implemented over
        /// </summary>
        /// <returns>unitID from config.xml</returns>
        public string GetUnits()
        {
            return _omiUnits.ID;
        }
    }
}