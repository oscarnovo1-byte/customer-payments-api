namespace CustomerPayments.Api.DTOs;

public sealed record LoginRequest(
    string Email,
    string Password
);

public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
);

public sealed record RefreshTokenRequest(
    string RefreshToken
);

public sealed record RevokeRefreshTokenRequest(
    string RefreshToken
);