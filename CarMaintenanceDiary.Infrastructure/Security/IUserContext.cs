using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Infrastructure.Security
{
    public interface IUserContext
    {
        string? GetCurrentUserId();
        bool IsInRole(string role);
    }
}
