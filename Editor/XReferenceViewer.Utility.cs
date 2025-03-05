using System.Linq;
using UnityEditor;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer
    {
        public static T LoadAssetFromPackage<T>(string packageFilePath) where T : UnityEngine.Object
        {
            // 记录日志，帮助调试
            UnityEngine.Debug.Log($"尝试加载资源: {packageFilePath}");
            
            // try to load as a package path
            var possibleAssetFilePath = $"Assets/{packageFilePath}";
            UnityEngine.Debug.Log($"尝试路径1: {possibleAssetFilePath}");
            var asset = AssetDatabase.LoadAssetAtPath<T>(possibleAssetFilePath);
            if (asset != null)
                return asset;

            // 直接尝试从本地包路径加载
            var packagePath = $"Packages/com.xframe.xreferenceviewer/{packageFilePath}";
            UnityEngine.Debug.Log($"尝试路径2: {packagePath}");
            asset = AssetDatabase.LoadAssetAtPath<T>(packagePath);
            if (asset != null)
                return asset;
            
            // 尝试从absolute path加载
            var absolutePackagePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(
                    AssetDatabase.FindAssets("t:Script XReferenceViewer")[0])), 
                    "../" + packageFilePath);
            UnityEngine.Debug.Log($"尝试路径3: {absolutePackagePath}");
            asset = AssetDatabase.LoadAssetAtPath<T>(absolutePackagePath);
            if (asset != null)
                return asset;

            // 最后一种尝试方法 - 直接加载包本身目录下的资源
            var directPackagePath = packageFilePath.Replace("XReferenceViewer/", "");
            UnityEngine.Debug.Log($"尝试路径4: {directPackagePath}");
            asset = AssetDatabase.LoadAssetAtPath<T>(directPackagePath);
            if (asset != null)
                return asset;
                
            // 尝试直接从文件系统加载
            try 
            {
                string fullPath = System.IO.Path.Combine("/Users/wuchunpeng/Documents/XReferenceViewer", packageFilePath.Replace("XReferenceViewer/", ""));
                UnityEngine.Debug.Log($"尝试路径5 (文件系统): {fullPath}");
                
                // 对于不同类型的资源，使用不同的加载方式
                if (typeof(T) == typeof(UnityEngine.UIElements.StyleSheet))
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        string ussContent = System.IO.File.ReadAllText(fullPath);
                        UnityEngine.Debug.Log($"读取到USS内容: {ussContent.Length} 字符");
                        
                        // 创建临时USS文件在Assets目录下
                        string tempPath = "Assets/Temp_Style.uss";
                        System.IO.File.WriteAllText(tempPath, ussContent);
                        AssetDatabase.ImportAsset(tempPath);
                        
                        var tempAsset = AssetDatabase.LoadAssetAtPath<T>(tempPath);
                        if (tempAsset != null)
                        {
                            UnityEngine.Debug.Log("成功从临时文件加载样式表");
                            return tempAsset;
                        }
                    }
                }
                else if (typeof(T) == typeof(UnityEngine.UIElements.VisualTreeAsset))
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        string uxmlContent = System.IO.File.ReadAllText(fullPath);
                        UnityEngine.Debug.Log($"读取到UXML内容: {uxmlContent.Length} 字符");
                        
                        // 创建临时UXML文件在Assets目录下
                        string tempPath = "Assets/Temp_Node.uxml";
                        System.IO.File.WriteAllText(tempPath, uxmlContent);
                        AssetDatabase.ImportAsset(tempPath);
                        
                        var tempAsset = AssetDatabase.LoadAssetAtPath<T>(tempPath);
                        if (tempAsset != null)
                        {
                            UnityEngine.Debug.Log("成功从临时文件加载UXML");
                            return tempAsset;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"从文件系统加载资源时出错: {e.Message}");
            }

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