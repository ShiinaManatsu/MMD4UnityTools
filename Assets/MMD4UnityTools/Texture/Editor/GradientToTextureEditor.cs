using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GradientToTextureEditor : EditorWindow
{
    [MenuItem("MMDExtensions/Texture/Gradient To Texture")]
    public static void OpenWindow()
    {
        GetWindow<GradientToTextureEditor>("Gradient To Texture").Show();
    }

    private Texture2D texture;
    private int resolution;

    private void OnGUI()
    {
        texture = EditorGUILayout.ObjectField("texture", texture, typeof(Texture2D), false) as Texture2D;
        resolution = EditorGUILayout.IntField(resolution);

        if (GUILayout.Button("Convert"))
        {
            var mat = new Material(Shader.Find("Hidden/GradientToTexture"));
            texture = new Texture2D(resolution, 1, TextureFormat.RGBAFloat, false);
            var rt = RenderTexture.GetTemporary(resolution, 1, 0, RenderTextureFormat.ARGBFloat);
            Graphics.SetRenderTarget(rt);
            GL.Clear(true, true, Color.clear, 0);
            Graphics.Blit(null, rt, mat);

            var temp = RenderTexture.active;
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, resolution, 1), 0, 0);
            texture.Apply();
            RenderTexture.active = temp;

            //Graphics.CopyTexture(rt, texture);
            //System.GC.Collect();
            //Resources.UnloadUnusedAssets();
        }

        if (texture && GUILayout.Button("Save To Asset"))
        {
            texture.Apply();
            var bytes = texture.EncodeToPNG();
            var path = EditorUtility.SaveFilePanelInProject("Save to png...", "Gradient", "png", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                System.IO.File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
            }
        }
    }
}
