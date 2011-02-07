using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Arena.Core;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    public class FamilyPlacemark : Placemark
    {
        public String PinImage;


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
    }
}
