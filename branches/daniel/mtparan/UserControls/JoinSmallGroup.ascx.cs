using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Organization;
using Arena.Portal;
using Arena.SmallGroup;
using Arena.Utility;
using ArenaWeb;
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
    public partial class JoinSmallGroup : PortalControl
    {
        const String FieldValueEmail = "1";
        const String FieldValueHomePhone = "2";
        const String FieldValueCellPhone = "3";
        const String FieldValueAddress = "4";
        const String FieldValueComments = "5";

        #region Module Settings

        [BooleanSetting("Notify Group Leader", "Notify the small group leader about the request to join the small group.", true, false)]
        public Boolean NotifyGroupLeaderSetting { get { return Convert.ToBoolean(Setting("NotifyGroupLeader", "true", true)); } }

        [TextSetting("Notify Address", "Enter one or more e-mail addresses, separated by a comma, to be notified of the request to join the small group.", false)]
        public String[] NotifyAddressSetting { get { return Setting("NotifyAddress", "", false).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); } }

        [LookupSetting("New Member Role", "To automatically add the user to the small group select the role to use. (User must be logged in for this to work)", false, "BDF83C84-489B-401C-8B65-36C399D91B6E")]
        public Int32 NewMemberRoleSetting { get { return Convert.ToInt32(Setting("NewMemberRole", "-1", false)); } }

        [LookupSetting("Member Status", "The Member Status to set a user to when they add themself through this form. If not set them no records will be created.", false, "0B4532DB-3188-40F5-B188-E7E6E4448C85")]
        public int MemberStatusIDSetting { get { return Convert.ToInt32(Setting("MemberStatusID", "-1", false)); } }

        [CustomListSetting("Available Fields", "Select which fields are available for the user to fill in. Defaults to all fields.", false, "",
            new String[] { "E-mail", "HomePhone", "CellPhone", "Address", "Comments" },
            new String[] { FieldValueEmail, FieldValueHomePhone, FieldValueCellPhone, FieldValueAddress, FieldValueComments }, ListSelectionMode.Multiple)]
        public String[] AvailableFieldsSetting { get { return Setting("AvailableFields", "", false).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); } }

        [CssSetting("Style Css", "If you wish to customize the styling you can duplicate the joinsmallgroup.css file and enter the path to it here. Defaults to UserControls/Custom/HDC/GoogleMaps/Includes/joinsmallgroup.css", false)]
        public String StyleCssSetting { get { return Setting("StyleCss", "UserControls/Custom/HDC/GoogleMaps/Includes/joinsmallgroup.css", false); } }

        #endregion


        #region Private Variables

        private Person person = null;
        private Person spouse = null;

        #endregion


        /// <summary>
        /// The page is loading and not yet displayed. Prepare any dynamic information
        /// that needs to be set before page processing begins.
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            BasePage.AddCssLink(Page, StyleCssSetting);
            BasePage.AddJavascriptInclude(Page, BasePage.JQUERY_INCLUDE);

            //
            // Load the original person and spouse, if we know them.
            //
            person = (CurrentPerson != null ? CurrentPerson : new Person());
            spouse = (person.Spouse() != null ? person.Spouse() : new Person());

            //
            // Set the initial information on the page as well as the field visibility.
            //
            if (!IsPostBack)
            {
                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueEmail))
                {
                    spFieldEmail.Visible = false;
                    spFieldSpouseEmail.Visible = false;
                }

                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueHomePhone))
                    spFieldHomePhone.Visible = false;

                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueCellPhone))
                {
                    spFieldCellPhone.Visible = false;
                    spFieldSpouseCellPhone.Visible = false;
                }

                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueAddress))
                    divFieldAddress.Visible = false;
                
                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueComments))
                    spFieldComments.Visible = false;

                SetInfo();
            }
        }


        /// <summary>
        /// Set initial information to be displayed on the page. If the user is logged in
        /// then this function will fill in their personal information automatically for them.
        /// </summary>
        void SetInfo()
        {
            PersonPhone phone;


            //
            // Load up the available states to choose from.
            //
            ddlState.SelectedIndex = -1;
            Utilities.LoadStates(ddlState);
            try
            {
                ddlState.SelectedValue = CurrentOrganization.Address.State;
            }
            catch { }

            //
            // Load name information for primary and spouse.
            //
            tbFirstName.Text = person.FirstName;
            tbLastName.Text = person.LastName;
            tbSpouseFirstName.Text = spouse.FirstName;
            tbSpouseLastName.Text = spouse.LastName;
            
            //
            // Load e-mail information for primary and spouse.
            //
            tbEmail.Text = person.Emails.FirstActive;
            tbSpouseEmail.Text = spouse.Emails.FirstActive;

            //
            // Load phone information for primary and spouse.
            //
            phone = person.Phones.FindByType(SystemLookup.PhoneType_Home);
            if (phone != null)
                tbHomePhone.PhoneNumber = phone.Number;
            phone = person.Phones.FindByType(SystemLookup.PhoneType_Cell);
            if (phone != null)
                tbCellPhone.PhoneNumber = phone.Number;
            phone = spouse.Phones.FindByType(SystemLookup.PhoneType_Cell);
            if (phone != null)
                tbSpouseCellPhone.PhoneNumber = phone.Number;

            //
            // Load address information for family.
            //
            if (person.PrimaryAddress != null && person.PrimaryAddress.AddressID != -1)
            {
                ListItem li;

                tbStreet.Text = person.PrimaryAddress.StreetLine1;
                tbCity.Text = person.PrimaryAddress.City;
                li = ddlState.Items.FindByValue(person.PrimaryAddress.State);
                if (li != null)
                    li.Selected = true;
                tbZipcode.Text = person.PrimaryAddress.PostalCode;
            }
        }


        /// <summary>
        /// Save the information the user entered on the form into their person records.
        /// </summary>
        void SaveInfo()
        {
            Boolean newPerson = (person.PersonID == -1);
            Boolean newSpouse = (spouse.PersonID == -1 && cbSpouse.Checked && tbSpouseFirstName.Text.Trim().Length > 0 && tbSpouseLastName.Text.Trim().Length > 0);
            string userID = (!String.IsNullOrEmpty(CurrentUser.Identity.Name) ? CurrentUser.Identity.Name : "smallgrouplocator");
            PersonPhone phone;
            PersonAddress address;


            //
            // Set the primary's name.
            //
            person.FirstName = tbFirstName.Text.Trim();
            person.LastName = tbLastName.Text.Trim();

            //
            // Set the primary's e-mail.
            //
            person.Emails.FirstActive = tbEmail.Text.Trim();

            //
            // Set the primary's home phone.
            //
            phone = person.Phones.FindByType(SystemLookup.PhoneType_Home);
            if (phone == null)
            {
                phone = new PersonPhone();
                phone.PhoneType = new Lookup(SystemLookup.PhoneType_Home);
                person.Phones.Add(phone);
            }
            phone.Number = tbHomePhone.PhoneNumber.Trim();

            //
            // Set the primary's cell phone.
            //
            phone = person.Phones.FindByType(SystemLookup.PhoneType_Cell);
            if (phone == null)
            {
                phone = new PersonPhone();
                phone.PhoneType = new Lookup(SystemLookup.PhoneType_Cell);
                person.Phones.Add(phone);
            }
            phone.Number = tbCellPhone.PhoneNumber.Trim();

            //
            // Set the primary's address.
            //
            if (tbStreet.Text.Trim().Length > 0)
            {
                address = person.Addresses.FindByType(SystemLookup.AddressType_Home);
                if (address == null)
                {
                    address = new PersonAddress();
                    address.AddressType = new Lookup(SystemLookup.AddressType_Home);
                    person.Addresses.Add(address);
                }
                address.Address = new Address(tbStreet.Text.Trim(), String.Empty, tbCity.Text.Trim(), ddlState.SelectedValue, tbZipcode.Text.Trim(), false);
                address.Primary = true;
            }

            //
            // Set some final information about the primary person if we are creating a new record.
            //
            if (person.PersonID == -1)
            {
                person.RecordStatus = Arena.Enums.RecordStatus.Pending;
                person.MemberStatus = new Lookup(MemberStatusIDSetting);
                if (tbSpouseFirstName.Text.Trim().Length == 0)
                    person.MaritalStatus = new Lookup(SystemLookup.MaritalStatus_Unknown);
                else
                    person.MaritalStatus = new Lookup(SystemLookup.MaritalStatus_Married);
                person.Gender = Arena.Enums.Gender.Unknown;
            }

            //
            // Save the person record.
            //
            person.Save(CurrentPortal.OrganizationID, userID, false);
            if (tbStreet.Text.Trim().Length > 0)
                person.SaveAddresses(CurrentPortal.OrganizationID, userID);
            person.SavePhones(CurrentPortal.OrganizationID, userID);
            person.SaveEmails(CurrentPortal.OrganizationID, userID);

            //
            // Process the spouse if we have spousal(?) information.
            //
            if (tbSpouseFirstName.Text.Trim().Length > 0 && tbSpouseLastName.Text.Trim().Length > 0)
            {
                //
                // Set the spouse's name.
                //
                spouse.FirstName = tbSpouseFirstName.Text.Trim();
                spouse.LastName = tbSpouseLastName.Text.Trim();

                //
                // Set the spouse's e-mail.
                //
                spouse.Emails.FirstActive = tbSpouseEmail.Text.Trim();

                //
                // Set the spouse's home phone.
                //
                phone = spouse.Phones.FindByType(SystemLookup.PhoneType_Home);
                if (phone == null)
                {
                    phone = new PersonPhone();
                    phone.PhoneType = new Lookup(SystemLookup.PhoneType_Home);
                    spouse.Phones.Add(phone);
                }
                phone.Number = tbHomePhone.PhoneNumber.Trim();

                //
                // Set the spouse's cell phone.
                //
                phone = spouse.Phones.FindByType(SystemLookup.PhoneType_Cell);
                if (phone == null)
                {
                    phone = new PersonPhone();
                    phone.PhoneType = new Lookup(SystemLookup.PhoneType_Cell);
                    spouse.Phones.Add(phone);
                }
                phone.Number = tbSpouseCellPhone.PhoneNumber.Trim();

                //
                // Set the spouse's address.
                //
                address = spouse.Addresses.FindByType(SystemLookup.AddressType_Home);
                if (address == null)
                {
                    address = new PersonAddress();
                    address.AddressType = new Lookup(SystemLookup.AddressType_Home);
                    spouse.Addresses.Add(address);
                }
                address.Address = new Address(tbStreet.Text.Trim(), String.Empty, tbCity.Text.Trim(), ddlState.SelectedValue, tbZipcode.Text.Trim(), false);
                address.Primary = true;

                //
                // Update some final information about the spouse.
                //
                if (spouse.PersonID == -1)
                {
                    spouse.RecordStatus = Arena.Enums.RecordStatus.Pending;
                    spouse.MemberStatus = new Lookup(MemberStatusIDSetting);
                    spouse.MaritalStatus = new Lookup(SystemLookup.MaritalStatus_Married);
                    spouse.Gender = Arena.Enums.Gender.Unknown;
                }

                //
                // Save the spouse record.
                //
                spouse.Save(CurrentPortal.OrganizationID, userID, false);
                spouse.SaveAddresses(CurrentPortal.OrganizationID, userID);
                spouse.SavePhones(CurrentPortal.OrganizationID, userID);
                spouse.SaveEmails(CurrentPortal.OrganizationID, userID);
            }

            //
            // If we created a new person (and possibly spouse) record then setup
            // the family information.
            //
            if (newPerson || newSpouse)
            {
                FamilyMember fm;
                Family family;

                //
                // Create a new family record if we need one.
                //
                if (newPerson)
                {
                    family = new Family();
                    family.OrganizationID = CurrentPortal.OrganizationID;
                    family.FamilyName = tbLastName.Text.Trim() + " Family";
                    family.Save(userID);

                    fm = new FamilyMember(family.FamilyID, person.PersonID);
                    fm.FamilyID = family.FamilyID;
                    fm.FamilyRole = new Lookup(SystemLookup.FamilyRole_Adult);
                    fm.Save(userID);
                }
                else
                    family = person.Family();

                //
                // Add the spouse to the family if we have a spouse.
                //
                if (newSpouse)
                {
                    fm = new FamilyMember(family.FamilyID, spouse.PersonID);
                    fm.FamilyID = family.FamilyID;
                    fm.FamilyRole = new Lookup(SystemLookup.FamilyRole_Adult);
                    fm.Save(userID);
                }
            }
        }


        /// <summary>
        /// The user has clicked the submit button, perform any necessary steps to put them
        /// in the small group.
        /// </summary>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Group group = new Group(Convert.ToInt32(Request.QueryString["group"]));


            //
            // Verify information.
            //
            if (Page.IsValid == false)
            {
                Page.FindControl("valSummary").Visible = true;

                return;
            }

            //
            // Save people records if we are configured that way.
            //
            if (MemberStatusIDSetting != -1 || person.PersonID != -1)
            {
                SaveInfo();
            }

            //
            // Join the member into the small group if that has been requested.
            //
            if (NewMemberRoleSetting != -1 && group.GroupID != -1)
            {
                //
                // If we have a valid person object then add them to the small group.
                //
                if (person.PersonID != -1)
                {
                    GroupMember gm = new GroupMember(group.GroupID, person.PersonID);

                    if (gm.GroupID == -1)
                    {
                        gm.Active = true;
                        gm.DateJoined = DateTime.Now;
                        gm.GroupID = group.GroupID;
                        gm.MemberNotes = tbComments.Text;
                        gm.Role = new Lookup(NewMemberRoleSetting);

                        gm.Save(ArenaContext.Current.Organization.OrganizationID,
                            (CurrentPerson != null ? ArenaContext.Current.User.Identity.Name : "smallgrouplocator"));
                    }
                }

                //
                // If we have a valid spouse object then add them to the small group.
                //
                if (spouse.PersonID != -1)
                {
                    GroupMember gm = new GroupMember(group.GroupID, spouse.PersonID);

                    if (gm.GroupID == -1)
                    {
                        gm.Active = true;
                        gm.DateJoined = DateTime.Now;
                        gm.GroupID = group.GroupID;
                        gm.MemberNotes = tbComments.Text;
                        gm.Role = new Lookup(NewMemberRoleSetting);

                        gm.Save(ArenaContext.Current.Organization.OrganizationID,
                            (CurrentPerson != null ? ArenaContext.Current.User.Identity.Name : "smallgrouplocator"));
                    }
                }
            }

            //
            // E-mail the small group leader if the leader should be notified.
            //
            if (NotifyGroupLeaderSetting == true && !String.IsNullOrEmpty(group.Leader.Emails.FirstActive))
            {
                ArenaSendMail.SendMail(String.Empty, String.Empty, group.Leader.Emails.FirstActive, "Small Group Request", MailMessageContents(group, true));
            }

            //
            // E-mail each of the specified contact addresses.
            //
            foreach (String email in NotifyAddressSetting)
            {
                ArenaSendMail.SendMail(String.Empty, String.Empty, email, "Small Group Request", MailMessageContents(group, false));
            }
        }


        /// <summary>
        /// Retrieves the contents of the e-mail that will be e-mailed to a person.
        /// </summary>
        /// <param name="group">The small group that is being joined.</param>
        /// <param name="toLeader">Wether or not this e-mail is being sent to the leader.</param>
        /// <returns>An HTML formatted e-mail messages body.</returns>
        String MailMessageContents(Group group, Boolean toLeader)
        {
            StringBuilder sb = new StringBuilder();


            //
            // Put up the header information.
            //
            sb.AppendFormat("Somebody is interested in joining {0} small group {1}. The information they provided is below.<br /><br />\r\n",
                (toLeader ? "your" : "the"), group.Name);

            sb.AppendFormat("<b>Group ID:</b> {0}<br />\r\n", group.GroupID.ToString());

            //
            // Include the primary person's information.
            //
            sb.AppendFormat("<b>First Name:</b> {0}<br />\r\n", tbFirstName.Text);
            sb.AppendFormat("<b>Last Name:</b> {0}<br />\r\n", tbLastName.Text);

            if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueEmail))
                sb.AppendFormat("<b>E-mail:</b> {0}<br />\r\n", tbEmail.Text);

            if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueHomePhone))
                sb.AppendFormat("<b>Home Phone:</b> {0}<br />\r\n", tbHomePhone.PhoneNumber);

            if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueCellPhone))
                sb.AppendFormat("<b>Cell Phone:</b> {0}<br />\r\n", tbCellPhone.PhoneNumber);

            //
            // If the spouse is also interested then include their information.
            //
            if (cbSpouse.Checked && tbSpouseFirstName.Text.Length > 0 && tbSpouseLastName.Text.Length > 0)
            {
                sb.AppendFormat("<b>First Name:</b> {0}<br />\r\n", tbFirstName.Text);
                sb.AppendFormat("<b>Last Name:</b> {0}<br />\r\n", tbLastName.Text);

                if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueEmail))
                    sb.AppendFormat("<b>E-mail:</b> {0}<br />\r\n", tbEmail.Text);

                if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueHomePhone))
                    sb.AppendFormat("<b>Home Phone:</b> {0}<br />\r\n", tbHomePhone.PhoneNumber);

                if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueCellPhone))
                    sb.AppendFormat("<b>Cell Phone:</b> {0}<br />\r\n", tbCellPhone.PhoneNumber);
            }

            //
            // Include standard address information.
            //
            if (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueAddress))
                sb.AppendFormat("<b>Address:</b> {0}, {1}, {2} {3}<br />\r\n", tbStreet.Text, tbCity.Text, ddlState.SelectedValue, tbZipcode.Text);

            //
            // Include any comments if the person entered any.
            //
            if (tbComments.Text.Length > 0 && (AvailableFieldsSetting.Length == 0 || AvailableFieldsSetting.Contains(FieldValueComments)))
                sb.AppendFormat("<b>Comments:</b> {0}<br />\r\n", tbComments.Text);

            //
            // If the person has been added to the group then indicate that as well.
            //
            if (NewMemberRoleSetting != -1 && ArenaContext.Current.Person.PersonID != -1 &&
                new GroupMember(group.GroupID, ArenaContext.Current.Person.PersonID).GroupID != -1)
            {
                sb.AppendFormat("<br />This person has been added to the small group as a {0}.<br />\r\n",
                    new Lookup(NewMemberRoleSetting).Value);
            }

            return sb.ToString();
        }
    }
}