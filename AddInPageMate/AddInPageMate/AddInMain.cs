
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.PMPage;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using CodeStack.SwEx.PMPage.Controls;
using System.Collections.Generic;

namespace AddInPageMate
{
    [ComVisible(true), Guid("64C4DE0F-9B6F-47F1-81D3-6AE8BBF69163")]
    [AutoRegister("MatePMPage", "Create Mate with Property Manager Page")]
 
    public class AddInMain : SwAddInEx
    {
        private PropertyManagerPageEx<MatePmpHandler, Model> pageMate;
        MatePmpHandler matePmpHandler = new MatePmpHandler();
       

        private Model model;
        ISldWorks sldWorks;
     
        private enum Commands_e
        {
     

            [CommandItemInfo(true, true,swWorkspaceTypes_e.Assembly)]
            ShowPmpPage
        }

        public override bool OnConnect()
        {

            pageMate = new PropertyManagerPageEx<MatePmpHandler, Model>(App);
            AddCommandGroup<Commands_e>(ShowPmpPage);
            AddContextMenu<Commands_e>(ShowPmpPage);
            sldWorks = (ISldWorks)App;
          
            
            return true;
        }

    

        public override bool OnDisconnect()
        {
            return base.OnDisconnect();
        }
        private void ShowPmpPage(Commands_e obj)
        {       
            model = new Model();
             SolidServise.SetSolidServise(sldWorks);
            pageMate.Handler.Closed += Handler_Closed;
            pageMate.Handler.Closing += Handler_Closing;
            pageMate.Handler.DataChanged += Handler_DataChanged;           
            pageMate.Show(model);           
        }


        private void Handler_DataChanged()
        {
             
        }

        private void Handler_Closing(swPropertyManagerPageCloseReasons_e reason, CodeStack.SwEx.PMPage.Base.ClosingArg arg)
        {
           
           

        }

        private void Handler_Closed(swPropertyManagerPageCloseReasons_e reason)
        {
            if (reason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                if (model.groupComp.components.Count == 0) return;
               
                SolidServise.Proccesing(model);

            }
            else
            {
             
              
            }
                pageMate.Handler.Closing -= Handler_Closing;
                pageMate.Handler.Closed-= Handler_Closed;
                pageMate.Handler.DataChanged -= Handler_DataChanged;
                model = null;


        }
    }
}
