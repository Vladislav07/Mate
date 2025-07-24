using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.swconst;

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

    }
    internal class ComponentCuby
    {
        string[] planesBase;
       public ComponentCuby(string[] _planesBase)
        {
            planesBase = _planesBase;
        }
    }
}
