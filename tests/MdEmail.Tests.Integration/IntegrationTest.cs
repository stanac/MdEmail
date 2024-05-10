using MdEmail.Contracts;

namespace MdEmail.Tests.Integration;

// ReSharper disable once UnusedMember.Global
public class IntegrationTest
{
    // [Fact] // do not run automatically
    public async Task SendEmailTest()
    {
        MdEmailConfiguration config = TestConfig.LoadTestConfigFromUserSecrets();

        IMdEmailSender sender = new MdEmailSender(config);

        SendEmailRequest request = new SendEmailRequest
        {
            Subject = "test s ubj"
        };
        request.To.Add(TestConfig.GetTestRecipientEmailAddress());

        request.Body.SetMarkdownBody(@"**bold** *italic*

Test msg
");
        await sender.SendAsync(request);
    }
}