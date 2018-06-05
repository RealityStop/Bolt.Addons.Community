using System;
using System.Collections.Generic;
using System.Collections;
using Bolt;
using Ludiq;
using UnityEngine;
using System.Linq;

namespace Lasm.BoltAddons.VariableTags
{
    [ExecuteInEditMode]
    public class VariableTagDatabase : MonoBehaviour
    {
        // From left to right: {string = Tag Name; {string = Variable Name; object = Variable Value } }
        public IDictionary<string, VariableDeclarations> tags = new Dictionary<string, VariableDeclarations>();

        public void AddTag(string tag, FlowGraph graph)
        {
            VariableDeclarations variableDeclarations = Variables.Graph(graph);
            tags.Add(tag, variableDeclarations);
        }

        public bool TagExists(string tag)
        {
            bool exists = false;

            if (tags.ContainsKey(tag))
            {
                exists = true;
            }

            return exists;
        }
    }
}
