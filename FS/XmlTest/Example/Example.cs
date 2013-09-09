using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.AssetSpace
{
    public class AssetData
    {
        public string Name { get; set; }
        public string Id { get; set; }
    };

    public class Compartment : AssetData
    {
        List<Compartment> _compartments = new List<Compartment>();

        public void Add(Compartment child)
        {
            if (null != child)
                _compartments.Add(child);
        }
    }

    public class ExampleStorage : AssetData
    {
        List<Compartment> _compartments = new List<Compartment>();

        public void Add(Compartment child)
        {
            if (null != child)
                _compartments.Add(child);
        }

        public void Add(List<Compartment> children)
        {
            foreach (var child in children)
                Add(child);
        }
    };
}
