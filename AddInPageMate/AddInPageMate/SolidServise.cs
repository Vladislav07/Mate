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
        public double dist;
        public string baseComp;
        public string childComp;
    }
    public enum PlaneName
    {
        Right = 0,
        Top = 1,
        Left = 2
    }
    internal class SolidServise
    {
        ModelDoc2 swModel;
        MathUtility utility;
        IModelDocExtension swDocExt;
        AssemblyDoc swAssemblyDoc;
        SelectionMgr swSelMgr;
        Dictionary<string, RefPlane> planes;
        bool boolstat;
        object[] Mates = null;
        Mate2 swMate;
        int mateError;
        string nameAssemble;

        public bool AddPairingMultyComp(Model model)
        {
            PlaneName pn;
            double ScaleOutb = 0;
            MathVector Xch = null;
            MathVector Ych = null;
            MathVector Zch = null;
            MathVector TrObjOutch = null;
            double ScaleOutch = 0;

            string nBaseRignt = PlaneName.Right.ToString();
            // string nBaseUp = GetName(list[1]);
            // string nBaseLeft = GetName(list[2]);
           // string nChild = GetName(list[3]);
            //  string nameAssemble;
            Component2 compChild;

            List<LocationComponent> listLocComp = new List<LocationComponent>();
            compChild=(Component2)model.components[0];
            string nChild = compChild.Name2;
            // compChild = (Component2)swAssemblyDoc.GetComponentByName(nChild);

            MathTransform swTrChild = (MathTransform)compChild.Transform2;

            MathTransform MtrInvPlaneBase = (MathTransform)GetMathTrsPlaneBase(nBaseRignt);


            MathTransform compInNewSKR = (MathTransform)swTrChild.Multiply(MtrInvPlaneBase);

            compInNewSKR.IGetData2(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
            MathVector[] listVecor = new MathVector[3];
            listVecor[0] = Xch;
            listVecor[1] = Ych;
            listVecor[2] = Zch;

            double[] coord = (double[])TrObjOutch.ArrayData;

            for (int i = 0; i < 1; i++)
            {
                LocationComponent l = new LocationComponent();
                l.baseComp = nBaseRignt + "@" + nameAssemble;
                l.childComp = nChild + "@" + nameAssemble;
                l = IsFlipedAndAlign(l, listVecor[i], coord[i]);
                l.dist = Math.Abs(Math.Round(coord[i], 3));
                listLocComp.Add(l);
            }
            DeletingMateComp(compChild);
            AddMate(listLocComp);
            return true;
        }
        private string GetName(string str)
        {
            int pos = str.IndexOf("@");
            string temp = str.Substring(0, pos);
            return temp;
        }
        private MathTransform GetMathTrsPlaneBase(string nBase)
        {
            boolstat = swDocExt.SelectByID2(nBase + "@" + nameAssemble, "PLANE", 0, 0, 0, false, 0, null, (int)swSelectOption_e.swSelectOptionDefault);
            Feature swFeature = (Feature)swSelMgr.GetSelectedObject6(1, -1);
            RefPlane swRefPlane = (RefPlane)swFeature.GetSpecificFeature2();
            RefPlaneFeatureData p = (RefPlaneFeatureData)swFeature.GetDefinition();
            double d = (double)p.Distance;
            MathTransform m = swRefPlane.Transform;
            double[] dOrigPt = new double[] { 0, 0, 0 };
            MathPoint point = (MathPoint)utility.CreatePoint((object)dOrigPt);
            point = (MathPoint)point.MultiplyTransform(m);
            double[] pointsArray = new double[3];
            pointsArray = (double[])point.ArrayData;
            return m;
        }
        private LocationComponent IsFlipedAndAlign(LocationComponent loc, MathVector vector, double coord)
        {
            PlaneName plane = PlaneName.Right;
            double[] orientation = (double[])vector.ArrayData;
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
        private void DeletingMateComp(Component2 swComp)
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
                        swModel.EditDelete();
                    }
                }
            }
        }
        private void AddMate(List<LocationComponent> orientation)
        {
            PlaneName pnMate = PlaneName.Right;
            foreach (LocationComponent compLocal in orientation)
            {
                AddMateToAssemble(compLocal, pnMate.ToString());
                pnMate++;
            }
        }
        private void AddMateToAssemble(LocationComponent orientation, string planeCoord)
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
            boolstat = swDocExt.SelectByID2(FirstSelection, "PLANE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
            boolstat = swDocExt.SelectByID2(SecondSelection, "PLANE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
            matefeature = (Feature)swAssemblyDoc.AddMate3((int)swMateType_e.swMateDISTANCE, (int)align, flipped, distance, distance, distance, 0, 0, 0, 0, 0, false, out mateError);
            matefeature.Name = MateName;
            swAssemblyDoc.EditRebuild();

        }
        private string GetNameAssemble(AssemblyDoc swAssemblyDoc)
        {
            string AssemblyTitle;
            string AssemblyName;
            AssemblyTitle = swModel.GetTitle();
            int index = AssemblyTitle.LastIndexOf('.');
            int len = AssemblyTitle.Length;
            AssemblyName = AssemblyTitle.Substring(0, index);
            return AssemblyName;
        }
    }
}
