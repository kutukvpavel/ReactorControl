using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ModbusRegisterMap;
using ReactorControl.Models;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace ReactorControl.Views;

public partial class RegisterView : Window
{ 
    public RegisterView()
    {
        InitializeComponent();
    }
}