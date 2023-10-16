using System.Collections.Generic;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer
    {
        private class AssetType
        {
            private static readonly AssetType[] FILTERS =
            {
                new AssetType("Scene", ".unity"),
                new AssetType("Prefab", ".prefab"),
                new AssetType("Model", ".3df", ".3dm", ".3dmf", ".3dv", ".3dx", ".c5d", ".lwo", ".lws", ".ma", ".mb",
                    ".mesh", ".vrl", ".wrl", ".wrz", ".fbx", ".dae", ".3ds", ".dxf", ".obj", ".skp", ".max", ".blend"),
                new AssetType("Material", ".mat", ".cubemap", ".physicsmaterial"),
                new AssetType("Texture", ".ai", ".apng", ".png", ".bmp", ".cdr", ".dib", ".eps", ".exif", ".ico",
                    ".icon",
                    ".j", ".j2c", ".j2k", ".jas", ".jiff", ".jng", ".jp2", ".jpc", ".jpe", ".jpeg", ".jpf", ".jpg",
                    "jpw",
                    "jpx", "jtf", ".mac", ".omf", ".qif", ".qti", "qtif", ".tex", ".tfw", ".tga", ".tif", ".tiff",
                    ".wmf",
                    ".psd", ".exr", ".rendertexture"),
                new AssetType("Video", ".asf", ".asx", ".avi", ".dat", ".divx", ".dvx", ".mlv", ".m2l", ".m2t", ".m2ts",
                    ".m2v", ".m4e", ".m4v", "mjp", ".mov", ".movie", ".mp21", ".mp4", ".mpe", ".mpeg", ".mpg", ".mpv2",
                    ".ogm", ".qt", ".rm", ".rmvb", ".wmv", ".xvid", ".flv"),
                new AssetType("Audio", ".mp3", ".wav", ".ogg", ".aif", ".aiff", ".mod", ".it", ".s3m", ".xm"),
                new AssetType("Script", ".cs", ".js", ".boo", ".h"),
                new AssetType("Text", ".txt", ".json", ".xml", ".bytes", ".sql"),
                new AssetType("Shader", ".shader", ".cginc"),
                new AssetType("Animation", ".anim", ".controller", ".overridecontroller", ".mask"),
                new AssetType("Unity Asset", ".asset", ".guiskin", ".flare", ".fontsettings", ".prefs"),
                new AssetType("Others") //
            };

            public HashSet<string> extension;
            public string name;

            public AssetType(string name, params string[] exts)
            {
                this.name = name;
                extension = new HashSet<string>();
                for (var i = 0; i < exts.Length; i++)
                {
                    extension.Add(exts[i]);
                }
            }
        }
    }
}