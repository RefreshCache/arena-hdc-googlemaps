using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps
{
    [Serializable]
    public class RadiusLoader
    {
        public Double Latitude, Longitude;
        public Double Distance;
        public RadiusLoaderType LoaderType;


        public RadiusLoader()
        {
            this.Latitude = 0;
            this.Longitude = 0;
            this.Distance = 0;
            this.LoaderType = RadiusLoaderType.Individuals;
        }
    }


    public enum RadiusLoaderType
    {
        Individuals = 0,
        Families = 1
    }
}
