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
using Arena.Custom.HDC.GoogleMaps;
using Arena.Custom.HDC.GoogleMaps.Maps;

namespace ArenaWeb.UserControls.Custom.HDC.GoogleMaps
{
    public partial class JoinSmallGroup : PortalControl
    {
        const String FieldValueEmail = "1";
        const String FieldValuePhone = "2";
        const String FieldValueAddress = "3";
        const String FieldValueComments = "4";

        #region Module Settings

        [BooleanSetting("Notify Group Leader", "Notify the small group leader about the request to join the small group.", true, false)]
        public Boolean NotifyGroupLeaderSetting { get { return Convert.ToBoolean(Setting("NotifyGroupLeader", "true", true)); } }

        [TextSetting("Notify Address", "Enter one or more e-mail addresses, separated by a comma, to be notified of the request to join the small group.", false)]
        public String[] NotifyAddressSetting { get { return Setting("NotifyGroupLeader", "", false).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); } }

        [LookupSetting("New Member Role", "To automatically add the user to the small group select the role to use. (User must be logged in for this to work)", false)]
        public Int32 NewMemberRoleSetting { get { return Convert.ToInt32(Setting("NewMemberRole", "-1", false)); } }

        [CustomListSetting("Available Fields", "Select which fields are available for the user to fill in. Defaults to all fields.", false, "",
            new String[] { "E-mail", "Phone", "Address", "Comments" },
            new String[] { FieldValueEmail, FieldValuePhone, FieldValueAddress, FieldValueComments })]
        public String[] AvailableFieldsSetting { get { return Setting("AvailableFields", "", false).Split(new char[] { ',' }); } }

        #endregion


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueEmail))
                    lbFieldEmail.Visible = false;
                
                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValuePhone))
                    lbFieldPhone.Visible = false;
                
                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueAddress))
                    divFieldAddress.Visible = false;
                
                if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueComments))
                    lbFieldComments.Visible = false;
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
            // Join the member into the small group if that has been requested.
            //
            if (NewMemberRoleSetting != -1 && ArenaContext.Current.Person.PersonID != -1)
            {
                GroupMember gm = new GroupMember(group.GroupID, ArenaContext.Current.Person.PersonID);

                if (gm.GroupID == -1)
                {
                    gm.Active = true;
                    gm.DateJoined = DateTime.Now;
                    gm.GroupID = group.GroupID;
                    gm.MemberNotes = tbComments.Text;
                    gm.Role = new Lookup(NewMemberRoleSetting);

                    gm.Save(ArenaContext.Current.Organization.OrganizationID,
                        (ArenaContext.Current.Person.PersonID != -1 ? ArenaContext.Current.User.Identity.Name : "smallgrouplocator"));
                }
            }

            //
            // E-mail the small group leader if the leader should be notified.
            //
            if (NotifyGroupLeaderSetting == true)
            {
                ArenaSendMail.SendMail("daniel@hdcnet.org",
                    "Daniel Hazelbaker",
                    group.Leader.Emails.FirstActive,
                    "subject",
                    MailMessageContents(group, true));
            }

            //
            // E-mail each of the specified contact addresses.
            //
            foreach (String email in NotifyAddressSetting)
            {
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

            sb.AppendFormat("<b>Name:</b> {0}<br />\r\n", tbName.Text);
            sb.AppendFormat("<b>Group ID:</b> {0}<br />\r\n", group.GroupID.ToString());

            if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueEmail))
            {
                sb.AppendFormat("<b>E-mail:</b> {0}<br />\r\n", tbEmail.Text);
            }

            if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValuePhone))
            {
                sb.AppendFormat("<b>Phone:</b> {0}<br />\r\n", tbPhone.Text);
            }

            if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueAddress))
            {
                sb.AppendFormat("<b>Address:</b> {0}, {1}, {2} {3}<br />\r\n", tbStreet.Text, tbCity.Text, tbState.Text, tbZipcode.Text);
            }

            if (AvailableFieldsSetting.Length > 0 && !AvailableFieldsSetting.Contains(FieldValueComments))
            {
                sb.AppendFormat("<b>Comments:</b> {0}<br />\r\n", tbComments.Text);
            }

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