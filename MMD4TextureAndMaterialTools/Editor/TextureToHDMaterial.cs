using UnityEditor;
using UnityEngine;

namespace MMD4MaterialTools
{
    public class TextureToHDMaterials : Editor
    {
        [MenuItem("MMDHelper/TextureToHDMaterial")]
        [MenuItem("Assets/MMDHelper/Create Lit Material")]
        public static void CreateAssetBunldes()
        {
            foreach (var file in Selection.objects)
            {
                if (file.GetType() == typeof(Texture2D))
                {
                    Material hdrpMaterial = Helpers.CreateMaterial();
                    hdrpMaterial.SetTexture("_BaseColorMap", file as Texture2D);

                    Helpers.SaveMaterial(hdrpMaterial, file);
                }
            }
        }
    }

}
