namespace TC.Agro.Identity.Application.UseCases.ChangePassword
{
    public static class ChangePasswordMapper
    {
        public static ChangePasswordResponse FromAggregate(UserAggregate aggregate)
            => new(
                Id: aggregate.Id,
                Email: aggregate.Email.Value,
                Message: "Password changed successfully.");
    }
}
