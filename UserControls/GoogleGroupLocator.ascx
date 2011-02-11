<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GoogleGroupLocator.ascx.cs" CodeBehind="GoogleGroupLocator.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.GoogleGroupLocator" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>

<p>
    <GMap:GoogleMap runat="server" ID="map" HideDownload="true" />
</p>

<asp:Panel ID="pnlAddress" runat="server">
    <p id="pAddressError" runat="server" style="color: red;" visible="false">
        Could not find address, please check address and try again.
    </p>
    <p>
        Street: <asp:TextBox runat="server" ID="txtAddress" Width="240" /> City: <asp:TextBox runat="server" ID="txtCity" Width="140" /><br />
        State: <asp:TextBox runat="server" ID="txtState" Width="25" /> Zipcode: <asp:TextBox runat="server" ID="txtPostal" Width="60" /><br />
        <Arena:ArenaButton runat="server" ID="btnUpdate" Text="Center" OnClick="btnCenter_Click" />
    </p>
</asp:Panel>

<asp:Panel ID="pnlFilter" runat="server">
    <asp:HiddenField runat="server" ID="hfFilterVisible" Value="0" />
    <div>
        <span ID="toggleFilter" runat="server" style="color: blue; text-decoration: underline; cursor: pointer;" class="smallText">Show Filter</span>
    </div>
    <div id="divFilter" runat="server" style="display: none;">
        <table border="0">
            <tr runat="server" id="trCampus">
                <td style="text-align: right;"><%= CampusCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlCampus"></asp:DropDownList></td>
            </tr>
            <tr runat="server" id="trMeetingDay">
                <td style="text-align: right;"><%= MeetingDayCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlMeetingDay"></asp:DropDownList></td>
            </tr>
            <tr runat="server" id="trTopic">
                <td style="text-align: right;"><%= TopicCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlTopic"></asp:DropDownList></td>
            </tr>
            <tr runat="server" id="trMaritalPreference">
                <td style="text-align: right;"><%= MaritalPreferenceCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlMaritalPreference"></asp:DropDownList></td>
            </tr>
            <tr runat="server" id="trAgeRange">
                <td style="text-align: right;"><%= AgeRangeCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlAgeRange"></asp:DropDownList></td>
            </tr>
            <tr runat="server" id="trType">
                <td style="text-align: right;"><%= TypeCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlType"></asp:DropDownList></td>
            </tr>
            <tr runat="server" id="trArea">
                <td style="text-align: right;"><%= AreaCaption %>:</td>
                <td><asp:DropDownList runat="server" ID="ddlArea"></asp:DropDownList></td>
            </tr>
        </table>
        <Arena:ArenaButton runat="server" ID="btnFilter" Text="Filter" OnClick="btnFilter_Click" />
    </div>
</asp:Panel>
