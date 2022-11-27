using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;

namespace MMD4UnityTools.Editor
{
    public class AlembicImportEditor
    {
        [MenuItem("Assets/MMDExtensions/Alembic/Import From File %#&a")]
        public static void ImportAlembicFromFileMenu()
        {
            var savePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = GetCurrentFolder();
            }
            savePath = Path.HasExtension(savePath) ? Path.GetDirectoryName(savePath).Replace("\\", "/") : savePath;
            var path = EditorUtility.OpenFilePanel("Select alembic file to import...", "", "abc");
            if (!string.IsNullOrEmpty(path))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                CreateAlembicPrefabFromFile(path, $"{savePath}/{fileName}.prefab");
            }
        }

        public static string GetCurrentFolder()
        {
            var getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            return getActiveFolderPath.Invoke(null, new object[0]).ToString();
        }

        public static void CreateAlembicPrefabFromFile(string alembicPath, string assetPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(alembicPath);
            var parent = new GameObject(fileName);
            var abc = parent.AddComponent<AlembicStreamPlayer>();
            abc.LoadFromFile(alembicPath);
            abc.Settings.ImportCameras = false;
            abc.Settings.Tangents = UnityEngine.Formats.Alembic.Sdk.TangentsMode.None;
            foreach (var renderer in parent.GetComponentsInChildren<Renderer>())
            {
                renderer.rayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.DynamicGeometry;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(parent, assetPath);
            EditorGUIUtility.PingObject(prefab);
            Object.DestroyImmediate(parent);
        }
    }

    //class AlembicPostprocessor : AssetPostprocessor
    //{
    //    void OnPreprocessAsset()
    //    {
    //        foreach (var path in DragAndDrop.paths)
    //        {
    //            if (!assetImporter.importSettingsMissing) continue;
    //            if (!Path.HasExtension(path)) continue;
    //            if (!Path.GetExtension(path.ToLower()).Equals(".abc")) continue;

    //            var savePath = Path.HasExtension(assetPath) ? Path.GetDirectoryName(assetPath).Replace("\\", "/") : assetPath;
    //            var fileName = Path.GetFileNameWithoutExtension(path);
    //            var pathToSave = $"{savePath}/{fileName}.prefab";
    //            File.Delete(assetPath);
    //            //AssetDatabase.DeleteAsset(assetPath);
    //            if (File.Exists(pathToSave))
    //            {
    //                continue;
    //            }

    //            AlembicImportEditor.CreateAlembicPrefabFromFile(path, pathToSave);
    //        }
    //    }
    //} 
}