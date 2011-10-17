<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GoogleGroupLocator.ascx.cs" CodeBehind="GoogleGroupLocator.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.GoogleGroupLocator" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>
<%@ Register TagPrefix="GMap" Namespace="Arena.Custom.HDC.GoogleMaps.UI" Assembly="Arena.Custom.HDC.GoogleMaps" %>

<script type="text/javascript" src="UserControls/Custom/HDC/GoogleMaps/Includes/jquery.tablesorter.min.js"></script>
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
</script>

<GMap:GoogleMap runat="server" ID="map" HideDownload="true" HideAddtoTag="true" ShowMapType="false" ShowStreetView="false" MinZoomLevel="4" MaxZoomLevel="14" />

<asp:Panel ID="pnlAddress" class="sgl_address" runat="server">
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
    <div class = "sgf_showhide" style="margin-bottom: 3px;">
        <span ID="toggleFilter" runat="server" style="color: blue; text-decoration: underline; cursor: pointer;" class="smallText">Show Filter</span>
    </div>
    <div id="divFilter" class="sg_filter" runat="server" style="display: none;">
        <label ID="lbFilterCampus" runat="server" class="sgf_campus" for="<%= ddlCampus.ClientID %>">
            <%= CampusCaption %>
            <asp:DropDownList runat="server" ID="ddlCampus"></asp:DropDownList>
        </label>
        <label ID="lbFilterMeetingDay" runat="server" class="sgf_meetingday" for="<%= ddlMeetingDay.ClientID %>">
            <%= MeetingDayCaption %>
            <asp:DropDownList runat="server" ID="ddlMeetingDay"></asp:DropDownList>
        </label>
        <label ID="lbFilterTopic" runat="server" class="sgf_topic" for="<%= ddlTopic.ClientID %>">
            <%= TopicCaption %>
            <asp:DropDownList runat="server" ID="ddlTopic"></asp:DropDownList>
        </label>
        <label ID="lbFilterMaritalPreference" runat="server" class="sgf_maritalpreference" for="<%= ddlMaritalPreference.ClientID %>">
            <%= MaritalPreferenceCaption %>
            <asp:DropDownList runat="server" ID="ddlMaritalPreference"></asp:DropDownList>
        </label>
        <label ID="lbFilterAgeRange" runat="server" class="sgf_agerange" for="<%= ddlAgeRange.ClientID %>">
            <%= AgeRangeCaption %>
            <asp:DropDownList runat="server" ID="ddlAgeRange"></asp:DropDownList>
        </label>
        <label ID="lbFilterType" runat="server" class="sgf_type" for="<%= ddlType.ClientID %>">
            <%= TypeCaption %>
            <asp:DropDownList runat="server" ID="ddlType"></asp:DropDownList>
        </label>
        <label ID="lbFilterArea" runat="server" class="sgf_area" for="<%= ddlArea.ClientID %>">
            <%= AreaCaption %>
            <asp:DropDownList runat="server" ID="ddlArea"></asp:DropDownList>
        </label>

        <Arena:ArenaButton runat="server" ID="btnFilter" Text="Apply Filter" OnClick="btnFilter_Click" />
    </div>
</asp:Panel>

<asp:Panel ID="pnlListResults" runat="server" Visible="true">
    <asp:Literal ID="ltResultsContent" runat="server" />
</asp:Panel>
