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
			AreaDetail = 2
		}
		#region Module Settings

		[CustomListSetting("Module Type", "Select the type of module, also located on this page, that this KML download module will be associated with.", true,
			"1",
			new string[] { "List Report View", "Area Detail" },
			new string[] { "1", "2" }
		)]
		public KMLInsertType ModuleTypeSetting { get { return (KMLInsertType)Convert.ToInt32(Setting("ModuleType", "1", true)); } }

		[PageSetting("KML Download Page", "The page that has the KML Downloader module installed on it.", true)]
		public string KMLDownloadPageIDSetting { get { return Setting("KMLDownloadPageID", "", true); } }

		[BooleanSetting("Area Maps Option", "Allow the user to turn on the display of area overlay maps.", true, true)]
		public Boolean AreaMapsSetting { get { return Convert.ToBoolean(Setting("AreaMaps", "1", true)); } }

		[TextSetting("Small Group ClusterID", "Allow the user to turn on the display of small group locations in this clusterID. -1 or empty to disable.", false)]
		public int ClusterIDSetting { get { return Convert.ToInt32(Setting("ClusterID", "-1", false)); } }

		[BooleanSetting("Campus Locations Option", "Allow the user to turn on the display of campus locations.", true, true)]
		public Boolean CampusLocationsSetting { get { return Convert.ToBoolean(Setting("CampusLocations", "true", true)); } }

		#endregion

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			smpScripts.Scripts.Add(new ScriptReference("~/Include/scripts/jquery.1.3.2.min.js"));
			smpScripts.Scripts.Add(new ScriptReference("~/Include/scripts/jquery.jgrowl.min.js"));
			smpScripts.Scripts.Add(new ScriptReference("Includes/jqModal.js"));
			smpScripts.Scripts.Add(new ScriptReference("Includes/jquery.iphone-switch.js"));

			showAreaSwitchDiv.Visible = AreaMapsSetting;
			smallGroupsSwitchDiv.Visible = (ClusterIDSetting != -1);
			campusLocationsSwitchDiv.Visible = CampusLocationsSetting;

			if (ModuleTypeSetting == KMLInsertType.ListReportView)
				Module_ListReportView();
			else if (ModuleTypeSetting == KMLInsertType.AreaDetail)
				Module_AreaDetail();
			else
				throw new Exception("Invalid Module Type has been specified.");
		}
		
		#endregion

		void Module_ListReportView()
		{
			String script;


			script = "$(document).ready(function() {\n" +
				"  var container = $(\"input[title='Export Data to Excel']\").parent().get(0);\n" +
				"  $(container).append(\"<a href=\\\"#\\\" onclick=\\\"$('#KMLDownloadDialog').jqmShow(); return false;\\\"><img src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\"></a>\");\n" +
				"});\n" +
				"var KMLDownloadURL = '&populateReportID=" + Request.QueryString["REPORTID"] + "';";
			Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geListReportView", script, true);
		}

		void Module_AreaDetail()
		{
			String script;


			script = "$(document).ready(function() {\n" +
				"  var download = $(\"nobr:contains('Download')\").parent().parent().parent().parent()\n" +
				"  if (download.length == 0) {\n" +
				"    var topGroup = $(\".TopGroup\");\n" +
				"    var tabStrip = eval(topGroup.attr(\"id\"));\n" +
				"    tabStrip.beginUpdate();\n" +
				"    var topTabs = tabStrip.get_tabs();\n" +
				"    var newTab = new ComponentArt.Web.UI.TabStripTab();\n" +
				"    newTab.set_text('Download');\n" +
				"    newTab.set_id('" + this.ClientID + "_geDownload');\n" +
				"    topTabs.add(newTab);\n" +
				"    tabStrip.endUpdate();\n" +
				"    download = $(\"nobr:contains('Download')\").parent().parent().parent().parent()\n" +
				"    download.find(\"nobr\").html(\"<img style=\\\"margin-top: -2px; margin-bottom: -4px; margin-right: 4px;\\\" src=\\\"UserControls/Custom/HDC/GoogleMaps/Images/darkearth.png\\\" width=\\\"16\\\" border=\\\"0\\\">Download\");\n" +
				"  }\n" +
				"  download.removeAttr(\"onclick\");\n" +
				"  download.click(function() {$('#KMLDownloadDialog').jqmShow();});\n" +
				"});\n" +
				"var KMLDownloadURL = '&populateAreaID=" + Request.QueryString["AREA"] + "';";
			Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_geAreaDetail", script, true);
		}
	}
}