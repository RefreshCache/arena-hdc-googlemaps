using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Arena.Custom.HDC.GoogleMaps.Maps
{
    public class Placemark
    {
        public String Name, Unique;
        public Double Latitude, Longitude;


        public Placemark()
        {
            this.Name = "";
            this.Unique = "";
            this.Latitude = 0;
            this.Longitude = 0;
        }


        protected Placemark(String name, String unique, double latitude, double longitude)
        {
            this.Name = name;
            this.Unique = unique;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }
}
