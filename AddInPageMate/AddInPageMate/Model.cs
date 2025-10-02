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
       // private string m_Text1;

    /*    [ControlOptions(backgroundColor: KnownColor.Yellow, textColor: KnownColor.Red)]
        public string Text1
        {
            get
            {
                return m_Text1;
            }
            set
            {
                m_Text1 = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text1)));
            }
        }*/
        [SelectionBox(2, typeof(ComponentBaseLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("BaseComponent")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
   
        public Component2 baseComp { get; set; }

        [IgnoreBinding]
        public List<MateFeature> listMate {  get; set; }= new List<MateFeature>();

        public event PropertyChangedEventHandler PropertyChanged;


        public Action Button => OnButtonClick;

        private void OnButtonClick()
        {
            listMate.ForEach(m => {
                if (m.isBackMate) { }
                else { }
            });   
         
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
              c.Select2(false, -1);            
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
                c.Select2(false, -1);
                return false;
            }

            itemText = c.Name2;
            return true;
        }

    }

}
