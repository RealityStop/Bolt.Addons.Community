using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Bolt.Addons.Community.Code
{
    // No idea if this actually works. It says missing script, but if you replace it with CustomType script, it will have all the data. *Shrug*
    [MovedFrom(false, sourceAssembly:"Bolt.Addons.Community.Editor", sourceNamespace:"Bolt.Addons.Community.Code.Editor", sourceClassName:"CSharpObject")]
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Custom Type")]
    public class CustomType : CodeAsset
    {
        [Inspectable]
        [InspectorWide]
        public ObjectType objectType;
        [Inspectable]
        [InspectorWide]
        public List<FieldDeclaration> fields = new List<FieldDeclaration>();
        [Inspectable]
        public bool inspectable = true;
        [Inspectable]
        public bool serialized = true;
        [Inspectable]
        public bool includeInSettings = true;
        [Inspectable]
        public bool definedEvent;
        [Inspectable]
        public bool scriptableObject;
        [Inspectable]
        public string fileName, menuName;
        [Inspectable]
        public int order;
        public bool fieldsOpened;
    }
}
