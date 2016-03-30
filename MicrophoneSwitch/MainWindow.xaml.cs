using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace MicrophoneSwitch
{
    public class Win32
    {

        public static bool IsEnabled { get; set; }

        public const UInt32 WM_KEYDOWN = 0x0100;
        public const int VK_1 = 0x31;
        public const int VK_2 = 0x32;
        public const int VK_3 = 0x33;
        public const int VK_4 = 0x34;
        public const int VK_5 = 0x35;
        public const int VK_6 = 0x36;
        public const int VK_7 = 0x37;
        public const int VK_8 = 0x38;
        public const int VK_9 = 0x38;
        public const int VK_A = 0x41;
        public const int VK_B = 0x42;

        public static int NumberToVK(int number)
        {
            switch (number)
            {
                case 1:
                    return VK_1;
                case 2:
                    return VK_2;
                case 3:
                    return VK_3;
                case 4:
                    return VK_4;
                case 5:
                    return VK_5;
                case 6:
                    return VK_6;
                case 7:
                    return VK_7;
                case 8:
                    return VK_8;
                case 9:
                    return VK_9;
                default:
                    throw new ArgumentException("Not a valid number");
            }
        }

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("User32.dll")]
        public static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<IntPtr> _wirecastWindows = new List<IntPtr>();
        
        public MainWindow()
        { 
            InitializeComponent();
            
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            try
            {
                Process[] wirecastApps = Process.GetProcessesByName("wirecast");
                if (wirecastApps.Count() > 0 && wirecastApps[0] != null)
                {
                    IntPtr mainWindowHandle = wirecastApps[0].MainWindowHandle;
                    IntPtr currChild = Win32.FindWindowEx(mainWindowHandle, IntPtr.Zero, null, null);
                    while (currChild != IntPtr.Zero)
                    {
                        currChild = Win32.FindWindowEx(mainWindowHandle, currChild, null, null);

                        int capacity = Win32.GetWindowTextLength(new HandleRef(this, currChild)) * 2;
                        if (capacity > 0)
                        {
                            StringBuilder stringBuilder = new StringBuilder(capacity);
                            Win32.GetWindowText(new HandleRef(this, currChild), stringBuilder, stringBuilder.Capacity);
                            if (stringBuilder.ToString().ToLower().Contains("wirecast"))
                            {
                                string windowTitle = stringBuilder.ToString();
                                _wirecastWindowComboBox.Items.Add(windowTitle);
                                _wirecastWindows.Add(currChild);
                            }
                        }
                    }
                }

                if (_wirecastWindows.Count == 0)
                {
                    _mainPanel.Children.Add(new TextBlock() { Text = "Did not detect any running instances of Wirecast. Please Close this application, launch Wirecast, and re-open.", TextWrapping = TextWrapping.Wrap });
                }
                else
                {
                    _wirecastWindowComboBox.SelectedIndex = 0;
                    HotkeyManager.SetAppWindowHandle(_wirecastWindows[0]);
                }

                List<string> micNames = new List<string>();
                for (int n = 0; n < WaveIn.DeviceCount; n++)
                {
                    MicrophoneViewModel micVM = new MicrophoneViewModel(n);
                    _micContainer.Children.Add(new MicrophoneView() { DataContext = micVM });
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Sorry, an error occurred during startup. More information: " + exc.Message);
                MessageBox.Show(exc.InnerException.Message);                
            }
        }

        private void _wirecastWindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HotkeyManager.SetAppWindowHandle(_wirecastWindows[_wirecastWindowComboBox.SelectedIndex]);
        }

        private void _switchDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HotkeyManager.SetDelaySeconds(_switchDelaySlider.Value);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HotkeyManager.SetNoiseReduction(false);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HotkeyManager.SetNoiseReduction(true);
        }

        #region Handle Global Hotkey stuff

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint VK_F10 = 0x79;
            const uint MOD_CTRL = 0x0002;
            if (!Win32.RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            Win32.UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            _micEnabledCheckbox.IsChecked = !_micEnabledCheckbox.IsChecked;
        }

        #endregion Handle global hotkey stuff

        private void _micEnabledCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Win32.IsEnabled = true;
        }

        private void _micEnabledCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Win32.IsEnabled = false;
        }
    }
}
