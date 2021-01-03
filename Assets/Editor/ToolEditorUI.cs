using ShiinaManatsu.Tools.UI.Pages;
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

namespace ShiinaManatsu.Tools.UI
{
    public class ToolEditorUI : StatefulWidget
    {
        public ToolEditorUI(Key key = null) : base(key: key) { }
        public override State createState() => new ToolEditorUIStatus<ToolEditorUI>();
    }

    internal class ToolEditorUIStatus<T> : State<T> where T : ToolEditorUI
    {
        public static GlobalKey<ToolEditorUIStatus<ToolEditorUI>> ToolEditorUIKey { get; set; } = new LabeledGlobalKey<ToolEditorUIStatus<ToolEditorUI>>();

        private PageController PageController { get; set; } = new PageController(initialPage: 0, keepPage: true);

        public void GoHome() => PageController.animateToPage(page: 0, duration: TimeSpan.FromMilliseconds(500), curve: Curves.ease);
        //public void GoPage(int index) => PageController.animateToPage(page: index, duration: TimeSpan.FromMilliseconds(700), curve: Curves.ease);
        public void GoPage(int index) => PageController.jumpToPage(page: index);

        public override Widget build(BuildContext context) => new Stack(
            fit: StackFit.expand,
            children: new List<Widget>
            {
                new Unity.UIWidgets.widgets.Image(image:new AssetImage("MMDExtensionsBackgound"),
                        fit:BoxFit.cover,
                        alignment:Alignment.topCenter),
                new BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 6, sigmaY: 6),
                    child:new Container(color:Colors.black38)
                ),
                new PageView(
                    controller: PageController,
                    physics:new NeverScrollableScrollPhysics(),
                    children: new List<Widget>
                    {
                        new HomePage(),
                        new MaterialPage(title:"Fix materials"),
                        new MappingMaterialPage(title:"Mapping from material lib"),
                        new MaterialLibPage(title:"Manage Materials"),
                    }
                ),
            }
        );
    }

    #region Pages

    internal class HomePage : StatefulWidget
    {
        public override State createState() => new HomePageStatus<HomePage>();
    }

    internal class HomePageStatus<T> : State<T> where T : HomePage
    {

        public override Widget build(BuildContext context) => new Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: new List<Widget>
            {
                new BlurredAppBar(child:new Padding(child:new Text("Menu",style:Theme.of(context).textTheme.title),padding:EdgeInsets.only(left:16))),
                new Expanded(
                    child:new SingleChildScrollView(
                        child:new Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children:new List<Widget>
                            {
                               new MenuButton(content:"Upgrade to HDRP Material",onPressed:()=>ToolEditorUIStatus<ToolEditorUI>.ToolEditorUIKey.currentState.GoPage(1)),
                               new MenuButton(content:"Mapping Material",onPressed:()=>ToolEditorUIStatus<ToolEditorUI>.ToolEditorUIKey.currentState.GoPage(2)),
                               new MenuButton(content:"Material Lib",onPressed:()=>ToolEditorUIStatus<ToolEditorUI>.ToolEditorUIKey.currentState.GoPage(3)),
                            }
                        )
                    )
                ),
            }
        );
    }
    #endregion
}

namespace ShiinaManatsu.Tools.Widgets
{
    public class PrimaryButton : StatefulWidget
    {
        public string Content { get; set; }
        public VoidCallback OnPressed { get; set; }
        public Widget Child { get; set; }
        public PrimaryButton(string content = "", Widget child = null, VoidCallback onPressed = null)
        {
            Content = content;
            OnPressed = onPressed;
            Child = child;
        }
        public override State createState() => new PrimaryButtonStatus<PrimaryButton>();
    }

    internal class PrimaryButtonStatus<T> : State<T> where T : PrimaryButton
    {
        private bool isPointIn = false;
        public override Widget build(BuildContext context) => new Listener(
            onPointerEnter: (x) => setState(() => isPointIn = true),
            onPointerExit: (x) => setState(() => isPointIn = false),
            child: new AnimatedContainer(
                duration: TimeSpan.FromMilliseconds(100),
                curve: Curves.ease,
                child: new FlatButton(
                    color: Theme.of(context).primaryColor.withOpacity(0.7f),
                    disabledColor: Theme.of(context).primaryColor.withOpacity(0.2f),
                    disabledTextColor: Theme.of(context).textTheme.button.color.withOpacity(0.4f),
                    textColor: Theme.of(context).textTheme.button.color,
                    padding: EdgeInsets.all(8),
                    child: widget.Child == null ? new Text(widget.Content, style: Theme.of(context).textTheme.button) : widget.Child,
                    onPressed: widget.OnPressed
                ),
                margin: EdgeInsets.all(8),
                decoration: new BoxDecoration(
                    color: Colors.transparent,
                    borderRadius: BorderRadius.circular(radius: 4f),
                    boxShadow: new List<BoxShadow>{
                        new BoxShadow(color: Theme.of(context).backgroundColor.withOpacity(0.3f),blurRadius:isPointIn?8f:4f)
                    }
                )
            )
        );
    }

    public class RoundedIconButton : StatefulWidget
    {
        public VoidCallback OnPressed { get; set; }

        public IconData Icon { get; set; }

        public RoundedIconButton(IconData icon, VoidCallback onPressed = null)
        {
            OnPressed = onPressed;
            Icon = icon;
        }
        public override State createState() => new RoundedButtonStatus<RoundedIconButton>();
    }

    internal class RoundedButtonStatus<T> : SingleTickerProviderStateMixin<T> where T : RoundedIconButton
    {
        private AnimationController AnimationController { get; set; }
        public override void initState()
        {
            base.initState();
            AnimationController = new AnimationController(value: 0, vsync: this);
        }

        public override Widget build(BuildContext context) => new Container(
            padding: EdgeInsets.only(bottom: 5),
            child: new Listener(
                onPointerEnter: (x) => AnimationController.animateTo(target: 1, duration: TimeSpan.FromMilliseconds(100), curve: Curves.ease),
                onPointerExit: (x) => AnimationController.animateTo(target: 0, duration: TimeSpan.FromMilliseconds(100), curve: Curves.ease),
                onPointerUp: (x) => widget.OnPressed(),
                child: new AnimatedBuilder(
                    animation: AnimationController,
                    builder: (context, child) => new RichText(
                       overflow: TextOverflow.visible, // Never clip.
                       text: new TextSpan(
                         text: char.ConvertFromUtf32(widget.Icon.codePoint), style: new TextStyle(
                           inherit: false,
                           color: Colors.white,
                           fontSize: 36f,
                           fontFamily: widget.Icon.fontFamily,
                           shadows: new List<BoxShadow> {
                            new BoxShadow(
                              color: Colors.white,
                              blurRadius: AnimationController.value * 2)
                           }
                         )
                       )
                     )
                )
            )
        );
    }

    public class BlurredAppBar : StatelessWidget
    {
        private Widget Child { get; set; }
        public BlurredAppBar(Widget child)
        {
            Child = child;
        }
        public override Widget build(BuildContext context) =>
            new Container(
                height: 64,
                margin: EdgeInsets.only(bottom: 4),
                child: new Stack(
                    alignment: Alignment.centerLeft,
                    children: new List<Widget>
                    {
                        new BackdropFilter(filter: ImageFilter.blur(sigmaX: 6, sigmaY: 6),child:new Container(width:float.MaxValue,color:Colors.black26)), Child
                    }
                ),
                decoration: new BoxDecoration(boxShadow: new List<BoxShadow> { new BoxShadow(color: Colors.black38, blurRadius: 4) })
            );
    }

    public class BlurredWidget : StatelessWidget
    {
        private Widget Child { get; set; }
        private float Height { get; set; }
        public BlurredWidget(Widget child, float height = 64)
        {
            Child = child;
            Height = height;
        }
        public override Widget build(BuildContext context) =>
            new Container(
                height: Height,
                margin: EdgeInsets.only(bottom: 4),
                child: new Stack(
                    alignment: Alignment.centerLeft,
                    children: new List<Widget>
                    {
                        new BackdropFilter(filter: ImageFilter.blur(sigmaX: 6, sigmaY: 6),child:new Container(width:float.MaxValue,color:Colors.black26)), Child
                    }
                ),
                decoration: new BoxDecoration(boxShadow: new List<BoxShadow> { new BoxShadow(color: Colors.black38, blurRadius: 4) })
            );
    }

    public class DragableBox : StatefulWidget
    {
        public UnityEngine.Object Object { get; set; }
        public string Hint { get; set; }
        public DragableBox(string hint = "")
        {
            Hint = hint;
        }
        public override State createState() => new DragableBoxStatus<DragableBox>();
    }

    internal class DragableBoxStatus<T> : State<T> where T : DragableBox
    {
        public override Widget build(BuildContext context) => new Container(
            margin: EdgeInsets.only(top: 8),
            child: new UnityObjectDetector(
                onRelease: (x) => setState(() => widget.Object = x.objectReferences.FirstOrDefault()),
                child: new BlurredWidget(
                    child: new Container(child: new Text(widget?.Object?.name ?? widget.Hint,
                        style: widget?.Object?.name != null ? Theme.of(context).textTheme.body1 : Theme.of(context).textTheme.body2),
                    margin: EdgeInsets.symmetric(horizontal: 8)), height: 24f)
                )
            );
    }

    public class GlowIcon : StatelessWidget
    {
        private IconData Icon { get; set; }
        private float BlurRadius { get; set; }
        private float FontSize { get; set; }
        public GlowIcon(IconData icon, float blurRadius = 2, float fontSize = 16)
        {
            Icon = icon;
            BlurRadius = blurRadius;
            FontSize = fontSize;
        }
        public override Widget build(BuildContext context) => new RichText(
            overflow: TextOverflow.visible, // Never clip.
            text: new TextSpan(
                text: char.ConvertFromUtf32(Icon.codePoint), style: new TextStyle(
                inherit: false,
                color: Colors.white,
                fontSize: FontSize,
                fontFamily: Icon.fontFamily,
                shadows: new List<BoxShadow> {
                new BoxShadow(
                    color: Colors.white,
                    blurRadius: BlurRadius)
                }
                )
            )
         );
    }
    public class GlowText : StatelessWidget
    {
        private string Text { get; set; }
        private float BlurRadius { get; set; }
        private float FontSize { get; set; }
        public GlowText(string text, float blurRadius = 2, float fontSize = 16)
        {
            Text = text;
            BlurRadius = blurRadius;
            FontSize = fontSize;
        }
        public override Widget build(BuildContext context) => new RichText(
            overflow: TextOverflow.visible, // Never clip.
            text: new TextSpan(
                text: Text, style: new TextStyle(
                inherit: false,
                color: Colors.white,
                fontSize: FontSize,
                shadows: new List<BoxShadow> {
                new BoxShadow(
                    color: Colors.white,
                    blurRadius: BlurRadius)
                }
                )
            )
         );
    }

    public class MenuButton : StatefulWidget
    {
        public VoidCallback OnPressed { get; set; }
        public string Content { get; set; }
        public MenuButton(string content = "", VoidCallback onPressed = null)
        {
            Content = content;
            OnPressed = onPressed;
        }
        public override State createState() => new MenuButtonStatus<MenuButton>();
    }

    internal class MenuButtonStatus<T> : SingleTickerProviderStateMixin<T> where T : MenuButton
    {
        private AnimationController AnimationController { get; set; }
        public override void initState()
        {
            base.initState();
            AnimationController = new AnimationController(value: 0, vsync: this);
        }
        public override Widget build(BuildContext context) => new Listener(
            onPointerEnter: (x) => AnimationController.animateTo(target: 1, duration: TimeSpan.FromMilliseconds(100), curve: Curves.ease),
            onPointerExit: (x) => AnimationController.animateTo(target: 0, duration: TimeSpan.FromMilliseconds(100), curve: Curves.ease),
            onPointerUp: (x) => widget.OnPressed(),
            child: new AnimatedBuilder(
                animation: AnimationController,
                builder: (context, child) => new Container(
                    padding: EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: new BoxDecoration(
                        border: Border.all(color: Theme.of(context).primaryColor.withOpacity(AnimationController.value * 0.1f)),
                        color: Theme.of(context).primaryColor.withOpacity(AnimationController.value * 0.3f)),
                    margin: EdgeInsets.only(top: 4, left: 8, right: 8),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget>{
                            new GlowText(text:widget.Content,blurRadius:AnimationController.value*2),
                            new GlowIcon(icon:Icons.arrow_forward,blurRadius:AnimationController.value*2),
                        }
                    )
                )
            )
        );
    }
}

namespace ShiinaManatsu.Tools.Widgets.Extension
{
    public static class WidgetExtension
    {
    }
}