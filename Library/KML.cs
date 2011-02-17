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
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace Arena.Custom.HDC.GoogleMaps
{
    public class KML
    {
        #region Properties

        private XmlDocument xmlDoc;
        private XmlNode kmlRoot, kmlDocument;
        private String[] areaColorList;
        private int currentAreaColor;
        private Dictionary<String, String> pinStyles = null;
        private int nextPinStyle = 0;

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
            // Save the google reference as we will need it later.
            //
            this.Google = google;
            pinStyles = new Dictionary<string, string>();

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


        #region Pin and Color Registration

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
        /// Add all the style information for a new pin color.
        /// </summary>
        /// <param name="styleName">The name for this style tag.</param>
        /// <param name="link">The hyperlink to use for the base image.</param>
        /// <param name="color">The color as a google color string, #aabbggrr (aa=alpha, bb=blue, gg=green, rr=red).</param>
        public string RegisterPinStyle(String link, Double scale, String color)
        {
            XmlNode style = xmlDoc.CreateElement("Style");
            XmlAttribute idAttrib = xmlDoc.CreateAttribute("id");
            String styleName;


            //
            // If the link does not include a "://", assume it is relative to the Arena URL.
            //
            if (link.Contains("://") == false)
            {
                if (link[0] == '/')
                    link = Google.ArenaUrl + link.Substring(1);
                else
                    link = Google.ArenaUrl + link;
            }

            //
            // Check if this link has already been registered.
            //
            if (pinStyles.ContainsKey(link))
                return pinStyles[link];

            //
            // Generate a new style.
            //
            styleName = "#s" + nextPinStyle.ToString();
            pinStyles[link] = styleName;
            nextPinStyle++;

            idAttrib.Value = styleName.Substring(1);
            style.Attributes.Append(idAttrib);
            kmlDocument.AppendChild(style);
            style.InnerXml = "<IconStyle>" +
                "<scale>" + scale.ToString() + "</scale>" +
                "<Icon><href>" + SecurityElement.Escape(link) + "</href></Icon>" +
                (color != null ? "<color>" + color + "</color>" : "") +
                "</IconStyle>";

            return styleName;
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


        #region Adding Map Objects

        /// <summary>
        /// Add a new placemark to the map. The placemark is immediately added so
        /// you cannot add a placemark and then make changes to it.
        /// </summary>
        /// <param name="placemark">The placemark to add.</param>
        public void AddPlacemark(Placemark placemark)
        {
            XmlElement element = null;

            element = placemark.KMLPlacemark(this);
            if (element != null)
                kmlDocument.AppendChild(element);
        }


        /// <summary>
        /// Add the placemarks that the given loader will want to populate us with.
        /// </summary>
        /// <param name="loader">The placemark loader to use.</param>
        public void AddLoader(Loader loader)
        {
            foreach (Placemark placemark in loader.LoadPlacemarks(Google))
            {
                XmlElement element = null;

                element = placemark.KMLPlacemark(this);
                if (element != null)
                    kmlDocument.AppendChild(element);
            }
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

        #endregion
    }
}
