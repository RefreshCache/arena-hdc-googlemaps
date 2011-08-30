using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Arena.Core;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines all the information needed to translate an Arena
    /// area object into a Polygon object.
    /// </summary>
    [Serializable]
    public class AreaPolygon : Polygon, IEquatable<AreaPolygon>
    {
        #region Properties

        private Area _area;

        #endregion


        #region Constructors

        /// <summary>
        /// Create a new, empty, area polygon.
        /// </summary>
        protected AreaPolygon()
            : base()
        {
        }


        /// <summary>
        /// Create a new AreaPolygon object and populate it with information from the
        /// Area object.
        /// </summary>
        /// <param name="area">The Arena Area object which contains the coordinate information.</param>
        public AreaPolygon(Area area)
            : base()
        {
            if (area.Coordinates.Count < 2)
                throw new ArgumentException("Area must have at-least two coordinate pairs to be mapped.");

            this.Name = area.Name;
            this.Unique = area.AreaID.ToString();
            this.PolyLines = new List<LatLng>();
            foreach (AreaCoordinate ac in area.Coordinates)
            {
                this.PolyLines.Add(new LatLng(ac.Latitude, ac.Longitude));
            }
        }

        #endregion


        #region Serialization

        protected AreaPolygon(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (info.GetInt32("Area") != -1)
                this._area = new Area(info.GetInt32("Area"));
        }


        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (_area == null)
                info.AddValue("Area", -1);
            else
                info.AddValue("Area", _area.AreaID);
        }

        #endregion

        
        /// <summary>
        /// Determines if this area polygon is the same as another. Two area polygon objects
        /// are considered equal if they have the same area ID.
        /// </summary>
        /// <param name="other">The AreaPolygon object to compare this object against.</param>
        /// <returns>true if the two objects are equal, false otherwise.</returns>
        public bool Equals(AreaPolygon other)
        {
            if (this._area == null || other._area == null)
                return false;

            return (this._area.AreaID == other._area.AreaID);
        }


        /// <summary>
        /// Retrieve the Area object associated with this polygon.
        /// </summary>
        /// <returns>Returns a valid Area object or null if this area is unknown.</returns>
        public Area GetArea()
        {
            return _area;
        }
    }
}
