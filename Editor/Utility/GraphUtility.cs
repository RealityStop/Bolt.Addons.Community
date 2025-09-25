using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class GraphUtility
    {
        public static void AddNewUnitContextual(FlowGraph graph, FlowCanvas canvas, System.Action<IGraphElement> added, System.Action canceled = null)
        {
            OverrideContextIfNeeded(async () =>
            {
                canvas.NewUnitContextual();
                var elementResult = await WaitForElementAddedOrCanceled(graph);
                if (elementResult.Canceled)
                {
                    canceled?.Invoke();
                    return;
                }

                added?.Invoke(elementResult.Element);
            });
        }

        public static void OverrideContextIfNeeded(System.Action action)
        {
            bool manuallySet = false;
            if (LudiqGraphsEditorUtility.editedContext.value == null && GraphWindow.active != null && GraphWindow.active.reference != null)
            {
                var context = GraphWindow.active.reference.Context();
                if (context != null)
                {
                    manuallySet = true;
                    LudiqGraphsEditorUtility.editedContext.BeginOverride(context);
                }
            }
            action?.Invoke();
            if (manuallySet) LudiqGraphsEditorUtility.editedContext.EndOverride();
        }

        public static void DescribeAnalyzeAndDefineFlowGraph(this IGraphContext context)
        {
            if (context.graph is FlowGraph graph)
            {
                context.DescribeAndAnalyze();
                foreach (var unit in graph.units)
                {
                    unit.Define();
                }
            }
        }

        private static Task<ElementAddResult> WaitForElementAddedOrCanceled(FlowGraph graph)
        {
            var tcs = new TaskCompletionSource<ElementAddResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void AddedHandler(IGraphElement element)
            {
                graph.elements.ItemAdded -= AddedHandler;
                EditorApplication.update -= CheckClosed;
                tcs.TrySetResult(new ElementAddResult { Canceled = false, Element = element });
            }

            void CheckClosed()
            {
                if (FuzzyWindow.instance == null)
                {
                    graph.elements.ItemAdded -= AddedHandler;
                    EditorApplication.update -= CheckClosed;
                    tcs.TrySetResult(new ElementAddResult { Canceled = true, Element = null });
                }
            }

            void WaitForOpen()
            {
                if (FuzzyWindow.instance != null)
                {
                    EditorApplication.update -= WaitForOpen;

                    graph.elements.ItemAdded += AddedHandler;
                    EditorApplication.update += CheckClosed;
                }
            }

            EditorApplication.update += WaitForOpen;

            return tcs.Task;
        }
        private struct ElementAddResult
        {
            public bool Canceled;
            public IGraphElement Element;
        }
    }
}
