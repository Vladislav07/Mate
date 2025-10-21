using System;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using SolidWorks.Interop.swconst;

namespace MyMacroAddIn
{
    public class UserPMPage
    {
        //Local Objects
        public IPropertyManagerPage2 swPropertyPage = null;
        PMPHandler handler = null;
        ISldWorks iSwApp = null;
        SwAddin userAddin = null;

        #region Property Manager Page Controls
        //Groups
        public IPropertyManagerPageGroup groupSelComp;
        public IPropertyManagerPageGroup groupBase;
        public IPropertyManagerPageGroup groupBasePlane;
        public IPropertyManagerPageGroup groupBaseComp;

        //Controls
        public IPropertyManagerPageSelectionbox selComp;
        public IPropertyManagerPageCheckbox AllComponents;
        public IPropertyManagerPageCheckbox AllStandart;
        public IPropertyManagerPageCheckbox Right;
        public IPropertyManagerPageCheckbox Top;
        public IPropertyManagerPageCheckbox Left;
        public IPropertyManagerPageSelectionbox selBaseComp;
        public IPropertyManagerPageOption GlobalCoordinat;
        public IPropertyManagerPageOption SelectPlane;
        public IPropertyManagerPageOption OthersComponent;

        //Control IDs
        public const int groupSelCompID = 1;
        public const int groupBaseID = 2;
        public const int groupBasePlaneID = 5;
        public const int groupBaseCompID = 6;

        public const int AllComponentsID = 3;
        public const int AllStandartID = 4;

        public const int GlobalCoordinatID = 9;
        public const int SelectPlaneID = 10;
        public const int OthersComponentID = 11;

        public const int RightID = 8;
        public const int TopID = 16;
        public const int LeftID = 32;

        public const int selCompID = 64;
        public const int selBaseCompID = 128;


        #endregion

        public UserPMPage(SwAddin addin)
        {
            userAddin = addin;
            if (userAddin != null)
            {
                iSwApp = (ISldWorks)userAddin.SwApp;
                CreatePropertyManagerPage();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("SwAddin not set.");
            }
        }


        protected void CreatePropertyManagerPage()
        {
            int errors = -1;
            int options = (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton |
                (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton;

            handler = new PMPHandler(userAddin, this);
            swPropertyPage = (IPropertyManagerPage2)iSwApp.CreatePropertyManagerPage("Manager Mate", options, handler, ref errors);
            if (swPropertyPage != null && errors == (int)swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                try
                {
                    AddControls();
                }
                catch (Exception e)
                {
                    iSwApp.SendMsgToUser2(e.Message, 0, 0);
                }
            }
        }


        protected void AddControls()
        {
            short controlType = -1;
            short align = -1;
            int options = -1;
            bool retval;

            //Add Message
            string message = "Select one or more circular edges where the component will be mated. "
                + "Browse to a fastener to insert, then click OK.";
;
            retval = swPropertyPage.SetMessage3(message,
                                            (int)swPropertyManagerPageMessageVisibility.swImportantMessageBox,
                                            (int)swPropertyManagerPageMessageExpanded.swMessageBoxExpand,
                                            "Message");

            options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                     (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;
            groupSelComp = (IPropertyManagerPageGroup)swPropertyPage.AddGroupBox(groupSelCompID, "Add Components", options);

            //SelBoxFirst
            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Selectionbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            selComp = (IPropertyManagerPageSelectionbox)groupSelComp.AddControl(selCompID, controlType, "Select Component",
               align, options, "Select Component");
            if (selComp != null)
            {
                int[] filter = { (int)swSelectType_e.swSelCOMPONENTS };
                selComp.Height = 50;
                selComp.SetSelectionFilters(filter);
            }

            //CheckBox

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            AllComponents = (IPropertyManagerPageCheckbox)groupSelComp.AddControl(AllComponentsID, controlType, "Select All Components",
               align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            AllStandart = (IPropertyManagerPageCheckbox)groupSelComp.AddControl(AllStandartID, controlType, "Select All No Cuby",
             align, options, "Default");



            //Group2
            options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                     (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;
            groupBase = (IPropertyManagerPageGroup)swPropertyPage.AddGroupBox(groupBaseID, "Define the Base", options);

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Option;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            GlobalCoordinat = (IPropertyManagerPageOption)groupBase.AddControl(GlobalCoordinatID, controlType, "GlobalCoordinat",
              align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Option;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            SelectPlane = (IPropertyManagerPageOption)groupBase.AddControl(SelectPlaneID, controlType, "SelectPlane",
              align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Option;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            OthersComponent = (IPropertyManagerPageOption)groupBase.AddControl(OthersComponentID, controlType, "OthersComponent",
              align, options, "Default");

            GlobalCoordinat.Checked = true;

           //pageSelectPlane

           options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                   (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;
            groupBasePlane = (IPropertyManagerPageGroup)swPropertyPage.AddGroupBox(groupBaseID, "SELECT plane", options);

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
             align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            Right = (IPropertyManagerPageCheckbox)groupBasePlane.AddControl(RightID, controlType, "IsRight",
              align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                       (int)swAddControlOptions_e.swControlOptions_Visible;
            Top = (IPropertyManagerPageCheckbox)groupBasePlane.AddControl(TopID, controlType, "IsTop",
               align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            Left = (IPropertyManagerPageCheckbox)groupBasePlane.AddControl(LeftID, controlType, "IsLeft",
            align, options, "Default");

            Right.Checked = true;
            Top.Checked = true;
            Left.Checked = true;

            groupBasePlane.Visible = false;

            //OtherComponent

            options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded | (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;
            groupBaseComp = (IPropertyManagerPageGroup)swPropertyPage.AddGroupBox(groupBaseID, "SELECT plane", options);

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Selectionbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            selBaseComp = (IPropertyManagerPageSelectionbox)groupBaseComp.AddControl(selBaseCompID, controlType, "Select Base Component",
               align, options, "Select Base");
            if (selBaseComp != null)
            {
                selBaseComp.SingleEntityOnly = true;
                int[] filter = { (int)swSelectType_e.swSelCOMPONENTS };

                selBaseComp.SetSelectionFilters(filter);

            }
            groupBaseComp.Visible = false;

        }

        public void Show()
        {
            if (swPropertyPage != null)
            {
                swPropertyPage.Show();
            }
        }
    }
}
