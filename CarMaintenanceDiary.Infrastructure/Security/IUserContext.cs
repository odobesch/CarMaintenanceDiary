using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Infrastructure.Security
{
    public interface IUserContext
    {
        string? GetCurrentUserId();
        bool IsInRole(string role);

        /// <summary>
        /// Returns true if the current user is an Admin or owns the resource identified by <paramref name="resourceOwnerUserId"/>.
        /// </summary>
        bool CanAccessOwnedResource(string? resourceOwnerUserId);
    }
}
