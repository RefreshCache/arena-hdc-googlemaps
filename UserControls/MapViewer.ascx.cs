using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.List;
using Arena.Organization;
using Arena.Portal;
using Arena.SmallGroup;
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;


namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
    public partial class MapViewer : PortalControl
    {
        #region Module Settings

        [NumericSetting("Map Width", "The width of the map to use, defaults to 640 pixels.", false)]
        public int MapWidthSetting { get { return Convert.ToInt32(Setting("MapWidth", "640", false)); } }

        [NumericSetting("Map Height", "The height of the map to use, defaults to 480 pixels.", false)]
        public int MapHeightSetting { get { return Convert.ToInt32(Setting("MapHeight", "480", false)); } }

        #endregion


        #region Event Handlers

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                map.Width = MapWidthSetting;
                map.Height = MapHeightSetting;

                PopulateMap();
            }
        }


        /// <summary>
        /// User has changed the type of things to populate the map with, update.
        /// </summary>
        protected void ddlPopulateWith_SelectedIndexChanged(object sender, EventArgs e)
        {
            map.ClearContent();
            PopulateMap();
        }

        #endregion


        #region Private Populate Methods

        /// <summary>
        /// Populate the entire map from what the user wants to see.
        /// </summary>
        private void PopulateMap()
        {
            PopulateByArea();
            PopulateByProfile();
            PopulateByReport();
            PopulateByCategory();
            PopulateByCluster();
            PopulateBySmallGroup();
        }


        /// <summary>
        /// Populate the map via the area they are a member of.
        /// </summary>
        private void PopulateByArea()
        {
            if (Request.Params["populateAreaID"] != null)
            {
                if (Request.Params["populateAreaID"] == "all")
                {
                    AreaCollection ac = new AreaCollection(ArenaContext.Current.Organization.OrganizationID);

                    foreach (Area a in ac)
                    {
                        PlacemarkLoader loader = new AreaLoader(a.AreaID);

                        loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                        map.Loaders.Add(loader);
                    }
                }
                else
                {
                    foreach (String areaString in Request.Params["populateAreaID"].Split(','))
                    {
                        PlacemarkLoader loader = new AreaLoader(Convert.ToInt32(areaString));

                        loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                        map.Loaders.Add(loader);
                    }
                }
            }
        }


        /// <summary>
        /// Populate the map from the members of a profile/tag.
        /// </summary>
        private void PopulateByProfile()
        {
            if (Request.Params["populateProfileID"] != null)
            {
                foreach (String profileString in Request.Params["populateProfileID"].Split(','))
                {
                    PlacemarkLoader loader = new ProfileLoader(Convert.ToInt32(profileString));

                    loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                    map.Loaders.Add(loader);
                }
            }
        }


        /// <summary>
        /// Populate the map from a report (List) in Arena.
        /// </summary>
        private void PopulateByReport()
        {
            if (Request.Params["populateReportID"] != null)
            {
                PlacemarkLoader loader = new ReportLoader(Convert.ToInt32(Request.Params["populateReportID"]));

                loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                map.Loaders.Add(loader);
            }
        }


        /// <summary>
        /// Populate the map based upon the passed category ID.
        /// </summary>
        private void PopulateByCategory()
        {
            if (Request.Params["populateCategoryID"] != null)
            {
                PlacemarkLoader loader = new CategoryLoader(Convert.ToInt32(Request.Params["populateCategoryID"]));

                loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                map.Loaders.Add(loader);
            }
        }


        /// <summary>
        /// Populate everything based upon the passed cluster IDs.
        /// </summary>
        private void PopulateByCluster()
        {
            if (Request.Params["populateClusterID"] != null)
            {
                foreach (String clusterString in Request.Params["populateClusterID"].Split(','))
                {
                    PlacemarkLoader loader = new ClusterLoader(Convert.ToInt32(clusterString));

                    loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                    map.Loaders.Add(loader);
                }
            }
        }


        /// <summary>
        /// Populate the map by the passed small group IDs.
        /// </summary>
        private void PopulateBySmallGroup()
        {
            if (Request.Params["populateSmallGroupID"] != null)
            {
                foreach (String groupString in Request.Params["populateSmallGroupID"].Split(','))
                {
                    PlacemarkLoader loader = new GroupLoader(Convert.ToInt32(groupString));

                    loader.PopulateWith = (PopulationType)Convert.ToInt32(ddlPopulateWith.SelectedValue);
                    map.Loaders.Add(loader);
                }
            }
        }

        #endregion
    }
}