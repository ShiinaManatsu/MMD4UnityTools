using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

/// <summary>
/// Loose parts to submeshes
/// </summary>
[RequireComponent(typeof(AlembicStreamPlayer)), ExecuteInEditMode, DefaultExecutionOrder(int.MaxValue)]
[AddComponentMenu("MMDExtensions/AlembicLooseSeperator")]
public class AlembicLooseSeperator : MonoBehaviour
{
    public AlembicStreamPlayer player;
    public new MeshRenderer renderer;
    public MeshFilter meshFilter;
    public List<int> submeshTriangles = new();
    public List<SubMeshDescriptor> subMeshDescriptors = new();

    public void PrepareComponent()
    {
        player = GetComponent<AlembicStreamPlayer>();
        renderer = GetComponentInChildren<MeshRenderer>();
        meshFilter = GetComponentInChildren<MeshFilter>();
    }

    private void LateUpdate()
    {
        Profiler.BeginSample("AlembicLooseSeperator");
        if (submeshTriangles.Count == 0)
        {
            return;
        }
        if (subMeshDescriptors.Count != submeshTriangles.Count)
        {
            var startFrom = 0;
            subMeshDescriptors = submeshTriangles.Select(x =>
            {
                var s = new SubMeshDescriptor(startFrom, x * 3);
                startFrom += x * 3;
                return s;
            }).ToList();
        }
        meshFilter.sharedMesh.SetSubMeshes(subMeshDescriptors, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontRecalculateBounds);

        Profiler.EndSample();
    }
}
