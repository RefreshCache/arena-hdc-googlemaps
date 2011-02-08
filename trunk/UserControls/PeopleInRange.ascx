<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PeopleInRange.ascx.cs" CodeBehind="PeopleInRange.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.PeopleInRange" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Porta.UI" Assembly="Arena.Portal.UI" %>

Street: <asp:TextBox runat="server" ID="txtAddress" Width="180" /> City: <asp:TextBox runat="server" ID="txtCity" Width="120" /> <br />
State: <asp:TextBox runat="server" ID="txtState" Width="25" /> Zipcode: <asp:TextBox runat="server" ID="txtPostal" Width="60" /> Distance: <asp:TextBox runat="server" ID="txtDistance" Width="35" /> <br />
<asp:Button runat="server" ID="btnPopulate" OnClick="btnPopulate_Click" Text="Populate" />

<GMap:GoogleMap ID="myMap" runat="server" />
