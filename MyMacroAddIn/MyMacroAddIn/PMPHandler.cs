using System;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using SolidWorks.Interop.swconst;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MyMacroAddIn
{

public class PMPHandler : IPropertyManagerPage2Handler9
{
    ISldWorks iSwApp;
    SwAddin userAddin;
    UserPMPage ppage;

    string filePath;
    bool cancelled;

    public PMPHandler(SwAddin addin, UserPMPage page)
    {
        userAddin = addin;
        iSwApp = (ISldWorks)userAddin.SwApp;
        ppage = page;
        AddComps.m_swApp = (SldWorks)iSwApp;
    }

    //Implement these methods from the interface
    public void AfterClose()
    {
        if (!cancelled)
        {
            AddComps.AddCompAndMate(filePath);
        }
    }

    public void OnCheckboxCheck(int id, bool status)
    {

    }

    public void OnClose(int reason)
    {
        if (reason == (int)swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
        {
            cancelled = false;
            filePath = ppage.textbox1.Text;
            if (!System.IO.File.Exists(filePath))
            {
                iSwApp.SendMsgToUser2("Browse to a valid part first.",
                    (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
                //prevent the page from closing
                COMException ex = new COMException("cancel close", 1);
                throw ex;
            }                
        }
        else
        {
            cancelled = true;
        }
    }

    public void OnComboboxEditChanged(int id, string text)
    {

    }

    public int OnActiveXControlCreated(int id, bool status)
    {
        return -1;
    }

    public void OnButtonPress(int id)
    {
        if (id == UserPMPage.buttonID1)        // Toggle the textbox control visibility state
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SOLIDWORKS Parts (*.sldprt)|*.sldprt";
            DialogResult diaRes = ofd.ShowDialog();
            if (diaRes == DialogResult.OK)
            {
                ppage.textbox1.Text = ofd.FileName;
            }
        }
    }

        public void OnComboboxSelectionChanged(int id, int item)
        {

        }

        public void OnGroupCheck(int id, bool status)
        {

        }

        public void OnGroupExpand(int id, bool status)
        {

        }

        public bool OnHelp()
        {
            string helppath;
            System.Windows.Forms.Form helpForm = new System.Windows.Forms.Form();

            // Specify a url path or a path to a chm file
            helppath = "http://help.solidworks.com/2021/English/api/sldworksapiprogguide/Welcome.htm";
            //helppath = "C:\\Program Files\\SolidWorks Corp\\SOLIDWORKS\\api\\apihelp.chm";

            System.Windows.Forms.Help.ShowHelp(helpForm, helppath);

            return true;
        }

        public void OnListboxSelectionChanged(int id, int item)
        {

        }

        public bool OnNextPage()
        {
            return true;
        }

        public void OnNumberboxChanged(int id, double val)
        {

        }

        public void OnNumberBoxTrackingCompleted(int id, double val)
        {

        }

        public void OnOptionCheck(int id)
        {

        }

        public bool OnPreviousPage()
        {
            return true;
        }

        public void OnSelectionboxCalloutCreated(int id)
        {

        }

        public void OnSelectionboxCalloutDestroyed(int id)
        {

        }

        public void OnSelectionboxFocusChanged(int id)
        {

        }

        public void OnSelectionboxListChanged(int id, int item)
        {
            // When a user selects entities to populate the selection box, display a popup cursor.
            ppage.swPropertyPage.SetCursor((int)swPropertyManagerPageCursors_e.swPropertyManagerPageCursors_Advance);
        }

        public void OnTextboxChanged(int id, string text)
        {

        }

        public void AfterActivation()
        {

        }

        public bool OnKeystroke(int Wparam, int Message, int Lparam, int Id)
        {
            return true;
        }

        public void OnPopupMenuItem(int Id)
        {

        }

        public void OnPopupMenuItemUpdate(int Id, ref int retval)
        {

        }

        public bool OnPreview()
        {
            return true;
        }

        public void OnSliderPositionChanged(int Id, double Value)
        {

        }

        public void OnSliderTrackingCompleted(int Id, double Value)
        {

        }

        public bool OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText)
        {
            return true;
        }

        public bool OnTabClicked(int Id)
        {
            return true;
        }

        public void OnUndo()
        {

        }

        public void OnWhatsNew()
        {

        }


        public void OnGainedFocus(int Id)
        {

        }

        public void OnListboxRMBUp(int Id, int PosX, int PosY)
        {

        }

        public void OnLostFocus(int Id)
        {

        }

        public void OnRedo()
        {

        }

        public int OnWindowFromHandleControlCreated(int Id, bool Status)
        {
            return 0;
        }


    }
}
