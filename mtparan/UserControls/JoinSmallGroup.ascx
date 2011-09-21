<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JoinSmallGroup.ascx.cs" CodeBehind="JoinSmallGroup.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.JoinSmallGroup" %>

<script type="text/javascript">
    $(document).ready(function () {
        $('input.jsg_spouseinterest').click(function () {
            if ($(this).attr('checked'))
                $('#divFieldSpouseContent').css('display', '');
            else
                $('#divFieldSpouseContent').css('display', 'none');
        });
    });
</script>

<div id="jsg_fields">
    <label ID="lbFieldFirstName" runat="server" class="jsg_firstname" for="<%= tbFirstName.ClientID %>">
        First Name
        <asp:TextBox runat="server" ID="tbFirstName"></asp:TextBox>
    </label>
    <label ID="lbFieldLastName" runat="server" class="jsg_lastname" for="<%= tbLastName.ClientID %>">
        Last Name
        <asp:TextBox runat="server" ID="tbLastName"></asp:TextBox>
    </label>
    <label ID="lbFieldEmail" runat="server" class="jsg_email" for="<%= tbEmail.ClientID %>">
        E-mail
        <asp:TextBox runat="server" ID="tbEmail"></asp:TextBox>
    </label>
    <label ID="lbFieldHomePhone" runat="server" class="jsg_homephone" for="<%= tbHomePhone.ClientID %>">
        Home Phone
        <asp:TextBox runat="server" ID="tbHomePhone"></asp:TextBox>
    </label>
    <label ID="lbFieldCellPhone" runat="server" class="jsg_cellphone" for="<%= tbCellPhone.ClientID %>">
        Cell Phone
        <asp:TextBox runat="server" ID="tbCellPhone"></asp:TextBox>
    </label>
    <div id="divFieldSpouse" runat="server" class="jsg_spouse">
        <asp:CheckBox ID="cbSpouse" CssClass="jsg_spouseinterest" runat="server" Text="My spouse is interested in joining to..." />
        <div id="divFieldSpouseContent" runat="server" class="jsg_spousecontent">
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
                <asp:TextBox runat="server" ID="tbSpouseCellPhone"></asp:TextBox>
            </label>
        </div>
    </div>
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
            <asp:DropDownList runat="server" ID="ddlState"></asp:DropDownList>
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
    <asp:Button ID="btnSubmit" CssClass="jsg_submit" Text="Submit" runat="server" OnClick="btnSubmit_Click" />
</div>
