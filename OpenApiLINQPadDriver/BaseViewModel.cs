using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace OpenApiLINQPadDriver;

//https://stackoverflow.com/questions/56606441/how-to-add-validation-to-view-model-properties-or-how-to-implement-inotifydataer
public abstract class BaseViewModel : INotifyDataErrorInfo , INotifyPropertyChanged
{
    public bool IsPropertyValid<TValue>(TValue propertyValue, [CallerMemberName] string propertyName = null!)
    {
        _ = ClearErrors(propertyName);

        if (!ValidationRules.TryGetValue(propertyName, out var propertyValidationRules))
            return true;

        var errorMessages = propertyValidationRules.Select(validationRule => validationRule.Validate(propertyValue, CultureInfo.CurrentCulture))
            .Where(result => !result.IsValid)
            .Select(invalidResult => invalidResult.ErrorContent)
            .ToList();

        AddErrorRange(propertyName, errorMessages);

        return errorMessages.Count == 0;
    }

    private void AddErrorRange(string propertyName, ICollection<object> newErrors, bool isWarning = false)
    {
        if (newErrors.Count == 0)
            return;

        if (!Errors.TryGetValue(propertyName, out var propertyErrors))
        {
            propertyErrors = new List<object>();
            Errors.Add(propertyName, propertyErrors);
        }

        if (isWarning)
        {
            foreach (var error in newErrors)
            {
                propertyErrors.Add(error);
            }
        }
        else
        {
            foreach (var error in newErrors)
            {
                propertyErrors.Insert(0, error);
            }
        }

        OnErrorsChanged(propertyName);
    }

    public bool ClearErrors(string propertyName)
    {
        if (Errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
            return true;
        }
        return false;
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
      => string.IsNullOrWhiteSpace(propertyName)
        ? Errors.SelectMany(entry => entry.Value).ToList()
        : Errors.TryGetValue(propertyName, out var errors)
          ? errors
          : new List<object>();

    public bool HasErrors => Errors.Count > 0;
    public bool HasNoErrors => Errors.Count == 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(HasNoErrors));
    }

    private Dictionary<string, IList<object>> Errors { get; } = [];

    protected virtual Dictionary<string, IList<ValidationRule>> ValidationRules { get; } = [];
}

