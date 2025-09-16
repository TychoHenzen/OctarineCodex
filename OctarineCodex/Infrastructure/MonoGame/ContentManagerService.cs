using Microsoft.Xna.Framework.Content;

namespace OctarineCodex.Infrastructure.MonoGame;

public class ContentManagerService(ContentManager contentManager) : IContentManagerService
{
    public T Load<T>(string assetName)
    {
        return contentManager.Load<T>(assetName);
    }

    public string RootDirectory => contentManager.RootDirectory;
}
