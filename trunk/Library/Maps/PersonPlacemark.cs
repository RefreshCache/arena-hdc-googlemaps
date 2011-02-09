﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using Arena.Core;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines the information needed to put a single individual person
    /// on the map as a placemark object.
    /// </summary>
    [Serializable]
    public class PersonPlacemark : Placemark
    {
        #region Properties

        /// <summary>
        /// Identifies the Person object used to create this placemark.
        /// </summary>
        [ScriptIgnore]
        [XmlIgnore]
        public Person Person;

        #endregion


        #region Constructors

        /// <summary>
        /// Empty constructor for use with serialization.
        /// </summary>
        public PersonPlacemark()
            : base()
        {
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

            this.Name = p.FullName;
            this.Unique = p.PersonGUID.ToString();
            this.PinImage = (String.IsNullOrEmpty(p.MemberStatus.Qualifier) ? "pin_grey.png" : p.MemberStatus.Qualifier);
            this.Latitude = p.PrimaryAddress.Latitude;
            this.Longitude = p.PrimaryAddress.Longitude;
            this.Person = p;
        }

        #endregion
    }
}
