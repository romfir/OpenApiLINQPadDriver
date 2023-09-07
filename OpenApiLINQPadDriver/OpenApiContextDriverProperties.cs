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
        get => GetValue(false);
        set => SetValue(value);
    }

    protected override Dictionary<string, IList<ValidationRule>> ValidationRules { get; } = new()
    {
        [nameof(OpenApiDocumentUri)] = new List<ValidationRule> { new OpenApiUriValidationRule() },
        [nameof(ApiUri)] = new List<ValidationRule> { new ApiUriValidationRule() }
    };

    private T GetValue<T>(Func<string?, T> convert, T defaultValue, [CallerMemberName] string callerMemberName = "")
        => convert(_driverData.Element(callerMemberName)?.Value) ?? defaultValue;

    private bool GetValue(bool defaultValue, [CallerMemberName] string callerMemberName = "") =>
        GetValue(static v => v.ToBoolSafe(), defaultValue, callerMemberName)!.Value;

    //private int GetValue(int defaultValue, Func<int, int> adjustValueFunc, [CallerMemberName] string callerMemberName = "") =>
    //    adjustValueFunc(GetValue(static v => v.ToIntSafe(), defaultValue, callerMemberName)!.Value);

    //private string? GetValue(string defaultValue, [CallerMemberName] string callerMemberName = "") =>
    //    GetValue(static v => v, defaultValue, callerMemberName);

    private string? GetValue(string defaultValue, [CallerMemberName] string callerMemberName = "") =>
        GetValue(static v => v, defaultValue, callerMemberName);

    private T GetValue<T>(T defaultValue, [CallerMemberName] string callerMemberName = "")
        where T : Enum
        => (T)GetValue(v => Enum.TryParse(typeof(T), v, out var val) ? val : defaultValue, defaultValue, callerMemberName)!;

    private void SetValue<T>(T value, [CallerMemberName] string callerMemberName = "")
    {
        IsPropertyValid(value, callerMemberName);
        OnPropertyChanged(callerMemberName);
        _driverData.SetElementValue(callerMemberName, value);
    }
}