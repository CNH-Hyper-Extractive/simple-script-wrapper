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
using System.Xml;
using Oatc.OpenMI.Sdk.Backbone;
using OpenMI.Standard;

namespace KansasState.Ssw.FileElementSet
{
    public class XmlElementSet : IElementSet
    {
        private string _description; // info about these data points
        private int _elementCount; // number of wells
        private ElementType _elementType;
        private string _id; // the name of these data points
        private int[] _ids; // the well pdiv id
        private int _version; // info about these data points
        private double[] _x; // x coordinate
        private double[] _y; // y coordinate

        private XmlElementSet()
        {
        }

        #region IElementSet Members

        public string Description
        {
            get { return _description; }
        }

        public string ID
        {
            get { return _id; }
        }

        public int[] GetFaceVertexIndices(int elementIndex, int faceIndex)
        {
            return null;
        }

        public int GetFaceCount(int elementIndex)
        {
            return 0;
        }

        public ElementType ElementType
        {
            get { return _elementType; }
        }

        public double GetXCoordinate(int elementIndex, int vertexIndex)
        {
            return _x[elementIndex];
        }

        public double GetYCoordinate(int elementIndex, int vertexIndex)
        {
            return _y[elementIndex];
        }

        public double GetZCoordinate(int elementIndex, int vertexIndex)
        {
            return 0;
        }

        public int Version
        {
            get { return _version; }
        }

        public int ElementCount
        {
            get { return _elementCount; }
        }

        public int GetVertexCount(int elementIndex)
        {
            return 1;
        }

        public ISpatialReference SpatialReference
        {
            get { return new SpatialReference("no reference"); }
        }

        public int GetElementIndex(string elementID)
        {
            for (var i = 0; i < _elementCount; i++)
                if (_ids[i].ToString() == elementID)
                    return i;
            throw new Exception("Unable to find index for elementID [" + elementID + "]");
        }

        public string GetElementID(int elementIndex)
        {
            return _ids[elementIndex].ToString();
        }

        #endregion

        public static List<XmlElementSet> Read(String filename)
        {
            var elementSetList = new List<XmlElementSet>();

            // read the xml document
            var doc = new XmlDocument();
            doc.Load(filename);

            // get the list of element sets
            var elementSets = doc.SelectNodes("/ElementSets");

            // load each element set
            foreach (XmlNode node in elementSets)
            {
                var elementSet = new XmlElementSet();
                elementSetList.Add(elementSet);

                // get the element set node
                var elementSetNode = node.SelectSingleNode("ElementSet");

                // read the properties
                elementSet._id = elementSetNode.SelectSingleNode("ID").FirstChild.Value;
                elementSet._description = elementSetNode.SelectSingleNode("Description").FirstChild.Value;
                elementSet._version = Convert.ToInt32(elementSetNode.SelectSingleNode("Version").FirstChild.Value);
                elementSet._elementType = getElementType(elementSetNode.SelectSingleNode("Kind").FirstChild.Value);

                // read the elements
                var elementNodes = elementSetNode.SelectNodes("Elements/Element");

                // create the arrays to hold the elements
                elementSet._elementCount = elementNodes.Count;
                var ids = new List<int>();
                var xs = new List<double>();
                var ys = new List<double>();

                // read each element
                foreach (XmlNode elementNode in elementNodes)
                {
                    var idString = elementNode.SelectSingleNode("ID").FirstChild.Value;
                    var xString = elementNode.SelectSingleNode("X").FirstChild.Value;
                    var yString = elementNode.SelectSingleNode("Y").FirstChild.Value;

                    ids.Add(Convert.ToInt32(idString));
                    xs.Add(Double.Parse(xString));
                    ys.Add(Double.Parse(yString));
                }

                // set the arrays
                elementSet._ids = ids.ToArray();
                elementSet._x = xs.ToArray();
                elementSet._y = ys.ToArray();
            }

            return elementSetList;
        }

        private static ElementType getElementType(String value)
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
    }
}