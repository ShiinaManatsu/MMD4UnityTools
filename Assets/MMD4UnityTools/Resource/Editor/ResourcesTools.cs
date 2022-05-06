using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

public static class ResourcesTools
{
    enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }

    [DllImport("kernel32.dll")]
    static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);


    [MenuItem("Assets/MMDExtensions/Resources/Import folder as junction")]
    public static void ImportFolderJunction()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var import = EditorUtility.OpenFolderPanel("Selection a folder to import", "", "");

        if (string.IsNullOrEmpty(import)) return;
        var dest = Path.Combine(path, Path.GetFileName(import));
        //var cmd = $"MKLINK /j \"{Path.GetFullPath(dest)}\" \"{import.Replace("//", "/").Replace('/', '\\')}\"";
        dest = Path.GetFullPath(dest).Replace("\\\\", "\\").Replace("\\", "/");
        var target = Path.GetFullPath(import).Replace("\\\\", "\\").Replace("\\", "/");

        CreateMaps.JunctionPoint.Create(dest, target, true);

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/MMDExtensions/Resources/Import folder as junction", validate = true)]
    public static bool ImportFolderJunctionValidate()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return Directory.Exists(path);

    }

    [InitializeOnLoad]
    public static class JunctionStatus
    {
        static JunctionStatus()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        // FileAttributes that match a junction folder.
        const FileAttributes FOLDER_SYMLINK_ATTRIBS = FileAttributes.Directory | FileAttributes.ReparsePoint;

        private static GUIStyle markerStyle = null;
        private static GUIStyle MarkerStyle
        {
            get
            {
                if (markerStyle == null || true)
                {
                    markerStyle = new GUIStyle(EditorStyles.label);
                    markerStyle.normal.textColor = new Color(1f, 193 / 255f, 7 / 255f, 1f);
                    markerStyle.alignment = TextAnchor.MiddleRight;
                    var f = AssetDatabase.FindAssets("Font Awesome 5 Free-Solid-900 t:font").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Font>).First();
                    if (f != null)
                    {
                        markerStyle.font = f;
                    }
                }
                return markerStyle;
            }
        }

        private static Texture2D icon = null;
        private static Texture2D Icon
        {
            get
            {
                if (icon == null)
                {
                    icon = EditorGUIUtility.IconContent("d_Linked").image as Texture2D;
                }
                return icon;
            }
        }

        private static void OnProjectWindowItemGUI(string guid, Rect r)
        {
            try
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(path))
                {
                    FileAttributes attribs = File.GetAttributes(path);

                    if ((attribs & FOLDER_SYMLINK_ATTRIBS) == FOLDER_SYMLINK_ATTRIBS)
                    //GUI.Label(r, "Junction", MarkerStyle);
                    {
                        var rect = new Rect(r.x + r.width - r.height, r.y, r.height, r.height);
                        GUI.DrawTexture(rect, Icon, ScaleMode.ScaleToFit, true, 1f, MarkerStyle.normal.textColor, 0, 0);
                    }
                }
            }
            catch { }
        }

        [MenuItem("ZTools/LogLan")]
        public static void LogLan()
        {
            Debug.Log(System.Globalization.CultureInfo.InstalledUICulture.Name);
        }
    }
}