<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KMLInsert.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.KMLInsert" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:ScriptManagerProxy ID="smpScripts" runat="server" />

<link type="text/css" rel="Stylesheet" href="UserControls/Custom/HDC/GoogleMaps/Includes/jqModal.css" />
<link type="text/css" rel="Stylesheet" href="css/jquery.jgrowl.css" />

<style type="text/css">
.jqmTitle {
    background-image: url('UserControls/Custom/HDC/GoogleMaps/Images/title-bg.png');
    background-repeat: repeat-x;
    border-bottom: 1px solid black;
    margin: -12px -12px 0px -12px;
    padding: 3px 12px 3px 12px;
}
</style>

<div class="jqmWindow" id="KMLDownloadDialog" style="display: none;">
  <div class="jqmTitle">
    Select your export options
  </div>

  <div style="margin-top: 6px;">
    <div id="showAreaSwitchDiv" runat="server" style="margin-bottom: 6px;">
      <span style="display: inline-table; vertical-align: middle;">Include area overlays</span><span id="showAreaSwitch" style="margin-left: 10px; display: inline-table; vertical-align: middle;"></span>
    </div>
    <div id="smallGroupsSwitchDiv" runat="server" style="margin-bottom: 6px;">
      <span style="display: inline-table; vertical-align: middle;">Include small group locations</span><span id="smallGroupsSwitch" style="margin-left: 10px; display: inline-table; vertical-align: middle;"></span>
    </div>
    <div id="campusLocationsSwitchDiv" runat="server" style="margin-bottom: 6px;">
      <span style="display: inline-table; vertical-align: middle;">Include campus locations</span><span id="campusLocationsSwitch" style="margin-left: 10px; display: inline-table; vertical-align: middle;"></span>
    </div>
    <hr style="margin-top: 14px;"/>
    <div style="text-align: right;">
      <span class="jqmClose" style="cursor: pointer; margin-right: 4px;">Cancel</span>
      <span style="cursor: pointer;" id="KMLDownloadLink">Download</span>
    </div>
  </div>
</div>

<script type="text/javascript">
  $(document).ready(function() {
    $('<div id="googleEarthExport" style="left: 38%; width: 25%; top: 35%;"></div>').appendTo('body');
    $('#KMLDownloadDialog').jqm({modal: true, speed: 300});
    $('#showAreaSwitch').iphoneSwitch("off", function(){}, function(){}, {switch_on_container_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch_container_on.png', switch_off_container_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch_container_off.png', switch_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch.png' });
    $('#smallGroupsSwitch').iphoneSwitch("off", function(){}, function(){}, {switch_on_container_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch_container_on.png', switch_off_container_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch_container_off.png', switch_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch.png' });
    $('#campusLocationsSwitch').iphoneSwitch("off", function(){}, function(){}, {switch_on_container_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch_container_on.png', switch_off_container_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch_container_off.png', switch_path: 'UserControls/Custom/HDC/GoogleMaps/Images/iphone_switch.png' });

    $('#KMLDownloadLink').click(function(){
        var url = "Default.aspx?page=<%= KMLDownloadPageIDSetting %>";

        if ($('#showAreaSwitch').data('switch_state') == 'on')
            url += '&showAreaID=all';
        if ($('#smallGroupsSwitch').data('switch_state') == 'on')
            url += '&populateCategoryID=<%= CategoryIDSetting %>';
        if ($('#campusLocationsSwitch').data('switch_state') == 'on')
            url += '&populatCampus=yes';

        url += KMLDownloadURL;

        $('#KMLDownloadDialog').jqmHide();
        $('#googleEarthExport').jGrowl('Your Google Earth export is being prepared. Please wait and the download will begin shortly.', {life:6000});    

        window.location = url;
    });
  });
</script>
