﻿using System;
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
        #region Properties

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether or not to hide all the controls on the map. To hide individual controls use the Show... properties.")]
        public Boolean HideControls { get; set; }

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether or not to hide the download link under the map.")]
        public Boolean HideDownload { get; set; }

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

        //
        // These controls comprise the Download commands.
        //
        private LinkButton downloadButton;
        private HtmlGenericControl downloadDiv;
        private CheckBox downloadIncludeCampus;
        private CheckBox downloadIncludeAreaOverlays;

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
                "    $(document).ready(function() {\n" +
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
            Profile p = new Profile();


            //
            // Create the personal tag.
            //
            p.ProfileType = Enums.ProfileType.Personal;
            p.Name = "test";
            p.Active = true;
            p.Save(ArenaContext.Current.User.Identity.Name);
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
            downloadShow = new HtmlGenericControl("A");
            downloadShow.InnerText = "Download...";
            downloadShow.Attributes.Add("class", "smallText");
            downloadShow.Attributes.Add("href", "#");
            downloadShow.Attributes.Add("onclick", this.ClientID + "_ShowDownload(); return false;");
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
            downloadCancel = new HtmlGenericControl("A");
            downloadDiv.Controls.Add(downloadCancel);
            downloadCancel.InnerText = "Cancel";
            downloadCancel.Attributes.Add("class", "smallText");
            downloadCancel.Attributes.Add("href", "#");
            downloadCancel.Attributes.Add("onclick", this.ClientID + "_HideDownload(); return false;");

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
