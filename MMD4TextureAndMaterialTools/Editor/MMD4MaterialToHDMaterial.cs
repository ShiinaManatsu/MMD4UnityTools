using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MMD4MaterialTools
{
    public class MMD4MaterialToHDMaterial : Editor
    {
        [MenuItem("MMDHelper/MMD4MaterialToHDMaterial")]
        [MenuItem("Assets/MMDHelper/Update To Lit Material")]
        public static void CreateAssetBunldes()
        {
            foreach (var file in Selection.objects)
            {
                Material hdrpMaterial = new Material(Shader.Find("HDRP/Lit"));
                if (file.GetType() == typeof(Material))
                {
                    var mat = file as Material;
                    hdrpMaterial.SetTexture("_BaseColorMap", mat.GetTexture("_MainTex"));
                    AssetDatabase.CreateAsset(hdrpMaterial, AssetDatabase.GetAssetPath(file.GetInstanceID()));
                }
            }
        }

    }

}