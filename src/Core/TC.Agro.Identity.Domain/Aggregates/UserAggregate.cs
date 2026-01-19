namespace TC.Agro.Identity.Domain.Aggregates
{
    public sealed class UserAggregate : BaseAggregateRoot
    {
        public string Name { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public Password PasswordHash { get; private set; } = default!;
        public Role Role { get; private set; } = default!;

        // Construtor privado para factories
        private UserAggregate(Guid id) : base(id) { }

        #region Factories e Criação

        public static Result<UserAggregate> Create(string name, string emailValue, string username, string passwordValue, string roleValue)
        {
            var emailResult = ValueObjects.Email.Create(emailValue);
            var passwordResult = Password.Create(passwordValue);
            var roleResult = ValueObjects.Role.Create(roleValue);

            var errors = new List<ValidationError>();
            errors.AddErrorsIfFailure(emailResult);
            errors.AddErrorsIfFailure(passwordResult);
            errors.AddErrorsIfFailure(roleResult);
            errors.AddRange(ValidateNameAndUsername(name, username));

            if (errors.Count > 0) return Result.Invalid(errors.ToArray());

            return CreateAggregate(name, emailResult.Value, username, passwordResult.Value, roleResult.Value);
        }

        private static Result<UserAggregate> CreateAggregate(string name, Email email, string username, Password password, Role role)
        {
            var aggregate = new UserAggregate(Guid.NewGuid());
            var @event = new UserCreatedDomainEvent(aggregate.Id, name, email.Value, username, password.Hash, role.Value, DateTimeOffset.UtcNow);
            aggregate.ApplyEvent(@event);
            return Result.Success(aggregate);
        }

        #endregion

        #region Domain Events Apply

        public void Apply(UserCreatedDomainEvent @event)
        {
            SetId(@event.AggregateId);
            Name = @event.Name;
            Email = ValueObjects.Email.FromDb(@event.Email).Value;
            Username = @event.Username;
            PasswordHash = Password.FromHash(@event.Password).Value;
            Role = ValueObjects.Role.FromDb(@event.Role).Value;
            SetCreatedAt(@event.OccurredOn);
            SetActivate();
        }

        private void ApplyEvent(BaseDomainEvent @event)
        {
            AddNewEvent(@event);
            switch (@event)
            {
                case UserCreatedDomainEvent createdEvent: Apply(createdEvent); break;
                    ////case UserUpdatedDomainEvent updatedEvent: Apply(updatedEvent); break;
                    ////case UserPasswordChangedDomainEvent passwordChangedEvent: Apply(passwordChangedEvent); break;
                    ////case UserRoleChangedDomainEvent roleChangedEvent: Apply(roleChangedEvent); break;
                    ////case UserActivatedDomainEvent activatedEvent: Apply(activatedEvent); break;
                    ////case UserDeactivatedDomainEvent deactivatedEvent: Apply(deactivatedEvent); break;
            }
        }

        #endregion

        #region Validation Helpers

        private static IEnumerable<ValidationError> ValidateName(string name)
        {
            var maxLength = 200;
            if (string.IsNullOrWhiteSpace(name))
                yield return new ValidationError($"{nameof(Name)}.Required", "Name is required.");
            else if (name.Length > maxLength)
                yield return new ValidationError($"{nameof(Name)}.TooLong", $"Name must be at most {maxLength} characters.");
        }

        private static IEnumerable<ValidationError> ValidateUsername(string username)
        {
            var maxLength = 50;
            var minLength = 3;
            var regex = new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);

            if (string.IsNullOrWhiteSpace(username))
                yield return new ValidationError($"{nameof(Username)}.Required", "Username is required.");
            else if (username.Length < minLength)
                yield return new ValidationError($"{nameof(Username)}.TooShort", $"Username must be at least {minLength} characters.");
            else if (username.Length > maxLength)
                yield return new ValidationError($"{nameof(Username)}.TooLong", $"Username must be at most {maxLength} characters.");
            else if (!regex.IsMatch(username))
                yield return new ValidationError($"{nameof(Username)}.InvalidFormat", "Username contains invalid characters.");
        }

        private static IEnumerable<ValidationError> ValidateNameAndUsername(string name, string username)
        {
            foreach (var error in ValidateName(name)) yield return error;
            foreach (var error in ValidateUsername(username)) yield return error;
        }

        #endregion

        #region Domain Events

        public record UserCreatedDomainEvent(Guid AggregateId, string Name, string Email, string Username, string Password, string Role, DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);
        public record UserUpdatedDomainEvent(Guid AggregateId, string Name, string Email, string Username, DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);
        public record UserPasswordChangedDomainEvent(Guid AggregateId, string NewPassword, DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);
        public record UserRoleChangedDomainEvent(Guid AggregateId, string NewRole, DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);
        public record UserActivatedDomainEvent(Guid AggregateId, DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);
        public record UserDeactivatedDomainEvent(Guid AggregateId, DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        #endregion
    }
}
