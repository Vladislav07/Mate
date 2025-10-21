using CodeStack.SwEx.PMPage;
using CodeStack.SwEx.PMPage.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddInPageMate
{
    internal class UIHandler
    {
        private PropertyManagerPageEx<MatePmpHandler, Model> pageMate;
        private IPropertyManagerPageControlEx opt;
        private IPropertyManagerPageControlEx r;
        private IPropertyManagerPageControlEx t;
        private IPropertyManagerPageControlEx l;
        private IPropertyManagerPageControlEx otherComp;
        public UIHandler(PropertyManagerPageEx<MatePmpHandler, Model> _pageMate) {
            pageMate = _pageMate;
            InitControls();
            
        }
        ~UIHandler()
        {
            opt.ValueChanged-= Opt_ValueChanged;
        }

        private void InitControls()
        {
            foreach (IPropertyManagerPageControlEx item in pageMate.Controls)
            {
                switch (item.Tag)
                {
                    case "Base":
                        opt = item;
                        opt.ValueChanged += Opt_ValueChanged;
                        break;
                    case "Right":
                        r = item;
                        r.Visible = false;
                        break;
                    case "Top":
                        t= item;
                        t.Visible = false;
                        break;
                    case "Left":
                        l = item;
                        l.Visible = false;
                        break;
                    case "BaseComponent":
                        otherComp = item;
                        otherComp.Visible = false;
                        break;
                }
            }
           // opt.SetValue(Model.IsBase_e.IsPlane);
        }

        private void Opt_ValueChanged(Xarial.VPages.Framework.Base.IControl sender, object newValue)
        {
            int value=(int)newValue;
            switch (value)
            {
                case 0:
                    r.Visible = false;
                    t.Visible = false;
                    l.Visible = false;
                    otherComp.Visible = false;  
                    break;
                case 1:
                    r.Visible = true;
                    t.Visible = true;
                    l.Visible = true;
                    otherComp.Visible = false;
                    break;
                case 2:
                    r.Visible = false;
                    t.Visible = false;
                    l.Visible = false;
                    otherComp.Visible = true;
                    break;
            }

        }

        
    }
}
