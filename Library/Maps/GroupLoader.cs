using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines a group of placemarks that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses an Arena Group ID to locate the
    /// population.
    /// The GroupLoader only supports Individual population types.
    /// </summary>
    [Serializable]
    public class GroupLoader : PlacemarkLoader
    {
        #region Properties

        /// <summary>
        /// The ID of the Arena Group to use when populating.
        /// </summary>
        public Int32 GroupID;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        public GroupLoader()
            : base()
        {
            this.GroupID = -1;
            this.PopulateWith = PopulationType.Individuals;
        }


        /// <summary>
        /// Create a new GroupLoader with the given group ID.
        /// </summary>
        /// <param name="groupid">The ID number of the group to load from.</param>
        public GroupLoader(Int32 groupid)
            : this()
        {
            this.GroupID = groupid;
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are active members of the group.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects inside the group.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInGroup(GroupID, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }
            else
                throw new NotSupportedException();

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
                // This CategoryLoader is loading individuals.
                //
                return "        " + javascriptObject + ".LoadPeopleInGroup(" + GroupID.ToString() + ",null);\n";
            }
            else
                throw new NotSupportedException();
        }

        #endregion
    }
}
