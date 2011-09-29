<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JoinSmallGroup.ascx.cs" CodeBehind="JoinSmallGroup.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.GoogleMaps.JoinSmallGroup" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<script type="text/javascript">
    $(document).ready(function () {
        $('span.jsg_spouseinterest input').click(function () {
            if ($(this).attr('checked')) {
                $('div.jsg_spouseholder').animate({ height: $('div.jsg_spousecontent').height() + 'px' }, 300);
            }
            else
                $('div.jsg_spouseholder').animate({ height: '0px' }, 300);
        });
    });
</script>

<asp:Panel ID="pnlForm" runat="server">
    <div class="jsg_fields">
        <span ID="spFieldFirstName" runat="server" class="jsg_firstname">
            <label for="<%= tbFirstName.ClientID %>">First Name</label>
            <asp:TextBox runat="server" ID="tbFirstName"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqFirstName" Runat="server" ControlToValidate="tbFirstName" CssClass="errorText" 
						Display="Static" ErrorMessage="First Name is required"> *</asp:RequiredFieldValidator>
        </span>
        <span ID="spFieldLastName" runat="server" class="jsg_lastname">
            <label for="<%= tbLastName.ClientID %>">Last Name</label>
            <asp:TextBox runat="server" ID="tbLastName"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqLastName" Runat="server" ControlToValidate="tbLastName" CssClass="errorText" 
						Display="Static" ErrorMessage="First Name is required"> *</asp:RequiredFieldValidator>
        </span>
        <span ID="spFieldEmail" runat="server" class="jsg_email">
            <label for="<%= tbEmail.ClientID %>">E-mail</label>
            <asp:TextBox runat="server" ID="tbEmail"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqEmail" Runat="server" ControlToValidate="tbEmail" CssClass="errorText" 
						Display="Static" ErrorMessage="Email is required"> *</asp:RequiredFieldValidator>
        </span>
        <span ID="spFieldHomePhone" runat="server" class="jsg_phone">
            <label for="<%= tbHomePhone.ClientID %>">Home Phone</label>
            <Arena:PhoneTextBox runat="server" ID="tbHomePhone" ShowExtension="false" />
        </span>
        <span ID="spFieldCellPhone" runat="server" class="jsg_phone">
            <label for="<%= tbCellPhone.ClientID %>">Cell Phone</label>
            <Arena:PhoneTextBox runat="server" ID="tbCellPhone" ShowExtension="false" />
        </span>

        <div id="divFieldSpouse" runat="server" class="jsg_spouse">
            <asp:CheckBox ID="cbSpouse" CssClass="jsg_spouseinterest" runat="server" Text="My spouse is also interested in joining..." />
            <div class="jsg_spouseholder">
                <div id="divFieldSpouseContent" runat="server" class="jsg_spousecontent">
                    <span ID="spFieldSpouseFirstName" runat="server" class="jsg_firstname">
                        <label for="<%= tbSpouseFirstName.ClientID %>">First Name</label>
                        <asp:TextBox runat="server" ID="tbSpouseFirstName"></asp:TextBox>
                    </span>
                    <span ID="spFieldSpouseLastName" runat="server" class="jsg_lastname">
                        <label for="<%= tbSpouseLastName.ClientID %>">Last Name</label>
                        <asp:TextBox runat="server" ID="tbSpouseLastName"></asp:TextBox>
                    </span>
                    <span ID="spFieldSpouseEmail" runat="server" class="jsg_email">
                        <label for="<%= tbSpouseEmail.ClientID %>">E-mail</label>
                        <asp:TextBox runat="server" ID="tbSpouseEmail"></asp:TextBox>
                    </span>
                    <span ID="spFieldSpouseCellPhone" runat="server" class="jsg_phone">
                        <label for="<%= tbSpouseCellPhone.ClientID %>">Cell Phone</label>
                        <Arena:PhoneTextBox ID="tbSpouseCellPhone" runat="server" ShowExtension="false" />
                    </span>
                </div>
            </div>
        </div>

        <div id="divFieldAddress" runat="server" class="jsg_address">
            <span class="jsg_street">
                <label for="<%= tbStreet.ClientID %>">Street</label>
                <asp:TextBox runat="server" ID="tbStreet"></asp:TextBox>
                <asp:RequiredFieldValidator ID="reqStreetAddress" Runat="server" ControlToValidate="tbStreet" CssClass="errorText" 
						Display="Static" ErrorMessage="Street Address is required"> *</asp:RequiredFieldValidator>
            </span>
            <span class="jsg_city">
                <label for="<%= tbCity.ClientID %>">City</label>
                <asp:TextBox runat="server" ID="tbCity"></asp:TextBox>
                <asp:RequiredFieldValidator ID="reqCity" Runat="server" ControlToValidate="tbCity" CssClass="errorText" 
						Display="Static" ErrorMessage="City is required"> *</asp:RequiredFieldValidator>
            </span>
            <span class="jsg_state">
                <label for="<%= ddlState.ClientID %>">State</label>
                <asp:DropDownList runat="server" ID="ddlState"></asp:DropDownList>
            </span>
            <span class="jsg_zipcode">
                <label for="<%= tbZipcode.ClientID %>">Zipcode</label>
                <asp:TextBox runat="server" ID="tbZipcode"></asp:TextBox>
                <asp:RequiredFieldValidator ID="reqZipCode" Runat="server" ControlToValidate="tbZipcode" CssClass="errorText" 
						Display="Static" ErrorMessage="Zip Code is required"> *</asp:RequiredFieldValidator>
            </span>
        </div>

        <span id="spFieldComments" runat="server" class="jsg_comments">
            <label for="<%= tbComments.ClientID %>">Comments</label>
            <asp:TextBox runat="server" ID="tbComments" TextMode="MultiLine"></asp:TextBox>
        </span>

        <asp:Button ID="btnSubmit" CssClass="jsg_submit" Text="Submit" runat="server" OnClick="btnSubmit_Click" />
    </div>
</asp:Panel>

<asp:Panel ID="pnlThankYou" runat="server" Visible="false">
    <div class="jsg_thankyou">
        Thank you for your interest in joining a small group. Your request will be reviewed shortly.
    </div>
</asp:Panel>