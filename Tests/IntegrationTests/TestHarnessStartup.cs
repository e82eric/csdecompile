using NUnit.Framework;

namespace IntegrationTests;

[SetUpFixture]
public class TestHarnessStartup
{
    [OneTimeSetUp]
    public void Start()
    {
        TestHarness.Init();
    }

    [OneTimeTearDown]
    public void Stop()
    {
        TestHarness.Destroy();
    }
}