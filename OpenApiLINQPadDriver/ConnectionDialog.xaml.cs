using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using OpenApiLINQPadDriver.Extensions;

namespace OpenApiLINQPadDriver;

/// <summary>
/// Interaction logic for ConnectionDialog.xaml
/// </summary>
internal partial class ConnectionDialog
{
    public ConnectionDialog(OpenApiContextDriverProperties openApiContextDriverProperties)
    {
        DataContext = openApiContextDriverProperties;
        InitializeComponent();
    }

    private void BtnOK_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogProperties = (OpenApiContextDriverProperties)DataContext;

        if (dialogProperties is { IsOpenApiDocumentUriValid: true, IsApiUriValid: true })
            DialogResult = true;
    }

    private void GetOpenApiJsonHyperlink_OnClick(object sender, RoutedEventArgs e)
    {
        var hyperlink = (Hyperlink)sender;

        hyperlink.IsEnabled = false;

        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Get swagger.json",
                Multiselect = false,
                ValidateNames = true,
                CheckPathExists = true,
                CheckFileExists = true,
                DefaultExt = "json",
                Filter = "*.json|*.json",
                AddExtension = true
            };

            var result = openFileDialog.ShowDialog(this) == true;

            if (result)
            {
                ((OpenApiContextDriverProperties)DataContext).OpenApiDocumentUri = openFileDialog.FileName;
                OpenApiDocumentUri.UpdateText();
            }
        }
        finally
        {
            hyperlink.IsEnabled = true;
        }
    }
    private async void DownloadApiUriHyperlink_OnClick(object sender, RoutedEventArgs e)
    {
        var hyperlink = (Hyperlink)sender;

        var dialogProperties = (OpenApiContextDriverProperties)DataContext;

        if (!dialogProperties.IsOpenApiDocumentUriValid)
            return;

        hyperlink.IsEnabled = false;

        try
        {
            var document = await OpenApiDocumentHelper.GetFromUriAsync(new Uri(dialogProperties.OpenApiDocumentUri!)).ConfigureAwait(false);

            var firstServerOrNull = document.Servers.FirstOrDefault();
            if (firstServerOrNull != null)
            {
                dialogProperties.ApiUri = firstServerOrNull.Url;

                Dispatcher.Invoke(ApiUri.UpdateText);
            }
            else
            {
                MessageBox.Show("This Open API documentation does not contain server address, please input it manually", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Dispatcher.Invoke(() => hyperlink.IsEnabled = true);
        }
    }
}
