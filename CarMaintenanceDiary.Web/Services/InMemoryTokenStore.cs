namespace CarMaintenanceDiary.Web.Services
{
    public class InMemoryTokenStore : ITokenStore
    {
        private string? _token;
        private string? _refresh;
        public event Action<string?>? TokenChanged;
        public string? GetToken() => _token;
        public string? GetRefreshToken() => _refresh;
        public Task SetTokensAsync(string? token, string? refreshToken)
        {
            _token = token;
            _refresh = refreshToken;
            TokenChanged?.Invoke(_token);
            return Task.CompletedTask;
        }
        public Task SetTokenAsync(string? accessToken)
        {
            _token = accessToken;
            TokenChanged?.Invoke(_token);
            return Task.CompletedTask;
        }
    }
}