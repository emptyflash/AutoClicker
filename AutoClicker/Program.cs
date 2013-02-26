using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsInput;

public class Clicker
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;
    public static bool click = false;
    private int delay;
    private bool rightClick;

    public Clicker(int d, bool right)
    {
        delay = d;
        rightClick = right;
    }

   public void DoMouseClick()
   {
      //Call the imported function with the cursor's current position
      int X = Cursor.Position.X;
      int Y = Cursor.Position.Y;
      if (rightClick)
      {
          mouse_event(MOUSEEVENTF_RIGHTDOWN, X, Y, 0, 0);
          mouse_event(MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
      }
      else
      {
          mouse_event(MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
          mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
      }
   }
    public void run()
    {
        while (true)
        {
            if(click)
                DoMouseClick();
            Thread.Sleep(delay);
        }
    }
    public static void toggle()
    {
        click = !click;
    }
};


class AutoClicker
{

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Main()
    {
        _hookID = SetHook(_proc);
        Console.WriteLine("Enter the delay between each click in milliseconds (50 is pretty good): ");
        int delay = int.Parse(Console.ReadLine());
        Console.WriteLine("Would you like right click? (y/n)");
        String input = Console.ReadLine();
        input = input.ToLower();
        bool rightClick = input.Equals("y");
        Console.WriteLine("Press the \"Insert\" key to start and stop the clicker");
        Clicker clicks = new Clicker(delay, rightClick);
        Thread thread = new Thread(new ThreadStart(clicks.run));
        thread.Start();
        while (!thread.IsAlive)
        {
            Thread.Sleep(1);
        }
        Application.Run();
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (((Keys)vkCode).ToString().Equals("Insert"))
            {
                Clicker.toggle();
                Console.WriteLine((Keys)vkCode + " pressed, clicker is now " + (Clicker.click ? "on" : "off"));
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
};