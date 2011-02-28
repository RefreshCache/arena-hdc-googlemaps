using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines a group of placemarks that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses an Arena Category ID to locate the
    /// population.
    /// The CategoryLoader only supports Individual population types.
    /// </summary>
    [Serializable]
    public class CategoryLoader : PlacemarkLoader
    {
        #region Properties

        /// <summary>
        /// The ID of the Arena Category to use when populating.
        /// </summary>
        public Int32 CategoryID;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        public CategoryLoader()
            : base()
        {
            this.CategoryID = -1;
            this.PopulateWith = PopulationType.Individuals;
        }


        /// <summary>
        /// Create a new CategoryLoader with the given category ID.
        /// </summary>
        /// <param name="categoryid">The ID number of the category to load from.</param>
        public CategoryLoader(Int32 categoryid)
            : this()
        {
            this.CategoryID = categoryid;
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are active members of the category.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects inside the radius.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInCategory(CategoryID, 0, Int32.MaxValue))
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
                return "        " + javascriptObject + ".LoadPeopleInCategory(" + CategoryID.ToString() + ",null);\n";
            }
            else
                throw new NotSupportedException();
        }

        #endregion
    }
}
