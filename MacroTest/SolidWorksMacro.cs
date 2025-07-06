using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using CodeStack.SwEx.PMPage;
using CodeStack.SwEx.Common;

namespace MacroTest
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
            Front = 2
        }
    public partial class SolidWorksMacro
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
        private PropertyManagerPageEx<MatePmpHandler, Model> pageMate;
        private Model model;

        public void Main()
        {
      
            swModel = (ModelDoc2)swApp.ActiveDoc;
            swAssemblyDoc = (AssemblyDoc)swModel;
            utility = (MathUtility)swApp.GetMathUtility();
            nameAssemble = GetNameAssemble(swAssemblyDoc);
            swDocExt = swModel.Extension;
            swSelMgr = (SelectionMgr)swModel.SelectionManager;
            model = new Model();
            pageMate = new PropertyManagerPageEx<MatePmpHandler, Model>(swApp);
            // AddPairingMultyComp();
           // pageMate.Handler.Closed += Handler_Closed;
          //  pageMate.Handler.Closing += Handler_Closing;
            pageMate.Show(model);
            return;
        }

        private void Handler_Closing(swPropertyManagerPageCloseReasons_e reason, CodeStack.SwEx.PMPage.Base.ClosingArg arg)
        {
            MessageBox.Show("2222");
        }

        private void Handler_Closed(swPropertyManagerPageCloseReasons_e reason)
        {
            pageMate.Handler.Closing -= Handler_Closing;
            pageMate.Handler.Closed -= Handler_Closed;
        }

        // The SldWorks swApp variable is pre-assigned for you.
        public SldWorks swApp;
        private string GetNameAssemble(AssemblyDoc swAssemblyDoc)
        {
            string AssemblyTitle;
            string AssemblyName;
            AssemblyTitle = swModel.GetTitle();
            return AssemblyTitle;      
        }
        public bool AddPairingMultyComp()
        {
            PlaneName pn;
            double ScaleOutb = 0;
            Object Xch = null;
            Object Ych = null;
            Object Zch = null;
            Object TrObjOutch = null;
            double ScaleOutch = 0;

            string nBaseRignt = PlaneName.Right.ToString();
       
            List<LocationComponent> listLocComp = new List<LocationComponent>();

            Component2 compChild =(Component2) swSelMgr.GetSelectedObject6(1, -1);
            string nChild = compChild.Name2;
            MathTransform swTrChild = (MathTransform)compChild.Transform2;

            swTrChild.GetData(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
            MathVector[] listVecor = new MathVector[4];
            listVecor[0] = (MathVector)Xch;
            listVecor[1] = (MathVector)Ych;
            listVecor[2] = (MathVector)Zch;
            listVecor[3] = (MathVector)TrObjOutch;
            double[] coord = (double[])listVecor[3].ArrayData;

            for (int i = 0; i < 3; i++)
            {
                LocationComponent l = new LocationComponent();
                l.baseComp = nameAssemble;
                l.childComp = nChild + "@" + nameAssemble;
                l = IsFlipedAndAlign(l, listVecor[i], coord[i]);
                l.dist = Math.Abs(Math.Round(coord[i], 3));
                listLocComp.Add(l);
            }
            DeletingMateComp(compChild);
            AddMate(listLocComp);
            return true;
        }
        private LocationComponent IsFlipedAndAlign(LocationComponent loc, MathVector vector, double coord)
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
    }
}

