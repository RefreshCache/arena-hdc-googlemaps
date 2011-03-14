using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Arena.Core;
using Arena.Organization;
using Arena.Portal.UI;
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace Arena.Custom.HDC.GoogleMaps.UI
{
    /// <summary>
    /// The GoogleMap control provides a simple way to add a GoogleMap into your UserControl.
    /// Via the provided interfaces you can either add pins individually (not really recommended)
    /// or can add pins in bulk via the contents of profiles, small groups, lists, etc.
    /// </summary>
    public class GoogleMap : WebControl, INamingContainer
    {
        #region WebControl Properties

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether or not to hide all the controls on the map. To hide individual controls use the Show... properties.")]
        public Boolean HideControls { get; set; }

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether or not to hide the download link under the map.")]
        public Boolean HideDownload { get; set; }

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether or not to hide the add-to-tag link under the map.")]
        public Boolean HideAddToTag { get; set; }

        [Category("Appearance")]
        [DefaultValue(480)]
        [Description("The width of the map on screen.")]
        public new Int32 Width { get; set; }

        [Category("Appearance")]
        [DefaultValue(360)]
        [Description("The height of the map on screen.")]
        public new Int32 Height { get; set; }

        [Category("Appearance")]
        [DefaultValue(12)]
        [Description("The default zoom level to use on the map, higher number is zoomed in further.")]
        public Int32 ZoomLevel { get; set; }

        [Category("Behavior")]
        [DefaultValue(-1)]
        [Description("The maximum zoom level that can be used. Default value is -1 for no limit.")]
        public Int32 MaxZoomLevel { get; set; }

        [Category("Behavior")]
        [DefaultValue(-1)]
        [Description("The minimum zoom level that can be used. Default value is -1 for no limit.")]
        public Int32 MinZoomLevel { get; set; }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If set to true the user will not be able to zoom, pan or otherwise modify the viewport of the map.")]
        public Boolean StaticMap { get; set; }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Shows the pan controls on the map.")]
        public Boolean ShowPanControls { get; set; }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Shows the scale/zoom control on the map.")]
        public Boolean ShowZoomControls { get; set; }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Shows the street view icon on the map which enables the user to switch into street view mode.")]
        public Boolean ShowStreetView { get; set; }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Shows the map type controls (i.e. street, satellite, etc.) on the map.")]
        public Boolean ShowMapType { get; set; }

        #endregion


        #region Properties

        /// <summary>
        /// A list of Placemark objects that will be placed on the map at load time. This should only
        /// be used for showing things like addresses. People or any other bulk list of items should
        /// by loaded dynamically via other lists.
        /// </summary>
        public List<Placemark> Placemarks { get { return _Placemarks; } }
        private List<Placemark> _Placemarks;

        /// <summary>
        /// A list of RadiusLoader objects that will be used to populate the map with placemarks.
        /// </summary>
        public List<PlacemarkLoader> Loaders { get { return _Loaders; } }
        private List<PlacemarkLoader> _Loaders;

        /// <summary>
        /// The Javascript object that can be used to interact with the Google Map.
        /// </summary>
        public String ClientObject { get { return "GMap_" + this.ClientID; } }

        /// <summary>
        /// The center point to use for this map, defaults to the primary campus address.
        /// </summary>
        public GeocodedAddress Center;

        private HtmlGenericControl commandDiv;

        /// <summary>
        /// The Lookup of the ProfileSource to use when adding a person
        /// to a tag.
        /// </summary>
        public Lookup ProfileSource
        {
            get
            {
                if (_ProfileSource == null)
                {
                    if (!String.IsNullOrEmpty(ArenaContext.Current.Organization.Settings["GoogleMaps_ProfileSource"]))
                        _ProfileSource = new Lookup(Convert.ToInt32(ArenaContext.Current.Organization.Settings["GoogleMaps_ProfileSource"]));
                    else
                    {
                        foreach (Lookup l in new LookupCollection(SystemLookupType.ProfileSource))
                        {
                            if (l.Active)
                            {
                                _ProfileSource = l;
                                break;
                            }
                        }
                    }
                }

                return _ProfileSource;
            }
        }
        private Lookup _ProfileSource;

        /// <summary>
        /// The Lookup of the Profile Status to use when adding a person
        /// to a tag.
        /// </summary>
        public Lookup ProfileStatus
        {
            get
            {
                if (_ProfileStatus == null)
                {
                    if (!String.IsNullOrEmpty(ArenaContext.Current.Organization.Settings["GoogleMaps_ProfileStatus"]))
                        _ProfileStatus = new Lookup(Convert.ToInt32(ArenaContext.Current.Organization.Settings["GoogleMaps_ProfileStatus"]));
                    else
                    {
                        foreach (Lookup l in new LookupCollection(SystemLookupType.TagMemberStatus))
                        {
                            if (l.Active)
                            {
                                _ProfileStatus = l;
                                break;
                            }
                        }
                    }
                }

                return _ProfileStatus;
            }
        }
        private Lookup _ProfileStatus;

        //
        // These controls comprise the Download commands.
        //
        private LinkButton downloadButton;
        private HtmlGenericControl downloadDiv;
        private CheckBox downloadIncludeCampus;
        private CheckBox downloadIncludeAreaOverlays;

        //
        // These controls comprise the Add To Tag commands.
        //
        private LinkButton createTagButton;
        private HtmlGenericControl profileDiv;
        private TextBox profileName;
        private ProfilePicker profilePicker;
        private CheckBox profileOnlyAdults;

        #endregion


        #region Constructor

        /// <summary>
        /// Create a new GoogleMap instance. Set some defaults and other things.
        /// </summary>
        public GoogleMap()
            : base()
        {
            this.Width = 480;
            this.Height = 360;
            this._Placemarks = new List<Placemark>();
            this._Loaders = new List<PlacemarkLoader>();
            this.Center = new GeocodedAddress();
            this.Center.Latitude = ArenaContext.Current.Organization.Address.Latitude;
            this.Center.Longitude = ArenaContext.Current.Organization.Address.Longitude;
            this.ZoomLevel = 12;
            this.MinZoomLevel = -1;
            this.MaxZoomLevel = -1;
            this.StaticMap = false;
            this.ShowStreetView = true;
            this.ShowPanControls = true;
            this.ShowZoomControls = true;
            this.ShowMapType = true;
            this.HideAddToTag = false;
            this.HideDownload = false;
        }

        #endregion


        #region WebControl override methods.

        /// <summary>
        /// Load in all the Javascript references we need on this page.
        /// </summary>
        /// <param name="e">Unused parameter.</param>
        protected override void OnLoad(EventArgs e)
        {
            BasePage.AddJavascriptInclude(Page, BasePage.JQUERY_INCLUDE);
            BasePage.AddJavascriptInclude(Page, "http://maps.google.com/maps/api/js?sensor=false");
            BasePage.AddJavascriptInclude(Page, "Custom/Website/googlemaps.js");
            BasePage.AddJavascriptInclude(Page, "Custom/Website/json2.js");
        }


        /// <summary>
        /// Render all the HTML and Javascript needed to make this control work properly.
        /// </summary>
        /// <param name="output">Unused parameter</param>
        protected override void Render(HtmlTextWriter output)
        {
            StringBuilder script = new StringBuilder();


            //
            // Generate the DIV tag on the page that google maps will use.
            //
            output.Write("<div id=\"{0}\" style=\"width: {1}px; height: {2}px;\"></div>", this.ClientID, this.Width, this.Height);

            //
            // Generate the start of the script needed to populate the map.
            //
            script.Append("<script language=\"javascript\" type=\"text/javascript\">\n" +
                "    var " + this.ClientObject + " = null;\n" +
                "    $(window).load(function() {\n" +
                "        var o = {};\n");
            script.AppendLine("        o.zoom = " + ZoomLevel.ToString() + ";");
            if (MinZoomLevel != -1)
                script.AppendLine("        o.minZoom = " + MinZoomLevel.ToString() + ";");
            if (MaxZoomLevel != -1)
                script.AppendLine("        o.maxZoom = " + MaxZoomLevel.ToString() + ";");
            if (HideControls == true)
                script.AppendLine("        o.disableDefaultUI = true;");
            else
            {
                if (ShowMapType == false)
                    script.AppendLine("        o.mapTypeControl = false;");
                if (ShowPanControls == false)
                    script.AppendLine("        o.panControl = false;");
                if (ShowStreetView == false)
                    script.AppendLine("        o.streetViewControl = false;");
                if (ShowZoomControls == false)
                {
                    script.AppendLine("        o.scaleControl = false;");
                    script.AppendLine("        o.zoomControl = false;");
                }
            }
            if (StaticMap == true)
            {
                script.AppendLine("        o.disableDoubleClickZoom = true");
                script.AppendLine("        o.draggable = false;");
                script.AppendLine("        o.keyboardShortcuts = false;");
                script.AppendLine("        o.mapTypeControl = false;");
                script.AppendLine("        o.minZoom = " + ZoomLevel.ToString() + ";");
                script.AppendLine("        o.maxZoom = " + ZoomLevel.ToString() + ";");
                script.AppendLine("        o.panControl = false;");
                script.AppendLine("        o.scaleControl = false;");
                script.AppendLine("        o.scrollwheel = false;");
                script.AppendLine("        o.streetViewControl = false;");
                script.AppendLine("        o.zoomControl = false;");
            }
            script.Append("        var " + this.ClientObject + " = new GoogleMap(\"" + this.ClientID + "\", new GeoAddress(" + Center.Latitude.ToString() + ", " + Center.Longitude.ToString() + "), \"\", o);\n");

            //
            // Render in all the other elements.
            //
            RenderPlacemarks(script);
            RenderLoaders(script);

            //
            // Generate the final sequence of the script.
            //
            script.Append("    });\n</script>");

            //
            // Register the startup script commands.
            //
            Page.ClientScript.RegisterStartupScript(typeof(Page), this.ClientID, script.ToString());

            //
            // Render child controls.
            //
            commandDiv.RenderControl(output);
            if (HideDownload == false)
            {
                downloadDiv.RenderControl(output);
            }
            if (HideAddToTag == false)
            {
                profileDiv.RenderControl(output);
            }
        }


        /// <summary>
        /// Create all child controls that will be used by this control.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();


            commandDiv = new HtmlGenericControl("DIV");
            Controls.Add(commandDiv);
            commandDiv.ID = "commandArea";

            //
            // Create the download button below the map.
            //
            if (HideDownload == false)
            {
                CreateDownloadControls();
            }

            //
            // Create the "Add To Tag" button below the map.
            //
            if (HideAddToTag == false)
            {
                CreateProfileControls();
            }
        }


        /// <summary>
        /// Load the saved information from the previous postback.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            this.Center = (GeocodedAddress)ViewState["Center"];
            this._Loaders = (List<PlacemarkLoader>)ViewState["Loaders"];
            this._Placemarks = (List<Placemark>)ViewState["Placemarks"];
            this.HideControls = (Boolean)ViewState["HideControls"];
            this.HideDownload = (Boolean)ViewState["HideDownload"];
            this.HideAddToTag = (Boolean)ViewState["HideAddToTag"];
            this.Height = (Int32)ViewState["Height"];
            this.Width = (Int32)ViewState["Width"];
            this.StaticMap = (Boolean)ViewState["StaticMap"];
            this.ShowStreetView = (Boolean)ViewState["ShowStreetView"];
            this.ShowPanControls = (Boolean)ViewState["ShowPanControls"];
            this.ShowZoomControls = (Boolean)ViewState["ShowZoomControls"];
            this.ShowMapType = (Boolean)ViewState["ShowMapType"];
            this.MinZoomLevel = (Int32)ViewState["MinZoomLevel"];
            this.MaxZoomLevel = (Int32)ViewState["MaxZoomLevel"];
        }


        /// <summary>
        /// Save information about this object into the next postback.
        /// </summary>
        protected override object SaveViewState()
        {
            ViewState["MaxZoomLevel"] = this.MaxZoomLevel;
            ViewState["MinZoomLevel"] = this.MinZoomLevel;
            ViewState["ShowMapType"] = this.ShowMapType;
            ViewState["ShowZoomControls"] = this.ShowZoomControls;
            ViewState["ShowPanControls"] = this.ShowPanControls;
            ViewState["ShowStreetView"] = this.ShowStreetView;
            ViewState["StaticMap"] = this.StaticMap;
            ViewState["Width"] = this.Width;
            ViewState["Height"] = this.Height;
            ViewState["HideAddToTag"] = this.HideAddToTag;
            ViewState["HideDownload"] = this.HideDownload;
            ViewState["HideControls"] = this.HideControls;
            ViewState["Placemarks"] = this._Placemarks;
            ViewState["Loaders"] = this._Loaders;
            ViewState["Center"] = this.Center;

            return base.SaveViewState();
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// User wants to download the data on the map into a KML file for use
        /// in Google Earth.
        /// </summary>
        void downloadButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            Google google;
            KML kml;


            EnsureChildControls();

            //
            // Create the KML object to work with.
            //
            google = new Google(ArenaContext.Current.User, BaseUrl());
            kml = new KML(google);

            //
            // Add in each individual placemark.
            //
            foreach (Placemark placemark in Placemarks)
            {
                kml.AddPlacemark(placemark);
            }

            //
            // Run through each RadiusLoader and process it.
            //
            foreach (PlacemarkLoader loader in Loaders)
            {
                kml.AddLoader(loader);
            }

            //
            // Include the church campuses if they checked the box.
            //
            if (downloadIncludeCampus.Checked)
            {
                foreach (Campus c in ArenaContext.Current.Organization.Campuses)
                {
                    kml.AddPlacemark(new CampusPlacemark(c));
                }
            }

            //
            // Include the area overlays if the user wants them.
            //
            if (downloadIncludeAreaOverlays.Checked)
            {
                foreach (Area a in new AreaCollection(ArenaContext.Current.Organization.OrganizationID))
                {
                    kml.AddAreaPolygon(a);
                }
            }

            //
            // Convert the KML into it's raw XML data and send it to the client.
            //
            kml.xml.Save(writer);
            Page.Response.Clear();
            Page.Response.ContentType = "application/vnd.google-earth.kml+xml";
            Page.Response.AppendHeader("Content-Disposition", "attachment; filename=arena.kml");
            Page.Response.Write(sb.ToString());
            Page.Response.End();
        }


        /// <summary>
        /// Create a new personal tag with the given name for the user. The tag will have the
        /// "person contents" of this map.
        /// </summary>
        void createTagButton_Click(object sender, EventArgs e)
        {
            Profile profile;
            Google google;


            google = new Google(ArenaContext.Current.User, BaseUrl());
            EnsureChildControls();

            //
            // Determine the profile to work with.
            //
            if (profilePicker.ProfileID != -1)
                profile = new Profile(profilePicker.ProfileID);
            else if (!String.IsNullOrEmpty(profileName.Text))
            {
                //
                // Create the personal tag.
                //
                profile = new Profile();
                profile.OrganizationID = ArenaContext.Current.Organization.OrganizationID;
                profile.Owner = ArenaContext.Current.Person;
                profile.ProfileType = Enums.ProfileType.Personal;
                profile.Name = profileName.Text;
                profile.Active = true;
                profile.Save(ArenaContext.Current.User.Identity.Name);
            }
            else
                return;

            //
            // Add each individual placemark to the profile.
            //
            foreach (Placemark p in Placemarks)
            {
                AddPlacemarkToProfile(p, profile);
            }

            //
            // Add all the placemarks from each loader to the profile.
            //
            foreach (PlacemarkLoader pl in Loaders)
            {
                foreach (Placemark p in pl.LoadPlacemarks(google))
                {
                    AddPlacemarkToProfile(p, profile);
                }
            }
        }


        #endregion


        #region Support Methods

        /// <summary>
        /// Render the Javascript needed to put specific placemarks on the map.
        /// </summary>
        /// <param name="script">The script string to append our JS to.</param>
        private void RenderPlacemarks(StringBuilder script)
        {
            foreach (Placemark placemark in _Placemarks)
            {
                script.Append(placemark.JavascriptCode(this.ClientObject, "marker"));
            }
        }


        /// <summary>
        /// Render the Javascript needed to put all radius loaders onto the map. These
        /// render multiple placemarks based on distance from a central address.
        /// </summary>
        /// <param name="script">The script string to append our JS to.</param>
        private void RenderLoaders(StringBuilder script)
        {
            foreach (PlacemarkLoader loader in _Loaders)
            {
                script.Append(loader.AjaxLoadPopulation(this.ClientObject));
            }
        }


        /// <summary>
        /// Create all the controls needed to process download requests.
        /// </summary>
        private void CreateDownloadControls()
        {
            HtmlGenericControl downloadCancel, downloadShow;
            Literal lt;

            //
            // Create the "download..." link that will show the download controls.
            //
            downloadShow = new HtmlGenericControl("SPAN");
            downloadShow.InnerText = "Download...";
            downloadShow.Attributes.Add("class", "smallText");
            downloadShow.Style.Add("cursor", "pointer");
            downloadShow.Attributes.Add("onclick", this.ClientID + "_ShowDownload();");
            commandDiv.Controls.Add(downloadShow);

            //
            // Create the download controls area.
            //
            downloadDiv = new HtmlGenericControl("DIV");
            Controls.Add(downloadDiv);
            downloadDiv.ID = "downloadDiv";
            downloadDiv.Style.Add("display", "none");
            lt = new Literal();
            lt.Text = "<br />";
            downloadDiv.Controls.Add(lt);

            //
            // Create the "include campuses" checkbox.
            //
            downloadIncludeCampus = new CheckBox();
            downloadDiv.Controls.Add(downloadIncludeCampus);
            downloadIncludeCampus.ID = "downloadIncludeCampus";
            downloadIncludeCampus.Text = "Include Campus Locations";
            downloadIncludeCampus.ToolTip = "This will cause your KML download to include pins which identify the various campuses defined in your organization.";
            downloadIncludeCampus.CssClass = "smallText";
            lt = new Literal();
            lt.Text = "<br />";
            downloadDiv.Controls.Add(lt);

            //
            // Create the "include areas" checkbox.
            //
            downloadIncludeAreaOverlays = new CheckBox();
            downloadDiv.Controls.Add(downloadIncludeAreaOverlays);
            downloadIncludeAreaOverlays.ID = "downloadIncludeAreaOverlays";
            downloadIncludeAreaOverlays.Text = "Include Area Overlays";
            downloadIncludeAreaOverlays.ToolTip = "Your KML will include colored overlays which identify the various small group areas you have defined.";
            downloadIncludeAreaOverlays.CssClass = "smallText";
            lt = new Literal();
            lt.Text = "<br />";
            downloadDiv.Controls.Add(lt);

            //
            // Create the button that triggers a download action.
            //
            downloadButton = new LinkButton();
            downloadDiv.Controls.Add(downloadButton);
            downloadButton.CssClass = "smallText";
            downloadButton.ID = "download";
            downloadButton.Text = "Download";
            downloadButton.OnClientClick = this.ClientID + "_HideDownload();";
            downloadButton.Click += new EventHandler(downloadButton_Click);
            lt = new Literal();
            lt.Text = "&nbsp;&nbsp;&nbsp;";
            downloadDiv.Controls.Add(lt);

            //
            // Create the "cancel" link that will hide the download controls.
            //
            downloadCancel = new HtmlGenericControl("SPAN");
            downloadDiv.Controls.Add(downloadCancel);
            downloadCancel.InnerText = "Cancel";
            downloadCancel.Attributes.Add("class", "smallText");
            downloadCancel.Style.Add("cursor", "pointer");
            downloadCancel.Attributes.Add("onclick", this.ClientID + "_HideDownload();");

            //
            // Generate all the javascript needed.
            //
            StringBuilder script = new StringBuilder();

            script.Append("<script language=\"javascript\" type=\"text/javascript\">\n" +
                "function " + this.ClientID + "_ShowDownload() {\n" +
                "  $('#" + commandDiv.ClientID + "').hide('fast', function() { $('#" + downloadDiv.ClientID + "').show(); });\n" +
                "}\n" +
                "function " + this.ClientID + "_HideDownload() {\n" +
                "  $('#" + downloadDiv.ClientID + "').hide('fast', function() { $('#" + commandDiv.ClientID + "').show(); });\n" +
                "}\n" +
                "</script>\n");
            Page.ClientScript.RegisterStartupScript(typeof(Page), this.ClientID + "_Download", script.ToString());
        }


        /// <summary>
        /// Create all the controls necessary for the user to be able to add
        /// the placemarks into a personal tag.
        /// </summary>
        private void CreateProfileControls()
        {
            HtmlGenericControl profileCancel, profileShow, span;
            Literal lt;

            //
            // Create the "Create Tag..." link that will show the download controls.
            //
            profileShow = new HtmlGenericControl("SPAN");
            profileShow.InnerText = "Create Tag...";
            profileShow.Attributes.Add("class", "smallText");
            profileShow.Style.Add("cursor", "pointer");
            profileShow.Attributes.Add("onclick", this.ClientID + "_ShowProfile();");
            if (commandDiv.Controls.Count > 0)
            {
                lt = new Literal();
                lt.Text = "&nbsp;&nbsp;&nbsp;";
                commandDiv.Controls.Add(lt);
            }
            commandDiv.Controls.Add(profileShow);

            //
            // Create the profile controls area.
            //
            profileDiv = new HtmlGenericControl("DIV");
            Controls.Add(profileDiv);
            profileDiv.ID = "profileDiv";
            profileDiv.Style.Add("display", "none");
            lt = new Literal();
            lt.Text = "<br />";
            profileDiv.Controls.Add(lt);

            //
            // Create the "new tag" text field.
            //
            span = new HtmlGenericControl("SPAN");
            span.Attributes.Add("class", "smallText");
            span.InnerText = "Add to new tag: ";
            profileDiv.Controls.Add(span);
            profileName = new TextBox();
            profileDiv.Controls.Add(profileName);
            profileName.ID = "profileName";
            profileName.Width = Unit.Pixel(150);
            lt = new Literal();
            lt.Text = "<br />";
            profileDiv.Controls.Add(lt);

            //
            // Create the "Existing tag" selection.
            //
            span = new HtmlGenericControl("SPAN");
            span.Attributes.Add("class", "smallText");
            span.InnerText = "Or existing tag: ";
            profileDiv.Controls.Add(span);
            profilePicker = new ProfilePicker();
            profileDiv.Controls.Add(profilePicker);
            profilePicker.ID = "profilePicker";
            profilePicker.AllowRemove = true;
            profilePicker.IncludeEvents = false;
            profilePicker.SelectableRoots = false;
            profilePicker.ProfileType = Enums.ProfileType.Ministry;
            profilePicker.ProfileID = -1;
            profilePicker.AllowedPermission = (short)Arena.Security.OperationType.Edit_People;
            lt = new Literal();
            lt.Text = "<br />";
            profileDiv.Controls.Add(lt);

            //
            // Add the "only adults" check box.
            //
            profileOnlyAdults = new CheckBox();
            profileDiv.Controls.Add(profileOnlyAdults);
            profileOnlyAdults.ID = "profileOnlyAdults";
            profileOnlyAdults.Text = "Only include adults";
            profileOnlyAdults.ToolTip = "This will only add adults to the tag you have selected, no children will be added.";
            profileOnlyAdults.CssClass = "smallText";
            lt = new Literal();
            lt.Text = "<br />";
            profileDiv.Controls.Add(lt);

            //
            // Create the button that triggers a download action.
            //
            createTagButton = new LinkButton();
            profileDiv.Controls.Add(createTagButton);
            createTagButton.CssClass = "smallText";
            createTagButton.ID = "createTag";
            createTagButton.Text = "Add to tag";
            createTagButton.OnClientClick = this.ClientID + "_HideProfile();";
            createTagButton.Click += new EventHandler(createTagButton_Click);
            lt = new Literal();
            lt.Text = "&nbsp;&nbsp;&nbsp;";
            profileDiv.Controls.Add(lt);

            //
            // Create the "cancel" link that will hide the download controls.
            //
            profileCancel = new HtmlGenericControl("SPAN");
            profileDiv.Controls.Add(profileCancel);
            profileCancel.InnerText = "Cancel";
            profileCancel.Attributes.Add("class", "smallText");
            profileCancel.Style.Add("cursor", "pointer");
            profileCancel.Attributes.Add("onclick", this.ClientID + "_HideProfile();");

            //
            // Generate all the javascript needed.
            //
            StringBuilder script = new StringBuilder();

            script.Append("<script language=\"javascript\" type=\"text/javascript\">\n" +
                "function " + this.ClientID + "_ShowProfile() {\n" +
                "  $('#" + commandDiv.ClientID + "').hide('fast', function() { $('#" + profileDiv.ClientID + "').show(); });\n" +
                "}\n" +
                "function " + this.ClientID + "_HideProfile() {\n" +
                "  $('#" + profileDiv.ClientID + "').hide('fast', function() { $('#" + commandDiv.ClientID + "').show(); });\n" +
                "}\n" +
                "</script>\n");
            Page.ClientScript.RegisterStartupScript(typeof(Page), this.ClientID + "_Profile", script.ToString());
        }


        /// <summary>
        /// Add a placemark object to a profile. If the placemark is a PersonPlacemark
        /// then the associated person record is used. If it is a FamilyPlacemark then
        /// all the members of the family are used. Otherwise the placemark is ignored.
        /// This method only adds individuals who are not already a member of the tag.
        /// </summary>
        /// <param name="placemark">The placemark to add to the profile.</param>
        /// <param name="profile">The profile to add the members to.</param>
        private void AddPlacemarkToProfile(Placemark placemark, Profile profile)
        {
            List<Person> people = new List<Person>();


            //
            // Convert the placemark into a list of people that should be added.
            //
            if (placemark.GetType() == typeof(PersonPlacemark))
            {
                people.Add(((PersonPlacemark)placemark).GetPerson());
            }
            else if (placemark.GetType() == typeof(FamilyPlacemark))
            {
                foreach (Person p in ((FamilyPlacemark)placemark).GetFamily().FamilyMembersActive)
                {
                    people.Add(p);
                }
            }

            //
            // For each person found, verify that they should be added and then
            // add them if they are not already in the profile.
            //
            foreach (Person p in people)
            {
                ProfileMember pm;

                //
                // Verify they are an adult if the user wants only adults.
                //
                if (profileOnlyAdults.Checked == true)
                {
                    FamilyMember fm = new FamilyMember(p.FamilyId, p.PersonID);

                    if (!fm.FamilyRole.Guid.ToString().Equals("e410e1a6-8715-4bfb-bf03-1cd18051f815", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                }

                //
                // Verify they are not already in the profile.
                //
                pm = new ProfileMember(profile.ProfileID, p);
                if (pm.ProfileID == -1)
                {
                    pm = new ProfileMember();

                    //
                    // Add them to the profile.
                    //
                    pm.ProfileID = profile.ProfileID;
                    pm.PersonID = p.PersonID;
                    pm.Source = ProfileSource;
                    pm.Status = ProfileStatus;
                    pm.DateActive = DateTime.Now;
                    pm.Save(ArenaContext.Current.User.Identity.Name);

                    //
                    // If this is a serving profile, we need a little more information.
                    //
                    if (profile.ProfileType == Enums.ProfileType.Serving)
                    {
                        ServingProfile sp = new ServingProfile(profile.ProfileID);
                        ServingProfileMember spm = new ServingProfileMember(pm.ProfileID, pm.PersonID);

                        spm.HoursPerWeek = sp.DefaultHoursPerWeek;
                        spm.Save(ArenaContext.Current.User.Identity.Name);
                    }
                }
            }
        }


        /// <summary>
        /// Retrieve the base url (the portion of the URL without the last path
        /// component, that is the filename and query string) of the current
        /// web request.
        /// </summary>
        /// <returns>Base url as a string.</returns>
        private string BaseUrl()
        {
            StringBuilder url = new StringBuilder();
            string[] segments;
            int i;


            url.Append(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
            url.Append(":" + HttpContext.Current.Request.Url.Port.ToString());
            segments = HttpContext.Current.Request.Url.Segments;
            for (i = 0; i < segments.Length - 1; i++)
            {
                url.Append(segments[i]);
            }

            return url.ToString();
        }

        #endregion


        #region Commands

        /// <summary>
        /// This method clears the content on the map of all placemarks.
        /// </summary>
        public void ClearContent()
        {
            this.Placemarks.Clear();
            this.Loaders.Clear();
        }

        #endregion
    }
}
