using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;


namespace CopyServise
{
    public partial class SolidWorksMacro
    {
        public ModelDoc2 Part;
        public clsPropMgr pm;
        SolidService service;
        public void Main()
        {
            int openDocErrors = 0;
            int openDocWarnings = 0;
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swStopDebuggingVstaOnExit, false);
            pm = new clsPropMgr(swApp);
            pm.run += new Action<string[]>(pm_run);
            pm.SelectPlane += new Action<string>(pm_SelectPlane);
            pm.SelectPlaneComp += new Action<string[]>(pm_SelectPlaneComp);
            pm.Show();
            service = new SolidService(swApp);



        }

        void pm_SelectPlaneComp(string[] obj)
        {
            string nameComp = obj[0];
            string namePlane;
            List<string> planes;
            for (int i = 1; i < 4; i++)
            {
                switch (i)
                {
                    case 1:
                        if (obj[i] == "") { continue; }
                        namePlane = obj[i];
                        planes = service.listPlaneComp(namePlane, nameComp);
                        pm.FillComboBox("Справа", planes);
                        break;
                    case 2:
                        if (obj[i] == "") { continue; }
                        namePlane = obj[i];
                        planes = service.listPlaneComp(namePlane, nameComp);
                        pm.FillComboBox("Сверху", planes);
                        break;
                    case 3:
                        if (obj[i] == "") { continue; }
                        namePlane = obj[i];
                        planes = service.listPlaneComp(namePlane, nameComp);
                        pm.FillComboBox("Спереди", planes);
                        break;
                    default:
                        break;
                }

            }
        }

        void pm_SelectPlane(string obj)
        {
            List<string> listPlanes = (List<string>)service.listPlane(obj);
            pm.FillComboBox(obj, listPlanes);
        }

        void pm_run(string[] obj)
        {
            // service.AddPairing(obj);
            bool st = service.AddPairingMultyComp(obj);
        }

        public SldWorks swApp;

    }
}

