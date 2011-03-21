using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    /// <summary>
    /// This class defines a group of placemarks that will be loaded and placed on a
    /// GoogleMap when the page is rendered. It uses an Arena Cluster ID to locate the
    /// population.
    /// The ClusterLoader only supports Individual population types.
    /// </summary>
    [Serializable]
    public class ClusterLoader : PlacemarkLoader
    {
        #region Properties

        /// <summary>
        /// The ID of the Arena Cluster to use when populating.
        /// </summary>
        public Int32 ClusterID;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor, create an empty loader.
        /// </summary>
        public ClusterLoader()
            : base()
        {
            this.ClusterID = -1;
            this.PopulateWith = PopulationType.Individuals;
        }


        /// <summary>
        /// Create a new ClusterLoader with the given cluster ID.
        /// </summary>
        /// <param name="clusterid">The ID number of the cluster to load from.</param>
        public ClusterLoader(Int32 clusterid)
            : this()
        {
            this.ClusterID = clusterid;
            this.PopulateWith = PopulationType.Individuals;
        }

        #endregion


        #region Loader methods

        /// <summary>
        /// Load all the placemark objects that are active members of the cluster.
        /// </summary>
        /// <param name="google">The Google helper class that does the database leg-work.</param>
        /// <returns>A collection of Placemark objects inside the cluster.</returns>
        public override List<Placemark> LoadPlacemarks(Google google)
        {
            List<Placemark> items = new List<Placemark>();


            if (PopulateWith == PopulationType.Individuals)
            {
                foreach (Placemark p in google.PersonPlacemarksInCluster(ClusterID, 0, Int32.MaxValue))
                {
                    items.Add(p);
                }
            }
            else if (PopulateWith == PopulationType.SmallGroups)
            {
                foreach (Placemark p in google.SmallGroupPlacemarksInCluster(ClusterID, 0, Int32.MaxValue))
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
                // This ClusterLoader is loading individuals.
                //
                return "        " + javascriptObject + ".LoadPeopleInCluster(" + ClusterID.ToString() + ",null);\n";
            }
            else if (PopulateWith == PopulationType.SmallGroups)
            {
                //
                // This ClusterLoader is loading small groups.
                //
                return "        " + javascriptObject + ".LoadGroupsInCluster(" + ClusterID.ToString() + ",null);\n";
            }
            else
                return "";
        }

        #endregion
    }
}
