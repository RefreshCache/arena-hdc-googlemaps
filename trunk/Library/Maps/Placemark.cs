using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Xml;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// Generic placemark object. Each placemark has a latitude and longitude pair that
    /// identifies exactly where it shows up on the map; it also has a pin image, name and
    /// generic unique identifier.
    /// </summary>
    [Serializable]
    public class Placemark : ISerializable
    {
        #region Properties

        /// <summary>
        /// The name that will be displayed for this placemark. The name is usually displayed
        /// when the mouse hovers over the placemark pin.
        /// </summary>
        public String Name;
        
        /// <summary>
        /// Unique identifier for this placemark. This identifier is normally used by the client
        /// to request more detailed information about this placemark.
        /// </summary>
        public String Unique;
        
        /// <summary>
        /// The latitude coordinate for this placemark.
        /// </summary>
        public Double Latitude;
        
        /// <summary>
        /// The longitude coordinate for this placemark.
        /// </summary>
        public Double Longitude;
        
        /// <summary>
        /// The image to use when drawing the pin for this placemark. Subclasses may handle
        /// pins differently and only use a relative URL instead of an absolute URL.
        /// </summary>
        public String PinImage;

        protected String _AddedHandler, javascriptClassName;

        #endregion


        #region Constructors

        /// <summary>
        /// Empty constructor for Serialization as well as to just create an empty placemark.
        /// </summary>
        public Placemark()
        {
            this.javascriptClassName = "GenericMarker";
            this.Name = "";
            this.Unique = "";
            this.Latitude = 0;
            this.Longitude = 0;
            this.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=|FFFF00|000000";
        }


        /// <summary>
        /// Create a new placemark with the given name, unique identifier, latitude and longitude.
        /// This method is protected because it is not meant to by used by the user, the user should
        /// generally use one of the subclasses to define their placemark.
        /// </summary>
        /// <param name="name">The name of this placemark.</param>
        /// <param name="unique">Unique identifier for this object.</param>
        /// <param name="latitude">Latitude coordinate this placemark will be placed at.</param>
        /// <param name="longitude">Longitude coordinate this placemark will be placed at.</param>
        protected Placemark(String name, String unique, double latitude, double longitude)
            : this()
        {
            this.Name = name;
            this.Unique = unique;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        #endregion


        #region Serialization

        protected Placemark(SerializationInfo info, StreamingContext context)
        {
            this.Name = info.GetString("Name");
            this.Unique = info.GetString("Unique");
            this.Latitude = info.GetDouble("Latitude");
            this.Longitude = info.GetDouble("Longitude");
            this.PinImage = info.GetString("PinImage");
            this._AddedHandler = info.GetString("AddedHandler");
            this.javascriptClassName = info.GetString("javascriptClassName");
        }


        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Unique", Unique);
            info.AddValue("Latitude", Latitude);
            info.AddValue("Longitude", Longitude);
            info.AddValue("PinImage", PinImage);
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


            sb.AppendLine(javascriptVariable + " = new " + javascriptClassName + "({" +
                "icon: \"" + PinImage + "\"" +
                ",position: new google.maps.LatLng(" + Latitude.ToString() + "," + Longitude.ToString() + ")" +
                ",map: " + javascriptObject + ".map" +
                ",title: \"" + Name.Replace("\"", "\\\"") + "\"" +
                "}, '" + Unique + "');");
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
        public virtual XmlElement KMLPlacemark(KML kml)
        {
            XmlElement placemark, name, point, styleUrl, coordinates;


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
            styleUrl = kml.xml.CreateElement("styleUrl");
            styleUrl.AppendChild(kml.xml.CreateTextNode(kml.RegisterPinStyle(PinImage, 1, null)));
            placemark.AppendChild(styleUrl);

            //
            // Set the coordinates and store the placemark.
            //
            point = kml.xml.CreateElement("Point");
            coordinates = kml.xml.CreateElement("coordinates");
            coordinates.AppendChild(kml.xml.CreateTextNode(String.Format("{0},{1},0", Longitude, Latitude)));
            point.AppendChild(coordinates);
            placemark.AppendChild(point);

            return placemark;
        }
    }
}
