using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

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

        [BooleanSetting("Show Campuses", "Shows the campus locations on the map.", false, true)]
        public Boolean ShowCampusesSetting { get { return Convert.ToBoolean(Setting("ShowCampuses", "true", false)); } }

        [NumericSetting("Map Width", "The width in pixels to make the map. Default is 480.", false)]
        public int MapWidthSetting { get { return Convert.ToInt32(Setting("MapWidth", "480", false)); } }

        [NumericSetting("Map Height", "The height in pixels to make the map. Default is 360.", false)]
        public int MapHeightSetting { get { return Convert.ToInt32(Setting("MapHeight", "360", false)); } }

        [CustomListSetting("Filter Options", "The list of filter options to provide the user. If no options are selected then all filters will be provided.", false, "",
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

        [PageSetting("Registration Page", "The page to redirect to when somebody wishes to join a small group.", true)]
        public int RegistrationPageSetting { get { return Convert.ToInt32(Setting("RegistrationPage", "", true)); } }

        [BooleanSetting("Address On Top", "Show the center-on-address fields above the map instead of below.", false, false)]
        public Boolean AddressOnTopSetting { get { return Convert.ToBoolean(Setting("AddressOnTop", "false", false)); } }

        [BooleanSetting("Filter On Top", "Show the filter fields above the map instead of below.", false, false)]
        public Boolean FilterOnTopSetting { get { return Convert.ToBoolean(Setting("FilterOnTop", "false", false)); } }

        [ClusterTypeSetting("Limit To Cluster Type", "Limit the results to the selected cluster type, as defined in the small group structure.", false)]
        public int LimitToClusterTypeSetting { get { return Convert.ToInt32(Setting("LimitToClusterType", "-1", false)); } }

        [BooleanSetting("Show List Results", "Shows the results in list-view below the map.", false, false)]
        public Boolean ShowListResultsSetting { get { return Convert.ToBoolean(Setting("ShowListResults", "false", false)); } }

        [CssSetting("Style Css", "If you wish to customize the styling you can duplicate the grouplocator.css file and enter the path to it here. Defaults to UserControls/Custom/HDC/GoogleMaps/Includes/grouplocator.css", false)]
        public String StyleCssSetting { get { return Setting("StyleCss", "UserControls/Custom/HDC/GoogleMaps/Includes/grouplocator.css", false); } }

        [FileSetting("Xslt File", "The list results uses an XSLT file to display the data. Defaults to UserControls/Custom/HDC/GoogleMaps/Includes/grouplocator.xslt.", false)]
        public String XsltFileSetting { get { return Setting("XsltFile", "UserControls/Custom/HDC/GoogleMaps/Includes/grouplocator.xslt", false); } }

        [BooleanSetting("Map Visible", "Wether or not to display a single common map.", false, true)]
        public Boolean MapVisibleSetting { get { return Convert.ToBoolean(Setting("MapVisible", "true", false)); } }

        [FileSetting("Info Xslt File", "The small group popup information window uses an XSLT file to format the data displayed. Defaults to UserControls/Custom/HDC/GoogleMaps/Includes/groupinfo.xslt.", false)]
        public String InfoXsltFileSetting { get { return Setting("InfoXsltFile", "UserControls/Custom/HDC/GoogleMaps/Includes/groupinfo.xslt", false); } }

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
            BasePage.AddCssLink(Page, StyleCssSetting);


            //
            // If the page is requesting the info to be displayed in the small group popup info window
            // then send it.
            //
            if (!String.IsNullOrEmpty(Request.Params["info_group_id"]))
            {
                SmallGroupInfo(Convert.ToInt32(Request.Params["info_group_id"]));
                return;
            }

            if (!IsPostBack)
            {
                map.Width = MapWidthSetting;
                map.Height = MapHeightSetting;
                map.Center.Latitude = ArenaContext.Current.Organization.Address.Latitude;
                map.Center.Longitude = ArenaContext.Current.Organization.Address.Longitude;
                AddCampusPlacemarks();

                //
                // Hide the filter if it is not available.
                //
                if (FilterOptionsSetting.Count == 0)
                    pnlFilter.Visible = false;
                hfFilterVisible.Value = (FilterExpandedSetting == true ? "1" : "0");

                //
                // Hide the list results if they don't want them shown.
                //
                if (ShowListResultsSetting == false)
                    pnlListResults.Visible = false;

                //
                // Hide the map if they don't want it.
                //
                if (MapVisibleSetting == false)
                {
                    map.Visible = false;
                    btnUpdate.Text = "Nearby Groups";
                }

                ViewState["has_distance"] = 0;

                SetupCaptions();
                SetupFilters();

                //
                // Set the default area selection.
                //
                if (!String.IsNullOrEmpty(Request.QueryString["area"]))
                    ddlArea.SelectedValue = Request.QueryString["area"];

                //
                // Show the groups that have been selected by filter.
                //
                AddFilteredGroups();
            }

            //
            // Generate the start of the script needed to populate the map.
            //
            StringBuilder script = new StringBuilder();
            
            script.AppendLine("<script language=\"javascript\" type=\"text/javascript\">");

            //
            // Put the address on top if that is what they want.
            //
            if (AddressOnTopSetting)
            {
                script.AppendLine("$(document).ready(function() {");
                script.AppendLine("    $('#" + pnlAddress.ClientID + "').insertBefore('#" + map.ClientID + "');");
                script.AppendLine("});");
            }

            //
            // Put the filter on top if that is what they want.
            //
            if (FilterOnTopSetting)
            {
                script.AppendLine("$(document).ready(function() {");
                script.AppendLine("    $('#" + pnlFilter.ClientID + "').insertBefore('#" + map.ClientID + "');");
                script.AppendLine("});");
            }

            if (pnlFilter.Visible == true)
            {
                if (hfFilterVisible.Value == "1")
                {
                    divFilter.Style.Remove("display");
                    toggleFilter.InnerText = "Hide Filter";
                }
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
            }

            script.AppendLine("</script>");
            Page.ClientScript.RegisterStartupScript(typeof(Page), this.ClientID, script.ToString());
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
                    if (MapVisibleSetting)
                    {
                        map.Center.Latitude = address.Latitude;
                        map.Center.Longitude = address.Longitude;
                    }
                }
                else
                    pAddressError.Visible = true;

                //
                // Store the fact that we have distance information available.
                //
                ViewState["has_distance"] = 1;
            }
            else
            {
                if (MapVisibleSetting)
                {
                    map.Center.Latitude = ArenaContext.Current.Organization.Address.Latitude;
                    map.Center.Longitude = ArenaContext.Current.Organization.Address.Longitude;
                }

                //
                // Store the fact that we no longer have distance information available.
                //
                ViewState["has_distance"] = 0;
            }

            btnFilter_Click(sender, e);
        }


        /// <summary>
        /// Apply the filter data to the list of groups.
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        public void btnFilter_Click(object sender, EventArgs e)
        {
            map.ClearContent();
            AddCampusPlacemarks();
            AddCenterPlacemark();
            AddFilteredGroups();
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


            //
            // Only add the center placemark if it is not on the same location as a campus.
            //
            if (ShowCampusesSetting)
            {
                foreach (Campus c in ArenaContext.Current.Organization.Campuses)
                {
                    if (c.Address != null && map.Center.Latitude == c.Address.Latitude && map.Center.Longitude == c.Address.Longitude)
                        return;
                }
            }

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
        /// Add the campus placemarks on the map.
        /// </summary>
        private void AddCampusPlacemarks()
        {
            if (ShowCampusesSetting)
            {
                foreach (Campus c in ArenaContext.Current.Organization.Campuses)
                {
                    map.Placemarks.Add(new CampusPlacemark(c));
                }
            }
        }


        /// <summary>
        /// Go through the loaded small groups and apply the filter to the list and then
        /// add everything remaining to the map.
        /// </summary>
        private void AddFilteredGroups()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNode xroot;
            XmlAttribute attrib;
            SqlConnection con;
            SqlDataReader rdr;
            List<Group> groups = new List<Group>();
            SqlCommand cmd;
            Placemark placemark;


            //
            // Build a SQL query to enumerate groups in these categories.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT sg.group_id,dbo.cust_hdc_googlemaps_funct_distance_between(@LatFrom, @LongFrom, ca.Latitude, ca.Longitude) AS 'distance'" +
                "    FROM smgp_group AS sg" +
                "    LEFT OUTER JOIN core_person_address AS cpa ON cpa.person_id = sg.target_location_person_id" +
                "    LEFT OUTER JOIN core_address AS ca ON ca.address_id = cpa.address_id" +
                "    LEFT JOIN smgp_group_cluster AS sgc ON sgc.group_cluster_id = sg.group_cluster_id" +
                "    LEFT JOIN smgp_cluster_type AS sgt ON sgt.cluster_type_id = sgc.cluster_type_id" +
                "    WHERE sgt.category_id = @CategoryID" +
                "      AND sg.is_group_private = 0" +
                "      AND sg.active = 1" +
                "      AND ISNULL(cpa.primary_address, 1) = 1" +
                "      AND ISNULL(dbo.cust_hdc_googlemaps_funct_distance_between(@LatFrom, @LongFrom, ca.Latitude, ca.Longitude), -1) <= " + Convert.ToInt32(ddlDistance.SelectedValue).ToString();
            if (LimitToClusterTypeSetting != -1)
                cmd.CommandText += "      AND sgc.cluster_type_id = @ClusterTypeID";
            cmd.CommandText += "    ORDER BY 'distance'";
            cmd.Parameters.Add(new SqlParameter("@CategoryID", CategorySetting.CategoryID));
            cmd.Parameters.Add(new SqlParameter("@ClusterTypeID", LimitToClusterTypeSetting));
            cmd.Parameters.Add(new SqlParameter("@LatFrom", map.Center.Latitude));
            cmd.Parameters.Add(new SqlParameter("@LongFrom", map.Center.Longitude));

            //
            // Setup the XML document.
            //
            xroot = xdoc.CreateElement("groups");
            xdoc.AppendChild(xroot);

            //
            // Add the registration page setting for use during XSLT.
            //
            attrib = xdoc.CreateAttribute("registration_page");
            attrib.InnerText = RegistrationPageSetting.ToString();
            xroot.Attributes.Append(attrib);

            //
            // Inform the XSLT translator if we have distance calculations available.
            //
            attrib = xdoc.CreateAttribute("has_distance");
            attrib.InnerText = ViewState["has_distance"].ToString();
            xroot.Attributes.Append(attrib);

            //
            // Configuration item indicating if the map is visible or not..
            //
            attrib = xdoc.CreateAttribute("has_map");
            attrib.InnerText = MapVisibleSetting.ToString();
            xroot.Attributes.Append(attrib);

            //
            // Execute the reader and process all results.
            //
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                //
                // Process each small group.
                //
                try
                {
                    Group g = new Group(Convert.ToInt32(rdr[0]));
                    double distance;

                    //
                    // Filter out any groups we don't want.
                    //
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

                    //
                    // Add the group to the map.
                    //
                    placemark = new SmallGroupPlacemark(g);
                    placemark.SetAddedHandler("RegisterSmallGroup");
                    map.Placemarks.Add(placemark);

                    //
                    // Calculate the distance if we can.
                    //
                    try
                    {
                        distance = Math.Round(Convert.ToDouble(rdr[1]), 2);
                    }
                    catch
                    {
                        distance = -1.0f;
                    }

                    //
                    // Add the group to the XML node for translation.
                    //
                    xroot.AppendChild(GroupXmlNode(g, xdoc, distance));
                }
                catch { }
            }
            rdr.Close();

            XPathNavigator xnav = xdoc.CreateNavigator();
            XslCompiledTransform xtrans = new XslCompiledTransform();
            xtrans.Load(base.Server.MapPath(XsltFileSetting));
            StringBuilder sb = new StringBuilder();
            xtrans.Transform((IXPathNavigable)xnav, null, new StringWriter(sb));
            ltResultsContent.Text = sb.ToString();
        }


        /// <summary>
        /// Create an XML node for the given small group. The distance is the distance from
        /// a single point to the location at which this small group meets.
        /// </summary>
        /// <param name="g">The Group's properties to populate the XML node with.</param>
        /// <param name="xdoc">The XmlDocument that this node will be a part of.</param>
        /// <param name="distance">The distance from a central point to where this group meets.</param>
        /// <returns>A new XmlNode which identifies this small group.</returns>
        XmlNode GroupXmlNode(Group g, XmlDocument xdoc, double distance)
        {
            XmlElement group = xdoc.CreateElement("group");
            XmlAttribute attrib;


            attrib = xdoc.CreateAttribute("id");
            attrib.InnerText = g.GroupID.ToString();
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("name");
            attrib.InnerText = g.Name;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("leadername");
            attrib.InnerText = g.Leader.FullName;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("meetingday");
            attrib.InnerText = g.MeetingDay.Value;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("meetingstarttime");
            attrib.InnerText = g.MeetingStartTime.ToString("t");
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("meetingendtime");
            attrib.InnerText = g.MeetingEndTime.ToString("t");
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("type");
            attrib.InnerText = g.GroupType.Value;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("topic");
            attrib.InnerText = g.Topic.Value;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("averageage");
            attrib.InnerText = g.AverageAge.ToString();
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("agerange");
            attrib.InnerText = g.PrimaryAge.Value;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("maritalpreference");
            attrib.InnerText = g.PrimaryMaritalStatus.Value;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("description");
            attrib.InnerText = g.Description;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("schedule");
            attrib.InnerText = g.Schedule;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("notes");
            attrib.InnerText = g.Notes;
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("picture");
            if (g.ImageBlob.BlobID != -1)
                attrib.InnerText = String.Format("CachedBlob.aspx?guid={0}", g.ImageBlob.GUID);
            else
                attrib.InnerText = "";
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("distance");
            attrib.InnerText = distance.ToString();
            group.Attributes.Append(attrib);

            attrib = xdoc.CreateAttribute("latitude");
            attrib.InnerText = g.TargetLocation.Latitude.ToString();
            group.Attributes.Append(attrib);
            attrib = xdoc.CreateAttribute("longitude");
            attrib.InnerText = g.TargetLocation.Longitude.ToString();
            group.Attributes.Append(attrib);

            return group;
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
            lbFilterCampus.Visible = FilterOptionsSetting.Contains(FilterOptions.Campus);

            //
            // Setup the Meeting Day choices.
            //
            ddlMeetingDay.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.DayOfWeek))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlMeetingDay.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            lbFilterMeetingDay.Visible = FilterOptionsSetting.Contains(FilterOptions.MeetingDay);

            //
            // Setup the Topic choices.
            //
            ddlTopic.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.SmallGroupTopic))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlTopic.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            lbFilterTopic.Visible = FilterOptionsSetting.Contains(FilterOptions.Topic);
            
            //
            // Setup the Marital Preference choices.
            //
            ddlMaritalPreference.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.MaritalPreference))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlMaritalPreference.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            lbFilterMaritalPreference.Visible = FilterOptionsSetting.Contains(FilterOptions.MaritalPreference);

            //
            // Setup the Age Range choices.
            //
            ddlAgeRange.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.AgeRangePreference))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlAgeRange.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            lbFilterAgeRange.Visible = FilterOptionsSetting.Contains(FilterOptions.AgeRange);

            //
            // Setup the Type choices.
            //
            ddlType.Items.Add(new ListItem("Any", "-1"));
            foreach (Lookup lkup in new LookupCollection(SystemLookupType.SmallGroupType))
            {
                if (lkup.Value != "Any" && lkup.Value != "Unknown")
                    ddlType.Items.Add(new ListItem(lkup.Value, lkup.LookupID.ToString()));
            }
            lbFilterType.Visible = FilterOptionsSetting.Contains(FilterOptions.Type);

            //
            // Setup the Area choices.
            //
            ddlArea.Items.Add(new ListItem("Any", "-1"));
            foreach (Area a in new AreaCollection(ArenaContext.Current.Organization.OrganizationID))
            {
                ddlArea.Items.Add(new ListItem(a.Name, a.AreaID.ToString()));
            }
            lbFilterArea.Visible = FilterOptionsSetting.Contains(FilterOptions.Area);
        }


        private void SmallGroupInfo(int groupID)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNode xroot;
            XmlAttribute xattrib;
            Group group = new Group(groupID);


            //
            // Setup the XML document.
            //
            xroot = xdoc.CreateElement("Data");
            xdoc.AppendChild(xroot);

            //
            // Add the registration page setting for use during XSLT.
            //
            xattrib = xdoc.CreateAttribute("registration_page");
            xattrib.InnerText = RegistrationPageSetting.ToString();
            xroot.Attributes.Append(xattrib);

            //
            // Add all the various captions.
            //
            xattrib = xdoc.CreateAttribute("agegroup_caption");
            xattrib.InnerText = group.ClusterType.Category.AgeGroupCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("description_caption");
            xattrib.InnerText = group.ClusterType.Category.DescriptionCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("leader_caption");
            xattrib.InnerText = group.ClusterType.Category.LeaderCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("maritalpreference_caption");
            xattrib.InnerText = group.ClusterType.Category.MaritalPreferenceCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("meetingday_caption");
            xattrib.InnerText = group.ClusterType.Category.MeetingDayCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("meetingendtime_caption");
            xattrib.InnerText = group.ClusterType.Category.MeetingEndTimeCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("meetingstarttime_caption");
            xattrib.InnerText = group.ClusterType.Category.MeetingStartTimeCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("name_caption");
            xattrib.InnerText = group.ClusterType.Category.NameCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("notes_caption");
            xattrib.InnerText = group.ClusterType.Category.NotesCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("picture_caption");
            xattrib.InnerText = group.ClusterType.Category.PictureCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("schedule_caption");
            xattrib.InnerText = group.ClusterType.Category.ScheduleCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("topic_caption");
            xattrib.InnerText = group.ClusterType.Category.TopicCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("type_caption");
            xattrib.InnerText = group.ClusterType.Category.TypeCaption.ToString();
            xroot.Attributes.Append(xattrib);
            xattrib = xdoc.CreateAttribute("url_caption");
            xattrib.InnerText = group.ClusterType.Category.UrlCaption.ToString();
            xroot.Attributes.Append(xattrib);

            //
            // Add in the small group information.
            //
            xroot.AppendChild(GroupXmlNode(group, xdoc, -1));

            //
            // Translate it with XSLT.
            //
            XPathNavigator xnav = xdoc.CreateNavigator();
            XslCompiledTransform xtrans = new XslCompiledTransform();
            xtrans.Load(base.Server.MapPath(InfoXsltFileSetting));
            StringBuilder sb = new StringBuilder();
            xtrans.Transform((IXPathNavigable)xnav, null, new StringWriter(sb));

            //
            // Send the response.
            //
            Response.Clear();
            Response.Write(sb.ToString());
            Response.End();
        }

        #endregion
    }
}