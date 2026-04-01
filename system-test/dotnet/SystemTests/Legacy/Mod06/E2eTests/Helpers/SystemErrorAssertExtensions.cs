using Driver.Port.Shop.Dtos.Error;
using Shouldly;

namespace SystemTests.Legacy.Mod06.E2eTests.Helpers;

public static class SystemErrorAssertExtensions
{
    public static void ShouldHaveMessageAndField(this SystemError error, string expectedMessage, string expectedField, string expectedFieldMessage)
    {
        error.Message.ShouldBe(expectedMessage);
        error.Fields.ShouldNotBeNull();
        error.Fields!.Any(f => f.Field == expectedField && f.Message == expectedFieldMessage).ShouldBeTrue(
            $"Expected field error {{ field: '{expectedField}', message: '{expectedFieldMessage}' }}. Actual: [{string.Join(", ", error.Fields.Select(f => $"{{ field: '{f.Field}', message: '{f.Message}' }}"))}]");
    }
}











