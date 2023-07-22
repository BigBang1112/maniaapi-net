namespace ManiaAPI.NadeoAPI;

sealed record AuthorizationResponse(string AccessToken, string RefreshToken);