using UnityEditor;
using UnityEngine;

namespace MMD4MaterialTools
{
    public static class Helpers
    {
        /// <summary>
        /// Save material to assets
        /// </summary>
        /// <param name="hdrpMaterial">The material will be saved</param>
        /// <param name="file">Selected texture file</param>
        public static void SaveMaterial(Material hdrpMaterial, UnityEngine.Object file)
        {
            var path = AssetDatabase.GetAssetPath(file.GetInstanceID()).Split('/');
            var filename = path[path.Length - 1];
            filename = filename.Remove(filename.LastIndexOf('.')) + ".mat";
            path[path.Length - 2] = "Materials";

            var fixedPath = "";
            for (int i = 0; i < path.Length - 1; i++)
            {
                fixedPath += path[i] + "/";
            }
            if (!AssetDatabase.IsValidFolder(fixedPath.Remove(fixedPath.Length - 1)))
            {
                var p = fixedPath.Remove(fixedPath.Length - 11);
                AssetDatabase.CreateFolder(p, "Materials");
            }
            AssetDatabase.CreateAsset(hdrpMaterial, fixedPath + filename);
        }

        /// <summary>
        /// Save material to assets
        /// </summary>
        /// <param name="hdrpMaterial">The material will be saved</param>
        /// <param name="file">Selected texture file</param>
        /// <param name="newFilename">File name</param>
        public static void SaveMaterial(Material hdrpMaterial, UnityEngine.Object file,string newFilename)
        {
            var path = AssetDatabase.GetAssetPath(file.GetInstanceID()).Split('/');
            path[path.Length - 2] = "Materials";

            var fixedPath = string.Empty;
            for (int i = 0; i < path.Length - 1; i++)
            {
                fixedPath += path[i] + "/";
            }
            if (!AssetDatabase.IsValidFolder(fixedPath.Remove(fixedPath.Length - 1)))
            {
                // 11 means the length of "Materials/" add 1
                var p = fixedPath.Remove(fixedPath.Length - 11);
                AssetDatabase.CreateFolder(p, "Materials");
            }
            AssetDatabase.CreateAsset(hdrpMaterial, fixedPath + newFilename+".mat");
        }

        /// <summary>
        /// Set material 
        /// </summary>
        /// <param name="material"></param>
        public static Material CreateMaterial()
        {
#if UNITY_2018
            return new Material(Shader.Find("HDRenderPipeline/Lit"));
#endif

#if UNITY_2019
            return new Material(Shader.Find("HDRP/Lit"));
#endif
        }
    }
}
