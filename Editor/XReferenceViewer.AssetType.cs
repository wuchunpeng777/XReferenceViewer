using System.Collections.Generic;
using UnityEngine;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer
    {
        private class AssetType
        {
            private static readonly AssetType[] FILTERS =
            {
                new AssetType("Scene", new Color(255f / 255, 153f / 255, 0, 1), ".unity"),
                new AssetType("Prefab", new Color(33f / 255, 71f / 255, 149f / 255, 1), ".prefab"),
                new AssetType("Model", new Color(102f / 255, 0, 153f / 255, 1), ".3df", ".3dm", ".3dmf", ".3dv", ".3dx",
                    ".c5d", ".lwo", ".lws", ".ma", ".mb",
                    ".mesh", ".vrl", ".wrl", ".wrz", ".fbx", ".dae", ".3ds", ".dxf", ".obj", ".skp", ".max", ".blend"),
                new AssetType("Material", new Color(255f / 255, 153f / 255, 51f / 255, 1), ".mat", ".cubemap",
                    ".physicsmaterial"),
                new AssetType("Texture", new Color(204f / 255, 204f / 255, 0, 1), ".ai", ".apng", ".png", ".bmp",
                    ".cdr", ".dib", ".eps", ".exif", ".ico",
                    ".icon",
                    ".j", ".j2c", ".j2k", ".jas", ".jiff", ".jng", ".jp2", ".jpc", ".jpe", ".jpeg", ".jpf", ".jpg",
                    "jpw",
                    "jpx", "jtf", ".mac", ".omf", ".qif", ".qti", "qtif", ".tex", ".tfw", ".tga", ".tif", ".tiff",
                    ".wmf",
                    ".psd", ".exr", ".rendertexture"),
                new AssetType("Video", new Color(255f / 255, 51f / 255, 51f / 255, 1), ".asf", ".asx", ".avi", ".dat",
                    ".divx", ".dvx", ".mlv", ".m2l", ".m2t", ".m2ts",
                    ".m2v", ".m4e", ".m4v", "mjp", ".mov", ".movie", ".mp21", ".mp4", ".mpe", ".mpeg", ".mpg", ".mpv2",
                    ".ogm", ".qt", ".rm", ".rmvb", ".wmv", ".xvid", ".flv"),
                new AssetType("Audio", new Color(204f / 255, 102f / 255, 255f / 255, 1), ".mp3", ".wav", ".ogg", ".aif",
                    ".aiff", ".mod", ".it", ".s3m", ".xm"),
                new AssetType("Script", Color.blue, ".cs", ".js", ".boo", ".h"),
                new AssetType("Text", Color.gray, ".txt", ".json", ".xml", ".bytes", ".sql"),
                new AssetType("Shader", new Color(166f / 255, 77f / 255, 121f / 255, 1), ".shader", ".cginc"),
                new AssetType("Animation", Color.magenta, ".anim", ".controller", ".overridecontroller", ".mask"),
                new AssetType("Unity Asset", new Color(204f / 255, 25f / 255, 51f / 255, 1), ".asset", ".guiskin",
                    ".flare", ".fontsettings", ".prefs"),
                new AssetType("Others", Color.black) //
            };

            public HashSet<string> extension;
            public string name;
            public Color color;

            public AssetType(string name, Color color, params string[] exts)
            {
                this.color = color;
                this.name = name;
                extension = new HashSet<string>();
                for (var i = 0; i < exts.Length; i++)
                {
                    extension.Add(exts[i]);
                }
            }

            public static AssetType GetAssetType(string ext)
            {
                for (var i = 0; i < FILTERS.Length - 1; i++)
                {
                    if (FILTERS[i].extension.Contains(ext))
                    {
                        return FILTERS[i];
                    }
                }

                return FILTERS[FILTERS.Length - 1];
            }
        }
    }
}