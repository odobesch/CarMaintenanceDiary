using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Application.Interfaces
{
    public interface IAuthTokenAccessor
    {
        Task<string?> GetTokenAsync();
    }
}
