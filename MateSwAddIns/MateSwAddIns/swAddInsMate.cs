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
        //public SldWorks swApp;
        TaskpaneView taskPaneView;
        PanelTree ctrl;
        public override bool OnConnect()
        {
            taskPaneView =(TaskpaneView) CreateTaskPane<PanelTree>(out ctrl);
            taskPaneView.AddStandardButton((int)swTaskPaneBitmapsOptions_e.swTaskPaneBitmapsOptions_Ok, "Connect");
            taskPaneView.TaskPaneToolbarButtonClicked += TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.ShowView();
            return base.OnConnect();
        }

        private int TaskPaneView_TaskPaneToolbarButtonClicked(int ButtonIndex)
        {
            swDoc = (ModelDoc2)App.ActiveDoc;
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
            swMainAssy = (AssemblyDoc)swDoc;
            selMgr = (SelectionMgr)swDoc.ISelectionManager;
            swMainAssy.UserSelectionPostNotify += SwMainAssy_UserSelectionPostNotify;
           
            return 0;


        }

        private int SwMainAssy_UserSelectionPostNotify()
        {
            Component2 childComp;
            int count = selMgr.GetSelectedObjectCount2(-1);
            if (count > 0)
            {
                childComp = (Component2)selMgr.GetSelectedObjectsComponent4(1, -1);
                if (childComp != null)
                {
                    GetComponentPosition(childComp);
                }

            }

            return 0;
        }
        private void GetComponentPosition(Component2 swComponent)
        {
            MathTransform transform = swComponent.Transform2;
            double[] translation = transform.ArrayData as double[];

            double offsetX = translation[9]; // Смещение по оси X
            double offsetY = translation[10]; // Смещение по оси Y
            double offsetZ = translation[11]; // Смещение по оси Z

            double[] rotationMatrix = new double[9];
            Array.Copy(translation, 0, rotationMatrix, 0, 9);

            double angleX = Math.Atan2(rotationMatrix[7], rotationMatrix[8]); // Угол вокруг оси X
            double angleY = Math.Atan2(-rotationMatrix[6], Math.Sqrt(Math.Pow(rotationMatrix[7], 2) + Math.Pow(rotationMatrix[8], 2))); // Угол вокруг оси Y
            double angleZ = Math.Atan2(rotationMatrix[3], rotationMatrix[0]); // Угол вокруг оси Z
            double k = 180 / Math.PI;
            String msg = swComponent.Name2;
             msg = msg + "Смещение: X={0},Y={1}, Z={2},\n" + offsetX*1000 + "\n" + offsetY*1000 + "\n" + offsetZ*1000 + "\n";
            msg=msg + "Углы: X={0}, Y={1}, Z={2}" + "\n" + k*angleX + "\n" + k*angleY + "\n" + k * angleZ;
            ctrl.lblMessage.Text = msg;
        }

        public override bool OnDisconnect()
        {
            taskPaneView.TaskPaneToolbarButtonClicked -= TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.DeleteView();
            return base.OnDisconnect();

        }
       
    }
}
