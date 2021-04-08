using System;
using System.Collections.Generic;
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

namespace TextureFinder
{
    /// <summary>
    /// Interaction logic for IntegerControl.xaml
    /// </summary>
    public partial class IntegerControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(IntegerControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public IntegerControl()
        {
            InitializeComponent();
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            Value--;
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            Value++;
        }
    }
}
