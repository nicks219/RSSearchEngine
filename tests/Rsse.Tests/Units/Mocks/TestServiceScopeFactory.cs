using System;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Tests.Units.Mocks;

public class TestServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TestServiceScopeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();
}