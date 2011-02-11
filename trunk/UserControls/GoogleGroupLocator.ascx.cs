﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Organization;
using Arena.Portal;
using Arena.SmallGroup;
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
    public partial class GoogleGroupLocator : PortalControl
    {
        public enum FilterOptions
        {
            None = 0,
            MeetingDay = 1,
            Topic = 2,
            MaritalPreference = 3,
            AgeRange = 4,
            Type = 5,
            Campus = 6,
            Area = 7
        }

        #region Module Settings

        [NumericSetting("Map Width", "The width in pixels to make the Google Map. Default is 480.", false)]
        public int MapWidthSetting { get { return Convert.ToInt32(Setting("MapWidth", "480", false)); } }

        [NumericSetting("Map Height", "The height in pixels to make the Google Map. Default is 360.", false)]
        public int MapHeightSetting { get { return Convert.ToInt32(Setting("MapHeight", "360", false)); } }

        [CustomListSetting("Filter Options", "The list of filter options to provide the user.", false, "",
            new string[] { "No Filtering", "Meeting Day", "Topic", "Marital Preference", "Age Range", "Type", "Campus", "Area" },
            new string[] { "0", "1", "2", "3", "4", "5", "6", "7" },
            ListSelectionMode.Multiple)]
        public List<FilterOptions> FilterOptionsSetting
        {
            get
            {
                if (String.IsNullOrEmpty(Setting("FilterOptions", "", false)))
                {
                    return new List<FilterOptions>(new FilterOptions[] { FilterOptions.MeetingDay, FilterOptions.Topic,
                        FilterOptions.MaritalPreference, FilterOptions.AgeRange, FilterOptions.Type,
                        FilterOptions.Campus, FilterOptions.Area });
                }

                List<FilterOptions> opts = new List<FilterOptions>();
                foreach (string s in Setting("FilterOptions", "", false).Split(new char[] { ',' }))
                {
                    if ((FilterOptions)Enum.Parse(typeof(FilterOptions), s) == FilterOptions.None)
                        return new List<FilterOptions>();

                    opts.Add((FilterOptions)Enum.Parse(typeof(FilterOptions), s));
                }

                return opts;
            }
        }

        [ClusterCategorySetting("Category ID", "The category to use for searching for a small group.", true)]
        public Category CategorySetting { get { return new Category(Convert.ToInt32(Setting("Category", "", true))); } }

        [BooleanSetting("Filter Expanded", "If set to true then the filter will be expanded and visible by default.", true, true)]
        public Boolean FilterExpandedSetting { get { return Convert.ToBoolean(Setting("FilterExpanded", "true", true)); } }

        #endregion


        #region Properties

        protected string MeetingDayCaption = "Meeting Day";
        protected string TopicCaption = "Topic";
        protected string MaritalPreferenceCaption = "Marital Preference";
        protected string AgeRangeCaption = "Age Range";
        protected string TypeCaption = "Type";
        protected string CampusCaption = "Campus";
        protected string AreaCaption = "Area";

        #endregion


        #region Event Handlers

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                map.Width = MapWidthSetting;
                map.Height = MapHeightSetting;
                map.Center.Latitude = ArenaContext.Current.Organization.Address.Latitude;
                map.Center.Longitude = ArenaContext.Current.Organization.Address.Longitude;
                AddCenterPlacemark();

                //
                // Process each small group.
                //
                foreach (Group g in LoadGroups())
                {
                    try
                    {
                        map.Placemarks.Add(new SmallGroupPlacemark(g));
                    }
                    catch { }
                }

                //
                // Hide the filter if it is not available.
                //
                if (FilterOptionsSetting.Count == 0)
                    pnlFilter.Visible = false;
                hfFilterVisible.Value = (FilterExpandedSetting == true ? "1" : "0");

                SetupCaptions();
                SetupFilters();
            }

            //
            // Generate the start of the script needed to populate the map.
            //
            if (pnlFilter.Visible == true)
            {
                StringBuilder script = new StringBuilder();

                if (hfFilterVisible.Value == "1")
                {
                    divFilter.Style.Remove("display");
                    toggleFilter.InnerText = "Hide Filter";
                }
                script.AppendLine("<script language=\"javascript\" type=\"text/javascript\">");
                script.AppendLine("$(document).ready(function() {");
                script.AppendLine("  $('#" + toggleFilter.ClientID + "').click(function() {");
                script.AppendLine("    if ($('#" + divFilter.ClientID + "').css('display') != 'none') {");
                script.AppendLine("      $(this).text('Show Filter');");
                script.AppendLine("      $('#" + hfFilterVisible.ClientID + "').val('0');");
                script.AppendLine("    }");
                script.AppendLine("    else {");
                script.AppendLine("      $(this).text('Hide Filter');");
                script.AppendLine("      $('#" + hfFilterVisible.ClientID + "').val('1');");
                script.AppendLine("    }");
                script.AppendLine("    $('#" + divFilter.ClientID + "').slideToggle('fast');");
                script.AppendLine("  });");
                script.AppendLine("});");
                script.AppendLine("</script>");
                Page.ClientScript.RegisterStartupScript(typeof(Page), this.ClientID, script.ToString());
            }
        }


        /// <summary>
        /// Center the map on the address entered.
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        public void btnCenter_Click(object sender, EventArgs e)
        {
            Address address;


            //
            // Center the map either on the supplied address or on the churches location.
            //
            pAddressError.Visible = false;
            if (txtAddress.Text != "" || txtCity.Text != "" || txtState.Text != "" || txtPostal.Text != "")
            {
                //
                // Try to geocode the address.
                //
                address = new Address();
                address.StreetLine1 = txtAddress.Text;
                address.City = txtCity.Text;
                address.State = txtState.Text;
                address.PostalCode = txtPostal.Text;
                address.Geocode("Google Group Locator");

                //
                // If we were able to geocode the address then switch the map center to that.
                //
                if (address.Latitude != 0 && address.Longitude != 0)
                {
                    map.Center.Latitude = address.Latitude;
                    map.Center.Longitude = address.Longitude;
                }
                else
                    pAddressError.Visible = true;
            }
            else
            {
                map.Center.Latitude = ArenaContext.Current.Organization.Address.Latitude;
                map.Center.Longitude = ArenaContext.Current.Organization.Address.Longitude;
            }

            AddCenterPlacemark();
        }


        /// <summary>
        /// Apply the filter data to the list of groups.
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        public void btnFilter_Click(object sender, EventArgs e)
        {
            map.ClearContent();
            AddCenterPlacemark();

            //
            // Process each small group and filter.
            //
            foreach (Group g in LoadGroups())
            {
                try
                {
                    if (ddlMeetingDay.SelectedValue != "-1" && g.MeetingDay.LookupID != Convert.ToInt32(ddlMeetingDay.SelectedValue))
                        continue;

                    if (ddlTopic.SelectedValue != "-1" && g.Topic.LookupID != Convert.ToInt32(ddlTopic.SelectedValue))
                        continue;

                    if (ddlMaritalPreference.SelectedValue != "-1" && g.PrimaryMaritalStatus.LookupID != Convert.ToInt32(ddlMaritalPreference.SelectedValue))
                        continue;

                    if (ddlAgeRange.SelectedValue != "-1" && g.PrimaryAge.LookupID != Convert.ToInt32(ddlAgeRange.SelectedValue))
                        continue;

                    if (ddlType.SelectedValue != "-1" && g.GroupType.LookupID != Convert.ToInt32(ddlType.SelectedValue))
                        continue;

                    if (ddlArea.SelectedValue != "-1" && g.AreaID != -1 && g.AreaID != Convert.ToInt32(ddlArea.SelectedValue))
                        continue;

                    if (ddlCampus.SelectedValue != "-1" && g.Leader.Campus.CampusId != Convert.ToInt32(ddlCampus.SelectedValue))
                        continue;

                    map.Placemarks.Add(new SmallGroupPlacemark(g));
                }
                catch { }
            }
        }

        #endregion


        #region Private Support Methods

        /// <summary>
        /// Add the center placemark on the map, if one already exists then replace
        /// it with the new location.
        /// </summary>
        private void AddCenterPlacemark()
        {
            Placemark placemark = null;


            foreach (Placemark p in map.Placemarks)
            {
                if (p.Unique == "Home")
                {
                    placemark = p;
                    break;
                }
            }

            if (placemark == null)
                placemark = new Placemark();

            placemark.Latitude = map.Center.Latitude;
            placemark.Longitude = map.Center.Longitude;
            placemark.Unique = "Home";
            placemark.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=home|FFFF00";
            placemark.Name = "";
            map.Placemarks.Add(placemark);
        }


        /// <summary>
        /// Load all possible small groups that should be displayed on the map. If no
        /// filter is active then all these groups should be displayed.
        /// </summary>
        /// <returns></returns>
        private List<Group> LoadGroups()
        {
            SqlConnection con;
            SqlDataReader rdr;
            List<Group> groups = new List<Group>();
            SqlCommand cmd;

            
            //
            // Build a SQL query to enumerate groups in these categories.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT sg.group_id FROM smgp_group AS sg" +
                "    LEFT JOIN smgp_group_cluster AS sgc ON sgc.group_cluster_id = sg.group_cluster_id" +
                "    LEFT JOIN smgp_cluster_type AS sgt ON sgt.cluster_type_id = sgc.cluster_type_id" +
                "    WHERE sgt.category_id = @CategoryID" +
                "      AND sg.is_group_private = 0";
            cmd.Parameters.Add(new SqlParameter("@CategoryID", CategorySetting.CategoryID));

            //
            // Execute the reader and process all results.
            //
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                try
                {
                    groups.Add(new Group(Convert.ToInt32(rdr[0])));
                }
                catch { }
            }
            rdr.Close();

            return groups;
        }


        /// <summary>
        /// Setup the caption strings that will be displayed to the user.
        /// </summary>
        private void SetupCaptions()
        {
            if (!String.IsNullOrEmpty(CategorySetting.MeetingDayCaption))
                MeetingDayCaption = CategorySetting.MeetingDayCaption;

            if (!String.IsNullOrEmpty(CategorySetting.TopicCaption))
                TopicCaption = CategorySetting.TopicCaption;

            if (!String.IsNullOrEmpty(CategorySetting.MaritalPreferenceCaption))
                MaritalPreferenceCaption = CategorySetting.MaritalPreferenceCaption;

            if (!String.IsNullOrEmpty(CategorySetting.AgeGroupCaption))
                AgeRangeCaption = CategorySetting.AgeGroupCaption;

            if (!String.IsNullOrEmpty(CategorySetting.TypeCaption))
                TypeCaption = CategorySetting.TypeCaption;
            
            CampusCaption = "Campus";
            AreaCaption = "Area";
        }


        /// <summary>
        /// Setup all the filter drop down list selections.
        /// </summary>
        private void SetupFilters()
        {
            //
            // Setup the Campus choices.
            //
            ddlCampus.Items.Add(new ListItem("Any", "-1"));
            foreach (Campus c in ArenaContext.Current.Organization.Campuses)
            {
                ddlCampus.Items.Add(new ListItem(c.Name, c.CampusId.ToString()));
            }
            trCampus.Visible = FilterOptionsSetting.Contains(FilterOptions.Campus);

            //
            // Setup the Meeting Day choices.
            //
            ddlMeetingDay.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.DayOfWeek))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlMeetingDay.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            trMeetingDay.Visible = FilterOptionsSetting.Contains(FilterOptions.MeetingDay);

            //
            // Setup the Topic choices.
            //
            ddlTopic.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.SmallGroupTopic))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlTopic.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            trTopic.Visible = FilterOptionsSetting.Contains(FilterOptions.Topic);
            
            //
            // Setup the Marital Preference choices.
            //
            ddlMaritalPreference.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.MaritalPreference))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlMaritalPreference.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            trMaritalPreference.Visible = FilterOptionsSetting.Contains(FilterOptions.MaritalPreference);

            //
            // Setup the Age Range choices.
            //
            ddlAgeRange.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.AgeRangePreference))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlAgeRange.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            trAgeRange.Visible = FilterOptionsSetting.Contains(FilterOptions.AgeRange);

            //
            // Setup the Type choices.
            //
            ddlType.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.SmallGroupType))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlType.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            trType.Visible = FilterOptionsSetting.Contains(FilterOptions.Type);

            //
            // Setup the Area choices.
            //
            ddlArea.Items.Add(new ListItem("Any", "-1"));
            foreach (Area a in new AreaCollection(ArenaContext.Current.Organization.OrganizationID))
            {
                ddlArea.Items.Add(new ListItem(a.Name, a.AreaID.ToString()));
            }
            trArea.Visible = FilterOptionsSetting.Contains(FilterOptions.Area);
        }

        #endregion
    }
}