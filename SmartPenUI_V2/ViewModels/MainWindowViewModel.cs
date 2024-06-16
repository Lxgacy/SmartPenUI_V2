// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartPenUI_V2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<object> _navigationItems = [];

    [ObservableProperty]
    private ObservableCollection<object> _navigationFooter = [];

    [ObservableProperty]
    private ObservableCollection<MenuItem> _trayMenuItems = [];

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Demo"
    )]
    public MainWindowViewModel(INavigationService navigationService)
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }

    private void InitializeViewModel()
    {
        ApplicationTitle = "SmartPen UI";

        NavigationItems =
        [
            new NavigationViewItem()
            {
                Content = "Data Labeling",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Save24 },
                TargetPageType = typeof(Views.Pages.DataLabelingPage)
            },
            new NavigationViewItem()
            {
                Content = "Prediction",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Lightbulb24 },
                TargetPageType = typeof(Views.Pages.PredictionPage)
            },
        ];

        NavigationFooter =
        [
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            },
        ];

        TrayMenuItems = [new() { Header = "Home", Tag = "tray_home" }];

        _isInitialized = true;
    }
}
