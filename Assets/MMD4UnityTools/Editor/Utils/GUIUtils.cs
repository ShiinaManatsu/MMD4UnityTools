using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MMD4UnityTools.Editor.GUI
{
    public class BoxGroup : IDisposable
    {
        string Title { get; }
        Orientation Orientation { get; }

        public BoxGroup(string title, Orientation orientation)
        {
            Title = title;
            Orientation = orientation;

            GUILayout.Space(12);

            if (Orientation == Orientation.Horizontal)
            {
                GUILayout.BeginHorizontal(title, "window");
            }
            else
            {
                GUILayout.BeginVertical(title, "window");
            }
        }

        public void Dispose()
        {
            if (Orientation == Orientation.Horizontal)
            {
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.EndVertical();
            }
        }
    }
}