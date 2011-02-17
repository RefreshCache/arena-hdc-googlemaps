using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Arena.Core;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace Arena.Custom.HDC.GoogleMaps
{
    /// <summary>
    /// Provides an abstract layer of functionality that can be used to
    /// load placemarks of any kind onto a map.
    /// </summary>
    [Serializable]
    public abstract class Loader
    {
        #region Properties

        /// <summary>
        /// The type of things to load.
        /// </summary>
        public PopulationType PopulateWith;

        #endregion


        #region Abstracts

        /// <summary>
        /// This method must be overridden by subclasses. The LoadPopulation method is called
        /// to signal that the loader should return a list of all placemark objects that should
        /// go onto the map.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A List of Placemark objects (or it's subclasses).</returns>
        public abstract List<Placemark> LoadPlacemarks(Google google);

        /// <summary>
        /// Returns a Javascript string that will use the GoogleMap JS API to populate the map
        /// in the background. The passed parameter should be the ClientObject of the GoogleMap
        /// web control.
        /// </summary>
        /// <returns>A Javascript string used to populate the map.</returns>
        public abstract String AjaxLoadPopulation(string javascriptObject);

        #endregion
    }


    /// <summary>
    /// Defines the type of objects that will be loaded in a RadiusLoader.
    /// </summary>
    public enum PopulationType
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
