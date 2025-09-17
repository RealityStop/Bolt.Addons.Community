#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;

[NodeGenerator(typeof(Todo))]
public class TodoGenerator : NodeGenerator<Todo>
{
	public TodoGenerator(Todo Unit) : base(Unit)
	{

	}

	public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
	{
		return MakeClickableForThisUnit(CodeBuilder.Indent(indent) + CodeBuilder.CommentHighlight("//TODO: " + base.Unit.CustomMessage)) + GetNextUnit(Unit.exit, data, indent);
	}
}

