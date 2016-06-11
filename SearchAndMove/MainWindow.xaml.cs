using SearchAndMove.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace SearchAndMove
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainWindowViewModel();
            this.DataContext = vm;
        }

        private void SearchPaths_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void SearchPaths_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var items = e.Data.GetData(DataFormats.FileDrop);

                foreach (string dir in items as string[])
                {
                    if (Directory.Exists(dir))
                        vm.AddPath(dir);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
