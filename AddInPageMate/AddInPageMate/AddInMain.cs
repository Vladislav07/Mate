
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

namespace AddInPageMate
{
    [ComVisible(true), Guid("64C4DE0F-9B6F-47F1-81D3-6AE8BBF69163")]
    [AutoRegister("MatePMPage", "Create Mate with Property Manager Page")]
 
    public class AddInMain : SwAddInEx
    {
        private PropertyManagerPageEx<MatePmpHandler, Model> pageMate;
        private Model model;

        private enum Commands_e
        {
            ShowPmpPage
        }

        public override bool OnConnect()
        {
            model = new Model();
            pageMate = new PropertyManagerPageEx<MatePmpHandler, Model>(App);
            AddCommandGroup<Commands_e>(ShowPmpPage);
            return true;
        }

        private void ShowPmpPage(Commands_e obj)
        {
            pageMate.Handler.Closed += Handler_Closed;
            pageMate.Handler.Closing += Handler_Closing;
            pageMate.Show(model);
        }

        private void Handler_Closing(swPropertyManagerPageCloseReasons_e reason, CodeStack.SwEx.PMPage.Base.ClosingArg arg)
        {
            ISldWorks sldWorks = (ISldWorks)App;
            SolidServise ss = new SolidServise(sldWorks);
            ss.AddPairingMultyComp(model);
        }

        private void Handler_Closed(swPropertyManagerPageCloseReasons_e reason)
        {
         
            pageMate.Handler.Closing-= Handler_Closing;
            pageMate.Handler.Closed-= Handler_Closed;

        }
    }
}
