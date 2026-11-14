using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Abstractions.DependencyInjection;

/// <summary>
/// A fluent registration builder that writes into one <see cref="IServiceCollection"/>.
/// Optional packages constrain their chain methods on this so they can continue a builder
/// owned by another product, without those products referencing each other.
/// </summary>
public interface IServiceCollectionBuilder
{
    /// <summary>The collection the chained registrations write to.</summary>
    IServiceCollection Services { get; }
}