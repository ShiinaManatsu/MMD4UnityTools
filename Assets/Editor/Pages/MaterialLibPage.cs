using ShiinaManatsu.Tools.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace ShiinaManatsu.Tools.UI.Pages
{
    public class MaterialLibPage : StatefulWidget
    {
        public string Title { get; set; }

        public static HashSet<UnityEngine.Material> Materials { get; } = new HashSet<UnityEngine.Material>();


        public MaterialLibPage(string title)
        {
            Title = title;
        }
        public override State createState() => new MaterialLibPageStatus<MaterialLibPage>();
    }

    internal class MaterialLibPageStatus<T> : State<T> where T : MaterialLibPage
    {
        private GameObject Current { get; set; }

        public override void initState()
        {
            base.initState();
            Selection.selectionChanged += OnSelectionChanged;
        }

        public override void dispose()
        {
            base.dispose();
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            var g = Selection.activeGameObject;
            var gs = g?.GetComponent<SkinnedMeshRenderer>();
            var gsc = g?.GetComponentInChildren<SkinnedMeshRenderer>();
            var gm = g?.GetComponent<MeshRenderer>();
            var gmc = g?.GetComponentInChildren<MeshRenderer>();
            using (WindowProvider.of(context).getScope())
            {
                setState(() =>
                {
                    if (gs == gsc == gm == gmc == null)
                        Current = null;
                    else
                        Current = g;
                });
            }
        }

        public override Widget build(BuildContext context) => new Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: new List<Widget>
            {
                new BlurredAppBar(child:new Row(
                    children:new List<Widget>
                    {
                        new SizedBox(width:8),
                        new RoundedIconButton(icon:Icons.arrow_back,onPressed:ToolEditorUIStatus<ToolEditorUI>.ToolEditorUIKey.currentState.GoHome),
                        new Container(child:new Text(widget.Title,style:Theme.of(context).textTheme.title),
                            margin:EdgeInsets.all(8)),
                    }
                )),
                new Expanded(
                    child:new SingleChildScrollView(
                        child:new Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children:new List<Widget>
                            {
                                //  Drag box
                                BuildDragBox(),
                                new Container(
                                    margin:EdgeInsets.symmetric(horizontal:8),
                                    child:new Text("Click to assign to selected gameobject",style:new TextStyle(color:Theme.of(context).backgroundColor.withOpacity(0.6f),fontSize:14))
                                )
                            }
                            .Concat(MaterialLibPage.Materials.Select(x=>new PrimaryButton(
                                content:x.name,
                                onPressed:Current==null
                                    ?(VoidCallback)null
                                    :()=>{
                                        var gs = Current.GetComponent<SkinnedMeshRenderer>();
                                        var gsc = Current.GetComponentInChildren<SkinnedMeshRenderer>();
                                        var gm = Current.GetComponent<MeshRenderer>();
                                        var gmc = Current.GetComponentInChildren<MeshRenderer>();
                                        if(gs!=null)
                                        {
                                            gs.material=x;
                                            return;
                                        }
                                        if (gsc != null)
                                        {
                                            gsc.material=x;
                                            return;
                                        }
                                        if (gm != null)
                                        {
                                            gm.material=x;
                                            return;
                                        }
                                        if (gmc != null)
                                        {
                                            gmc.material=x;
                                            return;
                                        }
                                    }
                                )))
                            .ToList()
                        )
                    )
                ),
            }
        );

        private Widget BuildDragBox() => new UnityObjectDetector(
            onRelease: (x) => setState(() =>
            {
                x.objectReferences
                    .Where(o => o is UnityEngine.Material)
                    .ToList()
                    .ForEach(m => MaterialLibPage.Materials.Add(m as UnityEngine.Material));
            }),
            child: new Container(
                padding: EdgeInsets.all(10),
                margin: EdgeInsets.all(8),
                alignment: Alignment.center,
                decoration: new BoxDecoration(
                    border: Border.all(color: Theme.of(context).primaryColor.withOpacity(0.1f), width: 4),
                    color: Theme.of(context).primaryColor.withOpacity(0.3f)),
                child: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>
                    {
                        new GlowIcon(icon:Icons.add,fontSize:20),
                        new SizedBox(width:8),
                        new GlowText(text:"Grab your materials here"),
                    }
                )
            )
        );
    }
}