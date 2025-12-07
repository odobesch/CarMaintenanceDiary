using CarMaintenanceDiary.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserModel> GetUserByLogin(string username, string password);
        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);
    }
}
