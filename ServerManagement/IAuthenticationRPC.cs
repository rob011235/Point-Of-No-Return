public interface IAuthenticationRPC
{
    void AuthenticatePlayer(string userId, string password);

    void AuthenticationFailed(string errorMessage);

    void AuthenticationSucceeded(string sessionToken);

    void StartGame();
}
