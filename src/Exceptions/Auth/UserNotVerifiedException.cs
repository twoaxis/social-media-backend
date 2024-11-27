namespace social_media_backend.Exceptions.Auth;

public class UserNotVerifiedException(int userId) : Exception
{
    public int userId { get; set; } = userId;
}