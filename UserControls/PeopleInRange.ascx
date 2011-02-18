<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PeopleInRange.ascx.cs" CodeBehind="PeopleInRange.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.PeopleInRange" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<p>
Street: <asp:TextBox runat="server" ID="txtAddress" Width="180" /> City: <asp:TextBox runat="server" ID="txtCity" Width="120" /><br />
State: <asp:TextBox runat="server" ID="txtState" Width="25" /> Zipcode: <asp:TextBox runat="server" ID="txtPostal" Width="60" /> Distance: <asp:TextBox runat="server" ID="txtDistance" Width="35" /><br />
Populate with: <asp:DropDownList runat="server" ID="ddlType" /> <asp:CheckBox runat="server" ID="cbShowHome" Text="Show Address" Checked="true" /><br />
<asp:CheckBox runat="server" ID="cbShowCampus" Text="Show Campuses" /><br />
<asp:Button runat="server" ID="btnPopulate" OnClick="btnPopulate_Click" Text="Populate" /><br />
<asp:Literal runat="server" ID="ltError" Text="" />
</p>

<GMap:GoogleMap ID="myMap" runat="server" Width="640" Height="480" />
