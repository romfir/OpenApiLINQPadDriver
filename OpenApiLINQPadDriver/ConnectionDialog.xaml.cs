using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using OpenApiLINQPadDriver.Enums;
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

        DataObject.AddPastingHandler(OpenApiDocumentUri, OnPasteOpenApiDocumentUri);
    }

    private OpenApiContextDriverProperties Properties => (OpenApiContextDriverProperties)DataContext;

    private void OnPasteOpenApiDocumentUri(object sender, DataObjectPastingEventArgs e)
    {
        var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);

        if (!isText)
            return;

        if (e.SourceDataObject.GetData(DataFormats.UnicodeText) is not string text)
            return;

        if (text.EndsWith(".json"))
        {
            Properties.OpenApiFormat = OpenApiFormat.Json;
            OpenApiFormatComboBox.UpdateSelectedValueProperty();
        }
        else if (text.EndsWith(".yaml"))
        {
            Properties.OpenApiFormat = OpenApiFormat.Yaml;
            OpenApiFormatComboBox.UpdateSelectedValueProperty();
        }
    }

    private void BtnOK_OnClick(object sender, RoutedEventArgs e)
    {
        if (Properties is { IsOpenApiDocumentUriValid: true, IsApiUriValid: true })
            DialogResult = true;
    }

    private void GetOpenApiJsonHyperlink_OnClick(object sender, RoutedEventArgs e)
    {
        var hyperlink = (Hyperlink)sender;

        hyperlink.IsEnabled = false;
        var properties = Properties;
        var mode = properties.OpenApiFormat;

        var extension = mode switch
        {
            OpenApiFormat.Json => "json",
            OpenApiFormat.Yaml => "yaml",
            _ => throw new InvalidOperationException()
        };
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = $"Get swagger.{extension}",
                Multiselect = false,
                ValidateNames = true,
                CheckPathExists = true,
                CheckFileExists = true,
                DefaultExt = extension,
                Filter = $"*.{extension}|*.{extension}",
                AddExtension = true
            };

            var result = openFileDialog.ShowDialog(this) == true;

            if (result)
            {
                properties.OpenApiDocumentUri = openFileDialog.FileName;
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

        var properties = Properties;

        if (!properties.IsOpenApiDocumentUriValid)
            return;

        hyperlink.IsEnabled = false;

        try
        {
            var document = await OpenApiDocumentHelper.GetFromUriAsync(new Uri(properties.OpenApiDocumentUri!), properties.OpenApiFormat).ConfigureAwait(false);

            var firstServerOrNull = document.Servers.FirstOrDefault();
            if (firstServerOrNull != null)
            {
                properties.ApiUri = firstServerOrNull.Url;

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
