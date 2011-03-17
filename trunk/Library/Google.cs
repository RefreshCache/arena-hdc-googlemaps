using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

using Arena.Core;
using Arena.List;
using Arena.DataLayer.Core;
using Arena.SmallGroup;
using Arena.Security;
using Arena.Custom.HDC.GoogleMaps.Maps;


namespace Arena.Custom.HDC.GoogleMaps
{
    /// <summary>
    /// The Google class provides all the methods for loading information to be used with
    /// either Google Maps or Google Earth. All the information returned should be generic
    /// enough to use with either. Google Earth export requires a little bit more information
    /// due to the KML format, but helper methods are provided.
    /// </summary>
    public class Google
    {
        #region Properties

        /// <summary>
        /// Specifies the relative URL to use when generating links to things
        /// in Arena, for example https://www.highdesertchurch.com/Arena
        /// </summary>
        public String ArenaUrl
        {
            set
            {
                if (value == null || value[value.Length - 1] == '/')
                    _ArenaUrl = value;
                else
                    _ArenaUrl = value + '/';
            }
            get { return _ArenaUrl; }
        }
        private String _ArenaUrl;

        private GenericPrincipal _User;

        #endregion


        #region Constructors

        /// <summary>
        /// The Google class provides pretty much the core commands to interface
        /// and prepare the information for use.
        /// </summary>
        /// <param name="baseUrl">The URL to Arena, see the ArenaUrl property.</param>
        public Google(GenericPrincipal user, String baseUrl)
        {
            this._User = user;
            this.ArenaUrl = baseUrl;
        }

        #endregion


        #region ProfileLoader Methods

        /// <summary>
        /// Load a list of person placemark objects from the given profile ID. The
        /// list is constrained to the start and count parameters.
        /// </summary>
        /// <param name="profileid">The Arena Profile to generate a list of people from, only active members are returned.</param>
        /// <param name="start">The member index to start loading from.</param>
        /// <param name="count">The maximum number of people to load, pass Int32.MaxValue for complete load.</param>
        /// <returns>A list of PersonPlacemark objects.</returns>
        public List<PersonPlacemark> PersonPlacemarksInProfile(int profileid, int start, int count)
        {
            List<PersonPlacemark> people = new List<PersonPlacemark>();
            ProfileMember p;
            Profile profile;
            int i;


            if (PermissionsOperationAllowed(new PermissionCollection(ObjectType.Tag, profileid), OperationType.View) == false)
                return people;

            profile = new Profile(profileid);
            for (i = start; i < profile.Members.Count && people.Count < count; i++)
            {
                p = profile.Members[i];

                if (p.Status.Qualifier != "D")
                {
                    try
                    {
                        people.Add(new PersonPlacemark(p));
                    }
                    catch { }
                }
            }

            return people;
        }

        #endregion


        #region CategoryLoader Methods

        /// <summary>
        /// Load a list of person placemark objects from the given category ID. The
        /// list is constrained to the start and count parameters.
        /// </summary>
        /// <param name="categoryid">The Arena Category to generate a list of people from, only active members are returned.</param>
        /// <param name="start">The member index to start loading from.</param>
        /// <param name="count">The maximum number of people to load, pass Int32.MaxValue for complete load.</param>
        /// <returns>A list of PersonPlacemark objects.</returns>
        public List<PersonPlacemark> PersonPlacemarksInCategory(int categoryid, int start, int count)
        {
            GroupClusterCollection gcc;
            List<PersonPlacemark> people = new List<PersonPlacemark>();
            int i, p;


            gcc = new GroupClusterCollection(categoryid, ArenaContext.Current.Organization.OrganizationID);
            for (i = 0; i < gcc.Count && people.Count < count; i++)
            {
                List<PersonPlacemark> list = PersonPlacemarksInCluster(gcc[i].GroupClusterID, 0, Int32.MaxValue);

                for (p = 0; p < list.Count && people.Count < count; p++)
                {
                    if (people.Contains(list[p]))
                        continue;

                    if (start-- > 0)
                        continue;

                    people.Add(list[p]);
                }
            }

            return people;
        }

        #endregion


        #region ClusterLoader Methods

        /// <summary>
        /// Load a list of person placemark objects from the given cluster ID. The
        /// list is constrained to the start and count parameters.
        /// </summary>
        /// <param name="clusterid">The Arena Cluster to generate a list of people from, only active members are returned.</param>
        /// <param name="start">The member index to start loading from.</param>
        /// <param name="count">The maximum number of people to load, pass Int32.MaxValue for complete load.</param>
        /// <returns>A list of PersonPlacemark objects.</returns>
        public List<PersonPlacemark> PersonPlacemarksInCluster(int clusterid, int start, int count)
        {
            List<PersonPlacemark> people = new List<PersonPlacemark>();
            List<Int32> peopleids = new List<Int32>();
            GroupCluster cluster;
            int i;


            if (PermissionsOperationAllowed(new PermissionCollection(ObjectType.Group_Cluster, clusterid), OperationType.View) == false)
                return people;

            cluster = new GroupCluster(clusterid);

            //
            // This is one ugly method, but someday God will grace us with his presence and
            // say "thou shalt not use they Small Group Structure."
            //
            cluster.PopulateDescendents();
            Thread.Sleep(2000);
            while (cluster.WaitingForCount)
                Thread.Sleep(50);

            foreach (Person p in cluster.ActiveChildGroupLeaders)
            {
                if (peopleids.Contains(p.PersonID) == false)
                    peopleids.Add(p.PersonID);
            }
            foreach (GroupMember gm in cluster.ActiveChildGroupMembers)
            {
                if (peopleids.Contains(gm.PersonID) == false)
                    peopleids.Add(gm.PersonID);
            }

            for (i = start; i < peopleids.Count && people.Count < count; i++)
            {
                try
                {
                    people.Add(new PersonPlacemark(new Person(peopleids[i])));
                }
                catch { }
            }

            return people;
        }

        #endregion


        #region GroupLoader Methods

        /// <summary>
        /// Load a list of person placemark objects from the given group ID. The
        /// list is constrained to the start and count parameters.
        /// </summary>
        /// <param name="groupid">The Arena Group to generate a list of people from, only active members are returned.</param>
        /// <param name="start">The member index to start loading from.</param>
        /// <param name="count">The maximum number of people to load, pass Int32.MaxValue for complete load.</param>
        /// <returns>A list of PersonPlacemark objects.</returns>
        public List<PersonPlacemark> PersonPlacemarksInGroup(int groupid, int start, int count)
        {
            List<PersonPlacemark> people = new List<PersonPlacemark>();
            List<Int32> peopleids = new List<Int32>();
            Group group;
            int i;


            group = new Group(groupid);

            if (PermissionsOperationAllowed(new PermissionCollection(ObjectType.Group_Cluster, group.GroupClusterID), OperationType.View) == false)
                return people;

            //
            // Add in the leader of this group.
            //
            if (group.Leader != null && group.Leader.PersonID != -1)
            {
                try
                {
                    people.Add(new PersonPlacemark(group.Leader));
                }
                catch { }
            }

            for (i = start; i < group.Members.Count && people.Count < count; i++)
            {
                try
                {
                    people.Add(new PersonPlacemark(group.Members[i]));
                }
                catch { }
            }

            return people;
        }

        #endregion


        #region ReportLoader Methods

        /// <summary>
        /// Load a list of person placemark objects from the given report ID. The
        /// list is constrained to the start and count parameters.
        /// </summary>
        /// <param name="reportid">The Arena Report to generate a list of people from.</param>
        /// <param name="start">The member index to start loading from.</param>
        /// <param name="count">The maximum number of people to load, pass Int32.MaxValue for complete load.</param>
        /// <returns>A list of PersonPlacemark objects.</returns>
        public List<PersonPlacemark> PersonPlacemarksInReport(int reportid, int start, int count)
        {
            List<PersonPlacemark> people = new List<PersonPlacemark>();
            SqlDataReader rdr;
            ListReport report;


            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.View) == false)
                return people;

            report = new ListReport(reportid);
            rdr = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(report.Query);
            people = PersonPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return people;
        }

        #endregion


        #region RadiusLoader Methods

        /// <summary>
        /// Retrieve a list of PersonPlacemark objects that are in the designated radius of the given
        /// latitude and longitude coordinates. Possibly retrieve only a subset of those individuals.
        /// </summary>
        /// <param name="latitude">The latitude of the center point.</param>
        /// <param name="longitude">The longitude of the center point.</param>
        /// <param name="radius">The distance from the center point to look up to.</param>
        /// <param name="start">The 0-based starting index of the records to retrieve.</param>
        /// <param name="count">The maximum number of records to retrieve. If the returned number is less than this number then no more records are available.</param>
        /// <returns>List of PersonPlacemarks that identify people in range.</returns>
        public List<PersonPlacemark> PersonPlacemarksInRadius(Double latitude, Double longitude, Double radius, int start, int count)
        {
            List<PersonPlacemark> people;
            SqlConnection con;
            SqlDataReader rdr;
            SqlCommand cmd;


            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.View) == false)
                return new List<PersonPlacemark>();

            if (latitude == 0 && longitude == 0)
                throw new ArgumentException("Address has not been geocoded.");

            //
            // Prepare the SQL query to request all people within a radius of an address.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT(cpa.person_id) FROM core_person_address AS cpa" +
                " LEFT JOIN core_address AS ca ON ca.address_id = cpa.address_id" +
                " LEFT JOIN core_person AS cp ON cp.person_id = cpa.person_id" +
                " WHERE cpa.primary_address = 1" +
                " AND dbo.cust_hdc_googlemaps_funct_distance_between(@LatFrom, @LongFrom, ca.Latitude, ca.Longitude) < @Distance" +
                " AND cp.record_status = 0" +
                " ORDER BY cpa.person_id";
            cmd.Parameters.Add(new SqlParameter("@LatFrom", latitude));
            cmd.Parameters.Add(new SqlParameter("@LongFrom", longitude));
            cmd.Parameters.Add(new SqlParameter("@Distance", radius));

            //
            // Execute the reader and process all results.
            //
            rdr = cmd.ExecuteReader();
            people = PersonPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return people;
        }

        /// <summary>
        /// Retrieve a list of FamilyPlacemark objects that are in the designated radius of the given
        /// latitude and longitude coordinates. Possibly retrieve only a subet of those individuals.
        /// </summary>
        /// <param name="latitude">The latitude of the center point.</param>
        /// <param name="longitude">The longitude of the center point.</param>
        /// <param name="radius">The distance from the center point to look up to.</param>
        /// <param name="start">The 0-based starting index of the records to retrieve.</param>
        /// <param name="count">The maximum number of records to retrieve. If the returned number is less than this number then no more records are available.</param>
        /// <returns>List of FamilyPlacemarks that identify families in range.</returns>
        public List<FamilyPlacemark> FamilyPlacemarksInRadius(Double latitude, Double longitude, Double radius, int start, int count)
        {
            List<FamilyPlacemark> families;
            SqlConnection con;
            SqlDataReader rdr;
            SqlCommand cmd;


            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.View) == false)
                return new List<FamilyPlacemark>();

            if (latitude == 0 && longitude == 0)
                throw new ArgumentException("Address has not been geocoded.");

            //
            // Prepare the SQL query to request all people within a radius of an address.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT(cfm.family_id) FROM core_family_member AS cfm" +
                " LEFT JOIN core_person_address AS cpa ON cpa.person_id = cfm.person_id" +
                " LEFT JOIN core_address AS ca ON ca.address_id = cpa.address_id" +
                " LEFT JOIN core_person AS cp ON cp.person_id = cfm.person_id" +
                " WHERE cpa.primary_address = 1" +
                " AND dbo.cust_hdc_googlemaps_funct_distance_between(@LatFrom, @LongFrom, ca.Latitude, ca.Longitude) < @Distance" +
                " AND cp.record_status = 0" +
                " ORDER BY cfm.family_id";
            cmd.Parameters.Add(new SqlParameter("@LatFrom", latitude));
            cmd.Parameters.Add(new SqlParameter("@LongFrom", longitude));
            cmd.Parameters.Add(new SqlParameter("@Distance", radius));

            //
            // Execute the reader and process all results.
            rdr = cmd.ExecuteReader();
            families = FamilyPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return families;
        }

        /// <summary>
        /// Retrieve a list of SmallGroupPlacemark objects that are in the designated radius of the given
        /// latitude and longitude coordinates. Possibly retrieve only a subet of those.
        /// </summary>
        /// <param name="latitude">The latitude of the center point.</param>
        /// <param name="longitude">The longitude of the center point.</param>
        /// <param name="radius">The distance from the center point to look up to.</param>
        /// <param name="start">The 0-based starting index of the records to retrieve.</param>
        /// <param name="count">The maximum number of records to retrieve. If the returned number is less than this number then no more records are available.</param>
        /// <returns>List of SmallGroupPlacemarks that identify groups in range.</returns>
        public List<SmallGroupPlacemark> SmallGroupPlacemarksInRadius(Double latitude, Double longitude, Double radius, int start, int count)
        {
            List<SmallGroupPlacemark> groups;
            SqlConnection con;
            SqlDataReader rdr;
            SqlCommand cmd;


            if (latitude == 0 && longitude == 0)
                throw new ArgumentException("Address has not been geocoded.");

            //
            // Prepare the SQL query to request all small groups within a radius of an address.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT(sg.group_id) FROM smgp_group AS sg" +
                " LEFT JOIN core_person_address AS cpa ON cpa.person_id = sg.target_location_person_id" +
                " LEFT JOIN core_address AS ca ON ca.address_id = cpa.address_id" +
                " WHERE cpa.primary_address = 1" +
                " AND dbo.cust_hdc_googlemaps_funct_distance_between(@LatFrom, @LongFrom, ca.Latitude, ca.Longitude) < @Distance" +
                " AND sg.is_group_private = 0" +
                " AND sg.active = 1" +
                " ORDER BY sg.group_id";
            cmd.Parameters.Add(new SqlParameter("@LatFrom", latitude));
            cmd.Parameters.Add(new SqlParameter("@LongFrom", longitude));
            cmd.Parameters.Add(new SqlParameter("@Distance", radius));

            //
            // Execute the reader and process all results.
            rdr = cmd.ExecuteReader();
            groups = SmallGroupPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return groups;
        }

        #endregion


        #region AreaLoader Methods

        /// <summary>
        /// Retrieve a list of PersonPlacemark objects that are in the designated Arena Area.
        /// Possibly retrieve only a subset of those individuals.
        /// </summary>
        /// <param name="areaid">The ID number of the Arena Area.</param>
        /// <param name="start">The 0-based starting index of the records to retrieve.</param>
        /// <param name="count">The maximum number of records to retrieve. If the returned number is less than this number then no more records are available.</param>
        /// <returns>List of PersonPlacemarks that identify people in range.</returns>
        public List<PersonPlacemark> PersonPlacemarksInArea(int areaid, int start, int count)
        {
            List<PersonPlacemark> people;
            SqlDataReader rdr;


            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.View) == false)
                return new List<PersonPlacemark>();

            //
            // Execute the reader and process all results.
            //
            rdr = new PersonData().GetPersonByArea(areaid);
            people = PersonPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return people;
        }

        /// <summary>
        /// Retrieve a list of FamilyPlacemark objects that are in the designated Arena Area.
        /// Possibly retrieve only a subet of those individuals.
        /// </summary>
        /// <param name="areaid">The Arena area to load from.</param>
        /// <param name="start">The 0-based starting index of the records to retrieve.</param>
        /// <param name="count">The maximum number of records to retrieve. If the returned number is less than this number then no more records are available.</param>
        /// <returns>List of FamilyPlacemarks that identify families in range.</returns>
        public List<FamilyPlacemark> FamilyPlacemarksInArea(int areaid, int start, int count)
        {
            List<FamilyPlacemark> families;
            SqlConnection con;
            SqlDataReader rdr;
            SqlCommand cmd;


            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.View) == false)
                return new List<FamilyPlacemark>();

            //
            // Prepare the SQL query to request all people within a radius of an address.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT(cfm.family_id) FROM core_family_member AS cfm" +
                " LEFT JOIN core_person_address AS cpa ON cpa.person_id = cfm.person_id" +
                " LEFT JOIN core_address AS ca ON ca.address_id = cpa.address_id" +
                " LEFT JOIN core_person AS cp ON cp.person_id = cfm.person_id" +
                " WHERE cpa.primary_address = 1" +
                " AND ca.area_id = @AreaID" +
                " AND cp.record_status = 0" +
                " ORDER BY cfm.family_id";
            cmd.Parameters.Add(new SqlParameter("@AreaID", areaid));

            //
            // Execute the reader and process all results.
            rdr = cmd.ExecuteReader();
            families = FamilyPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return families;
        }

        /// <summary>
        /// Retrieve a list of SmallGroupPlacemark objects that are in the designated Arena Area.
        /// Possibly retrieve only a subet of those.
        /// </summary>
        /// <param name="areaid">The Area Id to load from.</param>
        /// <param name="start">The 0-based starting index of the records to retrieve.</param>
        /// <param name="count">The maximum number of records to retrieve. If the returned number is less than this number then no more records are available.</param>
        /// <returns>List of SmallGroupPlacemarks that identify groups in range.</returns>
        public List<SmallGroupPlacemark> SmallGroupPlacemarksInArea(int areaid, int start, int count)
        {
            List<SmallGroupPlacemark> groups;
            SqlConnection con;
            SqlDataReader rdr;
            SqlCommand cmd;


            //
            // Prepare the SQL query to request all small groups within a radius of an address.
            //
            con = new Arena.DataLib.SqlDbConnection().GetDbConnection();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT(sg.group_id) FROM smgp_group AS sg" +
                " LEFT JOIN core_person_address AS cpa ON cpa.person_id = sg.target_location_person_id" +
                " LEFT JOIN core_address AS ca ON ca.address_id = cpa.address_id" +
                " WHERE cpa.primary_address = 1" +
                " AND ca.area_id = @AreaID" +
                " AND sg.is_group_private = 0" +
                " AND sg.active = 1" +
                " ORDER BY sg.group_id";
            cmd.Parameters.Add(new SqlParameter("@AreaID", areaid));

            //
            // Execute the reader and process all results.
            rdr = cmd.ExecuteReader();
            groups = SmallGroupPlacemarksFromReader(rdr, start, count);
            rdr.Close();

            return groups;
        }

        #endregion


        #region Placemark Details

        /// <summary>
        /// Retrieve the full HTML used for a pin-popup in Maps or Earth for a single person.
        /// </summary>
        /// <param name="p">The Person object to retrieve information about.</param>
        /// <param name="includeName">Whether or not the name should be included.</param>
        /// <param name="useSecurity">If security should be enforced.</param>
        /// <returns>An HTML formatted string.</returns>
        public string PersonDetailsPopup(Person p, Boolean includeName, Boolean useSecurity)
        {
            String info;
            Boolean first = true;


            //
            // Load the basic person information and picture.
            //
            info = "<div style=\"font-size: 12px;\">";
            info += PersonInfo(p, includeName, useSecurity);

            //
            // Include the address, if security allowed.
            //
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Addresses, OperationType.View))
            {
                if (first)
                {
                    info += "<hr />";
                    first = false;
                }

                info += p.PrimaryAddress.StreetLine1 + "<br />" +
                        (String.IsNullOrEmpty(p.PrimaryAddress.StreetLine2) ? "" : p.PrimaryAddress.StreetLine2 + "<br />") +
                        p.PrimaryAddress.City + ", " + p.PrimaryAddress.State + " " + p.PrimaryAddress.PostalCode;
            }

            //
            // Include the phone numbers, if security allows it.
            //
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Phones, OperationType.View))
            {
                if (first)
                {
                    info += "<hr />";
                    first = false;
                }
                else
                    info += "<br /><br />";

                info += PersonPhoneNumbers(p, true).ToString();
            }

            info += "</div>";

            return info;
        }

        /// <summary>
        /// Retrieve the full HTML used for a pin-popup in Maps or Earth for a family unit.
        /// </summary>
        /// <param name="f">The Family object to retrieve information about.</param>
        /// <param name="useSecurity">If security should be enforced.</param>
        /// <returns>An HTML formatted string.</returns>
        public string FamilyDetailsPopup(Family f, Boolean includeName, Boolean useSecurity)
        {
            String personInfo;
            Person head = f.FamilyHead;


            personInfo = "<div style=\"font-size: 12px;\">";
            if (includeName)
                personInfo += "<div style=\"text-align: center; margin-bottom: 4px;\"><b>" + f.FamilyName + "</b></div>";

            //
            // Store address information.
            //
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Addresses, OperationType.View))
            {
                personInfo += head.PrimaryAddress.StreetLine1 + "<br />" +
                    (String.IsNullOrEmpty(head.PrimaryAddress.StreetLine2) ? "" : head.PrimaryAddress.StreetLine2 + "<br />") +
                    head.PrimaryAddress.City + ", " + head.PrimaryAddress.State + " " + head.PrimaryAddress.PostalCode + "<br />";
            }

            //
            // Store the family phone numbers.
            //
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Phones, OperationType.View))
                personInfo += FamilyPhoneNumbers(f);

            foreach (Person p in f.FamilyMembers)
            {
                String phones;

                //
                // Build up the person information and phone numbers.
                //
                personInfo += "<hr width=\"75%\"/>";
                personInfo += PersonInfo(p, true, true);

                if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Phones, OperationType.View))
                {
                    phones = PersonPhoneNumbers(p, false);
                    if (phones.Length > 0)
                        personInfo += phones;
                }
            }

            personInfo += "</div>";

            return personInfo;
        }

        /// <summary>
        /// Retrieve the full HTML used for a pin-popup in Maps or Earth for a family unit.
        /// </summary>
        /// <param name="f">The Family object to retrieve information about.</param>
        /// <param name="useSecurity">If security should be enforced.</param>
        /// <returns>An HTML formatted string.</returns>
        public string SmallGroupDetailsPopup(Group g, Boolean includeName, Boolean useSecurity)
        {
            String info;


            //
            // Load in the group name and picture, if available.
            //
            info = "<div style=\"font-size: 12px;\">";
            if (includeName)
                info += String.Format("<p style=\"text-align: center;\"><b>{0}</b></p>", g.Name);
            if (g.ImageBlob != null && g.ImageBlob.BlobID != -1)
                info += String.Format("<p align=\"center\"><img src=\"{0}\" /></p>", ArenaUrl + "CachedBlob.aspx?guid=" + g.ImageBlob.GUID.ToString() + "&width=100&height=100");

            //
            // Load in the basic group details.
            //
            info += "<table>";
            info += String.Format("<tr><td>{0}:</td><td>{1} {2}</td></tr>", g.ClusterType.Category.MeetingDayCaption, g.MeetingDay, g.MeetingStartTime.ToString("t"));
            info += String.Format("<tr><td>{0}:</td><td>{1}</td></tr>", g.ClusterType.Category.TypeCaption, g.GroupType.ToString());
            info += String.Format("<tr><td>{0}:</td><td>{1}</td></tr>", g.ClusterType.Category.TopicCaption, g.Topic);
            info += String.Format("<tr><td>{0}:</td><td>{1}</td></tr>", "Average Age", g.AverageAge.ToString());
            if (!String.IsNullOrEmpty(g.Notes))
                info += String.Format("<tr><td colspan=\"2\">Notes:<br />{0}</td></tr>", g.Notes);
            info += "</table></div>";

            return info;
        }

        #endregion


        #region Misc API Methods

        /// <summary>
        /// Create an HTML formatted string that contains the relavent person
        /// information.
        /// </summary>
        /// <param name="p">The person object to retrieve information about.</param>
        /// <param name="includeName">Wether or not to include the name of the person.</param>
        /// <returns>An HTML formatted string.</returns>
        public string PersonInfo(Person p, Boolean includeName, Boolean useSecurity)
        {
            String personInfo = "";


            //
            // Build up the person information.
            //
            personInfo = "<table>";
            if (includeName)
                personInfo += "<tr><td align=\"center\" colspan=\"2\"><b>" + p.FullName + "</b></td></tr>";

            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Member_Status, OperationType.View))
                personInfo += "<tr><td>Member Status:</td><td>" + p.MemberStatus.Value + "</td></tr>";
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.View))
                personInfo += "<tr><td>Gender:</td><td>" + p.Gender.ToString() + "</td></tr>";
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Marital_Status, OperationType.View))
                personInfo += "<tr><td>Marital Status:</td><td>" + p.MaritalStatus.Value + "</td></tr>";
            if (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Age, OperationType.View))
                personInfo += "<tr><td>Age:</td><td>" + (p.Age != -1 ? p.Age.ToString() : "<i>Unknown</i>") + "</td></tr>";

            personInfo += "</table>";

            if (p.BlobID != -1 && (useSecurity == false || PersonFieldOperationAllowed(PersonFields.Profile_Photo, OperationType.View)))
            {
                personInfo = "<table width=\"100%\"><tr>" +
                    "</td><td valign=\"top\">" + personInfo + "</td>" +
                    "<td width=\"100px\"><img src=\"" + ArenaUrl + "CachedBlob.aspx?guid=" + p.Blob.GUID.ToString() + "&width=100&height=100\" />" +
                    "</tr></table>";
            }

            return personInfo;
        }

        /// <summary>
        /// Build an HTML formatted string that contains all personal phone numbers
        /// for the given person object.
        /// </summary>
        /// <param name="p">The person whose phone numbers we want.</param>
        /// <param name="includeFamily">Wether or not to include family numbers for this person.</param>
        /// <returns>An HTML formatted string.</returns>
        public string PersonPhoneNumbers(Person p, Boolean includeFamily)
        {
            StringBuilder phoneStrings = new StringBuilder();


            //
            // Build up the phone numbers.
            //
            foreach (PersonPhone phone in p.Phones)
            {
                //
                // Skip unlisted, empty or family (propagated) numbers.
                //
                if (phone.Unlisted == true || phone.Number.Length == 0 || (includeFamily == false && phone.PhoneType.Qualifier.Contains("propagate") == true))
                    continue;

                if (String.IsNullOrEmpty(phone.Extension))
                    phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + "<br />");
                else
                    phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + " x" + phone.Extension + "<br />");
            }

            return phoneStrings.ToString();
        }

        /// <summary>
        /// Build an HTML formatted string that contains all shared family numbers
        /// for the given family object.
        /// </summary>
        /// <param name="f">The family whose members will be searched for phone numbers.</param>
        /// <returns>An HTML formatted string.</returns>
        public string FamilyPhoneNumbers(Family f)
        {
            StringBuilder phoneStrings = new StringBuilder();
            ArrayList phones = new ArrayList();


            foreach (Person p in f.FamilyMembers)
            {
                //
                // Build up the phone numbers.
                //
                foreach (PersonPhone phone in p.Phones)
                {
                    //
                    // Skip unlisted, empty or non-family (propagated) numbers. Also skip
                    // any numbers we have already dealt with.
                    //
                    if (phone.Unlisted == true || phone.Number.Length == 0 ||
                        phone.PhoneType.Qualifier.Contains("propagate") == false || phones.Contains(phone.Number) == true)
                        continue;

                    if (String.IsNullOrEmpty(phone.Extension))
                        phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + "<br />");
                    else
                        phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + " x" + phone.Extension + "<br />");

                    phones.Add(phone.Number);
                }
            }

            return phoneStrings.ToString();
        }

        #endregion


        #region Private Support Methods

        /// <summary>
        /// Load a collection of PersonPlacemark objects from the SqlDataReader object. Only read
        /// 'count' items starting at 'start' index.
        /// </summary>
        /// <param name="rdr">The SqlDataReader to use.</param>
        /// <param name="start">The starting index to load placemarks.</param>
        /// <param name="count">The maximum number of placemarks to load.</param>
        /// <returns>A collection of PersonPlacemark objects.</returns>
        private List<PersonPlacemark> PersonPlacemarksFromReader(SqlDataReader rdr, int start, int count)
        {
            List<PersonPlacemark> people = new List<PersonPlacemark>();
            int i;

            
            try
            {
                i = 0;
                while (rdr.Read() && people.Count < count)
                {
                    if (i++ < start)
                        continue;

                    try
                    {
                        people.Add(new PersonPlacemark(new Person(Convert.ToInt32(rdr["person_id"]))));
                    }
                    catch { }
                }
            }
            catch { }

            return people;
        }

        /// <summary>
        /// Load a collection of FamilyPlacemark objects from the SqlDataReader object. Only read
        /// 'count' items starting at 'start' index.
        /// </summary>
        /// <param name="rdr">The SqlDataReader to use.</param>
        /// <param name="start">The starting index to load placemarks.</param>
        /// <param name="count">The maximum number of placemarks to load.</param>
        /// <returns>A collection of FamilyPlacemark objects.</returns>
        private List<FamilyPlacemark> FamilyPlacemarksFromReader(SqlDataReader rdr, int start, int count)
        {
            List<FamilyPlacemark> families = new List<FamilyPlacemark>();
            int i;


            try
            {
                i = 0;
                while (rdr.Read() && families.Count < count)
                {
                    if (i++ < start)
                        continue;

                    try
                    {
                        families.Add(new FamilyPlacemark(new Family(Convert.ToInt32(rdr["family_id"]))));
                    }
                    catch { }
                }
            }
            catch { }

            return families;
        }

        /// <summary>
        /// Load a collection of SmallGroupPlacemark objects from the SqlDataReader object. Only read
        /// 'count' items starting at 'start' index.
        /// </summary>
        /// <param name="rdr">The SqlDataReader to use.</param>
        /// <param name="start">The starting index to load placemarks.</param>
        /// <param name="count">The maximum number of placemarks to load.</param>
        /// <returns>A collection of SmallGroupPlacemark objects.</returns>
        private List<SmallGroupPlacemark> SmallGroupPlacemarksFromReader(SqlDataReader rdr, int start, int count)
        {
            List<SmallGroupPlacemark> groups = new List<SmallGroupPlacemark>();
            int i;


            try
            {
                i = 0;
                while (rdr.Read() && groups.Count < count)
                {
                    if (i++ < start)
                        continue;

                    try
                    {
                        groups.Add(new SmallGroupPlacemark(new Group(Convert.ToInt32(rdr["group_id"]))));
                    }
                    catch { }
                }
            }
            catch { }

            return groups;
        }

        #endregion


        #region Security Methods

        /// <summary>
        /// Determines if the current user has access to perform the
        /// indicated operation on the person field in question.
        /// </summary>
        /// <param name="field">The ID number of the PersonField that the user wants access to.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        public bool PersonFieldOperationAllowed(int field, OperationType operation)
        {
            PermissionCollection permissions;

            //
            // Load the permissions.
            //
            permissions = new PermissionCollection(ObjectType.PersonField, field);

            return PermissionsOperationAllowed(permissions, operation);
        }

        /// <summary>
        /// Checks the PermissionCollection class to determine if the
        /// indicated operation is allowed for the current user.
        /// </summary>
        /// <param name="permissions">The collection of permissions to check. These should be object permissions.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        public bool PermissionsOperationAllowed(PermissionCollection permissions, OperationType operation)
        {
            return permissions.Allowed(operation, _User);
        }

        #endregion
    }
}
