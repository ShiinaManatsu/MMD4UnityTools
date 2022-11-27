using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MMD4UnityTools.Editor.Utils
{
    public static class EditorGUIHelpers
    {
        public static T ObjectField<T>(this T obj, string label, bool allowSceneObject) where T : Object
        {
            var value = EditorGUILayout.ObjectField(label ?? obj.name, obj, typeof(T), allowSceneObject) as T;
            return value;
        }
    }

}