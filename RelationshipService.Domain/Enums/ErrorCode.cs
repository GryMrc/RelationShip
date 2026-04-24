namespace RelationshipService.Domain.Enums;

public enum ErrorCode
{
    None = 0,

    #region Auth
    Auth_InvalidGoogleToken,
    Auth_UnsupportedProvider,
    Auth_TokenNotFound,
    Auth_InvalidRefreshToken,
    Auth_AppleAuthNotImplemented,
    Auth_TokenRequired,
    Auth_ProviderRequired,
    Auth_InvalidProvider,
    #endregion

    #region General
    General_InternalServerError,
    Validation_Error,
    #endregion
}
