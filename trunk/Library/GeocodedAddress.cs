using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.HDC.GoogleMaps
{
    /// <summary>
    /// Generic holder class that defines the Latitude and Longitude of an address.
    /// </summary>
    public class GeocodedAddress
    {
        /// <summary>
        /// The latitude of the address.
        /// </summary>
        public Double Latitude;

        /// <summary>
        /// The longitude of the address.
        /// </summary>
        public Double Longitude;


        /// <summary>
        /// Generic constructor.
        /// </summary>
        public GeocodedAddress()
        {
        }
    }
}
