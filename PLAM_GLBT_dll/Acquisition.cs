using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using GBMSAPI_NET;
using GBMSAPI_NET.GBMSAPI_NET_Defines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_AcquisitionProcessDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLEDsDefines;
using GBMSGUI_NET;
using GBMSAPI_CS_Example.UTILITY;


namespace SC_PLAM_GLBT_DLL
{
    public class CaptureUtils
    {
        public static int GBMSAPI_ExampleFrCount = 0;
        public static void SaveInBmp(string FileName, byte[] FrameArray, uint SX, uint SY)
        {
            /////////////////////////////////////////////
            // ALLOCATE BITMAP AND ITS INTERNAL FRAME
            /////////////////////////////////////////////
            IntPtr FramePtr = Marshal.AllocHGlobal((int)(SX * SY));

            if (FramePtr == IntPtr.Zero)
            {
                Console.WriteLine("Cannot allocate memory: press any key to close the application");
                return;
            }
            Bitmap bmp = new Bitmap((int)SX, (int)SY, (int)SX, PixelFormat.Format8bppIndexed, FramePtr);
            if (bmp == null)
            {
                Console.WriteLine("Cannot allocate memory: press any key to close the application");
                return;
            }
            ColorPalette pal = bmp.Palette;
            for (int i = 0; i < pal.Entries.Length; i++)
            {
                pal.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            bmp.Palette = pal;

            /////////////////////////////////////////////////////////
            // FILL BMP INTERNAL FRAME AND SAVE
            /////////////////////////////////////////////////////////
            Marshal.Copy(FrameArray, 0, FramePtr, (int)(SX * SY));
            bmp.Save(FileName, ImageFormat.Bmp);
            Marshal.FreeHGlobal(FramePtr);
        }

        public static int AcquisitionCallback(
            uint OccurredEventCode,
            int GetFrameErrorCode,
            uint EventInfo,
            byte[] FramePtr,
            int FrameSizeX,
            int FrameSizeY,
            double CurrentFrameRate,
            double NominalFrameRate,
            uint GB_Diagnostic,
            System.IntPtr UserDefinedParameters
            )
        {
            try
            {
                ////////////////
                // check error
                ////////////////
                if (GetFrameErrorCode != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    CaptureGlobals.LastErrorCode = GetFrameErrorCode;
                    Console.WriteLine("AcquisitionCallback: GetFrameErrorCode = " + GetFrameErrorCode);
                    CaptureGlobals.AcquisitionEnded = true;
                    return 1;
                }
              
                //////////////////////////////
                // check event:
                // ERROR EVENT ALREADY CHECKED
                //////////////////////////////
                switch (OccurredEventCode)
                {
                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_SCANNER_STARTED:
                        {
                            /////////
                            // ....
                            /////////
                            if (CaptureGlobals.ClippingRegionSizeX > 0 && CaptureGlobals.ClippingRegionSizeY > 0)
                            {
                                int RetVal = GBMSAPI_NET_ScannerStartedRoutines.GBMSAPI_NET_SetClippingRegionSize(CaptureGlobals.ClippingRegionSizeX, CaptureGlobals.ClippingRegionSizeY);
                                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                {
                                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                    CaptureGlobals.LastErrorCode = RetVal;
                                    return 0;
                                }
                            }
                            break;
                        }

                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_PREVIEW_PHASE_END:
                        {
                            // Sound to advise operator that the preview has ended
                            if ((CaptureGlobals.ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0)
                            {
                                Console.WriteLine("END PREVIEW");
                                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(12, 2, 1);
                            }
                            else
                            {
                                //Console.Beep(4000, 200);
                                CaptureGlobals.DSBeep(4000, 200);
                            }
                            break;
                        }

                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_VALID_FRAME_ACQUIRED:
                        {
                            CaptureGlobals.FrameNumber++;

                            // check diagnostic
                            if (GB_Diagnostic != 0 && 
                                GB_Diagnostic != GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_LEFT &&
                                GB_Diagnostic != GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_RIGHT)
                            {
                                Console.WriteLine("DIAGNOSTIC: " + GB_Diagnostic);
                            }
                            if (!CaptureGlobals.FirstFrameAcquired)
                            {
                                CaptureGlobals.FirstFrameAcquired = true;
                                Console.WriteLine();
                                Console.WriteLine("PUT THE OBJECT ON THE SCANNER");
                                Console.WriteLine();
                            }

                            CaptureGlobals.LastFrameSizeX = FrameSizeX;
                            CaptureGlobals.LastFrameSizeY = FrameSizeY;

                             int RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetClippingRegionPosition(
                                out CaptureGlobals.ClippingRegionPosX,
                                out CaptureGlobals.ClippingRegionPosY,
                                out CaptureGlobals.ClippingRegionSizeX,
                                out CaptureGlobals.ClippingRegionSizeY
                            );
                            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                            {
                                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                CaptureGlobals.LastErrorCode = RetVal;
                                return 0;
                            }

                            if (
                             !GBMSGUI.IsRolled(CaptureGlobals.ObjectName) &&
                             CaptureGlobals.ClippingRegionPosX >= 0 &&
                             CaptureGlobals.ClippingRegionPosY >= 0 &&
                             CaptureGlobals.ClippingRegionSizeX != 0 &&
                             CaptureGlobals.ClippingRegionSizeY != 0
                             )
                            {
                                FramePtr = CutImage(
                                    FramePtr,
                                    FrameSizeX, FrameSizeY,
                                    CaptureGlobals.ClippingRegionPosX, CaptureGlobals.ClippingRegionPosY,
                                    (int)CaptureGlobals.ClippingRegionSizeX, (int)CaptureGlobals.ClippingRegionSizeY);
                            }

                            if (FrameSizeX != CaptureGlobals.PreviewImageSizeX || FrameSizeY != CaptureGlobals.PreviewImageSizeY)
                            {
                                CaptureGlobals.PreviewImage = new Bitmap(FrameSizeX, FrameSizeX, PixelFormat.Format8bppIndexed);
                                GBMSGUI.SetGrayPalette(ref CaptureGlobals.PreviewImage);
                                CaptureGlobals.FullResolutionImage = new Bitmap(FrameSizeX, FrameSizeX, PixelFormat.Format8bppIndexed);
                                GBMSGUI.SetGrayPalette(ref CaptureGlobals.FullResolutionImage);
                            }

                            GBMSGUI.CopyRawImageIntoBitmap(FramePtr, ref CaptureGlobals.PreviewImage);

                            // Get contrast and size: if not supported, they keep their value at 0,
                            // without checking return value
                            CaptureGlobals.ImageContrast = 0;
                            CaptureGlobals.ImageSize = 0;
                            GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintContrast(
                                out CaptureGlobals.ImageContrast);

                            GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintSize(
                                out CaptureGlobals.ImageSize);
                            CaptureGlobals.LastDiagnosticValue = GB_Diagnostic;

                            if (CaptureGlobals.FigGBMS != null)
                            {
                                CaptureGlobals.FigGBMS.Invoke((Action)delegate { CaptureGlobals.FigGBMS.FrameAcquired(); });
                            }

                            GBMSAPI_Example_Globals.LastFrameSizeX = FrameSizeX;
                            GBMSAPI_Example_Globals.LastFrameSizeY = FrameSizeY;
                            GBMSAPI_Example_Globals.LastFrameCurrentFrameRate = CurrentFrameRate;
                            GBMSAPI_Example_Globals.LastFrameNominalFrameRate = NominalFrameRate;
                            GBMSAPI_Example_Globals.LastDiagnosticValue = GB_Diagnostic;
                            break;
                        }

                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_ACQUISITION_END:
                        {

                            // Sound to advise operator that the fingerprint can be
                            // released
                            if ((CaptureGlobals.ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0)
                            {
                                Console.WriteLine("Calling GBMSAPI_NET_Sound at the end of acquisition");
                                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(12, 4, 1);
                            }
                            else
                            {
                                //Console.Beep(4000, 200);
                                CaptureGlobals.DSBeep(4000, 200);
                            }

                            int RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetClippingRegionPosition(
                                out CaptureGlobals.ClippingRegionPosX,
                                out CaptureGlobals.ClippingRegionPosY,
                                out CaptureGlobals.ClippingRegionSizeX,
                                out CaptureGlobals.ClippingRegionSizeY
                            );
                            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                            {
                                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                CaptureGlobals.LastErrorCode = RetVal;
                                return 0;
                            }
                            /******************指纹采集状体提示***********************/
                            GBMSAPI_Example_Globals.LastFrameSizeX = FrameSizeX;
                            GBMSAPI_Example_Globals.LastFrameSizeY = FrameSizeY;
                            GBMSAPI_Example_Globals.LastFrameCurrentFrameRate = CurrentFrameRate;
                            GBMSAPI_Example_Globals.LastFrameNominalFrameRate = NominalFrameRate;
                            GBMSAPI_Example_Globals.LastDiagnosticValue = GB_Diagnostic;

                            // finalize image
                           GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_ImageFinalization(FramePtr);

                            if (
                             CaptureGlobals.ClippingRegionPosX >= 0 &&
                             CaptureGlobals.ClippingRegionPosY >= 0 &&
                             CaptureGlobals.ClippingRegionSizeX != 0 &&
                             CaptureGlobals.ClippingRegionSizeY != 0
                             )
                            {
                                FramePtr = CutImage(
                                    FramePtr,
                                    FrameSizeX, FrameSizeY,
                                    CaptureGlobals.ClippingRegionPosX, CaptureGlobals.ClippingRegionPosY,
                                    (int)CaptureGlobals.ClippingRegionSizeX, (int)CaptureGlobals.ClippingRegionSizeY);
                            }
                            // copy frame
                            GBMSGUI.CopyRawImageIntoBitmap(FramePtr, ref CaptureGlobals.FullResolutionImage);
                            CaptureGlobals.FullResolutionSizeX = FrameSizeX;
                            CaptureGlobals.FullResolutionSizeY = FrameSizeY;

                            // diagnostic
                            CaptureGlobals.LastDiagnosticValue = GB_Diagnostic;

                            // Get contrast and size: if not supported, they keep their value at 0,
                            // without checking return value
                            CaptureGlobals.ImageContrast = 0;
                            CaptureGlobals.ImageSize = 0;
                            GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintContrast(
                                out CaptureGlobals.ImageContrast);

                            GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintSize(
                                out CaptureGlobals.ImageSize);
                           
                            // ver 4.0.0.0: fake fingerprint
                            uint devFeatures = 0;
                            RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out devFeatures);
                            if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                            {
                                if ((devFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_HW_ANTIFAKE) != 0)
                                {
                                    bool isFake;
                                    uint fakeDiagnostic;
                                    RetVal = GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_HardwareFakeFingerDetection(out isFake, out fakeDiagnostic);
                                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_UNAVAILABLE_OPTION)
                                    {
                                        Console.WriteLine("HW FFD RESULT NOT AVAILABLE (acquisition manually stopped)");
                                    }
                                    else if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        Console.WriteLine("Error in GBMSAPI_NET_HardwareFakeFingerDetection: " + RetVal);
                                    }
                                    else
                                    {
                                        Console.WriteLine("GBMSAPI_HardwareFakeFingerDetection: isFake = " + isFake +
                                            "fakeDiagnostic = 0X" + fakeDiagnostic.ToString("X"));
                                    }
                                }
                                if ((devFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_SW_ANTIFAKE) != 0)
                                {
                                    bool isFake;
                                    RetVal = GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_SoftwareFakeFingerDetection(FramePtr,
                                        FrameSizeX, FrameSizeY,
                                        out isFake);
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        Console.WriteLine("Error in GBMSAPI_NET_SoftwareFakeFingerDetection: " + RetVal);
                                    }
                                    else
                                    {
                                        Console.WriteLine("GBMSAPI_NET_SoftwareFakeFingerDetection: isFake = " + isFake);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("GBMSAPI_NET_GetDeviceFeatures returned: " + RetVal);
                            }
                            // end ver 4.0.0.0: fake fingerprint

                            CaptureGlobals.AcquisitionEnded = true;

                            if (CaptureGlobals.FigGBMS != null)
                            {
                                CaptureGlobals.FigGBMS.Invoke((Action)delegate { CaptureGlobals.FigGBMS.AcquisitionEnded(); });
                            }
                            break;
                        }
                }


                return 1;
            }
            catch (Exception ex)
            {
                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                Console.WriteLine("Exception in acquisitioncallback: " + ex.Message);
                return 0;
            }
        }


        private static byte[] CutImage(
            Byte[] FramePtr,
            int OrigSizeX, int OrigSizeY,
            int ClPosX, int ClPosY,
            int ClSizeX, int ClSizeY)
        {
            //VER3000 check that size x is divisible by 4 
            if ((ClSizeX & (int)0x03) != 0)
            {
                ClSizeX -= (ClSizeX & (int)0x03);
            }
            Byte[] ClippedFrame = new byte[ClSizeX * ClSizeY];
            int ClippedFrameIndex = 0;
            int minx = ClPosX, maxx = ClPosX + ClSizeX - 1;
            int miny = ClPosY, maxy = ClPosY + ClSizeY - 1;

            for (int y = miny; y <= maxy; y++)
            {
                if (y < OrigSizeY)
                {
                    for (int x = minx; x <= maxx; x++)
                    {
                        if (x < OrigSizeX)
                        {
                            ClippedFrame[ClippedFrameIndex++] = FramePtr[y * OrigSizeX + x];
                        }
                        else x = maxx + 1;
                    }
                }
                else y = maxy + 1;
            }

            return ClippedFrame;
        }

        public static void ResetCaptureGlobals()
        {
            CaptureGlobals.AcquisitionEnded = false;
            CaptureGlobals.FrameNumber = 0;
            CaptureGlobals.ImageContrast = 0;
            CaptureGlobals.ImageSize = 0;
            CaptureGlobals.LastDiagnosticValue = 0;
            CaptureGlobals.LastErrorCode = GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR;
            CaptureGlobals.FirstFrameAcquired = false;
        }
    }
    public class CaptureGlobals
    {
        public static uint ObjectName;
        public static Bitmap PreviewImage;
        public static Bitmap FullResolutionImage;

        public static uint PreviewImageSizeX, PreviewImageSizeY;
        public static uint MaxImageSizeX, MaxImageSizeY;
        public static int FullResolutionSizeX, FullResolutionSizeY;


        public static bool AcquisitionEnded;
        public static uint LastDiagnosticValue;
        public static uint ImageSize;
        public static byte ImageContrast;
        public static int FrameNumber;
        public static int LastErrorCode;
        public static uint ExternalEquipment;
        public static bool FirstFrameAcquired;
        public static PLAM_GLBT FigGBMS;
        public static uint ClippingRegionSizeX, ClippingRegionSizeY;
        public static int ClippingRegionPosX, ClippingRegionPosY;
        public static int LastFrameSizeX, LastFrameSizeY;

        /**************************************
         * DSInit
        **************************************/
        public const uint COINIT_APARTMENTTHREADED = (uint)(0x02);
        //[DllImport("DSBeep.dll", EntryPoint = "DSInit")]
        //public static extern bool DSInit(uint CoInit, bool ForceDS);
        public static bool DSInit(uint x, bool y)
        {
            return true;
        }

        /**************************************
         * DSInit
        **************************************/
        //[DllImport("DSBeep.dll", EntryPoint = "DSBeep")]
        //public static extern bool DSBeep(int Frequency, int Duration);
        public static bool DSBeep(int Frequency, int Duration)
        {
            return true;
        }
    }
    class Acquisition
    {
        public static uint SupportedScanOptions;
        public static uint ScanOptions;
        public static uint ScannedObjectID;

        /// <summary>
        /// This function captures the object whose name is
        /// passed as parameter
        /// </summary>
        /// <param name="ObjectName">
        /// Objetc to be captured
        /// </param>
        /// <returns>
        /// see GBMSAPI_NET_ErrorCodes
        /// </returns>
        public static int CaptureObject(uint ObjectName)
        {
            Initialize(ScannedObjectID = ObjectName);

            ///////////////////
            // check parameters
            ///////////////////
            uint ObjType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(ObjectName);
            // check if supported
            uint SupportedObjMask;
            GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetScannableTypes(out SupportedObjMask);

            if ((ObjType & SupportedObjMask) == 0)
            {
                Console.WriteLine("Object not supported");
                return GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_PARAMETER;
            }

            // ver 4.0.0.0: fake fingerprint
            uint devFeatures = 0;
            int RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out devFeatures);
            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                Console.WriteLine("GBMSAPI_NET_GetDeviceFeatures returned: " + RetVal);
                return RetVal;
            }
            if ((devFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_HW_ANTIFAKE) != 0)
            {
                Console.WriteLine("Scanner supports HW antifake");
                RetVal = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_SetHardwareFakeFingerDetectionThreshold(99);
                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    Console.WriteLine("Error in GBMSAPI_NET_SetHardwareFakeFingerDetectionThreshold: " + RetVal);
                    return RetVal;
                }
            }
            else
            {
                Console.WriteLine("Scanner does not support HW antifake");
            }
            if ((devFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_SW_ANTIFAKE) != 0)
            {
                Console.WriteLine("Scanner supports SW antifake");
                RetVal = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_SetSoftwareFakeFingerDetectionThreshold(50);
                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    Console.WriteLine("Error in GBMSAPI_NET_SetSoftwareFakeFingerDetectionThreshold: " + RetVal);
                    return RetVal;
                }
            }
            else
            {
                Console.WriteLine("Scanner does not support SW antifake");
            }
            // end ver 4.0.0.0: fake fingerprint

            GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetOptionalExternalEquipment(out CaptureGlobals.ExternalEquipment);

            CaptureUtils.ResetCaptureGlobals();

            Console.WriteLine("Starting Acquisition for object");

            // Set autocapture: if not supported it's not considered by the
            // GBMSAPI library
             RetVal = GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StartAcquisition(
                                    ObjectName,
                                    GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_AUTOCAPTURE | GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_ADAPT_ROLL_AREA_POSITION,
                                    CaptureUtils.AcquisitionCallback,
                                    IntPtr.Zero,
                                    0,
                                    0,
                                    0
                                    );
            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                Console.WriteLine("GBMSAPI_NET_StartAcquisition returned: " + RetVal);
                return RetVal;
            }
            // enter a cycle waiting for the capture
            int FrameNumber = 0;
            Console.WriteLine("Acquisition Started");
            // VER 4.2
            long ReferenceMs = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            // end VER 4.2
            while (!CaptureGlobals.AcquisitionEnded)
            {
                //////////////////////////////////////////////////
                // VERY IMPORTANT TO CALL THE DoEvents FUNCTION,
                // OTHERWISE OS MESSAGES ARE NOT PROCESSED!!!!!
                //////////////////////////////////////////////////
                Application.DoEvents();

                if (FrameNumber != CaptureGlobals.FrameNumber)
                {
                    // a new frame has been acquired
                    FrameNumber = CaptureGlobals.FrameNumber;
                }
                // VER 4.2
                long ActualMs = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                if ((ActualMs - ReferenceMs) >= 5000 &&
                    ((ObjType & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER) == 0)
                    )
                {
                    // Manually stop capture
                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                }
                // end VER 4.2
            }
            //for (int i = 0; i < 500; i++)
            //{
            //    Application.DoEvents();
            //    Thread.Sleep(1);
            //}
            Console.WriteLine("Acquisition Ended for object. Result:");
            if (CaptureGlobals.LastErrorCode != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                Console.WriteLine("Error: " + CaptureGlobals.LastErrorCode);
            }
            else
            {
                Console.WriteLine("Acquisition ok");
            }

            return CaptureGlobals.LastErrorCode;
        }

        public static void Initialize(uint ObjectName)
        {
            try
            {
                int RetVal;
                ////////////////////////////////////////////////////
                // CHECK SCANNER PRESENCE
                ////////////////////////////////////////////////////
                //GBMSAPI_NET_DeviceInfoStruct[] AttachedDeviceList;
                //int AttachedDeviceNumber;
                //uint USBErrorCode, ScannableTypesMask;

                //AttachedDeviceList = new GBMSAPI_NET_DeviceInfoStruct[GBMSAPI_NET_DeviceInfoConstants.GBMSAPI_NET_MAX_PLUGGED_DEVICE_NUM];

                //if (AttachedDeviceList == null)
                //{
                //    Console.WriteLine("Cannot allocate memory: press any key to close the application");
                //    Console.Read();
                //    return;
                //}
                //for (int i = 0; i < GBMSAPI_NET_DeviceInfoConstants.GBMSAPI_NET_MAX_PLUGGED_DEVICE_NUM; i++)
                //{
                //    AttachedDeviceList[i] = new GBMSAPI_NET_DeviceInfoStruct();
                //    if (AttachedDeviceList[i] == null)
                //    {
                //        Console.WriteLine("Cannot allocate memory: press any key to close the application");
                //        Console.Read();
                //        return;
                //    }
                //}
                //int RetVal = GBMSAPI_NET_DeviceSettingRoutines.GBMSAPI_NET_GetAttachedDeviceList(
                //    AttachedDeviceList, out AttachedDeviceNumber, out USBErrorCode);
                //if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                //{
                //    Console.WriteLine("Error in GBMSAPI_NET_GetAttachedDeviceList: " + RetVal);
                //    Console.WriteLine("Press any key to close the application");
                //    Console.Read();
                //    return;
                //}
                //if (AttachedDeviceNumber == 0)
                //{
                //    Console.WriteLine("No scanners detected");
                //    Console.WriteLine("Press any key to close the application");
                //    Console.Read();
                //    return;
                //}

                ////////////////////////////////////////////////////
                // SET DEVICE TO BE USED
                ////////////////////////////////////////////////////
                /***************************************************
                 * Here the first device found will be used.
                 * If a specified device (discriminated by type, or
                 * serial number, or whatever else) should be
                 * used, put your code here.
                ***************************************************/
                //RetVal = GBMSAPI_NET_DeviceSettingRoutines.GBMSAPI_NET_SetCurrentDevice(
                //    AttachedDeviceList[0].DeviceID, AttachedDeviceList[0].DeviceSerialNumber);
                //if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                //{
                //    Console.WriteLine("Error in GBMSAPI_NET_SetCurrentDevice: " + RetVal);
                //    Console.WriteLine("Press any key to close the application");
                //    Console.Read();
                //    return;
                //}

                //RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetSupportedScanOptions(out SupportedScanOptions);
                //if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                //{
                //    Console.WriteLine("Error in GBMSAPI_NET_GetSupportedScanOptions: " + RetVal);
                //    return;
                //}

                ////////////////////////////////////////////////////
                // ALLOCATE MEMORY
                ////////////////////////////////////////////////////
                uint ScanArea = SetScanArea(ObjectName);
                SetScanningOptions(SupportedScanOptions);

                RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetImageSize3(GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(ObjectName), ScanOptions, ScanArea,
                    out CaptureGlobals.MaxImageSizeX, out CaptureGlobals.MaxImageSizeY, out CaptureGlobals.PreviewImageSizeX, out CaptureGlobals.PreviewImageSizeY);
                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    Console.WriteLine("Error in GBMSAPI_NET_GetMaxImageSize: " + RetVal);
                    Console.WriteLine("Press any key to close the application");
                    Console.Read();
                    return;
                }

                if (!GBMSGUI.IsRolled(ObjectName) && CaptureGlobals.ClippingRegionSizeX > 0 && CaptureGlobals.ClippingRegionSizeY > 0)
                {
                    CaptureGlobals.PreviewImage = new Bitmap((int)CaptureGlobals.ClippingRegionSizeX / 4, (int)CaptureGlobals.ClippingRegionSizeY / 4, PixelFormat.Format8bppIndexed);
                }
                else
                {
                    CaptureGlobals.PreviewImage = new Bitmap((int)CaptureGlobals.PreviewImageSizeX, (int)CaptureGlobals.PreviewImageSizeY, PixelFormat.Format8bppIndexed);
                }

                if (CaptureGlobals.PreviewImage == null)
                {
                    Console.WriteLine("Cannot allocate memory: press any key to close the application");
                    Console.Read();
                    return;
                }
                GBMSGUI.SetGrayPalette(ref CaptureGlobals.PreviewImage);

                if (!GBMSGUI.IsRolled(ObjectName) && CaptureGlobals.ClippingRegionSizeX > 0 && CaptureGlobals.ClippingRegionSizeY > 0)
                {
                    CaptureGlobals.FullResolutionImage = new Bitmap((int)CaptureGlobals.ClippingRegionSizeX, (int)CaptureGlobals.ClippingRegionSizeY, PixelFormat.Format8bppIndexed);
                }
                else
                {
                    CaptureGlobals.FullResolutionImage = new Bitmap((int)CaptureGlobals.MaxImageSizeX, (int)CaptureGlobals.MaxImageSizeY, PixelFormat.Format8bppIndexed);
                }

                if (CaptureGlobals.FullResolutionImage == null)
                {
                    Console.WriteLine("Cannot allocate memory: press any key to close the application");
                    Console.Read();
                    return;
                }
                GBMSGUI.SetGrayPalette(ref CaptureGlobals.FullResolutionImage);
                return;

                //////////////////////////////////////////////////
                // GET SUPPORTED OBJECTS
                //////////////////////////////////////////////////
                //RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetScannableTypes(
                //    out ScannableTypesMask);
                //if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                //{
                //    Console.WriteLine("Error in GBMSAPI_NET_GetScannableTypes: " + RetVal);
                //    Console.WriteLine("Press any key to close the application");
                //    return;
                //}

                //////////////////////////////////////////////////////
                // Check scanner internal beeper
                //////////////////////////////////////////////////////
                //RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetOptionalExternalEquipment(
                //    out CaptureGlobals.ExternalEquipment);
                //if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                //{
                //    Console.WriteLine("Error in GBMSAPI_NET_GetOptionalExternalEquipment: " + RetVal);
                //    Console.WriteLine("Press any key to close the application");
                //    return;
                //}
                //CaptureGlobals.DSInit(CaptureGlobals.COINIT_APARTMENTTHREADED, false);

                /*****************************************************
                 * Here developer can change settings in order to
                 * fit his needs (that means, what object he wants
                 * to be captured)
                *****************************************************/
                //////////////////////////////////////////////////////
                // ACQUIRE PALM
                //////////////////////////////////////////////////////
                /*
                if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("      ACQUIRING PALM");
                    Console.WriteLine("-----------------------------------------");
                    RetVal = CaptureObject(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_LEFT);
                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        // SAVE IN BMP
                        CaptureUtils.SaveInBmp("PALM_IMAGE.bmp", CaptureGlobals.FullResolutionImage,
                            CaptureGlobals.FullResolutionSizeX, CaptureGlobals.FullResolutionSizeY);
                    }
                }
                //////////////////////////////////////////////////////
                // ACQUIRE SLAP-4
                //////////////////////////////////////////////////////
                if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_4) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("       ACQUIRING SLAP-4");
                    Console.WriteLine("-----------------------------------------");
                    RetVal = CaptureObject(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT);
                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        // SAVE IN BMP
                        CaptureUtils.SaveInBmp("SLAP_4_IMAGE.bmp", CaptureGlobals.FullResolutionImage,
                            CaptureGlobals.FullResolutionSizeX, CaptureGlobals.FullResolutionSizeY);
                    }
                }
                
                //////////////////////////////////////////////////////
                // ACQUIRE THHUMBS-2
                //////////////////////////////////////////////////////
                if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_THUMBS_2) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("       ACQUIRING THUMBS-2");
                    Console.WriteLine("-----------------------------------------");
                    RetVal = CaptureObject(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS);
                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        // SAVE IN BMP
                        CaptureUtils.SaveInBmp("THUMBS_2_IMAGE.bmp", CaptureGlobals.FullResolutionImage,
                            CaptureGlobals.FullResolutionSizeX, CaptureGlobals.FullResolutionSizeY);
                    }
                }
                //////////////////////////////////////////////////////
                // ACQUIRE SINGLE FLAT FINGER
                //////////////////////////////////////////////////////
                if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SINGLE_FINGER) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("       ACQUIRING SINGLE FLAT FINGER");
                    Console.WriteLine("-----------------------------------------");
                    RetVal = CaptureObject(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX);
                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        // SAVE IN BMP
                        CaptureUtils.SaveInBmp("FLAT_SINGLE_IMAGE.bmp", CaptureGlobals.FullResolutionImage,
                            CaptureGlobals.FullResolutionSizeX, CaptureGlobals.FullResolutionSizeY);
                    }
                }
                //////////////////////////////////////////////////////
                // ACQUIRE ROLL FINGER
                //////////////////////////////////////////////////////
                if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("       ACQUIRING ROLL FINGER");
                    Console.WriteLine("-----------------------------------------");
                    RetVal = CaptureObject(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX);
                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        // SAVE IN BMP
                        CaptureUtils.SaveInBmp("ROLL_SINGLE_IMAGE.bmp", CaptureGlobals.FullResolutionImage,
                            CaptureGlobals.FullResolutionSizeX, CaptureGlobals.FullResolutionSizeY);
                    }
                }
                
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in main: " + ex.Message);
            }

            Console.Read();
        }

        private static void SetScanningOptions(uint SupportedOptions)
        {
            uint AcquisitionOptions = 2114039;
            ScanOptions = 0;

            // 1.12.0.0
            // here do only set od ScanOptions; display is moved in ShowScanningOptions

            // verify that requested options are available; if not, show a warning
            if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.FullResPreview))
            {
                if (GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FULL_RES_PREVIEW))
                    ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FULL_RES_PREVIEW;
                //else
                //    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_Full_Res_Preview"), "Warning.bmp");
            }

            // Autocapture only for flat (for Rolled is always active)
            if (!GBMSGUI.IsRolled(ScannedObjectID))
            {
                if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.Autocapture))
                {
                    if (GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_AUTOCAPTURE))
                    {
                        ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_AUTOCAPTURE;
                        //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Autocapture"), "Info.bmp");
                    }
                    //else
                    //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_Autocapture"), "Warning.bmp");
                }
                //else
                //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_Autocapture"), "Info.bmp");
            }

            // Roll preview; only for rolled
            if (GBMSGUI.IsRolled(ScannedObjectID))
            {
                if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.NoRollPreview)
                    && GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_NO_ROLL_PREVIEW))
                {
                    ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_NO_ROLL_PREVIEW;
                    //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_Roll_Preview"), "Info.bmp");
                }
                else
                {
                    if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.RollPreviewManualStop))
                    {
                        if (GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_MANUAL_ROLL_PREVIEW_STOP))
                        {
                            ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_MANUAL_ROLL_PREVIEW_STOP;
                            //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Roll_Preview_Manual_Stop"), "Info.bmp");
                        }
                        //else
                        //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_Roll_Preview_Manual_Stop"), "Warning.bmp");
                    }
                    else // BUG FIX - the else was missing - 07/10/2009
                    {
                        // automatic roll preview modes are always supported
                        if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.RollPreviewType))
                        {
                            ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_ROLL_PREVIEW_TYPE;
                            //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Roll_Preview_Center"), "Info.bmp");
                        }
                        //else
                        //{
                        //    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Roll_Preview_Side"), "Info.bmp");
                        //}
                    }
                }
            }

            // 1.12.0.0 - GBMSAPI_AO_FLAT_SINGLE_FINGER_ON_ROLL_AREA is obsolete, now set with ScanArea
            /*
            // Flat finger on roll scanner
            if (GBMSGUI.IsFlatSingleFinger(ScannedObject.ScannedObjectID)
                && MSGUIRef.CfgData.FlatFingerOnRollArea
                && GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FLAT_SINGLE_FINGER_ON_ROLL_AREA))
                ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FLAT_SINGLE_FINGER_ON_ROLL_AREA;
            */

            // High speed full res preview
            if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.HighSpeedPreview))
            {
                if (GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_HIGH_SPEED_PREVIEW))
                    ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_HIGH_SPEED_PREVIEW;
                //else
                //listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_High_Speed_Preview"), "Warning.bmp");
            }

            // show other options

            /*
            if (!GBMSGUI.IsRolled(ScannedObject.ScannedObjectID))
            {
                // Block autocapture
                if (GBMSGUI.CheckMask(MSGUIRef.AcquisitionOptions, GBMSGUI.AcquisitionOption.BlockAutocapture)
                    && !MSGUIRef.BlockAutocaptureSupported)
                {
                    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_BlockAutocpature"), "Warning.bmp");
                }
            }
            */

            /*
            // show only for Slap
            // 1.11.0.0 - also joints
            if (GBMSGUI.IsSlap(ScannedObject.ScannedObjectID) || GBMSGUI.IsJoint(ScannedObject.ScannedObjectID))
            {
                if (GBMSGUI.CheckMask(MSGUIRef.AcquisitionOptions, GBMSGUI.AcquisitionOption.Segmentation))
                    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Segmentation_On"), "Info.bmp");
                else
                    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Segmentation_Off"), "Info.bmp");
            }

            if (GBMSGUI.CheckMask(MSGUIRef.SessionOptions, GBMSGUI.SessionOption.SequenceCheck))
                listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Sequence_Check_ON"), "Info.bmp");
            else
                listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Sequence_Check_OFF"), "Info.bmp");

            if (GBMSGUI.IsRolled(ScannedObject.ScannedObjectID))
            {
                // Auto clear outside
                if (GBMSGUI.CheckMask(MSGUIRef.AcquisitionOptions, GBMSGUI.AcquisitionOption.AutoClearOutsideRoll))
                {
                    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_Auto_Clear_Outside_Roll_Active"), "Info.bmp");
                }
            }
            */

            // 1.9.12.0 - dry finger enhancement
            //if (DryFingerImageEnhancementSupported)
            //{
            //    if (!SetDryFingerImageEnhancementOption(chkDryFingerImageEnhancement.Checked))
            //        return;
            //}
            /*
            else
            {
                if (GBMSGUI.CheckMask(MSGUIRef.SessionOptions, GBMSGUI.SessionOption.DryFingerImageEnhancement))
                {
                    listViewMessages.Items.Add(MSGUIRef.GetLocalizedString("Msg_No_Dry_Finger_Image_Enhancement"), "Warning.bmp");
                }
            }
            */

            // 1.12.0.0
            if (GBMSGUI.IsRolled(ScannedObjectID))
            {
                if (GBMSGUI.CheckMask(AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                {
                    if (GBMSGUI.CheckMask(SupportedOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_ADAPT_ROLL_AREA_POSITION))
                    {
                        ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_ADAPT_ROLL_AREA_POSITION;

                        // roll direction
                        //if (MSGUIRef.AdaptiveRollDir == GBMSGUI.AdaptiveRollDirection.ToLeft)
                        //ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FORCE_ROLL_TO_LEFT;
                        //else if (MSGUIRef.AdaptiveRollDir == GBMSGUI.AdaptiveRollDirection.ToRight)
                        //ScanOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FORCE_ROLL_TO_RIGHT;
                    }
                }
            }
        }


        public static uint SetScanArea(uint ScannedObjectID)
        {
            uint ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;

            if (GBMSGUI.IsRolledThenar(ScannedObjectID)
                // 1.14.0.0
                || GBMSGUI.IsRolledHypothenar(ScannedObjectID))
            {
                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR;
            }
            else if (GBMSGUI.IsRolledJoint(ScannedObjectID) ||
                 GBMSGUI.IsFlatJoint(ScannedObjectID))
            {
                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
            }
            else if (GBMSGUI.IsRolled(ScannedObjectID))
            {
                // 1.12.0.0
                //if (CfgData.RollArea == RollAreaType.RollAreaGA)
                //    ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                //else
                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
            }
            else if (GBMSGUI.IsFlatSingleFinger(ScannedObjectID)
                && false
                && GBMSGUI.CheckMask(SupportedScanOptions, GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_FLAT_SINGLE_FINGER_ON_ROLL_AREA))
            {
                // 1.12.0.0
                //if (CfgData.RollArea == RollAreaType.RollAreaGA)
                //    ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                //else
                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
            }

            return ScanArea;
        }


    }
}
