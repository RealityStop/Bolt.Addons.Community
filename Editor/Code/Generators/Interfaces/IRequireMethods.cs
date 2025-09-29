using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    public interface IRequireMethods
    {
        IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data);
    } 
}