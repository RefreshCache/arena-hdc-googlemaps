/**********************************************************************
* Description:	Provides a mechanism to download data in Google Earth
*				and Maps KML format.
* Created By:	Daniel Hazelbaker @ High Desert Church
* Date Created:	3/13/2010 11:45:37 AM
*
**********************************************************************/

namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
	using System;
	using System.Configuration;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Data.SqlClient;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Web.UI.HtmlControls;
	using System.Text;
	using System.Xml;

	using Arena.Core;
	using Arena.List;
	using Arena.Organization;
	using Arena.Portal;
	using Arena.SmallGroup;
    using Arena.Custom.HDC.GoogleMaps;
    using Arena.Custom.HDC.GoogleMaps.Maps;

	public partial class KMLDownloader : PortalControl
	{

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			String filename = "ArenaReport.kml";
			KML kml;
			bool dumpXml = false;


			//
			// Create the KML object to interface with.
			//
            kml = new KML(new Google(ArenaContext.Current.User, BaseUrl()));

			//
			// Request to have area IDs filled.
			//
			if (Request.Params["showAreaID"] != null)
			{
				if (Request.Params["showAreaID"] == "all")
				{
					AreaCollection ac = new AreaCollection(ArenaContext.Current.Organization.OrganizationID);

					foreach (Area a in ac)
						kml.AddAreaPolygon(a);
				}
				else
				{
					foreach (String areaString in Request.Params["showAreaID"].Split(','))
						kml.AddAreaPolygon(new Area(Convert.ToInt32(areaString)));
				}

				dumpXml = true;
			}

			//
			// Request to populate based on area IDs.
			//
			if (Request.Params["populateAreaID"] != null)
			{
				if (Request.Params["populateAreaID"] == "all")
				{
					AreaCollection ac = new AreaCollection(ArenaContext.Current.Organization.OrganizationID);

                    foreach (Area a in ac)
                        kml.AddLoader(new AreaLoader(a.AreaID));
				}
				else
				{
                    foreach (String areaString in Request.Params["populateAreaID"].Split(','))
                        kml.AddLoader(new AreaLoader(Convert.ToInt32(areaString)));
				}

				filename = "ArenaAreas.kml";
				dumpXml = true;
			}

			//
			// Request to populate based on profile/tag IDs.
			//
			if (Request.Params["populateProfileID"] != null)
			{
				foreach (String profileString in Request.Params["populateProfileID"].Split(','))
				{
					Profile tag = new Profile(Convert.ToInt32(profileString));

                    foreach (ProfileMember p in tag.Members)
                    {
                        if (p.Status.Qualifier != "D")
                        {
                            kml.AddPlacemark(new PersonPlacemark(p));
                        }
                    }
				}

				if (Request.Params["populateProfileID"].Split(',').Length == 1)
					filename = new Profile(Convert.ToInt32(Request.Params["populateProfileID"])).Name + ".kml";
				dumpXml = true;
			}

			//
			// Request to populate from the results of a list.
			//
			if (Request.Params["populateReportID"] != null)
			{
				ListReport report = new ListReport(Convert.ToInt32(Request.Params["populateReportID"]));
				SqlDataReader rdr;


				rdr = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(report.Query);
				while (rdr.Read())
				{
                    if (rdr["person_id"] != null)
                    {
                        kml.AddPlacemark(new PersonPlacemark(new Person(Convert.ToInt32(rdr["person_id"]))));
                    }
				}

				filename = report.Name + ".kml";
				dumpXml = true;
			}

			//
			// Populate the small group locations.
			//
			if (Request.Params["populateCategoryID"] != null)
			{
				GroupClusterCollection gcc = new GroupClusterCollection(Convert.ToInt32(Request.Params["populateCategoryID"]), ArenaContext.Current.Organization.OrganizationID);


				foreach (GroupCluster gc in gcc)
				{
					PopulateKmlFromCluster(kml, gc, false, true);
				}

				dumpXml = true;
			}

			//
			// Populate people from the given cluster IDs.
			//
			if (Request.Params["populateClusterID"] != null)
			{
				foreach (String clusterString in Request.Params["populateClusterID"].Split(','))
				{
					PopulateKmlFromCluster(kml, new GroupCluster(Convert.ToInt32(clusterString)), true, false);
				}

				if (Request.Params["populateClusterID"].Split(',').Length == 1)
					filename = new GroupCluster(Convert.ToInt32(Request.Params["populateClusterID"])).Name + ".kml";
				dumpXml = true;
			}

			//
			// Populate the given small group IDs.
			//
			if (Request.Params["populateSmallGroupID"] != null)
			{
				foreach (String groupString in Request.Params["populateSmallGroupID"].Split(','))
				{
					Group g = new Group(Convert.ToInt32(groupString));

					kml.AddPlacemark(new PersonPlacemark(g.Leader));
					foreach (GroupMember p in g.Members)
					{
						if (p.Active == true)
							kml.AddPlacemark(new PersonPlacemark(p));
					}
				}

				if (Request.Params["populateSmallGroupID"].Split(',').Length == 1)
					filename = new Group(Convert.ToInt32(Request.Params["populateSmallGroupID"])).Name + ".kml";
				dumpXml = true;
			}

			//
			// Request to include the campus locations.
			//
			if (Request.Params["populateCampus"] != null)
			{
				foreach (Campus c in ArenaContext.Current.Organization.Campuses)
				{
					kml.AddPlacemark(new CampusPlacemark(c));
				}

				dumpXml = true;
			}

			if (dumpXml)
			{
				kml.xml.Save(writer);
				Response.ContentType = "application/vnd.google-earth.kml+xml";
				Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename);
				Response.Write(sb.ToString());
				Response.End();
			}
		}

        #endregion


        #region Support Methods

        /// <summary>
		/// Populate the KML data stream with all the small groups that are
		/// a direct or indirect descendant of the given GroupCluster.
		/// </summary>
		/// <param name="kml">The KML object to populate.</param>
		/// <param name="parent">The GroupCluster object to populate from.</param>
		/// <param name="includePeople">Wether or not to include people in this dump.</param>
		/// <param name="includeGroups">Wether or not to include small groups in this dump.</param>
		void PopulateKmlFromCluster(KML kml, GroupCluster parent, Boolean includePeople, Boolean includeGroups)
		{
			foreach (GroupCluster gc in parent.ChildClusters)
			{
				PopulateKmlFromCluster(kml, gc, includePeople, includeGroups);
			}

			foreach (Group g in parent.SmallGroups)
			{
				if (includeGroups)
				{
					kml.AddPlacemark(new SmallGroupPlacemark(g));
				}

				if (includePeople)
				{
					kml.AddPlacemark(new PersonPlacemark(g.Leader));

					foreach (Person p in g.Members)
					{
						kml.AddPlacemark(new PersonPlacemark(p));
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
        static public string BaseUrl()
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
    }
}