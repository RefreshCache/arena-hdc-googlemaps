<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AreaPicker.ascx.cs" CodeBehind="AreaPicker.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.AreaPicker" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>

<script language="javascript" type="text/javascript">
    function AP_PolygonAdded(gm, polygon) {
        polygon.googlemap = gm;
        google.maps.event.addListener(polygon, 'click', AP_SelectArea);
    }

    function AP_SelectArea() {
        window.location = "default.aspx?page=<%= SmallGroupLocatorPageSetting %>&area=" + this.uniqueID;
    }
</script>

<GMap:GoogleMap ID="map" runat="server" Width="640" Height="480" HideControls="true" />
