using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using System.Windows.Media;
using SmartPenUI_V2.Models;

namespace SmartPenUI_V2.ViewModels
{
    public partial class DataLabelingViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        public void OnNavigatedTo()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            // do some stuff

            _isInitialized = true;
        }
    }
}
