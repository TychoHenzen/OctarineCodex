using OctarineCodex.Application.Services;

namespace OctarineCodex.Infrastructure.MonoGame;

/// <summary>
///     Abstraction over MonoGame ContentManager for dependency injection.
/// </summary>
[Service<ContentManagerService>]
public interface IContentManagerService
{
    /// <summary>
    ///     Gets the content root directory.
    /// </summary>
    string RootDirectory { get; }

    /// <summary>
    ///     Loads a content asset of the specified type.
    /// </summary>
    T Load<T>(string assetName);
}
