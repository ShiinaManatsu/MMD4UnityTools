using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureExtensions
{
    /// <summary>
    /// Convert a <see cref="RenderTexture"/> to <see cref="Texture2D"/>
    /// </summary>
    public static Texture2D ToTexture2D(this RenderTexture rt)
    {
        var temp = RenderTexture.active;
        RenderTexture.active = rt;
        var texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
        RenderTexture.active = temp;
        rt.Release();
        return texture;
    }
}
