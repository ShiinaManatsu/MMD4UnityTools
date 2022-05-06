using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

/// <summary>
/// A collection contains texture tools that create hdrp specified textures and materials.
/// </summary>
public class StandardPBRTextureTool
{
    private static readonly List<string> suffix = new() { "metallic", "ambientOcclusion", "roughness", "OCC", "COLOR", "ROUGH", "SPEC" };

    private const string csPath = "Assets/Editor/MMD4UnityTools/StandardPBR2HDRP/CombineTexture.compute";

    private static void TransformToPBRMask()
    {
        var others = Selection.GetFiltered<Texture2D>(SelectionMode.Assets).ToList();

        foreach (var o in others)
        {
            var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o)) as TextureImporter;
            ti.isReadable = true;
            ti.SaveAndReimport();
        }

        var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(csPath);

        var rt = new RenderTexture(others.First().width, others.First().height, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };

        cs.SetTexture(0, "Metallic", others.Find(x => x.name.Contains("metallic")) ?? others.Find(x => x.name.Contains("SPEC")) ?? Texture2D.blackTexture);
        cs.SetTexture(0, "AO", others.Find(x => x.name.Contains("ambientOcclusion")) ?? others.Find(x => x.name.Contains("OCC")) ?? Texture2D.whiteTexture);
        cs.SetTexture(0, "Roughness", others.Find(x => x.name.Contains("roughness")) ?? others.Find(x => x.name.Contains("ROUGH")) ?? Texture2D.grayTexture);
        cs.SetTexture(0, "Result", rt);

        cs.Dispatch(0, others.First().width, others.First().height, 1);

        var match = suffix.Where(x => others.Any(o => o.name.Contains(x))).First();

        SaveRTToFile(rt, Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(others.First())), others.Find(x => x.name.Contains(match)).name.Replace(match, "MASK")) + ".png");

        try { FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(others.Find(x => x.name.Contains("metallic")) ?? others.Find(x => x.name.Contains("SPEC")))); } catch { }
        try { FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(others.Find(x => x.name.Contains("ambientOcclusion")) ?? others.Find(x => x.name.Contains("OCC")))); } catch { }
        try { FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(others.Find(x => x.name.Contains("roughness")) ?? others.Find(x => x.name.Contains("ROUGH")))); } catch { }

        AssetDatabase.Refresh();
    }

    private static void TransformToPBRMask(string path)
    {
        var others = AssetDatabase.FindAssets("t:texture", new[] { path })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
            .ToList();

        foreach (var o in others)
        {
            var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o)) as TextureImporter;
            ti.isReadable = true;
            ti.SaveAndReimport();
        }

        var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(csPath);

        var rt = new RenderTexture(others.First().width, others.First().height, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };

        cs.SetTexture(0, "Metallic", others.Find(x => x.name.Contains("metallic")) ?? others.Find(x => x.name.Contains("SPEC")) ?? Texture2D.blackTexture);
        cs.SetTexture(0, "AO", others.Find(x => x.name.Contains("ambientOcclusion")) ?? others.Find(x => x.name.Contains("OCC")) ?? Texture2D.whiteTexture);
        cs.SetTexture(0, "Roughness", others.Find(x => x.name.Contains("roughness")) ?? others.Find(x => x.name.Contains("ROUGH")) ?? Texture2D.grayTexture);
        cs.SetTexture(0, "Result", rt);

        cs.Dispatch(0, others.First().width, others.First().height, 1);

        var match = suffix.Where(x => others.Any(o => o.name.Contains(x))).First();

        SaveRTToFile(rt, Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(others.First())), others.Find(x => x.name.Contains(match)).name.Replace(match, "MASK")) + ".png");

        try { FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(others.Find(x => x.name.Contains("metallic")) ?? others.Find(x => x.name.Contains("SPEC")))); } catch { }
        try { FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(others.Find(x => x.name.Contains("ambientOcclusion")) ?? others.Find(x => x.name.Contains("OCC")))); } catch { }
        try { FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(others.Find(x => x.name.Contains("roughness")) ?? others.Find(x => x.name.Contains("ROUGH")))); } catch { }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Create hdrp material by selected files, these files should contains at least a basecolor map, hardcoded name matching.
    /// The selected files will be combined to a HDRP mask texture and create a material from them.
    /// </summary>
    [MenuItem("Assets/MMDExtensions/Materials/FromStandardPBR/Create HDRP Material %&m")]
    public static void CreateHDRPMaterial()
    {
        TransformToPBRMask();
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets).ToList();
        var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));

        var mat = new Material(Shader.Find("HDRP/Lit"));
        mat.mainTexture = textures.Find(x => x.name.ToLower().Contains("basecolor")) ?? textures.Find(x => x.name.Contains("COLOR"));
        mat.SetTexture("_MaskMap", textures.Find(x => x.name.ToLower().Contains("mask")));
        var normal = textures.Find(x => x.name.ToLower().Contains("normal")) ?? textures.Find(x => x.name.Contains("NORM"));
        if (normal != null)
        {
            var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(normal)) as TextureImporter;
            ti.textureType = TextureImporterType.NormalMap;
            ti.SaveAndReimport();
        }
        mat.SetTexture("_NormalMap", normal);
        mat.SetTexture("_HeightMap", textures.Find(x => x.name.ToLower().Contains("height")) ?? textures.Find(x => x.name.Contains("DISP")));

        AssetDatabase.CreateAsset(mat, Path.Combine(path, Path.GetFileName(path)) + ".mat");
    }

    /// <summary>
    /// Create hdrp materials for each selected folder, each folder should contains only one set of pbr textures.
    /// </summary>
    [MenuItem("Assets/MMDExtensions/Materials/FromStandardPBR/Create HDRP Material By selected folders")]
    public static void CreateHDRPMaterialInFolders()
    {
        foreach (var f in Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets).Select(AssetDatabase.GetAssetPath))
        {
            TransformToPBRMask(f);
            var textures = AssetDatabase.FindAssets("t:texture", new[] { f })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
            .ToList();

            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(textures.First()));

            var mat = new Material(Shader.Find("HDRP/Lit"));
            mat.mainTexture = textures.Find(x => x.name.ToLower().Contains("basecolor")) ?? textures.Find(x => x.name.Contains("COLOR"));
            mat.SetTexture("_MaskMap", textures.Find(x => x.name.ToLower().Contains("mask")));
            var normal = textures.Find(x => x.name.ToLower().Contains("normal")) ?? textures.Find(x => x.name.Contains("NORM"));
            if (normal != null)
            {
                var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(normal)) as TextureImporter;
                ti.textureType = TextureImporterType.NormalMap;
                ti.SaveAndReimport();
            }
            mat.SetTexture("_NormalMap", normal);
            mat.SetTexture("_HeightMap", textures.Find(x => x.name.ToLower().Contains("height")) ?? textures.Find(x => x.name.Contains("DISP")));

            AssetDatabase.CreateAsset(mat, Path.Combine(path, Path.GetFileName(path)) + ".mat");
        }
    }

    public static void SaveRTToFile(RenderTexture rt, string path)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
    }

    /// <summary>
    /// Set standard material to HDRP/Lit shader and map textures to correct slot.
    /// This will be useful when you create model materials from model importer.
    /// </summary>
    [MenuItem("Assets/MMDExtensions/Materials/FromStandardPBR/UpgradeMateirals")]
    public static void UpgradeMateirals()
    {
        var mats = Selection.GetFiltered<Material>(SelectionMode.Assets);
        var lit = Shader.Find("HDRP/Lit");
        foreach (var mat in mats)
        {
            var baseColor = mat.GetColor("_Color");
            var baseColorMap = mat.GetTexture("_MainTex");
            var normal = mat.GetTexture("_BumpMap");

            mat.shader = lit;
            if (baseColorMap != null)
                mat.mainTexture = baseColorMap;
            mat.color = baseColor;
            if (normal != null)
                mat.SetTexture("_NormalMap", normal);

            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();
    }
}
