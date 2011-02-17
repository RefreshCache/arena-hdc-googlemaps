using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Arena.Organization;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// Identifies a church campus on the map.
    /// </summary>
    [Serializable]
    public class CampusPlacemark : Placemark
    {
        #region Properties

        private Campus _campus;

        #endregion


        #region Constructors

        /// <summary>
        /// Generic constructor for Serialization.
        /// </summary>
        protected CampusPlacemark()
            : base()
        {
            this.javascriptClassName = "GenericPlacemark";
        }

        /// <summary>
        /// Create a new CampusPlacemark from the given Campus object.
        /// </summary>
        /// <param name="c"></param>
        public CampusPlacemark(Campus c)
            : base()
        {
            if (c.Address == null || (c.Address.Latitude == 0 && c.Address.Longitude == 0))
                throw new ArgumentException("Campus " + c.Name + " has not been properly geocoded.");

            this.javascriptClassName = "GenericPlacemark";
            this.Name = c.Name;
            this.Unique = c.CampusId.ToString();
            this.PinImage = "UserControls/Custom/HDC/GoogleMaps/Images/chapel.png";
            this.Latitude = c.Address.Latitude;
            this.Longitude = c.Address.Longitude;
            this._campus = c;
        }

        #endregion


        #region Serialization

        protected CampusPlacemark(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (info.GetInt32("Campus") != -1)
                this._campus = new Campus(info.GetInt32("Campus"));
        }


        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (_campus == null)
                info.AddValue("Campus", -1);
            else
                info.AddValue("Campus", _campus.CampusId);
        }

        #endregion


        /// <summary>
        /// Generate an XML element that has all the information needed for this placemark to
        /// exist on a Google Earth KML document.
        /// </summary>
        /// <param name="kml">The KML object that will be used for generating this KML.</param>
        /// <returns>An XmlElement or null if this placemark cannot exist in KML.</returns>
        public override XmlElement KMLPlacemark(KML kml)
        {
            XmlElement placemark, name, point, styleUrl, coordinates;
            string style;


            //
            // Create the placemark tag.
            //
            placemark = kml.xml.CreateElement("Placemark");

            //
            // Create the name tag.
            //
            name = kml.xml.CreateElement("name");
            name.AppendChild(kml.xml.CreateTextNode(Name));
            placemark.AppendChild(name);

            //
            // Create the style tag.
            //
            style = kml.RegisterPinStyle(PinImage, 0.75, null);
            styleUrl = kml.xml.CreateElement("styleUrl");
            styleUrl.AppendChild(kml.xml.CreateTextNode(style));
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
