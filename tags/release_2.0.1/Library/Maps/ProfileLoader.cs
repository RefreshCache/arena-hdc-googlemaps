using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines a group of placemarks that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses an Arena ProfileID to locate the
    /// population.
    /// The ProfileLoader only supports Individual population types.
    /// </summary>
    [Serializable]
    public class ProfileLoader : PlacemarkLoader
    {
        #region Properties

        /// <summary>
        /// The ID of the Arena Profile to use when populating.
        /// </summary>
        public Int32 ProfileID;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        public ProfileLoader()
            : base()
        {
            this.ProfileID = -1;
            this.PopulateWith = PopulationType.Individuals;
        }


        /// <summary>
        /// Create a new ProfileLoader with the given profile ID.
        /// </summary>
        /// <param name="areaid">The ID number of the profile to load from.</param>
        public ProfileLoader(Int32 profileid)
            : this()
        {
            this.ProfileID = profileid;
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are active members of the profile.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects inside the radius.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInProfile(ProfileID, 0, Int32.MaxValue))
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
            if (PopulateWith == PopulationType.Individuals)
            {
                //
                // This ProfileLoader is loading individuals.
                //
                return "        " + javascriptObject + ".LoadPeopleInProfile(" + ProfileID.ToString() + ",null);\n";
            }
            else
                return "";
        }

        #endregion
    }
}
