using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls;
using ModbusRegisterMap;
using ReactorControl.Models;

namespace ReactorControl.ViewModels;

public class RegisterViewViewModel : ViewModelBase
{
    protected Window mOwner;
    protected IEnumerable<RegisterEditViewModel> CollectionHelper(Controller controller, ICollection c)
    {
        foreach (IRegister item in c)
        {
            yield return new RegisterEditViewModel(controller, item, mOwner);
        }
    }

    public RegisterViewViewModel(ControllerControlViewModel c, Window w)
    {
        mOwner = w;
        HoldingRegisters = CollectionHelper(c.Instance, c.HoldingRegisters.Values);
        InputRegisters = CollectionHelper(c.Instance, c.InputRegisters.Values);
    }

    public IEnumerable<RegisterEditViewModel> HoldingRegisters { get; }
    public IEnumerable<RegisterEditViewModel> InputRegisters { get; }
}