using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Arena.Core;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace Arena.Custom.HDC.GoogleMaps
{
    /// <summary>
    /// This class defines a group of people that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses a center point and distance
    /// from that point to determine which people to place on the map.
    /// </summary>
    [Serializable]
    public class RadiusLoader : Loader
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
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are within the specified radius.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects inside the radius.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInRadius(Latitude, Longitude, Distance, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }
            else if (PopulateWith == PopulationType.Families)
            {
                foreach (Placemark p in google.FamilyPlacemarksInRadius(Latitude, Longitude, Distance, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }
            else if (PopulateWith == PopulationType.SmallGroups)
            {
                foreach (Placemark p in google.SmallGroupPlacemarksInRadius(Latitude, Longitude, Distance, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }

            return items;
        }

        /// <summary>
        /// Retrieve a javascript string representation that allows the web page to
        /// populate a GoogleMap control in the background.
        /// </summary>
        /// <param name="javascriptObject">The name of the javascript object which identifies the GoogleMap control.</param>
        /// <returns>A javascript executable string.</returns>
        public override string AjaxLoadPopulation(string javascriptObject)
        {
            if (PopulateWith == PopulationType.Families)
            {
                //
                // This RadiusLoader is loading family units.
                //
                return "        " + javascriptObject + ".LoadFamiliesInGeoRadius(" +
                    "new GeoAddress(" + Latitude.ToString() + "," + Longitude.ToString() + ")," + Distance.ToString() + ",null);\n";
            }
            else if (PopulateWith == PopulationType.Individuals)
            {
                //
                // This RadiusLoader is loading individuals.
                //
                return "        " + javascriptObject + ".LoadPeopleInGeoRadius(" +
                    "new GeoAddress(" + Latitude.ToString() + "," + Longitude.ToString() + ")," + Distance.ToString() + ",null);\n";
            }
            else if (PopulateWith == PopulationType.SmallGroups)
            {
                //
                // This RadiusLoader is loading small groups.
                //
                return "        " + javascriptObject + ".LoadGroupsInGeoRadius(" +
                    "new GeoAddress(" + Latitude.ToString() + "," + Longitude.ToString() + ")," + Distance.ToString() + ",null);\n";
            }
            else
                return "";
        }

        #endregion
    }
}
