using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.CheckEmailAvailability;

namespace TC.Agro.Identity.Tests.Application.UseCases.CheckEmailAvailability;

public sealed class CheckEmailAvailabilityQueryHandlerTests
{
    private readonly IUserReadStore _readStore = A.Fake<IUserReadStore>();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_ShouldReturnStoreAvailability(bool isAvailable)
    {
        const string email = "candidate@tcagro.com";

        A.CallTo(() => _readStore.IsEmailAvailableAsync(email, A<CancellationToken>._)).Returns(isAvailable);

        var sut = new CheckEmailAvailabilityQueryHandler(_readStore);

        var result = await sut.ExecuteAsync(new CheckEmailAvailabilityQuery { Email = email }, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe(email);
        result.Value.IsAvailable.ShouldBe(isAvailable);
    }
}
