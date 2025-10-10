using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;
using Microsoft.VisualBasic;

namespace MyMacroAddIn
{
    public static class AddComps
    {
        public static SldWorks m_swApp;

        public static void AddCompAndMate(string PartPath)
        {
            ModelDoc2 Model = (ModelDoc2)m_swApp.ActiveDoc;
            ModelView MyView = (ModelView)Model.ActiveView;
            int errors;
            if (Model.GetType() != (int)swDocumentTypes_e.swDocASSEMBLY)
            {
                m_swApp.SendMsgToUser2("For use with assemblies only.", 
                    (int)swMessageBoxIcon_e.swMbWarning, (int)swMessageBoxBtn_e.swMbOk);
                return;
            }

            AssemblyDoc MyAssy = (AssemblyDoc)Model;
            // continue adding processing code here
            // get the current selection
            // user should have selected the edge of a hole
            // or an entire face for parts to be inserted on
            var Edges = new List<Edge>();
            SelectionMgr selMgr = (SelectionMgr)Model.SelectionManager;
            object SelectedObject = null;
            int SelCount = selMgr.GetSelectedObjectCount2(-1);
            if (SelCount > 0)
            {
                // make sure they have selected a face
                SelectedObject = selMgr.GetSelectedObject6(1, -1);
                int SelType = selMgr.GetSelectedObjectType3(1, -1);
                if (SelType == (int)swSelectType_e.swSelFACES & SelCount == 1)
                {
                    // get all circular edges on the face
                    Edges = GetAllFaceCircularEdges((Face2)SelectedObject);
                }
                else if (SelType == (int)swSelectType_e.swSelEDGES)
                {
                    // get all circular edges selected
                    Edges = GetSelectedCircularEdges(selMgr);
                }
            }
            else
            {
                m_swApp.SendMsgToUser2("Please select a face or hole edges.", 
                    (int)swMessageBoxIcon_e.swMbWarning, (int)swMessageBoxBtn_e.swMbOk);
                return;
            }

            // open the part now
            PartDoc Part = OpenPartInvisible(PartPath);
            if (Part == null | Edges == null)
        {
                CleanUp(MyView, Model);
                return;
            }


            // get the component that was selected
            Component2 SelectedComp = null;
            SelectedComp = (Component2)selMgr.GetSelectedObjectsComponent2(1);

            // turn off assembly graphics update
            // add it to each circular edge
            MyView.EnableGraphicsUpdate = false;
            foreach (Edge CircEdge in Edges)
            {
                // get the center of the circular edge
                double[] Center;
                Center = GetCircleCenter((Curve)CircEdge.GetCurve(), SelectedComp);
                // insert the part at the circular edge's center
                Component2 MyComp = MyAssy.AddComponent4(PartPath, "", Center[0], Center[1], Center[2]);

                // get the two faces from the edge
                // set the first face to the cylinder
                // the second to the flat face
                var MyFaces = GetEdgeFaces(CircEdge);
                // Add Mates
                // add a coincident mate bewteen the flat face
                // and the Front Plane of the added component
                Feature MyPlane = MyComp.FeatureByName("Front Plane");
                Mate2 MyMate;
                Entity ent;
                if (MyPlane is object)
                {
                    MyPlane.Select2(false, -1);
                    ent = (Entity)MyFaces[1];
                    ent.Select(true);
                    MyMate = MyAssy.AddMate5((int)swMateType_e.swMateCOINCIDENT, 
                        (int)swMateAlign_e.swMateAlignALIGNED, false, 0, 0, 0, 0, 
                        0, 0, 0, 0, false, false, 0, out errors);
                }

                // mate the cylinder concentric to "Axis1"
                Feature MyAxis = MyComp.FeatureByName("Axis1");
                if (MyAxis is object)
                {
                    MyAxis.Select2(false, -1);
                    // select the cylindrical face
                    ent = (Entity)MyFaces[0];
                    ent.Select(true);
                    MyMate = MyAssy.AddMate5((int)swMateType_e.swMateCONCENTRIC, 
                        (int)swMateAlign_e.swMateAlignCLOSEST, false, 0, 0, 0, 0, 
                        1, 0, 0, 0, false, false, 0, out errors);

                }
            }

            CleanUp(MyView, Model);
        }

        private static void CleanUp(ModelView MyView, ModelDoc2 Model)
        {
            // turn part visibility back on
            m_swApp.DocumentVisible(true, (int)swDocumentTypes_e.swDocPART);
            // turn graphics updating back on
            MyView.EnableGraphicsUpdate = true;
            Model.ClearSelection2(true);
        }

        private static PartDoc OpenPartInvisible(string PartPath)
        {
            var errors = default(int);
            var warnings = default(int);
            // open the file invisibly
            m_swApp.DocumentVisible(false, (int)swDocumentTypes_e.swDocPART);
            PartDoc Part = (PartDoc)m_swApp.OpenDoc6(PartPath, (int)swDocumentTypes_e.swDocPART, 
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "",  errors,  warnings);

            if (Part == null)
            {
                m_swApp.SendMsgToUser2("Unable to open " + PartPath, 
                    (int)swMessageBoxIcon_e.swMbWarning, (int)swMessageBoxBtn_e.swMbOk);
                return null;
            }

            return Part;
        }

        private static List<Edge> GetSelectedCircularEdges(SelectionMgr selMgr)
        {
            object SelectedObject;
            var Edges = new List<Edge>();
            for (int i = 1, loopTo = selMgr.GetSelectedObjectCount2(-1); i <= loopTo; i++)
            {
                SelectedObject = selMgr.GetSelectedObject6(i, -1);
                int SelType = selMgr.GetSelectedObjectType3(i, -1);
                // make sure each selection is an edge
                if (SelType == (int)swSelectType_e.swSelEDGES)
                {
                    Edge MyEdge = (Edge)SelectedObject;
                    if (IsFullCircle(MyEdge))
                    {
                        Edges.Add(MyEdge);
                    }
                }
            }

            return Edges;
        }

        private static List<Edge> GetAllFaceCircularEdges(Face2 SelectedFace)
        {
            var Edges = new List<Edge>();
            object[] FaceEdges = (object[])SelectedFace.GetEdges();
            foreach (Edge MyEdge in FaceEdges)
            {
                if (IsFullCircle(MyEdge))
                {
                    Edges.Add(MyEdge);
                }
            }

            return Edges;
        }

        private static bool IsFullCircle(Edge EdgeToCheck)
        {
            bool IsFullCircleRet;
            Curve MyCurve = (Curve)EdgeToCheck.GetCurve();
            if (MyCurve.IsCircle())
            {
                // you have a circular edge
                // is it a complete circle?
                if (EdgeToCheck.GetStartVertex() == null)
                {
                    // full circle
                    IsFullCircleRet = true;
                    return IsFullCircleRet;
                }
            }

            IsFullCircleRet = false;
            return IsFullCircleRet;
        }

        // function to get and return the two adjacent faces of the edge
        private static Face2[] GetEdgeFaces(Edge MyEdge)
        {
            Face2[] GetEdgeFacesRet = null;
            var tmpFaces = new Face2[2];
            object[] faces = (object[])MyEdge.GetTwoAdjacentFaces2();
            Face2 tmpFace0 = (Face2)faces[0];
            Surface tmpSurf0 = (Surface)tmpFace0.GetSurface();
            // check if the surface is a cylinder
            if (tmpSurf0.IsCylinder())
            {
                tmpFaces[0] = (Face2)faces[0];
                tmpFaces[1] = (Face2)faces[1];
            }
            else
            {
                tmpFaces[0] = (Face2)faces[1];
                tmpFaces[1] = (Face2)faces[0];
            }
            // the zero element should be a cylinder
            GetEdgeFacesRet = tmpFaces;
            return GetEdgeFacesRet;
        }

        // return an array of doubles for the x, y, z circle center
        // relative to the assembly
        private static double[] GetCircleCenter(Curve MyCurve, Component2 Comp)
        {
            double[] GetCircleCenterRet = null;
            var MyCenter = new double[3];
            double[] returnValues = (double[])MyCurve.CircleParams;
            MyCenter[0] = returnValues[0];
            MyCenter[1] = returnValues[1];
            MyCenter[2] = returnValues[2];
            MathUtility MathUtil = (MathUtility)m_swApp.GetMathUtility();
            MathPoint mPoint = null;
            mPoint = (MathPoint)MathUtil.CreatePoint(MyCenter);
            MathTransform CompTransform = Comp.Transform2;
            mPoint = (MathPoint)mPoint.MultiplyTransform(CompTransform);
            // return the x,y,z location in assembly space
            GetCircleCenterRet = (double[])mPoint.ArrayData;
            return GetCircleCenterRet;
        }
    }
}
