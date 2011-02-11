using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
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
        #region Constructors

        /// <summary>
        /// Generic constructor for use with serialization.
        /// </summary>
        public FamilyPlacemark()
            : base()
        {
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

            this.Name = f.FamilyName;
            this.Unique = f.FamilyID.ToString();
            this.PinImage = (String.IsNullOrEmpty(f.FamilyHead.MemberStatus.Qualifier) ? "pin_grey.png" : f.FamilyHead.MemberStatus.Qualifier);
            this.Latitude = f.FamilyHead.PrimaryAddress.Latitude;
            this.Longitude = f.FamilyHead.PrimaryAddress.Longitude;
        }

        #endregion
    }
}
