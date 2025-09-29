using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class AssetFieldUnit : AssetMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public AssetFieldUnit()
        {
        }

        public AssetFieldUnit(string _name, FieldDeclaration _field, ActionDirection actionDirection)
        {
            name = _name;
            this.actionDirection = actionDirection;
            field = _field;
            fieldType = field.type;
        }

        public string name;
        public FieldDeclaration field;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        public ActionDirection actionDirection;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput get;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput value;

        [Serialize]
        public Type fieldType;

        protected override void Definition()
        {
            // Insures that the type is correct if the Field Type is changed
            if (field != null)
                field.OnChanged += UpdateFieldType;
            if (fieldType == null) UpdateFieldType();
            switch (actionDirection)
            {
                case ActionDirection.Set:
                    {
                        enter = ControlInput(nameof(enter), (flow) =>
                        {
                            Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                            return exit;
                        });
                        exit = ControlOutput(nameof(exit));
                        Succession(enter, exit);
                        value = ValueInput(fieldType, nameof(value));
                        value.SetDefaultValue(fieldType.PseudoDefault());
                        break;
                    }
                case ActionDirection.Get:
                    get = ValueOutput(fieldType, nameof(get), (flow) =>
                    {
                        Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                        return fieldType.PseudoDefault();
                    });
                    break;
            }
        }
        Recursion recursion;
        private void UpdateFieldType()
        {
            recursion ??= Recursion.New();
            if (recursion?.TryEnter(field) == true)
            {
                fieldType = field.type;
                Define();
            }
            recursion?.Exit(field);
        }
    }
}