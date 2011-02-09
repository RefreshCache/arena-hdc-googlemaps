using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml;

using Arena.Core;
using Arena.Organization;
using Arena.SmallGroup;

namespace Arena.Custom.HDC.GoogleMaps
{
    public class KML
    {
        #region Properties

        private XmlDocument xmlDoc;
        private XmlNode kmlRoot, kmlDocument;
        private String[] areaColorList;
        private int currentAreaColor;
        private bool pinStylesRegistered = false;

        /// <summary>
        /// The Google API interface we will use to generate information.
        /// </summary>
        public Google Google;

        /// <summary>
        /// Retrieve the XmlDocument that describes this KML object.
        /// </summary>
        public XmlDocument xml
        {
            get { return xmlDoc; }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Initialize a new KML object. The object can be rendered right
        /// away but will be empty and not contain any relevent information.
        /// </summary>
        public KML(Google google)
        {
            XmlDeclaration xDec;


            //
            // Prepare the url.
            //
            this.Google = google;

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

        #endregion


        #region Private Pin and Color Registration

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
        private void RegisterPinStyles()
        {
            LookupCollection lc;


            if (pinStylesRegistered == true)
                return;

            lc = new LookupCollection(new Guid("0B4532DB-3188-40F5-B188-E7E6E4448C85"));
            foreach (Lookup l in lc)
            {
                if (!String.IsNullOrEmpty(l.Qualifier))
                {
                    RegisterPinStyle("p" + l.Value, Google.ArenaUrl + "Images/Map/" + l.Qualifier, 0.6, null);
                    RegisterPinStyle("f" + l.Value, Google.ArenaUrl + "Images/Map/" + l.Qualifier, 0.6, null);
                }
                else
                {
                    RegisterPinStyle("p" + l.Value, Google.ArenaUrl + "Images/Map/pin_grey.png", 0.6, null);
                    RegisterPinStyle("f" + l.Value, Google.ArenaUrl + "Images/Map/pin_grey.png", 0.6, null);
                }
            }

            RegisterPinStyle("smallgroup", "http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=glyphish_group|4040FF|000000", 0.75, null);
            RegisterPinStyle("campus", Google.ArenaUrl + "UserControls/Custom/HDC/GoogleMaps/Images/chapel.png", 1, null);

            pinStylesRegistered = true;
        }

        /// <summary>
        /// Add all the style information for a new pin color.
        /// </summary>
        /// <param name="styleName">The name for this style tag.</param>
        /// <param name="link">The hyperlink to use for the base image.</param>
        /// <param name="color">The color as a google color string, #aabbggrr (aa=alpha, bb=blue, gg=green, rr=red).</param>
        private void RegisterPinStyle(String styleName, String link, Double scale, String color)
        {
            XmlNode style = xmlDoc.CreateElement("Style");
            XmlAttribute idAttrib = xmlDoc.CreateAttribute("id");


            idAttrib.Value = styleName;
            style.Attributes.Append(idAttrib);
            kmlDocument.AppendChild(style);
            style.InnerXml = "<IconStyle>" +
                "<scale>" + scale.ToString() + "</scale>" +
                "<Icon><href>" + SecurityElement.Escape(link) + "</href></Icon>" +
                (color != null ? "<color>" + color + "</color>" : "") +
                "</IconStyle>";
        }

        /// <summary>
        /// Add a color style for the given polygon.
        /// </summary>
        /// <param name="styleName">The name of the style to use.</param>
        /// <param name="fillColor">The google color code to fill the polygon with.</param>
        /// <param name="outlineColor">The google color code to outline the polygon with.</param>
        /// <param name="outlineWidth">The width of the outline to draw, 0 for no outline.</param>
        private void AddPolyColorStyle(String styleName, String fillColor, String outlineColor, int outlineWidth)
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

        #endregion


        #region Adding Placemarks

        /// <summary>
        /// Create a new placemark object on the map that will identify a
        /// single person in the database.
        /// </summary>
        /// <param name="p">The Person object to display.</param>
        public void AddPersonPlacemark(Person p)
        {
            XmlNode placemark, name, point, styleUrl, coordinates, description;


            //
            // If there is not a valid address, skip this person.
            //
            if (p.PrimaryAddress == null ||
                (p.PrimaryAddress.Latitude == 0 && p.PrimaryAddress.Longitude == 0))
                return;

            //
            // Make sure all the membership pin types are registered.
            //
            RegisterPinStyles();

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
            styleUrl.AppendChild(xmlDoc.CreateTextNode("#p" + p.MemberStatus.Value));
            placemark.AppendChild(styleUrl);

            //
            // Store the description information.
            //
            description = xmlDoc.CreateElement("description");
            description.InnerXml = "<![CDATA[" +
                Google.PersonDetailsPopup(p, false, true) +
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
            RegisterPinStyles();

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
            styleUrl.AppendChild(xmlDoc.CreateTextNode("#f" + head.MemberStatus.Value));
            placemark.AppendChild(styleUrl);

            //
            // Store the description information.
            //
            description = xmlDoc.CreateElement("description");
            description.InnerXml = "<![CDATA[" +
                Google.FamilyDetailsPopup(f, true) +
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
        /// Create a new placemark object on the map that will identify a
        /// single campus in the database.
        /// </summary>
        /// <param name="c">The Campus object to display.</param>
        public void AddCampusPlacemark(Campus c)
        {
            XmlNode placemark, name, point, styleUrl, coordinates, description;


            //
            // If there is not a valid address, skip this person.
            //
            if (c.Address == null ||
                (c.Address.Latitude == 0 && c.Address.Longitude == 0))
                return;

            //
            // Make sure all the pin types are registered.
            //
            RegisterPinStyles();

            //
            // Create the placemark tag.
            //
            placemark = xmlDoc.CreateElement("Placemark");
            kmlDocument.AppendChild(placemark);

            //
            // Create the name tag.
            //
            name = xmlDoc.CreateElement("name");
            name.AppendChild(xmlDoc.CreateTextNode(c.Name));
            placemark.AppendChild(name);

            //
            // Create the style tag.
            //
            styleUrl = xmlDoc.CreateElement("styleUrl");
            styleUrl.AppendChild(xmlDoc.CreateTextNode("#campus"));
            placemark.AppendChild(styleUrl);

            //
            // Store the description information.
            //
            description = xmlDoc.CreateElement("description");
            description.InnerXml = "<![CDATA[" +
                c.Address.StreetLine1 + "<br />" +
                (String.IsNullOrEmpty(c.Address.StreetLine2) ? "" : c.Address.StreetLine2 + "<br />") +
                c.Address.City + ", " + c.Address.State + " " + c.Address.PostalCode + "<br />" +
                "]]>";
            placemark.AppendChild(description);

            //
            // Set the coordinates and store the placemark.
            //
            point = xmlDoc.CreateElement("Point");
            coordinates = xmlDoc.CreateElement("coordinates");
            coordinates.AppendChild(xmlDoc.CreateTextNode(String.Format("{0},{1},0", c.Address.Longitude, c.Address.Latitude)));
            point.AppendChild(coordinates);
            placemark.AppendChild(point);
        }


        /// <summary>
        /// Create a new placemark object on the map that will identify a
        /// small group in the database.
        /// </summary>
        /// <param name="group">The Group object to display.</param>
        public void AddSmallGroupPlacemark(Group group)
        {
            XmlNode placemark, name, point, styleUrl, coordinates, description;


            //
            // If there is not a valid address, skip this group.
            //
            if (group.TargetLocation == null ||
                (group.TargetLocation.Latitude == 0 && group.TargetLocation.Longitude == 0))
                return;

            //
            // Make sure all the pin types are registered.
            //
            RegisterPinStyles();

            //
            // Create the placemark tag.
            //
            placemark = xmlDoc.CreateElement("Placemark");
            kmlDocument.AppendChild(placemark);

            //
            // Create the name tag.
            //
            name = xmlDoc.CreateElement("name");
            name.AppendChild(xmlDoc.CreateTextNode(group.Name));
            placemark.AppendChild(name);

            //
            // Create the style tag.
            //
            styleUrl = xmlDoc.CreateElement("styleUrl");
            styleUrl.AppendChild(xmlDoc.CreateTextNode("#smallgroup"));
            placemark.AppendChild(styleUrl);

            //
            // Store the description information.
            //
            description = xmlDoc.CreateElement("description");
            description.InnerXml = "<![CDATA[" +
                Google.SmallGroupDetailsPopup(group, true) +
                "]]>";
            placemark.AppendChild(description);

            //
            // Set the coordinates and store the placemark.
            //
            point = xmlDoc.CreateElement("Point");
            coordinates = xmlDoc.CreateElement("coordinates");
            coordinates.AppendChild(xmlDoc.CreateTextNode(String.Format("{0},{1},0", group.TargetLocation.Longitude, group.TargetLocation.Latitude)));
            point.AppendChild(coordinates);
            placemark.AppendChild(point);
        }

        #endregion


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
    }
}
