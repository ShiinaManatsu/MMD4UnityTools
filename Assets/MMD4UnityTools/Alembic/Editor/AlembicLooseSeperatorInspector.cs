using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(AlembicLooseSeperator))]
public class AlembicLooseSeperatorInspector : Editor
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

    private void OnEnable()
    {
        Seperator.PrepareComponent();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh submesh info"))
        {
            var abcPath = Seperator.player.PathToAbc;
            if (string.IsNullOrEmpty(abcPath))
            {
                Debug.Log($"Seperator.player.PathToAbc returns empty");
            }
            //abcPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(alembic));
            var info = new ProcessStartInfo();
            info.FileName = AssocQueryString(AssocStr.Executable, ".blend");
            info.Arguments = $"-b -P {Path.GetFullPath("Assets/MMD4UnityTools/Alembic/Editor/SubmeshTriangleCount.py")}";
            info.EnvironmentVariables[S_Unity_Alembic_Path] = Path.GetFullPath(abcPath).Replace('/', '\\');
            info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            //var p = Process.Start(AssocQueryString(AssocStr.Executable, ".blend"), $"-P {Path.GetFullPath("Assets/SubmeshTest/Editor/SubmeshTriangleCount.py")}");
            var p = Process.Start(info);
            p.WaitForExit();
            var textPath = abcPath[0..^3] + "txt";
            var txt = File.ReadAllText(textPath);
            Seperator.submeshTriangles = txt.Split('\n').Select(x => int.Parse(x)).ToList();
            File.Delete(textPath);
            Resources.UnloadUnusedAssets();
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
                Seperator.renderer.sharedMaterials = Seperator.renderer.sharedMaterials.Concat(Enumerable.Range(0, Seperator.meshFilter.sharedMesh.subMeshCount - Seperator.renderer.sharedMaterials.Length)
                    .Select(_ => default(Material)))
                    .ToArray();
            }
        }
    }
}
