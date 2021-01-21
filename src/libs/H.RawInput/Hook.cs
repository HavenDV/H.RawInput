using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using H.Hooks.Core.Interop;
using H.Hooks.Core.Interop.WinUser;
using H.Windows;
using RawInput_dll;

namespace H.Hooks
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Hook : IDisposable
    {
        #region Properties

        private RawKeyboard Keyboard { get; set; }

        /// <summary>
        /// If activated, you need to use <see cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/>
        /// when handling events(After set up args.Handled = true).
        /// </summary>
        public bool Handling { get; set; }

        /// <summary>
        /// Returns <see langword="true"/> if thread is started.
        /// </summary>
        public bool IsStarted => Thread != null;

        /// <summary>
        /// 
        /// </summary>
        protected bool PushToThreadPool => !Handling;

        private int Type { get; }
        private Thread? Thread { get; set; }
        private uint ThreadId { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        private void OnExceptionOccurred(Exception value)
        {
            ExceptionOccurred?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public Hook(int type)
        {
            Type = type;
        }

        #endregion

        #region Public methods

        static IntPtr RegisterForDeviceNotifications(IntPtr parent)
        {
            var usbNotifyHandle = IntPtr.Zero;
            var bdi = new BroadcastDeviceInterface();
            bdi.DbccSize = Marshal.SizeOf(bdi);
            bdi.BroadcastDeviceType = BroadcastDeviceType.DBT_DEVTYP_DEVICEINTERFACE;
            bdi.DbccClassguid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");

            var mem = IntPtr.Zero;
            try
            {
                mem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BroadcastDeviceInterface)));
                Marshal.StructureToPtr(bdi, mem, false);
                usbNotifyHandle = Win32.RegisterDeviceNotification(parent, mem, DeviceNotification.DEVICE_NOTIFY_WINDOW_HANDLE);
            }
            catch (Exception e)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
                Debug.Print(e.StackTrace);
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }

            if (usbNotifyHandle == IntPtr.Zero)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
            }

            return usbNotifyHandle;
        }

        /// <summary>
        /// Starts hook thread.
        /// </summary>
        /// <exception cref="Win32Exception">If SetWindowsHookEx return error code</exception>
        public void Start()
        {
            if (Thread != null)
            {
                return;
            }

            Thread = new Thread(() =>
            {
                try
                {
                    ThreadId = Kernel32.GetCurrentThreadId();

                    User32.PeekMessage(
                        out _, 
                        0, 
                        0,
                        0, 
                        PM.NOREMOVE);

                    //Keyboard = new RawKeyboard(false);
                    //Keyboard.EnumerateDevices();
                    //var handle = RegisterForDeviceNotifications(stdHandle);

                    try
                    {
                        while (true)
                        {
                            var result = User32.GetMessage(
                                out var message,
                                0,
                                0,
                                0);
                            if (result == -1)
                            {
                                InteropUtilities.ThrowWin32Exception();
                            }
                            switch (message.msg)
                            {
                                case Win32.WM_INPUT:
                                    {
                                        Console.WriteLine("WM_INPUT");
                                        Keyboard.ProcessRawInput(message.lParam);
                                    }
                                    break;

                                case Win32.WM_USB_DEVICECHANGE:
                                    {
                                        Console.WriteLine("USB Device Arrival / Removal");
                                        Keyboard.EnumerateDevices();
                                    }
                                    break;
                            }

                            if (message.msg == WM.QUIT)
                            {
                                break;
                            }

                            User32.DefWindowProc(message.handle, message.msg, message.wParam, message.lParam);
                        }
                    }
                    finally
                    {
                        //Win32.UnregisterDeviceNotification(handle).Check();
                    }
                }
                catch (Exception exception)
                {
                    OnExceptionOccurred(exception);
                }
            })
            {
                IsBackground = true,
            };
            Thread.Start();
        }

        /// <summary>
        /// Stops hook thread.
        /// </summary>
        public void Stop()
        {
            if (Thread == null)
            {
                return;
            }

            User32.PostThreadMessage(ThreadId, WM.QUIT, 0, 0).Check();
            Thread?.Join();
            Thread = null;
        }

        /// <summary>
        /// Dispose internal system hook resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}