using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Xml;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// The polygon class will draw a polygon overlay on the map area. Each polygon
    /// must have at-least 3 lines to form a valid area. The end-point of the last
    /// line should equal the start-point of the first line.
    /// </summary>
    [Serializable]
    public class Polygon : ISerializable
    {
        #region Properties

        /// <summary>
        /// The name for a polygon is only used when exporting to KML.
        /// </summary>
        public String Name = "";

        /// <summary>
        /// The color to fill the polygon with. The color should be in a hex-string
        /// format following the style AABBGGRR.
        /// </summary>
        public String FillColor = "800000ff";

        /// <summary>
        /// The color to stroke the outside of the polygon with. If this value is not
        /// set then no stroke is applied. The color should be in a hex-string format
        /// following the style AABBGGRR
        /// </summary>
        public String StrokeColor = null;

        /// <summary>
        /// The width of the line to use when stroking the polygon. Defaults to 0 which
        /// means no stroke will be applied.
        /// </summary>
        public int StrokeWidth = 0;
        
        /// <summary>
        /// Unique identifier for this placemark. This identifier is normally used by the client
        /// to request more detailed information about this placemark.
        /// </summary>
        public String Unique = "";

        /// <summary>
        /// The lines that make up this polygon.
        /// </summary>
        List<LatLng> PolyLines = null;
        
        protected String _AddedHandler, javascriptClassName;

        #endregion


        #region Constructors

        /// <summary>
        /// Empty constructor for Serialization as well as to just create an empty placemark.
        /// </summary>
        public Polygon()
        {
            this.javascriptClassName = "GenericPolygon";
        }


        /// <summary>
        /// Create a new placemark with the given name, unique identifier, latitude and longitude.
        /// This method is protected because it is not meant to by used by the user, the user should
        /// generally use one of the subclasses to define their placemark.
        /// </summary>
        /// <param name="name">The name of this polygon area.</param>
        /// <param name="unique">Unique identifier for this object.</param>
        /// <param name="lines">A list of LatLng structs to identify the lines for this polygon.</param>
        protected Polygon(String name, String unique, List<LatLng> lines)
            : this()
        {
            this.Name = name;
            this.Unique = unique;
            this.PolyLines = lines;
        }

        #endregion


        #region Serialization

        protected Polygon(SerializationInfo info, StreamingContext context)
        {
            this.Name = info.GetString("Name");
            this.Unique = info.GetString("Unique");
            this.FillColor = info.GetString("FillColor");
            this.StrokeColor = info.GetString("StrokeColor");
            this.StrokeWidth = info.GetInt32("StrokeWidth");
            this.PolyLines = (List<LatLng>)info.GetValue("PolyLines", typeof(List<LatLng>));
            this._AddedHandler = info.GetString("AddedHandler");
            this.javascriptClassName = info.GetString("javascriptClassName");
        }


        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Unique", Unique);
            info.AddValue("FillColor", FillColor);
            info.AddValue("StrokeColor", StrokeColor);
            info.AddValue("StrokeWidth", StrokeWidth);
            info.AddValue("PolyLines", PolyLines);
            info.AddValue("AddedHandler", _AddedHandler);
            info.AddValue("javascriptClassName", javascriptClassName);
        }


        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }

        #endregion


        /// <summary>
        /// Sets the Javascript method to call when the placemark is added
        /// to the map. This is only used when the placemark is being added
        /// to the map manually via the Placemarks property.
        /// </summary>
        /// <param name="handler">The javascript function name to call, only include the function name not any parenthesis. It will be called with 2 parameters, the GoogleMap reference and a reference to the marker itself.</param>
        public void SetAddedHandler(String handler)
        {
            _AddedHandler = handler;
        }


        /// <summary>
        /// Retrieve the javascript function to call when the marker is added to the map.
        /// </summary>
        /// <returns>A string identifying the javascript function.</returns>
        public String GetAddedHandler()
        {
            return _AddedHandler;
        }


        /// <summary>
        /// Generates the Javascript code needed to create this placemark directly in
        /// the code as opposed to dynamically loading it via an AJAX request.
        /// </summary>
        /// <param name="javascriptObject">The ClientObject of the GoogleMap control to put this placemark on.</param>
        /// <param name="javascriptVariable">The javascript variable name to use when creating this placemark.</param>
        /// <returns>A string of javascript code that can be executed by the web browser.</returns>
        public string JavascriptCode(string javascriptObject, string javascriptVariable)
        {
            StringBuilder sb = new StringBuilder();


            sb.Append(javascriptVariable + " = new " + javascriptClassName + "({" +
                "fillColor: \"#" + FillColor.Substring(2) + "\"" +
                ",fillOpacity: " + Math.Round(((Double)Convert.ToInt32(FillColor.Substring(0, 2), 16) / 255.0f), 2) +
                ",map: " + javascriptObject + ".map"
                );

            //
            // Add the options for the stroke color (if it is turned on).
            //
            if (StrokeWidth > 0 && StrokeColor != null)
            {
                sb.Append(
                    ",strokeColor: \"#" + StrokeColor.Substring(2) + "\"" +
                    ",strokeOpacity: " + Math.Round(((Double)Convert.ToInt32(StrokeColor.Substring(0, 2), 16) / 255.0f), 2) +
                    ",strokeWeight: " + StrokeWidth
                    );
            }

            //
            // Add the options for the poly lines.
            //
            sb.Append(",paths: Array(");
            for (int i = 0; i < PolyLines.Count; i++)
            {
                LatLng latlng = PolyLines[i];

                if (i > 0)
                    sb.Append(",");
                sb.Append("new google.maps.LatLng(" + latlng.Latitude.ToString() + "," + latlng.Longitude.ToString() + ")");
            }
            sb.Append(")");

            sb.AppendLine("}, '" + Unique + "');");
            if (!String.IsNullOrEmpty(_AddedHandler))
                sb.AppendLine(GetAddedHandler() + "(" + javascriptObject + "," + javascriptVariable + ");");

            return sb.ToString();
        }


        /// <summary>
        /// Generate an XML element that has all the information needed for this placemark to
        /// exist on a Google Earth KML document.
        /// </summary>
        /// <param name="kml">The KML object that will be used for generating this KML.</param>
        /// <returns>An XmlElement or null if this placemark cannot exist in KML.</returns>
        public virtual XmlElement KMLPolygon(KML kml)
        {
            XmlElement placemark, polygon, name, style, linestyle, polystyle, value;
            StringBuilder sb = new StringBuilder();


            //
            // Create the placemark tag.
            //
            placemark = kml.xml.CreateElement("Placemark");

            //
            // Create the name tag.
            //
            name = kml.xml.CreateElement("name");
            name.AppendChild(kml.xml.CreateTextNode(Name.Replace("\\n", ", ")));
            placemark.AppendChild(name);

            //
            // Create the style tag.
            //
            style = kml.xml.CreateElement("Style");
            polystyle = kml.xml.CreateElement("PolyStyle");
            style.AppendChild(polystyle);
            
            value = kml.xml.CreateElement("color");
            value.AppendChild(kml.xml.CreateTextNode(FillColor));
            polystyle.AppendChild(value);
 
            if (StrokeColor != null && StrokeWidth > 0)
            {
                linestyle = kml.xml.CreateElement("LineStyle");
                style.AppendChild(linestyle);

                value = kml.xml.CreateElement("color");
                value.AppendChild(kml.xml.CreateTextNode(StrokeColor));
                polystyle.AppendChild(value);
                
                value = kml.xml.CreateElement("width");
                value.AppendChild(kml.xml.CreateTextNode(StrokeWidth.ToString()));
                polystyle.AppendChild(value);
            }
            placemark.AppendChild(style);

            //
            // Set the coordinates for the polygon.
            //
            foreach (LatLng latlng in PolyLines)
            {
                sb.AppendFormat("{0},{1},0 ", latlng.Longitude.ToString(), latlng.Latitude,ToString());
            }
            polygon = kml.xml.CreateElement("Polygon");
            polygon.InnerXml = "<tessellate>1</tessellate><outerBoundaryIs><LinearRing><coordinates>" + sb.ToString() + "</coordinates></LinearRing></outerBoundaryIs>";
            placemark.AppendChild(polygon);

            return placemark;
        }
    }


    public struct LatLng
    {
        public Double Latitude;
        public Double Longitude;

        public LatLng(Double lat, Double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }
}
