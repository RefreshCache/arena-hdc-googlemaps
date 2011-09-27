<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JoinSmallGroup.ascx.cs" CodeBehind="JoinSmallGroup.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.JoinSmallGroup" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<script type="text/javascript">
    $(document).ready(function () {
        $('span.jsg_spouseinterest input').click(function () {
            if ($(this).attr('checked'))
                $('div.jsg_spousecontent').css('display', '');
            else
                $('div.jsg_spousecontent').css('display', 'none');
        });
    });
</script>

<div id="jsg_fields">
    <label ID="lbFieldFirstName" runat="server" class="jsg_firstname" for="<%= tbFirstName.ClientID %>">
        First Name
        <asp:TextBox runat="server" ID="tbFirstName"></asp:TextBox>
        <asp:RequiredFieldValidator ID="reqFirstName" Runat="server" ControlToValidate="tbFirstName" CssClass="errorText" 
						Display="Static" ErrorMessage="First Name is required"> *</asp:RequiredFieldValidator>
    </label>
    <label ID="lbFieldLastName" runat="server" class="jsg_lastname" for="<%= tbLastName.ClientID %>">
        Last Name
        <asp:TextBox runat="server" ID="tbLastName"></asp:TextBox>
        <asp:RequiredFieldValidator ID="reqLastName" Runat="server" ControlToValidate="tbLastName" CssClass="errorText" 
						Display="Static" ErrorMessage="First Name is required"> *</asp:RequiredFieldValidator>
    </label>
    <label ID="lbFieldEmail" runat="server" class="jsg_email" for="<%= tbEmail.ClientID %>">
        E-mail
        <asp:TextBox runat="server" ID="tbEmail"></asp:TextBox>
        <asp:RequiredFieldValidator ID="reqEmail" Runat="server" ControlToValidate="tbEmail" CssClass="errorText" 
						Display="Static" ErrorMessage="Email is required"> *</asp:RequiredFieldValidator>    </label>
    <label ID="lbFieldHomePhone" runat="server" class="jsg_homephone" for="<%= tbHomePhone.ClientID %>">
        Home Phone
        <Arena:PhoneTextBox runat="server" ID="tbHomePhone" ShowExtension="false" />
    </label>
    <label ID="lbFieldCellPhone" runat="server" class="jsg_cellphone" for="<%= tbCellPhone.ClientID %>">
        Cell Phone
        <Arena:PhoneTextBox runat="server" ID="tbCellPhone" ShowExtension="false" />
    </label>
    <div id="divFieldSpouse" runat="server" class="jsg_spouse">
        <asp:CheckBox ID="cbSpouse" CssClass="jsg_spouseinterest" runat="server" Text="My spouse is also interested in joining..." />
        <div id="divFieldSpouseContent" runat="server" class="jsg_spousecontent" style="display: none;">
            <label ID="lbFieldSpouseFirstName" runat="server" class="jsg_firstname" for="<%= tbSpouseFirstName.ClientID %>">
                First Name
                <asp:TextBox runat="server" ID="tbSpouseFirstName"></asp:TextBox>
            </label>
            <label ID="lbFieldSpouseLastName" runat="server" class="jsg_lastname" for="<%= tbSpouseLastName.ClientID %>">
                Last Name
                <asp:TextBox runat="server" ID="tbSpouseLastName"></asp:TextBox>
            </label>
            <label ID="lbFieldSpouseEmail" runat="server" class="jsg_email" for="<%= tbSpouseEmail.ClientID %>">
                E-mail
                <asp:TextBox runat="server" ID="tbSpouseEmail"></asp:TextBox>
            </label>
            <label ID="lbFieldSpouseCellPhone" runat="server" class="jsg_cellphone" for="<%= tbSpouseCellPhone.ClientID %>">
                Cell Phone
                <Arena:PhoneTextBox ID="tbSpouseCellPhone" runat="server" ShowExtension="false" />
            </label>
        </div>
    </div>
    <div id="divFieldAddress" runat="server" class="jsg_address">
        <label id="lbFieldStreet" class="jsg_street">
            Street
            <asp:TextBox runat="server" ID="tbStreet"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqStreetAddress" Runat="server" ControlToValidate="tbStreet" CssClass="errorText" 
						Display="Static" ErrorMessage="Street Address is required"> *</asp:RequiredFieldValidator>
        </label>
        <label id="lbFieldCity" class="jsg_city">
            City
            <asp:TextBox runat="server" ID="tbCity"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqCity" Runat="server" ControlToValidate="tbCity" CssClass="errorText" 
						Display="Static" ErrorMessage="City is required"> *</asp:RequiredFieldValidator>
        </label>
        <label id="lbFieldState" class="jsg_state">
            State
            <asp:DropDownList runat="server" ID="ddlState"></asp:DropDownList>
        </label>
        <label id="lbFieldZipcode" class="jsg_zipcode">
            Zipcode
            <asp:TextBox runat="server" ID="tbZipcode"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqZipCode" Runat="server" ControlToValidate="tbZipcode" CssClass="errorText" 
						Display="Static" ErrorMessage="Zip Code is required"> *</asp:RequiredFieldValidator>
        </label>
    </div>
    <label id="lbFieldComments" runat="server" class="jsg_comments">
        Comments
        <asp:TextBox runat="server" ID="tbComments" TextMode="MultiLine"></asp:TextBox>
    </label>
    <asp:Button ID="btnSubmit" CssClass="jsg_submit" Text="Submit" runat="server" OnClick="btnSubmit_Click" />
</div>
