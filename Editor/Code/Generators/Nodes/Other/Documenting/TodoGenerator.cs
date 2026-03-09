#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Todo))]
	public sealed class TodoGenerator : NodeGenerator<Todo>
	{
		public TodoGenerator(Todo Unit) : base(Unit)
		{
		}

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.Comment("TODO: " + Unit.CustomMessage, WriteOptions.IndentedNewLineAfter);
			if (Unit.ErrorIfHit)
			{
				writer.WriteIndented("throw ".ControlHighlight());
				writer.New(typeof(NotImplementedException), Unit.CustomMessage);
				writer.WriteEnd(EndWriteOptions.LineEnd);
			}
			GenerateExitControl(Unit.exit, data, writer);
        }
	} 
}