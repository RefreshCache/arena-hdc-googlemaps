using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Portal;
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;


namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
    public partial class AreaPicker : PortalControl
    {
        #region Module Settings

        [NumericSetting("Map Width", "The width of the map to use, defaults to 640 pixels.", false)]
        public int MapWidthSetting { get { return Convert.ToInt32(Setting("MapWidth", "640", false)); } }

        [NumericSetting("Map Height", "The height of the map to use, defaults to 480 pixels.", false)]
        public int MapHeightSetting { get { return Convert.ToInt32(Setting("MapHeight", "480", false)); } }

        [PageSetting("Small Group Locator Page", "The page to redirect to when an area is clicked.", true)]
        public int SmallGroupLocatorPageSetting { get { return Convert.ToInt32(Setting("SmallGroupLocatorPage", "", true)); } }

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

        #endregion


        #region Private Populate Methods

        /// <summary>
        /// Populate the entire map from what the user wants to see.
        /// </summary>
        private void PopulateMap()
        {
            AreaCollection ac = new AreaCollection(ArenaContext.Current.Organization.OrganizationID);
            AreaPolygon poly;
            Google google = new Google(ArenaContext.Current.User, map.BaseUrl());
            Double left = 0, right = 0, top = 0, bottom = 0;

            foreach (Area a in ac)
            {
                if (a.Coordinates.Count >= 2)
                {
                    //
                    // Pre-scan the coordinates to be used later for centering the map.
                    //
                    foreach (AreaCoordinate coord in a.Coordinates)
                    {
                        if (left == 0 || coord.Longitude < left)
                            left = coord.Longitude;
                        if (right == 0 || coord.Longitude > right)
                            right = coord.Longitude;
                        if (bottom == 0 || coord.Latitude < bottom)
                            bottom = coord.Latitude;
                        if (top == 0 || coord.Latitude > top)
                            top = coord.Latitude;
                    }

                    //
                    // Create the area and use the next available fill color.
                    //
                    poly = new AreaPolygon(a);
                    poly.FillColor = google.NextAreaColor();
                    poly.SetAddedHandler("AP_PolygonAdded");
                    map.Polygons.Add(poly);
                }
            }

            //
            // Center the map in the areas shown.
            //
            map.Center.Latitude = (bottom / 2) + (top / 2);
            map.Center.Longitude = (right / 2) + (left / 2);

            //
            // Auto-zoom the map.
            //
            String script = "<script language=\"javascript\" type=\"text/javascript\">" + 
                "$(document).bind('GoogleMap_Ready', function(event, map) {" +
                "map.map.fitBounds(new google.maps.LatLngBounds(new google.maps.LatLng(" + bottom.ToString() + "," + left.ToString() + "), new google.maps.LatLng(" + top.ToString() + "," + right.ToString() + ")));" +
                "});" +
                "</script>";
            Page.ClientScript.RegisterClientScriptBlock(typeof(Page), "autozoom", script);
        }

        #endregion
    }
}