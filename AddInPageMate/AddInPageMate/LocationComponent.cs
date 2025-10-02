using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddInPageMate
{
    public class LocationComponent
    {
        public bool Fliped;
        public swMateAlign_e Align;
        public swMateType_e mateType;
        public string PlanComp;
        public string PlanBaseComp;
        public double dist;
        public string baseComp;
        public string childComp;
        public string TypeSelect;
        public string TypeSelectBase;
        public double Angle;
        public event Action OverDefiningAssembly;
        public void InvokeOverDefiningAssembly()
        {
            OverDefiningAssembly?.Invoke();
        }

    }
    public class LocationAngleComp: LocationComponent
    {
  
        public double LT;
        public double RT;
        public double LB;
        public double RB;
        public string[] plane;     
       
        private int currentIndex;
        private double[,]matr;
        public LocationAngleComp()
        {
            currentIndex = 0;
            plane = new string[4];
            matr = new double[2,2];

        }
        public double this[int index]
        {
            set
            {
                if (index >= 0 && index < 4)
                {

                    switch (index)
                    {
                        case 0:
                            LT = value;
                            matr[0,0] = value;
                            break;
                        case 1:
                            RT = value;
                            matr[0, 1] = value;
                            break;
                        case 2:
                            LB = value;
                            matr[1, 0] = value;
                            break;
                        case 3:
                            RB = value;
                            matr[1, 1] = value;
                            break;

                    }
                    currentIndex = (currentIndex + 1);
                }
            }
        }
        public void SetNextValue(double value, string planeComp)
        {
            if (currentIndex < 4)
            {
                this[currentIndex] = value;
                plane[currentIndex - 1] = planeComp;
            }
            else
            {
                throw new Exception("Индексатор заполнен");
            }
        }
        public double[,] GetMatr()
        {
            return matr;
        }
        public void SetPlane()
        {
            this.PlanComp = plane[3];

        }
    }
/*    internal class CompLocation
    {
        public double[] matrixSw;
        public string[] planes { get;}
        public string nChild { get;}
        public MathTransform compInNewSKR {  get; }

        public string[] planesParent {  get;}
        public string nameParent { get; }
        public List<string> listNameMate;

        double ScaleOutb = 0;
        Object Xch = null;
        Object Ych = null;
        Object Zch = null;
        Object TrObjOutch = null;
        double ScaleOutch = 0;
        public MathVector[] listVector;
        public double[] coord;
        public CompLocation(string nameComp, string[] _planes, MathTransform m, string[] _planesParent, string _nameParent)
        {
            nChild = nameComp; 
            planes = _planes;
            compInNewSKR = m;
            planesParent = _planesParent;
            nameParent = _nameParent;
            compInNewSKR.GetData(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
            listVector = new MathVector[3];
            listVector[0] = (MathVector) Xch;
            listVector[1] = (MathVector) Ych;
            listVector[2] = (MathVector) Zch;
            coord = (double[])((MathVector)TrObjOutch).ArrayData;
        }
 
    }*/
    public class ElementSW
    {
        public double[] matrixSw {  get; set; }
        public string[] planes { get; }
        public string nameSwComponent { get; }

        public List<LocationComponent> listLocComp;

        public List<Feature> listFeatureMate;
        public MathTransform compTransform;
       
        public MathVector[] listVector;
        public double[] coord;
        public event Action<string> DeletingPairing;
        private Component2 component2=null;

        public ElementSW(string nameComp, string[] _planes, double[] _matrixSw)
        {
            nameSwComponent = nameComp;
            planes = _planes;
            matrixSw = _matrixSw;
            compTransform = InitializeMathTransform(matrixSw);
        }

        public ElementSW(Component2 comp)
        {
            this.component2 = comp;
            nameSwComponent = comp.Name2;
            planes = GetPlanesComp(comp);
            compTransform = (MathTransform)comp.Transform2;       
            matrixSw = (double[])compTransform.ArrayData;
           
            GetMate(comp);
        }
        public ElementSW(Component2 comp, string nameRoot)
        {
            this.component2 = comp;
            nameSwComponent = comp.Name2 + "@" + nameRoot;
            planes = GetPlanesComp(comp);
            compTransform = (MathTransform)comp.Transform2;
            matrixSw = (double[])compTransform.ArrayData;
            GetMate(comp);
        }

        public bool GetStatus()
        {
            if (component2 == null) return true;
            int solveInt = component2.GetConstrainedStatus();
            if (solveInt == 3)return true;
            return false;
        }

        private void GetMate(Component2 swComp)
        {
            object[] Mates = null;
            Mate2 swMate;
            Mates = (Object[])swComp.GetMates();
            if ((Mates != null))
            {
                listFeatureMate = new List<Feature>();
                Feature f;
                string nameMate;
                foreach (Object SingleMate in Mates)
                {
                    if (SingleMate is Mate2)
                    {
                        swMate = (Mate2)SingleMate;

                        f = (Feature)swMate;
                        nameMate = f.Name;
                        listFeatureMate.Add(f);

                    }
                }
            }
        }

        public void InitVector(MathTransform rootTrans)
        { 
            double ScaleOutb = 0;
            Object Xch = null;
            Object Ych = null;
            Object Zch = null;
            Object TrObjOutch = null;
            double ScaleOutch = 0;

            MathTransform compInNewSKR = (MathTransform)compTransform.Multiply(rootTrans.Inverse());
            compInNewSKR.GetData(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
            listVector = new MathVector[3];
            listVector[0] = (MathVector)Xch;
            listVector[1] = (MathVector)Ych;
            listVector[2] = (MathVector)Zch;
            coord = (double[])((MathVector)TrObjOutch).ArrayData;
            listLocComp = new List<LocationComponent>();
        }
        private MathTransform InitializeMathTransform(double[] transformArray)
        {
            MathTransform transform;
            if (SolidServise.utility == null)
            {
                SldWorks sldWorks=new SldWorks();
                IMathUtility mathUtility = (MathUtility)sldWorks.GetMathUtility();
                transform = (MathTransform)mathUtility.CreateTransform(transformArray);
            }
            else
            {
                transform = (MathTransform)SolidServise.utility.CreateTransform(transformArray);
            }


                return transform;
        }
        private string[] GetPlanesComp(Component2 comp)
        {
            Feature swFeat = comp.FirstFeature();
            string[] planesBase = new string[3];
            int i = 2;
            while (swFeat != null)
            {
                if ("RefPlane" == swFeat.GetTypeName())
                {
                    planesBase[i] = swFeat.Name;
                    i--;
                }
                if (i < 0) break;
                swFeat = swFeat.GetNextFeature() as Feature;
            }
            return planesBase;
        }

        public LocationComponent CreateLocalComponent(int typeLoc)
        {
            LocationComponent component;
            if (typeLoc == 0)
            {
                component = new LocationComponent();
            }else 
            {
                component = new LocationAngleComp();
            }
                component.OverDefiningAssembly += Component_OverDefiningAssembly;
            return component;
        }

        protected void Component_OverDefiningAssembly()
        {
            bool IsWarning;
            string nameFeature;
            int errorCode;
            if (listFeatureMate==null) return;
            foreach (Feature feat in listFeatureMate)
            {
                errorCode = feat.GetErrorCode2(out IsWarning);

                if (errorCode == 1 ||errorCode == 46 || errorCode == 47)
                {
                    nameFeature=feat.Name;
                    DeletingPairing?.Invoke(nameFeature);
                }
               
            }
        }
    }

    public struct MateFeature
    {
        public bool isBackMate {  get; }
        public string NameMate { get; }
        public MateFeature(bool _isBackMate, string _nameMate)
        {
            isBackMate = _isBackMate;
            NameMate = _nameMate;
        }

    }
}
