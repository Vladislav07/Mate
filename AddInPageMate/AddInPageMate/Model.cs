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
using CodeStack.SwEx.PMPage.Base;
using System.IO;
using CodeStack.SwEx.PMPage.Controls;
using System.Runtime.CompilerServices;
namespace AddInPageMate
{


    [PageOptions(swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
         | swPropertyManagerPageOptions_e.swPropertyManagerOptions_HandleKeystrokes)]

    [Message("select components", "Component selection page")]
    [System.ComponentModel.DisplayName("Component selection page")]
    public class Model
    {
        [SelectionBox(swSelectType_e.swSelCOMPONENTS)]
        [Description("Components")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
        [ControlOptions(height:120)]
        public List<Component2> components { get; set; } = new List<Component2>();

        [SelectionBox( swSelectType_e.swSelCOMPONENTS)]
        [Description("BaseComponent")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
    
        public IComponent2 baseComp { get; set; }

        [ControlTag(nameof(Right))]
        public bool Right { get; set; }
        [ControlTag(nameof(Top))]
        public bool Top { get; set; }
        [ControlTag(nameof(Left))]
        public bool Left { get; set; }
        public Action CreateMate => OnBtnClick;
        private void OnBtnClick()
        {
            //if (components.Count == 0 || baseComp == null) return;
            SolidServise.AddPairingMultyComp(this);
        }
    }
/*    public class ComponentLevelFilter : SelectionCustomFilter<Component2>
    {
        protected override bool Filter(IPropertyManagerPageControlEx selBox,
            Component2 selection,
            swSelectType_e selType,
            ref string itemText)
        {

            selBox.ValueChanged += SelBox_ValueChanged;
            return  true;
            
        }

        private void SelBox_ValueChanged(Xarial.VPages.Framework.Base.IControl sender, object newValue)
        {

                Component2 c = newValue as Component2;
                if (c.IsRoot())  return;
                c = c.GetParent();
                sender.SetValue(c.GetParent());

        }
    }*/
    
}
