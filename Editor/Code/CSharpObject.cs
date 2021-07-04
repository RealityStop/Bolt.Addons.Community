using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/CSharp Object")]
    public class CSharpObject : CodeAsset
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
        public bool fieldsOpened;
    }
}
