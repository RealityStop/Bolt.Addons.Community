using System.Collections;
using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
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
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.GetDictionaryVariableItem")]
	public sealed class GetDictionaryVariableItem : DictionaryVariableItem
	{
		/// <summary>
		/// The value to return if the variable is not defined.
		/// </summary>
		[DoNotSerialize]
		public ValueInput fallback { get; private set; }


		/// <summary>
		/// Whether a fallback value should be provided if the 
		/// variable is not defined.
		/// </summary>
		[Serialize]
		[Inspectable]
		[InspectorLabel("Fallback")]
		public bool specifyFallback { get; set; } = false;


		/// <summary>
		/// The value obtained from the dictionary.
		/// </summary>
		[DoNotSerialize]
		[PortLabelHidden]
		public ValueOutput output { get; private set; }


		protected override void Definition()
		{
			base.Definition();

			output = ValueOutput(nameof(output), Get).PredictableIf(IsDefined);


			if (specifyFallback)
			{
				fallback = ValueInput<object>(nameof(fallback), 0);
			}

			Requirement(name, output);
			Requirement(key, output);
			if (specifyFallback)
				Requirement(fallback, output);
		}



		object GetDefaultValue(Type t)
		{
			if (t.IsValueType)
				return Activator.CreateInstance(t);

			if (t == typeof(string))
				return "";

			return null;
		}



		private bool IsDefined(Flow flow)
		{
			var nameValue = flow.GetValue<string>(this.name);
			if (string.IsNullOrEmpty(nameValue))
				return false;
			var declarations = GetDeclarations(flow);
			if (declarations?.IsDefined(nameValue) ?? false)
			{
				var value = declarations?.Get(nameValue);

				return value != null && value is IDictionary;
			}
			return false;
		}



		private object Get(Flow flow)
		{
			var name = flow.GetValue<string>(this.name);
			var keyValue = flow.GetValue<string>(this.key);

			var variableValue = GetDeclarations(flow).Get(name);
			if (variableValue == null)
			{
				if (specifyFallback)
				{
					return flow.GetValue(fallback);
				}

				throw new ArgumentException($"Indicated variable '{name}' does not exist.");
			}

            if (!(variableValue is IDictionary))
            {
                if (specifyFallback)
                {
                    return flow.GetValue(fallback);
                }

                throw new ArgumentException($"Indicated variable '{name}' is not a dictionary.");
            }
			var dictionary = variableValue as IDictionary;
            if (specifyFallback && !dictionary.Contains(keyValue))
				return flow.GetValue(fallback);

			return dictionary[keyValue];
		}
	}
}
