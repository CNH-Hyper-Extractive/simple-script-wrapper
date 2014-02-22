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
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Oatc.OpenMI.Sdk.Backbone;
using OpenMI.Standard;
using SharpMap.Data.Providers;
using SharpMap.Geometries;
using SharpMap.Layers;

namespace Sc.Smw
{
    //
    // This is a derivative work of the Simple Model Wrapper library (MIT license)
    // available at http://code.google.com/p/smw/
    //
    public class ODM
    {
        #region Accessors for CreateODMcsv

        /// <summary>
        ///     Site latitude in degrees.
        /// </summary>
        /// <example> 41.7 </example>
        private string _Latitude = "41.7";

        /// <summary>
        ///     Site longitude in degrees.
        /// </summary>
        /// <example> -111.9 </example>
        private string _Longitude = "-111.9";

        private string _PosAccuracy = "1";

        /// <summary>
        ///     Name of the site. Required
        /// </summary>
        private string _SiteName = "unknown";

        private string _abstract = "none";
        private string _address = "unknown";
        private string _censorCode = "nc";
        private string _citation = "none";
        private string _city = "unknown";
        private string _comment = "none";
        private string _contactName = "unknown";

        /// <summary>
        ///     County in which the site is located
        /// </summary>
        /// <example> Richland </example>
        private string _county = "Richland";

        /// <summary>
        ///     Type of data used
        /// </summary>
        private string _dataType = "Incremental";

        /// <summary>
        ///     Coordinate datum.
        /// </summary>
        /// <example> 2 </example>
        private string _datum = "2";

        private string _definition = "none";

        /// <summary>
        ///     Site elevation
        /// </summary>
        /// <example> 0 </example>
        private string _elevation = "0";

        private string _email = "unknown";
        private string _explaination = "none";
        private string _generalCategory = "Hydrology";
        private string _isRegular = "1";
        private string _labMethodDesc = "";
        private string _labMethodLink = "";
        private string _labMethodName = "";
        private string _labName = "";
        private string _labOrganization = "";
        private string _labSampleCode = "";
        private string _localProjectionID = "105";

        /// <summary>
        ///     Site's 'X' coordinate
        /// </summary>
        /// <example> 421276.323 </example>
        private string _localX = "421276.323";

        /// <summary>
        ///     Sites 'Y' coordinate
        /// </summary>
        /// <example> 4618952.04 </example>
        private string _localY = "4618952.04";

        private string _metadata = "";

        /// <summary>
        ///     Description of modeling technique
        /// </summary>
        private string _methodDesc = "none";

        /// <summary>
        ///     Web link that explains the methods used
        /// </summary>
        private string _methodLink = "none";

        private string _noDataValue = "-9999";
        private string _offsetDesc = "";
        private string _offsetUnitsID = "";
        private string _offsetValue = "";
        private string _organization = "University of South Carolina";
        private string _phone = "unknown";
        private string _profileVersion = "none";
        private string _qualifierCode = "";
        private string _qualifierDesc = "";
        private string _qualityControlCode = "0";
        private string _sampleMedium = "Other";
        private string _sampleType = "";

        /// <summary>
        ///     Corresponding code associated with the SiteName.  If one doesn't exist this is can be used to assign one. Required
        /// </summary>
        private string _siteCode = "0";

        /// <summary>
        ///     State in which the site is located
        /// </summary>
        /// <example> SC </example>
        private string _siteState = "SC";

        private string _sourceDesc = "Continuous";
        private string _sourceLink = "none";
        private string _sourceState = "unknown";
        private string _speciation = "Not Applicable";

        /// <summary>
        ///     The time increment which is supported
        /// </summary>
        private string _timeSupport = "30";

        /// <summary>
        ///     Units of time support
        /// </summary>
        private string _timeUnitsID = "102";

        private string _title = "none";
        private string _topicCategory = "inlandWaters";

        /// <summary>
        ///     Value accuracy
        /// </summary>
        private string _valAccuracy = "1";

        private string _valueType = "Field Observation";
        private string _variableCode = "3";
        private string _variableName = "Streamflow";
        private string _variableUnitsID = "35";

        /// <summary>
        ///     Vertical Datum
        /// </summary>
        /// <example> NGVD29 </example>
        private string _verticalDatum = "NGVD29";

        private string _zipCode = "unknown";

        public string SiteName
        {
            get { return _SiteName; }
            set
            {
                //remove spaces from name
                _SiteName = value.Replace(" ", String.Empty);
            }
        }

        public string SiteCode
        {
            get { return _siteCode; }
            set { _siteCode = value; }
        }

        public string Latitude
        {
            get { return _Latitude; }
            set { _Latitude = value; }
        }

        public string Longitude
        {
            get { return _Longitude; }
            set { _Longitude = value; }
        }

        public string ValAccuracy
        {
            get { return _valAccuracy; }
            set { _valAccuracy = value; }
        }

        public string Datum
        {
            get { return _datum; }
            set { _datum = value; }
        }

        public string Elevation
        {
            get { return _elevation; }
            set { _elevation = value; }
        }

        public string VerticalDatum
        {
            get { return _verticalDatum; }
            set { _verticalDatum = value; }
        }

        public string LocalX
        {
            get { return _localX; }
            set { _localX = value; }
        }

        public string LocalY
        {
            get { return _localY; }
            set { _localY = value; }
        }

        public string LocalProjectionID
        {
            get { return _localProjectionID; }
            set { _localProjectionID = value; }
        }

        public string PosAccuracy
        {
            get { return _PosAccuracy; }
            set { _PosAccuracy = value; }
        }

        public string SiteState
        {
            get { return _siteState; }
            set { _siteState = value; }
        }

        public string County
        {
            get { return _county; }
            set { _county = value; }
        }


        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public string MethodDesc
        {
            get { return _methodDesc; }
            set { _methodDesc = value; }
        }

        public string MethodLink
        {
            get { return _methodLink; }
            set { _methodLink = value; }
        }

        public string QualifierCode
        {
            get { return _qualifierCode; }
            set { _qualifierCode = value; }
        }

        public string QualifierDesc
        {
            get { return _qualifierDesc; }
            set { _qualifierDesc = value; }
        }

        public string Speciation
        {
            get { return _speciation; }
            set { _speciation = value; }
        }

        public string SampleMedium
        {
            get { return _sampleMedium; }
            set { _sampleMedium = value; }
        }

        public string ValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        public string IsRegular
        {
            get { return _isRegular; }
            set { _isRegular = value; }
        }

        public string TimeSupport
        {
            get { return _timeSupport; }
            set { _timeSupport = value; }
        }

        public string TimeUnitsID
        {
            get { return _timeUnitsID; }
            set { _timeUnitsID = value; }
        }

        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public string GeneralCategory
        {
            get { return _generalCategory; }
            set { _generalCategory = value; }
        }

        public string NoDataValue
        {
            get { return _noDataValue; }
            set { _noDataValue = value; }
        }

        public string OffsetUnitsID
        {
            get { return _offsetUnitsID; }
            set { _offsetUnitsID = value; }
        }

        public string OffsetDesc
        {
            get { return _offsetDesc; }
            set { _offsetDesc = value; }
        }

        public string QualityControlCode
        {
            get { return _qualityControlCode; }
            set { _qualityControlCode = value; }
        }

        public string Definition
        {
            get { return _definition; }
            set { _definition = value; }
        }

        public string Explaination
        {
            get { return _explaination; }
            set { _explaination = value; }
        }

        public string Organization
        {
            get { return _organization; }
            set { _organization = value; }
        }

        public string SourceDesc
        {
            get { return _sourceDesc; }
            set { _sourceDesc = value; }
        }

        public string SourceLink
        {
            get { return _sourceLink; }
            set { _sourceLink = value; }
        }

        public string ContactName
        {
            get { return _contactName; }
            set { _contactName = value; }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        public string SourceState
        {
            get { return _sourceState; }
            set { _sourceState = value; }
        }

        public string ZipCode
        {
            get { return _zipCode; }
            set { _zipCode = value; }
        }

        public string Citation
        {
            get { return _citation; }
            set { _citation = value; }
        }

        public string TopicCategory
        {
            get { return _topicCategory; }
            set { _topicCategory = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Abstract
        {
            get { return _abstract; }
            set { _abstract = value; }
        }

        public string ProfileVersion
        {
            get { return _profileVersion; }
            set { _profileVersion = value; }
        }

        public string Metadata
        {
            get { return _metadata; }
            set { _metadata = value; }
        }

        public string SampleType
        {
            get { return _sampleType; }
            set { _sampleType = value; }
        }

        public string LabSampleCode
        {
            get { return _labSampleCode; }
            set { _labSampleCode = value; }
        }

        public string LabName
        {
            get { return _labName; }
            set { _labName = value; }
        }

        public string LabOrganization
        {
            get { return _labOrganization; }
            set { _labOrganization = value; }
        }

        public string LabMethodName
        {
            get { return _labMethodName; }
            set { _labMethodName = value; }
        }

        public string LabMethodDesc
        {
            get { return _labMethodDesc; }
            set { _labMethodDesc = value; }
        }

        public string LabMethodLink
        {
            get { return _labMethodLink; }
            set { _labMethodLink = value; }
        }

        public string OffsetValue
        {
            get { return _offsetValue; }
            set { _offsetValue = value; }
        }

        public string CensorCode
        {
            get { return _censorCode; }
            set { _censorCode = value; }
        }

        public string VariableCode
        {
            get { return _variableCode; }
            set { _variableCode = value; }
        }

        public string VariableName
        {
            get { return _variableName; }
            set { _variableName = value; }
        }

        public string VariableUnitsID
        {
            get { return _variableUnitsID; }
            set { _variableUnitsID = value; }
        }

        public ArrayList Values { get; set; }

        public ArrayList DateTimes { get; set; }

        #endregion

        //Define all of the necessary ODM parameters within this class (with default values) and then reference these
        // within the Utilities class.  The developer can choose to set the parameters they want before calling the 
        // ODM utiity methods (within their engine class).

        //The developer must enter Values, DateTimes, and StreamWriter object to create the .csv file

        /// <summary>
        ///     This method is used to add definitions (i.e. units, methods, variabletype, etc...) into ODM database.
        /// </summary>
        /// <param name="odm_path">Full path to ODM database</param>
        /// <param name="tablename">Name of table in which definition will be appended</param>
        /// <param name="parameter_names">
        ///     array of parameter names to be entered in database.   Must be in same order as existing tables.
        ///     NOTE: This method will fail if manditory data is omitted, see ODM database for required table entries.
        /// </param>
        /// ///
        /// <param name="parameter_values">
        ///     array of parameter values to be entered in database.   Must be in same order as
        ///     'parameter_names'.
        /// </param>
        public void Add_ODM_Definition(string odm_path, string tablename, string[] parameter_names, string[] parameter_values)
        {
            //C:\\Research\\CUAHSI\\ODM\\Blank Template ODM 1.1\\OD.mdf

            //HACK: The modeler should be responsible for creating the connection to the OD database
            //open Sql Connection
            var myconn = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;" +
                "Initial Catalog = OD;" +
                "User ID = sa" +
                "Password = sa");
            //System.Data.SqlClient.SqlConnection myconn = 
            //new System.Data.SqlClient.SqlConnection("Data Source=.\\SQLEXPRESS;" +
            //                                        "AttachDbFilename="+odm_path+";" +
            //                                        "Integrated Security=True;" +
            //                                        "Connect Timeout=30;" +
            //                                        "user id = ODM_Loader"+
            //                                        "Pwd=ODM_Loader");

            try
            {
                myconn.Open();
            }
            catch (Exception)
            {
            }
            ;

            //Create insert string
            var insert = "INSERT INTO" + tablename + "(";
            foreach (var param in parameter_names)
            {
                insert += (param + ",");
            }
            insert += ") Values (";
            foreach (var param in parameter_values)
            {
                insert += (param + ",");
            }
            insert += ")";

            //Create SQL command
            var myCommand = new SqlCommand(insert, myconn);

            //Load data into ODM
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
            ;
        }


        /// <summary>
        ///     Writes the header information needed prior to writing data
        /// </summary>
        /// <param name="SW"> StreamWriter object pointing to the location where the .csv file will be created</param>
        public void CreateODMcsv()
        {
            if (!Directory.Exists(CSVPath))
                Directory.CreateDirectory(CSVPath);


            var SW = new StreamWriter(CSVPath + "\\" + SiteName + ".csv");
            //Write Header Info
            SW.WriteLine("DataValue,ValueAccuracy,LocalDateTime,DateTimeUTC,SiteCode,SiteName," +
                         "Latitude,Longitude,LatLongDatumID,Elevation_m,VerticalDatum,LocalX," +
                         "LocalY,LocalProjectionID,PosAccuracy_m,SiteState,County,Comments," +
                         "MethodDescription,MethodLink,QualifierCode,QualifierDescription," +
                         "VariableCode,VariableName,Speciation,VariableUnitsID,SampleMedium," +
                         "ValueType,IsRegular,TimeSupport,TimeUnitsID,DataType,GeneralCategory," +
                         "NoDataValue,OffsetUnitsID,OffsetDescription,QualityControlLevelCode," +
                         "Definition,Explanation,Organization,SourceDescription,SourceLink,ContactName," +
                         "Phone,Email,Address,City,SourceState,ZipCode,Citation,TopicCategory,Title," +
                         "Abstract,ProfileVersion,MetadataLink,SampleType,LabSampleCode,LabName,LabOrganization," +
                         "LabMethodName,LabMethodDescription,LabMethodLink,OffsetValue,CensorCode");


            //Write out values


            //loop through all the values stored in the Values arraylist
            var i = 0;

            foreach (double value in Values)
            {
                var UTCDateTime = Convert.ToDateTime(DateTimes[i]);

                SW.Write(value + "," + //Data Values
                         ValAccuracy + "," + //Value Accuracy
                         DateTimes[i] + ", " + //LocalDateTime
                         UTCDateTime.ToUniversalTime() + "," + //DateTimeUTC
                         SiteCode + " ," + //SiteCode
                         SiteName + "," + //SiteName
                         Latitude + "," + //Latitude
                         Longitude + "," + //Longitude
                         Datum + "," + //Lat Lon Datum
                         Elevation + "," + //Elevation
                         VerticalDatum + "," + //Vertical Datum
                         LocalX + "," + //Local X
                         LocalY + "," + //Local Y
                         LocalProjectionID + "," + //Local Projection ID
                         PosAccuracy + "," + //Pos Accuracy_m
                         SiteState + "," + //SiteState
                         County + "," + //County
                         Comment + "," + //Comment
                         MethodDesc + "," + //Method Description
                         MethodLink + "," + //MethodLink
                         QualifierCode + "," + //Qualifier Code
                         QualifierDesc + "," + //Qualifier Desc
                         VariableCode + "," + //Variable Code
                         VariableName + "," + //Variable Name
                         Speciation + "," + //Speciation
                         VariableUnitsID + "," + //VariableUnitsID
                         SampleMedium + "," + //Sample Medium
                         ValueType + "," + //ValueType
                         IsRegular + "," + //IsRegular
                         TimeSupport + "," + //TimeSupport
                         TimeUnitsID + "," + //TimeUnitsID
                         DataType + "," + //DataType
                         GeneralCategory + "," + //General Category
                         NoDataValue + "," + //NoDataValue
                         OffsetUnitsID + "," + //OffsetUnitsID
                         OffsetDesc + "," + //Offset Desciption
                         QualityControlCode + "," + //Quality Control Level Code
                         Definition + "," + //Definition
                         Explaination + "," + //Explaination
                         Organization + "," + //Organization
                         SourceDesc + "," + //Source Desc
                         SourceLink + "," + //SourceLink
                         ContactName + "," + //ContactName
                         Phone + "," + //Phone
                         Email + "," + //Email
                         Address + "," + //Address
                         City + "," + //City
                         SourceState + "," + //Source State
                         ZipCode + "," + //Zip Code
                         Citation + "," + //Citation
                         TopicCategory + "," + //Topic Category
                         Title + "," + //Title
                         Abstract + "," + //Abstract
                         ProfileVersion + "," + //Profile version
                         Metadata + "," + //Metadata
                         SampleType + "," + //SampleType
                         LabSampleCode + "," + //LabSampleCode
                         LabName + "," + //LabName
                         LabOrganization + "," + //LabOrganization
                         LabMethodName + "," + //LabMethodName
                         LabMethodDesc + "," + //LabMethodDescription
                         LabMethodLink + "," + //LabMethodLink
                         OffsetValue + "," + //Offset Value
                         CensorCode + "\n"); //Censor Code

                i++;
            }
            SW.Close();
        }


        //HACK: Remove this because it has phased out
        /// <summary>
        ///     Writes model data to csv file, in a format compatible with ODM.Loader (use Utilites.CreateODMcsv to write header
        ///     info first)
        /// </summary>
        /// <param name="SW">Stream Writer instance </param>
        /// <param name="UTCDateTimes">Array of DateTimes, UTC format</param>
        /// <param name="values">Array of output values, should be organized to match with UTCDateTimes[]</param>
        /// <param name="UnitCode">integer code corresponding to the Unit ID, from ODM 'Units' table </param>
        /// <param name="VariableCode">integer code corresponding to the Variable ID, from ODM 'Variables' table</param>
        /// <param name="variableName">Name of variable in string format</param>
        /// <param name="SiteName">Name of Site in string format</param>
        public void WriteToODMcsv
            (StreamWriter SW, ArrayList DateTimes, ArrayList values, int UnitCode,
                int VariableCode, string variableName, string Sitename)
        {
            var Params = new ODM();


            var i = 0;
            foreach (double value in values)
            {
                var UTCDateTime = Convert.ToDateTime(DateTimes[i]);

                SW.Write(value + "," + //Data Values
                         "1," + //Value Accuracy
                         DateTimes[i] + ", " + //LocalDateTime
                         UTCDateTime.ToUniversalTime() + "," + //DateTimeUTC
                         "1 ," + //SiteCode
                         Sitename + "," + //SiteName
                         "41.718473," + //Latitude
                         "-111.946402," + //Longitude
                         "2," + //Lat Lon Datum
                         "0," + //Elevation
                         "NGVD29," + //Vertical Datum
                         "421276.323," + //Local X
                         "4618952.04," + //Local Y
                         "105," + //Local Projection ID
                         "," + //Pos Accuracy_m
                         "SC," + //SiteState
                         "Richland," + //County
                         "none," + //Comment
                         "Component Modeling," + //Method Description
                         "http://www.campbellsci.com," + //MethodLink
                         "," + //Qualifier Code
                         "," + //Qualifier Desc
                         VariableCode + "," + //Variable Code
                         variableName + "," + //Variable Name
                         "Not Applicable," + //Speciation
                         UnitCode + "," + //VariableUnitsID
                         "Other," + //Sample Medium
                         "Field Observation," + //ValueType
                         "1," + //IsRegular
                         "30," + //TimeSupport
                         "102," + //TimeUnitsID
                         "Incremental," + //DataType
                         "Hydrology," + //General Category
                         "-9999," + //NoDataValue
                         "," + //OffsetUnitsID
                         "," + //Offset Desciption
                         "0," + //Quality Control Level Code
                         "none," + //Definition
                         "none," + //Explaination
                         "Universit of South Carolina," + //Organization
                         "Continuous," + //Source Desc
                         "none," + //SourceLink
                         "unknown," + //ContactName
                         "unknown," + //Phone
                         "unknown," + //Email
                         "unknown," + //Address
                         "unknown," + //City
                         "unknown," + //Source State
                         "unknown," + //Zip Code
                         "none," + //Citation
                         "inlandWaters," + //Topic Category
                         "none," + //Title
                         "none," + //Abstract
                         "none," + //Profile version
                         "," + //Metadata
                         "," + //SampleType
                         "," + //LabSampleCode
                         "," + //LabName
                         "," + //LabOrganization
                         "," + //LabMethodName
                         "," + //LabMethodDescription
                         "," + //LabMethodLink
                         "," + //Offset Value
                         "nc" + //Censor Code
                         "\n");
                i++;
            }
        }


        //TODO: accept relative paths
        /// <summary>
        ///     This method loads a .csv file into ODM.
        /// </summary>
        /// <param name="path">path to the odm .bat file</param>
        public void LoadIntoODM()
        {
            var p = new Process();
            p.EnableRaisingEvents = false;

            p.StartInfo.FileName = CSVPath + "//" + SiteName + ".bat";
            p.Start();
            p.Close();
        }


        //TODO: accept relative paths
        //HACK: Put into separate class, so that all of the accessors for this method are in the same location
        /// <summary>
        ///     This method creates a .bat file is necessary for the LoadODMcsv method.  Returns the full path of the bat file.
        /// </summary>
        /// <param name="path">path to save the .bat file</param>
        /// <remarks>Before calling this method, alter the values of Server, database. user, password, file, and log</remarks>
        public void CreateBat()
        {
            var sw = new StreamWriter(CSVPath + "\\" + SiteName + ".bat", false);

            sw.WriteLine("odmloader.exe -server {0} -db {1} -user {2} -password {3} -file {4} -log {5}", Server, Database, User, Password, SiteName + ".csv", Log);
            //TODO: For debugging only, Remove.
            sw.WriteLine("pause");
            sw.Close();
        }

        #region Accessors for CreateBAT

        private string _database = "OD";
        private string _file = "";
        private string _log = "log.txt";
        private string _password = "sa";
        private string _server = "CE-51\\SQLEXPRESS";
        private string _user = "sa";

        public ODM()
        {
            DateTimes = null;
            Values = null;
        }

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public string Database
        {
            get { return _database; }
            set { _database = value; }
        }

        public string User
        {
            get { return _user; }
            set { _user = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string CSVPath
        {
            get { return _file; }
            set { _file = value; }
        }

        public string Log
        {
            get { return _log; }
            set { _log = value; }
        }

        #endregion
    }


    //HACK: change this class name to GIS
    public class Utilities
    {
        #region GIS Utility Methods

        /// <summary>
        ///     This method adds vertices to the omiElementSet.  It provides the spatial representation for
        ///     the element set.  The vertices are extracted from the components input shapefile.
        /// </summary>
        /// <param name="omiElementSet">the components element set object</param>
        /// <param name="shapefilePath">path to a shapefile, spatially defining the elementset</param>
        /// <returns>the elementset with vertices added to it from the shapefile</returns>
        public ElementSet AddElementsFromShapefile(ElementSet omiElementSet, string shapefilePath)
        {
            //this uses the free SharpMap API for reading a shapefile
            var myLayer = new VectorLayer("elements_layer");
            myLayer.DataSource = new ShapeFile(shapefilePath);
            myLayer.DataSource.Open();

            //set spatial reference from shapefile
            var sprf = new SpatialReference();
            sprf.ID = myLayer.DataSource.SRID.ToString();
            omiElementSet.SpatialReference = sprf;

            //add elements to elementset from shapefile
            for (uint i = 0; i < myLayer.DataSource.GetFeatureCount(); ++i)
            {
                var feat = myLayer.DataSource.GetFeature(i);
                var GeometryType = Convert.ToString(
                    feat.Geometry.AsText().Substring(
                        0, feat.Geometry.AsText().IndexOf(' ')));

                var e = new Element();

                if (feat.Table.Columns.IndexOf("HydroCode") != -1)
                    e.ID = feat.ItemArray[feat.Table.Columns.IndexOf("HydroCode")].ToString();

                if (GeometryType == "POINT")
                {
                    omiElementSet.ElementType = ElementType.XYPoint;
                    var p = (Point)feat.Geometry;
                    var v = new Vertex();
                    v.x = p.X;
                    v.y = p.Y;
                    e.AddVertex(v);
                }
                if (GeometryType == "POLYGON")
                {
                    omiElementSet.ElementType = ElementType.XYPolygon;
                    var p = (Polygon)feat.Geometry;
                    var lr = p.ExteriorRing;

                    //Only loop until lr.Vertices.Count-2 b/c the first element is the same
                    // as the last element within the exterior ring.  This will thrown an error
                    // within the OATC element mapper, when trying to map elements.  Also this
                    // loop arranges the vertices of the exterior ring in counter clockwise order
                    // as needed for the element mapping.
                    for (var j = lr.Vertices.Count - 2; j >= 0; j--)
                    {
                        var v = new Vertex();
                        v.x = lr.Vertices[j].X;
                        v.y = lr.Vertices[j].Y;
                        e.AddVertex(v);
                    }
                }
                if (GeometryType == "LINESTRING")
                {
                    omiElementSet.ElementType = ElementType.XYPolyLine;
                    var ls = (LineString)feat.Geometry;
                    //Point endpt = ls.EndPoint;
                    //Point startpt = ls.StartPoint;
                    for (var j = 0; j < ls.Vertices.Count; j++)
                    {
                        var v = new Vertex();
                        v.x = ls.Vertices[j].X;
                        v.y = ls.Vertices[j].Y;
                        e.AddVertex(v);
                    }
                }
                omiElementSet.AddElement(e);
            }
            return omiElementSet;
        }

        /// <summary>
        ///     This method adds vertices to the omiElementSet. It provides the spatial representation for
        ///     the element set. The vertices are extracted from the components input xml file.
        /// </summary>
        /// <param name="omiElementSet">the components element set object</param>
        /// <param name="xmlFilePath">path to an xml file, spatially defining the elementset</param>
        /// <returns>the elementset with vertices added to it from the shapefile</returns>
        public ElementSet AddElementsFromXmlFile(ElementSet omiElementSet, string xmlFilePath)
        {
            // we want to look through the given xml file to find the element
            // set that corresponds to the element set that we were given, and
            // then add the vertices from the file into the element set.

            // read the xml document
            var doc = new XmlDocument();
            doc.Load(xmlFilePath);

            // get the list of element sets
            var elementSets = doc.SelectNodes("/ElementSets");

            // look through each element set
            foreach (XmlNode node in elementSets)
            {
                // get the element set node
                var elementSetNode = node.SelectSingleNode("ElementSet");

                // read the properties
                var id = elementSetNode.SelectSingleNode("ID").FirstChild.Value;
                var description = elementSetNode.SelectSingleNode("Description").FirstChild.Value;
                var version = Convert.ToInt32(elementSetNode.SelectSingleNode("Version").FirstChild.Value);
                var elementType = getXmlElementType(elementSetNode.SelectSingleNode("Kind").FirstChild.Value);

                // see if this is the one we're looking for
                if (id == omiElementSet.ID)
                {
                    // read the elements
                    var elementNodes = elementSetNode.SelectNodes("Elements/Element");

                    // read each element
                    foreach (XmlNode elementNode in elementNodes)
                    {
                        var e = new Element();

                        e.ID = elementNode.SelectSingleNode("ID").FirstChild.Value;

                        var xString = elementNode.SelectSingleNode("X").FirstChild.Value;
                        var yString = elementNode.SelectSingleNode("Y").FirstChild.Value;

                        var v = new Vertex();
                        v.x = Double.Parse(xString);
                        v.y = Double.Parse(yString);
                        e.AddVertex(v);

                        omiElementSet.AddElement(e);
                    }

                    // we found the one we're looking for, so we can stop
                    break;
                }
            }

            return omiElementSet;
        }

        private static ElementType getXmlElementType(String value)
        {
            switch (value)
            {
                case "XYPoint":
                    return ElementType.XYPoint;
                case "XYLine":
                    return ElementType.XYLine;
                case "XYPolygon":
                    return ElementType.XYPolygon;
                default:
                    return ElementType.XYPoint;
            }
        }

        #endregion
    }
}