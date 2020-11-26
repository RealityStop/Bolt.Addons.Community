using Bolt;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.BAC.Runtime.Fundamentals.Units.Variables
{
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
					return Bolt.Variables.Graph(flow.stack);
				case VariableKind.Object:
					return Bolt.Variables.Object(@flow.GetValue<GameObject>(@object));
				case VariableKind.Scene:
					return Bolt.Variables.Scene(flow.stack.scene);
				case VariableKind.Application:
					return Bolt.Variables.Application;
				case VariableKind.Saved:
					return Bolt.Variables.Saved;
				default:
					throw new UnexpectedEnumValueException<VariableKind>(kind);
			}
		}
	}
}
