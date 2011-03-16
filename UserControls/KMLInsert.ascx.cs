/**********************************************************************
* Description:	This module inserts a link to the KML Download popup as
*				well as defines the popup window to choose export options.
* Created By:	Daniel Hazelbaker @ High Desert Church
* Date Created:	3/27/2010 4:11:24 PM
**********************************************************************/

namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
	using System;
	using System.Data;
	using System.Configuration;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Web.UI.HtmlControls;

	using Arena.Portal;
	using Arena.Core;

	public partial class KMLInsert : PortalControl
	{
		public enum KMLInsertType
		{
			ListReportView = 1,
			AreaDetail = 2,
			SmallGroupTabControl = 3,
			SmallGroupClusterTabControl = 4,
			ProfileTabControl = 5
		}

		#region Module Settings

		[CustomListSetting("Module Type", "Select the type of module, also located on this page, that this KML download module will be associated with.", true,
			"1",
			new string[] { "List Report View", "Area Detail", "Small Group Tab Control",
							"Small Group Cluster Tab Control", "Profile Tab Control" },
			new string[] { "1", "2", "3", "4", "5" }
		)]
		public KMLInsertType ModuleTypeSetting { get { return (KMLInsertType)Convert.ToInt32(Setting("ModuleType", "1", true)); } }

		[PageSetting("Map Viewer Page", "The page that has the Google Map Viewer module installed on it.", true)]
		public string KMLDownloadPageIDSetting { get { return Setting("KMLDownloadPageID", "", true); } }

		#endregion

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			//
			// Call the appropriate module handler.
			//
			if (ModuleTypeSetting == KMLInsertType.ListReportView)
				Module_ListReportView();
			else if (ModuleTypeSetting == KMLInsertType.AreaDetail)
				Module_AreaDetail();
			else if (ModuleTypeSetting == KMLInsertType.SmallGroupTabControl)
				Module_SmallGroupTabControl();
			else if (ModuleTypeSetting == KMLInsertType.SmallGroupClusterTabControl)
				Module_SmallGroupClusterTabControl();
			else if (ModuleTypeSetting == KMLInsertType.ProfileTabControl)
				Module_ProfileTabControl();
			else
				throw new Exception("Invalid Module Type has been specified.");
		}
		
		#endregion

		void Module_ListReportView()
		{
			String script;


            script = "$(document).ready(function() {\n" +
                "  var container = $(\"input[title='Export Data to Excel']\").parent().get(0);\n" +
                "  $(container).append(\"<a href=\\\"default.aspx?page=" + KMLDownloadPageIDSetting + "&populateReportID=" + Request.QueryString["REPORTID"] + "\\\"><img src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\"></a>\");\n" +
                "});\n";
			Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geListReportView", script, true);
		}

		void Module_AreaDetail()
		{
			String script;


            script = "$(document).ready(function() {\n" +
                "  var button = $(\"nobr:contains('Google Map')\").parent().parent().parent().parent()\n" +
                "  if (button.length == 0) {\n" +
                "    var topGroup = $(\".TopGroup\");\n" +
                "    var tabStrip = eval(topGroup.attr(\"id\"));\n" +
                "    tabStrip.beginUpdate();\n" +
                "    var topTabs = tabStrip.get_tabs();\n" +
                "    var newTab = new ComponentArt.Web.UI.TabStripTab();\n" +
                "    newTab.set_text('Google Map');\n" +
                "    newTab.set_id('" + this.ClientID + "_geView');\n" +
                "    topTabs.add(newTab);\n" +
                "    tabStrip.endUpdate();\n" +
                "    button = $(\"nobr:contains('Google Map')\").parent().parent().parent().parent()\n" +
                "  }\n" +
                "  button.find(\"nobr\").html(\"<img style=\\\"margin-top: -2px; margin-bottom: -4px; margin-right: 4px;\\\" src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\">Download\");\n" +
                "  button.removeAttr(\"onclick\");\n" +
                "  button.click(function() {window.location = \"default.aspx?page=" + KMLDownloadPageIDSetting + "populateAreaID=" + Request.QueryString["AREA"] + "\"});\n" +
                "});\n";
			Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geAreaDetail", script, true);
		}

		void Module_SmallGroupTabControl()
		{
			if (String.IsNullOrEmpty(Request.QueryString["GROUP"]) == false)
			{
				String script;


                script = "$(document).ready(function() {\n" +
                    "  var container = $(\"td.listPager[align='right']\");\n" +
                    "  container.append(\"<a href=\\\"default.aspx?page=" + KMLDownloadPageIDSetting + "&populateSmallGroupID=" + Request.QueryString["GROUP"] + "\\\"><img src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\"></a>\");\n" +
                    "});\n";
				Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geSmallGroupTabControl", script, true);
			}
		}

		void Module_SmallGroupClusterTabControl()
		{
			if (String.IsNullOrEmpty(Request.QueryString["CLUSTER"]) == false)
			{
				String script;


                script = "$(document).ready(function() {\n" +
                    "  var container = $(\"td.listPager[align='right']\");\n" +
                    "  container.append(\"<a href=\\\"default.aspx?page=" + KMLDownloadPageIDSetting + "&populateClusterID=" + Request.QueryString["CLUSTER"] + "\\\"><img src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\"></a>\");\n" +
                    "});\n";
				Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geSmallGroupTabControl", script, true);
			}
		}

		void Module_ProfileTabControl()
		{
			if (String.IsNullOrEmpty(Request.QueryString["PROFILE"]) == false)
			{
				String script;


                script = "$(document).ready(function() {\n" +
                    "  var container = $(\"td.listPager[align='right']\");\n" +
                    "  container.append(\"<a href=\\\"default.aspx?page=" + KMLDownloadPageIDSetting + "&populateProfileID=" + Request.QueryString["PROFILE"] + "\\\"><img src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\"></a>\");\n" +
                    "});\n";
				Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geProfileTabControl", script, true);
			}
		}
	}
}