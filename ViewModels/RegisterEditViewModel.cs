using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ModbusRegisterMap;
using ReactorControl.Models;
using ReactorControl.Views;

namespace ReactorControl.ViewModels;

public class RegisterEditViewModel : ViewModelBase, INotifyDataErrorInfo
{
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public RegisterEditViewModel(Controller c, IRegister r, Window owner)
    {
        Owner = owner;
        mRegister = r;
        mController = c;
        mRegister.PropertyChanged += MRegister_PropertyChanged;
    }

    private void MRegister_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsComplex) return;
        RaisePropertyChanged(nameof(TextboxValue));
    }

    private readonly Controller mController;
    private readonly IRegister mRegister;
    private string? mWriteTxt = null;

    public Window Owner { get; }
    public bool IsComplex => mRegister.Value is ComplexDevTypeBase;
    public string Name { get => mRegister.Name; }
    public string? TextboxValue
    {
        get => IsComplex ? "" : mRegister.Value.ToString();
        set
        {
            if (!IsComplex) mWriteTxt = value;
        }
    }
    public bool IsReadOnly => mRegister.IsReadOnly;
    public bool HasErrors => Errors.Count > 0;
    public Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();

    public async Task Read()
    {
        await mController.ReadRegister(mRegister);
    }
    public bool TrySet()
    {
        if (mWriteTxt == null) return false;
        bool success = false;
        string pn = nameof(TextboxValue);
        try
        {
            mRegister.Value.TrySet(mWriteTxt);
            success = true;
        }
        catch (FormatException)
        {
            if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
            Errors[pn] = "Invalid format";
        }
        catch (OverflowException)
        {
            if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
            Errors[pn] = "Out of range";
        }
        catch (Exception ex)
        {
            if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
            Errors[pn] = ex.GetType().Name;
        }
        if (success)
        {
            if (Errors.ContainsKey(pn)) Errors.Remove(pn);
        }
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(TextboxValue)));
        return success;
    }
    public async Task Write()
    {
        if (TrySet())
        {
            string pn = nameof(TextboxValue);
            bool success = false;
            try
            {
                await mController.WriteRegister(mRegister);
                success = true;
            }
            catch (TimeoutException)
            {
                if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
                Errors[pn] = "Write timeout";
            }
            catch (Exception ex)
            {
                if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
                Errors[pn] = ex.GetType().Name;
            }
            if (success)
            {
                if (Errors.ContainsKey(pn)) Errors.Remove(pn);
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(TextboxValue)));
        }
        await mController.ReadRegister(mRegister);
    }

    private static string[] UnknownError { get; } = { "Unknown error" };

    public System.Collections.IEnumerable GetErrors(string? propertyName)
    {
        if (!HasErrors) return Enumerable.Empty<string>();
        try
        {
            return new string[] { Errors[propertyName ?? string.Empty] };
        }
        catch (KeyNotFoundException)
        {
            return UnknownError;
        }
    }

    public async Task Edit(Window owner)
    {
        await mController.ReadRegister(mRegister);
        var dialog = new ComplexRegisterEdit() { DataContext = mRegister };
        await dialog.ShowDialog(owner);
        if (dialog.Result)
        {
            await mController.WriteRegister(mRegister);
        }
        await mController.ReadRegister(mRegister);
    }
}