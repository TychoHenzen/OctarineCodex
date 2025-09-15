using System;
using Microsoft.Xna.Framework.Content;

namespace OctarineCodex.Infrastructure.MonoGame;

public class ContentManagerService : IContentManagerService
{
    private readonly ContentManager _contentManager;

    public ContentManagerService(ContentManager contentManager)
    {
        _contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
    }

    public T Load<T>(string assetName)
    {
        return _contentManager.Load<T>(assetName);
    }

    public string RootDirectory => _contentManager.RootDirectory;
}
