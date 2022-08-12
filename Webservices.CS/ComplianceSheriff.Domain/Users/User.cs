using ComplianceSheriff.UserMapping;
using System;
using System.Security.Principal;

namespace ComplianceSheriff.Users
{
    public class User : UserMapper, IPrincipal
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string TempPassword { get; set; }

        public string VerificationToken { get; set; }

        public string TimeZone { get; set; }

        public int UserGroupId { get; set; }

        public string UserGroupName { get; set; }

        public IIdentity Identity { get; set; }


        public User()
        {

        }

        public User(string userName)
        {
            Name = userName;
            Identity = new GenericIdentity(this.Name);
        }

        public User(string userName, string password)
        {
            Name = userName;
            Password = password;
            Identity = new GenericIdentity(this.Name);
        }

        public User(string userName, 
                    string password, 
                    string firstName, 
                    string lastName,
                    string emailAddress,
                    string orgId,
                    string userGroupName = "",
                    string tempPassword = "",
                    string verificationToken = "")
        {
            Name = userName;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            OrganizationId = orgId;
            UserGroupName = userGroupName;
            TempPassword = tempPassword;
            VerificationToken = verificationToken;
            Identity = new GenericIdentity(this.Name);
        }

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}
