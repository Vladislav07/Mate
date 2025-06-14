using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MateSw
{
    public partial class Form1 : Form
    {

        private SldWorks swApp;
        private ModelDoc2 swMainModel;
        private AssemblyDoc swMainAssy;
        private Configuration swMainConfig;
        private SelectionMgr selMgr;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConnectSW();
        }
        public void ConnectSW()
        {
            string strAttach = swAttach();
            if (strAttach != null)
            {
                DialogResult dr = MessageBox.Show(strAttach,
                "Loading SW",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);

                return;
            }

            string[] strResult = LoadActiveModel();
            swMainAssy.UserSelectionPostNotify += SwMainAssy_UserSelectionPostNotify;
        }

        private int SwMainAssy_UserSelectionPostNotify()
        {
            Component2 childComp;
            int count = selMgr.GetSelectedObjectCount2(-1);
            if (count > 0) {
                childComp = selMgr.GetSelectedObjectsComponent4(1, -1);
                GetComponentPosition(childComp);
            }
           
            return 0;
        }

        string swAttach()
        {

            string strMessage;
            //bool blnStatus = true;

            //Check for the status of existing solidworks apps
            if (System.Diagnostics.Process.GetProcessesByName("sldworks").Length < 1)
            {
                strMessage = "Solidworks instance not detected.";
                //blnStatus = false;
            }
            else if (System.Diagnostics.Process.GetProcessesByName("sldworks").Length > 1)
            {
                strMessage = "Multiple instances of Solidworks detected.";
                //blnStatus = false;
            }
            else
            {
                strMessage = null;
                //swApp = System.Diagnostics.Process.GetProcessesByName("SldWorks.Application");
                swApp = (SldWorks)System.Runtime.InteropServices.Marshal.GetActiveObject("SldWorks.Application");
                //swApp = (SolidWorks.Interop.sldworks.Application)
            }

            return (strMessage);

        }
        string[] LoadActiveModel()
        {

            // returns string array
            //   element 0 = error message
            //   element 1 = model name with path
            //   element 2 = model name
            //   element 3 = referenced configuration name


            ModelDoc2 swDoc;
         
            // DrawingDoc swDrawDoc;
            swDocumentTypes_e swDocType;

            string strModelFile;
            string strModelName;
            //string strFileExt;
            string strConfigName = null;

            string[] strReturn = new string[4];
            strReturn[0] = "";
            int intErrors = 0;
            int intWarnings = 0;

            // Get the active document
            swDoc = (ModelDoc2)swApp.ActiveDoc;
            selMgr=(SelectionMgr)swDoc.ISelectionManager;

            if (swDoc == null)
            {
                strReturn[0] = "Could not acquire an active document";
                return (strReturn);
            }



            //Check for the correct doc type
            strModelFile = swDoc.GetPathName();
            strModelName = strModelFile.Substring(strModelFile.LastIndexOf("\\") + 1, strModelFile.Length - strModelFile.LastIndexOf("\\") - 1);
            swDocType = (swDocumentTypes_e)swDoc.GetType();


            if (swDocType != swDocumentTypes_e.swDocASSEMBLY)
            {
                strReturn[0] = "This program only works with assemblies";
                return (strReturn);
            }

            // Try to load the model file
            try
            {
                swMainModel = swApp.OpenDoc6(strModelFile, (int)swDocType, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, strConfigName, ref intErrors, ref intWarnings);
            }
            catch (Exception e)
            {
                strReturn[0] = e.Message;
                return (strReturn);
            }

            // Write model info to shared variables
            if (strConfigName != null)
            {
                swMainConfig = (Configuration)swMainModel.GetConfigurationByName(strConfigName);
            }
            else
            {
                swMainConfig = (Configuration)swMainModel.GetActiveConfiguration();
            }
            swMainAssy = (AssemblyDoc)swMainModel;
            Component2 swRootComp;
            swRootComp = (Component2)swMainConfig.GetRootComponent();
     

            // Write model info to return array
            strReturn[1] = strModelFile;
            strReturn[2] = strModelName;
            strReturn[3] = strConfigName;
            return (strReturn);

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
   /*      void CreateMateWithGlobalAssembly(Component2 swComponent, double offsetX, double offsetY, double offsetZ, double angleX, double angleY, double angleZ)
        {


            // Создание сопряжения на основе смещения и угла
            Mate2 swMate1 = swMainAssy.CreateMate(swAssembly, swComponent, swReferenceComponent, (int)swMateType_e.swMateCOINCIDENT, (int)swMateAlign_e.swMateAlignALIGNED);
            Mate2 swMate2 = CreateMate(swAssembly, swComponent, swReferenceComponent, (int)swMateType_e.swMateCONCENTRIC, (int)swMateAlign_e.swMateAlignALIGNED);

            // Установка угла для второго сопряжения
            swMate2.SetTorque(angleZ, 0, 0); // Устанавливаем угол только вокруг оси Z

            // Создание объекта MathVector для указания направления силы
            SolidWorksMath swMath = swAssembly.GetMathUtility();
            MathVector swDirection = swMath.CreateVector(offsetX, offsetY, offsetZ);

            // Установка силы с направлением
            swMate2.SetForce(1, swDirection);

            // Применение сопряжений к компонентам
            swAssembly.EditRebuild3();


        }*/
    }
}
