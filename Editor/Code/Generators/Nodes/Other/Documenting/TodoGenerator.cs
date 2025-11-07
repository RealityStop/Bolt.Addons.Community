#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Todo))]
	public sealed class TodoGenerator : NodeGenerator<Todo>
	{
		public TodoGenerator(Todo Unit) : base(Unit)
		{
	
		}
	
		public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
		{
			return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeBuilder.CommentHighlight("//TODO: " + base.Unit.CustomMessage)) + GetNextUnit(Unit.exit, data, indent);
		}
	} 
}