using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    public interface IRequireVariables
    {
        IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data);
    } 
}
