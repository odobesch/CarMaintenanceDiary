using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Model.Entities
{
    public class UserModel
    {
        public UserModel()
        {
            UserRoles = new List<UserRoleModel>();
        }
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public virtual ICollection<UserRoleModel> UserRoles { get; set; }
    }
}
