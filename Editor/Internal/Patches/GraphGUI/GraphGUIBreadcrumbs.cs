using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUIBreadcrumbs
    {
        public static VisualElement Create(GraphWindow window, WindowState windowState, VisualElement toolbar)
        {
            Sprite breadCrumbRootIcon = null;
            Sprite breadCrumbIcon = null;

            var breadcrumbContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 20,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,
                }
            };

            SyncBreadcrumbs();

            void SyncBreadcrumbs()
            {
                if (window.reference == null || !window.reference.isValid)
                    return;

                var breadcrumbs = window.reference.GetBreadcrumbs().ToList();

                while (breadcrumbContainer.childCount > breadcrumbs.Count)
                    breadcrumbContainer.RemoveAt(breadcrumbContainer.childCount - 1);

                for (int i = 0; i < breadcrumbs.Count; i++)
                {
                    var breadcrumb = breadcrumbs[i];

                    ToolbarButton btn;

                    if (i >= breadcrumbContainer.childCount)
                    {
                        btn = CreateBreadcrumbButton();
                        breadcrumbContainer.Add(btn);
                    }
                    else
                    {
                        btn = breadcrumbContainer[i] as ToolbarButton;
                    }

                    if (breadcrumb.isRoot)
                        btn.style.marginLeft = 0;
                    else
                        btn.style.marginLeft = -10;

                    btn.userData = breadcrumb;

                    var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);

#if !UNITY_2023_2_OR_NEWER
                    var img = btn.Q<Image>("BreadcrumbIcon");
                    if (img != null)
                    {
                        img.image = title.image;
                        if (breadcrumb.isRoot)
                        {
                            img.style.marginRight = 45;
                        }
                    }

                    btn.text = (breadcrumb.isRoot ? "       " : " ") + title.text;
                    btn.style.overflow = Overflow.Hidden;
                    btn.style.textOverflow = TextOverflow.Ellipsis;

                    btn.RegisterCallback<ChangeEvent<string>>(s =>
                    {
                        var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);
                        var size = btn.MeasureTextSize(title.text, btn.resolvedStyle.width, VisualElement.MeasureMode.Undefined, 20, VisualElement.MeasureMode.Exactly).x + 50;
                        if (size > btn.resolvedStyle.minWidth.value)
                            btn.style.width = size;
                        else
                            btn.style.width = 80;
                    });

                    btn.RegisterCallback<GeometryChangedEvent>(evt =>
                    {
                        var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);

                        var textSize = btn.MeasureTextSize(
                            title.text,
                            float.NaN,
                            VisualElement.MeasureMode.Undefined,
                            20,
                            VisualElement.MeasureMode.Exactly
                        ).x + 50;

                        btn.style.width = Mathf.Max(80, textSize);
                    });
#endif
                    bool isCurrent = breadcrumb == window.reference;
                    btn.style.unityFontStyleAndWeight = isCurrent ? FontStyle.Bold : FontStyle.Normal;
                }
            }

            windowState.contextChanged += () =>
            {
                toolbar.schedule.Execute(() =>
                {
                    SyncBreadcrumbs();
                });
            };

            ToolbarButton CreateBreadcrumbButton()
            {
                var btn = new ToolbarButton();
                btn.focusable = false;
                btn.style.height = 20;
                btn.style.fontSize = 9;
                btn.style.backgroundColor = Color.clear;

#if !UNITY_2023_2_OR_NEWER
                var img = new Image
                {
                    name = "BreadcrumbIcon"
                };
                img.AlignSelf(Align.FlexStart);

                btn.Insert(0, img);
#endif

                btn.style.marginRight = 0;
                btn.style.paddingLeft = 10;
#if UNITY_2023_2_OR_NEWER
                btn.style.paddingRight = 10;
#else
                btn.style.paddingRight = 0;
#endif
                btn.style.borderLeftWidth = 0;
                btn.style.borderRightWidth = 0;

                btn.style.minWidth = 80;

                btn.style.borderRightWidth = 0;
                btn.style.borderLeftWidth = 0;

                btn.style.unityTextAlign = TextAnchor.MiddleCenter;
                btn.style.whiteSpace = WhiteSpace.NoWrap;

                btn.clicked += () =>
                {
                    var target = btn.userData as GraphReference;
                    if (target != null && target != window.reference)
                    {
                        window.reference = target;
                        SyncBreadcrumbs();
                    }
                };

                var assigned = false;
                btn.Add(new IMGUIContainer(() =>
                {
                    var breadcrumb = btn.userData as GraphReference;
                    if (breadcrumb == null) return;

                    if (breadCrumbRootIcon == null || breadCrumbIcon == null)
                    {
                        var root = Styles.toolbarBreadcrumbRoot.normal.background;
                        var child = Styles.toolbarBreadcrumb.normal.background;

                        breadCrumbRootIcon = Sprite.Create(
                            root,
                            new Rect(0, 0, root.width, root.height),
                            new Vector2(0.5f, 0.5f),
                            100,
                            0,
                            SpriteMeshType.FullRect,
                            new Vector4(10, 0, 10, 0)
                        );

                        breadCrumbIcon = Sprite.Create(
                            child,
                            new Rect(0, 0, child.width, child.height),
                            new Vector2(0.5f, 0.5f),
                            100,
                            0,
                            SpriteMeshType.FullRect,
                            new Vector4(10, 0, 10, 0)
                        );
                    }

                    if (!DescriptorProvider.instance.IsValid(breadcrumb.parent)) return;

                    var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);

#if !UNITY_2023_2_OR_NEWER
                    btn.text = (breadcrumb.isRoot ? "       " : " ") + title.text;
#else
                    btn.text = title.text;

                    btn.schedule.Execute(() =>
                    {
                        if (title.image is Texture2D iconTex)
                            btn.iconImage = iconTex;
                    });
#endif
                    if (!assigned)
                    {
                        assigned = true;
                        btn.style.backgroundImage = Background.FromSprite(
                            breadcrumb.isRoot ? breadCrumbRootIcon : breadCrumbIcon
                        );
                    }
                }));

                return btn;
            }

            return breadcrumbContainer;
        }

        public static class Styles
        {
            static Styles()
            {
                toolbarBreadcrumbRoot = new GUIStyle(new GUIStyle("GUIEditor.BreadcrumbLeftBackground"))
                {
                    alignment = TextAnchor.MiddleCenter
                };
                toolbarBreadcrumbRoot.padding.bottom++;
                toolbarBreadcrumbRoot.padding.left = 0;
                toolbarBreadcrumbRoot.padding.right = 15;
                toolbarBreadcrumbRoot.fontSize = 9;

                toolbarBreadcrumb = new GUIStyle("GUIEditor.BreadcrumbMidBackground")
                {
                    alignment = TextAnchor.MiddleCenter
                };
                toolbarBreadcrumb.padding.bottom++;
                toolbarBreadcrumb.padding.right = 15;
                toolbarBreadcrumb.padding.left = 10;
                toolbarBreadcrumb.fontSize = 9;
            }

            public static readonly GUIStyle toolbarBreadcrumbRoot;
            public static readonly GUIStyle toolbarBreadcrumb;
        }
    }
}