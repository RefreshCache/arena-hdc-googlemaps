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
	using Arena.Portal;

	public partial class KMLDownloader : PortalControl
	{

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			KML kml;
			bool dumpXml = false;


			if ( ! IsPostBack )
			{
				
			}

			kml = new KML();

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
						PopulateKmlFromArea(kml, a.AreaID);
				}
				else
				{
					foreach (String areaString in Request.Params["populateAreaID"].Split(','))
						PopulateKmlFromArea(kml, Convert.ToInt32(areaString));
				}

				dumpXml = true;
			}

			//
			// Request to populate based on area IDs.
			//
			if (Request.Params["populateProfileID"] != null)
			{
				foreach (String profileString in Request.Params["populateProfileID"].Split(','))
				{
					Profile tag = new Profile(Convert.ToInt32(profileString));

					foreach (Person p in tag.Members)
						kml.AddPersonPlacemark(p);
				}

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
					kml.AddPersonPlacemark(new Person(Convert.ToInt32(rdr["person_id"])));
				}

				dumpXml = true;
			}

			if (dumpXml)
			{
				kml.xml.Save(writer);
				Response.ContentType = "application/vnd.google-earth.kml+xml";
				Response.Write(sb.ToString());
				Response.End();
			}
		}

		private void PopulateKmlFromArea(KML kml, int areaID)
		{
			PersonCollection pc = new PersonCollection();


			pc.LoadByArea(areaID);
			foreach (Person p in pc)
			{
				kml.AddPersonPlacemark(p);
			}
		}
		
		#endregion
	}

	class KML
	{
		private XmlDocument xmlDoc;
		private XmlNode kmlRoot, kmlDocument;
		private String[] areaColorList;
		private int currentAreaColor;
		private bool membershipStylesRegistered = false;

		public XmlDocument xml
		{
			get { return xmlDoc; }
		}

		public KML()
		{
			XmlDeclaration xDec;


			xmlDoc = new XmlDocument();
			xDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDoc.InsertBefore(xDec, xmlDoc.DocumentElement);

			kmlRoot = xmlDoc.CreateElement("kml", "http://www.opengis.net/kml/2.2");
			xmlDoc.AppendChild(kmlRoot);

			kmlDocument = xmlDoc.CreateElement("Document");
			kmlRoot.AppendChild(kmlDocument);

			SetupColorTables();
		}

		private void SetupColorTables()
		{
			currentAreaColor = 0;
			areaColorList = new String[] {	"4b0000ff", "4b00ff00", "4bff0000",
											"8b000066", "8b006600", "8b660000",
											"4bffff00", "4bff00ff", "4b00ffff",
											"6b666600", "6b660066", "6b006666" };
		}

		private void RegisterMembershipStyles()
		{
			LookupCollection lc;


			if (membershipStylesRegistered == true)
				return;

			lc = new LookupCollection(new Guid("0B4532DB-3188-40F5-B188-E7E6E4448C85"));
			foreach (Lookup l in lc)
			{
				if (String.IsNullOrEmpty(l.Qualifier) == false)
				{
					if (l.Qualifier == "pin_green.png")
						RegisterPinStyle(l.Value, "#ff00ff00");
					else if (l.Qualifier == "pin_red.png")
						RegisterPinStyle(l.Value, "#ff0000d0");
					else if (l.Qualifier == "pin_yellow.png")
						RegisterPinStyle(l.Value, "#ff00ffff");
					else if (l.Qualifier == "pin_grey.png")
						RegisterPinStyle(l.Value, "#ff909090");
					else
						RegisterPinStyle(l.Value, "#ffffffff");
				}
				else
					RegisterPinStyle(l.Value, "#ffffffff");
			}

			membershipStylesRegistered = true;
		}

		private void RegisterPinStyle(String styleName, String color)
		{
			XmlNode style = xmlDoc.CreateElement("Style");
			XmlAttribute idAttrib = xmlDoc.CreateAttribute("id");

	
			idAttrib.Value = styleName;
			style.Attributes.Append(idAttrib);
			kmlDocument.AppendChild(style);
			style.InnerXml = "<IconStyle><Icon>" +
				"<href>http://maps.google.com/mapfiles/kml/pushpin/wht-pushpin.png</href>" +
				"</Icon><color>" + color + "</color></IconStyle>";
		}

		public void AddPersonPlacemark(Person p)
		{
			XmlNode placemark, name, point, styleUrl, coordinates, description;
			StringBuilder phoneStrings = new StringBuilder();
			String personInfo;


			if (p.PrimaryAddress == null || (p.PrimaryAddress.Latitude == 0 && p.PrimaryAddress.Longitude == 0))
				return;

			RegisterMembershipStyles();

			placemark = xmlDoc.CreateElement("Placemark");
			kmlDocument.AppendChild(placemark);

			name = xmlDoc.CreateElement("name");
			name.AppendChild(xmlDoc.CreateTextNode(p.FullName));
			placemark.AppendChild(name);

			styleUrl = xmlDoc.CreateElement("styleUrl");
			styleUrl.AppendChild(xmlDoc.CreateTextNode(p.MemberStatus.Value));
			placemark.AppendChild(styleUrl);

			//
			// Build up the phone numbers.
			//
			foreach (PersonPhone phone in p.Phones)
			{
				if (phone.Unlisted == true || phone.Number.Length == 0)
					continue;

				if (String.IsNullOrEmpty(phone.Extension))
					phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + "<br />");
				else
					phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + " x" + phone.Extension + "<br />");
			}

			//
			// Build up the person information.
			//
			personInfo = p.MemberStatus.Value + "<br />" +
				p.Gender.ToString() + "<br />" +
				p.MaritalStatus.Value + "<br />" +
				new FamilyMember(p.PersonID).FamilyRole.Value + "<br />" +
				"Age: " + p.Age.ToString() + "<br />";
			if (p.BlobID != -1)
			{
				personInfo = "<table><tr><td>" +
					"<img src=\"" + BaseUrl() + "CachedBlob.aspx?guid=" + p.Blob.GUID.ToString() + "&width=100&height=100\" />" +
					"</td><td valign=\"top\">" + personInfo + "</td></tr></table>";
			}

			//
			// Store the description information.
			//
			description = xmlDoc.CreateElement("description");
			description.InnerXml = "<![CDATA[" +
				personInfo +
				"<hr />" +
				p.PrimaryAddress.StreetLine1 + "<br />" +
				(String.IsNullOrEmpty(p.PrimaryAddress.StreetLine2) ? "" : p.PrimaryAddress.StreetLine2 + "<br />") +
				p.PrimaryAddress.City + ", " + p.PrimaryAddress.State + " " + p.PrimaryAddress.PostalCode + "<br />" +
				phoneStrings.ToString() +
				"]]>";
			placemark.AppendChild(description);

			point = xmlDoc.CreateElement("Point");
			coordinates = xmlDoc.CreateElement("coordinates");
			coordinates.AppendChild(xmlDoc.CreateTextNode(String.Format("{0},{1},0", p.PrimaryAddress.Longitude, p.PrimaryAddress.Latitude)));
			point.AppendChild(coordinates);
			placemark.AppendChild(point);
		}

		public void AddAreaPolygon(Area a)
		{
			StringBuilder sb = new StringBuilder();
			XmlNode placemark;


			//
			// Build the coordinates for this area.
			//
			foreach (AreaCoordinate ac in a.Coordinates)
			{
				sb.AppendFormat("{0},{1},0 ", ac.Longitude, ac.Latitude);
			}

			//
			// Set the color for the area.
			//
			AddPolyColorStyle("area" + a.AreaID.ToString(), areaColorList[currentAreaColor], "ff000000", 2);
			if (++currentAreaColor >= areaColorList.Length)
				currentAreaColor = 0;

			//
			// Build the element data.
			//
			placemark = xmlDoc.CreateElement("Placemark");
			kmlDocument.AppendChild(placemark);
			placemark.InnerXml = String.Format("<name>{0}</name><styleUrl>#{1}</styleUrl>" +
				"<Polygon><tessellate>1</tessellate><outerBoundaryIs><LinearRing><coordinates>{2}" +
				"</coordinates></LinearRing></outerBoundaryIs></Polygon>",
				a.Name, "area" + a.AreaID.ToString(), sb.ToString());
		}

		public void AddPolyColorStyle(String styleName, String fillColor, String outlineColor, int outlineWidth)
		{
			XmlNode style;
			XmlAttribute idAttrib;


			style = xmlDoc.CreateElement("StyleMap");
			kmlDocument.AppendChild(style);
			idAttrib = xmlDoc.CreateAttribute("id");
			idAttrib.Value = styleName;
			style.Attributes.Append(idAttrib);
			style.InnerXml = String.Format("<Pair><key>normal</key><styleUrl>#{0}-n</styleUrl></Pair>" +
				"<Pair><key>highlight</key><styleUrl>#{0}-h</styleUrl></Pair>",
				styleName, styleName);

			style = xmlDoc.CreateElement("Style");
			kmlDocument.AppendChild(style);
			idAttrib = xmlDoc.CreateAttribute("id");
			idAttrib.Value = String.Format("{0}-n", styleName);
			style.Attributes.Append(idAttrib);
			style.InnerXml = "<LineStyle><color>" + outlineColor + "</color><width>" + outlineWidth.ToString() + "</width></LineStyle>" +
				"<PolyStyle><color>" + fillColor + "</color>" +
				"<outline>" + (outlineWidth > 0 ? "1" : "0") + "</outline></PolyStyle>";

			style = xmlDoc.CreateElement("Style");
			kmlDocument.AppendChild(style);
			idAttrib = xmlDoc.CreateAttribute("id");
			idAttrib.Value = String.Format("{0}-h", styleName);
			style.Attributes.Append(idAttrib);
			style.InnerXml = "<LineStyle><color>" + outlineColor + "</color><width>" + outlineWidth.ToString() + "</width></LineStyle>" +
				"<PolyStyle><color>" + fillColor + "</color>" +
				"<outline>" + (outlineWidth > 0 ? "1" : "0") + "</outline></PolyStyle>";
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
			segments = HttpContext.Current.Request.Url.Segments;
			for (i = 0; i < segments.Length - 1; i++)
			{
				url.Append(segments[i]);
			}

			return url.ToString();
		}

	}
}