using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace AddInsMate_01
{ 
    [Guid("69B630D4-F02C-4FC3-ACC6-BF25024A7636"), ComVisible(true)]
    [AutoRegister("Sample AddInExMate", "AddInExMateHoum", true)]
    public class MainAddIns:SwAddInEx
    {
       
        public override bool OnConnect()
        {
            
            return base.OnConnect();
        }
    }
}
