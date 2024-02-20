using Microsoft.Xna.Framework.Content;
using System.Reflection;

namespace CrossPlatformDesktop.Blazor
{
    public interface IContentManagerExtension
    {
        Stream OpenStream(ContentManager content, string assetName);
    }

    public class CustomEmbeddedResourceLoader : IContentManagerExtension
    {
        public Stream OpenStream(ContentManager content, string assetName)
        {
            Assembly asm = this.GetType().Assembly;
            var asmName = asm.GetName().Name;

            var assetFullPath = Path.Combine(asmName, content.RootDirectory, assetName);
            assetFullPath = assetFullPath.Replace('/', '.');

            Stream stream = asm.GetManifestResourceStream(assetFullPath + ".lnx");
            if (stream == null)
            {
                stream = asm.GetManifestResourceStream(assetFullPath + ".IMG");
            }

            return stream;
        }
    }
}
