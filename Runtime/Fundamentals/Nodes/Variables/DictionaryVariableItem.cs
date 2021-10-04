using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Assets.BAC.Runtime.Fundamentals.Units.Variables.DictionaryVariableItem")]
    public abstract class DictionaryVariableItem : UnifiedVariableUnit
	{
		[SerializeAs(nameof(keyType))]
		private System.Type _keyType = typeof(string);

		[DoNotSerialize]
		[UnitHeaderInspectable("Key Type")]
		[Inspectable]
		public System.Type keyType
		{
			get
			{
				return _keyType;
			}
			set
			{
				_keyType = value;
			}
		}


		/// <summary>
		/// The key to fetch from the dictionary.
		/// </summary>
		[DoNotSerialize]
		public ValueInput key { get; private set; }



		protected override void Definition()
		{
			base.Definition();


			key = ValueInput(keyType, nameof(key));
			key.SetDefaultValue(GetDefaultValue(keyType));

		}

		object GetDefaultValue(Type t)
		{
			if (t.IsValueType)
				return Activator.CreateInstance(t);

			if (t == typeof(string))
				return "";

			return null;
		}



		protected VariableDeclarations GetDeclarations(Flow flow)
		{
			switch (kind)
			{
				case VariableKind.Flow:
					return flow.variables;
				case VariableKind.Graph:
					return Unity.VisualScripting.Variables.Graph(flow.stack);
				case VariableKind.Object:
					return Unity.VisualScripting.Variables.Object(@flow.GetValue<GameObject>(@object));
				case VariableKind.Scene:
					return Unity.VisualScripting.Variables.Scene(flow.stack.scene);
				case VariableKind.Application:
					return Unity.VisualScripting.Variables.Application;
				case VariableKind.Saved:
					return Unity.VisualScripting.Variables.Saved;
				default:
					throw new UnexpectedEnumValueException<VariableKind>(kind);
			}
		}
	}
}
