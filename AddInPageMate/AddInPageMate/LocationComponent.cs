using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string TypeSelect;
    }
    internal class LocationAngleComp
    {
        public double LT;
        public double RT;
        public double LB;
        public double RB;
        public string[] plane;
        public bool Fliped;
        public swMateAlign_e Align;
        public double Angle;
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
                plane[currentIndex - 1] = planeComp;

            }
            else
            {
                throw new Exception("Индексатор заполнен");
            }
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
