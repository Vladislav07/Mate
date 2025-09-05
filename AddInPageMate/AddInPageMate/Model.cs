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
        [SelectionBox(typeof(ComponentLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("Components")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
        [ControlOptions(height:120)]
        public List<Component2> components { get; set; } = new List<Component2>();

        [SelectionBox(typeof(ComponentLevelFilter), swSelectType_e.swSelCOMPONENTS)]
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
        public Action WriteModel => OnWriteClick;

        private void OnWriteClick()
        {
            if (components.Count == 0) return;
            SolidServise.Proccesing(this);
        }

        private void OnBtnClick()
        {
            if (components.Count == 0) return;
           // SolidServise.AddPairingMultyComp(this);
        }
    }
    public class ComponentLevelFilter : SelectionCustomFilter<Component2>
    {
        protected override bool Filter(IPropertyManagerPageControlEx selBox,
            Component2 selection,
            swSelectType_e selType,
            ref string itemText)
        {

            Component2 c = selection;

            while (c.Name2.Contains("/")) {
              c = (Component2)c.GetParent();
            }

            // selBox.SetValue(c);
             
             selection = c;
             itemText = c.Name2;
             return  true;           
        }    
    }
    
}
