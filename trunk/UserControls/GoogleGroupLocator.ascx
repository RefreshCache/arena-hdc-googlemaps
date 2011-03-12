<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GoogleGroupLocator.ascx.cs" CodeBehind="GoogleGroupLocator.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.GoogleGroupLocator" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>

<style type="text/css">
    tr.sglRow:hover, tr.over td { background-color: #C8C8C8; cursor: pointer; }
    tr.sglHeader { background-color: #761C1C; color: #E7E7DB; font-size: small; }
    tr.sglRow { font-size: small; }
    tr.sglAlternateRow { font-size: small; background-color: #EAE4E4; }
    tr.sglAlternateRow:hover, tr.over td { background-color: #C8C8C8; cursor: pointer; }
</style>

<script language="javascript" type="text/javascript">
    function RegisterSmallGroup(gm, marker) {
        marker.googlemap = gm;
        google.maps.event.addListener(marker, 'click', ShowGroupPopup);
    }

    function ShowGroupPopup() {
        var g = this.googlemap;
        var id = this.groupID;

        if (g.infowindow != null)
            g.infowindow.close();
        g.infowindow = new google.maps.InfoWindow({ content: '<div style="text-align: center"><img src="' + GoogleMapRoot + 'ajax-spin.gif" style="border: none;" /></div>', maxWidth: 350 });
        g.infowindow.open(this.map, this);

        $.ajax({
            url: g.serviceurl + "/GroupDetailsInfoWindow",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ groupID: id }),
            dataType: "json",
            success: function (data, status) {
                g.infowindow.setContent(data.d + '<div style="text-align: center"><hr width="75%" /><a href="default.aspx?page=<%= RegistrationPageSetting %>&group=' + id + '">Register for this group</a></div>');
            },
            error: function () {
                g.infowindow.close();
                g.infowindow.setContent('Failed to load details.');
                g.infowindow.open(this.map, this);
            }
        });
    }
</script>

<GMap:GoogleMap runat="server" ID="map" HideDownload="true" HideAddtoTag="true" ShowMapType="false" ShowStreetView="false" MinZoomLevel="10" MaxZoomLevel="14" />

<asp:Panel ID="pnlAddress" runat="server">
    <p id="pAddressError" runat="server" style="color: red;" visible="false">
        Could not find address, please check address and try again.
    </p>
    <p>
        Street: <asp:TextBox runat="server" ID="txtAddress" Width="240" /> City: <asp:TextBox runat="server" ID="txtCity" Width="140" /><br />
        State: <asp:TextBox runat="server" ID="txtState" Width="25" /> Zipcode: <asp:TextBox runat="server" ID="txtPostal" Width="60" /><br />
        <Arena:ArenaButton runat="server" ID="btnUpdate" Text="Center Map" OnClick="btnCenter_Click" />
    </p>
</asp:Panel>

<asp:Panel ID="pnlFilter" runat="server">
    <asp:HiddenField runat="server" ID="hfFilterVisible" Value="0" />
    <div style="margin-bottom: 3px;">
        <span ID="toggleFilter" runat="server" style="color: blue; text-decoration: underline; cursor: pointer;" class="smallText">Show Filter</span>
    </div>
    <div id="divFilter" runat="server" style="display: none; margin-bottom: 8px;">
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
        <Arena:ArenaButton runat="server" ID="btnFilter" Text="Apply Filter" OnClick="btnFilter_Click" />
    </div>
</asp:Panel>

<asp:Panel ID="pnlListResults" runat="server" Visible="true">
    <asp:DataGrid ID="dgResults" runat="server" CellPadding="5" AllowPaging="false" AutoGenerateColumns="false" GridLines="Horizontal" HeaderStyle-Font-Bold="true" OnItemDataBound="dgResults_ItemDataBound" HeaderStyle-CssClass="sglHeader">
        <ItemStyle CssClass="sglRow" />
        <AlternatingItemStyle CssClass="sglAlternateRow" />
        <Columns>
            <asp:BoundColumn HeaderText="Group" Visible="true" DataField="Name"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Meeting Day" Visible="true" ItemStyle-Wrap="false" DataField="MeetingDay"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Type" Visible="true" ItemStyle-Wrap="false" DataField="Type"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Topic" Visible="true" ItemStyle-Wrap="false" DataField="Topic"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Avg Age" Visible="true" ItemStyle-Wrap="false" DataField="AverageAge"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Notes" Visible="false" DataField="Notes"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Distance" Visible="false" DataField="Distance"></asp:BoundColumn>
        </Columns>
    </asp:DataGrid>
</asp:Panel>