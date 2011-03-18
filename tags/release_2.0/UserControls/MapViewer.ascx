<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MapViewer.ascx.cs" CodeBehind="MapViewer.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.MapViewer" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<span class="smallText">Populate map with</span>
<asp:DropDownList ID="ddlPopulateWith" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPopulateWith_SelectedIndexChanged">
    <asp:ListItem Selected="True" Value="0" Text="Individuals" />
    <asp:ListItem Value="1" Text="Families" />
    <asp:ListItem Value="2" Text="Small Groups" />
</asp:DropDownList><br />
<div class="smallText" style="margin-bottom: 3px;">
Note: If the map is blank try selecting a different population type.
</div>
<GMap:GoogleMap ID="map" runat="server" Width="640" Height="480" />
