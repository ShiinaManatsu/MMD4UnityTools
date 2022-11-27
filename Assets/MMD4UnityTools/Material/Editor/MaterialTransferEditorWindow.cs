using MMD4UnityTools.Editor.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEditor;
using UnityEngine;


namespace MMD4UnityTools.Editor
{
    public class MaterialTransferEditorWindow : EditorWindow
    {
        #region OW

        [MenuItem("MMDExtensions/Material/Material Transfer")]
        public static void OpenWindow()
        {
            GetWindow<MaterialTransferEditorWindow>("Material Transfer").Show();
        }

        #endregion OW

        private GameObject copyFrom;
        private GameObject copyTo;
        private TransferMethod transferMethod;

        private enum TransferMethod
        {
            Index,
            VertexCount,
            ClosestVertexCount,
            //ApproximateLocalPosition,
        }

        private void OnGUI()
        {
            GUILayout.Label("Works only for single material renderer", EditorStyles.helpBox);
            copyFrom = copyFrom.ObjectField("copyFrom", true);
            copyTo = copyTo.ObjectField("copyTo", true);
            transferMethod = (TransferMethod)EditorGUILayout.EnumPopup(transferMethod);
            if (copyFrom && copyTo && GUILayout.Button("Transfer"))
            {
                var renderersToCopy = copyTo.GetComponentsInChildren<Renderer>();
                var renderersCopyFrom = copyFrom.GetComponentsInChildren<Renderer>();
                if (renderersToCopy.Length != renderersCopyFrom.Length && (!EditorUtility.DisplayDialog("Notice...", "Render count doesn't match, still process?", "Proceed!")))
                {
                    return;
                }

                switch (transferMethod)
                {
                    case TransferMethod.Index:
                        TransferIndex(renderersCopyFrom, renderersToCopy);
                        break;
                    case TransferMethod.VertexCount:
                        TransferVertexCount(renderersCopyFrom, renderersToCopy);
                        break;
                    case TransferMethod.ClosestVertexCount:
                        TransferClosestVertexCount(renderersCopyFrom, renderersToCopy);
                        break;
                    default:
                        break;
                }
            }
        }


        private void TransferIndex(Renderer[] from, Renderer[] to)
        {
            try
            {
                Undo.RecordObjects(to, "Copy Materials");
                for (int i = 0; i < to.Length; i++)
                {
                    to[i].sharedMaterials = from[i].sharedMaterials;
                }
            }
            finally
            {

            }
        }

        private void TransferVertexCount(Renderer[] from, Renderer[] to)
        {
            foreach (var rendererTo in to)
            {
                var meshTo = GetMeshFilter(rendererTo);
                var match = from.Select(x => new { Renderer = x, Mesh = GetMeshFilter(x) })
                    .Where(x => x.Mesh.vertexCount == meshTo.vertexCount)
                    .FirstOrDefault();
                if (match != null)
                {
                    rendererTo.sharedMaterials = match.Renderer.sharedMaterials;
                }
                else
                {
                    Debug.LogError($"No mesh has same vertex count with {rendererTo}", rendererTo);
                }
            }
        }

        private Mesh GetMeshFilter(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer)
            {
                return (renderer as SkinnedMeshRenderer).sharedMesh;
            }
            else
            {
                return renderer.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        private void TransferClosestVertexCount(Renderer[] from, Renderer[] to)
        {
            foreach (var rendererTo in to)
            {
                var meshTo = GetMeshFilter(rendererTo);
                var match = from.Select(x => new { Renderer = x, Mesh = GetMeshFilter(x) })
                    .OrderBy(x => Mathf.Abs(x.Mesh.vertexCount - meshTo.vertexCount))
                    .FirstOrDefault();
                if (match != null)
                {
                    rendererTo.sharedMaterials = match.Renderer.sharedMaterials;
                }
                else
                {
                    Debug.LogError($"No mesh has same vertex count with {rendererTo}", rendererTo);
                }
            }
        }

        //private void TransferApproximateLocalPosition(Renderer[] from, Renderer[] to)
        //{
        //    //  Stop doing this on cpu, or build accelerate structure
        //    const float threshold = 0.01f;
        //    const int quality = 10;
        //    foreach (var rendererTo in to)
        //    {
        //        var meshTo = rendererTo.GetComponent<MeshFilter>().sharedMesh;
        //        var match = from.Select(x => new { Renderer = x, Mesh = x.GetComponent<MeshFilter>().sharedMesh })
        //            .OrderBy(x =>
        //            {
        //                Enumerable.Range(0, x.Mesh.vertexCount / Mathf.Min(quality, x.Mesh.vertexCount))
        //                    .Select(o => x.Mesh.vertices[o])
        //                    .Select(o => o)
        //            })
        //            .FirstOrDefault();
        //        if (match != null)
        //        {
        //            rendererTo.sharedMaterials = match.Renderer.sharedMaterials;
        //        }
        //        else
        //        {
        //            Debug.LogError($"No mesh has same vertex count with {rendererTo}", rendererTo);
        //        }
        //    }
        //}
    }
}