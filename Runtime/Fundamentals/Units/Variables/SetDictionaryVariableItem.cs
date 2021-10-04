using System.Collections;
using System;

namespace Unity.VisualScripting.Community
{
	/* Implementors Note:
	 * 
	 * We do NOT derive from SetVariableUnit, though we could, which would give us a
	 * lot of the functionality that we want.  However, those units are based on the 
	 * older VariableUnit class, which has been obsoleted.
	 */

	/// <summary>
	/// Sets a dictionary item with the specified key.
	/// </summary>
    [UnitTitle("Set Dictionary Variable")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.SetDictionaryVariableItem")]
	public sealed class SetDictionaryVariableItem : DictionaryVariableItem
	{
		/// <summary>
		/// The entry point to assign the variable reference.
		/// </summary>
		[DoNotSerialize]
		[PortLabelHidden]
		public ControlInput assign { get; set; }

		/// <summary>
		/// The value to assign to the variable.
		/// </summary>
		[DoNotSerialize]
		[PortLabel("New Value")]
		[PortLabelHidden]
		public ValueInput newValue { get; private set; }

		/// <summary>
		/// The action to execute once the variable has been assigned.
		/// </summary>
		[DoNotSerialize]
		[PortLabelHidden]
		public ControlOutput assigned { get; set; }

		/// <summary>
		/// The value obtained from the dictionary.
		/// </summary>
		[DoNotSerialize]
		[PortLabelHidden]
		public ValueOutput output { get; private set; }


		protected override void Definition()
		{
			base.Definition();

			assign = ControlInput(nameof(assign), Assign);

			newValue = ValueInput<object>(nameof(newValue));
			assigned = ControlOutput(nameof(assigned));
			output = ValueOutput<object>(nameof(output));

			Requirement(name, assign);
			Requirement(key, assign);
			Requirement(newValue, assign);
			Assignment(assign, output);
			Succession(assign, assigned);
		}

		private ControlOutput Assign(Flow flow)
		{
			var dictionaryKey = flow.GetValue<object>(key);
			var newValue = flow.GetValue<object>(this.newValue);
			var name = flow.GetValue<string>(this.name);

			var variableValue = GetDeclarations(flow).Get(name);

			if(variableValue == null)
			{
				throw new ArgumentException("Indicated variable does not exist.");
			}

			var dictionary = variableValue as IDictionary;
			if (dictionary == null)
			{
				throw new ArgumentException("Indicated variable is not a dictionary.");
			}


			dictionary[dictionaryKey] = newValue;
			flow.SetValue(output, newValue);

			return assigned;
		}

	}
}