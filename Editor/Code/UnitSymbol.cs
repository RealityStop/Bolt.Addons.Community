using System;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Represents a unique symbol or identifier for a visual scripting node
/// </summary>
public class UnitSymbol
{
    public Unit Unit { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the node.
    /// </summary>
    public string NodeId { get; }

    /// <summary>
    /// Gets or sets the type of the value that the node represents.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets additional metadata specific to the generator that may be required.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }

    public UnitSymbol(Unit Unit, Type Type, Dictionary<string, object> Metadata = null)
    {
        this.Unit = Unit;
        NodeId = Unit.GetHashCode().ToString();
        this.Type = Type;
        this.Metadata = Metadata ?? new Dictionary<string, object>();
    }
}
