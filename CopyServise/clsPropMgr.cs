using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace CopyServise
{

    [ComVisibleAttribute(true)]
    public class clsPropMgr : PropertyManagerPage2Handler9
    {
        //General objects required for the PropertyManager page 
        PropertyManagerPage2 pm_Page;
        PropertyManagerPageGroup pm_Group;
        PropertyManagerPageSelectionbox pm_Selection;
        PropertyManagerPageSelectionbox pm_Selection2;
        PropertyManagerPageCombobox planeOnRight_Combo;
        PropertyManagerPageCombobox planeOnLeft_Combo;
        PropertyManagerPageCombobox planeOnUp_Combo;
        PropertyManagerPageListbox pm_List;
        PropertyManagerPageButton pm_Button;
        PropertyManagerPageGroup plane_Group;
        PropertyManagerPageLabel RigntLabel;
        PropertyManagerPageLabel LeftLabel;
        PropertyManagerPageLabel UpLabel;
        PropertyManagerPageCheckbox RigntCheck;
        PropertyManagerPageCheckbox LeftCheck;
        PropertyManagerPageCheckbox UpCheck;
        //Each object in the page needs a unique ID 

        const int GroupID = 1;
        const int SelectionID = 3;
        const int ComboID = 4;
        const int ListID = 5;
        const int Selection2ID = 6;
        const int ButtonID = 7;
        const int planeGroupID = 8;
        const int planeOnRight = 9;
        const int planeOnLeft = 10;
        const int planeOnUp = 11;
        const int RigntLabelID = 12;
        const int LeftLabelID = 13;
        const int UpLabelID = 14;
        const int RigntCheckID = 15;
        const int LeftCheckID = 16;
        const int UpCheckID = 17;

        public event Action<string[]> run;
        public event Action<string> SelectPlane;
        public event Action<string[]> SelectPlaneComp;
        public void Show()
        {

            pm_Page.Show2(0);

        }

        //The following runs when a new instance 

        //of the class is created 

        public clsPropMgr(SldWorks swApp)
        {

            string PageTitle = null;
            string caption = null;
            string tip = null;
            long options = 0;
            int longerrors = 0;
            int controlType = 0;
            int alignment = 0;

            //Set the variables for the page 
            PageTitle = "Comps";
            options = (int)swPropertyManagerButtonTypes_e.swPropertyManager_OkayButton
                + (int)swPropertyManagerButtonTypes_e.swPropertyManager_CancelButton
                + (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_LockedPage
                + (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_PushpinButton;

            //Create the PropertyManager page 
            pm_Page = (PropertyManagerPage2)swApp.CreatePropertyManagerPage(PageTitle,
                (int)options,
                this,
                ref longerrors);

            //Make sure that the page was created properly 
            if (longerrors == (int)swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                //Begin adding the controls to the page 
                //Create the group box 
                caption = "Comps";
                options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible
                    + (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded;
                pm_Group = (PropertyManagerPageGroup)pm_Page.AddGroupBox(GroupID, caption, (int)options);

                //Create two selection boxes 
                controlType = (int)swPropertyManagerPageControlType_e.swControlType_Selectionbox;
                caption = "";
                alignment = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent;
                options = (int)swAddControlOptions_e.swControlOptions_Visible
                    + (int)swAddControlOptions_e.swControlOptions_Enabled;
                tip = "Select a component";
                pm_Selection = (PropertyManagerPageSelectionbox)pm_Group.AddControl(SelectionID, (short)controlType, caption, (short)alignment, (int)options, tip);
                pm_Selection2 = (PropertyManagerPageSelectionbox)pm_Group.AddControl(Selection2ID, (short)controlType, caption, (short)alignment, (int)options, tip);


                swSelectType_e[] filters = new swSelectType_e[1];

                filters[0] = swSelectType_e.swSelCOMPONENTS;


                object filterObj = null;

                filterObj = filters;

                pm_Selection.SingleEntityOnly = false;
                // pm_Selection.AllowMultipleSelectOfSameEntity = true;
                pm_Selection.Height = 50;
                pm_Selection.SetSelectionFilters(filterObj);
                pm_Selection2.SingleEntityOnly = true;
                // pm_Selection2.AllowMultipleSelectOfSameEntity = true;
                pm_Selection2.Height = 15;
                pm_Selection2.SetSelectionFilters(filterObj);

                // Create a Button
                controlType = (int)swPropertyManagerPageControlType_e.swControlType_Button;
                caption = "Run";
                alignment = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_DoubleIndent;
                options = (int)swAddControlOptions_e.swControlOptions_Visible
                    + (int)swAddControlOptions_e.swControlOptions_Enabled;
                tip = "Run Process";
                pm_Button = (PropertyManagerPageButton)pm_Group.AddControl(ButtonID,
                    (short)controlType,
                    caption,
                    (short)alignment,
                    (int)options,
                    tip);
                //Create second group box  
                caption = "Plane";
                options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible
                    + (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded;
                plane_Group = (PropertyManagerPageGroup)pm_Page.AddGroupBox(planeGroupID, caption, (int)options);


                //Option control
                options = (int)swAddControlOptions_e.swControlOptions_Visible
                   + (int)swAddControlOptions_e.swControlOptions_Enabled;
                //Create Label
                caption = "Справа";
                string tipLabel = "Справа";
                // RigntLabel = CreateLabel(plane_Group, RigntLabelID, caption, (int)options, tipLabel);
                RigntCheck = CreateCheckBox(plane_Group, RigntCheckID, caption, (int)options, tipLabel);
                // Create a combo box                            
                tip = "Select a plane from the drop-down";
                planeOnRight_Combo = CreateComBox(plane_Group, planeOnRight, caption, (int)options, tip);

                //Create Label
                caption = "Сверху";
                tipLabel = "Сверху";
                // UpLabel = CreateLabel(plane_Group, UpLabelID, caption, (int)options, tipLabel);
                UpCheck = CreateCheckBox(plane_Group, UpCheckID, caption, (int)options, tipLabel);
                // Create a combo box                            
                tip = "Select a plane from the drop-down";
                planeOnUp_Combo = CreateComBox(plane_Group, planeOnUp, caption, (int)options, tip);

                //Create Label
                caption = "Спереди";
                tipLabel = "Спереди";
                // LeftLabel = CreateLabel(plane_Group, LeftLabelID, caption, (int)options, tipLabel);
                LeftCheck = CreateCheckBox(plane_Group, LeftCheckID, caption, (int)options, tipLabel);
                // Create a combo box                            
                tip = "Select a plane from the drop-down";
                planeOnLeft_Combo = CreateComBox(plane_Group, planeOnLeft, caption, (int)options, tip);
                #region CreateControls
                /*
                controlType = (int)swPropertyManagerPageControlType_e.swControlType_Combobox;
                caption = "my";
                alignment = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent;
                options = (int)swAddControlOptions_e.swControlOptions_Visible
                    + (int)swAddControlOptions_e.swControlOptions_Enabled;

                tip = "Select a value from the drop-down";
                pm_Combo = (PropertyManagerPageCombobox)pm_Group.AddControl(ComboID,
                    (short)controlType, caption, (short)alignment, (int)options, tip);

                if ((pm_Combo != null))
                {
                    pm_Combo.Height = 50;
                    listItems[0] = "Value 1";
                    listItems[1] = "Value 2";
                    listItems[2] = "Value 3";
                    listItems[3] = "Value 4";
                    pm_Combo.AddItems(listItems);
                    pm_Combo.CurrentSelection = 0;
                }

                // Create a list box 
                controlType = (int)swPropertyManagerPageControlType_e.swControlType_Listbox;
                caption = "";
                alignment = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent;
                options = (int)swAddControlOptions_e.swControlOptions_Visible + (int)swAddControlOptions_e.swControlOptions_Enabled;
                tip = "Multi-select values from the list box";
                pm_List = (PropertyManagerPageListbox)pm_Group.AddControl(ListID, (short)controlType, caption, (short)alignment, (int)options, tip);
                if ((pm_List != null))
                {
                    pm_List.Style = (int)swPropMgrPageListBoxStyle_e.swPropMgrPageListBoxStyle_MultipleItemSelect;
                    pm_List.Height = 50;
                    listItems[0] = "Value 1";
                    listItems[1] = "Value 2";
                    listItems[2] = "Value 3";
                    listItems[3] = "Value 4";
                    pm_List.AddItems(listItems);
                    pm_List.SetSelectedItem(1, true);
                }
                 * */
                #endregion
            }
            else
            {
                //If the page is not created 

                System.Windows.Forms.MessageBox.Show("An error occurred while attempting to create the " + "PropertyManager Page");

            }
        }
        PropertyManagerPageLabel CreateLabel(PropertyManagerPageGroup group, int ID, string caption, int options, string tip)
        {
            int controlTypeLabel = (int)swPropertyManagerPageControlType_e.swControlType_Label;
            int alignmentLabel = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent;
            PropertyManagerPageLabel label = (PropertyManagerPageLabel)group.AddControl(ID, (short)controlTypeLabel, caption, (short)alignmentLabel, options, tip);
            label.Height = 10;
            return label;
        }

        PropertyManagerPageCombobox CreateComBox(PropertyManagerPageGroup group, int ID, string caption, int options, string tip)
        {
            int controlTypeBox = (int)swPropertyManagerPageControlType_e.swControlType_Combobox;
            int alignment = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent;
            PropertyManagerPageCombobox comboBox = (PropertyManagerPageCombobox)group.AddControl(ID,
            (short)controlTypeBox, caption, (short)alignment, (int)options, tip);
            comboBox.Height = 50;
            return comboBox;
        }
        PropertyManagerPageCheckbox CreateCheckBox(PropertyManagerPageGroup group, int ID, string caption, int options, string tip)
        {
            int controlTypeCheckBox = (int)swPropertyManagerPageControlType_e.swControlType_Checkbox;
            int alignment = (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_Indent;
            PropertyManagerPageCheckbox checkBox = (PropertyManagerPageCheckbox)group.AddControl(ID,
              (short)controlTypeCheckBox, caption, (short)alignment, (int)options, tip);
            return checkBox;

        }
        public void FillComboBox(string nameBox, List<string> list)
        {

            int count = list.Count;
            string[] listItems = new string[count];
            for (int i = 0; i < count; i++)
            {
                listItems[i] = list[i];
            }

            switch (nameBox)
            {
                case "Справа":
                    planeOnRight_Combo.AddItems(listItems);
                    planeOnRight_Combo.CurrentSelection = 0;
                    break;
                case "Сверху":
                    planeOnUp_Combo.AddItems(listItems);
                    planeOnUp_Combo.CurrentSelection = 0;
                    break;
                case "Спереди":
                    planeOnLeft_Combo.AddItems(listItems);
                    planeOnLeft_Combo.CurrentSelection = 0;
                    break;
                default:
                    break;
            }
        }




        #region IPropertyManagerPage2Handler9 Members

        void IPropertyManagerPage2Handler9.AfterActivation()
        {

           // throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.AfterClose()
        {
            throw new Exception("The method or operation is not implemented.");

        }

        int IPropertyManagerPage2Handler9.OnActiveXControlCreated(int Id, bool Status)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnButtonPress(int Id)
        {
            string[] str = new string[4];
            if (Id == 7)
            {
                str[0] = planeOnRight_Combo.get_ItemText(0);
                str[1] = planeOnUp_Combo.get_ItemText(1);
                str[2] = planeOnLeft_Combo.get_ItemText(0);
                str[3] = pm_Selection2.get_ItemText(0);


                run.Invoke(str);
            };

        }

        void IPropertyManagerPage2Handler9.OnCheckboxCheck(int Id, bool Checked)
        {
            string[] option = new string[4];
            option[0] = (string)pm_Selection2.get_ItemText(0);

            switch (Id)
            {
                case 15:
                    if (Checked == true)
                    {
                        planeOnRight_Combo.Clear();
                        SelectPlane.Invoke("Справа");
                    }
                    else
                    {
                        planeOnRight_Combo.Clear();
                        if (option[0] != "")
                        {
                            option[1] = "Справа";
                            SelectPlaneComp.Invoke(option);
                        }
                    }
                    break;
                case 16:
                    if (Checked == true)
                    {
                        planeOnLeft_Combo.Clear();
                        SelectPlane.Invoke("Спереди");
                    }
                    else
                    {
                        planeOnLeft_Combo.Clear();
                        if (option[0] != "")
                        {
                            option[3] = "Спереди";
                            SelectPlaneComp.Invoke(option);
                        }
                    }
                    break;
                case 17:
                    if (Checked == true)
                    {
                        planeOnUp_Combo.Clear();
                        SelectPlane.Invoke("Сверху");
                    }
                    else
                    {
                        planeOnUp_Combo.Clear();
                        if (option[0] != "")
                        {
                            option[2] = "Сверху";
                            SelectPlaneComp.Invoke(option);
                        }
                    }
                    break;

                default:
                    break;
            }

        }

        void IPropertyManagerPage2Handler9.OnClose(int Reason)
        {

            if (Reason == (int)swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Cancel)
            {

                //Do something when the cancel button is clicked 

            }

            else if (Reason == (int)swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {

                //Do something else when the OK button is clicked 

            }

        }

        void IPropertyManagerPage2Handler9.OnComboboxEditChanged(int Id, string Text)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnComboboxSelectionChanged(int Id, int Item)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnGainedFocus(int Id)
        {

            short[] varArray = null;

            Debug.Print("Control box " + Id + " has gained focus");

            varArray = (short[])pm_List.GetSelectedItems();

            // pm_Combo.CurrentSelection = varArray[0];

        }

        void IPropertyManagerPage2Handler9.OnGroupCheck(int Id, bool Checked)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnGroupExpand(int Id, bool Expanded)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        bool IPropertyManagerPage2Handler9.OnHelp()
        {

            throw new Exception("The method or operation is not implemented.");

        }

        bool IPropertyManagerPage2Handler9.OnKeystroke(int Wparam, int Message, int Lparam, int Id)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnListboxSelectionChanged(int Id, int Item)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnLostFocus(int Id)
        {

            Debug.Print("Control box " + Id + " has lost focus");

        }

        bool IPropertyManagerPage2Handler9.OnNextPage()
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnNumberboxChanged(int Id, double Value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IPropertyManagerPage2Handler9.OnOptionCheck(int Id)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IPropertyManagerPage2Handler9.OnPopupMenuItem(int Id)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IPropertyManagerPage2Handler9.OnPopupMenuItemUpdate(int Id, ref int retval)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        bool IPropertyManagerPage2Handler9.OnPreview()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        bool IPropertyManagerPage2Handler9.OnPreviousPage()
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnRedo()
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnSelectionboxCalloutCreated(int Id)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnSelectionboxCalloutDestroyed(int Id)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnSelectionboxFocusChanged(int Id)
        {

            Debug.Print("The focus has moved to selection box " + Id);

        }

        void IPropertyManagerPage2Handler9.OnSelectionboxListChanged(int Id, int Count)
        {

            if (Id == 6)
            {
                if (Count == 1)
                {
                    string[] option = new string[4];
                    if (!RigntCheck.Checked) { option[1] = "Справа"; };
                    if (!LeftCheck.Checked) { option[3] = "Спереди"; };
                    if (!UpCheck.Checked) { option[2] = "Сверху"; };
                    string o = (string)pm_Selection2.get_ItemText(0);
                    option[0] = o;
                    SelectPlaneComp.Invoke(option);
                }
                else
                {
                    planeOnRight_Combo.Clear();
                    planeOnLeft_Combo.Clear();
                    planeOnUp_Combo.Clear();
                }
            }
        }

        void IPropertyManagerPage2Handler9.OnSliderPositionChanged(int Id, double Value)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnSliderTrackingCompleted(int Id, double Value)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        bool IPropertyManagerPage2Handler9.OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText)
        {

            // This method must return true for selections to occur

            return true;

        }

        bool IPropertyManagerPage2Handler9.OnTabClicked(int Id)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnTextboxChanged(int Id, string Text)
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnUndo()
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnWhatsNew()
        {

            throw new Exception("The method or operation is not implemented.");

        }

        void IPropertyManagerPage2Handler9.OnListboxRMBUp(int Id, int PosX, int PosY)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        int IPropertyManagerPage2Handler9.OnWindowFromHandleControlCreated(int Id, bool Status)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IPropertyManagerPage2Handler9.OnNumberBoxTrackingCompleted(int Id, double Value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

    }

}

