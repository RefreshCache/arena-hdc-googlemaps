using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
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
        [Description("Whether or not to hide the controls on the map.")]
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
        public List<RadiusLoader> RadiusLoaders { get { return _RadiusLoaders; } }
        private List<RadiusLoader> _RadiusLoaders;

        /// <summary>
        /// The Javascript object that can be used to interact with the Google Map.
        /// </summary>
        public String ClientObject { get { return "GMap_" + this.ClientID; } }

        private LinkButton downloadButton;

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
            _Placemarks = new List<Placemark>();
            _RadiusLoaders = new List<RadiusLoader>();
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
            if (HideControls == true)
                script.Append("        o.disableDefaultUI = true;\n");
            script.Append("        var " + this.ClientObject + " = new GoogleMap(\"" + this.ClientID + "\", new GeoAddress(" + "34.5212" + ", " + "-117.3456" + "), \"\", o);\n");

            //
            // Render in all the other elements.
            //
            RenderPlacemarks(script);
            RenderRadiusLoaders(script);

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
            if (HideDownload == false)
                downloadButton.RenderControl(output);
        }


        /// <summary>
        /// Create all child controls that will be used by this control.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();


            //
            // Create the download button below the map.
            //
            if (HideDownload == false)
            {
                downloadButton = new LinkButton();
                downloadButton.ID = "download";
                downloadButton.Style.Add("font-size", "small");
                downloadButton.Style.Add("text-decoration", "none");
                downloadButton.Text = "Download";
                downloadButton.Click += new EventHandler(downloadButton_Click);

                Controls.Add(downloadButton);
            }
        }


        /// <summary>
        /// Load the saved information from the previous postback.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            this._RadiusLoaders = (List<RadiusLoader>)ViewState["RadiusLoaders"];
            this._Placemarks = (List<Placemark>)ViewState["Placemarks"];
            this.HideControls = (Boolean)ViewState["HideControls"];
            this.HideDownload = (Boolean)ViewState["HideDownload"];
            this.Height = (Int32)ViewState["Height"];
            this.Width = (Int32)ViewState["Width"];
        }


        /// <summary>
        /// Save information about this object into the next postback.
        /// </summary>
        protected override object SaveViewState()
        {
            ViewState["Width"] = this.Width;
            ViewState["Height"] = this.Height;
            ViewState["HideDownload"] = this.HideDownload;
            ViewState["HideControls"] = this.HideControls;
            ViewState["Placemarks"] = this._Placemarks;
            ViewState["RadiusLoaders"] = this._RadiusLoaders;

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


            //
            // Create the KML object to work with.
            //
            google = new Google(ArenaContext.Current.User, BaseUrl());
            kml = new KML(BaseUrl());

            //
            // Add in each placemark.
            //
            foreach (Placemark placemark in Placemarks)
            {
                if (typeof(PersonPlacemark).IsInstanceOfType(placemark))
                {
                    kml.AddPersonPlacemark(((PersonPlacemark)placemark).Person);
                }
                else if (typeof(FamilyPlacemark).IsInstanceOfType(placemark))
                {
                    kml.AddFamilyPlacemark(((FamilyPlacemark)placemark).Family);
                }
                else if (typeof(SmallGroupPlacemark).IsInstanceOfType(placemark))
                {
                    kml.AddSmallGroupPlacemark(((SmallGroupPlacemark)placemark).Group);
                }
                else
                {
                }
            }

            //
            // Run through each RadiusLoader and process it.
            //
            foreach (RadiusLoader loader in RadiusLoaders)
            {
                if (loader.LoaderType == RadiusLoaderType.Individuals)
                {
                    List<PersonPlacemark> items;

                    items = google.PersonPlacemarksInRadius(loader.Latitude, loader.Longitude, loader.Distance, 0, Int32.MaxValue);
                    foreach (PersonPlacemark p in items)
                    {
                        kml.AddPersonPlacemark(p.Person);
                    }
                }
                else if (loader.LoaderType == RadiusLoaderType.Families)
                {
                    List<FamilyPlacemark> items;

                    items = google.FamilyPlacemarksInRadius(loader.Latitude, loader.Longitude, loader.Distance, 0, Int32.MaxValue);
                    foreach (FamilyPlacemark p in items)
                    {
                        kml.AddFamilyPlacemark(p.Family);
                    }
                }
                else if (loader.LoaderType == RadiusLoaderType.SmallGroups)
                {
                    List<SmallGroupPlacemark> items;

                    items = google.SmallGroupPlacemarksInRadius(loader.Latitude, loader.Longitude, loader.Distance, 0, Int32.MaxValue);
                    foreach (SmallGroupPlacemark p in items)
                    {
                        kml.AddSmallGroupPlacemark(p.Group);
                    }
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
                script.AppendLine("        marker = new google.maps.Marker({" +
                    "icon: \"" + placemark.PinImage + "\"" +
                    ",position: new google.maps.LatLng(" + placemark.Latitude.ToString() + "," + placemark.Longitude.ToString() + ")" +
                    ",map: " + this.ClientObject + ".map" +
                    ",title: \"" + placemark.Name + "\"" +
                    "});");
            }
        }


        /// <summary>
        /// Render the Javascript needed to put all radius loaders onto the map. These
        /// render multiple placemarks based on distance from a central address.
        /// </summary>
        /// <param name="script">The script string to append our JS to.</param>
        private void RenderRadiusLoaders(StringBuilder script)
        {
            foreach (RadiusLoader loader in _RadiusLoaders)
            {
                if (loader.LoaderType == RadiusLoaderType.Families)
                {
                    //
                    // This RadiusLoader is loading family units.
                    //
                    script.AppendLine("        " + this.ClientObject + ".LoadFamiliesInGeoRadius(" +
                        "new GeoAddress(" + loader.Latitude.ToString() + "," + loader.Longitude.ToString() + ")," + loader.Distance.ToString() + ",null);");
                }
                else if (loader.LoaderType == RadiusLoaderType.Individuals)
                {
                    //
                    // This RadiusLoader is loading individuals.
                    //
                    script.AppendLine("        " + this.ClientObject + ".LoadPeopleInGeoRadius(" +
                        "new GeoAddress(" + loader.Latitude.ToString() + "," + loader.Longitude.ToString() + ")," + loader.Distance.ToString() + ",null);");
                }
                else if (loader.LoaderType == RadiusLoaderType.SmallGroups)
                {
                    //
                    // This RadiusLoader is loading small groups.
                    //
                    script.AppendLine("        " + this.ClientObject + ".LoadGroupsInGeoRadius(" +
                        "new GeoAddress(" + loader.Latitude.ToString() + "," + loader.Longitude.ToString() + ")," + loader.Distance.ToString() + ",null);");
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
            this.RadiusLoaders.Clear();
        }

        #endregion
    }
}
