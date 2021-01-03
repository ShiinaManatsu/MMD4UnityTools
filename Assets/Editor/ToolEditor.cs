using ShiinaManatsu.Tools.UI;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace ShiinaManatsu.Tools
{
    public class ToolEditor : UIWidgetsEditorWindow
    {
        [MenuItem("MMDExtensions/Open MMDExtensions Window", priority = 0)]
        public static void OpenWindow()
        {
            GetWindow<ToolEditor>("MMD4UnityTools").Show();
        }

        protected override void OnEnable()
        {
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/MATERIALICONS-REGULAR"), "Material Icons");
            base.OnEnable();
        }

        protected override Widget createWidget()
        {
            return new MaterialApp(
                home: new Scaffold(body: new ToolEditorUI(key: ToolEditorUIStatus<ToolEditorUI>.ToolEditorUIKey)),
                theme: new ThemeData(
                    primaryColor: Colors.pink,
                    primarySwatch: Colors.pink,
                    backgroundColor: Colors.white,
                    textTheme: new TextTheme(
                        title: new TextStyle(fontSize: 36, color: Colors.white.withOpacity(0.9f)),
                        button: new TextStyle(fontSize: 20, color: Colors.white.withOpacity(0.9f)),
                        headline: new TextStyle(fontSize: 18, color: Colors.white.withOpacity(0.9f)),
                        subhead: new TextStyle(fontSize: 14, color: Colors.white.withOpacity(0.9f)),
                        body1: new TextStyle(fontSize: 13, color: Colors.white.withOpacity(0.9f)),
                        body2: new TextStyle(fontSize: 13, color: Colors.white.withOpacity(0.6f))
                    )
                )
            );
        }

    }
}