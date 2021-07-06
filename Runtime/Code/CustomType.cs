using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Bolt.Addons.Community.Code
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Class")]
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
