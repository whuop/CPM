using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CPM.Editor.UIElements
{
    public class SplitViewPane : VisualElement
    {
        public SplitViewPane()
        {
            this.AddToClassList("unity-two-pane-split-view__content-container");
        }
    }
    
    public class TwoPaneSplitView : VisualElement
    {
        private static readonly string USS_CLASS_NAME = "unity-two-pane-split-view";
        private static readonly string CONTENT_CONTAINER_CLASS_NAME = "unity-two-pane-split-view__content-container";
        private static readonly string HANDLE_DRAG_LINE_CLASS_NAME = "unity-two-pane-split-view__dragline";

        private static readonly string HANDLE_DRAG_LINE_VERTICAL_CLASS_NAME =
            HANDLE_DRAG_LINE_CLASS_NAME + "--vertical";

        private static readonly string HANDLE_DRAG_LINE_HORIZONTAL_CLASS_NAME =
            HANDLE_DRAG_LINE_CLASS_NAME + "--horizontal";

        private static readonly string HANDLE_DRAG_LINE_ANCHOR_CLASS_NAME =
            "unity-two-pane-split-view__dragline-anchor";

        private static readonly string HANDLE_DRAG_LINE_ANCHOR_VERTICAL_CLASS_NAME =
            HANDLE_DRAG_LINE_CLASS_NAME + "--vertical";

        private static readonly string HANDLE_DRAG_LINE_ANCHOR_HORIZONTAL_CLASS_NAME =
            HANDLE_DRAG_LINE_CLASS_NAME + "--horizontal";

        private static readonly string VERTICAL_CLASS_NAME = "unity-two-pane-split-view--vertical";
        private static readonly string HORIZONTAL_CLASS_NAME = "unity-two-pane-split-view--horizontal";

        private static readonly string SPLIT_PANE_CLASS_NAME = "split-pane";
        
        public new class UxmlFactory : UxmlFactory<TwoPaneSplitView, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlIntAttributeDescription m_fixedPaneIndex = new UxmlIntAttributeDescription
                {name = "fixed-pane-index", defaultValue = 0};

            private UxmlIntAttributeDescription m_fixedPaneInitialSize = new UxmlIntAttributeDescription { name = "fixed-pane-initial-size", defaultValue = 0 };

            private UxmlStringAttributeDescription m_orientation = new UxmlStringAttributeDescription
                {name = "orientation", defaultValue = "horizontal"};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var fixedPaneIndex = m_fixedPaneIndex.GetValueFromBag(bag, cc);
                var fixedPaneInitialSize = m_fixedPaneInitialSize.GetValueFromBag(bag, cc);
                var orientationStr = m_orientation.GetValueFromBag(bag, cc);
                var orientation = orientationStr == "horizontal" ? Orientation.Horizontal : Orientation.Vertical;
                
                ((TwoPaneSplitView)ve).Init(fixedPaneIndex, fixedPaneInitialSize, orientation);
            }
        }

        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        private VisualElement m_leftPane;
        public VisualElement LeftPane
        {
            get => m_leftPane;
        }
        private VisualElement m_rightPane;

        public VisualElement RightPane
        {
            get => m_rightPane;
        }
        
        private VisualElement m_fixedPane;
        private VisualElement m_flexedPane;

        private VisualElement m_dragLine;
        private VisualElement m_dragLineAnchor;
        private float m_minDimension;

        private VisualElement m_content;

        private Orientation m_orientation;
        private int m_fixedPaneIndex;
        private float m_fixedPAneInitialDimension;
        
        private SquareResizer m_resizer;

        public override VisualElement contentContainer
        {
            get => m_content;
        }
        
        public TwoPaneSplitView()
        {
            AddToClassList(USS_CLASS_NAME);
            
            m_content = new VisualElement();
            m_content.name = "unity-content-container";
            m_content.AddToClassList(CONTENT_CONTAINER_CLASS_NAME);
            hierarchy.Add(m_content);
            
            //    Create dragline anchor
            m_dragLineAnchor = new VisualElement();
            m_dragLineAnchor.name = "unity-dragline-anchor";
            m_dragLineAnchor.AddToClassList(HANDLE_DRAG_LINE_ANCHOR_CLASS_NAME);
            hierarchy.Add(m_dragLineAnchor);
            
            //    Create drag
            m_dragLine = new VisualElement();
            m_dragLine.name = "unity-dragline";
            m_dragLine.AddToClassList(HANDLE_DRAG_LINE_CLASS_NAME);
            m_dragLineAnchor.Add(m_dragLine);
        }

        public TwoPaneSplitView(int fixedPaneIndex, float fixedPaneStartDimension, Orientation orientation) : this()
        {
            Init(fixedPaneIndex, fixedPaneStartDimension, orientation);
        }

        public void Init(int fixedPaneIndex, float fixedPaneInitialDimension, Orientation orientation)
        {
            m_orientation = orientation;
            m_minDimension = 100;
            m_fixedPaneIndex = fixedPaneIndex;
            m_fixedPAneInitialDimension = fixedPaneInitialDimension;

            if (m_orientation == Orientation.Horizontal)
                style.minWidth = m_fixedPAneInitialDimension;
            else
                style.minHeight = m_fixedPAneInitialDimension;
            
            
            SetupDirectionClasses(m_content, m_orientation, HORIZONTAL_CLASS_NAME, VERTICAL_CLASS_NAME);
            SetupDirectionClasses(m_dragLineAnchor, m_orientation, HANDLE_DRAG_LINE_ANCHOR_HORIZONTAL_CLASS_NAME, HANDLE_DRAG_LINE_ANCHOR_VERTICAL_CLASS_NAME);
            SetupDirectionClasses(m_dragLine, m_orientation, HANDLE_DRAG_LINE_HORIZONTAL_CLASS_NAME, HANDLE_DRAG_LINE_VERTICAL_CLASS_NAME);

            if (m_resizer != null)
            {
                m_dragLineAnchor.RemoveManipulator(m_resizer);
                m_resizer = null;
            }

            if (m_content.childCount != 2)
                RegisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
            else
                PostDisplaySetup();
        }

        /*public void SetPanes(VisualElement leftPane, VisualElement rightPane)
        {
            //    First clear previous content
            this.contentContainer.Clear();

            leftPane.style.position = Position.Absolute;
            leftPane.style.marginRight = 15;
            rightPane.style.position = Position.Absolute;
            rightPane.style.marginLeft = 15;
            
            leftPane.name = "LeftPane";
            rightPane.name = "RightPane";

            //    Add new panes to the split view
            this.contentContainer.hierarchy.Add(leftPane);
            this.contentContainer.hierarchy.Add(rightPane);

            //leftPane.style.height = contentContainer.style.height;
        }*/

        public void ResizePanes(float height)
        {
            m_leftPane.style.height = height;
            m_rightPane.style.height = height;
        }

        private void OnPostDisplaySetup(GeometryChangedEvent evt)
        {
            if (m_content.childCount != 2)
            {
                Debug.LogError("TwoPaneSplitView needs exactly 2 children!");
                return;
            }

            PostDisplaySetup();
            UnregisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
            RegisterCallback<GeometryChangedEvent>(OnSizeChange);
        }

        private void PostDisplaySetup()
        {
            Debug.Log("GEOMETRY CHANGE!");
            if (m_content.childCount != 2)
            {
                Debug.LogError("TwoPaneSplitView needs exactly 2 children!");
                return;
            }

            m_leftPane = m_content[0];
            if (m_fixedPaneIndex == 0)
            {
                m_fixedPane = m_leftPane;
                if (m_orientation == Orientation.Horizontal)
                {
                    m_leftPane.style.width = m_fixedPAneInitialDimension;
                }
                else
                {
                    m_leftPane.style.height = m_fixedPAneInitialDimension;
                }
            }
            else
            {
                m_flexedPane = m_leftPane;
            }

            m_rightPane = m_content[1];
            if (m_fixedPaneIndex == 1)
            {
                m_fixedPane = m_rightPane;
                if (m_orientation == Orientation.Horizontal)
                {
                    m_rightPane.style.width = m_fixedPAneInitialDimension;
                }
                else
                {
                    m_rightPane.style.height = m_fixedPAneInitialDimension;
                }
            }
            else
            {
                m_flexedPane = m_rightPane;
            }

            m_fixedPane.style.flexShrink = 0;
            m_flexedPane.style.flexGrow = 1;
            m_flexedPane.style.flexShrink = 0;
            m_flexedPane.style.flexBasis = 0;

            if (m_orientation == Orientation.Horizontal)
            {
                if (m_fixedPaneIndex == 0)
                {
                    m_dragLineAnchor.style.left = m_fixedPAneInitialDimension;
                }
                else
                {
                    m_dragLineAnchor.style.left = this.resolvedStyle.width - m_fixedPAneInitialDimension;
                }
            }
            else
            {
                if (m_fixedPaneIndex == 0)
                    m_dragLineAnchor.style.top = m_fixedPAneInitialDimension;
                else
                    m_dragLineAnchor.style.top = this.resolvedStyle.height - m_fixedPAneInitialDimension;
            }

            int direction = 1;
            if (m_fixedPaneIndex == 0)
                direction = 1;
            else
                direction = -1;
            
            if (m_fixedPaneIndex == 0)
                m_resizer = new SquareResizer(this, direction, m_minDimension, m_orientation);
            else
                m_resizer = new SquareResizer(this, direction, m_minDimension, m_orientation);
                
            m_dragLineAnchor.AddManipulator(m_resizer);
            
            UnregisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
            RegisterCallback<GeometryChangedEvent>(OnSizeChange);
        }

        private void OnSizeChange(GeometryChangedEvent evt)
        {
            Debug.Log("SIZE CHANGE!!!!");
            var maxLength = this.resolvedStyle.width;
            var dragLinePos = m_dragLineAnchor.resolvedStyle.left;
            var activeElementPos = m_fixedPane.resolvedStyle.left;
            if (m_orientation == Orientation.Vertical)
            {
                maxLength = this.resolvedStyle.height;
                dragLinePos = m_dragLineAnchor.resolvedStyle.top;
                activeElementPos = m_fixedPane.resolvedStyle.top;
            }

            if (m_fixedPaneIndex == 0 && dragLinePos > maxLength)
            {
                var delta = maxLength - dragLinePos;
                m_resizer.Apply(delta);
            }
            else if (m_fixedPaneIndex == 1)
            {
                if (activeElementPos < 0)
                {
                    var delta = -dragLinePos;
                    m_resizer.Apply(delta);
                }
                else
                {
                    if (m_orientation == Orientation.Horizontal)
                    {
                        m_dragLineAnchor.style.left = activeElementPos;
                    }
                    else
                    {
                        m_dragLineAnchor.style.top = activeElementPos;
                    }
                }
            }
        }
        
        private void SetupDirectionClasses(VisualElement element, Orientation orientation, string horizontalClass,
            string verticalClass)
        {
            element.RemoveFromClassList(horizontalClass);
            element.RemoveFromClassList(verticalClass);
            if (orientation == Orientation.Horizontal)
                element.AddToClassList(horizontalClass);
            else
                element.AddToClassList(verticalClass);
        }

        class SquareResizer : MouseManipulator
        {
            private Vector2 m_start;
            protected bool m_active;
            private TwoPaneSplitView m_splitView;
            private VisualElement m_pane;
            private int m_direction;
            private float m_minWidth;
            private Orientation m_orientation;

            public SquareResizer(TwoPaneSplitView splitView, int dir, float minWidth, Orientation orientation)
            {
                m_orientation = orientation;
                m_minWidth = minWidth;
                m_splitView = splitView;
                m_pane = splitView.m_fixedPane;
                m_direction = dir;
                activators.Add(new ManipulatorActivationFilter{ button = MouseButton.LeftMouse});
                m_active = false;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<MouseDownEvent>(OnMouseDown);
                target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            }

            public void Apply(float delta)
            {
                float oldDimension = m_orientation == Orientation.Horizontal
                    ? m_pane.resolvedStyle.width
                    : m_pane.resolvedStyle.height;
                float newDimension = oldDimension + delta;

                if (newDimension < oldDimension && newDimension < m_minWidth)
                    newDimension = m_minWidth;

                float maxLength = m_orientation == Orientation.Horizontal
                    ? m_splitView.resolvedStyle.width
                    : m_splitView.resolvedStyle.height;
                if (newDimension > oldDimension && newDimension > maxLength)
                    newDimension = maxLength;

                if (m_orientation == Orientation.Horizontal)
                {
                    m_pane.style.width = newDimension;
                    if (m_splitView.m_fixedPaneIndex == 0)
                    {
                        target.style.left = newDimension;
                        //m_splitView.m_flexedPane.style.left = newDimension;
                    }
                    else
                    {
                        target.style.left = m_splitView.resolvedStyle.width - newDimension;
                        //m_splitView.m_flexedPane.style.left = m_splitView.resolvedStyle.width - newDimension;
                    }
                        
                }
                else
                {
                    m_pane.style.height = newDimension;
                    if (m_splitView.m_fixedPaneIndex == 0)
                    {
                        target.style.top = newDimension;
                        //m_splitView.m_flexedPane.style.top = newDimension;
                    }
                    else
                    {
                        target.style.top = m_splitView.resolvedStyle.height - newDimension;
                        //m_splitView.m_flexedPane.style.top = m_splitView.resolvedStyle.height - newDimension;
                    }
                }
            }

            protected virtual void OnMouseDown(MouseDownEvent e)
            {
                if (m_active)
                {
                    e.StopImmediatePropagation();
                    return;
                }

                if (CanStartManipulation(e))
                {
                    m_start = e.localMousePosition;

                    m_active = true;
                    target.CaptureMouse();
                    e.StopPropagation();
                }
            }

            protected virtual void OnMouseMove(MouseMoveEvent e)
            {
                if (!m_active || !target.HasMouseCapture())
                    return;

                Vector2 diff = e.localMousePosition - m_start;
                float mouseDiff = diff.x;
                if (m_orientation == Orientation.Vertical)
                    mouseDiff = diff.y;

                float delta = m_direction * mouseDiff;
                Apply(delta);
                e.StopPropagation();
            }

            protected virtual void OnMouseUp(MouseUpEvent e)
            {
                if (!m_active || !target.HasMouseCapture() || !CanStopManipulation(e))
                {
                    return;
                }

                m_active = false;
                target.ReleaseMouse();
                e.StopPropagation();
            }
        }
    }

}

