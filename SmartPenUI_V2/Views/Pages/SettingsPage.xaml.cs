// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls;

namespace SmartPenUI_V2.Views.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : INavigableView<ViewModels.SettingsViewModel>
{
    public ViewModels.SettingsViewModel ViewModel { get; }

    public SettingsPage(ViewModels.SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
	private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
	{
		// Use regular expression to check if input is numeric
		e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
	}

	private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
	{
		if (e.DataObject.GetDataPresent(typeof(string)))
		{
			string text = (string)e.DataObject.GetData(typeof(string));
			// Check if pasted text is numeric
			if (!System.Text.RegularExpressions.Regex.IsMatch(text, "^[0-9]+$"))
			{
				e.CancelCommand();
			}
		}
		else
		{
			e.CancelCommand();
		}
	}
}
