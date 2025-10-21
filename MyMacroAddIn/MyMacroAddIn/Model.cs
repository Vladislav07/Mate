using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMacroAddIn
{
    public enum IsBase
    {
        GlobalCoordinat=0,
        SelectPlane=1,
        OthersComponent=2

    }
    public class Model
    {
        public List<Component2> components { get; set; }
        public Component2 baseComp { get; set; }
        public IsBase Base { get; set; }
        public bool[] IsPlane;

        public Model(IsBase b, List<Component2> comps) {
            Base = b;
            components=comps;          
        }
        public Model(IsBase b, List<Component2> comps, Component2 bComp)
        {
            Base = b;
            components = comps;
            baseComp = bComp;
        }

        public Model(IsBase b, List<Component2> comps, bool[] Plane)
        {
            Base = b;
            components = comps;
            IsPlane = Plane;
        }

    }
}
