using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Organization;
using Arena.Portal;
using Arena.Portal.UI;
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
    public partial class PeopleInRange : PortalControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlType.Items.Add(new ListItem("People", RadiusLoaderType.Individuals.ToString()));
                ddlType.Items.Add(new ListItem("Families", RadiusLoaderType.Families.ToString()));
                ddlType.Items.Add(new ListItem("Small Groups", RadiusLoaderType.SmallGroups.ToString()));
                ddlType.SelectedIndex = 0;
            }
        }

        public void btnPopulate_Click(object sender, EventArgs e)
        {
            Address address;


            //
            // Geocode the address.
            //
            address = new Address();
            address.StreetLine1 = txtAddress.Text;
            address.City = txtCity.Text;
            address.State = txtState.Text;
            address.PostalCode = txtPostal.Text;
            address.Geocode("GoogleMaps");

            if (address.Latitude != 0 && address.Longitude != 0)
            {
                Placemark placemark;
                RadiusLoader loader;

                
                //
                // Clear the map.
                //
                myMap.ClearContent();

                //
                // Put the center placemark if they want one.
                //
                if (cbShowHome.Checked == true)
                {
                    placemark = new Placemark();
                    placemark.Latitude = address.Latitude;
                    placemark.Longitude = address.Longitude;
                    placemark.Unique = "Home";
                    placemark.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=home|FFFF00";
                    placemark.Name = address.StreetLine1 + "\\n" + address.City + ", " + address.State + " " + address.PostalCode;
                    myMap.Placemarks.Add(placemark);
                }

                //
                // Put all the campuses if those are requested.
                //
                if (cbShowCampus.Checked == true)
                {
                    CampusCollection campuses = new CampusCollection(ArenaContext.Current.Organization.OrganizationID);

                    foreach (Campus c in campuses)
                    {
                        if (c.Address != null && c.Address.Latitude != 0 && c.Address.Longitude != 0)
                        {
                            placemark = new Placemark();
                            placemark.Latitude = c.Address.Latitude;
                            placemark.Longitude = c.Address.Longitude;
                            placemark.Unique = c.Name;
                            placemark.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=home|FFFF00";
                            placemark.Name = c.Name + "\\n" + c.Address.StreetLine1 + "\\n" + c.Address.City + ", " + c.Address.State + " " + c.Address.PostalCode;
                            myMap.Placemarks.Add(placemark);
                        }
                    }
                }

                //
                // Build a loader of the apropriate type.
                //
                loader = new RadiusLoader();
                loader.Latitude = address.Latitude;
                loader.Longitude = address.Longitude;
                loader.Distance = Convert.ToDouble(txtDistance.Text);
                loader.LoaderType = (RadiusLoaderType)Enum.Parse(typeof(RadiusLoaderType), ddlType.SelectedValue);
                myMap.RadiusLoaders.Add(loader);
            }
            else
            {
                ltError.Text = "Could not geocode address, please try entering it again.";
            }
        }
    }
}