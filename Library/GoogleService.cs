using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

using Arena.Core;
using Arena.Security;
using Arena.Custom.HDC.GoogleMaps.Maps;


namespace Arena.Custom.HDC.GoogleMaps
{
    /// <summary>
    /// The GoogleService WebService provides a way to interface your UserControl with the Google Maps API
    /// and the Arena database via a Javascript interface.
    /// </summary>
    [WebService(Namespace = "Arena.Custom.HDC.GoogleMaps.GoogleService")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class GoogleService : System.Web.Services.WebService
    {
        /// <summary>
        /// Retrieve the information to be displayed in the person details popup.
        /// </summary>
        /// <param name="personID">The GUID of the person in question.</param>
        /// <returns>An HTML formatted string.</returns>
        [WebMethod]
        public string PersonDetailsInfoWindow(String personID)
        {
            Person p;
            Google google;


            google = new Google(ArenaContext.Current.User, HttpContext.Current.Request.ApplicationPath);

            //
            // Load the person we are trying to retrieve details for.
            //
            p = new Person(new Guid(personID));

            return google.PersonDetailsPopup(p, true, true);
        }


        /// <summary>
        /// Retrieve the information to be displayed in the family details popup.
        /// </summary>
        /// <param name="personID">The ID of the family in question.</param>
        /// <returns>An HTML formatted string.</returns>
        [WebMethod]
        public string FamilyDetailsInfoWindow(String familyID)
        {
            Family f;
            Google google;


            google = new Google(ArenaContext.Current.User, HttpContext.Current.Request.ApplicationPath);

            //
            // Load the family we are trying to retrieve details for.
            //
            f = new Family(Convert.ToInt32(familyID));

            return google.FamilyDetailsPopup(f, true);
        }


        /// <summary>
        /// Get a list of people in the given profile ID, constraining the list to a specified
        /// subset of people.
        /// </summary>
        /// <param name="profileID">The ID number of the profile.</param>
        /// <param name="start">The starting 0-based index to begin loading at.</param>
        /// <param name="count">The maximum number of people to load.</param>
        /// <returns>An array of PersonPlacemark objects which identify the members of the profile.</returns>
        [WebMethod]
        public List<PersonPlacemark> LoadPeopleFromProfile(String profileID, String start, String count)
        {
            Google google;


            google = new Google(ArenaContext.Current.User, HttpContext.Current.Request.ApplicationPath);

            return google.PersonPlacemarksFromProfile(new Profile(Convert.ToInt32(profileID)),
                (String.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start)),
                (String.IsNullOrEmpty(count) ? Int32.MaxValue : Convert.ToInt32(count)));
        }


        /// <summary>
        /// Request that the given address be geocoded. If the address cannot be geocoded then the latitude
        /// and longitude are returned as 0,0.
        /// </summary>
        /// <param name="street">The street number and name of the address.</param>
        /// <param name="city">The city the address is located in.</param>
        /// <param name="state">The state as a 2 letter designation, i.e. CA.</param>
        /// <param name="postal">The zipcode of the address.</param>
        /// <returns>A latitude/longitude pair encoded as a GeocodedAddress object.</returns>
        [WebMethod]
        public GeocodedAddress GeocodeAddress(String street, String city, String state, String postal)
        {
            GeocodedAddress geocoded;
            Address address;


            //
            // Geocode the address.
            //
            address = new Address();
            address.StreetLine1 = street;
            address.City = city;
            address.State = state;
            address.PostalCode = postal;
            address.Geocode("GoogleMaps API");

            //
            // Convert the latitude/longitude into a GeocodedAddress result.
            //
            geocoded = new GeocodedAddress();
            geocoded.Latitude = address.Latitude;
            geocoded.Longitude = address.Longitude;

            return geocoded;
        }


        /// <summary>
        /// Retrieve a list of people who are inside the radius of the given latitude
        /// and longitude coordinates.
        /// </summary>
        /// <param name="latitude">The latitude of the center point to search from.</param>
        /// <param name="longitude">The longitude of the center point to search from.</param>
        /// <param name="distance">The distance to search out from the center point.</param>
        /// <param name="start">The starting index of the records to retrieve. To retrieve all records pass 0.</param>
        /// <param name="count">The number of records to retrieve. To retrieve all records pass an empty string. Due to a limitation in Microsofts JSON implementation you should not retrieve more than 100 records at a time.</param>
        /// <returns>A list of PersonPlcemark objects that can be placed on the map.</returns>
        [WebMethod]
        public List<PersonPlacemark> LoadPeopleInRadius(Double latitude, Double longitude, Double distance, String start, String count)
        {
            Google google;


            google = new Google(ArenaContext.Current.User, HttpContext.Current.Request.ApplicationPath);

            return google.PersonPlacemarksInRadius(latitude, longitude, distance,
                (String.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start)),
                (String.IsNullOrEmpty(count) ? Int32.MaxValue : Convert.ToInt32(count)));
        }


        /// <summary>
        /// Load a collection of families who are in range of the specific coordinates.
        /// </summary>
        /// <param name="latitude">The latitude coordinate of the center point to search from.</param>
        /// <param name="longitude">The longitude of the coordinate of the center point to search from.</param>
        /// <param name="distance">The distance out to search from the center point.</param>
        /// <param name="start">The starting index of the records to retrieve. To retrieve all records pass 0.</param>
        /// <param name="count">The number of records to retrieve. Pass an empty string for all records. Due to a limitation in Microsofts JSON implementation you should not retrieve more than 100 records at a time.</param>
        /// <returns>A list of FamilyPlacemarks that can be placed on a google map.</returns>
        [WebMethod]
        public List<FamilyPlacemark> LoadFamiliesInRadius(Double latitude, Double longitude, Double distance, String start, String count)
        {
            Google google;


            google = new Google(ArenaContext.Current.User, HttpContext.Current.Request.ApplicationPath);

            return google.FamilyPlacemarksInRadius(latitude, longitude, distance,
                (String.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start)),
                (String.IsNullOrEmpty(count) ? Int32.MaxValue : Convert.ToInt32(count)));
        }

        
        /// <summary>
        /// Load a collection of small groups who are in range of the specific coordinates.
        /// </summary>
        /// <param name="latitude">The latitude coordinate of the center point to search from.</param>
        /// <param name="longitude">The longitude of the coordinate of the center point to search from.</param>
        /// <param name="distance">The distance out to search from the center point.</param>
        /// <param name="start">The starting index of the records to retrieve. To retrieve all records pass 0.</param>
        /// <param name="count">The number of records to retrieve. Pass an empty string for all records. Due to a limitation in Microsofts JSON implementation you should not retrieve more than 100 records at a time.</param>
        /// <returns>A list of SmallGroupPlacemarks that can be placed on a google map.</returns>
        [WebMethod]
        public List<SmallGroupPlacemark> LoadGroupsInRadius(Double latitude, Double longitude, Double distance, String start, String count)
        {
            Google google;


            google = new Google(ArenaContext.Current.User, HttpContext.Current.Request.ApplicationPath);

            return google.SmallGroupPlacemarksInRadius(latitude, longitude, distance,
                (String.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start)),
                (String.IsNullOrEmpty(count) ? Int32.MaxValue : Convert.ToInt32(count)));
        }
    }
}
