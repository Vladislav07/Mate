using CodeStack.SwEx.PMPage.Attributes;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;
using System.Diagnostics;
using CodeStack.SwEx.PMPage;
using CodeStack.SwEx.Common;

namespace MacroTest
{


    [PageOptions(swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
         | swPropertyManagerPageOptions_e.swPropertyManagerOptions_HandleKeystrokes)]

    [Message("select components", "Component selection page")]
    [System.ComponentModel.DisplayName("Component selection page")]
    public class Model
    {
       // [SelectionBox(swSelectType_e.swSelCOMPONENTS)]
      //  [Description("Components")]
      //  [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
      //  public List<Component2> components { get; set; } = new List<Component2>();

        [SelectionBox(swSelectType_e.swSelCOMPONENTS)]
        [Description("BaseComponent")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
        public IComponent2 baseComp { get; set; }

       // [ControlTag(nameof(Right))]
        public bool Right { get; set; }
      //  [ControlTag(nameof(Top))]
        public bool Top { get; set; }
       // [ControlTag(nameof(Left))]
        public bool Left { get; set; }
        /* public Action CreateMate => OnBtnClick;
         private void OnBtnClick()
         {
             if (components.Count == 0 || baseComp == null) return;
             SolidServise solidServise = new SolidServise(this);
         }*/
    }
}
