using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace AddInPageMate
{

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
            string[] planeBase;
            List<LocationComponent> listLocComp = new List<LocationComponent>();
            LocationAngleComp angleComp = new LocationAngleComp();

            foreach (Component2 item in childrens)
            {
                Component2 compChild = item;
                string [] planeComp=GetPlanesComp(item);
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
                    planeBase = GetPlanesComp(baseComp);
                }
                else
                {
                    swTrCommand = swTrChild;
                    bChild = nameAssemble;
                    planeBase=new string[3];
                    planeBase[0] = "Right";
                    planeBase[1] = "Top";
                    planeBase[2] = "Front";
                }

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
    
                for (int i = 0; i <3; i++)
                {
                    LocationComponent l = new LocationComponent();
                    l.baseComp = bChild;
                    l.childComp = nChild;
                    l.PlanBaseComp = planeBase[i];
                    l = IsFlipedAndAlign(l, listVecor[i], coord[i], planeComp, angleComp);
                    l.dist = Math.Abs(Math.Round(coord[i], 3));
                    listLocComp.Add(l);
                }
                DeletingMateComp(compChild);
                AddMate(listLocComp);
                listLocComp.Clear();
            }
          
            return true;
        }
        private static string[] GetPlanesComp(Component2 comp)
        {
            Feature swFeat = comp.FirstFeature();
            string[] planesBase=new string[3];
            int i=2; 
            while (swFeat != null)
            {
                if ("RefPlane" == swFeat.GetTypeName())
                {
                    planesBase[i]=swFeat.Name;
                    i--;                         
                }
                if(i<0) break;
                swFeat = swFeat.GetNextFeature() as Feature;
            }

            return planesBase;

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
        private static LocationComponent IsFlipedAndAlign(LocationComponent loc, MathVector vector, double coord, string[]planeComp, LocationAngleComp angleComp)
        {
  
            double[] orientation = (double[])vector.ArrayData;
            if (Math.Abs(Math.Round(coord * 1000)) < 1) coord = 0;

            double temp;
            for (int i = 0; i < 3; i++)
            {
                temp = orientation[i];
               temp= Math.Round(temp);
                switch (temp)
                {
                    case 1:
                        loc.PlanComp = planeComp[i];
                        loc.TypeSelect = "PLANE";
                        if (coord > 0)
                        {
                            loc.Fliped = true;
                        }
                        else
                        {
                            loc.Fliped = false;
                        }
                        loc.Align = swMateAlign_e.swMateAlignALIGNED;

                        break;
                    case -1:
                        loc.PlanComp = planeComp[i];
                        loc.TypeSelect = "PLANE";
                        if (coord > 0)
                        {
                            loc.Fliped = false;
                        }
                        else
                        {
                            loc.Fliped = true;
                        }
                        loc.Align = swMateAlign_e.swMateAlignANTI_ALIGNED;
                        break;
                    case 0:

                        break;
                    case double val when val >= 0.1 && val <= 0.99:
                        loc.PlanComp = "Point1@origin";
                        loc.Align = swMateAlign_e.swMateAlignCLOSEST;
                        loc.TypeSelect = "EXTSKETCHPOINT";
                        if(coord > 0) {
                            loc.Fliped = true;
                        }
                        else if(coord < 0)
                        {
                            loc.Fliped = false;
                        }
                        angleComp.SetNextValue(Math.Round(temp, 3), planeComp[i]);
                        loc.PlanComp = planeComp[i];
                        break;
                    case double val when val >= -0.99 && val <= -0.1:
                        loc.PlanComp = "Point1@origin";
                        loc.Align = swMateAlign_e.swMateAlignCLOSEST;
                        loc.TypeSelect = "EXTSKETCHPOINT";
                        if(coord > 0)
                        {
                            loc.Fliped = false;
                        }
                        else if (coord < 0)
                        {
                            loc.Fliped = true;
                        }                   
                        angleComp.SetNextValue(Math.Round(temp, 3), planeComp[i]);
                        loc.PlanComp = planeComp[i];
                        break;
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
                boolstat = swDocExt.SelectByID2(FirstSelection, orientation.TypeSelect, 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                boolstat = swDocExt.SelectByID2(SecondSelection, orientation.TypeSelect, 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
        
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
        public static void DetermineTransformation(double[,] transformMatrix, LocationAngleComp angleComp)
        {
            bool alignment = Math.Sign(transformMatrix[0, 0]) == Math.Sign(transformMatrix[1, 1]);
            if (alignment)
            {
                angleComp.Align = swMateAlign_e.swMateAlignALIGNED;
            }
            else
            {
                angleComp.Align = swMateAlign_e.swMateAlignANTI_ALIGNED;
            }
            
            double angle = Math.Atan2(transformMatrix[1, 0], transformMatrix[0, 0]) * (180 / Math.PI);
            angleComp.Angle = angle;

           bool direction = (transformMatrix[0, 0] * transformMatrix[1, 1] - transformMatrix[1, 0] * transformMatrix[0, 1] > 0) ?
                true : false;  //"По : Против часовой стрелки"
            angleComp.Fliped = direction;
     
        }

    }
}
