using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.AddIn;
using System.Runtime.InteropServices.ComTypes;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

namespace AddInsMate_01
{ 
    [Guid("69B630D4-F02C-4FC3-ACC6-BF25024A7636"), ComVisible(true)]
    [AutoRegister("Sample AddInExMate", "AddInExMateHoum", true)]
    public class MainAddIns:SwAddInEx
    {

        private ModelDoc2 swDoc;
        private AssemblyDoc swMainAssy;
        private Configuration swMainConfig;
        private SelectionMgr selMgr;
        //public SldWorks swApp;
        TaskpaneView taskPaneView;
        PanelTree ctrl;
        public override bool OnConnect()
        {
            taskPaneView = (TaskpaneView)CreateTaskPane<PanelTree>(out ctrl);
            taskPaneView.AddStandardButton((int)swTaskPaneBitmapsOptions_e.swTaskPaneBitmapsOptions_Ok, "Connect");
            taskPaneView.TaskPaneToolbarButtonClicked += TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.ShowView();
            return base.OnConnect();
        }

        private int TaskPaneView_TaskPaneToolbarButtonClicked(int ButtonIndex)
        {
           
            if (App.ActiveDoc == null)
            {

                ctrl.lblMessage.Text = "Could not acquire an active document";
                return -1;
            }
            swDoc = (ModelDoc2)App.ActiveDoc;
            swDocumentTypes_e swDocType;
            swDocType = (swDocumentTypes_e)swDoc.GetType();


            if (swDocType != swDocumentTypes_e.swDocASSEMBLY)
            {
                ctrl.lblMessage.Text = "This program only works with assemblies";
                return -1;
            }
            return 0;
        }

        public override bool OnDisconnect()
        {
            taskPaneView.TaskPaneToolbarButtonClicked -= TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.DeleteView();
            return base.OnDisconnect();
        }
    }
}
