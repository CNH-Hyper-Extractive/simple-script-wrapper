//
// This is a derivative work of the Simple Model Wrapper library (MIT license)
// available at http://code.google.com/p/smw/
//
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Diagnostics;
using OpenMI.Standard;
using Oatc.OpenMI.Sdk.Backbone;
using Oatc.OpenMI.Sdk.DevelopmentSupport;
using Oatc.OpenMI.Sdk.Wrapper;
using System.Data;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Geometries;


//Notes
//Compiling with the "Release" option places the .dll into C:\Code\Dev.CSharp\OpenMI_Test\branches\Tony 01\Utilities\SDK
// so that all other projects are including the most recent version.  It might be better for all projects to reference
// the trunk version instead, to guarantee they are always using the most up-to-date version.
namespace Sc.Smw
{
    public abstract class Wrapper : Oatc.OpenMI.Sdk.Wrapper.IEngine
    {
        private const int SECONDS_IN_DAY = 86400;

        protected List<InputExchangeItem> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
            }
        }

        protected List<OutputExchangeItem> Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                _outputs = value;
            }
        }

        const int STEP_UNIT_SECOND = 1;
        const int STEP_UNIT_DAY = 2;
        const int STEP_UNIT_YEAR = 3;

        #region Global Objects
        private string _componentID = "Interpreter_Model_Component";
        private string _componentDescription = "Interpreter Model Component";
        private string _modelID;
        private string _modelDescription;
        private List<InputExchangeItem> _inputs = new List<InputExchangeItem>();
        private List<OutputExchangeItem> _outputs = new List<OutputExchangeItem>();
        private double _simulationStartTime;
        private double _simulationEndTime;
        private double _currentTime;        // in julian days
        //private double _timeStepLength;     // in days
        //private double _inputTimeOffset;    // + or - days from current time at which to get the inputs
        private int _stepLength;    // number of steps of the appropriate unit
        private int _stepUnit;      // time step units
        private int _stepInputOffset;
        private string _shapefilepath;
        private string _xmlFilePath;
        private Dictionary<string, Quantity> _quantities = new Dictionary<string, Quantity>();
        private Dictionary<string, ElementSet> _elementSets = new Dictionary<string, ElementSet>();
        private DataTable _values = new DataTable();
        Unit _omiUnits;
        protected string _adapterName;
        protected LogFile _logFile;
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Used to Initialize the component.  Performs routines that must be completed prior to simulation start.
        /// </summary>
        /// <param name="properties">properties extracted from the components *.omi file</param>
        public abstract void Initialize(System.Collections.Hashtable properties);
        public abstract bool PerformTimeStep();
        public abstract void Finish();
        #endregion

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
            else return _inputs.Count;
        }
        public int GetOutputExchangeItemCount()
        {
            if (_outputs == null) return 0;
            else return _outputs.Count;
        }
        public ITimeSpan GetTimeHorizon()
        {
            return new Oatc.OpenMI.Sdk.Backbone.TimeSpan(
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
        /// Returns the time at which the the inputs from other components is needed.
        /// </summary>
        public ITime GetInputTime(string QuantityID, string ElementSetID)
        {
            //return new TimeStamp(_currentTime); // original one where providing models run their time steps after this one
            //return new TimeStamp(_currentTime + (_timeStep / 86400)); // alternate one where providing models run their time steps before this one

            // use the GetCurrentTime() method so that it gets initialized if necessary
            TimeStamp ct = (TimeStamp)GetCurrentTime();

            // add the input time offset to the current time
            //double inputTime = ct.ModifiedJulianDay + _inputTimeOffset;

            DateTime dt = CalendarConverter.ModifiedJulian2Gregorian(ct.ModifiedJulianDay);
            switch (_stepUnit)
            {
                case STEP_UNIT_SECOND:
                    dt = dt.AddSeconds(_stepInputOffset * _stepLength);
                    break;
                case STEP_UNIT_DAY:
                    dt = dt.AddDays(_stepInputOffset * _stepLength);
                    break;
                case STEP_UNIT_YEAR:
                    dt = dt.AddYears(_stepInputOffset * _stepLength);
                    break;
            }
            double inputTime = CalendarConverter.Gregorian2ModifiedJulian(dt);

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
        /// This method is used to extract values from an upstream component.
        /// </summary>
        /// <param name="QuantityID">The input Quantity ID</param>
        /// <param name="ElementSetID">The input Element Set ID</param>
        /// <returns>the values saved under the matching QuantityID and ElementSetID, from an upstream component</returns>
        public IValueSet GetValues(string QuantityID, string ElementSetID)
        {
            DataRow[] rows = _values.Select("QuantityID = '" + QuantityID + "' AND ElementSetID = '" + ElementSetID + "'");
            if (rows.Length == 1)
                return (IValueSet)rows[0].ItemArray[2];
            else
            {
                ElementSet es = _elementSets[ElementSetID];
                ScalarSet ss = new ScalarSet();
                ss.data = new double[es.ElementCount];
                return (IValueSet)ss;
            }
        }

        public void SetValues(string QuantityID, string ElementSetID, IValueSet values)
        {
            //_logFile.Append("SetValues: " + QuantityID + " (" + values.Count + ") table: " + _values.Rows.Count);

            // TODO: change this to store all values with a time stamp and change
            // getvlaues to accept a parameter to get an aggregated set of values

            // -- check to see if QuantityID and ElementSetID already exits in Values.  If so, delete that row before adding.
            DataRow[] rows = _values.Select("QuantityID = '" + QuantityID + "' AND ElementSetID = '" + ElementSetID + "'");

            if (rows.Length == 1)
            {
                _values.Rows.Remove(rows[0]);
            }
            _values.BeginLoadData();
            DataRow dr = _values.LoadDataRow(new object[] { QuantityID, ElementSetID, values }, true);
            _values.EndLoadData();

        }

        #region Auxilary Methods

        /// <summary>
        /// This method will advance the components in time, by a single timestep.  
        /// </summary>
        /// <remarks>
        /// This should be called at the end of Perform Time Step.
        /// </remarks>
        public void AdvanceTime()
        {
            //TimeStamp ct = (TimeStamp)GetCurrentTime();
            //_currentTime = ct.ModifiedJulianDay + _timeStep / 86400;

            // use the GetCurrentTime() method so that it gets initialized if necessary
            TimeStamp ct = (TimeStamp)GetCurrentTime();
            //_currentTime = ct.ModifiedJulianDay + _timeStepLength;

            DateTime dt = CalendarConverter.ModifiedJulian2Gregorian(ct.ModifiedJulianDay);
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
        /// Reads the Configuration file, and creates OpenMI exchange items 
        /// </summary>
        /// <param name="configFile">path pointing to the components comfiguration (XML) file</param>
        public void SetVariablesFromConfigFile(string configFile)
        {
            //Read config file
            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);

            XmlElement root = doc.DocumentElement;

            XmlNode ID = root.SelectSingleNode("ModelInfo//ID");
            _modelID = ID.InnerText;

            XmlNode Desc = root.SelectSingleNode("ModelInfo//Description");
            if(Desc != null)
                _componentDescription = Desc.InnerText;

            XmlNode AdapterName = root.SelectSingleNode("ModelInfo//ScriptingLanguage");
            if(AdapterName != null)
                _adapterName = AdapterName.InnerText;

            XmlNodeList outputExchangeItems = root.SelectNodes("//OutputExchangeItem");
            foreach (XmlNode outputExchangeItem in outputExchangeItems)
            {
                CreateExchangeItemsFromXMLNode(outputExchangeItem, "OutputExchangeItem");
            }

            XmlNodeList inputExchangeItems = root.SelectNodes("//InputExchangeItem");
            foreach (XmlNode inputExchangeItem in inputExchangeItems)
            {
                CreateExchangeItemsFromXMLNode(inputExchangeItem, "InputExchangeItem");
            }

            XmlNode timeHorizon = root.SelectSingleNode("//TimeHorizon");

            //Set IEngine properties
            this._simulationStartTime = CalendarConverter.Gregorian2ModifiedJulian(Convert.ToDateTime(timeHorizon["StartDateTime"].InnerText));
            this._simulationEndTime = CalendarConverter.Gregorian2ModifiedJulian(Convert.ToDateTime(timeHorizon["EndDateTime"].InnerText));

            // get the units
            string timeStepUnit = timeHorizon["TimeStepUnit"].InnerText;
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
            Dimension omiDimensions = new Dimension();
            XmlNodeList dimensions = ExchangeItem.SelectNodes("//Dimensions/Dimension"); // You can filter elements here using XPath
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
            XmlNode units = ExchangeItem.SelectSingleNode("Quantity/Unit");
            if (units != null)
            {
                _omiUnits.ID = units["ID"].InnerText;
                if (units["Description"] != null) _omiUnits.Description = units["Description"].InnerText;
                if (units["ConversionFactorToSI"] != null) _omiUnits.ConversionFactorToSI = Convert.ToDouble(units["ConversionFactorToSI"].InnerText);
                if (units["OffSetToSI"] != null) _omiUnits.OffSetToSI = Convert.ToDouble(units["OffSetToSI"].InnerText);
            }

            //Create Quantity
            Quantity omiQuantity = new Quantity();
            XmlNode quantity = ExchangeItem.SelectSingleNode("Quantity");
            omiQuantity.ID = quantity["ID"].InnerText;
            if (quantity["Description"] != null) omiQuantity.Description = quantity["Description"].InnerText;
            omiQuantity.Dimension = omiDimensions;
            omiQuantity.Unit = _omiUnits;
            omiQuantity.ValueType = OpenMI.Standard.ValueType.Scalar;
            if (quantity["ValueType"] != null)
            {
                if (quantity["ValueType"].InnerText == "Scalar")
                {
                    omiQuantity.ValueType = OpenMI.Standard.ValueType.Scalar;
                }
                else if (quantity["ValueType"].InnerText == "Vector")
                {
                    omiQuantity.ValueType = OpenMI.Standard.ValueType.Vector;
                }
            }

            //Create Element Set
            ElementSet omiElementSet = new ElementSet();
            XmlNode elementSet = ExchangeItem.SelectSingleNode("ElementSet");
            omiElementSet.ID = elementSet["ID"].InnerText;
            if (elementSet["Description"] != null) omiElementSet.Description = elementSet["Description"].InnerText;

            try
            {
                //add elements from shapefile to element set
                Utilities utils = new Utilities();
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
                Utilities utils = new Utilities();
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
                OutputExchangeItem omiOutputExchangeItem = new OutputExchangeItem();
                omiOutputExchangeItem.Quantity = omiQuantity;
                omiOutputExchangeItem.ElementSet = omiElementSet;

                //add the output exchange item to the list of output exchange items for the component
                this._outputs.Add(omiOutputExchangeItem);
                if (!this._quantities.ContainsKey(omiQuantity.ID)) this._quantities.Add(omiQuantity.ID, omiQuantity);
                if (!this._elementSets.ContainsKey(omiElementSet.ID)) this._elementSets.Add(omiElementSet.ID, omiElementSet);
            }
            else if (Identifier == "InputExchangeItem")
            {
                //create exchange item
                InputExchangeItem omiInputExchangeItem = new InputExchangeItem();
                omiInputExchangeItem.Quantity = omiQuantity;
                omiInputExchangeItem.ElementSet = omiElementSet;


                //add the output exchange item to the list of output exchange items for the component
                this._inputs.Add(omiInputExchangeItem);
                if (!this._quantities.ContainsKey(omiQuantity.ID)) this._quantities.Add(omiQuantity.ID, omiQuantity);
                if (!this._elementSets.ContainsKey(omiElementSet.ID)) this._elementSets.Add(omiElementSet.ID, omiElementSet);
            }
        }

        /// <summary>
        /// Adds columns to hold the components input and output the SMW's global data structure to 
        /// </summary>
        public void SetValuesTableFields()
        {
            _values.Columns.Add("QuantityID", typeof(string));
            _values.Columns.Add("ElementSetID", typeof(string));
            _values.Columns.Add("ValueSet", typeof(IValueSet));

        }

        #endregion


        /// <summary>
        /// Returns the model's constant time step length in whatever the units are.
        /// </summary>
        public double GetTimeStepLength()
        {
            return _stepLength;
        }
        /// <summary>
        /// Use to get the shapefile path stored in config.xml
        /// </summary>
        /// <returns>the absolute path to the elementset shapefile</returns>
        public string GetShapefilePath()
        {
            return _shapefilepath;
        }
        /// <summary>
        /// Use to get the xml file path stored in config.xml
        /// </summary>
        /// <returns>the absolute path to the elementset xml file</returns>
        public string GetElementSetXmlFilePath()
        {
            return _xmlFilePath;
        }
        /// <summary>
        /// Gets the unit value that the component is implemented over
        /// </summary>
        /// <returns>unitID from config.xml</returns>
        public string GetUnits()
        {
            return _omiUnits.ID;
        }


    }

}
