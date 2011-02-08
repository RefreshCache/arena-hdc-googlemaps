using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps
{
    /// <summary>
    /// This class defines a group of people that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses a center point and distance
    /// from that point to determine which people to place on the map.
    /// </summary>
    [Serializable]
    public class RadiusLoader
    {
        #region Properties

        /// <summary>
        /// The latitude of the center point to use.
        /// </summary>
        public Double Latitude;

        /// <summary>
        /// The longitude of the center point to use.
        /// </summary>
        public Double Longitude;

        /// <summary>
        /// The distance, in miles, from the center point that people should be
        /// loaded.
        /// </summary>
        public Double Distance;

        /// <summary>
        /// The type of things to load.
        /// </summary>
        public RadiusLoaderType LoaderType;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        public RadiusLoader()
        {
            this.Latitude = 0;
            this.Longitude = 0;
            this.Distance = 0;
            this.LoaderType = RadiusLoaderType.Individuals;
        }

        #endregion
    }


    /// <summary>
    /// Defines the type of objects that will be loaded in a RadiusLoader.
    /// </summary>
    public enum RadiusLoaderType
    {
        /// <summary>
        /// Load individual people records onto the map.
        /// </summary>
        Individuals = 0,

        /// <summary>
        /// Loads entire family units onto the map.
        /// </summary>
        Families = 1,

        /// <summary>
        /// Load small groups onto the map.
        /// </summary>
        SmallGroups = 2
    }
}
