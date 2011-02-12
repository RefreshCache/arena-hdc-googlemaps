using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// Generic placemark object. Each placemark has a latitude and longitude pair that
    /// identifies exactly where it shows up on the map; it also has a pin image, name and
    /// generic unique identifier.
    /// </summary>
    [Serializable]
    public class Placemark
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

        private String _AddedHandler;

        #endregion


        #region Constructors

        /// <summary>
        /// Empty constructor for Serialization as well as to just create an empty placemark.
        /// </summary>
        public Placemark()
        {
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
    }
}
