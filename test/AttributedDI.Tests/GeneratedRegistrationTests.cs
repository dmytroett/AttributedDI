using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace AttributedDI.Tests
{
    public class GeneratedRegistrationTests
    {
        [Fact]
        public void AddServicesFromAssemblies_WithAssemblyContainingMultipleTypes_RegistersAllTypes()
        {
            // arrange
            var services = new ServiceCollection();
            var assembly = typeof(AssemblyWithMultipleTypesToRegister.Type1).Assembly;

            // act
            services.AddServicesFromAssemblies(assembly);

            // assert
            services.Should().HaveCount(2, "Assembly contains two types with registration attributes");
            services.Should().Contain(d => d.ServiceType == typeof(AssemblyWithMultipleTypesToRegister.Type1));
            services.Should().Contain(d => d.ServiceType == typeof(AssemblyWithMultipleTypesToRegister.Type2));
        }

        [Fact]
        public void AddServicesFromAssemblies_WithAssemblyWithNoTypes_RegistersNothing()
        {
            // arrange
            var services = new ServiceCollection();
            var assembly = typeof(AssemblyWithNoTypesToRegister.AssemblyWithNoTypesToRegisterDescriptor).Assembly;

            // act
            services.AddServicesFromAssemblies(assembly);

            // assert
            services.Should().BeEmpty("Assembly contains no types with registration attributes");
        }

        [Fact]
        public void AddServicesFromAssemblyContaining_Generic_RegistersServicesFromAssembly()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddServicesFromAssemblyContaining<AssemblyWithMultipleTypesToRegister.Type1>();

            // assert
            services.Should().HaveCount(2);
        }

        [Fact]
        public void AddServicesFromAssemblyContaining_Type_RegistersServicesFromAssembly()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddServicesFromAssemblyContaining(typeof(AssemblyWithMultipleTypesToRegister.Type1));

            // assert
            services.Should().HaveCount(2);
        }

        [Fact]
        public void RegisteredServices_HaveCorrectLifetime()
        {
            // arrange
            var services = new ServiceCollection();
            var assembly = typeof(AssemblyWithMultipleTypesToRegister.Type1).Assembly;

            // act
            services.AddServicesFromAssemblies(assembly);

            // assert
            var type1Descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(AssemblyWithMultipleTypesToRegister.Type1));
            type1Descriptor?.Lifetime.Should().Be(ServiceLifetime.Transient, "Type1 uses default Transient lifetime");
        }

        [Fact]
        public void TypeWithMultipleAttributes_RegistersMultipleTimes()
        {
            // arrange
            var services = new ServiceCollection();
            var assembly = typeof(AssemblyWithSingleTypeWithMultipleRegisterAttributes.TypeWithMultipleRegisterAttributes).Assembly;

            // act
            services.AddServicesFromAssemblies(assembly);

            // assert
            services.Should().HaveCount(2, "Type has two registration attributes");
            services.Should().Contain(d => d.ServiceType == typeof(AssemblyWithSingleTypeWithMultipleRegisterAttributes.Interface1));
            services.Should().Contain(d => d.ServiceType == typeof(AssemblyWithSingleTypeWithMultipleRegisterAttributes.Interface2));
        }
    }
}

