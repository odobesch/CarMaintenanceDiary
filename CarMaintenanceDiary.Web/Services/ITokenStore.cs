namespace CarMaintenanceDiary.Web.Services
{
    public interface ITokenStore
    {
        string? GetToken();
        string? GetRefreshToken();
        Task SetTokensAsync(string? accessToken, string? refreshToken);
        Task SetTokenAsync(string? accessToken);
        event Action<string?>? TokenChanged;
    }
}