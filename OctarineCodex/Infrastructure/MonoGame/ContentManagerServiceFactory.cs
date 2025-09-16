using System;
using Microsoft.Xna.Framework.Content;

namespace OctarineCodex.Infrastructure.MonoGame;

/// <summary>
///     Factory that provides ContentManagerService after ContentManager is available
/// </summary>
public class ContentManagerServiceFactory : IContentManagerService
{
    private ContentManagerService? _actualService;
    private bool _isInitialized;

    public string RootDirectory
    {
        get
        {
            EnsureInitialized();
            return _actualService!.RootDirectory;
        }
    }

    public T Load<T>(string assetName)
    {
        EnsureInitialized();
        return _actualService!.Load<T>(assetName);
    }

    public void Initialize(ContentManager contentManager)
    {
        _actualService = new ContentManagerService(contentManager);
        _isInitialized = true;
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized || _actualService == null)
        {
            throw new InvalidOperationException(
                "ContentManagerService has not been initialized. Call Initialize() first.");
        }
    }
}
