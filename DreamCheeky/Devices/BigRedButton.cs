using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DreamCheeky.Devices
{
    public class BigRedButton
    {
        #region Constant and readonly values
        public HidLibrary.HidDevice HidBtn;
        public const int DefaultVendorId = 0x1D34;  //Default Vendor ID for Dream Cheeky devices
        public const int DefaultProductId = 0x000d; //Default for Big Red Button button
        //Initialization values and test colors
        public static readonly byte[] CmdStatus = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
        #endregion

        private readonly AutoResetEvent _writeEvent = new AutoResetEvent(false);
        private readonly System.Timers.Timer _timer = new System.Timers.Timer(100); //Timer for checking USB status every 100ms
        private Action _timerCallback;
        private bool _lastWriteResult = false;

        #region Constructors
        /// <summary>
        /// Default constructor. Will used VendorID=0x1D34 and ProductID=0x000d. Will throw exception if no device is found.
        /// </summary>
        /// <param name="deviceIndex">Zero based device index if you have multiple devices plugged in.</param>
        public BigRedButton(int deviceIndex = 0) : this(DefaultVendorId, DefaultProductId, deviceIndex) { }

        /// <summary>
        /// Create object using VendorID and ProductID. Will throw exception if no USBLED is found.
        /// </summary>
        /// <param name="vendorId">Example to 0x1D34</param>
        /// <param name="productId">Example to 0x000d</param>
        /// <param name="deviceIndex">Zero based device index if you have multiple devices plugged in.</param>
        public BigRedButton(int vendorId, int productId, int deviceIndex = 0)
        {
            var devices = HidLibrary.HidDevices.Enumerate(vendorId, productId);
            if (deviceIndex >= devices.Count())
            {
                throw new ArgumentOutOfRangeException("deviceIndex", String.Format("DeviceIndex={0} is invalid. There are only {1} devices connected.", deviceIndex, devices.Count()));
            }
            HidBtn = devices.Skip(deviceIndex).FirstOrDefault<HidLibrary.HidDevice>();
            if (!Init())
            {
                throw new Exception(String.Format("Cannot find USB HID Device with VendorID=0x{0:X4} and ProductID=0x{1:X4}", vendorId, productId));
            }
        }

        /// <summary>
        /// Create object using Device path. Example: DreamCheekyBTN(@"\\?\hid#vid_1d34&pid_000d#6&1067c3dc&0&000d#{4d1e55b2-f16f-11cf-88cb-001111000d30}").
        /// </summary>
        /// <param name="devicePath">Example: @"\\?\hid#vid_1d34&pid_000d#6&1067c3dc&0&000d#{4d1e55b2-f16f-11cf-88cb-001111000030}"</param>
        public BigRedButton(string devicePath)
        {
            HidBtn = HidLibrary.HidDevices.GetDevice(devicePath);
            if (!Init())
            {
                throw new Exception(String.Format("Cannot find USB HID Device with DevicePath={0}", devicePath));
            }
        }

        /// <summary>
        /// Private init function for constructors.
        /// </summary>
        /// <returns>True if success, false otherwise.</returns>
        private bool Init()
        {
            this._writeEvent.Reset();
            _timer.AutoReset = true;
            _timer.Elapsed += t_Elapsed;
            _timer.Enabled = false;
            _timer.Stop();
            if (HidBtn == default(HidLibrary.HidDevice))
            {
                return false; //Device not found, return false.
            }
            else //Device is valid
            {
                Trace.WriteLine("Init HID device: " + HidBtn.Description + "\r\n");
                return true;
            }
        }

        ~BigRedButton()
        {
            _timer.Dispose();
            _timerCallback = null;

            if (HidBtn != null)
            {
                HidBtn.Dispose();
            }
        }
        #endregion

        public bool ButtonState { get; private set; }

        public async Task<bool> Write(byte[] data)
        {
            //Trace.WriteLine("\r\nWriteing Data=" + BitConverter.ToString(data));
            return await HidBtn.WriteAsync(data, 80);
        }

        public HidLibrary.HidDeviceData Read()
        {
            return HidBtn.Read();
        }

        public async Task<bool> GetStatus()
        {
            if (await Write(CmdStatus))
            {
                var data = Read();
                if (data.Data[1] == 0x16) // button is active
                {
                    return true;
                }
                else
                {
                    // data.Data[1] == 0x17 // button is inactive
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("Status CMD failed...");
                return false;
            }
        }

        public void RegisterCallback(Action callback)
        {
            _timerCallback = callback;
            _timer.Enabled = true;
            _timer.Start();
        }

        async void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (await GetStatus())
            {
                if (!ButtonState) //Only toggle if button not already set
                {
                    ButtonState = true;
                    _timerCallback();
                }
            }
            else
            {
                ButtonState = false; //Reset state
            }
        }
    }
}

