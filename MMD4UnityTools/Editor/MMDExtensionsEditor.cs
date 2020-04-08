/*
                                        DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
                                                Version 2, December 2004 

                                Copyright (C) 2004 Sam Hocevar <sam@hocevar.net> 

                                Everyone is permitted to copy and distribute verbatim or modified 
                                copies of this license document, and changing it is allowed as long 
                                as the name is changed. 

                                        DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
                                TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION 

                                0. You just DO WHAT THE FUCK YOU WANT TO.
 */

 /*
  Current Version: 1.1.0.1 +2
  */

using MMDExtensions.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Notes: To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
/// TODO: Add validate method
/// TODO: Fill material render queue
/// </summary>

namespace MMDExtensions
{
    public class MMDExtensionsEditor : Editor
    {
        /// <summary>
        /// Upgrade MMD4 materials to HDRP Lit Material 
        /// </summary>
        [MenuItem("MMDExtensions/Upgrade MMD4 Material (HDRP)")]
        [MenuItem("Assets/MMDExtensions/Upgrade MMD4 Material (HDRP)")]
        public static void UpgradeMaterial()
        {
            GetPmx(out string pmxPath, out PMX.PMXFormat pmx_format);

            var materials = from mat in Selection.objects
                            where mat is Material
                            select mat as Material;

            foreach (var mat in materials)
            {
                var index = int.Parse(mat.name.Split('.').First());
                try
                {
                    var texture = pmx_format.texture_list.texture_file[pmx_format.material_list.material[index].usually_texture_index];
                    texture = Path.Combine(pmxPath.Remove(pmxPath.LastIndexOf('/')), texture);
                    mat.SetTexture("_BaseColorMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texture));
                }
                catch
                {
                    mat.SetTexture("_BaseColorMap", Texture2D.redTexture);
                }
            }
            System.GC.Collect();
        }

        /// <summary>
        /// Upgrade materials that extracted from blender mmdtools fbx model
        /// </summary>
        [MenuItem("MMDExtensions/Upgrade Blender Materials")]
        [MenuItem("Assets/MMDExtensions/Upgrade Blender Materials")]
        public static void UpgradeMaterialBlender()
        {
            GetPmx(out string pmxPath, out PMX.PMXFormat pmx_format);

            var materials = from mat in Selection.objects
                            where mat is Material
                            select mat as Material;

            foreach (var mat in materials)
            {
                var name = mat.name;
                var index = pmx_format.material_list.material.Where((x) => x.name.Contains(name)).First().usually_texture_index;
                try
                {
                    var texture = pmx_format.texture_list.texture_file[index];
                    texture = !texture.Split('.').Last().ToLower().Contains("dds") ? texture : texture.Remove(texture.LastIndexOf(".")) + ".png";
                    texture = Path.Combine(pmxPath.Remove(pmxPath.LastIndexOf('/')), texture);
                    mat.SetTexture("_BaseColorMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texture));
                }
                catch
                {
                    mat.SetTexture("_BaseColorMap", Texture2D.redTexture);
                }
            }
            System.GC.Collect();
        }

        /// <summary>
        /// Upgrade materials from abc game object that exported from mmd bridge
        /// </summary>
        [MenuItem("MMDExtensions/Upgrade ABC Model Material")]
        [MenuItem("Assets/MMDExtensions/Upgrade ABC Model Material")]
        public static void UpgradeABCMaterial()
        {
            var objs = Selection.objects;
            var pmx = from x in objs
                      where x is DefaultAsset && AssetDatabase.GetAssetPath(x).ToUpper().Contains("PMX")
                      select x;
            var gameObjs = Selection.gameObjects.First();

            var savePath = EditorUtility.SaveFolderPanel("Choose where to save materials", AssetDatabase.GetAssetPath(pmx.First().GetInstanceID()), "");

            if (savePath == "")
            {
                Debug.Log("User canceled");
                return;
            }
            else
            {
                savePath = savePath.Remove(0, savePath.IndexOf("Assets"));
                Debug.Log(savePath);
            }

            var pmx_format = LoadPmxMaterials(pmx.First());
            var pmxPath = AssetDatabase.GetAssetPath(pmx.First());

            // get the model mesh renderer

            var meshRenderers = gameObjs.GetComponentsInChildren<MeshRenderer>();

            // sign it

            var textures = from x in pmx_format.material_list.material
                           select new { Path = pmx_format.texture_list.texture_file[x.usually_texture_index], Name = x.name };

            var materials = new List<Material>();

            foreach (var texture in textures)
            {
                var mat = CreateMaterial();
                mat.name = texture.Name;
                try
                {
                    if (texture.Path.ToUpper().EndsWith(".PNG") || texture.Path.ToUpper().EndsWith(".JPG"))
                    {
                        var path = Path.Combine(pmxPath.Remove(pmxPath.LastIndexOf('/')), texture.Path);
                        mat.SetTexture("_BaseColorMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                    }
                    else
                    {
                        mat.SetTexture("_BaseColorMap", Texture2D.redTexture);
                    }
                }
                catch
                {
                    mat.SetTexture("_BaseColorMap", Texture2D.redTexture);
                }
                materials.Add(mat);
            }

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = materials[i];

                AssetDatabase.CreateAsset(materials[i], Path.Combine(savePath, materials[i].name + ".mat"));
            }
        }

        /// <summary>
        /// Create basic HDRP Lit Material from selected textures
        /// </summary>
        [MenuItem("MMDExtensions/Create Materials From Textures")]
        [MenuItem("Assets/MMDExtensions/Create Materials From Textures")]
        public static void CreateAssetBunldes()
        {
            foreach (var file in Selection.objects)
            {
                if (file.GetType() == typeof(Texture2D))
                {
                    Material hdrpMaterial = CreateMaterial();
                    hdrpMaterial.SetTexture("_BaseColorMap", file as Texture2D);

                    SaveMaterial(hdrpMaterial, file);
                }
            }
        }

        #region VMD Methods

        /// <summary>
        /// Create camera animation assets
        /// </summary>
        [MenuItem("Assets/VMD/Create Camera Animation")]
        public static void CreateCameraAnimation()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (Path.GetExtension(path).ToUpper().Contains("VMD"))
            {
                var stream = File.Open(path, FileMode.Open);

                var vmd = VMDParser.ParseVMD(stream);

                var orderedFrames = from frame in vmd.Cameras
                                    orderby frame.FrameIndex
                                    select frame;
                var animationClip = new AnimationClip()
                {
                    frameRate = 30,
                };

                var delta = 1 / animationClip.frameRate;
                var scale = 0.085f;//1.76f / 2f;

                var quaternions = from frame in orderedFrames
                                  select new
                                  {
                                      Time = frame.FrameIndex * delta,
                                      Quaternion = Quaternion.Euler(new Vector3(frame.XRotation * Mathf.Rad2Deg, frame.YRotation * Mathf.Rad2Deg, frame.ZRotation * Mathf.Rad2Deg)),
                                      OutTangent = Mathf.Lerp(-1, 1, frame.Curve.AY / 127),
                                  };

                var q = quaternions.First().Quaternion;

                var xPosition = from position in orderedFrames
                                select new Keyframe(position.FrameIndex * delta, position.XPosition * scale);
                var YPosition = from position in orderedFrames
                                select new Keyframe(position.FrameIndex * delta, position.YPosition * scale);
                var ZPosition = from position in orderedFrames
                                select new Keyframe(position.FrameIndex * delta, position.ZPosition * scale);
                var XRoation = from quaternion in quaternions
                               select new Keyframe(quaternion.Time, quaternion.Quaternion.x);
                var YRoation = from quaternion in quaternions
                               select new Keyframe(quaternion.Time, quaternion.Quaternion.y);
                var ZRoation = from quaternion in quaternions
                               select new Keyframe(quaternion.Time, quaternion.Quaternion.z);
                var WRoation = from quaternion in quaternions
                               select new Keyframe(quaternion.Time, quaternion.Quaternion.w);
                var fov = from frame in orderedFrames
                          select new Keyframe(frame.FrameIndex * delta, (float)frame.FOV);


                var xPostionCurve = new AnimationCurve(xPosition.ToArray());
                var yPostionCurve = new AnimationCurve(YPosition.ToArray());
                var zPostionCurve = new AnimationCurve(ZPosition.ToArray());
                var xRotationCurve = new AnimationCurve(XRoation.ToArray());
                var yRotationCurve = new AnimationCurve(YRoation.ToArray());
                var zRotationCurve = new AnimationCurve(ZRoation.ToArray());
                var wRotationCurve = new AnimationCurve(WRoation.ToArray());
                var fovCurve = new AnimationCurve(fov.ToArray());
                animationClip.SetCurve("", typeof(Transform), "localPosition.x", xPostionCurve);
                animationClip.SetCurve("", typeof(Transform), "localPosition.y", yPostionCurve);
                animationClip.SetCurve("", typeof(Transform), "localPosition.z", zPostionCurve);
                animationClip.SetCurve("", typeof(Transform), "localRotation.x", xRotationCurve);
                animationClip.SetCurve("", typeof(Transform), "localRotation.y", yRotationCurve);
                animationClip.SetCurve("", typeof(Transform), "localRotation.z", zRotationCurve);
                animationClip.SetCurve("", typeof(Transform), "localRotation.w", wRotationCurve);
                animationClip.SetCurve("", typeof(Camera), "field of view", fovCurve);

                AssetDatabase.CreateAsset(animationClip, path.Replace("vmd", "anim"));//"Assets/VMDCamera.anim");
            }
        }

        /// <summary>
        /// Create morph animation assets
        /// </summary>
        [MenuItem("Assets/VMD/Create Morph Animation")]
        public static void CreateMorphAnimation()
        {
            System.GC.Collect();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (Path.GetExtension(path).ToUpper().Contains("VMD"))
            {
                var stream = File.Open(path, FileMode.Open);

                var vmd = VMDParser.ParseVMD(stream);

                var animationClip = new AnimationClip() { frameRate = 30 };

                var delta = 1 / animationClip.frameRate;


                var keyframes = from keys in vmd.Morphs.ToLookup(k => k.MorphName, v => new Keyframe(v.FrameIndex * delta, v.Weight * 100))
                                select keys;

                foreach (var package in keyframes)
                {
                    var name = package.Key;

                    var curve = new AnimationCurve(package.ToArray());
                    var gameObjectName = Selection.gameObjects.First().name;
                    var parentName = Selection.gameObjects.First().transform.parent.name;

                    var mesh = Selection.gameObjects.First().GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    var bsCounts = mesh.blendShapeCount;
                    var blendShapeNames = Enumerable.Range(0, bsCounts).ToList().ConvertAll(index => mesh.GetBlendShapeName(index));
                    try
                    {
                        var registerName = blendShapeNames.Where(x => x.Split('.').Last() == name).First();
                        animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer), $"blendShape.{registerName}", curve);
                    }
                    catch
                    {
                        continue;
                    }
                }

                AssetDatabase.CreateAsset(animationClip, path.Replace("vmd", "anim"));
            }
        }

        #endregion

        #region Helper Methods

        public static PMX.PMXFormat LoadPmxMaterials(Object @object)
        {

            var pmxPath = AssetDatabase.GetAssetPath(@object);
            var dataPath = Application.dataPath;
            dataPath = dataPath.Remove(dataPath.Length - 6);

            var absolutePath = Path.Combine(dataPath, pmxPath);

            var model_agent = new ModelAgent(absolutePath);
            return PMXLoaderScript.Import(model_agent.file_path_);
        }

        /// <summary>
        /// Save material to assets
        /// </summary>
        /// <param name="hdrpMaterial">The material will be saved</param>
        /// <param name="file">Selected texture file</param>
        public static void SaveMaterial(Material hdrpMaterial, UnityEngine.Object file)
        {
            var path = AssetDatabase.GetAssetPath(file.GetInstanceID()).Split('/');
            var filename = path[path.Length - 1];
            filename = filename.Remove(filename.LastIndexOf('.')) + ".mat";
            path[path.Length - 2] = "Materials";

            var fixedPath = "";
            for (int i = 0; i < path.Length - 1; i++)
            {
                fixedPath += path[i] + "/";
            }
            if (!AssetDatabase.IsValidFolder(fixedPath.Remove(fixedPath.Length - 1)))
            {
                var p = fixedPath.Remove(fixedPath.Length - 11);
                AssetDatabase.CreateFolder(p, "Materials");
            }
            AssetDatabase.CreateAsset(hdrpMaterial, fixedPath + filename);
        }

        /// <summary>
        /// Save material to assets
        /// </summary>
        /// <param name="hdrpMaterial">The material will be saved</param>
        /// <param name="file">Selected texture file</param>
        /// <param name="newFilename">File name</param>
        public static void SaveMaterial(Material hdrpMaterial, UnityEngine.Object file, string newFilename)
        {
            var path = AssetDatabase.GetAssetPath(file.GetInstanceID()).Split('/');
            path[path.Length - 2] = "Materials";

            var fixedPath = string.Empty;
            for (int i = 0; i < path.Length - 1; i++)
            {
                fixedPath += path[i] + "/";
            }
            if (!AssetDatabase.IsValidFolder(fixedPath.Remove(fixedPath.Length - 1)))
            {
                // 11 means the length of "Materials/" add 1
                var p = fixedPath.Remove(fixedPath.Length - 11);
                AssetDatabase.CreateFolder(p, "Materials");
            }
            AssetDatabase.CreateAsset(hdrpMaterial, fixedPath + newFilename + ".mat");
        }

        /// <summary>
        /// Load the pmx
        /// </summary>
        /// <param name="pmxPath">The pmx path we can get</param>
        /// <param name="pmx_format">The parsed pmx object</param>
        public static void GetPmx(out string pmxPath,out PMX.PMXFormat pmx_format)
        {

            var pmx = from x in Selection.objects
                      where x is DefaultAsset && AssetDatabase.GetAssetPath(x).ToUpper().Contains("PMX")
                      select x;

            Object pmxObject = null;
            pmxPath = "";

            if (pmx.ToList().Count != 0)
            {
                pmxObject = pmx.First();
                pmxPath = AssetDatabase.GetAssetPath(pmx.First());
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(Selection.objects.First());
                path = path.Remove(path.LastIndexOf("/"));
                path = path.Remove(path.LastIndexOf("/"));

                var pmxQueue = from guid in AssetDatabase.FindAssets("", new[] { path })
                               select AssetDatabase.GUIDToAssetPath(guid) into p
                               group p by p.EndsWith(".pmx", true, System.Globalization.CultureInfo.CurrentCulture) into g
                               where g.Key
                               select g;
                pmxPath = pmxQueue.First().First();
                pmxObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(pmxPath);
            }

            pmx_format = LoadPmxMaterials(pmxObject);
        }

        /// <summary>
        /// Set material 
        /// </summary>
        /// <param name="material"></param>
        public static Material CreateMaterial()
        {
#if UNITY_2018
            return new Material(Shader.Find("HDRenderPipeline/Lit"));

#else
            return new Material(Shader.Find("HDRP/Lit"));
#endif

        }

        #endregion
    }
}