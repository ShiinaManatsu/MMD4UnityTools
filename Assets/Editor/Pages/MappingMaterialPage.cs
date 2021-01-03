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
    public class MappingMaterialPage : StatefulWidget
    {
        public string Title { get; set; }
        public MappingMaterialPage(string title)
        {
            Title = title;
        }
        public override State createState() => new MappingMaterialPageStatus<MappingMaterialPage>();
    }

    internal class MappingMaterialPageStatus<T> : State<T> where T : MappingMaterialPage
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
                                }
                            )
                        )
                    )
                ),
            }
        );
    }
}