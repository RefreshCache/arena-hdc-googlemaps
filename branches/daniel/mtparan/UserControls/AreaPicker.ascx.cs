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

            foreach (Area a in ac)
            {
                map.Polygons.Add(new AreaPolygon(a));
            }

            //
            // TODO: Center the map automatically. Either come up with a way to auto
            // zoom the map to the appropriate zoom level or provide a module setting
            // to let the admin do so.
            //
        }

        #endregion
    }
}