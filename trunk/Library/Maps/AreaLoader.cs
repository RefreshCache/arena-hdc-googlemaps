using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Arena.Core;
using Arena.SmallGroup;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines a group of placemarks that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses an Arena AreaID to locate the
    /// population.
    /// </summary>
    [Serializable]
    public class AreaLoader : PlacemarkLoader
    {
        #region Properties

        /// <summary>
        /// The ID of the Arena Small Group Area to use when populating.
        /// </summary>
        public Int32 AreaID;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        public AreaLoader()
            : base()
        {
            this.AreaID = -1;
            this.PopulateWith = PopulationType.Individuals;
        }


        /// <summary>
        /// Create a new AreaLoader with the given area ID.
        /// </summary>
        /// <param name="areaid">The ID number of the small group Area to load from.</param>
        public AreaLoader(Int32 areaid)
            : this()
        {
            this.AreaID = areaid;
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are within the specified area.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects inside the radius.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInArea(AreaID, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }
            else if (PopulateWith == PopulationType.Families)
            {
                foreach (Placemark p in google.FamilyPlacemarksInArea(AreaID, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }
            else if (PopulateWith == PopulationType.SmallGroups)
            {
                foreach (Placemark p in google.SmallGroupPlacemarksInArea(AreaID, 0, Int32.MaxValue))
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
                return "        " + javascriptObject + ".LoadFamiliesInArea(" + AreaID.ToString() + ",null);\n";
            }
            else if (PopulateWith == PopulationType.Individuals)
            {
                //
                // This RadiusLoader is loading individuals.
                //
                return "        " + javascriptObject + ".LoadPeopleInArea(" + AreaID.ToString() + ",null);\n";
            }
            else if (PopulateWith == PopulationType.SmallGroups)
            {
                //
                // This RadiusLoader is loading small groups.
                //
                return "        " + javascriptObject + ".LoadGroupsInArea(" + AreaID.ToString() + ",null);\n";
            }
            else
                return "";
        }

        #endregion
    }
}
