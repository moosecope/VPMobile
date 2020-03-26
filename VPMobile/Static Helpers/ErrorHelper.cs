using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VP_Mobile.StaticHelpers
{
    public static class ErrorHelper
    {
        public static void OnError(String className, String description, Exception ex, [CallerMemberName] String callingMethod = "")
        {
            MessageBox.Show(className + "(" + callingMethod + ") - " + description + Environment.NewLine + Environment.NewLine + ex.ToString());
        }

        public static void OnMessage(String className, String message, [CallerMemberName] String callingMethod = "")
        {
            MessageBox.Show(className + "(" + callingMethod + ") - " + Environment.NewLine + Environment.NewLine + message);
        }

        public static MessageBoxResult OnMessage(String message, String caption, MessageBoxButton buttons)
        {
            return MessageBox.Show(message, caption, buttons);
        }

        public static MessageBoxResult OnMessage(String message)
        {
            return MessageBox.Show(message);
        }
    }
}
