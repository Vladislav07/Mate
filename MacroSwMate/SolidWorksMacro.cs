using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Threading;

namespace MacroSwMate
{
    public partial class SolidWorksMacro
    {
        public TaskpaneView swTaskPane;
        private ModelDoc2 swDoc;
        private AssemblyDoc swMainAssy;
        private Configuration swMainConfig;
        private SelectionMgr selMgr;
        public SldWorks swApp;

        public void Main()
        {
            // Get the active document
            swDoc = (ModelDoc2)swApp.ActiveDoc;
            selMgr = (SelectionMgr)swDoc.ISelectionManager;

            if (swDoc == null)
            {    
               MessageBox.Show( "Could not acquire an active document");
               return;
            }
            swDocumentTypes_e swDocType;
            swDocType = (swDocumentTypes_e)swDoc.GetType();


            if (swDocType != swDocumentTypes_e.swDocASSEMBLY)
            {
                MessageBox.Show("This program only works with assemblies");
                return ;
            }
            swMainAssy = (AssemblyDoc)swDoc;
            swMainAssy.UserSelectionPostNotify += SwMainAssy_UserSelectionPostNotify;
            swMainAssy.NewSelectionNotify += SwMainAssy_NewSelectionNotify;
            swMainAssy.UserSelectionPreNotify += SwMainAssy_UserSelectionPreNotify;
           // Thread.Sleep(10000000);
            // swTaskPane = swApp.CreateTaskpaneView2("", "Mate");
            //  swTaskPane.ShowView();
            //  return;
        }

        private int SwMainAssy_UserSelectionPreNotify(int SelType)
        {
            Console.Write("pppp1");
            return 0;
        }

        private int SwMainAssy_NewSelectionNotify()
        {
            MessageBox.Show("pppp");
            return 0;
        }

        private int SwMainAssy_UserSelectionPostNotify()
        {
            Component2 childComp;
            int count = selMgr.GetSelectedObjectCount2(-1);
            if (count > 0)
            {
                childComp = (Component2) selMgr.GetSelectedObjectsComponent4(1, -1);
                MessageBox.Show(childComp.Name2);
               // GetComponentPosition(childComp);
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

            Console.WriteLine("Смещение: X={0}, Y={1}, Z={2}", offsetX, offsetY, offsetZ);
            Console.WriteLine("Углы: X={0}, Y={1}, Z={2}", angleX, angleY, angleZ);
        }

        // The SldWorks swApp variable is pre-assigned for you.
       

    }
}

