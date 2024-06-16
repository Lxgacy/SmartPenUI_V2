using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace SmartPenUI_V2.Views.Pages
{
    /// <summary>
    /// Interaction logic for Prediction.xaml
    /// </summary>
    public partial class PredictionPage : INavigableView<ViewModels.PredictionViewModel>
    {
        public ViewModels.PredictionViewModel ViewModel { get; }

        public PredictionPage(ViewModels.PredictionViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
