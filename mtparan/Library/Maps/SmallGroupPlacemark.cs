using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

using Arena.SmallGroup;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// Each SmallGroupPlacemark is used to identify a single small group location
    /// on a map.
    /// </summary>
    [Serializable]
    public class SmallGroupPlacemark : Placemark, ISerializable, IEquatable<SmallGroupPlacemark>
    {
        #region Properties

        private Group _group;

        #endregion


        #region Constructors

        /// <summary>
        /// Empty constructor for use with serialization.
        /// </summary>
        protected SmallGroupPlacemark()
            : base()
        {
            this.javascriptClassName = "GroupMarker";
        }


        /// <summary>
        /// Create a new SmallGroupPlacemark for the given small group record. If the group does not have
        /// a valid geocoded address then an exception is thrown.
        /// </summary>
        /// <param name="g">The Arena Group to create a placemark for.</param>
        /// <returns>A new SmallGroupPlacemark object.</returns>
        public SmallGroupPlacemark(Group g)
            : this()
        {
            if (g.TargetLocation == null || (g.TargetLocation.Latitude == 0 && g.TargetLocation.Longitude == 0))
                throw new ArgumentException("Small group has not been properly geocoded.");

            this.Name = g.Name;
            this.Unique = g.GroupID.ToString();
            this.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=glyphish_group|4040FF|000000";
            this.Latitude = g.TargetLocation.Latitude;
            this.Longitude = g.TargetLocation.Longitude;
            this._group = g;
        }

        #endregion


        #region Serialization

        protected SmallGroupPlacemark(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (info.GetInt32("Group") != -1)
                this._group = new Group(info.GetInt32("Group"));
        }


        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (_group == null)
                info.AddValue("Group", -1);
            else
                info.AddValue("Group", _group.GroupID);
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
                kml.Google.SmallGroupDetailsPopup(_group, false, true) +
                "]]>";
            placemark.AppendChild(description);

            //
            // Create the style tag.
            //
            style = kml.RegisterPinStyle("http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=glyphish_group|4040FF|000000", 0.75, null);
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


        /// <summary>
        /// Determines if this small group placemark is the same as another. Two small group
        /// placemark objects are considered equal if they have the same small group ID.
        /// </summary>
        /// <param name="other">The SmallGroupPlacemark object to compare this object against.</param>
        /// <returns>true if the two objects are equal, false otherwise.</returns>
        public bool Equals(SmallGroupPlacemark other)
        {
            if (this._group == null || other._group == null)
                return false;

            return (this._group.GroupID == other._group.GroupID);
        }
    }
}
