﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines a group of placemarks that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses an Arena Report (List) to locate the
    /// population.
    /// The ProfileLoader only supports Individual population types.
    /// </summary>
    [Serializable]
    public class ReportLoader : PlacemarkLoader
    {
        #region Properties

        /// <summary>
        /// The ID of the Arena Report to use when populating.
        /// </summary>
        public Int32 ReportID;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        protected ReportLoader()
            : base()
        {
            this.ReportID = -1;
            this.PopulateWith = PopulationType.Individuals;
        }


        /// <summary>
        /// Create a new ProfileLoader with the given report ID.
        /// </summary>
        /// <param name="areaid">The ID number of the report to load from.</param>
        public ReportLoader(Int32 reportid)
            : this()
        {
            this.ReportID = reportid;
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are listed in the report.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInReport(ReportID, 0, Int32.MaxValue))
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
                return "        " + javascriptObject + ".LoadPeopleInReport(" + ReportID.ToString() + ",null);\n";
            }
            else
                return "";
        }

        #endregion
    }
}
