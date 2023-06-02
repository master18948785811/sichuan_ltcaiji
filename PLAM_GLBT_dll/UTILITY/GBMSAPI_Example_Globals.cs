using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using GBMSAPI_NET;
using GBMSAPI_NET.GBMSAPI_NET_Defines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_AcquisitionProcessDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_RollFunctionalityDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLCDDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLEDsDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;

using GBMSAPI_CS_Example.UTILITY;

namespace GBMSAPI_CS_Example.UTILITY
{
    class GBMSAPI_Example_Globals
    {
        public static IntPtr AcquisitionPreviewBuffer = IntPtr.Zero;
        public static IntPtr AcquisitionFullResBuffer = IntPtr.Zero;        
		public static Bitmap FullResImage ;
		public static Bitmap PreviewImage;
		///////////////////////////////////////////////////
        // Calibration image
        ///////////////////////////////////////////////////
        public static IntPtr CalibrationBuffer = IntPtr.Zero;
        public static IntPtr DummyCalibrationBuffer = IntPtr.Zero;

        ///////////////////////////////////////////////////
        // GLOBALS FOR INFO EXCHANGING WITH THE CALLBACK
        ///////////////////////////////////////////////////
        /*************************************************
        * Acquisition Options
        *************************************************/
        public static uint ObjToScan = 0;
        public static uint OptionMask = 0;
        public static uint DisplayOptionMask = 0;
        public static Byte ContrastLimitToDisplay = 0;
        public static Byte CompletenessLimitToDisplay = 0;
        public static uint ClipRegionW = 0;
        public static uint ClipRegionH = 0;
        public static uint RollPreviewTimeout = 0;
        public static Byte ArtefactCleaningDepth = 0;
        public static Byte ContrastIntermediateLimitToDisplay = 0;
        public static Byte CompletenessIntermediateLimitToDisplay = 0;
		// ver 2.10.0.0 
		public static bool IsRollAreaStandardGa = false;
		public static UInt32 AcquisitionScanArea = 0;
		// end ver 2.10.0.0
        // ver 3.2.0.0
        public static bool RollBlockCompositionIsEnabled = false;
        // end ver 3.2.0.0
        // Image dimension
        public static uint PreviewImSX = 0, PreviewImSY = 0, FullResImSX = 0, FullResImSY = 0;
        /*************************************************
        * Acquisition State Indicators
        *************************************************/
        public static int LastErrorCode = 0;
        public static Boolean AcquisitionStarted = false;
        public static uint LastEventInfo = 0;
        public static int LastFrameSizeX = 0;
        public static int LastFrameSizeY = 0;
        public static double LastFrameCurrentFrameRate = .0;
        public static double LastFrameNominalFrameRate = .0;
        public static uint LastDiagnosticValue = 0;
        public static uint OldDiagnosticValue = 0;
        public static int AutoCapturePhase = GBMSAPI_NET_AutoCapturePhase.GBMSAPI_NET_ACP_OFF;
        public static int ClippingRegionPosX = 0, ClippingRegionPosY = 0;
        public static uint ClippingRegionSizeX = 0, ClippingRegionSizeY = 0;
        public static uint RolledArtefactSize = 0;
        public static Byte[] MarkerFrame = null;
        public static Byte[] NotWipedArtefactFrame = null;
        public static uint FlatFingerprintSize = 0;
        public static Byte ImageContrast = 0;
        public static uint ImageSize = 0;
        public static Byte HalfLowerPalmCompleteness = 0;
        public static Boolean SkipRequested = false;
        public static Boolean PreviewEnded = false;

        public static Boolean AcquisitionButtonPressed = false;
        public static Boolean ScannerStarted = false;
        public static Boolean AcquisitionEnded = false;
        public static Boolean BusyFrame = false;
        public static uint AcquisitionStatus = 0;
        public static uint PreviousAcquisitionStatus = 0;

        public static uint FrameNumber = 0;
        public static uint PaintNumber = 0;

        public static uint AcquiredFramesNumber = 0, LostFramesNumber = 0;

        // ver 3.1.0.0
        public static Size AcqWinPaintBoxOriginalSize = new Size(600, 600);
        public static Point AcqWinPaintBoxOriginalLocation = new Point(3, 21);
        public static Size AcqWinPaintBoxSizeForPreview = new Size();
        public static Point AcqWinPaintBoxLocationForPreview = new Point();
        public static Size AcqWinPaintBoxSizeForAcq = new Size();
        public static Point AcqWinPaintBoxLocationForAcq = new Point();
        // end ver 3.1.0.0

        /*************************************************
        * Dry/wet finger percent
        *************************************************/
        // ver 3.1.0.1
        public static byte DryFingerPercent = 0;
        public static byte WetFingerPercent = 0;
        // end ver 3.1.0.1

        /*************************************************
        * Time measurement
        *************************************************/
        // ver 3.3.0.0
        public static DateTime AcquisitionStartTime;
        public static bool FirstFrameArrived;
        // end ver 3.3.0.0

        // ver 4.0.0.0
        /*************************************************
        * Hw Fake Detection
        *************************************************/
        public static int HwFfdThresholdToBeSet = -1;
        public static bool HwFfdFlag;
        public static UInt32 HwFfdDiagnosticValue;
        public static int HwFfdError = GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR;
        /*************************************************
        * Sw Fake Detection
        *************************************************/
        public static int SwFfdThresholdToBeSet = -1;
        public static bool SwFfdFlag;
        // end ver 4.0.0.0

        public static void InitGlobalAcqOptions()
        {
            /*************************************************
            * Acquisition Options
            *************************************************/
            ObjToScan = 0;
            OptionMask = 0;
            DisplayOptionMask = 0;
            ContrastLimitToDisplay = 0;
            CompletenessLimitToDisplay = 0;
            ClipRegionW = 0;
            ClipRegionH = 0;
            RollPreviewTimeout = 0;
            ArtefactCleaningDepth = 0;
            // Image dimension
            PreviewImSX = 0;
            PreviewImSY = 0;
            FullResImSX = 0;
            FullResImSY = 0;
            /*************************************************
            * Acquisition State Indicators
            *************************************************/
            LastErrorCode = 0;
            AcquisitionStarted = false;
            LastEventInfo = 0;
            LastFrameSizeX = 0;
            LastFrameSizeY = 0;
            LastFrameCurrentFrameRate = .0;
            LastFrameNominalFrameRate = .0;
            LastDiagnosticValue = 0;
            OldDiagnosticValue = 0;
            AutoCapturePhase = GBMSAPI_NET_AutoCapturePhase.GBMSAPI_NET_ACP_OFF;
            ClippingRegionPosX = 0;
            ClippingRegionPosY = 0;
            ClippingRegionSizeX = 0;
            ClippingRegionSizeY = 0;
            RolledArtefactSize = 0;
            MarkerFrame = null;
            NotWipedArtefactFrame = null;
            FlatFingerprintSize = 0;
            ImageContrast = 0;
            ImageSize = 0;
            HalfLowerPalmCompleteness = 0;
            SkipRequested = false;
            PreviewEnded = false;

            AcquisitionButtonPressed = false;
            ScannerStarted = false;
            AcquisitionEnded = false;
            BusyFrame = false;
            AcquisitionStatus = 0;
            PreviousAcquisitionStatus = 0;

            AcquiredFramesNumber = 0;
            LostFramesNumber = 0;
            
            // ver 4.0.0.0
            /*************************************************
             * * Hw Fake Detection
             * *************************************************/
            HwFfdThresholdToBeSet = -1;
            HwFfdFlag = false;
            HwFfdDiagnosticValue = 0;
            /*************************************************
             * * Sw Fake Detection
             * *************************************************/
            SwFfdThresholdToBeSet = -1;
            SwFfdFlag = false;
            // end ver 4.0.0.0
        }

        /**************************************
         * FRAME RATE SETTINGS
        **************************************/
		// ver 2.10.0.0: not needed anymore
        // public static UInt32 FrameRateOptions;
		// end ver 2.10.0.0: not needed anymore

        /**************************************
         * DSInit
        **************************************/
        public const uint COINIT_APARTMENTTHREADED = (uint)(0x02);
        [DllImport("DSBeep.dll", EntryPoint = "DSInit")]
        public static extern bool DSInit(uint CoInit, bool ForceDS);

        /**************************************
         * DSInit
        **************************************/
        [DllImport("DSBeep.dll", EntryPoint = "DSBeep")]
        public static extern bool DSBeep(int Frequency, int Duration);

        /**************************************
         * Image Finalization
        **************************************/
        public static bool UseImageFinalization = true;

        /**************************************
         * Acquisition Info Picture Box
        **************************************/
        public static int DesiredPictureBoxSize = 500;

        /**************************************
         * Display management
        **************************************/
        public static void DisplayImageEvaluationParameters()
        {
            uint LcdFeatures;
            int RetVal =
                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_GetLcdFeatures(out LcdFeatures);
            if ((GBMSAPI_Example_Globals.DisplayOptionMask &
                GBMSAPI_NET_DisplayOptions.GBMSAPI_NET_DO_FINAL_SCREEN) != 0
                // stop screen enabled after acquisition
            )
            {
                UInt32 ObjToScanType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(
                    GBMSAPI_Example_Globals.ObjToScan);
                            
                // set image evaluation parameters
                if (
                    // VER 3.1.0.0: check all rolled objects
                    (GBMSAPI_Example_Util.IsRolled(ObjToScanType))
                    // end VER 3.1.0.0: check all rolled objects
                )
                {
                    // ARTEFACTS
                    RetVal =
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetArtefactLimitOnStopScreen
                        (
                            7,
                            GBMSAPI_NET_LimitsDirection.GBMSAPI_NET_VILCD_SMALLER_THAN_LIMIT_DIRECTION
                        );
                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                            RetVal, "WAIT_PREVIEW_END_STATUS");
                    }
                    if ((LcdFeatures & GBMSAPI_NET_DisplayFeatures.GBMSAPI_NET_VILCD_LF_INTERMEDIATE_LIMIT) != 0)
                    {
                        RetVal = GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetArtefactIntermediateLimitOnStopScreen(14);
                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                        {
                            GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                                RetVal,
                                "WAIT_PREVIEW_END_STATUS, GBMSAPI_NET_VUI_LCD_SetArtefactIntermediateLimitOnStopScreen");
                        }
                    }

                    int ArtefactSize;
                    ArtefactSize = (int)(GBMSAPI_Example_Globals.RolledArtefactSize);

                    RetVal =
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetArtefactOnStopScreen
                        (ArtefactSize);
                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                            RetVal, "WAIT_PREVIEW_END_STATUS");
                    }
                    /////////////////////
                    // QUALITY
                    /////////////////////
                    // Quality parameter should be got by using the GBNFIQ or the
                    // GBFINIMG libraries (FULL-ENHANCED package), not included in
                    // the basic version of the GBMSAPI. Therefore quality will
                    // be calculated by dividing contrast value by 2,56, in order
                    // to have a value ranging from 0 (lowest) to 100 (highest)
                    RetVal =
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetQualityLimitOnStopScreen(
                        95,
                        GBMSAPI_NET_LimitsDirection.GBMSAPI_NET_VILCD_GREATER_THAN_LIMIT_DIRECTION
                        );
                    if ((LcdFeatures & GBMSAPI_NET_DisplayFeatures.GBMSAPI_NET_VILCD_LF_INTERMEDIATE_LIMIT) != 0)
                    {
                        RetVal = GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetQualityIntermediateLimitOnStopScreen(90);
                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                        {
                            GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                                RetVal,
                                "WAIT_PREVIEW_END_STATUS, GBMSAPI_NET_VUI_LCD_SetQualityIntermediateLimitOnStopScreen");
                        }
                    }
                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                            RetVal, "WAIT_PREVIEW_END_STATUS");
                    }
                    Byte QualityToShow = (Byte)(((double)GBMSAPI_Example_Globals.ImageContrast) / 2.56);
                    RetVal =
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetQualityOnStopScreen
                        (QualityToShow);
                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                            RetVal, "WAIT_PREVIEW_END_STATUS");
                    }
                }
                else if ((GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(
                    GBMSAPI_Example_Globals.ObjToScan) !=
                    GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO))
                {
                    // CONTRAST
                    RetVal =
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetContrastLimitOnStopScreen
                        (230);
                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                            RetVal, "WAIT_PREVIEW_END_STATUS");
                    }
                    if ((LcdFeatures & GBMSAPI_NET_DisplayFeatures.GBMSAPI_NET_VILCD_LF_INTERMEDIATE_LIMIT) != 0)
                    {
                        RetVal = GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetContrastIntermediateLimitOnStopScreen(200);
                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                        {
                            GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                                RetVal,
                                "WAIT_PREVIEW_END_STATUS, GBMSAPI_NET_VUI_LCD_SetContrastIntermediateLimitOnStopScreen");
                        }
                    }
                    RetVal =
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetContrastValueOnStopScreen
                        (GBMSAPI_Example_Globals.ImageContrast);
                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                            RetVal, "WAIT_PREVIEW_END_STATUS");
                    }
                    // COMPLETENESS
                    if ((GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(
                        GBMSAPI_Example_Globals.ObjToScan) ==
                        GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM))
                    {
                        RetVal =
                            GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetCompletenessLimitOnStopScreen
                            (90);
                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                        {
                            GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                                RetVal, "WAIT_PREVIEW_END_STATUS");
                        }
                        if ((LcdFeatures & GBMSAPI_NET_DisplayFeatures.GBMSAPI_NET_VILCD_LF_INTERMEDIATE_LIMIT) != 0)
                        {
                            RetVal = GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetCompletenessIntermediateLimitOnStopScreen(60);
                            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                            {
                                GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                                    RetVal,
                                    "WAIT_PREVIEW_END_STATUS, GBMSAPI_NET_VUI_LCD_SetContrastIntermediateLimitOnStopScreen");
                            }
                        }
                        RetVal =
                            GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetCompletenessValueOnStopScreen
                            (GBMSAPI_Example_Globals.HalfLowerPalmCompleteness);
                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                        {
                            GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                                RetVal, "WAIT_PREVIEW_END_STATUS");
                        }
                    }
                }

                RetVal =
                    GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_EnableOKButtonOnStopScreen();
                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                        RetVal, "WAIT_PREVIEW_END_STATUS");
                }
                // show message
                RetVal = GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetPreloadedMessageOnStopScreen(
                    GBMSAPI_NET_PreloadedGeneralMessages.GBMSAPI_NET_VILCD_MSG_ACQUISITION_SUCCESSFUL,
                    GBMSAPI_NET_PreloadedMessagesArea.GBMSAPI_NET_VILCD_MSG_AREA);
                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                        RetVal, "WAIT_PREVIEW_END_STATUS");
                }
            }
            else
            {
                RetVal = GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_EnableStartButtonOnLogoScreen();
                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(
                        RetVal, "WAIT_PREVIEW_END_STATUS");
                }
            }
        }
    }
}
