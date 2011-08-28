<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GoogleGroupLocator.ascx.cs" CodeBehind="GoogleGroupLocator.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.GoogleGroupLocator" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>

<script type="text/javascript" src="UserControls/Custom/HDC/GoogleMaps/Includes/jquery.tablesorter.js"></script>
<script type="text/javascript" src="UserControls/Custom/HDC/GoogleMaps/Includes/jquery.tablesorter.pager.js"></script>
<link rel="stylesheet" href="UserControls/Custom/HDC/GoogleMaps/Includes/tablesorter.css" type="text/css" media="print, projection, screen" />

<script language="javascript" type="text/javascript">
    var SGLMarkers = [];

    function RegisterSmallGroup(gm, marker) {
        SGLMarkers.push(marker);
        marker.googlemap = gm;
        google.maps.event.addListener(marker, 'click', ShowGroupPopup);
    }

    function ShowGroupPopup() {
        _ShowGroupPopup(this, null);
    }

    function _ShowGroupPopup(marker, groupID) {
        var g, id;

        if (marker == null) {
            for (i = 0; i < SGLMarkers.length; i++) {
                if (SGLMarkers[i].groupID == groupID)
                    marker = SGLMarkers[i];
            }

            $('html,body').animate({ scrollTop: ($('#<%= map.ClientID %>').offset().top - 5) }, 'medium');
        }

        g = marker.googlemap;
        id = marker.groupID;

        if (g.infowindow != null)
            g.infowindow.close();
        g.infowindow = new google.maps.InfoWindow({ content: '<div style="text-align: center"><img src="' + GoogleMapRoot + 'ajax-spin.gif" style="border: none;" /></div>', maxWidth: 350 });
        g.infowindow.open(g.map, marker);

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
                g.infowindow.open(g.map, marker);
            }
        });
    }

    function ApplyFilter() {
    }

    $(document).ready(function () {
        $('.sglMapLink').click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            _ShowGroupPopup(null, $(this).find('a').attr('href'));
        });
    });
</script>

<GMap:GoogleMap runat="server" ID="map" HideDownload="true" HideAddtoTag="true" ShowMapType="false" ShowStreetView="false" MinZoomLevel="10" MaxZoomLevel="14" />

<asp:Panel ID="pnlAddress" runat="server">
    <p id="pAddressError" runat="server" style="color: red;" visible="false">
        Could not find address, please check address and try again.
    </p>
    <p>
        Street: <asp:TextBox runat="server" ID="txtAddress" Width="240" />
        City: <asp:TextBox runat="server" ID="txtCity" Width="140" />
        <br />
        State: <asp:TextBox runat="server" ID="txtState" Width="25" />
        Zipcode: <asp:TextBox runat="server" ID="txtPostal" Width="60" />
        Distance:
        <asp:DropDownList runat="server" ID="ddlDistance">
            <asp:ListItem Text="Any" Value="9999" Selected="True" />
            <asp:ListItem Text="10 Miles" Value="10" />
            <asp:ListItem Text="5 Miles" Value="5" />
            <asp:ListItem Text="3 Miles" Value="3" />
            <asp:ListItem Text="1 Miles" Value="1" />
        </asp:DropDownList>
        <br />
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
        <Arena:ArenaButton runat="server" ID="btnFilter" Text="Apply Filter" OnClientClick="ApplyFilter(); return false;" OnClick="btnFilter_Click" />
    </div>
</asp:Panel>

<asp:Panel ID="pnlListResults" runat="server" Visible="true">
    <GMap:DataGridWithHeaders ID="dgResults" runat="server" CssClass="tablesorter" CellPadding="5" AllowPaging="false" AutoGenerateColumns="false" GridLines="Horizontal" HeaderStyle-Font-Bold="true" OnItemDataBound="dgResults_ItemDataBound" HeaderStyle-CssClass="sglHeader">
        <ItemStyle CssClass="sglRow" />
        <AlternatingItemStyle CssClass="sglAlternateRow" />
        <Columns>
            <asp:BoundColumn HeaderText="Group" Visible="true" DataField="Name"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Meeting Day" Visible="true" ItemStyle-Wrap="false" DataField="MeetingDay"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Time" Visible="true" ItemStyle-Wrap="false" DataField="MeetingTime" DataFormatString="{0:t}"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Type" Visible="true" ItemStyle-Wrap="false" DataField="Type"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Topic" Visible="true" ItemStyle-Wrap="false" DataField="Topic"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Avg Age" Visible="true" ItemStyle-Wrap="false" DataField="AverageAge"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Notes" Visible="false" DataField="Notes"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Distance" Visible="false" DataField="Distance"></asp:BoundColumn>
            <asp:TemplateColumn HeaderText="View Map" Visible="true" ItemStyle-Wrap="false" ItemStyle-CssClass="sglMapLink"><ItemTemplate><a href="<%# DataBinder.Eval(Container.DataItem, "ID") %>">View Map</a></ItemTemplate></asp:TemplateColumn>
        </Columns>
    </GMap:DataGridWithHeaders>

    <div id="<%= dgResults.ClientID %>_pager" class="pager">
		<img src="UserControls/Custom/HDC/GoogleMaps/Images/first.png" class="first"/>
		<img src="UserControls/Custom/HDC/GoogleMaps/Images/prev.png" class="prev"/>
		<input type="text" class="pagedisplay" readonly />
		<img src="UserControls/Custom/HDC/GoogleMaps/Images/next.png" class="next"/>
		<img src="UserControls/Custom/HDC/GoogleMaps/Images/last.png" class="last"/>
		<select class="pagesize" style="display: none">
			<option selected="selected"  value="10">10</option>
			<option value="25">25</option>
			<option value="50">50</option>
		</select>
    </div>
</asp:Panel>
