using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Domain.Service.Configuration;

namespace Rsse.Tests.Units;

[TestClass]
public class HostTests
{
    [TestMethod]
    public async Task Host_ShouldStartAndStop_Correctly()
    {
        // arrange:
        Environment.SetEnvironmentVariable(Constants.AspNetCoreEnvironmentName, Constants.TestingEnvironment);
        string[] args = ["--js false"];
        var mainTask = Program.Main(args);
        var initStatus = mainTask.Status;

        // act:
        var lifetime = Program.ApplicationLifetime ?? throw new NotSupportedException("Non testing environment");
        lifetime.StopApplication();
        await mainTask;
        var finalStatus = mainTask.Status;

        // assert:
        Assert.AreEqual(TaskStatus.WaitingForActivation, initStatus);
        Assert.AreEqual(TaskStatus.RanToCompletion, finalStatus);
    }
}
