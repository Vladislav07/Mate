using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace AddInPageMate
{
    internal struct LocationComponent
    {
        public bool Fliped;
        public swMateAlign_e Align;
        public string PlanComp;
        public string PlanBaseComp;
        public double dist;
        public string baseComp;
        public string childComp;
    }
    public enum PlaneName
    {
        Right = 0,
        Top = 1,
        Front = 2
    }
    internal static class SolidServise
    {
        private static ISldWorks sldWorks;
        private static ModelDoc2 swModel;
        private static MathUtility utility;
        private static IModelDocExtension swDocExt;
        private static AssemblyDoc swAssemblyDoc;
        private static SelectionMgr swSelMgr;
        private static Dictionary<string, RefPlane> planes;
        private static bool boolstat;
        private static object[] Mates = null;
        private static Mate2 swMate;
        private static int mateError;
        private static string nameAssemble;
        private static Component2 swRootComp;
        private static Model model;
        public static void SetSolidServise(ISldWorks _sldWorks)
        {
            sldWorks = _sldWorks;
            swModel = (ModelDoc2)sldWorks.ActiveDoc;
            swAssemblyDoc = (AssemblyDoc)swModel;
            utility=(MathUtility)sldWorks.GetMathUtility();
            nameAssemble= GetNameAssemble(swAssemblyDoc);
            swDocExt = swModel.Extension;
            swSelMgr = (SelectionMgr)swModel.SelectionManager;
            ConfigurationManager swConfMgr;
            Configuration swConf;            
            swConfMgr = (ConfigurationManager)swModel.ConfigurationManager;
            swConf = (Configuration)swConfMgr.ActiveConfiguration;
            swRootComp = (Component2)swConf.GetRootComponent();
        }
        public static bool AddPairingMultyComp(Model model)

        {
            List<Component2> childrens = model.components;
            Component2 baseComp=(Component2)model.baseComp;
            List<LocationComponent> listLocComp = new List<LocationComponent>();
            foreach (Component2 item in childrens)
            {
                Component2 compChild = item;
                MathTransform swTrChild = (MathTransform)compChild.Transform2;
                MathTransform swTrCommand;
                string nChild = compChild.Name2 + "@" + nameAssemble;

                string bChild;

                if (baseComp != null)
                {
                    MathTransform MtrInvPlaneBase = (MathTransform)baseComp.Transform2;
                    MathTransform compInNewSKR = (MathTransform)swTrChild.Multiply(MtrInvPlaneBase.Inverse());
                    bChild = baseComp.Name2 + "@" + nameAssemble;
                    swTrCommand = compInNewSKR;
                }
                else
                {
                    swTrCommand = swTrChild;
                    bChild = nameAssemble;
                }

                PlaneName pn;
                double ScaleOutb = 0;
                Object Xch = null;
                Object Ych = null;
                Object Zch = null;
                Object TrObjOutch = null;
                double ScaleOutch = 0;

                swTrCommand.GetData(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
                MathVector[] listVecor = new MathVector[3];
                listVecor[0] = (MathVector)Xch;
                listVecor[1] = (MathVector)Ych;
                listVecor[2] = (MathVector)Zch;

                double[] coord = (double[])((MathVector)TrObjOutch).ArrayData;
                PlaneName planeBase = PlaneName.Right;

               
                for (int i = 0; i < 3; i++, planeBase++)
                {
                    LocationComponent l = new LocationComponent();
                    l.baseComp = bChild;
                    l.childComp = nChild;
                    l.PlanBaseComp = planeBase.ToString();
                    l = IsFlipedAndAlign(l, listVecor[i], coord[i]);
                    l.dist = Math.Abs(Math.Round(coord[i], 3));
                    listLocComp.Add(l);
                }
                DeletingMateComp(compChild);
                AddMate(listLocComp);
                listLocComp.Clear();
            }
          
            return true;
        }
        private static MathTransform CreateRootMathTr()
        {
            double[] arr=new double[15];

            arr[0] = 1;   arr[1] = 0;  arr[2] = 0;
            arr[3] = 0;   arr[4] = 1;  arr[5]=0;
            arr[6] = 0;   arr[7] = 0;  arr[8] = 1;
            arr[9] = 0;   arr[10] = 0; arr[11] = 0;
            arr[12] = 1;
            arr[13] = 0;  arr[14] = 0; arr[15] = 0;
            MathTransform m = (MathTransform)utility.CreateTransform(arr);
            return m;

        }
        private static string GetName(string str)
        {
            int pos = str.IndexOf("@");
            string temp = str.Substring(0, pos);
            return temp;
        }
        private static LocationComponent IsFlipedAndAlign(LocationComponent loc, MathVector vector, double coord)
        {
            PlaneName plane = PlaneName.Right;
            double[] orientation = (double[])vector.ArrayData;
            if (Math.Abs(Math.Round(coord * 1000)) < 1) coord = 0;

            double temp;
            for (int i = 0; i < 3; i++, plane++)
            {       
                    temp = Math.Round(orientation[i]);

                if (temp == 1)
                { 
                    loc.PlanComp = plane.ToString();
                    if (coord > 0)
                    {
                        loc.Fliped = true;
                    }
                    else
                    {
                        loc.Fliped = false;
                    }
                    loc.Align = swMateAlign_e.swMateAlignALIGNED;
                    return loc;
                }
                else if (temp == -1)
                {
                    loc.PlanComp = plane.ToString();
                    if (coord > 0)
                    {
                        loc.Fliped = false;
                    }
                    else
                    {
                        loc.Fliped = true;
                    }
                    loc.Align = swMateAlign_e.swMateAlignANTI_ALIGNED;
                    return loc;
                }
                else
                {
                    continue;
                }

            }
            return loc;

        }
        private static void DeletingMateComp(Component2 swComp)
        {
            Mates = (Object[])swComp.GetMates();
            if ((Mates != null))
            {
                Feature f;
                string nameMate;
                foreach (Object SingleMate in Mates)
                {
                    if (SingleMate is Mate2)
                    {
                        swMate = (Mate2)SingleMate;
                        f = (Feature)swMate;
                        nameMate = f.Name;

                        swModel.ClearSelection2(true);
                        boolstat = swDocExt.SelectByID2(nameMate, "MATE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                        swModel.EditSuppress2();
                    }
                }
            }
        }
        private static void AddMate(List<LocationComponent> orientation)
        {
            PlaneName pnMate = PlaneName.Right;
            foreach (LocationComponent compLocal in orientation)
            {
                AddMateToAssemble(compLocal, pnMate.ToString());
                pnMate++;
            }
        }
        private static void AddMateToAssemble(LocationComponent orientation, string planeCoord)
        {
            string planePart = orientation.PlanComp;
            bool flipped = orientation.Fliped;
            swMateAlign_e align = orientation.Align;
            double distance = orientation.dist;
            string FirstSelection;
            string SecondSelection;
            string MateName;
            Feature matefeature;

            FirstSelection = planePart + "@" + orientation.childComp;  // +"@" + AssemblyName;
            SecondSelection = planeCoord + "@" + orientation.baseComp;
            MateName = planePart;
            swModel.ClearSelection2(true);
            try
            {
                boolstat = swDocExt.SelectByID2(FirstSelection, "PLANE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                boolstat = swDocExt.SelectByID2(SecondSelection, "PLANE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
        
                matefeature = (Feature)swAssemblyDoc.AddMate3((int)swMateType_e.swMateDISTANCE, (int)align, flipped, distance, distance, distance, 0, 0, 0, 0, 0, false, out mateError);
                if(matefeature == null)
                {
                    Console.WriteLine(mateError.ToString());
                }
                matefeature.Name = MateName;
                swAssemblyDoc.EditRebuild();
            }
         
            catch (Exception ex)
            {
                Console.WriteLine(mateError.ToString() );  
                Console.WriteLine(ex.Message);
            }
      

        }
        private static string GetNameAssemble(AssemblyDoc swAssemblyDoc)
        {
            
            string AssemblyTitle = swModel.GetTitle();
            return AssemblyTitle;      
        }

    }
}
