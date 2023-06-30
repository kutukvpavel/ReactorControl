using System;
using System.Collections;
using System.Collections.Generic;
using ModbusRegisterMap;
using ReactorControl.Models;

namespace ReactorControl.ViewModels;

public class RegisterViewViewModel : ViewModelBase
{
    protected static IEnumerable<RegisterEditViewModel> CollectionHelper(Controller controller, ICollection c)
    {
        foreach (IRegister item in c)
        {
            yield return new RegisterEditViewModel(controller, item);
        }
    }

    public RegisterViewViewModel(ControllerControlViewModel c)
    {
        HoldingRegisters = CollectionHelper(c.Instance, c.HoldingRegisters.Values);
        InputRegisters = CollectionHelper(c.Instance, c.InputRegisters.Values);
    }

    public IEnumerable<RegisterEditViewModel> HoldingRegisters { get; }
    public IEnumerable<RegisterEditViewModel> InputRegisters { get; }
}