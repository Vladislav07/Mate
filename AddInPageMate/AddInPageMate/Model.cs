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

using Xarial.VPages.Framework.Base;
using static AddInPageMate.Model;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;


namespace AddInPageMate
{


    [PageOptions(swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
         | swPropertyManagerPageOptions_e.swPropertyManagerOptions_HandleKeystrokes)]

   // [Message("select components", "Component selection page")]
    [DisplayName("Component selection page")]
   
    public class Model 

    {
        public class GroupComp :INotifyPropertyChanged
        {
            private List<Component2> _list;
            [SelectionBox(1,typeof(ComponentLevelFilter), swSelectType_e.swSelCOMPONENTS)]
            [Description("Components")]
            [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
            [ControlOptions(height:120)]
            public List<Component2> components {  get; set; }=new List<Component2>();
     
            private bool allComp = false;
            private bool allStand = false;

            public event PropertyChangedEventHandler PropertyChanged;

            public bool AllComponentCuby
            {
                get
                {
                    return allComp;
                }
                set
                {
                    allComp = value;
                    if (allComp)
                    {
                        List<Component2> comp = SolidServise.GetAllComponents(true);
                        if(comp != null && comp.Count > 0)
                        {
                          components.AddRange(comp);
                          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(components)));
                        } 
                       
                    }
                    else
                    {
                        SolidServise.ClearArea(components, true);
                       // components.Clear();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(components)));
                    }
                    
                }
            }
            public bool AllElementNoCuby
            {
                get
                {
                    return allStand;
                }
                set
                {
                    allStand = value;
                    if (allStand)
                    {
                        List<Component2> comp = SolidServise.GetAllComponents(false);
                        if (comp != null && comp.Count > 0)
                        {
                            components.AddRange(comp);
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(components)));
                        }
                        else
                        {
                            allStand = false;
                        }
                        
                    }
                    else
                    {

                        SolidServise.ClearArea(components, false);
                       // components.Clear();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(components)));
                    }
                    

                }
            }

        }
     

   /*     public class GroupButtonBack
        {  
            public Action Button => OnButtonClick;
            [IgnoreBinding]
            public Stack<MateFeature> listMate {  get; set; }= new Stack<MateFeature>();
            [IgnoreBinding]
            public Stack<int> ActionCountStorage { get; set; } = new Stack<int>();

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

        }*/

        public enum IsBase_e
            {
                GlobalCoordinate=0,
                IsPlane=1,
                OtherComponent=2
            }


        public class GroupPlane : INotifyPropertyChanged
        {
    
          
            [OptionBox]
            [ControlTag(nameof(Base))]
            public IsBase_e Base {  get; set; }= IsBase_e.GlobalCoordinate;
        
   
            [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
            [ControlTag(nameof(Right))]
            [DependentOn(typeof(EnableRightHandler), nameof(Base))]
            public bool Right { get; set; } = true;

            [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
            [ControlTag(nameof(Top))]
            [DependentOn(typeof(EnableTopHandler), nameof(Base))]
            public bool Top { get; set; } = true;
            [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
            [ControlTag(nameof(Front))]
            [DependentOn(typeof(EnableLeftHandler), nameof(Base))]
            public bool Front { get; set; } = true;

            [SelectionBox(8, typeof(ComponentBaseLevelFilter), swSelectType_e.swSelCOMPONENTS)]
            [Description("BaseComponent")]
            [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectComponent)]
            [ControlTag(nameof(BaseComponent))]
            [DependentOn(typeof(EnableDepHandler), nameof(Base))]
            public Component2 BaseComponent { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
 

          public GroupComp groupComp { get; set; }
    
         public GroupPlane groupPlane { get; set; }


       // public  GroupButtonBack groupButtonBack { get; set; }

    }

    internal class EnableRightHandler : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int v = (int)parents.First().GetValue();
            if (v == 1)
            {
                control.Visible = true;
            }
            else
            {
                control.Visible = false;
            }
        }
    }
    internal class EnableTopHandler : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int v = (int)parents.First().GetValue();
            if (v == 1)
            {
                control.Visible = true;
            }
            else
            {
                control.Visible = false;
            }
        }
    }
    internal class EnableLeftHandler : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int v = (int)parents.First().GetValue();
            if (v == 1)
            {
                control.Visible = true;
            }
            else
            {
                control.Visible = false;
            }
        }
    }
    public class EnableDepHandler : DependencyHandler
    {
        protected override void UpdateControlState(IPropertyManagerPageControlEx control, IPropertyManagerPageControlEx[] parents)
        {
            int v = (int)parents.First().GetValue();
            if(v== 2)
            {
                control.Visible = true;
            }
            else
            {
                control.Visible = false;
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
                c.Select2(false, 8);
                return false;
            }

            itemText = c.Name2;
            return true;
        }

    }
 
}
