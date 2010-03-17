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
			String filename = "ArenaReport.kml";
			KML kml;
			bool dumpXml = false;


			//
			// Create the KML object to interface with.
			//
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

				filename = "ArenaAreas.kml";
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
					kml.AddPersonPlacemark(new Person(Convert.ToInt32(rdr["person_id"])));
				}

				filename = report.Name + ".kml";
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

		//
		// Populate people from a given AreaID. A single placemark
		// will consist of an entire family rather than individuals.
		//
		private void PopulateKmlFromArea(KML kml, int areaID)
		{
			PersonCollection pc = new PersonCollection();


			pc.LoadByArea(areaID);
			foreach (Person p in pc)
			{
				kml.AddFamilyPlacemark(p.Family());
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

		/// <summary>
		/// Retrieve the XmlDocument that describes this KML object.
		/// </summary>
		public XmlDocument xml
		{
			get { return xmlDoc; }
		}


		/// <summary>
		/// Initialize a new KML object. The object can be rendered right
		/// away but will be empty and not contain any relevent information.
		/// </summary>
		public KML()
		{
			XmlDeclaration xDec;


			//
			// Initialize the XML document object.
			//
			xmlDoc = new XmlDocument();
			xDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDoc.InsertBefore(xDec, xmlDoc.DocumentElement);

			//
			// Create the root KML element with the proper namespace.
			//
			kmlRoot = xmlDoc.CreateElement("kml", "http://www.opengis.net/kml/2.2");
			xmlDoc.AppendChild(kmlRoot);

			//
			// The KML document element is the root of all KML data.
			//
			kmlDocument = xmlDoc.CreateElement("Document");
			kmlRoot.AppendChild(kmlDocument);

			SetupColorTables();
		}


		/// <summary>
		/// Setup all the color tables that will be used when cycling
		/// through colors.
		/// </summary>
		private void SetupColorTables()
		{
			currentAreaColor = 0;
			areaColorList = new String[] {	"4b0000ff", "4b00ff00", "4bff0000",
											"8b000066", "8b006600", "8b660000",
											"4bffff00", "4bff00ff", "4b00ffff",
											"6b666600", "6b660066", "6b006666" };
		}


		/// <summary>
		/// Create a style for each membership type. Detect common pin colors
		/// and define the pins with those colors.
		/// </summary>
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


		/// <summary>
		/// Add all the style information for a new pin color.
		/// </summary>
		/// <param name="styleName">The name for this style tag.</param>
		/// <param name="color">The color as a google color string, #aabbggrr (aa=alpha, bb=blue, gg=green, rr=red).</param>
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


		/// <summary>
		/// Create an HTML formatted string that contains the relavent person
		/// information.
		/// </summary>
		/// <param name="p">The person object to retrieve information about.</param>
		/// <returns>An HTML formatted string.</returns>
		private string PersonInfo(Person p)
		{
			String personInfo;


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
				personInfo = "<table width=\"100%\"<tr>" +
					"</td><td valign=\"top\">" + personInfo + "</td>" +
					"<td width=\"100px\"><img src=\"" + BaseUrl() + "CachedBlob.aspx?guid=" + p.Blob.GUID.ToString() + "&width=100&height=100\" />" +
					"</tr></table>";
			}

			return personInfo;
		}


		/// <summary>
		/// Build an HTML formatted string that contains all shared family numbers
		/// for the given family object.
		/// </summary>
		/// <param name="f">The family whose members will be searched for phone numbers.</param>
		/// <returns>An HTML formatted string.</returns>
		private string FamilyPhoneNumbers(Family f)
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


		/// <summary>
		/// Build an HTML formatted string that contains all personal phone numbers
		/// for the given person object.
		/// </summary>
		/// <param name="p">The person whose phone numbers we want.</param>
		/// <returns>An HTML formatted string.</returns>
		private string PersonPhoneNumbers(Person p)
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
				if (phone.Unlisted == true || phone.Number.Length == 0 || phone.PhoneType.Qualifier.Contains("propagate") == true)
					continue;

				if (String.IsNullOrEmpty(phone.Extension))
					phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + "<br />");
				else
					phoneStrings.Append(phone.PhoneType.Value + " #: " + phone.Number + " x" + phone.Extension + "<br />");
			}

			return phoneStrings.ToString();
		}


		/// <summary>
		/// Create a new placemark object on the map that will identify a
		/// single person in the database.
		/// </summary>
		/// <param name="p">The Person object to display.</param>
		public void AddPersonPlacemark(Person p)
		{
			XmlNode placemark, name, point, styleUrl, coordinates, description;
			String personInfo, phoneStrings;


			//
			// If there is not a valid address, skip this person.
			//
			if (p.PrimaryAddress == null ||
				(p.PrimaryAddress.Latitude == 0 && p.PrimaryAddress.Longitude == 0))
				return;

			//
			// Make sure all the membership pin types are registered.
			//
			RegisterMembershipStyles();

			//
			// Create the placemark tag.
			//
			placemark = xmlDoc.CreateElement("Placemark");
			kmlDocument.AppendChild(placemark);

			//
			// Create the name tag.
			//
			name = xmlDoc.CreateElement("name");
			name.AppendChild(xmlDoc.CreateTextNode(p.FullName));
			placemark.AppendChild(name);

			//
			// Create the style tag.
			//
			styleUrl = xmlDoc.CreateElement("styleUrl");
			styleUrl.AppendChild(xmlDoc.CreateTextNode(p.MemberStatus.Value));
			placemark.AppendChild(styleUrl);

			//
			// Build up the person information and phone numbers.
			//
			personInfo = PersonInfo(p);
			phoneStrings = PersonPhoneNumbers(p);

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

			//
			// Set the coordinates and store the placemark.
			//
			point = xmlDoc.CreateElement("Point");
			coordinates = xmlDoc.CreateElement("coordinates");
			coordinates.AppendChild(xmlDoc.CreateTextNode(String.Format("{0},{1},0", p.PrimaryAddress.Longitude, p.PrimaryAddress.Latitude)));
			point.AppendChild(coordinates);
			placemark.AppendChild(point);
		}


		/// <summary>
		/// Create a new placemark object on the map that identifies an entire
		/// family in the database. The family name is displayed in the placemark
		/// and the popup will display information about each member of the family.
		/// </summary>
		/// <param name="f">The Family object to display.</param>
		public void AddFamilyPlacemark(Family f)
		{
			XmlNode placemark, name, point, styleUrl, coordinates, description;
			String personInfo, phoneStrings;
			Person head = f.FamilyHead;


			//
			// If we don't have a valid address for this family then
			// do not display it.
			//
			if (head.PrimaryAddress == null ||
				(head.PrimaryAddress.Latitude == 0 && head.PrimaryAddress.Longitude == 0))
				return;

			//
			// Make sure all the membership pin types are registered.
			//
			RegisterMembershipStyles();

			//
			// Create the placemark tag.
			//
			placemark = xmlDoc.CreateElement("Placemark");
			kmlDocument.AppendChild(placemark);

			//
			// Create the name tag.
			//
			name = xmlDoc.CreateElement("name");
			name.AppendChild(xmlDoc.CreateTextNode(f.FamilyName));
			placemark.AppendChild(name);

			//
			// Create the style tag.
			//
			styleUrl = xmlDoc.CreateElement("styleUrl");
			styleUrl.AppendChild(xmlDoc.CreateTextNode(head.MemberStatus.Value));
			placemark.AppendChild(styleUrl);

			personInfo = "";
			foreach (Person p in f.FamilyMembers)
			{
				String info, phones;

				//
				// Build up the person information and phone numbers.
				//
				info = PersonInfo(p);
				phones = PersonPhoneNumbers(p);

				if (personInfo.Length > 0)
					personInfo += "<hr width=\"75%\"/>";

				personInfo += "<b>" + p.FullName + "</b><br />" + info;
				if (phones.Length > 0)
					personInfo += phones;
			}
			phoneStrings = FamilyPhoneNumbers(f);

			//
			// Store the description information.
			//
			description = xmlDoc.CreateElement("description");
			description.InnerXml = "<![CDATA[" +
				personInfo +
				"<hr />" +
				f.FamilyHead.PrimaryAddress.StreetLine1 + "<br />" +
				(String.IsNullOrEmpty(head.PrimaryAddress.StreetLine2) ? "" : head.PrimaryAddress.StreetLine2 + "<br />") +
				head.PrimaryAddress.City + ", " + head.PrimaryAddress.State + " " + head.PrimaryAddress.PostalCode + "<br />" +
				phoneStrings +
				"]]>";
			placemark.AppendChild(description);

			//
			// Set the coordinates and store the placemark.
			//
			point = xmlDoc.CreateElement("Point");
			coordinates = xmlDoc.CreateElement("coordinates");
			coordinates.AppendChild(xmlDoc.CreateTextNode(String.Format("{0},{1},0", head.PrimaryAddress.Longitude, head.PrimaryAddress.Latitude)));
			point.AppendChild(coordinates);
			placemark.AppendChild(point);
		}


		/// <summary>
		/// Add a polygon that will highlight an Area of the map which has
		/// been identified in the Arena database.
		/// </summary>
		/// <param name="a">The Arena object to display.</param>
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


		/// <summary>
		/// Add a color style for the given polygon.
		/// </summary>
		/// <param name="styleName">The name of the style to use.</param>
		/// <param name="fillColor">The google color code to fill the polygon with.</param>
		/// <param name="outlineColor">The google color code to outline the polygon with.</param>
		/// <param name="outlineWidth">The width of the outline to draw, 0 for no outline.</param>
		public void AddPolyColorStyle(String styleName, String fillColor, String outlineColor, int outlineWidth)
		{
			XmlNode style;
			XmlAttribute idAttrib;


			//
			// Create the StyleMap attribute and set it up to reference
			// the normal and highlighted styles.
			//
			style = xmlDoc.CreateElement("StyleMap");
			kmlDocument.AppendChild(style);
			idAttrib = xmlDoc.CreateAttribute("id");
			idAttrib.Value = styleName;
			style.Attributes.Append(idAttrib);
			style.InnerXml = String.Format("<Pair><key>normal</key><styleUrl>#{0}-n</styleUrl></Pair>" +
				"<Pair><key>highlight</key><styleUrl>#{0}-h</styleUrl></Pair>",
				styleName, styleName);

			//
			// Create the normal style for this polygon.
			//
			style = xmlDoc.CreateElement("Style");
			kmlDocument.AppendChild(style);
			idAttrib = xmlDoc.CreateAttribute("id");
			idAttrib.Value = String.Format("{0}-n", styleName);
			style.Attributes.Append(idAttrib);
			style.InnerXml = "<LineStyle><color>" + outlineColor + "</color><width>" + outlineWidth.ToString() + "</width></LineStyle>" +
				"<PolyStyle><color>" + fillColor + "</color>" +
				"<outline>" + (outlineWidth > 0 ? "1" : "0") + "</outline></PolyStyle>";

			//
			// Create the highlighted style for this polygon.
			//
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