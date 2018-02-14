using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Ludiq;
using UnityEngine;

namespace Lasm.BoltAddons.VariableTags
{
    [UnitTitle("Variable Tag")]
    [UnitCategory("Variables/Special")]
    public class VariableTagUnit : Unit
    {
        [SerializeAs("tag")]
        private string _tag;
        [DoNotSerialize]
        public string tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
        
        private VariableTagDatabase tagDatabase;

        protected override void Definition()
        {
            //// Make sure we set a reference to VariableTagDatabase, and if it doesn't exist, we add one to the gameobject.
            //if (tagDatabase == null)
            //{
            //    if (this.owner.GameObject() != null)
            //    {
            //        if (this.owner.GameObject().GetComponent<VariableTagDatabase>() != null)
            //        {
            //            tagDatabase = this.owner.GameObject().GetComponent<VariableTagDatabase>();
            //        }
            //        else
            //        {
            //            tagDatabase = this.owner.GameObject().AddComponent<VariableTagDatabase>();
            //        }
            //    }
            //}
            //else
            //{
            //    if (tag != string.Empty)
            //    {
            //        VariableDeclarations currentTagVariables = tagDatabase.tags[tag];

            //        // Does the tag we are typing in already exist?
            //        if (tagDatabase.TagExists(tag))
            //        {
            //            // Are the this graphs variables and the reference in the database not equal to each other?
            //            if (!Equals(this.graph.variables, currentTagVariables))
            //            {
            //                // Assign the tag in the database with this frames current set of variables;
            //                tagDatabase.tags[tag] = this.graph.variables;
            //            } 

                        
            //        }
            //        else
            //        {
            //                //The tag doesnt exist yet so lets add it
            //                tagDatabase.AddTag(tag, this.graph);
            //        }
            //    }
            //}
        }

       
    }

    

}
