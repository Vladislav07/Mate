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
            MathTransform MtrInvPlaneBase;
            string nameBase;
            string[] planeBase;

            if (baseComp != null)
            {
                MtrInvPlaneBase = (MathTransform)baseComp.Transform2;
                nameBase = baseComp.Name2 + "@" + nameAssemble;   
                planeBase = GetPlanesComp(baseComp);
            }
            else
            {
                MtrInvPlaneBase = (MathTransform)CreateRootMathTr();
                nameBase = nameAssemble;
                planeBase = new string[3];
                planeBase[0] = "Right";
                planeBase[1] = "Top";
                planeBase[2] = "Front";
            }

            
            List<LocationComponent> listLocComp = new List<LocationComponent>();
           

            foreach (Component2 item in childrens)
            {
                LocationAngleComp angleComp = new LocationAngleComp();
                Component2 compChild = item;
                string [] planeComp=GetPlanesComp(item);
                MathTransform swTrChild = (MathTransform)compChild.Transform2;  
                string nChild = compChild.Name2 + "@" + nameAssemble;         
                MathTransform compInNewSKR = (MathTransform)swTrChild.Multiply(MtrInvPlaneBase.Inverse());

                double[] arrayMatrixComp = (double[])compInNewSKR.ArrayData;
                List<LocationComponent> components;
                bool res = DetermineAngleAndPosition(arrayMatrixComp, out components);
                int u= 0;
                components.ForEach(comp =>
                {
                    comp.baseComp = nameBase;
                    comp.childComp = nChild;
                    comp.PlanBaseComp = planeBase[u];
                    comp.PlanComp = planeBase[u];
                    u++;
                });
                if (res) {
                    DeletingMateComp(compChild);
                    AddMate(components);
                    components=null;
                    return true;
                }
                    

                double ScaleOutb = 0;
                Object Xch = null;
                Object Ych = null;
                Object Zch = null;
                Object TrObjOutch = null;
                double ScaleOutch = 0;

                compInNewSKR.GetData(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
                MathVector[] listVecor = new MathVector[3];
                listVecor[0] = (MathVector)Xch;
                listVecor[1] = (MathVector)Ych;
                listVecor[2] = (MathVector)Zch;
                
                double[] coord = (double[])((MathVector)TrObjOutch).ArrayData;
                
                for (int i = 0; i <3; i++)
                {
                    LocationComponent l = new LocationComponent();
                    l.baseComp = nameBase;
                    l.childComp = nChild;
                    l.PlanBaseComp = planeBase[i];
                    l.TypeSelectBase = "PLANE";
                    l.mateType = swMateType_e.swMateDISTANCE;
                    l = IsFlipedAndAlign(l, listVecor[i], coord[i], planeComp, angleComp, planeBase[i]);
                    l.dist = Math.Abs(Math.Round(coord[i], 3));
                    l.Angle = 0;
                    listLocComp.Add(l);
                }
               
               
                if (angleComp.LT != 0)
                {
                    angleComp.baseComp = nameBase;
                    angleComp.childComp = nChild;
                    angleComp.TypeSelectBase = "PLANE";
                    angleComp.TypeSelect = "PLANE";
                    angleComp.mateType= swMateType_e.swMateANGLE;
                    DetermineTransformation(angleComp.GetMatr(), angleComp);
                    angleComp.dist = 0;
                }
                listLocComp.Add(angleComp);
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
            double[] arr=new double[16];

            arr[0] = 1;   arr[1] = 0;  arr[2] = 0;
            arr[3] = 0;   arr[4] = 1;  arr[5]=0;
            arr[6] = 0;   arr[7] = 0;  arr[8] = 1;
            arr[9] = 0;   arr[10] = 0; arr[11] = 0;
            arr[12] = 1;
            arr[13] = 0;  arr[14] = 0; arr[15] = 0;
            MathTransform m = (MathTransform)utility.CreateTransform(arr);
            return m;

        }
        private static LocationComponent IsFlipedAndAlign(LocationComponent loc, MathVector vector, double coord, string[]planeComp, LocationAngleComp angleComp, string planeBase)
        {
  
            double[] orientation = (double[])vector.ArrayData;

            int y=0;
            double temp;
            for (int i = 0; i < 3; i++)
            {
                temp = orientation[i];
                temp= Math.Round(temp,4);
                switch (temp)
                {
                    case 0:

                        break;
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
                   
                    case double val when val >= 0.1 && val <= 0.99 || val >= -0.99 && val <= -0.1:
                        loc.PlanComp = "Point1@Origin";
                        loc.Align = swMateAlign_e.swAlignAGAINST;
                        loc.TypeSelect = "EXTSKETCHPOINT";
                       
                        if (coord > 0) {
                            loc.Fliped = false;
                        }
                        else if(coord < 0)
                        {
                            loc.Fliped = true;
                        }
                        angleComp.PlanBaseComp = planeBase;
                        angleComp.PlanComp = planeComp[i];
                        angleComp.SetNextValue(Math.Round(temp, 3), planeComp[i]);                   
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
            foreach (LocationComponent compLocal in orientation)
            {
                AddMateToAssemble(compLocal);    
            }
        }
        private static void AddMateToAssemble(LocationComponent orientation)
        {
            if (orientation is LocationAngleComp)
            {
                orientation = orientation as LocationAngleComp;
            }
            string planePart = orientation.PlanComp;
            bool flipped = orientation.Fliped;
            swMateAlign_e align = orientation.Align;
            double distance = orientation.dist;
            double angle = Math.Abs(Math.Round(orientation.Angle, 2));
            string FirstSelection;
            string SecondSelection;
            string MateName;
            Feature matefeature;

            FirstSelection = planePart + "@" + orientation.childComp;  // +"@" + AssemblyName;
            SecondSelection = orientation.PlanBaseComp + "@" + orientation.baseComp;
            MateName = planePart;
            swModel.ClearSelection2(true);
            try
            {
                boolstat = swDocExt.SelectByID2(FirstSelection, orientation.TypeSelect, 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                boolstat = swDocExt.SelectByID2(SecondSelection, orientation.TypeSelectBase, 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
        
                matefeature = (Feature)swAssemblyDoc.AddMate4((int)orientation.mateType, (int)align, flipped, distance, distance, distance, 0, 0, angle, angle, angle, false, false, out mateError);
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

            double angle = Math.Atan2(transformMatrix[1, 0], transformMatrix[0, 0]); // * (180 / Math.PI);
            angleComp.Angle = angle;

           bool direction = (transformMatrix[0, 0] * transformMatrix[1, 1] - transformMatrix[1, 0] * transformMatrix[0, 1] > 0) ?
                true : false;  //"По : Против часовой стрелки"
            angleComp.Fliped = !direction;
     
        }
        public static bool DetermineAngleAndPosition(double[] arrMatrix, out List<LocationComponent> locationComp)
        {
            List<double> list = arrMatrix.Take(9).ToList();
            list.ForEach(n => Math.Round(n, 3));
            locationComp = null;
            if (list.Any(n=>n==0))return false;
            double[,] resultArray = new double[3, 3];
      
            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    resultArray[j, i] = list[index];
                    index++;
                }
            }
            locationComp = CalculateAnglesAndAlignmentWithGlobalPlanes(resultArray);

            return true;
        }
        private static LocationComponent GetLocationAngleComp(double angle, bool flip, bool direction)
        {
            LocationComponent locationComp = new LocationComponent();
            double temp = angle * 180 / Math.PI;
            switch (temp)
            {
                case double val when val > -90 && val < 0:
                    angle = -angle;
                  
                    break;
                case double val when val > -180 && val < -90:
                    angle=Math.PI + angle;
                       flip = !flip;                
                    break;
                case double val when val > 90 && val < 180:
                    angle = Math.PI - angle;
                    
                    break;
                case double val when val < 90 && val > 0:
                    flip = !flip;
                    break;
                default:
                    break;
            }

            locationComp.Fliped = flip;

            locationComp.Align = !direction ? swMateAlign_e.swAlignSAME:swMateAlign_e.swAlignAGAINST;

            locationComp.TypeSelectBase = "PLANE";
            locationComp.TypeSelect = "PLANE";
            locationComp.dist = 0;
            locationComp.mateType = swMateType_e.swMateANGLE;
            locationComp.Angle = angle;

            return locationComp;
        }
        private static List<LocationComponent> CalculateAnglesAndAlignmentWithGlobalPlanes(double[,] localTransformationMatrix)
        {
            List<LocationComponent> components = new List<LocationComponent>();
            // Нормали к плоскостям глобальной системы координат
            double[] globalNormalYZ = { 1, 0, 0 }; // Нормаль к плоскости YZ
            double[] globalNormalXZ = { 0, 1, 0 }; // Нормаль к плоскости XZ
            double[] globalNormalXY = { 0, 0, 1 }; // Нормаль к плоскости XY

            // Нормали плоскостей локального компонента из матрицы преобразования
            double[] localNormalX = { localTransformationMatrix[0, 0], localTransformationMatrix[1, 0], localTransformationMatrix[2, 0] };
            double[] localNormalY = { localTransformationMatrix[0, 1], localTransformationMatrix[1, 1], localTransformationMatrix[2, 1] };
            double[] localNormalZ = { localTransformationMatrix[0, 2], localTransformationMatrix[1, 2], localTransformationMatrix[2, 2] };

            // Вычисляем углы между плоскостями локального компонента и глобальными плоскостями
            double angleToYZ = CalculateAngleBetweenPlanes(localNormalX, globalNormalYZ);
            double angleToXZ = CalculateAngleBetweenPlanes(localNormalY, globalNormalXZ);
            double angleToXY = CalculateAngleBetweenPlanes(localNormalZ, globalNormalXY);

            // Определяем сонаправленность нормалей
            bool alignedWithYZ = AreNormalsAligned(localNormalX, globalNormalYZ);
            bool alignedWithXZ = AreNormalsAligned(localNormalY, globalNormalXZ);
            bool alignedWithXY = AreNormalsAligned(localNormalZ, globalNormalXY);

            bool isFlipedX = CalculateAngleDirectionWithGlobalPlanes(localNormalX, globalNormalYZ);
            bool isFlipedY = CalculateAngleDirectionWithGlobalPlanes(localNormalY, globalNormalXZ);
            bool isFlipedZ = CalculateAngleDirectionWithGlobalPlanes(localNormalZ, globalNormalXY);

            LocationComponent locationCompX = GetLocationAngleComp(angleToYZ, isFlipedX, alignedWithYZ);
            components.Add(locationCompX);               
            LocationComponent locationCompY = GetLocationAngleComp(angleToXZ, isFlipedY, alignedWithXZ);
            components.Add(locationCompY);               
            LocationComponent locationCompZ = GetLocationAngleComp(angleToXY, isFlipedZ, alignedWithXY);
            components.Add(locationCompZ);

            return components;
        }
        private static double CalculateAngleBetweenPlanes(double[] normal1, double[] normal2)
        {
            double dotProduct = normal1[0] * normal2[0] + normal1[1] * normal2[1] + normal1[2] * normal2[2];
            double magnitude1 = Math.Sqrt(normal1[0] * normal1[0] + normal1[1] * normal1[1] + normal1[2] * normal1[2]);
            double magnitude2 = Math.Sqrt(normal2[0] * normal2[0] + normal2[1] * normal2[1] + normal2[2] * normal2[2]);
            double cosAngle = dotProduct / (magnitude1 * magnitude2);

            // Вычисляем угол в радианах
            double angle = Math.Acos(cosAngle);
            // Вычисляем знак угла
            double sign = normal1[0] * normal2[1] - normal1[1] * normal2[0];
            if (sign < 0)
            {
                angle = -angle;
            }

            return angle;
        }
        private static bool AreNormalsAligned(double[] normal1, double[] normal2)
        {
            double dotProduct = normal1[0] * normal2[0] + normal1[1] * normal2[1] + normal1[2] * normal2[2];
            return dotProduct > 0;
        }
        public static bool CalculateAngleDirectionWithGlobalPlanes(double[] localNormal, double[] globalNormal)
        {
            // Векторное произведение нормалей плоскостей
            double[] crossProduct = CrossProduct(localNormal, globalNormal);
            // Определяем направление угла
            return crossProduct[2] > 0 ? true : false;         //"по часовой стрелке" : "против часовой стрелки"; 
        }
        private static double[] CrossProduct(double[] vector1, double[] vector2)
        {
            double x = vector1[1] * vector2[2] - vector1[2] * vector2[1];
            double y = vector1[2] * vector2[0] - vector1[0] * vector2[2];
            double z = vector1[0] * vector2[1] - vector1[1] * vector2[0];
            return new double[] { x, y, z };
        }

    }
}
