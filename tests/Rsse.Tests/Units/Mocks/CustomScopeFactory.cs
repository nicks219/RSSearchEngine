using System;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Tests.Units.Mocks;

/// <summary/> Для тестов
internal class CustomScopeFactory(IServiceProvider serviceProvider) : IServiceScopeFactory
{
    public IServiceScope CreateScope() => serviceProvider.CreateScope();
}
