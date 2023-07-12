using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace ReactorControl;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        //https://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c
        // get application GUID as defined in AssemblyInfo.cs
        string appGuid =
            (Assembly.GetExecutingAssembly().
                GetCustomAttributes(typeof(GuidAttribute), false).
                    GetValue(0) as GuidAttribute)?.ToString() ?? "7aeb79af-f9b0-4d97-9f6a-a96fea36d525";

        // unique id for global mutex - Global prefix means it is global to the machine
        string mutexId = string.Format("Global\\{{{0}}}", appGuid);

        // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
        using (var mutex = new Mutex(false, mutexId, out bool createdNew))
        {
            // edited by acidzombie24
            var hasHandle = false;
            try
            {
                try
                {
                    // note, you may want to time out here instead of waiting forever
                    // edited by acidzombie24
                    // mutex.WaitOne(Timeout.Infinite, false);
                    hasHandle = mutex.WaitOne(2000, false);
                    if (hasHandle == false)
                        throw new TimeoutException("Timeout waiting for exclusive access");
                }
                catch (AbandonedMutexException)
                {
                    // Log the fact that the mutex was abandoned in another process,
                    // it will still get acquired
                    hasHandle = true;
                }

                // Perform your work here.
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            finally
            {
                // edited by acidzombie24, added if statement
                if (hasHandle)
                    mutex.ReleaseMutex();
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia()
            .UseReactiveUI();
}
