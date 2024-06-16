using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace SmartPenUI_V2.ViewModels
{
    public partial class PredictionViewModel : ObservableObject, INavigationAware
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
