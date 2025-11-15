using BrickBreaker.UI.Ui.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    public class LoginMenu
    {
        public LoginMenuChoice Show()
        {
            return MenuHelper.ShowMenu<LoginMenuChoice>("Login Menu");
        }
    }
}
}
