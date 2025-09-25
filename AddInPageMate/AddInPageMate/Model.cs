using CodeStack.SwEx.PMPage.Attributes;
using CodeStack.SwEx.PMPage.Base;
using CodeStack.SwEx.PMPage.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;


namespace AddInPageMate
{


    [PageOptions(swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
         | swPropertyManagerPageOptions_e.swPropertyManagerOptions_HandleKeystrokes)]

    [Message("select components", "Component selection page")]
    [System.ComponentModel.DisplayName("Component selection page")]
   
    public class Model : INotifyPropertyChanged
    {
        [SelectionBox(typeof(ComponentLevelFilter), swSelectType_e.swSelCOMPONENTS)]
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
        [SelectionBox(typeof(ComponentBaseLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("BaseComponent")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
   
        public Component2 baseComp { get; set; }

        [ControlTag(nameof(Right))]
        public bool Right { get; set; }
        [ControlTag(nameof(Top))]
        public bool Top { get; set; }
        [ControlTag(nameof(Left))]
        public bool Left { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
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
            int solveInt = c.Solving;
            if (solveInt == 0 || solveInt == -1)
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
