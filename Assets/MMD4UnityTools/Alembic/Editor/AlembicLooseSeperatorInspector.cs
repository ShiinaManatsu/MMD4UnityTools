using Autodesk.Fbx;
using ComputeShaderReferences;
using MMD4UnityTools.Editor.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace MMD4UnityTools.Editor.Inspector
{
    [CustomEditor(typeof(AlembicLooseSeperator))]
    public class AlembicLooseSeperatorInspector : UnityEditor.Editor
    {
        private AlembicLooseSeperator Seperator => target as AlembicLooseSeperator;

        #region Helper Methods
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        [Flags]
        public enum AssocF
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200,
            Init_IgnoreUnknown = 0x400,
            Init_Fixed_ProgId = 0x800,
            Is_Protocol = 0x1000,
            Init_For_File = 0x2000
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            Supported_Uri_Protocols,
            ProgID,
            AppID,
            AppPublisher,
            AppIconReference,
            Max
        }

        static string AssocQueryString(AssocStr association, string extension)
        {
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = AssocQueryString(AssocF.None, association, extension, null, null, ref length);
            if (ret != S_FALSE)
            {
                throw new InvalidOperationException("Could not determine associated string");
            }

            var sb = new StringBuilder((int)length); // (length-1) will probably work too as the marshaller adds null termination
            ret = AssocQueryString(AssocF.None, association, extension, null, sb, ref length);
            if (ret != S_OK)
            {
                throw new InvalidOperationException("Could not determine associated string");
            }

            return sb.ToString();
        }
        #endregion

        const string S_Unity_Alembic_Path = "Unity_Alembic_Path";

        private string presetName = "";
        private string groupedMaterialPresetName = "";

        public ComputeShader MatchAlembicSubmeshWithFbxSubmeshCS;

        private void OnEnable()
        {
            Seperator.PrepareComponent();
            MatchAlembicSubmeshWithFbxSubmesh.Setup(MatchAlembicSubmeshWithFbxSubmeshCS);
        }

        private void GroupSubmesh(Mesh fbxMesh)
        {
            var fbxAsset = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(fbxMesh)) as ModelImporter;
            var isReadable = fbxAsset.isReadable;
            if (!isReadable)
            {
                fbxAsset.isReadable = true;
                fbxAsset.SaveAndReimport();
            }
            var alembicMesh = Seperator.meshFilter.sharedMesh;
            fbxMesh.RecalculateBounds();
            var scale = fbxMesh.bounds.extents.x / alembicMesh.bounds.extents.x;    //  alembic * scale == fbx
            var fbxVertexBuffer = new ComputeBuffer(fbxMesh.vertexCount, Marshal.SizeOf<Vector3>());
            var fbxIndexBuffer = new ComputeBuffer(fbxMesh.triangles.Length, Marshal.SizeOf<int>());
            var alembicVertexBuffer = new ComputeBuffer(alembicMesh.vertexCount, Marshal.SizeOf<Vector3>());
            var alembicIndexBuffer = new ComputeBuffer(alembicMesh.triangles.Length, Marshal.SizeOf<int>());
            var rwMatchCount = new ComputeBuffer(1, Marshal.SizeOf<int>());

            try
            {
                var defaultCount = new[] { 0 };
                var getCount = new[] { 0 };

                fbxVertexBuffer.SetData(fbxMesh.vertices);
                fbxIndexBuffer.SetData(fbxMesh.triangles);
                alembicVertexBuffer.SetData(alembicMesh.vertices);
                alembicIndexBuffer.SetData(alembicMesh.triangles);
                rwMatchCount.SetData(defaultCount);

                MatchAlembicSubmeshWithFbxSubmeshCS.SetFloat(MatchAlembicSubmeshWithFbxSubmesh.Scale, scale);

                MatchAlembicSubmeshWithFbxSubmeshCS.SetBuffer(MatchAlembicSubmeshWithFbxSubmesh.SubmeshMatch, MatchAlembicSubmeshWithFbxSubmesh.FbxVertexBuffer, fbxVertexBuffer);
                MatchAlembicSubmeshWithFbxSubmeshCS.SetBuffer(MatchAlembicSubmeshWithFbxSubmesh.SubmeshMatch, MatchAlembicSubmeshWithFbxSubmesh.FbxIndexBuffer, fbxIndexBuffer);
                MatchAlembicSubmeshWithFbxSubmeshCS.SetBuffer(MatchAlembicSubmeshWithFbxSubmesh.SubmeshMatch, MatchAlembicSubmeshWithFbxSubmesh.AlembicVertexBuffer, alembicVertexBuffer);
                MatchAlembicSubmeshWithFbxSubmeshCS.SetBuffer(MatchAlembicSubmeshWithFbxSubmesh.SubmeshMatch, MatchAlembicSubmeshWithFbxSubmesh.AlembicIndexBuffer, alembicIndexBuffer);
                MatchAlembicSubmeshWithFbxSubmeshCS.SetBuffer(MatchAlembicSubmeshWithFbxSubmesh.SubmeshMatch, MatchAlembicSubmeshWithFbxSubmesh.RWMatchCount, rwMatchCount);

                var results = Enumerable.Range(0, fbxMesh.subMeshCount).Select(x => new List<int>()).ToList();  //  Index represent to submesh index
                var loopCount = (float)(alembicMesh.subMeshCount * fbxMesh.subMeshCount);

                for (int i = 0; i < alembicMesh.subMeshCount; i++)
                {
                    var alembicDesc = alembicMesh.GetSubMesh(i);
                    var alembicStart = alembicDesc.indexStart;
                    var alembicCount = alembicDesc.indexCount;

                    MatchAlembicSubmeshWithFbxSubmeshCS.SetInt(MatchAlembicSubmeshWithFbxSubmesh.AlembicIndexStart, alembicStart);
                    MatchAlembicSubmeshWithFbxSubmeshCS.SetInt(MatchAlembicSubmeshWithFbxSubmesh.AlembicIndexCount, alembicCount);

                    var match = 0;
                    var index = 0;
                    for (int j = 0; j < fbxMesh.subMeshCount; j++)
                    {
                        var progress = i * fbxMesh.subMeshCount + j;
                        EditorUtility.DisplayProgressBar($"Compare submesh index {i} with fbx {j} submesh...", $"{progress} of {loopCount}", i / loopCount);
                        var fbxDesc = fbxMesh.GetSubMesh(j);
                        var fbxStart = fbxDesc.indexStart;
                        var fbxCount = fbxDesc.indexCount;
                        rwMatchCount.SetData(defaultCount);

                        MatchAlembicSubmeshWithFbxSubmeshCS.SetInt(MatchAlembicSubmeshWithFbxSubmesh.FbxIndexStart, fbxStart);
                        MatchAlembicSubmeshWithFbxSubmeshCS.SetInt(MatchAlembicSubmeshWithFbxSubmesh.FbxIndexCount, fbxCount);

                        MatchAlembicSubmeshWithFbxSubmeshCS.SetVector(MatchAlembicSubmeshWithFbxSubmesh.FbxBoundsCenter, fbxDesc.bounds.center);
                        MatchAlembicSubmeshWithFbxSubmeshCS.SetVector(MatchAlembicSubmeshWithFbxSubmesh.FbxBoundsExtents, fbxDesc.bounds.extents);

                        MatchAlembicSubmeshWithFbxSubmeshCS.Dispatch(MatchAlembicSubmeshWithFbxSubmesh.SubmeshMatch, alembicCount, 1, 1);
                        rwMatchCount.GetData(getCount);
                        if (getCount[0] > match)
                        {
                            match = getCount[0];
                            index = j;
                        }
                    }

                    results[index].Add(i);
                }

                Seperator.submeshMaterials = results.Select(x => new SubmeshMaterial() { submeshIndex = x }).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                fbxIndexBuffer.Dispose();
                fbxVertexBuffer.Dispose();
                alembicIndexBuffer.Dispose();
                alembicVertexBuffer.Dispose();
                rwMatchCount.Dispose();
                EditorUtility.ClearProgressBar();
            }
            if (!isReadable)
            {
                fbxAsset.isReadable = false;
                fbxAsset.SaveAndReimport();
            }
        }

        private void GetSubmeshInfo()
        {
            var abcPath = Seperator.player.PathToAbc;
            if (string.IsNullOrEmpty(abcPath))
            {
                Debug.Log($"Seperator.player.PathToAbc returns empty");
            }

            var info = new ProcessStartInfo();
            info.FileName = AssocQueryString(AssocStr.Executable, ".blend");
            info.Arguments = $"-b -P {Path.GetFullPath("Assets/MMD4UnityTools/Alembic/Editor/SubmeshTriangleCount.py")}";
            info.EnvironmentVariables[S_Unity_Alembic_Path] = Path.GetFullPath(abcPath).Replace('/', '\\');
            info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            var p = Process.Start(info);
            p.WaitForExit();
            var textPath = abcPath[0..^3] + "txt";
            var txt = File.ReadAllText(textPath);
            Seperator.submeshTriangles = txt.Split('\n').Select(x => int.Parse(x)).Where(x => x != 0).ToList();
            File.Delete(textPath);
            Resources.UnloadUnusedAssets();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Refresh submesh info"))
            {
                GetSubmeshInfo();
            }

            if (Seperator.submeshTriangles.Count != 0 && GUILayout.Button("Apply submesh"))
            {
                var startFrom = 0;
                var subMeshs = Seperator.submeshTriangles.Select(x =>
                {
                    var s = new SubMeshDescriptor(startFrom, x * 3);
                    startFrom += x * 3;
                    return s;
                }).ToArray();
                Seperator.meshFilter.sharedMesh.SetSubMeshes(subMeshs);

                var mats = Seperator.renderer.sharedMaterials;
                var submeshCount = Seperator.meshFilter.sharedMesh.subMeshCount;
                if (mats.Length < submeshCount)
                {
                    Seperator.renderer.sharedMaterials = mats.Concat(Enumerable.Range(0, submeshCount - mats.Length)
                        .Select(_ => default(Material)))
                        .ToArray();
                }
            }

            GUILayout.Label("Material should update manually to prevent GC.", EditorStyles.helpBox);
            if (Seperator.submeshTriangles.Count != 0 && GUILayout.Button("Match Material Count"))
            {
                if (Seperator.renderer.sharedMaterials.Length < Seperator.meshFilter.sharedMesh.subMeshCount)
                {
                    var mat = Seperator.renderer.sharedMaterials.FirstOrDefault();
                    Seperator.renderer.sharedMaterials = Seperator.renderer.sharedMaterials.Concat(Enumerable.Range(0, Seperator.meshFilter.sharedMesh.subMeshCount - Seperator.renderer.sharedMaterials.Length)
                        .Select(_ => mat))
                        .ToArray();
                }
            }

            using (new BoxGroup("Materials", Orientation.Vertical))
            {
                if (GUILayout.Button("Read from renderer"))
                {
                    Seperator.ReadRendererMateirals();
                }

                if (GUILayout.Button("Set to renderer"))
                {
                    Seperator.SetRendererMateirals();
                }
            }

            using (new BoxGroup("Presets", Orientation.Horizontal))
            {
                presetName = EditorGUILayout.TextField(presetName);

                if (GUILayout.Button("Save current to presets", GUILayout.Width(144.0f)))
                {
                    Seperator.presets.Add(new MaterialsWrapper() { materials = Seperator.materials.ToList(), name = presetName });
                }
            }

            if (Seperator.meshForSubmeshCompare)
            {
                using (new BoxGroup("Submesh Grouped Materials", Orientation.Vertical))
                {
                    var fbxMesh = Seperator.meshForSubmeshCompare.sharedMesh;
                    if (fbxMesh && GUILayout.Button("Group submesh"))
                    {
                        GroupSubmesh(fbxMesh);
                    }

                    if (Seperator.submeshMaterials.Count != 0 && GUILayout.Button("Apply grouped materials"))
                    {
                        Seperator.SetRendererMateiralsSubmeshGrouped();
                    }
                }
            }

            using (new BoxGroup("Submesh Grouped Material Presets", Orientation.Horizontal))
            {
                groupedMaterialPresetName = EditorGUILayout.TextField(groupedMaterialPresetName);

                if (GUILayout.Button("Save current to presets", GUILayout.Width(144.0f)))
                {
                    Seperator.submeshGroupedMaterialPresets.Add(new SubmeshMaterialWrapper() { name = groupedMaterialPresetName, submeshMaterials = Seperator.submeshMaterials.ToList() });
                }
            }
        }
    }
}