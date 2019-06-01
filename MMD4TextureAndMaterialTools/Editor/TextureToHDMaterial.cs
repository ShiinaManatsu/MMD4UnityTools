using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MMD4MaterialTools
{
    public class TextureToHDMaterial : Editor
    {
        [MenuItem("MMDHelper/TextureToHDMaterial")]
        [MenuItem("Assets/MMDHelper/Create Lit Material")]
        public static void CreateAssetBunldes()
        {
            foreach (var file in Selection.objects)
            {
                if (file.GetType() == typeof(Texture2D))
                {
                    Material hdrpMaterial;
#if UNITY_2018                    
                    hdrpMaterial = new Material(Shader.Find("HDRenderPipeline/Lit"));
#endif

#if UNITY_2019
                    hdrpMaterial = new Material(Shader.Find("HDRP/Lit"));
#endif
                    hdrpMaterial.SetTexture("_BaseColorMap", file as Texture2D);

                    Helpers.SaveMaterial(hdrpMaterial,file);
                }
            }
        }
    } 

}
