using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MateSwAddIns
{
    [Guid("D3B5A734-F667-42F0-B70E-A0CDCBAB3FD1"), ComVisible(true)]
    [AutoRegister("AddInSwMate", "AddInSwMate2", true)]
   public class swAddInsMate : SwAddInEx
    {
        private ModelDoc2 swDoc;
        private AssemblyDoc swMainAssy;
        private Configuration swMainConfig;
        private SelectionMgr selMgr;
        //public SldWorks swApp;
        TaskpaneView taskPaneView;
        PanelTree ctrl;
        string[] planeBase;
        public override bool OnConnect()
        {
            taskPaneView =(TaskpaneView) CreateTaskPane<PanelTree>(out ctrl);
            taskPaneView.AddStandardButton((int)swTaskPaneBitmapsOptions_e.swTaskPaneBitmapsOptions_Ok, "Connect");
            taskPaneView.TaskPaneToolbarButtonClicked += TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.ShowView();
            return base.OnConnect();
        }

        private int TaskPaneView_TaskPaneToolbarButtonClicked(int ButtonIndex)
        {
            swDoc = (ModelDoc2)App.ActiveDoc;
            if (swDoc == null)
            {
             
                ctrl.lblMessage.Text="Could not acquire an active document";
                return 1;
            }
            swDocumentTypes_e swDocType;
            swDocType = (swDocumentTypes_e)swDoc.GetType();


            if (swDocType != swDocumentTypes_e.swDocASSEMBLY)
            {
                ctrl.lblMessage.Text = "This program only works with assemblies";
                return 1;
            }
            swMainAssy = (AssemblyDoc)swDoc;
            selMgr = (SelectionMgr)swDoc.ISelectionManager;
            swMainAssy.UserSelectionPostNotify += SwMainAssy_UserSelectionPostNotify;
           
            return 0;


        }

        private int SwMainAssy_UserSelectionPostNotify()
        {
            Component2 childComp;
            int count = selMgr.GetSelectedObjectCount2(-1);
            if (count > 0)
            {
                childComp = (Component2)selMgr.GetSelectedObjectsComponent4(1, -1);
                if (childComp != null)
                {
                    GetComponentPosition(childComp);
                }

            }

            return 0;
        }
        private void GetComponentPosition(Component2 swComponent)
        {
            planeBase = new string[3];
            planeBase[0] = "Right";
            planeBase[1] = "Top";
            planeBase[2] = "Front";
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
            double k = 180 / Math.PI;

            String msg = swComponent.Name2 + "\n";
            msg = msg + rotationMatrix[0] +":"+ rotationMatrix[1] + ":"+rotationMatrix[2] + "\n ";
            msg = msg + rotationMatrix[3] + ":" + rotationMatrix[4] + ":"+rotationMatrix[5] + "\n ";
            msg = msg + rotationMatrix[6] + ":" + rotationMatrix[7] + ":"+rotationMatrix[8] + "\n ";
            msg = msg + "Смещение: X={0},Y={1}, Z={2},\n" + offsetX*1000 + "\n" + offsetY*1000 + "\n" + offsetZ*1000 + "\n";
            msg=msg + "Углы: X={0}, Y={1}, Z={2}" + "\n" + k*angleX + "\n" + k*angleY + "\n" + k * angleZ + "\n";

            double ScaleOutb = 0;
            Object Xch = null;
            Object Ych = null;
            Object Zch = null;
            Object TrObjOutch = null;
            double ScaleOutch = 0;

            transform.GetData(ref Xch, ref Ych, ref Zch, ref TrObjOutch, ref ScaleOutch);
            MathVector[] listVecor = new MathVector[3];
            listVecor[0] = (MathVector)Xch;
            listVecor[1] =(MathVector) Ych;
            listVecor[2] = (MathVector)Zch;
            double[] coord = (double[])((MathVector)TrObjOutch).ArrayData;

            double[] orientationX= (double[])listVecor[0].ArrayData;
            double[] orientationY= (double[])listVecor[1].ArrayData;
            double[] orientationZ = (double[])listVecor[2].ArrayData;

            msg = msg + orientationX[0] + ":" + orientationX[1] + ":" + orientationX[2] + "\n ";
            msg = msg + orientationY[0] + ":" + orientationY[1] + ":" + orientationY[2] + "\n ";
            msg = msg + orientationZ[0] + ":" + orientationZ[1] + ":" + orientationZ[2] + "\n ";

            msg = msg + Math.Round(translation[0],3) + ":" + Math.Round(translation[1], 3) + ":" + Math.Round(translation[2], 3) + "\n";
            msg = msg + Math.Round(translation[3], 3) + ":" + Math.Round(translation[4], 3) + ":" + Math.Round(translation[5], 3) + "\n";
            msg = msg + Math.Round(translation[6], 3) + ":" + Math.Round(translation[7], 3) + ":" + Math.Round(translation[8], 3) + "\n";

            List<LocationComponent> listLocComp = new List<LocationComponent>();
            LocationAngleComp angleComp = new LocationAngleComp();
            for (int i = 0; i < 3; i++)
            {
                LocationComponent l = new LocationComponent();
          
                l.baseComp = planeBase[i];
                l = IsFlipedAndAlign(l, listVecor[i], coord[i], angleComp);
                l.dist = Math.Abs(Math.Round(coord[i], 3));
                listLocComp.Add(l);
                msg = msg + "**" + l.PlanComp + " : " + l.baseComp + "\n";
            }
            msg = msg + "angle1: " + angleComp.LT + " =: " + angleComp.plane[0] + " - " + angleComp.RT + " =: " + angleComp.plane[1] + "\n";
            msg = msg + "angle2: " + angleComp.LB + " =: " + angleComp.plane[2] + " - " + angleComp.RB + " =: " + angleComp.plane[3] + "\n";
             double[,] angleValue= new double[2,2];
            angleValue[0,0] =angleComp.LT; angleValue[0, 1] = angleComp.RT;
            angleValue[1,0]=angleComp.LB; angleValue[1, 1] = angleComp.RB;
           
            ctrl.lblMessage.Text = msg +  DetermineTransformation(angleValue);
        }
        private  LocationComponent IsFlipedAndAlign(LocationComponent loc, MathVector vector, double coord, LocationAngleComp angleComp)
        {
            // PlaneName plane = PlaneName.Right;
            double[] orientation = (double[])vector.ArrayData;
            if (Math.Abs(Math.Round(coord * 1000)) < 1) coord = 0;

            double temp;
       
            for (int i = 0; i < 3; i++)
            {
                temp = orientation[i];
                switch (temp)
                {
                  case 1:
                    loc.PlanComp = planeBase[i];
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
                    loc.PlanComp = planeBase[i];
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
                        angleComp.SetNextValue(Math.Round(temp,3), planeBase[i]);
                        loc.PlanComp = planeBase[i];
                        break;
                    case double val when val >= -0.99 && val <= -0.1:
                        angleComp.SetNextValue(Math.Round(temp,3), planeBase[i]);
                        loc.PlanComp = planeBase[i];
                        break;
                    default:
                        break;
                }
               

            }
            return loc;

        }
        public string DetermineTransformation(double[,] transformMatrix )
        {
            string msg = "";
            // Определение выравнивания тела
            string alignmentType = "";
            bool alignment = Math.Sign(transformMatrix[0, 0]) == Math.Sign(transformMatrix[1, 1]);
            alignmentType = alignment ? "Совместное" : "Противоположное";

            // Нахождение угла поворота тела
            double angle = Math.Atan2(transformMatrix[1, 0], transformMatrix[0, 0]) * (180 / Math.PI);

            // Определение направления тела
            string direction = (transformMatrix[0, 0] * transformMatrix[1, 1] - transformMatrix[1, 0] * transformMatrix[0, 1] > 0) ? "По часовой стрелке" : "Против часовой стрелки";

            msg = msg + " Угол поворота: " + angle + "\n";
            msg = msg + "Выравнивание: " + alignmentType + "\n";
            msg = msg + "Направление: " + direction + "\n";
            return msg;
        }
        internal class LocationComponent
        {
            public bool Fliped;
            public swMateAlign_e Align;
            public double dist;
            public string baseComp;
            public string PlanComp;       
        }
        internal class LocationAngleComp
        {
            public double LT;
            public double RT;
            public double LB;
            public double RB;
            public string[] plane;
            
            private int currentIndex;
            public LocationAngleComp()
            {
               currentIndex = 0;
               plane = new string[4];
              
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
                                break;
                            case 1:
                                RT = value;
                                break;
                            case 2:
                                LB = value;
                                break;
                            case 3:
                                RB = value;
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
                    plane[currentIndex-1] = planeComp;
                   
                }
                else
                {
                    throw new Exception("Индексатор заполнен");
                }
            }
        }

        public override bool OnDisconnect()
        {
            taskPaneView.TaskPaneToolbarButtonClicked -= TaskPaneView_TaskPaneToolbarButtonClicked;
            taskPaneView.DeleteView();
            return base.OnDisconnect();

        }
       
    }
}
