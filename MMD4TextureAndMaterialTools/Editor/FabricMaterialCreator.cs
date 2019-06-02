using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MMD4MaterialTools
{
    public class FabricMaterialCreator : Editor
    {
        private static Dictionary<string, SPTexture> TextureList { get; set; } = new Dictionary<string, SPTexture>();

        [MenuItem("MMDHelper/Create fabric material with sp")]
        [MenuItem("Assets/MMDHelper/Create fabric material")]
        public static void CreateAssetBunldes()
        {
            Shader shader = null;
            Object path = null;

            #region Fill the texture list and shader
            foreach (var file in Selection.objects)
            {
                if (file.GetType() == typeof(Texture2D))
                {
                    path = path ?? file;

                    var textureName = (file.name.Split('_'));
                    string texture = string.Empty;
                    for (int i = 0; i < textureName.Length - 1; i++)
                    {
                        texture += textureName[i];
                    }

                    if (!TextureList.ContainsKey(texture))
                    {
                        TextureList.Add(texture, new SPTexture());
                    }

                    switch (file.name)
                    {
                        case var name when name.Contains("BaseColor"):
                            TextureList[texture].BaseColor = file as Texture2D; break;
                        case var name when name.Contains("MaskMap"):
                            TextureList[texture].MaskMap = file as Texture2D; break;
                        case var name when name.Contains("Normal"):
                            TextureList[texture].NormalMap = file as Texture2D; break;

                        default:
                            break;
                    }
                }
                if (file.GetType() == typeof(Shader))
                {
                    shader = file as Shader;
                }
                Debug.Log(file.name + " Success");
            }
            #endregion
            foreach (var tex in TextureList)
            {
                Material fabric = new Material(shader);
                fabric.SetTexture("_BaseColorMap", tex.Value.BaseColor);
                fabric.SetTexture("_NormalMap", tex.Value.NormalMap);
                fabric.SetTexture("_MaskMap", tex.Value.MaskMap);

                Helpers.SaveMaterial(fabric, path, tex.Key);
            }
        }
    }

    /// <summary>
    /// Represent a substance painter exported hdrp texture set
    /// </summary>
    public class SPTexture
    {
        #region Private Members

        private Texture2D _baseColor = null;
        private Texture2D _maskMap = null;
        private Texture2D _normalMap = null;

        #endregion


        /// <summary>
        /// Base Color Map
        /// </summary>
        public Texture2D BaseColor { get => _baseColor ?? Texture2D.whiteTexture; set { _baseColor = value; } }

        /// <summary>
        /// Mask Map
        /// </summary>
        public Texture2D MaskMap { get => _maskMap ?? Texture2D.blackTexture; set { _maskMap = value; } }

        /// <summary>
        /// Normal Map
        /// </summary>
        public Texture2D NormalMap { get => _normalMap ?? Texture2D.normalTexture; set { _normalMap = value; } }

    }
}
