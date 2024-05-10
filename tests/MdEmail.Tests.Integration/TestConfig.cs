using Microsoft.Extensions.Configuration;

namespace MdEmail.Tests.Integration;

public static class TestConfig
{
    /// <summary>
    /// Loads configuration from user secrets
    /// </summary>
    public static MdEmailConfiguration LoadTestConfigFromUserSecrets()
    {
        ConfigurationBuilder cb = new ConfigurationBuilder();
        cb.AddUserSecrets(typeof(TestConfig).Assembly, optional: false);
        return cb.Build().GetSection("smtp").Get<MdEmailConfiguration>()
            ?? throw new InvalidOperationException("Failed to get config from secrets");
    }

    public static string GetTestRecipientEmailAddress()
    {
        ConfigurationBuilder cb = new ConfigurationBuilder();
        cb.AddUserSecrets(typeof(TestConfig).Assembly, optional: false);
        return cb.Build().GetSection("testRecipient").Get<string>() ?? throw new InvalidOperationException("testRecipient not set in user secrets");
    }
}