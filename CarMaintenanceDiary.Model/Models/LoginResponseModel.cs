using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Model.Models
{
    public class LoginResponseModel
    {
        public string Token
        {
            get; set;
        }
        public long TokenExpired
        {
            get; set;
        }
        public string RefreshToken
        {
            get; set;
        }
    }
}

