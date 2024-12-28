using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Basler.Pylon;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Object = System.Object;


namespace TipShaping
{
    /// <summary>
    /// Interaction logic for CameraSetting.xaml
    /// </summary>
    public partial class CameraSetting : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        int CAMNum = 3;
        public List<string> cameraSerialNumbers = new List<string> { "21862500", "21806663", "21862541" };
        public List<string> CameraNames = new List<string>() { "BCAM","SCAM1", "SCAM2" };
        private double[] fpsArray = new double[3] { 0, 0, 0 };
        private List<string> FPSCamNames = new List<string>();
        public Dictionary<string, string> ImageToCameraSN = new Dictionary<string, string>();
        private List<string> ConnectedCamerasSerialNumbers = new List<string>();
        System.Timers.Timer fpstimer;
        public static double FrameRate = 10;
        private List<GUICamera> guiCameras = new List<GUICamera>();
        private delegate void ImageReady(Object sender, EventArgs e, GUICamera guicam);


        public CameraSetting()
        {
            
        // Get the screen dimensions
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
           
            double aspectRatio = 1296 / 972 * CAMNum;
            
            
            this.Height = screenHeight/2;
            this.Width = this.Height * aspectRatio;

            // Set the MainWindow position (upper half of the screen)
            this.Left = 0; // Start at the left edge
            this.Top = 0; // Start in the middle of the screen

            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
            this.Loaded += Cameras_Loaded;
            this.Closed += Cameras_Closed;
        }

        private void Cameras_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            SetImagesToDefault();
            try
            {
                FindCameras(Showmsg:true, SetupCams: true);
                if (FPSCamNames.Count != 0) FPS();
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: \n{ex.Message}", "Initilization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void Cameras_Closed(object sender, EventArgs e)
        {
            // this.Loaded -= Cameras_Loaded;
            //this.Closed -= Cameras_Closed;

            // Cancel any ongoing background tasks before window closes
            await Task.Delay(500);
            cancellationTokenSource?.Cancel();
            SaveSettings();
            CloseCameras();
            fpstimer?.Stop();
            fpstimer?.Dispose();
            //fpstimer. -= FPSTimer_Tick;
           
           


        }


        private void SetImagesToDefault()
        {
            Camera1Stream.Source = new BitmapImage(new Uri(@"Images\NoCameraStream.png", UriKind.Relative));
            Camera2Stream.Source = new BitmapImage(new Uri(@"Images\NoCameraStream.png", UriKind.Relative));
            Camera3Stream.Source = new BitmapImage(new Uri(@"Images\NoCameraStream.png", UriKind.Relative));
        }

        private void LoadSettings()
        {
            cameraSerialNumbers = !string.IsNullOrEmpty(Properties.Settings.Default.SavedCamerasSN) ? Properties.Settings.Default.SavedCamerasSN.Split(",").ToList() : new List<string>();
            CameraNames = !string.IsNullOrEmpty(Properties.Settings.Default.CameraOrientation) ? Properties.Settings.Default.CameraOrientation.Split(",").ToList() : new List<string>();
            if (cameraSerialNumbers.Count > 0)
            {
                ImageToCameraSN = cameraSerialNumbers.Zip(CameraNames, (cameraSerialNumber, value) => new { cameraSerialNumber, value }).ToDictionary(item => item.cameraSerialNumber, item => item.value);
            }
        }

        public List<string> FindCameras([Optional] bool Showmsg, bool SetupCams = true)
        {
            List<string> KnownCams = new List<string>();
            List<string> UnKnownCams = new List<string>();
            List<ICameraInfo> cameraInfos = CameraFinder.Enumerate();

            if (cameraInfos.Count > 0)
            {
                ConnectedCamerasSerialNumbers.Clear();

                // obey cameraSerialNumbers' order
                foreach (string serial in cameraSerialNumbers)
                {
                    ICameraInfo matchedCamera = cameraInfos.FirstOrDefault(info => info[CameraInfoKey.SerialNumber] == serial);
                    if (matchedCamera != null)
                    {
                        KnownCams.Add(serial);
                        FPSCamNames.Add(CameraNames[cameraSerialNumbers.IndexOf(serial)]);
                        ConnectedCamerasSerialNumbers.Add(serial);
                    }
                }

                // uknown camera
                foreach (ICameraInfo info in cameraInfos)
                {
                    if (!cameraSerialNumbers.Contains(info[CameraInfoKey.SerialNumber]))
                    {
                        UnKnownCams.Add(info[CameraInfoKey.SerialNumber]);
                        FPSCamNames.Add(info[CameraInfoKey.SerialNumber]);
                        ConnectedCamerasSerialNumbers.Add(info[CameraInfoKey.SerialNumber]);
                    }
                }

                // setupCameras
                if (SetupCams)
                {
                    SetupCameras(KnownCams.Concat(UnKnownCams));
                }

                // show message
                if (Showmsg && guiCameras.Count > 0)
                {
                    Debug.Print("Connected cameras successfully loaded!", "Success!", MessageBoxButton.OK);
                }

                return KnownCams.Concat(UnKnownCams).ToList();
            }

            return null;
        }


        private void OnImageReadyLeft(Object sender, EventArgs e, GUICamera guicam)
        {
            // Check if the cancellation token has been triggered
            if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                return;

            if (!Dispatcher.CheckAccess())
            {
                //Debug.Print("OnImageReadyLeft: Not on UI thread. Marshalling...");
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.Invoke(new Action(() => OnImageReadyLeft(sender, e, guicam)));
                return;
            }
            try
            {
                if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                    return;

                Bitmap newImage = guicam.GetLatestFrame();
                if (newImage != null)
                {
                    BitmapSource Bms = ConvertBitmapToBitmapSource(newImage);
                    Camera1Stream.Source = Bms;
                    newImage.Dispose();
                    fpsArray[0] = fpsArray[0] + 1;
                }
                
            }
            catch (Exception ex)
            {
                Debug.Print(ex.StackTrace);
            }

        }

        private void OnImageReadyMiddle(Object sender, EventArgs e, GUICamera guicam)
        {
            if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                return;

            if (!Dispatcher.CheckAccess())
            {
               // Debug.Print("OnImageReadyLeft: Not on UI thread. Marshalling...");
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.Invoke(new Action(() => OnImageReadyMiddle(sender, e, guicam)));
                return;
            }
            try
            {
                if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                    return;

                Bitmap newImage = guicam.GetLatestFrame();
                if (newImage != null)
                {
                    BitmapSource Bms = ConvertBitmapToBitmapSource(newImage);
                    Camera2Stream.Source = Bms;
                    newImage.Dispose();
                    fpsArray[1] = fpsArray[1] + 1;
                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.StackTrace);
            }

        }


        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void OnImageReadyRight(Object sender, EventArgs e, GUICamera guicam)
        {
            if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                return;

            if (!Dispatcher.CheckAccess())
            {
                
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.BeginInvoke(new Action(() => OnImageReadyRight(sender, e, guicam)));
                return;
            }
            try
            {
                if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                    return;
                //if (guiCameras[1] != null)
                //{
                Bitmap newImage = guicam.GetLatestFrame();
                if (newImage != null)
                {
                    BitmapSource Bms = ConvertBitmapToBitmapSource(newImage);
                    Camera3Stream.Source = Bms;
                    newImage.Dispose();
                    fpsArray[2] = fpsArray[2] + 1;
                }
               
            }
            catch (Exception Ex)
            {
                MessageBox.Show("at catch");
                Debug.Print(Ex.StackTrace);
            }

        }

        public void SetupCameras(IEnumerable<string> Cams)
        {
            CloseCameras();

            guiCameras.Clear();
            List<int> ImageControlsAvailable = new List<int> { 0, 1, 2 }; // 3 cams
            List<ImageReady> imageControlList = new List<ImageReady>()
            {
                OnImageReadyLeft, OnImageReadyMiddle, OnImageReadyRight
            };
            List<string> CameraOrder = CameraNames;

            // Initialize cameras
            foreach (string CamSerial in Cams)
            {
                if (CamSerial == "None") continue;
                GUICamera guicam = new GUICamera();
                try
                {
                    guicam.CreateCameraBySerialNumber(CamSerial);
                    guicam.OpenCamera();
                    guicam.camera.Parameters[PLCamera.AcquisitionFrameRate].SetValue(FrameRate);
                    guiCameras.Add(guicam);
                    //Debug.Print($"Initialized camera: {CamSerial}, total cameras: {guiCameras.Count}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error initializing camera {CamSerial}: {ex.Message}");
                }
            }

            // Set up cameras (3 max)
            for (int i = 0; i < Math.Min(guiCameras.Count, 3); i++)
            {
                int k = i; //i accidently jump to 3
                string Camorder = "None";
                if (Cams != null && Cams.Any())
                {
                    string camSerial = Cams.ElementAtOrDefault(k);
                    if (camSerial != null)
                    {
                        ImageToCameraSN.TryGetValue(camSerial, out Camorder);
                    }
                }
                else
                {
                    Debug.WriteLine("Cams list is empty.");
                }

                // Debugging the camera serial number and corresponding CameraOrder
               //Debug.Print($"Camera {i}: CamSerial={Cams.ElementAtOrDefault(i)}, Camorder={Camorder}, CameraOrder Index={CameraOrder.IndexOf(Camorder)}");

                // Ensure indx is valid within the available range
                int indx = CameraOrder.IndexOf(Camorder) == -1 ? ImageControlsAvailable[0] : CameraOrder.IndexOf(Camorder);

                // Ensure indx is within the bounds of imageControlList
                if (indx < 0 || indx >= imageControlList.Count)
                {
                    Debug.WriteLine($"Invalid index: {indx}, defaulting to 0.");
                    indx = 0; // Default to 0 if the index is invalid
                }

                // Set GUI image location and add event handlers
                guiCameras[k].GUIImageLoc = indx;

                // Safely access imageControlList with a valid index
               // Debug.Print($"i: {i}, indx: {indx}");

                
                guiCameras[k].GuiCameraFrameReadyForDisplay += (Object sender, EventArgs e) =>
                {
                    if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
                        return;
                    //Debug.Print($"Frame ready: i={i}, indx={indx}");
                    Debug.Print($"Frame ready: k={k}, indx={indx}");
                    if (indx >= 0 && indx < imageControlList.Count)
                    {
                        // Check if the cancellation token has been triggered

                        imageControlList[indx].Invoke(sender, e, guiCameras[k]);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid GUIImageLoc index: {indx}, skipping frame processing.");
                    }
                };

                // Remove used index from available list
                if (ImageControlsAvailable.Contains(indx))
                {
                    ImageControlsAvailable.Remove(indx);
                }

                guiCameras[k].GuiCameraConnectionToCameraLost += OnCameraConnectionLost;
            }

            // Start continuous shot grabbing for all cameras
            foreach (GUICamera guicam in guiCameras)
            {
                guicam.StartContinuousShotGrabbing();
            }

            SetImagesToDefault();
        }




        private void OnCameraConnectionLost(object arg1, EventArgs arg2)
        {
            GUICamera cm = (GUICamera)arg1;
            int indx = guiCameras.IndexOf(cm);
            guiCameras.Remove(cm);
            SetImagesToDefault();
            MessageBox.Show($"Connection lost to Camera:{FPSCamNames[indx]}", "Error: Connection Lost", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void FPS()
        {
            fpstimer = new System.Timers.Timer();
            fpstimer.Interval = 1000;
            fpstimer.Elapsed += FPSTimer_Tick;
            fpstimer.Start();
        }

        private async void FPSTimer_Tick(object sender, EventArgs e)
        {
            List<string> CameraOrder = CameraNames;

            // Ensure the correct UI updates using the Dispatcher
            Dispatcher.Invoke(() =>
            {
                // there are 3 cameras (Camera1StreamFPS, Camera2StreamFPS, Camera3StreamFPS)
                List<TextBlock> FpsTextBlocks = new List<TextBlock> { Camera1StreamFPS, Camera2StreamFPS, Camera3StreamFPS };

                // Loop through each camera in guiCameras
                for (int i = 0; i < guiCameras.Count; i++)
                {
                    GUICamera guicam = guiCameras[i];
                    string CamSerial = guicam.camera.CameraInfo[CameraInfoKey.SerialNumber].ToString();

                    // Update the corresponding FPS text block
                    if (cameraSerialNumbers.Contains(CamSerial))
                    {
                        FpsTextBlocks[guicam.GUIImageLoc].Text = $"{CameraOrder[guicam.GUIImageLoc]} {fpsArray[guicam.GUIImageLoc]} fps";
                    }
                    else
                    {
                        FpsTextBlocks[guicam.GUIImageLoc].Text = $"(SN:{CamSerial}) {fpsArray[guicam.GUIImageLoc]} fps";
                    }
                }
            });

            // Reset the FPS values in the array
            for (var i = 0; i < fpsArray.Length; i++)
            {
                fpsArray[i] = 0;
            }
        }


        private void SaveSettings()
        {

            // Save the updated camera serial numbers and names to settings
            Properties.Settings.Default.SavedCamerasSN = string.Join(",", cameraSerialNumbers);
            Properties.Settings.Default.CameraOrientation = string.Join(",", CameraNames);
            Properties.Settings.Default.Save();
        }

        public void CloseCameras()
        {
            List<TextBlock> CameraFPSTextblocks = new List<TextBlock> { Camera1StreamFPS, Camera2StreamFPS, Camera3StreamFPS };
            foreach (GUICamera cam in guiCameras)
            {
                try
                {
                    CameraFPSTextblocks[cam.GUIImageLoc].Text = "0 fps";
                    //cam.CloseCamera();
                    cam.DestroyCamera();
                    Debug.Print($"Camera IsGrabbing: {cam.IsGrabbing}");
                    Debug.Print($"Camera IsOpen: {cam.IsOpen}");
                    Debug.Print($"Camera IsCreated: {cam.IsCreated}");
                }
                catch (Exception ex)
                {
                    Debug.Print($"Error during camera destruction: {ex.Message}");
                }



           }
            
            guiCameras.Clear();

           
        }

        private void OpenCAM1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenCAM2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenCAM3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenCAM4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenAllCAM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseCAM1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseCAM2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseCAM3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseCAM4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseAllCAM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CAM1Lights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CAM2Lights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CAM3Lights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CAM4Lights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AllLights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveCAM1Image_Click(object sender, RoutedEventArgs e)
        {           

            try
            {
                // Check if the Camera1Stream.Source contains an image
                if (Camera1Stream.Source is BitmapSource bitmapSource)
                {
                    // Define the output file path
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string filePath = $"C:\\Users\\ying.cai\\source\\repos\\TipShaping\\CAM1Images\\CAM1_{timestamp}.png";

                    // Save the BitmapSource to a file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("Image saved successfully!");
                }
                else
                {
                    MessageBox.Show("No image available to save.");
                }
            }
            catch (Exception ex)
            {
                // Log or display any errors that occur
                Debug.Print(ex.StackTrace);
                MessageBox.Show($"An error occurred while saving the image: {ex.Message}");
            }

        }

        private void SaveCAM2Image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the Camera1Stream.Source contains an image
                if (Camera2Stream.Source is BitmapSource bitmapSource)
                {
                    // Define the output file path
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string filePath = $"C:\\Users\\ying.cai\\source\\repos\\TipShaping\\CAM2Images\\CAM2_{timestamp}.png";

                    // Save the BitmapSource to a file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("Image saved successfully!");
                }
                else
                {
                    MessageBox.Show("No image available to save.");
                }
            }
            catch (Exception ex)
            {
                // Log or display any errors that occur
                Debug.Print(ex.StackTrace);
                MessageBox.Show($"An error occurred while saving the image: {ex.Message}");
            }

        }

        private void SaveCAM3Image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the Camera1Stream.Source contains an image
                if (Camera3Stream.Source is BitmapSource bitmapSource)
                {
                    // Define the output file path
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string filePath = $"C:\\Users\\ying.cai\\source\\repos\\TipShaping\\CAM3Images\\CAM3_{timestamp}.png";

                    // Save the BitmapSource to a file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("Image saved successfully!");
                }
                else
                {
                    MessageBox.Show("No image available to save.");
                }
            }
            catch (Exception ex)
            {
                // Log or display any errors that occur
                Debug.Print(ex.StackTrace);
                MessageBox.Show($"An error occurred while saving the image: {ex.Message}");
            }

        }

        private void SaveCAM4Image_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveAllImages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FPSSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ViewCrossHair_Click(object sender, RoutedEventArgs e)
        {
            string[] hd = new string[] { "Show Crosshairs", "Hide Crosshairs" };
            Camera1Stream.CrosshairVisibility = !Camera1Stream.CrosshairVisibility;
            Camera2Stream.CrosshairVisibility = !Camera2Stream.CrosshairVisibility;
            Camera3Stream.CrosshairVisibility = !Camera3Stream.CrosshairVisibility;
            MenuItem m = (MenuItem)sender;
            m.Header = hd[Camera1Stream.CrosshairVisibility ? 1 : 0];
            
        }
    }
}