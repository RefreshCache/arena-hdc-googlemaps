using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using Arena.SmallGroup;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// Each SmallGroupPlacemark is used to identify a single small group location
    /// on a map.
    /// </summary>
    [Serializable]
    public class SmallGroupPlacemark : Placemark
    {
        #region Properties

        /// <summary>
        /// Identifies the Group object that was used to create this placemark.
        /// </summary>
        [ScriptIgnore]
        [XmlIgnore]
        public Group Group;

        #endregion


        #region Constructors

        /// <summary>
        /// Empty constructor for use with serialization.
        /// </summary>
        public SmallGroupPlacemark()
            : base()
        {
        }


        /// <summary>
        /// Create a new SmallGroupPlacemark for the given small group record. If the group does not have
        /// a valid geocoded address then an exception is thrown.
        /// </summary>
        /// <param name="g">The Arena Group to create a placemark for.</param>
        /// <returns>A new SmallGroupPlacemark object.</returns>
        public SmallGroupPlacemark(Group g)
            : base()
        {
            if (g.TargetLocation == null || (g.TargetLocation.Latitude == 0 && g.TargetLocation.Longitude == 0))
                throw new ArgumentException("Small group has not been properly geocoded.");

            this.Name = g.Name;
            this.Unique = g.GroupID.ToString();
            this.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=glyphish_group|4040FF|000000";
            this.Latitude = g.TargetLocation.Latitude;
            this.Longitude = g.TargetLocation.Longitude;
            this.Group = g;
        }

        #endregion
    }
}
