using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MMD4UnityTools.Editor
{
    public static class ConstantInterpolationHelper
    {
        [MenuItem("Assets/MMDExtensions/Animation/Set Interpolation For Stopmotion")]
        public static void SetToConstantMenu()
        {
            var clip = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets).FirstOrDefault();
            if (clip)
            {
                SetToConstant(clip);
            }
        }

        public static void SetToConstant(AnimationClip clip)
        {
            var clipDeltaTime = 1 / clip.frameRate;
            var bindings = AnimationUtility.GetCurveBindings(clip);
            clipDeltaTime *= 1.1f;

            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);

                var keys = curve.keys;

                for (int i = 0; i < keys.Length - 1; i++)
                {
                    var next = keys[i + 1];
                    var current = keys[i];

                    if (i > 1)
                    {
                        var previous = keys[i - 1];
                    }

                    if (next.time - current.time < clipDeltaTime)
                    {
                        next.inTangent = float.PositiveInfinity;
                        keys[i + 1] = next;
                        Debug.Log(current.time);
                    }
                }
                curve.keys = keys;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
            EditorUtility.SetDirty(clip);
            AssetDatabase.Refresh();
            //AssetDatabase.SaveAssets();
        }
    }
}