using desViewModel;
using System;
using System.Windows;
using System.Windows.Data;

namespace DesView
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DesController();
        }
    }
}
