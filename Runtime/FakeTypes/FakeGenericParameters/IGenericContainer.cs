using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community 
{
    public interface IGenericContainer
    {
        FakeGenericParameterType GetGeneric(int position);

        List<FakeGenericParameterType> GetFakeTypes();
    } 
}