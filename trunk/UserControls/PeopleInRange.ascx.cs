using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
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
            Placemark placemark;

            placemark = new Placemark();
            placemark.Latitude = 34.5212;
            placemark.Longitude = -117.3456;
            placemark.Unique = "";
            placemark.PinImage = "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=home|FFFF00";
            placemark.Name = "Home";
            myMap.Placemarks.Add(placemark);
        }

        public void btnPopulate_Click(object sender, EventArgs e)
        {
            Address address;
            RadiusLoader loader;


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
                loader = new RadiusLoader();
                loader.Latitude = address.Latitude;
                loader.Longitude = address.Longitude;
                loader.Distance = Convert.ToDouble(txtDistance.Text);
                myMap.RadiusLoaders.Add(loader);
            }
        }
    }
}