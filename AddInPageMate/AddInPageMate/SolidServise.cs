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
        public static MathUtility utility;
        private static IModelDocExtension swDocExt;
        private static AssemblyDoc swAssemblyDoc;
        private static SelectionMgr swSelMgr;
        private static Dictionary<string, RefPlane> planes;
        private static bool boolstat;
        private static int mateError;
        private static string nameAssemble;
        private static Component2 swRootComp;
        private static  Model model;

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

        public static void Proccesing(Model _model)
        {
            model = _model;
            List<ElementSW>listElement = new List<ElementSW>();
            Component2 rootComponent=(Component2)model.baseComp;
            List<Component2> childrens = model.components;
            ElementSW rootElement;
            if (rootComponent != null)
            {
                rootElement = new ElementSW(rootComponent, nameAssemble);
            
            }
            else
            {
                double[] arr = new double[16];

                arr[0] = 1; arr[1] = 0; arr[2] = 0;
                arr[3] = 0; arr[4] = 1; arr[5] = 0;
                arr[6] = 0; arr[7] = 0; arr[8] = 1;
                arr[9] = 0; arr[10] = 0; arr[11] = 0;
                arr[12] = 1;
                arr[13] = 0; arr[14] = 0; arr[15] = 0;
                string  nameBase = nameAssemble;
                string[] planeBase = new string[3];
                planeBase[0] = "Right";
                planeBase[1] = "Top";
                planeBase[2] = "Front";
                rootElement = new ElementSW(nameBase, planeBase,arr);
            }

            foreach (Component2 item in childrens)
            {
                ElementSW x = new ElementSW(item);
                if(x!=null) x.DeletingPairing += (message) => X_DeletingPairing(message);
                listElement.Add(x);
            }

            CreatePairingMultyComp(rootElement, listElement);
            foreach (ElementSW item in listElement)
            {
                AddMate(item.listLocComp);
            }
        }

        private static void X_DeletingPairing(string nameMate)
        {
             swModel.ClearSelection2(true);
             boolstat = swDocExt.SelectByID2(nameMate, "MATE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
             swModel.EditSuppress2();
            swAssemblyDoc.EditRebuild();
            model.listMate.Add(new MateFeature(false, nameMate));

        }

        public static void CreatePairingMultyComp(ElementSW rootElement, List<ElementSW> listElement)
        {
            foreach (ElementSW item in listElement)
            {
                CompLocationCalculation(rootElement, item); 
            }
           
        }

        public static void CompLocationCalculation(ElementSW rootElement, ElementSW childElement)
        {
            List<int> angleIndexMatrix = new List<int>();
            int angleBasePlaneIndex=-1;
            childElement.InitVector(rootElement.compTransform);
            for (int i = 0; i < 3; i++)
            {
                List<int> angleIndex = new List<int>();
                
                bool isAngle=false;
                LocationComponent l = childElement.CreateLocalComponent(0);
                l.baseComp = rootElement.nameSwComponent;
                l.childComp = childElement.nameSwComponent;
                l.PlanBaseComp = rootElement.planes[i];
                l.TypeSelectBase = "PLANE";
                l.mateType = swMateType_e.swMateDISTANCE;
                isAngle = IsFlipedAndAlign(l, childElement.listVector[i], childElement.coord[i], rootElement.planes,
                     angleIndex);
                if (!isAngle)
                {
                    l.dist = Math.Abs(childElement.coord[i]);
                    l.PlanComp = "Point1@Origin";
                    l.Align = swMateAlign_e.swAlignAGAINST;
                    l.TypeSelect = "EXTSKETCHPOINT";

                    if (childElement.coord[i] > 0)
                    {
                        l.Fliped = false;
                    }
                    else if (childElement.coord[i] < 0)
                    {
                        l.Fliped = true;
                    }
                    else
                    {
                        l.mateType = swMateType_e.swMateCOINCIDENT;
                    }              
                  
                    foreach (int item in angleIndex)
                    {
                        int y = i * 3 + item;
                        angleIndexMatrix.Add(y);
                    }
                     angleBasePlaneIndex = i;
                }
                else
                {
                   l.Angle = 0;
                }              
                childElement.listLocComp.Add(l);
            }
            if(angleIndexMatrix.Count == 4 && angleBasePlaneIndex!=-1)
                {
                    childElement.listLocComp.Add(CreateAngleLocation(childElement, angleIndexMatrix, rootElement,  angleBasePlaneIndex));
                }
            if (angleIndexMatrix.Count == 9)
            {
                bool res = CalculateAnglesAndAlignmentWithGlobalPlanes(childElement, rootElement);
            }
        }
        private static LocationAngleComp CreateAngleLocation(ElementSW childElement, List<int> angleIndexMatrix,
            ElementSW rootElement, int angleBasePlaneIndex)
        {
            LocationAngleComp angleComp = (LocationAngleComp)childElement.CreateLocalComponent(1);

            List<double> arrayTransp=new List<double>();
            arrayTransp.Add(childElement.matrixSw[0]);
            arrayTransp.Add(childElement.matrixSw[3]);
            arrayTransp.Add(childElement.matrixSw[6]);
            arrayTransp.Add(childElement.matrixSw[1]);
            arrayTransp.Add(childElement.matrixSw[4]);
            arrayTransp.Add(childElement.matrixSw[7]);
            arrayTransp.Add(childElement.matrixSw[2]);
            arrayTransp.Add(childElement.matrixSw[5]);
            arrayTransp.Add(childElement.matrixSw[8]);

            foreach (int item in angleIndexMatrix) 
            {
                    angleComp.SetNextValue(Math.Round(arrayTransp[item], 5), childElement.planes[item%3]);
            }
            angleComp.baseComp = rootElement.nameSwComponent;
            angleComp.childComp = childElement.nameSwComponent;
            angleComp.TypeSelectBase = "PLANE";
            angleComp.TypeSelect = "PLANE";
            angleComp.mateType = swMateType_e.swMateANGLE;
            DetermineTransformation(angleComp.GetMatr(), angleComp);
            angleComp.dist = 0;
            angleComp.PlanBaseComp = rootElement.planes[angleBasePlaneIndex];
            angleComp.SetPlane();
            return  angleComp;
        }
        private static bool IsFlipedAndAlign(LocationComponent loc, 
            MathVector vector, double coord, string[] planeComp, 
            List<int> angleIndex)
            {

            double[] orientation = (double[])vector.ArrayData;
    
            int y = 0;
            double temp;
            for (int i = 0; i < 3; i++)
            {
                temp = orientation[i];
                temp = Math.Round(temp, 4);
                switch (temp)
                {
                    case 0:

                        break;
                    case 1:
                        loc.PlanComp = planeComp[i];
                        loc.TypeSelect = "PLANE";
                        loc.dist = Math.Abs(coord);
                        if (coord > 0)
                        {
                            loc.Fliped = true;
                        }
                        else
                        {
                            loc.Fliped = false;
                        }
                        loc.Align = swMateAlign_e.swMateAlignALIGNED;
                        return true;
                    case -1:
                        loc.PlanComp = planeComp[i];
                        loc.TypeSelect = "PLANE";
                        loc.dist = Math.Abs(coord);
                        if (coord > 0)
                        {
                            loc.Fliped = false;
                        }
                        else
                        {
                            loc.Fliped = true;
                        }
                        loc.Align = swMateAlign_e.swMateAlignANTI_ALIGNED;
                        return true;

                    case double val when val >= 0.01 && val <= 0.99 || val >= -0.99 && val <= -0.01:
                        int index = i;
                        angleIndex.Add(index);
                        break;
                }
            }
            return false;

        }
 

        private static void AddMate(List<LocationComponent> orientation)
        {   
            foreach (LocationComponent compLocal in orientation)
            {
                AddMateToAssemble(compLocal);    
            }
            swAssemblyDoc.EditRebuild();
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
            double angle = Math.Abs(orientation.Angle);
            string FirstSelection;
            string SecondSelection;
            string MateName;
            Feature matefeature;

            FirstSelection = planePart + "@" + orientation.childComp + "@" +  nameAssemble;
            SecondSelection = orientation.PlanBaseComp + "@" + orientation.baseComp;
            MateName = planePart;
            swModel.ClearSelection2(true);
            try
            {
                boolstat = swDocExt.SelectByID2(FirstSelection, orientation.TypeSelect, 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                boolstat = swDocExt.SelectByID2(SecondSelection, orientation.TypeSelectBase, 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
        
                matefeature = (Feature)swAssemblyDoc.AddMate4((int)orientation.mateType, (int)align, flipped, distance, distance,
                    distance, 0, 0, angle, angle, angle, false, false, out mateError);

                if (matefeature != null)
                {
                    matefeature.Name = MateName;
                    string nameMate=matefeature.Name;
                    model.listMate.Add(new MateFeature(false, nameMate));
                    swAssemblyDoc.EditRebuild();
                }

               if (mateError == 1) {

                    Mate2 mate2 =(Mate2)matefeature.GetSpecificFeature2();
                    int count = mate2.GetMateEntityCount();
                    if (count == 2)
                    {
                        matefeature = null;
                       
                    }
                 
                }
                if(mateError == 5 )
                    {
                        orientation.InvokeOverDefiningAssembly();                
                    }
                
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
        public static double[,] ConvertMatrixToDoubleArray(MathTransform trans)
        {
            double[] arrMatrix = (double[])trans.ArrayData;
            List<double> list = arrMatrix.Take(9).ToList();
       
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
            return resultArray;
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
  
        private static bool CalculateAnglesAndAlignmentWithGlobalPlanes(ElementSW elementSW, ElementSW rootElement)
        {
            double[,] globalTransformationMatrix = ConvertMatrixToDoubleArray(rootElement.compTransform);

            double[] globalNormalYZ = { globalTransformationMatrix[0, 0], globalTransformationMatrix[1, 0], globalTransformationMatrix[2, 0] };
            double[] globalNormalXZ = { globalTransformationMatrix[0, 1], globalTransformationMatrix[1, 1], globalTransformationMatrix[2, 1] };
            double[] globalNormalXY = { globalTransformationMatrix[0, 2], globalTransformationMatrix[1, 2], globalTransformationMatrix[2, 2] };


            double[,] localTransformationMatrix = ConvertMatrixToDoubleArray(elementSW.compTransform);

            double[][] globalNormals = { globalNormalYZ, globalNormalXZ, globalNormalXY };

            double[] localNormalX = { localTransformationMatrix[0, 0], localTransformationMatrix[1, 0], localTransformationMatrix[2, 0] };
            double[] localNormalY = { localTransformationMatrix[0, 1], localTransformationMatrix[1, 1], localTransformationMatrix[2, 1] };
            double[] localNormalZ = { localTransformationMatrix[0, 2], localTransformationMatrix[1, 2], localTransformationMatrix[2, 2] };

            double[][] localNormals = { localNormalX, localNormalY, localNormalZ };

            for (int i = 0; i < 3; i++)
            {
                double angleToPlane = CalculateAngleBetweenPlanes(localNormals[i], globalNormals[i]);
                bool alignedWithPlane = AreNormalsAligned(localNormals[i], globalNormals[i]);
                bool isFliped = CalculateAngleDirectionWithGlobalPlanes(localNormals[i], globalNormals[i]);

                LocationComponent locationComp = GetLocationAngleComp(angleToPlane, isFliped, alignedWithPlane);
                locationComp.baseComp = rootElement.nameSwComponent;
                locationComp.childComp = elementSW.nameSwComponent;
                locationComp.PlanBaseComp = rootElement.planes[i];
                locationComp.PlanComp = elementSW.planes[i];
                elementSW.listLocComp.Add(locationComp);
            }

            return true;
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
