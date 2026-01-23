namespace TC.Agro.Identity.Domain.Aggregates
{
    [ExcludeFromCodeCoverage]
    public static class UserDomainErrors
    {
        public static readonly DomainError NotFound = new(
            "User.NotFound",
            "The user with the specified identifier was not found",
            "User.NotFound");

        public static readonly DomainError UserNotFound = new(
            "User",
            "User not found.",
            "User.NotFound");

        public static readonly DomainError UserNotFoundOrInactive = new(
            "User",
            "User not found or is inactive.",
            "User.NotFoundOrInactive");

        public static readonly DomainError InvalidCredentials = new(
            "User|Password",
            "Email or password provided are invalid.",
            "User.InvalidCredentials");

        public static readonly DomainError CreateUser = new(
            "User.CreateUser",
            "An error occurred while creating the user.",
            "User.CreateUser");

        public static readonly DomainError EmailAlreadyExists = new(
            "Email",
            "The email address already exists.",
            "User.EmailAlreadyExists");

        public static readonly DomainError JwtSecretKeyNotConfigured = new(
            "JWTSecretKey",
            "JWT secret key is not configured.",
            "JWT.SecretKeyNotConfigured");
    }
}
