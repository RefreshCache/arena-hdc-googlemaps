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
    /// This class defines the information needed to put a single individual person
    /// on the map as a placemark object.
    /// </summary>
    [Serializable]
    public class PersonPlacemark : Placemark, IEquatable<PersonPlacemark>
    {
        #region Properties

        private Person _person;

        #endregion


        #region Constructors

        /// <summary>
        /// Generic constructor for Serialization.
        /// </summary>
        protected PersonPlacemark()
            : base()
        {
            this.javascriptClassName = "PersonPlacemark";
        }


        /// <summary>
        /// Create a new PersonPlacemark for the given person record. If the person does not have
        /// a valid geocoded address then an exception is thrown.
        /// </summary>
        /// <param name="p">The Arena person to create a placemark for.</param>
        /// <returns>A new PersonPlacemark object.</returns>
        public PersonPlacemark(Person p)
            : base()
        {
            if (p.PrimaryAddress == null || (p.PrimaryAddress.Latitude == 0 && p.PrimaryAddress.Longitude == 0))
                throw new ArgumentException("Person has not been properly geocoded.");

            this.javascriptClassName = "PersonMarker";
            this.Name = p.FullName;
            this.Unique = p.PersonGUID.ToString();
            this.PinImage = "Images/Map/" + (String.IsNullOrEmpty(p.MemberStatus.Qualifier) ? "pin_grey.png" : p.MemberStatus.Qualifier);
            this.Latitude = p.PrimaryAddress.Latitude;
            this.Longitude = p.PrimaryAddress.Longitude;

            this._person = p;
        }
        #endregion


        #region Serialization

        protected PersonPlacemark(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (info.GetInt32("Person") != -1)
                this._person = new Person(info.GetInt32("Person"));
        }


        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (_person == null)
                info.AddValue("Person", -1);
            else
                info.AddValue("Person", _person.PersonID);
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
                kml.Google.PersonDetailsPopup(_person, false, true) +
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


        /// <summary>
        /// Determines if this person placemark is the same as another. Two person placemark objects
        /// are considered equal if they have the same person ID.
        /// </summary>
        /// <param name="other">The PersonPlacemark object to compare this object against.</param>
        /// <returns>true if the two objects are equal, false otherwise.</returns>
        public bool Equals(PersonPlacemark other)
        {
            if (this._person == null || other._person == null)
                return false;

            return (this._person.PersonID == other._person.PersonID);
        }
    }
}
