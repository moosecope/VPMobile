using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VPMobileAdmin
{
    public static class HelpCommand
    {
        private static readonly RoutedUICommand openHelpCommand = new RoutedUICommand("description", "OpenHelpCommand", typeof(HelpCommand));

        public static RoutedUICommand OpenHelpCommand
        {
            get
            {
                return openHelpCommand;
            }
        }
    }
}
