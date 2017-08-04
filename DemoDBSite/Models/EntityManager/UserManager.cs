using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DemoDBSite.Models.DB;
using DemoDBSite.Models.ViewModel;

namespace DemoDBSite.Models.EntityManager
{
    public class UserManager
    {
        public void AddUserAccount(UserSignUpView user)
        {
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                SYSUser SU = new SYSUser();
                SU.LoginName = user.LoginName;
                SU.PasswordEncryptedText = user.Password;
                SU.RowCreatedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SU.RowModifiedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SU.RowCreatedDateTime = DateTime.Now;
                SU.RowModifiedDateTime = DateTime.Now;

                db.SYSUsers.Add(SU);
                db.SaveChanges();

                SYSUserProfile SUP = new SYSUserProfile();
                SUP.SYSUserID = SU.SYSUserID;
                SUP.FirstName = user.FirstName;
                SUP.LastName = user.LastName;
                SUP.Gender = user.Gender;
                SUP.RowCreatedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SUP.RowModifiedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SUP.RowCreatedDateTime = DateTime.Now;
                SUP.RowModifiedDateTime = DateTime.Now;

                db.SYSUserProfiles.Add(SUP);
                db.SaveChanges();

                if (user.LOOKUPRoleID > 0)
                {
                    SYSUserRole SUR = new SYSUserRole();
                    SUR.LOOKUPRoleID = user.LOOKUPRoleID;
                    SUR.SYSUserID = user.SYSUserID;
                    SUR.IsActive = true;
                    SUR.RowCreatedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                    SUR.RowModifiedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                    SUR.RowCreatedDateTime = DateTime.Now;
                    SUR.RowModifiedDateTime = DateTime.Now;

                    db.SYSUserRoles.Add(SUR);
                    db.SaveChanges();
                }
            }
        }

        public bool IsLoginNameExist(string loginName)
        {
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                return db.SYSUsers.Where(o => o.LoginName.Equals(loginName)).Any();
            }
        }

        public string GetUserPassword(string loginName)
        {
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                var user = db.SYSUsers.Where(o => o.LoginName.ToLower().Equals(loginName));
                if (user.Any())
                    return user.FirstOrDefault().PasswordEncryptedText;
                else
                    return string.Empty;
            }
        }
        public bool IsUserInRole(string loginName, string roleName)
        {
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                SYSUser SU = db.SYSUsers.Where(o => o.LoginName.ToLower().Equals(loginName))?.FirstOrDefault();
                if (SU != null)
                {
                    var roles = from q in db.SYSUserRoles
                                join r in db.LOOKUPRoles on q.LOOKUPRoleID equals r.LOOKUPRoleID
                                where r.RoleName.Equals(roleName) && q.SYSUserID.Equals(SU.SYSUserID)
                                select r.RoleName;
                    if (roles != null)
                    {
                        return roles.Any();
                    }
                }
                return false;
            }
        }
        public List<LOOKUPAvailableRole> GetAllRoles()
        {
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                var roles = db.LOOKUPRoles.Select(o => new LOOKUPAvailableRole
                {
                    LOOKUPRoleID = o.LOOKUPRoleID,
                    RoleName = o.RoleName,
                    RoleDescription = o.RoleDescription
                }).ToList();
                return roles;
            }
        }
        public int GetUserID(string loginName)
        {
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                var user = db.SYSUsers.Where(o => o.LoginName.Equals(loginName));
                if (user.Any())
                    return user.FirstOrDefault().SYSUserID;
            }
            return 0;
        }
        public List<UserProfileView> GetAllUserProfiles()
        {
            List<UserProfileView> profiles = new List<UserProfileView>();
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                UserProfileView UPV;
                var users = db.SYSUsers.ToList();
                foreach (SYSUser u in db.SYSUsers)
                {
                    UPV = new UserProfileView();
                    UPV.SYSUserID = u.SYSUserID;
                    UPV.LoginName = u.LoginName;
                    UPV.Password = u.PasswordEncryptedText;

                    var SUP = db.SYSUserProfiles.Find(u.SYSUserID);
                    if (SUP != null)    
                    {
                        UPV.FirstName = SUP.FirstName;
                        UPV.LastName = SUP.LastName;
                        UPV.Gender = SUP.Gender;
                    }

                    var SUR = db.SYSUserRoles.Where(o => o.SYSUserID.Equals(u.SYSUserID));
                    if (SUR.Any())
                    {
                        var userRole = SUR.FirstOrDefault();
                        UPV.LOOKUPRoleID = userRole.LOOKUPRoleID;
                        UPV.RoleName = userRole.LOOKUPRole.RoleName;
                        UPV.IsRoleActive = userRole.IsActive;
                    }
                    profiles.Add(UPV);
                }
            }
            return profiles;
        }
        public UserDataView GetUserDataView(string loginName)
        {
            UserDataView UDV = new UserDataView();
            List<UserProfileView> profiles = GetAllUserProfiles();
            List<LOOKUPAvailableRole> roles = GetAllRoles();

            int? userAssignedRoleID = 0, userID = 0;
            string userGender = string.Empty;

            userID = GetUserID(loginName);
            using (BCDemoDBEntities db = new BCDemoDBEntities())
            {
                userAssignedRoleID = db.SYSUserRoles.Where(o => o.SYSUserID == userID)?.FirstOrDefault().LOOKUPRoleID;
                userGender = db.SYSUserProfiles.Where(o => o.SYSUserID == userID).FirstOrDefault()?.Gender;
            }

            List<Gender> genders = new List<Gender>();
            genders.Add(new Gender { Text = "Male", Value = "M" });
            genders.Add(new Gender { Text = "Female", Value = "F" });

            UDV.UserProfile = profiles;
            UDV.UserRoles = new UserRoles { SelectedRoleID = userAssignedRoleID, UserRoleList = roles };
            UDV.UserGender = new UserGender { SelectedGender = userGender, Gender = genders };
            return UDV;
        }
    }
}