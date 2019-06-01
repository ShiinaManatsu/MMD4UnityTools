using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace MMD4MaterialTools
{
    public class AutoAssign : Editor
    {
        private static Dictionary<string, Material> MaterialsList { get; set; }

        private new static GameObject gameObject;

        private static List<Texture2D> SelectedTexture { get; set; }

        private static Dictionary<string, GameObject> GameObjectsHasMeshRendererDictionary { get; set; }

        private static List<GameObject> GameObjectsHasMeshRendererDictionaryList { get; set; }


        private static string Json { get; set; }

        /// <summary>
        /// Create and Sign material via selected json file, place the json file with textures
        /// </summary>
        public static void Sign()
        {
            MaterialsList = new Dictionary<string, Material>();
            SelectedTexture = new List<Texture2D>();
            GameObjectsHasMeshRendererDictionary = new Dictionary<string, GameObject>();
            GameObjectsHasMeshRendererDictionaryList = new List<GameObject>();

            // TODO:
            // Get the json √
            // Create material according to the selected texture √
            // Sign the seleted prefab, find the material, the sign them with new sheet   ⁘⁏⁙⁚⁖⁝⁂√⨉

            // Return with the incorrect selections
            if (Selection.gameObjects.Length != 1)
            {
                Debug.LogError("Can only select one item");
                return;
            }

            #region Perpare the things we need that used to initials materials
            foreach (var file in Selection.gameObjects)
            {
                if (file.GetType() == typeof(GameObject))
                {
                    gameObject = file as GameObject;
                }
            }
            foreach (var f in Selection.objects)
            {
                if (f.GetType() == typeof(Texture2D))
                {
                    SelectedTexture.Add(f as Texture2D);
                }
                if (f.GetType() == typeof(TextAsset))
                {
                    Json = (f as TextAsset).text;
                }
            }
            #endregion

            #region Fill the MaterialList and save them to assets

            foreach (var i in SelectedTexture)
            {
                Material hdrpMaterial = Helpers.CreateMaterial();

                hdrpMaterial.SetTexture("_BaseColorMap", i);

                MaterialsList.Add(i.name, hdrpMaterial);

                Helpers.SaveMaterial(hdrpMaterial, i);
            }

            #endregion

            #region Find mesh with MeshRenderer

            //foreach (Transform child in gameObject.transform)
            //{
            //    if (child.childCount != 0)
            //    {
            //        foreach (Transform inChild in child.transform)
            //        {
            //            if (inChild.childCount != 0)
            //            {
            //                foreach (Transform transform in inChild)
            //                {
            //                    if (transform.GetComponent<MeshRenderer>())
            //                    {
            //                        GameObjectsHasMeshRendererDictionary.Add(transform.gameObject.name, transform.gameObject);
            //                        GameObjectsHasMeshRendererDictionaryList.Add(transform.gameObject);
            //                    }
            //                }
            //            }

            //            if (inChild.GetComponent<MeshRenderer>())
            //            {
            //                GameObjectsHasMeshRendererDictionary.Add(inChild.gameObject.name, inChild.gameObject);
            //                GameObjectsHasMeshRendererDictionaryList.Add(inChild.gameObject);
            //            }
            //        }
            //    }

            //    if (child.GetComponent<MeshRenderer>())
            //    {
            //        GameObjectsHasMeshRendererDictionary.Add(child.gameObject.name, child.gameObject);
            //        GameObjectsHasMeshRendererDictionaryList.Add(child.gameObject);
            //    }
            LoopChild(gameObject.transform);
            #endregion


        }

        private static void LoopChild(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.childCount != 0)
                {
                    LoopChild(child);
                }
                if (child.GetComponent<MeshRenderer>())
                {
                    GameObjectsHasMeshRendererDictionary.Add(child.gameObject.name, child.gameObject);
                    GameObjectsHasMeshRendererDictionaryList.Add(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Sign material for 
        /// </summary>
        [MenuItem("MMDHelper/AutoAssign")]
        private static void SignMaterial()
        {
            Sign();
            for (int i = 0; i < GameObjectsHasMeshRendererDictionaryList.Count; i++)
            {
                var list = ConvertToMMDMaterial();
                var name = (list[i].Texture.Split('\\'))[1];
                name = name.Remove(name.Length - 4);
                try
                {
                    GameObjectsHasMeshRendererDictionaryList[i].GetComponent<MeshRenderer>().material = MaterialsList[name];
                }
                catch (KeyNotFoundException e)
                {
                    Debug.Log($"{name} not found, check if pmx material has null texture");
                }
            }
        }

        public static List<MMDMaterial> ConvertToMMDMaterial()
        {
            return JsonConvert.DeserializeObject<List<MMDMaterial>>(Json);
        }
    }

    /// <summary>
    /// Definition of Material
    /// </summary>
    public class MMDMaterial
    {
        // Example:"Name":"eye","Texture":"tex\\2.png"

        /// <summary>
        /// Material name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Material texture name
        /// </summary>
        public string Texture { get; set; }
    }
}