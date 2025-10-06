using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.PMPage.Attributes;
using CodeStack.SwEx.PMPage.Base;
using CodeStack.SwEx.PMPage.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Resources;


namespace AddInPageMate
{


    [PageOptions(swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
         | swPropertyManagerPageOptions_e.swPropertyManagerOptions_HandleKeystrokes)]

    [Message("select components", "Component selection page")]
    [System.ComponentModel.DisplayName("Component selection page")]
   
    public class Model : INotifyPropertyChanged
    {
        [SelectionBox(1,typeof(ComponentLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("Components")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
        [ControlOptions(height:120)]
        public List<Component2> components { get; set; } = new List<Component2>();

        [SelectionBox(2, typeof(ComponentBaseLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("BaseComponent")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
   
        public Component2 baseComp { get; set; }

        [IgnoreBinding]
        public Stack<MateFeature> listMate {  get; set; }= new Stack<MateFeature>();
        [IgnoreBinding]
        public Stack<int> ActionCountStorage { get; set; } = new Stack<int>();

        public event PropertyChangedEventHandler PropertyChanged;


        public Action Button => OnButtonClick;

        private void OnButtonClick()
        {
            int countItems = ActionCountStorage.Pop();
            for (int i = 0; i < countItems; i++)
            {
                MateFeature m = listMate.Pop();
                if (m.isBackMate) { SolidServise.DeleteMate(m.NameMate); }
                else { SolidServise.DispleyMate(m.NameMate); }
            }                 
        }

    }
    public class ComponentLevelFilter : SelectionCustomFilter<Component2>
    {
        protected override bool Filter(IPropertyManagerPageControlEx selBox, Component2 selection, swSelectType_e selType, ref string itemText)
        {

             Component2 c = selection;
             
           if (c.Name2.Contains("/"))
            {
              c.DeSelect();
              c = (Component2)c.GetParent();
              c.Select2(false, 1);            
                return false;
            }

            itemText = c.Name2;
            return true;
        }

    }
    public class ComponentBaseLevelFilter : SelectionCustomFilter<Component2>
    {
        protected override bool Filter(IPropertyManagerPageControlEx selBox, Component2 selection, swSelectType_e selType, ref string itemText)
        {

            Component2 c = selection;
            int solveInt = c.GetConstrainedStatus();
            if (solveInt != 3 )
            {
                
                return false;
            }

            if (c.Name2.Contains("/"))
            {
                c.DeSelect();
                c = (Component2)c.GetParent();
                c.Select2(false, 2);
                return false;
            }

            itemText = c.Name2;
            return true;
        }

    }

}
