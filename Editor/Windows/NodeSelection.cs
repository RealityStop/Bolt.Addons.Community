using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

#if VISUAL_SCRIPTING_1_7
using NestedNode = Unity.VisualScripting.SubgraphUnit;
#else
using NestedNode = Unity.VisualScripting.SuperUnit;
#endif

namespace Unity.VisualScripting.Community
{
    public static partial class NodeSelection
    {
        public static void Convert(GraphSource source)
        {
            if (source == GraphSource.Embed)
            {
                ConvertToEmbed();
            }

            if (source == GraphSource.Macro)
            {
                ConvertToMacro();
            }
        }

        private static void ConvertToEmbed()
        {
            var selection = GraphWindow.active?.reference?.graph?.Canvas().selection;

            if (selection != null && selection.Count > 0)
            {
                Undo.RegisterCompleteObjectUndo(GraphWindow.active?.reference.rootObject, "Add Sub Graph to Script Graph");

                var superUnit = new NestedNode();
                var superUnitGraph = new FlowGraph();
                var superUnitCanvas = superUnitGraph.Canvas<FlowCanvas>();
                var elements = selection.ToList();

                superUnit.position = GetNestedNodePosition(elements);

                ((FlowGraph)GraphWindow.active.reference.graph).units.Add(superUnit);

                superUnit.nest.SwitchToEmbed(superUnitGraph);

                CopyElementsToGraph(superUnitCanvas);

                var graphInput = new GraphInput();
                var graphOutput = new GraphOutput();

                var listWithoutInputOutput = ((FlowGraph)GraphWindow.active.reference.graph).units.ToList();

                superUnitGraph.units.Add(graphInput);
                superUnitGraph.units.Add(graphOutput);

                CopyControlConnectionData(superUnitGraph, graphInput, graphOutput, GetControlIndices(selection, superUnit));
                CopyValueConnectionData(superUnitGraph, graphInput, graphOutput, GetValueIndices(selection, superUnit));
                CopyInvalidConnectionData(superUnitGraph, graphInput, graphOutput, GetInvalidIndices(selection, superUnit));

                SetInputOutputPosition(superUnitGraph, graphInput, graphOutput);

                superUnitGraph.pan = superUnit.position;

                RemoveUnusedDefinitions(superUnitGraph);

                superUnitGraph.PortDefinitionsChanged();

                GraphWindow.active.reference.graph.Canvas<FlowCanvas>().DeleteSelection();
                GraphWindow.active.reference.graph.Canvas<FlowCanvas>().selection.Add(superUnit);
            }
        }

        private static void ConvertToMacro()
        {
            var selection = GraphWindow.active?.reference?.graph?.Canvas().selection;

            if (selection != null && selection.Count > 0)
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Selection to Script Graph Asset", "", "asset", "");
                if (!string.IsNullOrEmpty(path))
                {
                    ConvertToEmbed();
                    var superUnit = selection.ToList()[0] as NestedNode;
                    var asset = ScriptGraphAsset.CreateInstance<ScriptGraphAsset>();
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    asset.graph = superUnit.nest.embed;
                    superUnit.nest.SwitchToMacro(asset);
                }
            }
        }

        private static void RemoveUnusedDefinitions(FlowGraph superUnitGraph)
        {
            var keyIncrements = new Dictionary<string, int>();

            var controlInputsToRemove = new List<ControlInputDefinition>();

            for (int i = 0; i < superUnitGraph.controlInputDefinitions.Count; i++)
            {
                if (!keyIncrements.ContainsKey(superUnitGraph.controlInputDefinitions[i].key))
                {
                    keyIncrements.Add(superUnitGraph.controlInputDefinitions[i].key, 0);
                }
                else
                {
                    controlInputsToRemove.Add(superUnitGraph.controlInputDefinitions[i]);
                }
            }

            var controlOutputsToRemove = new List<ControlOutputDefinition>();

            for (int i = 0; i < superUnitGraph.controlOutputDefinitions.Count; i++)
            {
                if (!keyIncrements.ContainsKey(superUnitGraph.controlOutputDefinitions[i].key))
                {
                    keyIncrements.Add(superUnitGraph.controlOutputDefinitions[i].key, 0);
                }
                else
                {
                    controlOutputsToRemove.Add(superUnitGraph.controlOutputDefinitions[i]);
                }
            }

            var valueInputsToRemove = new List<ValueInputDefinition>();

            for (int i = 0; i < superUnitGraph.valueInputDefinitions.Count; i++)
            {
                if (!keyIncrements.ContainsKey(superUnitGraph.valueInputDefinitions[i].key))
                {
                    keyIncrements.Add(superUnitGraph.valueInputDefinitions[i].key, 0);
                }
                else
                {
                    valueInputsToRemove.Add(superUnitGraph.valueInputDefinitions[i]);
                }
            }

            var valueOutputsToRemove = new List<ValueOutputDefinition>();

            for (int i = 0; i < superUnitGraph.valueOutputDefinitions.Count; i++)
            {
                if (!keyIncrements.ContainsKey(superUnitGraph.valueOutputDefinitions[i].key))
                {
                    keyIncrements.Add(superUnitGraph.valueOutputDefinitions[i].key, 0);
                }
                else
                {
                    valueOutputsToRemove.Add(superUnitGraph.valueOutputDefinitions[i]);
                }
            }

            for (int i = 0; i < controlInputsToRemove.Count; i++)
            {
                superUnitGraph.controlInputDefinitions.Remove(controlInputsToRemove[i]);
            }
            for (int i = 0; i < controlOutputsToRemove.Count; i++)
            {
                superUnitGraph.controlOutputDefinitions.Remove(controlOutputsToRemove[i]);
            }
            for (int i = 0; i < valueInputsToRemove.Count; i++)
            {
                superUnitGraph.valueInputDefinitions.Remove(valueInputsToRemove[i]);
            }
            for (int i = 0; i < valueOutputsToRemove.Count; i++)
            {
                superUnitGraph.valueOutputDefinitions.Remove(valueOutputsToRemove[i]);
            }
        }

        private static List<ConnectionData> GetControlIndices(GraphSelection selection, NestedNode superUnit)
        {
            List<ConnectionData> controlConnectionData = new List<ConnectionData>();
            var original = GraphWindow.active.reference.graph as FlowGraph;
            var units = selection.Where((element) => { return element.GetType() != typeof(GraphGroup); }).ToListPooled();

            var controlConnections = original.controlConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                selection.Contains(connection.source.unit) &&
                selection.Contains(connection.destination.unit);
            }).ToList();

            var controlConnectionsForInput = original.controlConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                !selection.Contains(connection.source.unit) &&
                selection.Contains(connection.destination.unit);
            }).ToList();

            var controlConnectionsForOutput = original.controlConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                selection.Contains(connection.source.unit) &&
                !selection.Contains(connection.destination.unit);
            }).ToList();

            for (int i = 0; i < controlConnections.Count; i++)
            {
                var connection = controlConnections[i];
                var connections = connection.graph.controlConnections.ToList();
                controlConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.controlOutputs.ToList().IndexOf(connection.source),
                        source = ConnectionDataSource.Node,
                        subgraph = superUnit,
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.controlInputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            for (int i = 0; i < controlConnectionsForInput.Count; i++)
            {
                var connection = controlConnectionsForInput[i];
                var connections = connection.graph.controlConnections.ToList();
                controlConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.controlOutputs.ToList().IndexOf(connection.source),
                        key = controlConnectionsForInput[i].destination.key,
                        source = ConnectionDataSource.GraphInput,
                        subgraph = superUnit,
                        externalPort = controlConnectionsForInput[i].source,
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.controlInputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            for (int i = 0; i < controlConnectionsForOutput.Count; i++)
            {
                var connection = controlConnectionsForOutput[i];
                var connections = connection.graph.controlConnections.ToList();
                controlConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.controlOutputs.ToList().IndexOf(connection.source),
                        key = controlConnectionsForOutput[i].source.key,
                        source = ConnectionDataSource.GraphOutput,
                        subgraph = superUnit,
                        externalPort = controlConnectionsForOutput[i].destination,
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.controlInputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            return controlConnectionData;
        }

        private static List<ConnectionData> GetValueIndices(GraphSelection selection, NestedNode superUnit)
        {
            List<ConnectionData> valueConnectionData = new List<ConnectionData>();
            var original = GraphWindow.active.reference.graph as FlowGraph;
            var units = selection.Where((element) => { return element.GetType() != typeof(GraphGroup); }).ToListPooled();

            var valueConnections = original.valueConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                selection.Contains(connection.source.unit) &&
                selection.Contains(connection.destination.unit);
            }).ToList();

            var valueConnectionsForInput = original.valueConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                !selection.Contains(connection.source.unit) &&
                selection.Contains(connection.destination.unit);
            }).ToList();

            var valueConnectionsForOutput = original.valueConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                selection.Contains(connection.source.unit) &&
                !selection.Contains(connection.destination.unit);
            }).ToList();

            for (int i = 0; i < valueConnections.Count; i++)
            {
                var connection = valueConnections[i];
                var connections = connection.graph.valueConnections.ToList();
                valueConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.valueOutputs.ToList().IndexOf(connection.source),
                        source = ConnectionDataSource.Node,
                        subgraph = superUnit,
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.valueInputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            for (int i = 0; i < valueConnectionsForInput.Count; i++)
            {
                var connection = valueConnectionsForInput[i];
                var connections = connection.graph.valueConnections.ToList();
                valueConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.valueOutputs.ToList().IndexOf(connection.source),
                        key = valueConnectionsForInput[i].destination.key,
                        valueType = valueConnectionsForInput[i].destination.type,
                        source = ConnectionDataSource.GraphInput,
                        subgraph = superUnit,
                        externalPort = valueConnectionsForInput[i].source,
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.valueInputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            for (int i = 0; i < valueConnectionsForOutput.Count; i++)
            {
                var connection = valueConnectionsForOutput[i];
                var connections = connection.graph.valueConnections.ToList();
                valueConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.valueOutputs.ToList().IndexOf(connection.source),
                        key = valueConnectionsForOutput[i].source.key,
                        valueType = valueConnectionsForOutput[i].source.type,
                        source = ConnectionDataSource.GraphOutput,
                        subgraph = superUnit,
                        externalPort = valueConnectionsForOutput[i].destination,
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.valueInputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            return valueConnectionData;
        }

        private static List<ConnectionData> GetInvalidIndices(GraphSelection selection, NestedNode superUnit)
        {
            List<ConnectionData> invalidConnectionData = new List<ConnectionData>();
            var original = GraphWindow.active.reference.graph as FlowGraph;
            var units = selection.ToListPooled();

            var invalidConnections = original.invalidConnections.Where((connection) =>
            {
                return connection.sourceExists &&
                connection.destinationExists &&
                selection.Contains(connection.source.unit) &&
                selection.Contains(connection.destination.unit);
            }).ToList();

            for (int i = 0; i < invalidConnections.Count; i++)
            {
                var connection = invalidConnections[i];
                var connections = connection.graph.invalidConnections.ToList();
                invalidConnectionData.Add(
                    new ConnectionData()
                    {
                        sourceUnitIndex = units.ToList().IndexOf(connection.source.unit),
                        sourceOutputIndex = connection.source.unit.outputs.ToList().IndexOf(connection.source),
                        destinationUnitIndex = units.ToList().IndexOf(connection.destination.unit),
                        destinationInputIndex = connection.destination.unit.inputs.ToList().IndexOf(connection.destination)
                    }
                );
            }

            return invalidConnectionData;
        }

        // Copies all elements that are selected as new clones of the units. A unit cannot occupy two graphs at once. 
        // In order to gather all necessary data, we must copy and assign it to the new graph via clones before we delete the pre copied units from the original graph.
        private static void CopyElementsToGraph(FlowCanvas superUnitCanvas)
        {
            var selection = GraphWindow.active.reference.graph.Canvas<FlowCanvas>().selection;
            var elements = selection.ToList();

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].GetType() == typeof(GraphGroup))
                {
                    var copy = (GraphGroup)elements[i].CloneViaFakeSerialization();
                    copy.position = ((GraphGroup)elements[i]).position;
                    superUnitCanvas?.graph.groups.Add(copy);
                    continue;
                }
                superUnitCanvas?.AddUnit(((IUnit)elements[i]).CloneViaFakeSerialization(), ((Unit)elements[i]).position);
            }
        }

        private static void CopyControlConnectionData(FlowGraph superUnitGraph, GraphInput graphInput, GraphOutput graphOutput, List<ConnectionData> connectionData)
        {
            List<string> keys = new List<string>();

            for (int i = 0; i < connectionData.Count; i++)
            {
                var index = i;

                var _key = GetKeyName(connectionData[i], keys);

                if (!keys.Contains(_key)) keys.Add(_key);

                switch (connectionData[i].source)
                {
                    case ConnectionDataSource.GraphInput:
                        
                        superUnitGraph.controlInputDefinitions.Add(new ControlInputDefinition()
                        {
                            key = _key
                        });
                        superUnitGraph.PortDefinitionsChanged();
                        graphInput.controlOutputs.Single((op) => { return op.key == connectionData[index].key; }).ConnectToValid(superUnitGraph.units[connectionData[index].destinationUnitIndex].controlInputs.ToList()[connectionData[index].destinationInputIndex] as ControlInput);
                        connectionData[index].subgraph.controlInputs.Single((op) => { return op.key == connectionData[index].key; }).ConnectToValid(connectionData[index].externalPort as ControlOutput);
                        break;

                    case ConnectionDataSource.GraphOutput:
                        superUnitGraph.controlOutputDefinitions.Add(new ControlOutputDefinition()
                        {
                            key = _key
                        });
                        superUnitGraph.PortDefinitionsChanged();
                        graphOutput.controlInputs.Single((op) => { return op.key == _key; }).ConnectToValid(superUnitGraph.units[connectionData[index].sourceUnitIndex].controlOutputs.ToList()[connectionData[index].sourceOutputIndex] as ControlOutput);
                        connectionData[index].subgraph.controlOutputs.Single((op) => { return op.key == _key; }).ConnectToValid(connectionData[index].externalPort as ControlInput);
                        break;

                    case ConnectionDataSource.Node:
                        superUnitGraph.controlConnections.Add(new ControlConnection(superUnitGraph.units[connectionData[index].sourceUnitIndex].controlOutputs.ToList()[connectionData[index].sourceOutputIndex] as ControlOutput,
                        superUnitGraph.units[connectionData[index].destinationUnitIndex].controlInputs.ToList()[connectionData[index].destinationInputIndex] as ControlInput));
                        break;
                }
            }
        }

        private static void CopyValueConnectionData(FlowGraph superUnitGraph, GraphInput graphInput, GraphOutput graphOutput, List<ConnectionData> connectionData)
        {
            List<string> keys = new List<string>();

            for (int i = 0; i < connectionData.Count; i++)
            {
                var index = i;

                var _key = GetKeyName(connectionData[i], keys);

                if (!keys.Contains(_key)) keys.Add(_key);

                switch (connectionData[index].source)
                {
                    case ConnectionDataSource.GraphInput:
                        var def = connectionData[index].valueType.Default();
                        var isDefault = def != null;
                        superUnitGraph.valueInputDefinitions.Add(new ValueInputDefinition()
                        {
                            key = _key,
                            type = connectionData[index].valueType
                        });
                        superUnitGraph.PortDefinitionsChanged();
                        graphInput.valueOutputs.Single((op) => { return op.key == _key; }).ConnectToValid(superUnitGraph.units[connectionData[index].destinationUnitIndex].valueInputs.ToList()[connectionData[index].destinationInputIndex] as ValueInput);
                        connectionData[index].subgraph.valueInputs.Single((op) => { return op.key == _key; }).ConnectToValid(connectionData[index].externalPort as ValueOutput);
                        break;

                    case ConnectionDataSource.GraphOutput:
                        superUnitGraph.valueOutputDefinitions.Add(new ValueOutputDefinition()
                        {
                            key = _key,
                            type = connectionData[index].valueType,
                        });
                        superUnitGraph.PortDefinitionsChanged();
                        graphOutput.valueInputs.Single((op) => { return op.key == _key; }).ConnectToValid(superUnitGraph.units[connectionData[index].sourceUnitIndex].valueOutputs.ToList()[connectionData[index].sourceOutputIndex] as ValueOutput);
                        connectionData[index].subgraph.valueOutputs.Single((op) => { return op.key == _key; }).ConnectToValid(connectionData[index].externalPort as ValueInput);
                        break;

                    case ConnectionDataSource.Node:
                        superUnitGraph.valueConnections.Add(new ValueConnection(superUnitGraph.units[connectionData[index].sourceUnitIndex].valueOutputs.ToList()[connectionData[index].sourceOutputIndex] as ValueOutput,
                        superUnitGraph.units[connectionData[index].destinationUnitIndex].valueInputs.ToList()[connectionData[index].destinationInputIndex] as ValueInput));
                        break;
                }
            }
        }

        private static string GetKeyName(ConnectionData data, List<string> keys)
        {
            if (!keys.Contains(data.key) || data.externalPort.GetType() == typeof(ControlOutput))
            {
                return data.key;
            }
            else
            {
                var count = 1;
                while (keys.Contains(data.key + count.ToString()))
                {
                    count++;
                }
                return data.key + count.ToString();
            }
        }

        private static void CopyInvalidConnectionData(FlowGraph superUnitGraph, GraphInput graphInput, GraphOutput graphOutput, List<ConnectionData> connectionData)
        {
            for (int i = 0; i < connectionData.Count; i++)
            {
                superUnitGraph.invalidConnections.Add(new InvalidConnection(superUnitGraph.units[connectionData[i].sourceUnitIndex].invalidOutputs.ToList()[connectionData[i].sourceOutputIndex] as IUnitOutputPort,
                superUnitGraph.units[connectionData[i].destinationUnitIndex].invalidInputs.ToList()[connectionData[i].destinationInputIndex] as IUnitInputPort));
            }
        }

        private static Vector2 GetNestedNodePosition(List<IGraphElement> elements)
        {
            var superUnitPosition = Vector2.zero;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].GetType() == typeof(GraphGroup))
                {
                    superUnitPosition = new Vector2(superUnitPosition.x + ((GraphGroup)elements[i]).position.x, superUnitPosition.y + ((GraphGroup)elements[i]).position.y);
                    continue;
                }

                superUnitPosition = new Vector2(superUnitPosition.x + ((IUnit)elements[i]).position.x, superUnitPosition.y + ((IUnit)elements[i]).position.y);
            }

            return new Vector2(superUnitPosition.x / elements.Count, superUnitPosition.y / elements.Count);
        }

        private static void SetInputOutputPosition(FlowGraph graph, GraphInput input, GraphOutput output)
        {
            var posXIn = 0f;
            var posYIn = 0f;
            var posXOut = 0f;
            var posYOut = 0f;

            for (int i = 0; i < graph.units.Count; i++)
            {
                if (posXIn >= graph.units[i].position.x)
                {
                    posXIn = graph.units[i].position.x;
                    posYIn = graph.units[i].position.y;
                }

                if (graph.units[i].position.x <= posXOut)
                {
                    posXOut = graph.units[i].position.x;
                    posYOut = graph.units[i].position.y;
                }
            }

            input.position = new Vector2(posXIn - 240, posYIn);
            output.position = new Vector2(posXOut + 240, posYOut);
        }

        public class ConnectionData
        {
            public int sourceUnitIndex;
            public int sourceOutputIndex;
            public int destinationUnitIndex;
            public int destinationInputIndex;
            public IUnitPort externalPort;
            public NestedNode subgraph;
            public Type valueType;
            public string key;
            public ConnectionDataSource source;
        }

        public enum ConnectionDataSource
        {
            GraphInput,
            GraphOutput,
            Node
        }

        private enum PortType
        {
            ValueInput,
            ValueOutput,
            ControlInput,
            ControlOutput
        }
    }
}