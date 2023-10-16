using System.Linq;
using UnityEditor;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer
    {
        public static T LoadAssetFromPackage<T>(string packageFilePath) where T : UnityEngine.Object
        {
            // try to load as a package path
            var possibleAssetFilePath = $"Assets/{packageFilePath}";
            var asset = AssetDatabase.LoadAssetAtPath<T>(possibleAssetFilePath);
            if (asset != null)
                return asset;

            // try to convert path to a package path from a presumed package display path
            var splits = packageFilePath.Split('/');
            if (splits.Length >= 1)
            {
                var possiblePackageDisplayName = splits[0];
                var possiblePackageName = PackageDisplayNameToPackageName(possiblePackageDisplayName);
                if (!string.IsNullOrEmpty(possiblePackageName))
                {
                    splits[1] = possiblePackageName;
                    var possiblePackageFilePath = string.Join('/', splits);

                    var possibleAsset = AssetDatabase.LoadAssetAtPath<T>(possiblePackageFilePath);
                    if (possibleAsset != null)
                        return possibleAsset;
                }
            }

            return null;
        }

        private static string PackageDisplayNameToPackageName(string packageDisplayName)
        {
            var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

            return packages
                .Where(package => package.displayName == packageDisplayName)
                .Select(package => package.name)
                .FirstOrDefault();
        }
    }
}