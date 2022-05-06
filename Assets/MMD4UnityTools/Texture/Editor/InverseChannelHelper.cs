using System.IO;
using UnityEditor;
using UnityEngine;

namespace MMD4UnityTools.Editor
{
    public static class InverseChannelHelper
    {
        public enum TextureChannel
        {
            R, G, B, A
        }

        public static void InverseMaskChanel(Texture2D texture, TextureChannel channel)
        {
            var compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(@"Assets/MMD4UnityTools/Texture/Editor/HDRPMaskUtils.compute");
            var rt = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true
            };

            compute.SetTexture(0, "Input", texture);
            compute.SetTexture(0, "Result", rt);

            compute.SetBool("InverseMetallic", channel == TextureChannel.R);
            compute.SetBool("InverseAO", channel == TextureChannel.G);
            compute.SetBool("InverseDetail", channel == TextureChannel.B);
            compute.SetBool("InverseSmoothness", channel == TextureChannel.A);

            compute.Dispatch(0, texture.width / 8, texture.height / 8, 1);

            var bytes = rt.ToTexture2D().EncodeToPNG();
            var path = AssetDatabase.GetAssetPath(texture);
            File.Delete(path);
            File.WriteAllBytes(path, bytes);
        }

        [MenuItem("Assets/MMDExtensions/Texture/InversChannel/A")]
        public static void InversMaskSmoothnessA()
        {
            foreach (var texture in Selection.GetFiltered<Texture2D>(SelectionMode.Assets))
            {
                var p = AssetDatabase.GetAssetPath(texture);
                InverseMaskChanel(texture, TextureChannel.A);
                var importer = AssetImporter.GetAtPath(p) as TextureImporter;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/MMDExtensions/Texture/InversChannel/B")]
        public static void InversMaskSmoothnessB()
        {
            foreach (var texture in Selection.GetFiltered<Texture2D>(SelectionMode.Assets))
            {
                var p = AssetDatabase.GetAssetPath(texture);
                InverseMaskChanel(texture, TextureChannel.B);
                var importer = AssetImporter.GetAtPath(p) as TextureImporter;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/MMDExtensions/Texture/InversChannel/G")]
        public static void InversMaskSmoothnessG()
        {
            foreach (var texture in Selection.GetFiltered<Texture2D>(SelectionMode.Assets))
            {
                var p = AssetDatabase.GetAssetPath(texture);
                InverseMaskChanel(texture, TextureChannel.G);
                var importer = AssetImporter.GetAtPath(p) as TextureImporter;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/MMDExtensions/Texture/InversChannel/R")]
        public static void InversMaskSmoothnessR()
        {
            foreach (var texture in Selection.GetFiltered<Texture2D>(SelectionMode.Assets))
            {
                var p = AssetDatabase.GetAssetPath(texture);
                InverseMaskChanel(texture, TextureChannel.R);
                var importer = AssetImporter.GetAtPath(p) as TextureImporter;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }
    }
}