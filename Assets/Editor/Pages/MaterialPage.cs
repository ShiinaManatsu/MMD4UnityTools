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
    public class MaterialPage : StatefulWidget
    {
        public string Title { get; set; }
        public MaterialPage(string title)
        {
            Title = title;
        }
        public override State createState() => new MaterialPageStatus<MaterialPage>();
    }

    internal class MaterialPageStatus<T> : State<T> where T : MaterialPage
    {
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
                    child:new Padding(
                        padding:EdgeInsets.symmetric(horizontal:8),
                        child:new SingleChildScrollView(
                            child:new Column(
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children:new List<Widget>
                                {
#region MMD4Mecanim Model
		                            new Text("MMD4Mecanim Model",style:Theme.of(context).textTheme.headline),
                                    new Text("FBX converted from MMD4Mecanim",style:Theme.of(context).textTheme.subhead),

                                    new DragableBox(hint:"Drag your pmx here"),
                                    new DragableBox(hint:"Drag your prefab here"), 
	#endregion

#region Blender FBX
		                            new Text("Blender FBX",style:Theme.of(context).textTheme.headline),
                                    new Text("FBX exported from blender with CATS or MMDTools",style:Theme.of(context).textTheme.subhead),

                                    new DragableBox(hint:"Drag your pmx here"),
                                    new DragableBox(hint:"Drag your prefab here"), 
	#endregion

                                    new Text("MMD Bridge Alembic",style:Theme.of(context).textTheme.headline),
                                    new Text("Alembic exported MMD Bridge",style:Theme.of(context).textTheme.subhead),


                                }
                            )
                        )
                    )
                ),
            }
        );
    }

}