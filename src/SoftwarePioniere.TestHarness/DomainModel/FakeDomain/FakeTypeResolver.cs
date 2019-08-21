using System;
using System.Collections.Generic;
using SoftwarePioniere.Domain;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public class FakeTypeResolver : IResolveType
    {

        public Dictionary<Type, object> Instances { get; set; } = new Dictionary<Type, object>();

        public object Resolve(Type t)
        {
            if (Instances.ContainsKey(t))
                return Instances[t];

            var inst = Activator.CreateInstance(t);
            Instances.Add(t, inst);
            return inst;
        }
    }
}
