using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace MMD4UnityTools
{
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
        private List<SubMeshDescriptor> subMeshDescriptors = new();

        public List<Material> materials = new();

        public List<MaterialsWrapper> presets = new();

        public List<SubmeshMaterial> submeshMaterials = new();

        public List<SubmeshMaterialWrapper> submeshGroupedMaterialPresets;

        public MeshFilter meshForSubmeshCompare;

        public void SetRendererMateiralsSubmeshGrouped()
        {
            var grouped = new Material[submeshTriangles.Count];
            foreach (var submeshMaterial in submeshMaterials)
            {
                foreach (var index in submeshMaterial.submeshIndex)
                {
                    grouped[index] = submeshMaterial.material;
                }
            }
            materials = grouped.ToList();
            SetRendererMateirals();
        }

        public void ReadRendererMateirals()
        {
            materials = renderer.sharedMaterials.ToList();
        }

        public void SetRendererMateirals()
        {
            renderer.sharedMaterials = materials.ToArray();
        }

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

    /// <summary>
    /// A collection holds all submeshs shared with same material
    /// </summary>
    [Serializable]
    public class SubmeshMaterial
    {
        public List<int> submeshIndex = new();
        public Material material;
    }

    [Serializable]
    public class SubmeshMaterialWrapper
    {
        public string name;
        public List<SubmeshMaterial> submeshMaterials = new();
    }

    [Serializable]
    public class MaterialsWrapper
    {
        public string name;
        public List<Material> materials = new();
    }
}