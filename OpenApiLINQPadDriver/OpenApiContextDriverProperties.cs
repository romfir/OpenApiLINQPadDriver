using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using OpenApiLINQPadDriver.Enums;
using OpenApiLINQPadDriver.Extensions;
using OpenApiLINQPadDriver.ValidationRules;

namespace OpenApiLINQPadDriver;
public class OpenApiContextDriverProperties : BaseViewModel
{
    private readonly IConnectionInfo _connectionInfo;
    private readonly XElement _driverData;
#if DEBUG_PUBLISH_TO_LINQPAD_FOLDER
    private const bool DefaultDebugInfo = true;
#else
    private const bool DefaultDebugInfo = false;
#endif

    public OpenApiContextDriverProperties(IConnectionInfo connectionInfo)
    {
        _connectionInfo = connectionInfo;
        _driverData = connectionInfo.DriverData;
    }

    public string[] GetCoreFxReferenceAssemblies() => DataContextDriver.GetCoreFxReferenceAssemblies(_connectionInfo);

    public string? OpenApiDocumentUri
    {
        get => GetValue(string.Empty);
        set
        {
            SetValue(value);
            OnPropertyChanged(nameof(IsOpenApiDocumentUriValid));
        }
    }

    public bool IsOpenApiDocumentUriValid => IsPropertyValid(OpenApiDocumentUri, nameof(OpenApiDocumentUri));

    public string? ApiUri
    {
        get => GetValue(string.Empty);
        set => SetValue(value);
    }

    public bool IsApiUriValid => IsPropertyValid(ApiUri, nameof(ApiUri));

    public EndpointGrouping EndpointGrouping
    {
        get => GetValue(EndpointGrouping.MultipleClientsFromFirstTagAndOperationName);
        set => SetValue(value);
    }

    public JsonLibrary JsonLibrary
    {
        get => GetValue(JsonLibrary.SystemTextJson);
        set => SetValue(value);
    }

    public ClassStyle ClassStyle
    {
        get => GetValue(ClassStyle.Poco);
        set => SetValue(value);
    }

    public bool GenerateSyncMethods
    {
        get => GetValue(false);
        set => SetValue(value);
    }

    public bool BuildInRelease
    {
        get => GetValue(false);
        set => SetValue(value);
    }

    public bool Persist
    {
        get => GetValue(true);
        set => SetValue(value);
    }

    public bool IsProduction
    {
        get => GetValue(false);
        set => SetValue(value);
    }

    public bool DebugInfo
    {
        get => GetValue(DefaultDebugInfo);
        set => SetValue(value);
    }

    protected override Dictionary<string, IList<ValidationRule>> ValidationRules { get; } = new()
    {
        [nameof(OpenApiDocumentUri)] = new List<ValidationRule> { new OpenApiUriValidationRule() },
        [nameof(ApiUri)] = new List<ValidationRule> { new ApiUriValidationRule() }
    };

    private T GetValue<T>(Func<string?, T> convert, T defaultValue, [CallerMemberName] string callerMemberName = "")
        => convert(_driverData.Element(callerMemberName)?.Value) ?? defaultValue;

    private bool GetValue(bool defaultValue, [CallerMemberName] string callerMemberName = "")
        => GetValue(static v => v.ToBoolSafe(), defaultValue, callerMemberName)!.Value;

    private string? GetValue(string defaultValue, [CallerMemberName] string callerMemberName = "")
        => GetValue(static v => v, defaultValue, callerMemberName);

    private T GetValue<T>(T defaultValue, [CallerMemberName] string callerMemberName = "")
        where T : struct, Enum
        => (T)GetValue(v => Enum.TryParse<T>(v, out var val) ? val : defaultValue, defaultValue, callerMemberName);

    private void SetValue<T>(T value, [CallerMemberName] string callerMemberName = "")
    {
        IsPropertyValid(value, callerMemberName);
        OnPropertyChanged(callerMemberName);
        _driverData.SetElementValue(callerMemberName, value);
    }
}