using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

using Arena.Core;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines the information needed to identify a family unit on
    /// the map as a placemark.
    /// </summary>
    [Serializable]
    public class FamilyPlacemark : Placemark
    {
        #region Properties

        private Family _family = null;

        #endregion


        #region Constructors

        /// <summary>
        /// Generic constructor for Serialization.
        /// </summary>
        protected FamilyPlacemark()
            : base()
        {
            this.javascriptClassName = "FamilyPlacemark";
        }

        /// <summary>
        /// Create a new FamilyPlacemark for the given family record. If the family does not have
        /// a valid geocoded address then an exception is thrown.
        /// </summary>
        /// <param name="p">The Arena person to create a placemark for.</param>
        /// <returns>A new PersonPlacemark object.</returns>
        public FamilyPlacemark(Family f)
            : base()
        {
            if (f.FamilyHead.PrimaryAddress == null || (f.FamilyHead.PrimaryAddress.Latitude == 0 && f.FamilyHead.PrimaryAddress.Longitude == 0))
                throw new ArgumentException("Family " + f.FamilyID.ToString() + " has not been properly geocoded.");

            this.javascriptClassName = "FamilyMarker";
            this.Name = f.FamilyName;
            this.Unique = f.FamilyID.ToString();
            this.PinImage = "Images/Map/" + (String.IsNullOrEmpty(f.FamilyHead.MemberStatus.Qualifier) ? "pin_grey.png" : f.FamilyHead.MemberStatus.Qualifier);
            this.Latitude = f.FamilyHead.PrimaryAddress.Latitude;
            this.Longitude = f.FamilyHead.PrimaryAddress.Longitude;
            this._family = f;
        }

        #endregion


        #region Serialization

        protected FamilyPlacemark(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (info.GetInt32("Family") != -1)
                this._family = new Family(info.GetInt32("Family"));
        }


        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (_family == null)
                info.AddValue("Family", -1);
            else
                info.AddValue("Family", _family.FamilyID);
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
            XmlElement placemark, name, point, styleUrl, coordinates, description;
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
            // Store the description information.
            //
            description = kml.xml.CreateElement("description");
            description.InnerXml = "<![CDATA[" +
                kml.Google.FamilyDetailsPopup(_family, false, true) +
                "]]>";
            placemark.AppendChild(description);

            //
            // Create the style tag.
            //
            style = kml.RegisterPinStyle(PinImage, 0.65, null);
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
