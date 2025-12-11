using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;


namespace ManagerMatePMP
{
    partial class SolidWorksMacro

    {

        public ModelDoc2 Part;
        public clsPropMgr pm;

        public void Main()

        {

            int openDocErrors = 0;
            int openDocWarnings = 0;
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swStopDebuggingVstaOnExit, false);
            string patn = @"C:\CUBY_PDM\Work\Other\Без проекта\CUBY-V1.1\CAD\Завод контейнер\14 Участок изготовления сэндвич панелей\Кран балка для штрипсы\CUBY-00266373.sldprt";
          //  Part = swApp.OpenDoc6(patn, (int)swDocumentTypes_e.swDocASSEMBLY, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref openDocErrors, ref openDocWarnings);
            //Create a new instance of the PropertyManager class
            pm = new clsPropMgr(swApp);
            pm.Show();

        }

        public SldWorks swApp;

    }
}

