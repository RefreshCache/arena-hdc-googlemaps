<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JoinSmallGroup.ascx.cs" CodeBehind="JoinSmallGroup.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.JoinSmallGroup" %>

<div id="jsg_fields">
    <label ID="lbFieldName" runat="server" class="jsg_name" for="<%= tbName.ClientID %>">
        Name
        <asp:TextBox runat="server" ID="tbName"></asp:TextBox>
    </label>
    <label ID="lbFieldEmail" runat="server" class="jsg_email" for="<%= tbEmail.ClientID %>">
        E-mail
        <asp:TextBox runat="server" ID="tbEmail"></asp:TextBox>
    </label>
    <label ID="lbFieldPhone" runat="server" class="jsg_phone" for="<%= tbPhone.ClientID %>">
        Phone
        <asp:TextBox runat="server" ID="tbPhone"></asp:TextBox>
    </label>
    <div id="divFieldAddress" runat="server" class="jsg_address">
        <label id="lbFieldStreet" class="jsg_street">
            Street
            <asp:TextBox runat="server" ID="tbStreet"></asp:TextBox>
        </label>
        <label id="lbFieldCity" class="jsg_city">
            City
            <asp:TextBox runat="server" ID="tbCity"></asp:TextBox>
        </label>
        <label id="lbFieldState" class="jsg_state">
            State
            <asp:TextBox runat="server" ID="tbState"></asp:TextBox>
        </label>
        <label id="lbFieldZipcode" class="jsg_zipcode">
            Zipcode
            <asp:TextBox runat="server" ID="tbZipcode"></asp:TextBox>
        </label>
    </div>
    <label id="lbFieldComments" runat="server" class="jsg_comments">
        Comments
        <asp:TextBox runat="server" ID="tbComments" TextMode="MultiLine"></asp:TextBox>
    </label>
    <asp:Button ID="btnSubmit" Text="Submit" runat="server" OnClick="btnSubmit_Click" />
</div>
