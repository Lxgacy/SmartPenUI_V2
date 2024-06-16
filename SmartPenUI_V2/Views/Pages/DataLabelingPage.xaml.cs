using Wpf.Ui.Controls;

namespace SmartPenUI_V2.Views.Pages
{
    /// <summary>
    /// Interaction logic for DataLabeling.xaml
    /// </summary>
    public partial class DataLabelingPage : INavigableView<ViewModels.DataLabelingViewModel>
    {
        public ViewModels.DataLabelingViewModel ViewModel { get; }

        public DataLabelingPage(ViewModels.DataLabelingViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
