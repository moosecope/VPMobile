﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VP_Mobile.StaticHelpers;
using VP_Mobile.ViewModels;

namespace VP_Mobile.Views
{
    /// <summary>
    /// Interaction logic for AvlUnitUserControl.xaml
    /// </summary>
    public partial class AvlGroupUserControl : UserControl, INotifyPropertyChanged
    {
        public AvlGroupUserControl()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }

        public AvlGroupViewModel AvlGroup
        {
            get { return (AvlGroupViewModel)this.GetValue(AvlGroupProperty); }
            set { this.SetValue(AvlGroupProperty, value); }
        }

        public static readonly DependencyProperty AvlGroupProperty = DependencyProperty.Register(
            "AvlGroup", typeof(AvlGroupViewModel), typeof(AvlGroupUserControl));

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        public event RoutedEventHandler AvlUnitSelected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }
        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent(
            "AvlUnitSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AvlGroupUserControl));
        #endregion

        #region private
        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Unit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SelectedEvent, sender));
        }

        private void Group_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                AvlGroup.Expanded = !AvlGroup.Expanded;
                if (AvlGroup.Expanded)
                {
                    bdrGroupColor.CornerRadius = new CornerRadius(10, 10, 0, 0);
                }
                else
                {
                    bdrGroupColor.CornerRadius = new CornerRadius(10);
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }
        #endregion
    }
}
