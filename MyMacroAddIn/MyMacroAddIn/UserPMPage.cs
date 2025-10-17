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
        IPropertyManagerPageGroup groupSelComp;
        IPropertyManagerPageGroup groupBase;

        //Controls


        public IPropertyManagerPageSelectionbox selComp;
        public IPropertyManagerPageCheckbox AllComponents;
        public IPropertyManagerPageCheckbox AllStandart;
        public IPropertyManagerPageCheckbox Right;
        public IPropertyManagerPageCheckbox Top;
        public IPropertyManagerPageCheckbox Left;
        public IPropertyManagerPageSelectionbox selBaseComp;

        //Control IDs
        public const int groupSelCompID = 1;
        public const int groupBaseID = 2;

        public const int AllComponentsID = 3;
        public const int AllStandartID = 4;
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

         /*   //Add the groups
            options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded | 
                      (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;

            group2 = (IPropertyManagerPageGroup)swPropertyPage.AddGroupBox(group2ID, "Add Component", options);

    
            //selection1
            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Selectionbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            selection1 = (IPropertyManagerPageSelectionbox)group2.AddControl(selection1ID, controlType, "Select Circular Edges", 
                align, options, "Select circular edges on flat faces where components will be added");
            if (selection1 != null)
            {
                int[] filter = { (int)swSelectType_e.swSelEDGES };
                selection1.Height = 50;
                selection1.SetSelectionFilters(filter);
            }

            // Button
            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Button;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                    (int)swAddControlOptions_e.swControlOptions_Visible;

            button1 = (IPropertyManagerPageButton)group2.AddControl2(buttonID1, controlType, "Browse...", 
                align, options, "Browse to a part");

            //label
            group2.AddControl(100, 1, "File path", 1, 3, "");

            //textbox1
            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Textbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            textbox1 = (IPropertyManagerPageTextbox)group2.AddControl(textbox1ID, controlType, "File path", 
                align, options, "File path");*/

            //MyGroup1
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

            //MyGroup2
            options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                     (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;
            groupBase = (IPropertyManagerPageGroup)swPropertyPage.AddGroupBox(groupBaseID, "Define the Base", options);

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Selectionbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            selBaseComp = (IPropertyManagerPageSelectionbox)groupBase.AddControl(selBaseCompID, controlType, "Select Base Component",
               align, options, "Select Base");
            if (selBaseComp != null)
            {
                selBaseComp.SingleEntityOnly = true;                  
            }

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
             align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;

            Right = (IPropertyManagerPageCheckbox)groupBase.AddControl(RightID, controlType, "IsRight",
              align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                       (int)swAddControlOptions_e.swControlOptions_Visible;
            Top = (IPropertyManagerPageCheckbox)groupBase.AddControl(TopID, controlType, "IsTop",
               align, options, "Default");

            controlType = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            align = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            options = (int)swAddControlOptions_e.swControlOptions_Enabled |
                      (int)swAddControlOptions_e.swControlOptions_Visible;
            Left = (IPropertyManagerPageCheckbox)groupBase.AddControl(LeftID, controlType, "IsLeft",
            align, options, "Default");


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
