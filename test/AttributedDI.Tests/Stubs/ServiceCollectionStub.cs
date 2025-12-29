using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace AttributedDI.Tests.Stubs;

public class ServiceCollectionStub : List<ServiceDescriptor>, IServiceCollection
{
}