using System.Data;
using System.Threading.Tasks;
using System.Resources;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using Blotch;
using System.Collections.Generic;
using System.Linq;
using Linearstar.Windows.RawInput;
using System.Diagnostics;
namespace penguin_new.Input
{
    public class RawInputHandler
    {
        RawInputKeyboard[] _keyboards; // keyboards we can handle with api
        RawInputMouse[] _mice;  // mice we can handle with api 

        RawInputReceiver _window;
        public RawInputHandler(IntPtr _handle)
        {
            RawInputDevice[] _devices = RawInputDevice.GetDevices(); // gets devices we can handle with rawinput api

            _keyboards = _devices.OfType<RawInputKeyboard>().ToArray(); // get all keyboards connected
            _mice = _devices.OfType<RawInputMouse>().ToArray(); // get all mice connected
            foreach (var i in _devices)
                Console.WriteLine(i.ProductName);

            _window = new RawInputReceiver(_handle); // create our receiver for inputs

            RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, _window.hWnd);
        }


    }

    public class RawInputReceiver : WindowsHook
    {
        public event EventHandler<RawInputEventArgs> Input;
        public RawInputReceiver(IntPtr _handle) : base(_handle)
        {

        }
        protected override void WndProc(ref Win32.Message m)
        {
            const int WM_INPUT = 0x00FF;


            // You can read inputs by processing the WM_INPUT message.
            if (m.msg == WM_INPUT)
            {
                // Create an RawInputData from the handle stored in lParam.
                var data = RawInputData.FromHandle(m.lparam);

                // You can identify the source device using Header.DeviceHandle or just Device.
                var sourceDeviceHandle = data.Header.DeviceHandle;
                var sourceDevice = data.Device;

                // The data will be an instance of either RawInputMouseData, RawInputKeyboardData, or RawInputHidData.
                // They contain the raw input data in their properties.
                switch (data)
                {
                    case RawInputMouseData mouse:
                        Console.WriteLine(mouse.Mouse);
                        break;
                    case RawInputKeyboardData keyboard:
                        Console.WriteLine(keyboard.Keyboard);
                        break;
                    case RawInputHidData hid:
                        Console.WriteLine(hid.Hid);
                        break;
                }
            }
        }
    }

    public class RawInputEventArgs : EventArgs
    {
        public RawInputEventArgs(RawInputData data)
        {
            Data = data;
        }

        public RawInputData Data { get; }
    }
    /*
        WindowsHook class from https://stackoverflow.com/a/647273
        author: SekHat
    */
    public abstract class WindowsHook : IDisposable
    {
        public IntPtr hHook;
        public IntPtr hWnd;
        // Stored here to stop it from getting garbage collected
        Win32.WndProcDelegate wndProcDelegate;

        public WindowsHook(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            wndProcDelegate = WndProcHook;

            CreateHook();
        }

        ~WindowsHook()
        {
            Dispose(false);
        }

        private void CreateHook()
        {

            uint threadId = Win32.GetWindowThreadProcessId(hWnd, IntPtr.Zero);

            hHook = Win32.SetWindowsHookEx(Win32.HookType.WH_CALLWNDPROC, wndProcDelegate, IntPtr.Zero, threadId);

        }

        private int WndProcHook(int nCode, IntPtr wParam, ref Win32.Message lParam)
        {
            if (nCode >= 0)
            {
                Win32.TranslateMessage(ref lParam); // You may want to remove this line, if you find your not quite getting the right messages through. This is here so that WM_CHAR is correctly called when a key is pressed.
                WndProc(ref lParam);
            }

            return Win32.CallNextHookEx(hHook, nCode, wParam, ref lParam);
        }

        protected abstract void WndProc(ref Win32.Message message);

        #region Interop Stuff
        protected static class Win32
        {
            public enum HookType : int
            {
                WH_JOURNALRECORD = 0,
                WH_JOURNALPLAYBACK = 1,
                WH_KEYBOARD = 2,
                WH_GETMESSAGE = 3,
                WH_CALLWNDPROC = 4,
                WH_CBT = 5,
                WH_SYSMSGFILTER = 6,
                WH_MOUSE = 7,
                WH_HARDWARE = 8,
                WH_DEBUG = 9,
                WH_SHELL = 10,
                WH_FOREGROUNDIDLE = 11,
                WH_CALLWNDPROCRET = 12,
                WH_KEYBOARD_LL = 13,
                WH_MOUSE_LL = 14
            }

            public struct Message
            {
                public IntPtr lparam;
                public IntPtr wparam;
                public uint msg;
                public IntPtr hWnd;
            }

            /// <summary>
            ///  Defines the windows proc delegate to pass into the windows hook
            /// </summary>                  
            public delegate int WndProcDelegate(int nCode, IntPtr wParam, ref Message m);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SetWindowsHookEx(HookType hook, WndProcDelegate callback,
                IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref Message m);

            [DllImport("coredll.dll", SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string module);

            [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
            public extern static bool TranslateMessage(ref Message m);

            [DllImport("user32.dll")]
            public extern static uint GetWindowThreadProcessId(IntPtr window, IntPtr module);
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources here
            }
            // Free unmanaged resources here
            if (hHook != IntPtr.Zero)
            {
                Win32.UnhookWindowsHookEx(hHook);
            }
        }

        #endregion
    }
}
