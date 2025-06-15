using System;
using System.Runtime.InteropServices;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MateSwAddIns
{
    [Guid("D3B5A734-F667-42F0-B70E-A0CDCBAB3FD1"), ComVisible(true)]
    [AutoRegister("AddInSwMate", "AddInSwMate2", true)]
   public class swAddInsMate : SwAddInEx
    {
        private ModelDoc2 swDoc;
        private AssemblyDoc swMainAssy;
        private Configuration swMainConfig;
        private SelectionMgr selMgr;
        public SldWorks swApp;
        TaskpaneView taskPaneView;
        PanelTree ctrl;
        public override bool OnConnect()
        {
            
            taskPaneView.AddStandardButton((int)swTaskPaneBitmapsOptions_e.swTaskPaneBitmapsOptions_Ok, "Connect");
            taskPaneView.TaskPaneToolbarButtonClicked += TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.ShowView();
            return base.OnConnect();
        }

        private int TaskPaneView_TaskPaneToolbarButtonClicked(int ButtonIndex)
        {
            swDoc = (ModelDoc2)swApp.ActiveDoc;
            if (swDoc == null)
            {
             
                ctrl.lblMessage.Text="Could not acquire an active document";
                return 1;
            }
            swDocumentTypes_e swDocType;
            swDocType = (swDocumentTypes_e)swDoc.GetType();


            if (swDocType != swDocumentTypes_e.swDocASSEMBLY)
            {
                ctrl.lblMessage.Text = "This program only works with assemblies";
                return 1;
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
