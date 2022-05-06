using MMD4UnityTools.Editor.Utils;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MMD4UnityTools.Editor
{
    public class TextureCombinerEditorWindow : EditorWindow
    {
        private Texture2D a;

        private Texture2D b;

        private Texture2D g;

        private bool invertA;

        private bool invertB;

        private bool invertG;

        private bool invertR;

        private Texture2D r;

        [MenuItem("MMDExtensions/Texture/Texture Combiner")]
        public static void OpenWindow()
        {
            GetWindow<TextureCombinerEditorWindow>("Texture Combiner").Show();
        }

        private void Combine()
        {
            var tex = new[] { r, g, b, a }.Where(x => x != null).FirstOrDefault();

            if (tex == null)
            {
                return;
            }

            if (!r) r = Texture2D.blackTexture;
            if (!g) g = Texture2D.blackTexture;
            if (!b) b = Texture2D.blackTexture;
            if (!a) a = Texture2D.blackTexture;

            var oneMinusR = invertR;
            var oneMinusG = invertG;
            var oneMinusB = invertB;
            var oneMinusA = invertA;

            var width = tex?.width ?? 1024;
            var height = tex?.height ?? 1024;
            var compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/MMD4UnityTools/Texture/Editor/CombineTexture.compute");
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { enableRandomWrite = true };
            rt.Create();

            compute.SetTexture(0, "Result", rt);
            compute.SetTexture(0, "R", r);
            compute.SetTexture(0, "G", g);
            compute.SetTexture(0, "B", b);
            compute.SetTexture(0, "A", a);
            compute.SetBool("oneMinusR", oneMinusR);
            compute.SetBool("oneMinusG", oneMinusG);
            compute.SetBool("oneMinusB", oneMinusB);
            compute.SetBool("oneMinusA", oneMinusA);

            compute.Dispatch(0, width / 8, height / 8, 1);

            var result = new Texture2D(width, height, TextureFormat.ARGB32, false);

            var temp = RenderTexture.active;
            RenderTexture.active = rt;

            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = temp;
            rt.Release();

            var regex = new Regex(@"\w*");
            var name = regex.Match(tex.name).Groups[0].Value;

            var path = EditorUtility.SaveFilePanelInProject("Save texture to...", $"{name}_MASK", "png", "", Path.GetDirectoryName(AssetDatabase.GetAssetPath(tex)));
            if (path != null)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.WriteAllBytes(path, result.EncodeToPNG());
                AssetDatabase.Refresh();
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            r = r.ObjectField("R: Metallic", false);
            invertR = EditorGUILayout.Toggle("Invert R", invertR);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            g = g.ObjectField("R: AO", false);
            invertG = EditorGUILayout.Toggle("Invert G", invertG);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            b = b.ObjectField("R: DetailMask", false);
            invertB = EditorGUILayout.Toggle("Invert B", invertB);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            a = a.ObjectField("R: Smoothness", false);
            invertA = EditorGUILayout.Toggle("Invert A", invertA);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Combine"))
            {
                Combine();
            }
        }
    }
}