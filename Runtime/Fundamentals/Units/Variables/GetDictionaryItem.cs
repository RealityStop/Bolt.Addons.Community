using System.Collections;
using Ludiq;
using UnityEngine;
using System;

namespace Bolt.Addons.Community.Fundamentals
{
	/* Implementors Note:
	 * 
	 * We do NOT derive from GetVariableUnit, though we could, which would give us a
	 * lot of the functionality that we want.  However, those units are based on the 
	 * older VariableUnit class, which has been obsoleted.
	 */


	/// <summary>
	/// Gets a dictionary item with the specified key.
	/// </summary>
    [UnitTitle("Get Dictionary Variable")]
	public sealed class GetDictionaryVariableItem : UnifiedVariableUnit	
	{
		/// <summary>
		/// The value to return if the variable is not defined.
		/// </summary>
		[DoNotSerialize]
		public ValueInput fallback { get; private set; }

		/// <summary>
		/// The key to fetch from the dictionary.
		/// </summary>
		[DoNotSerialize]
		[PortLabelHidden]
		public ValueInput key { get; private set; }

		/// <summary>
		/// The key to fetch from the dictionary.
		/// </summary>
		[DoNotSerialize]
		[PortLabelHidden]
		public ValueOutput value { get; private set; } 


		/// <summary>
		/// Whether a fallback value should be provided if the 
		/// variable is not defined.
		/// </summary>
		[Serialize]
		[Inspectable]
		[InspectorLabel("Fallback")]
		public bool specifyFallback { get; set; } = false;



		protected override void Definition()
		{
			base.Definition();


			key = ValueInput<string>(nameof(key), string.Empty);
			value = ValueOutput(nameof(value), Get).PredictableIf(IsDefined);


			if (specifyFallback)
			{
				fallback = ValueInput<object>(nameof(fallback), 0);
			}

			Requirement(name, value);
			Requirement(key, value);
		}


		private bool IsDefined(Flow flow)
		{
			var name = flow.GetValue<string>(this.name);

			return GetDeclarations(flow)?.IsDefined(name) ?? false;
		}


		private VariableDeclarations GetDeclarations(Flow flow)
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

		private object Get(Flow flow)
		{
			var name = flow.GetValue<string>(this.name);

			var variableValue = GetDeclarations(flow).Get(name);
			if (variableValue == null)
			{
				if (specifyFallback)
				{
					return flow.GetValue(fallback);
				}

				throw new ArgumentException("Indicated variable does not exist.");
			}

			var dictionary = variableValue as IDictionary;
			if (dictionary == null)
			{
				if (specifyFallback)
				{
					return flow.GetValue(fallback);
				}

				throw new ArgumentException("Indicated variable is not a dictionary.");
			}
			return dictionary[flow.GetValue(key)];
		}
	}
}
 