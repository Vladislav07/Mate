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
using System.Linq;
using System.Resources;


namespace AddInPageMate
{


    [PageOptions(swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
         | swPropertyManagerPageOptions_e.swPropertyManagerOptions_HandleKeystrokes)]

    [Message("select components", "Component selection page")]
    [DisplayName("Component selection page")]
   
    public class Model : INotifyPropertyChanged

    {
        [SelectionBox(1,typeof(ComponentLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("Components")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
        [ControlOptions(height:120)]
        public List<Component2> components { get; set; } = new List<Component2>();

        public enum SelectBase
        {
            None = 0,
            Component = 1,
            Ref = 2,
            Face = 3
        }
        [OptionBox]
  

        [ControlTag(nameof(selectBase))]
        public SelectBase selectBase { get; set; }

        [SelectionBox(2, typeof(ComponentBaseLevelFilter), swSelectType_e.swSelCOMPONENTS)]
        [Description("BaseComponent")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
        [DependentOn(typeof(EnableDepHandler), nameof(selectBase))]
        public Component2 baseComp { get; set; }


        [SelectionBox(3, swSelectType_e.swSelDATUMPLANES)]
        [Description("BaseRefPlane")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
        [DependentOn(typeof(EnableDepHandlerTwo), nameof(selectBase))]
        public RefPlane baseRef { get; set; }

        [SelectionBox(4, swSelectType_e.swSelFACES)]
        [Description("BaseFace")]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFace)]
        [DependentOn(typeof(EnableDepHandlerSecond), nameof(selectBase))]
        public Face2 baseFace { get; set; }


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

    internal class EnableDepHandlerTwo : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int val = (int)parents.Single(m=>m.Id==1).GetValue();

            if (val == 1 || val == 3 || val == 0) {
                control.SetValue(null);
                control.Visible = false;
           
            }
            else if (val == 2)
            {
               //control.Dispose();
                control.SwControl.Tip = "Select Plane";
                control.SetValue("");
           
                control.Visible = true;
            }
        }
    }
    internal class EnableDepHandlerSecond : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int val = (int)parents.Single(m => m.Id == 1).GetValue();
            Face2 f;
            if (val == 1 || val == 2 ||val==0)
            {
                control.Visible = false;
                control.SetValue(null);

            }
            else if (val == 3)
            {
               // f = control.GetValue<Face2>();
                control.SwControl.Tip = "Select Face";
                control.SetValue("");
                control.Visible = true;
              
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



    public class EnableDepHandler : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int val = (int)parents.First().GetValue();

            if (val == 1)
            {
               // control.Dispose();
                control.SetValue("");
                control.Visible = true;
            
            }
            else if (val == 3 || val == 2 || val == 0)
            {
                control.SetValue(null);
                control.Visible = false;
             
            }
        }
    }

}
