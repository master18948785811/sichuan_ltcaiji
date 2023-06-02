using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
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
using PLAM_GLBT_dll;

namespace GBMSAPI_CS_Example.ACQUISITION_FORM
{
    internal class MS500_NET_DLL_WRAPPER
    {
        [DllImport("MS500u.dll", EntryPoint = "MS500_SetDiagnosticSensitivity")]
        public static extern int MS500_SetDiagnosticSensitivity(
            Int32 ParameterType,
            IntPtr ParameterValuePtr
        );
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
    public partial class AcquisitionForm 
    {
        public static int GBMSAPI_ExampleFrCount = 0;
        

        public int AcquisitionCallback(
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
                // LastEventOccurred = OccurredEventCode;
                if (GBMSAPI_Example_Globals.SkipRequested == true)
                {
                    GBMSAPI_Example_Globals.SkipRequested = false;
                    return 0;
                }

                //////////////////////////////////
                // Get Optional external equipment
                //////////////////////////////////
                uint ExternalEquipment;
                int RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetOptionalExternalEquipment(out ExternalEquipment);

                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                    GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                    return 0;
                }


                ////////////////
                // check error
                ////////////////
                if (GetFrameErrorCode != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    // dummy pedal read in order to clear
                    Boolean PedalState = false;
                    if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_PEDAL) != 0)
                    {
                        // check pedal state
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_CheckPedalState(out PedalState);
                    }
                    GBMSAPI_Example_Globals.LastErrorCode = GetFrameErrorCode;
                    // Sound to advise operator that the fingerprint can be
                    // released
                    if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0)
                    {
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(12, 2, 1);
                    }
                    else
                    {
                        //Console.Beep(4000, 200);
                        GBMSAPI_Example_Globals.DSBeep(4000, 200);
                    }
                    GBMSAPI_Example_Globals.AcquisitionEnded = true;                            
                    return 1;
                }

                //////////////////////////////
                // check event:
                // ERROR EVENT ALREASY CHECKED
                //////////////////////////////
                switch (OccurredEventCode)
                {
                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_SCANNER_STARTED:
                        {
                            //GBMSAPI_Example_Util.GBMSAPI_Example_LogStart();
                            GBMSAPI_ExampleFrCount = 0;
                            // Set clipping region
                            if (GBMSAPI_Example_Globals.ClipRegionW > 0 && GBMSAPI_Example_Globals.ClipRegionH > 0)
                            {
                                RetVal = GBMSAPI_NET_ScannerStartedRoutines.GBMSAPI_NET_SetClippingRegionSize(
                                    GBMSAPI_Example_Globals.ClipRegionW, GBMSAPI_Example_Globals.ClipRegionH
                                    );
                                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                {
                                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                    GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                    return 0;
                                }
                            }

                            // Set Roll preview timeout and artefact cleaning depth
                            // VER 3.1.0.0: check all rolled objects
                            UInt32 ObjToScanType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan);
                            if (GBMSAPI_Example_Util.IsRolled(ObjToScanType))
                            {
                                if (GBMSAPI_Example_Globals.RollPreviewTimeout > 0)
                                {
                                    RetVal = GBMSAPI_NET_ScannerStartedRoutines.GBMSAPI_NET_ROLL_SetPreviewTimeout(
                                        GBMSAPI_Example_Globals.RollPreviewTimeout);
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        return 0;
                                    }
                                }

                                RetVal = GBMSAPI_NET_ScannerStartedRoutines.GBMSAPI_NET_ROLL_SetArtefactCleaningDepth(
                                    GBMSAPI_Example_Globals.ArtefactCleaningDepth);
                                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                {
                                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                    GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                    return 0;
                                }
                            }

                            GBMSAPI_Example_Globals.ScannerStarted = true;
                            break;
                        }

                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_PREVIEW_PHASE_END:
                        {
                            GBMSAPI_Example_Util.GBMSAPI_Example_LogError("PREVIEW END");
                            // VER 3.1.0.0: check all rolled objects
                            UInt32 ObjToScanType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan);
                            if (GBMSAPI_Example_Util.IsRolled(ObjToScanType))
                            // end VER 3.1.0.0: check all rolled objects
                            // Sound to advise operator that the preview has ended
                            {
                                // ver 3.1.0.0: sound if not external roll procedure
                                // or adaptative roll position
                                if (!((GBMSAPI_Example_Globals.OptionMask & 
                                    GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_EXTERNAL_ROLL_COMPOSITION) != 0) ||
                                    ((GBMSAPI_Example_Globals.OptionMask & 
                                    GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_ADAPT_ROLL_AREA_POSITION)) != 0)
                                {
                                    if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0)
                                    {
                                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(12, 2, 1);
                                    }
                                    else
                                    {
                                        //Console.Beep(4000, 200);
                                        GBMSAPI_Example_Globals.DSBeep(4000, 200);
                                    }
                                }
                                // end ver 3.1.0.0                               
                            }

                            GBMSAPI_Example_Globals.PreviewEnded = true;
                            break;
                        }

                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_VALID_FRAME_ACQUIRED:
                        {
                            GBMSAPI_Example_Globals.LastEventInfo = EventInfo;
                            if (GBMSAPI_Example_Globals.FirstFrameArrived == false)
                            {
                                GBMSAPI_Example_Globals.FirstFrameArrived = true;
                                long ElapsedTicks = DateTime.Now.Ticks - GBMSAPI_Example_Globals.AcquisitionStartTime.Ticks;
                                TimeSpan elapsedSpan = new TimeSpan(ElapsedTicks);
                                string strToLog = "" + DateTime.Now + ": FirstFrame arrival time = " + elapsedSpan.TotalMilliseconds;
                                GBMSAPI_Example_Util.GBMSAPI_Example_LogError(strToLog);
                            }
                            
                            if (GBMSAPI_Example_Globals.BusyFrame == false)
                            {
                                GBMSAPI_ExampleFrCount ++;
                                if ((GBMSAPI_Example_Globals.LastEventInfo & GBMSAPI_NET_EventInfo.GBMSAPI_NET_EI_ACQUISITION_PHASE) == 0)
                                {
                                    Marshal.Copy(FramePtr, 0, GBMSAPI_Example_Globals.AcquisitionPreviewBuffer, FrameSizeX * FrameSizeY);
									// ver 2.10.0.0: with stride
									GBMSAPI_Example_Util.CopyRawImageIntoBitmap(FramePtr, ref GBMSAPI_Example_Globals.PreviewImage);
									// end ver 2.10.0.0: with stride
                                }
                                else
                                {
                                    Marshal.Copy(FramePtr, 0, GBMSAPI_Example_Globals.AcquisitionFullResBuffer, FrameSizeX * FrameSizeY);
                                    // ver 2.10.0.0: with stride
                                    GBMSAPI_Example_Util.CopyRawImageIntoBitmap(FramePtr, ref GBMSAPI_Example_Globals.FullResImage);
									// end ver 2.10.0.0: with stride
								}
                                GBMSAPI_Example_Globals.BusyFrame = true;
                                GBMSAPI_Example_Globals.FrameNumber++;

                                // check diagnostic
                                if (GB_Diagnostic != 0 &&
                                    (
                                        ((GB_Diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_LEFT) == 0) &&
                                        ((GB_Diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_RIGHT) == 0) &&
                                        // ver 3.2.0.0
                                        ((GB_Diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_DOWN) == 0) &&
                                        ((GB_Diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_UP) == 0)
                                        // end ver 3.2.0.0
                                    )
                                )
                                {
                                    // if LED supported blink
                                    if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LED) != 0)
                                    {
                                        GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_VUI_LED_BlinkDuringAcquisition(1);
                                    }
                                }
                                else
                                {
                                    // if LED supported blink
                                    if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LED) != 0)
                                    {
                                        GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_VUI_LED_BlinkDuringAcquisition(0);
                                    }
                                }
                                // ver 3.1.0.1: dry/wet area percent
                                RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetDryWetFingerAreaPercent(
                                    out GBMSAPI_Example_Globals.DryFingerPercent, out GBMSAPI_Example_Globals.WetFingerPercent);
                                if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                {
                                    GBMSAPI_Example_Globals.DryFingerPercent = GBMSAPI_Example_Globals.WetFingerPercent = 0;
                                }
                                // end ver 3.1.0.1: dry/wet area percent

                                // if autocapture get autocapture phase
                                // VER 3.1.0.0: check all rolled objects
                                UInt32 ObjToScanType = 
                                    GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan);
                                if (
                                    !(GBMSAPI_Example_Util.IsRolled(ObjToScanType)) &&
                                    ObjToScanType != GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO
                                )
                                // end VER 3.1.0.0: check all rolled objects
                                {
                                    // check Auto-Capture option set and not ignored
                                    if (((GBMSAPI_Example_Globals.OptionMask & GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_AUTOCAPTURE) != 0)
                                        )
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetAutoCapturePhase(
                                            out GBMSAPI_Example_Globals.AutoCapturePhase);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            return 0;
                                        }

                                        // if diagnostic block autocapture
                                        uint DeviceFeatures;
                                        RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out DeviceFeatures);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            return 0;
                                        }
                                        if ((DeviceFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_AUTO_CAPTURE_BLOCKING) != 0)
                                        {
                                            if (GB_Diagnostic != 0)
                                            {
                                                GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_SetAutoCaptureBlocking(true);
                                                GBMSAPI_Example_Globals.AutoCapturePhase = -1;
                                            }
                                            else
                                            {
                                                GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_SetAutoCaptureBlocking(false);
                                            }
                                        }
                                    }

                                    // get clipping
                                    RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetClippingRegionPosition(
                                        out GBMSAPI_Example_Globals.ClippingRegionPosX,
                                        out GBMSAPI_Example_Globals.ClippingRegionPosY,
                                        out GBMSAPI_Example_Globals.ClippingRegionSizeX,
                                        out GBMSAPI_Example_Globals.ClippingRegionSizeY
                                        );
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        return 0;
                                    }

                                    // get contrast
                                    uint AvailableImageInfo;
                                    RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetAvailableImageInfo(
                                        GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan),
                                        out AvailableImageInfo);
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        return 0;
                                    }
                                    if ((AvailableImageInfo & GBMSAPI_NET_AvailableImageInfo.GBMSAPI_NET_AII_FINGERPRINT_CONTRAST) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintContrast(
                                            out GBMSAPI_Example_Globals.ImageContrast);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            return 0;
                                        }

                                        // if customized contrast, set on the LCD
                                        if (((GBMSAPI_Example_Globals.DisplayOptionMask &
                                            GBMSAPI_NET_DisplayOptions.GBMSAPI_NET_DO_CUSTOMIZED_CONTRAST) != 0) &&
                                            ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LCD) != 0)
                                            )
                                        {
                                            RetVal =
                                                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetContrastValueOnAcquisitionScreen(
                                                GBMSAPI_Example_Globals.ImageContrast);
                                            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                            {
                                                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                                GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                                return 0;
                                            }
                                        }
                                    }

                                    if ((AvailableImageInfo & GBMSAPI_NET_AvailableImageInfo.GBMSAPI_NET_AII_FINGERPRINT_SIZE) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintSize(
                                            out GBMSAPI_Example_Globals.ImageSize);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            return 0;
                                        }
                                    }
                                }

                                // if half lower palm get completeness
                                if (((GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan)) ==
                                    GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM))
                                {
                                    uint AvailableImageInfo;
                                    RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetAvailableImageInfo(
                                        GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan),
                                        out AvailableImageInfo);
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        return 0;
                                    }
                                    if ((AvailableImageInfo & GBMSAPI_NET_AvailableImageInfo.GBMSAPI_NET_AII_LOWER_HALF_PALM_COMPLETENESS) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetLowerHalfPalmCompleteness(
                                            out GBMSAPI_Example_Globals.HalfLowerPalmCompleteness);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            return 0;
                                        }

                                        // if customized completeness, set on the LCD
                                        if (((GBMSAPI_Example_Globals.DisplayOptionMask &
                                            GBMSAPI_NET_DisplayOptions.GBMSAPI_NET_DO_CUSTOMIZED_COMPLETENESS) != 0) &&
                                            ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LCD) != 0)
                                            )
                                        {
                                            RetVal =
                                                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetCompletenessValueOnAcquisitionScreen(
                                                GBMSAPI_Example_Globals.HalfLowerPalmCompleteness);
                                            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                            {
                                                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                                GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                                return 0;
                                            }
                                        }
                                    }
                                }

                                GBMSAPI_Example_Globals.LastFrameSizeX = FrameSizeX;
                                GBMSAPI_Example_Globals.LastFrameSizeY = FrameSizeY;
                                GBMSAPI_Example_Globals.LastFrameCurrentFrameRate = CurrentFrameRate;
                                GBMSAPI_Example_Globals.LastFrameNominalFrameRate = NominalFrameRate;
                                GBMSAPI_Example_Globals.LastDiagnosticValue = GB_Diagnostic;
                            }
                            break;
                        }

                    case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_ACQUISITION_END:
                        {
                            GBMSAPI_Example_Globals.LastEventInfo = EventInfo;
                            GBMSAPI_Example_Globals.AcquisitionEnded = true;

                            // Sound to advise operator that the fingerprint can be
                            // released
                            // Sound to advise operator that the preview has ended
                            if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0)
                            {
                                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(12, 2, 1);
                            }
                            else
                            {
                                //Console.Beep(4000, 200);
                                GBMSAPI_Example_Globals.DSBeep(4000, 200);
                            }

                            // dummy pedal read in order to clear
                            Boolean PedalState = false;
                            if ((ExternalEquipment & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_PEDAL) != 0)
                            {
                                // check pedal state
                                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_CheckPedalState(out PedalState);
                            }

                            GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_GetFrameStatistic(
                                out GBMSAPI_Example_Globals.AcquiredFramesNumber,
                                out GBMSAPI_Example_Globals.LostFramesNumber);

                            // Get Type for GetImageSize(...) function
                            uint ObjType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan);
                            if (ObjType != GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_NO_OBJECT_TYPE)
                            {
                                if ((GB_Diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_SURFACE_NOT_NORMA) == 0 &&
                                    (GB_Diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_FAILURE) == 0
                                )
                                {
                                    // VER 3.1.0.0: check all rolled objects
                                    if ((GBMSAPI_Example_Globals.LastEventInfo &
                                        GBMSAPI_NET_EventInfo.GBMSAPI_NET_EI_ACQUISITION_PHASE) != 0 &&
                                        !(GBMSAPI_Example_Util.IsRolled(ObjType)) &&
                                        (ObjType != GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO)
                                    )
                                    // end VER 3.1.0.0: check all rolled objects
                                    {
                                        if (GBMSAPI_Example_Globals.UseImageFinalization) 
                                            GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_ImageFinalization(
                                            FramePtr);
                                    } 
                                    // VER 3.1.0.0: check all rolled objects
                                    else if 
                                    (
                                        (GBMSAPI_Example_Util.IsRolled(ObjType)) &&
                                        (GBMSAPI_Example_Globals.OptionMask &
                                        GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_MANUAL_ROLL_PREVIEW_STOP) != 0
                                    )
                                    // end VER 3.1.0.0: check all rolled objects
                                    {
                                        if (GBMSAPI_Example_Globals.UseImageFinalization)
                                            GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_ImageFinalization(
                                            FramePtr);
                                    }
                                }


                                if ((GBMSAPI_Example_Globals.LastEventInfo & GBMSAPI_NET_EventInfo.GBMSAPI_NET_EI_ACQUISITION_PHASE) == 0)
                                {
                                    Marshal.Copy(FramePtr, 0, GBMSAPI_Example_Globals.AcquisitionPreviewBuffer, FrameSizeX * FrameSizeY);
									// ver 2.10.0.0: with stride
									GBMSAPI_Example_Util.CopyRawImageIntoBitmap(FramePtr, ref GBMSAPI_Example_Globals.PreviewImage);
									// end ver 2.10.0.0: with stride
								}
                                else
                                {
                                    Marshal.Copy(FramePtr, 0, GBMSAPI_Example_Globals.AcquisitionFullResBuffer, FrameSizeX * FrameSizeY);
									// ver 2.10.0.0: with stride
									GBMSAPI_Example_Util.CopyRawImageIntoBitmap(FramePtr, ref GBMSAPI_Example_Globals.FullResImage);
									// end ver 2.10.0.0: with stride
                                }

                                GBMSAPI_Example_Globals.LastFrameSizeX = FrameSizeX;
                                GBMSAPI_Example_Globals.LastFrameSizeY = FrameSizeY;
                                GBMSAPI_Example_Globals.LastFrameCurrentFrameRate = CurrentFrameRate;
                                GBMSAPI_Example_Globals.LastFrameNominalFrameRate = NominalFrameRate;
                                GBMSAPI_Example_Globals.LastDiagnosticValue = GB_Diagnostic;

                                // ver 4.0.0.0: fake fingerprint
                                uint devFeatures = 0;
                                RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out devFeatures);
                                if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                {
                                    if ((devFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_HW_ANTIFAKE) != 0)
                                    {
                                        GBMSAPI_Example_Globals.HwFfdError = GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_HardwareFakeFingerDetection(out GBMSAPI_Example_Globals.HwFfdFlag,
                                            out GBMSAPI_Example_Globals.HwFfdDiagnosticValue);
                                        if (GBMSAPI_Example_Globals.HwFfdError != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_Example_Globals.HwFfdFlag = false;
                                            GBMSAPI_Example_Globals.HwFfdDiagnosticValue = 0;
                                        }
                                    }
                                    if ((devFeatures & GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_SW_ANTIFAKE) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_SoftwareFakeFingerDetection(
                                            FramePtr, FrameSizeX, FrameSizeY,
                                            out GBMSAPI_Example_Globals.SwFfdFlag);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            return 0;
                                        }
                                    }
                                }
                                else
                                {
                                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                    GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                    return 0;
                                }
                                // end ver 4.0.0.0: fake fingerprint
                                GBMSAPI_Example_Globals.BusyFrame = true;


                                // if autocapture get autocapture phase
                                if ((GBMSAPI_Example_Globals.LastEventInfo & GBMSAPI_NET_EventInfo.GBMSAPI_NET_EI_ACQUISITION_PHASE) != 0 &&
                                    // VER 3.1.0.0: check all rolled objects
                                    !(GBMSAPI_Example_Util.IsRolled(ObjType)) &&
                                    // end VER 3.1.0.0: check all rolled objects
                                    (ObjType != GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO)
                                    )
                                {
                                    // check Auto-Capture option set and not ignored
                                    if (((GBMSAPI_Example_Globals.OptionMask & GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_AUTOCAPTURE) != 0)
                                        )
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetAutoCapturePhase(
                                            out GBMSAPI_Example_Globals.AutoCapturePhase);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
											//GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                            return 0;
                                        }
                                    }
                                }

                                // if clipping get clipping region
                                if ((ObjType != GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO) &&
                                    (GBMSAPI_Example_Globals.ClipRegionW > 0 && GBMSAPI_Example_Globals.ClipRegionH > 0))
                                {
                                    RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetClippingRegionPosition(
                                            out GBMSAPI_Example_Globals.ClippingRegionPosX,
                                            out GBMSAPI_Example_Globals.ClippingRegionPosY,
                                            out GBMSAPI_Example_Globals.ClippingRegionSizeX,
                                            out GBMSAPI_Example_Globals.ClippingRegionSizeY
                                            );
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                        return 0;
                                    }
                                }

                                // if not rolled or photo get contrast
                                if (
                                    // VER 3.1.0.0: check all rolled objects
                                    !(GBMSAPI_Example_Util.IsRolled(ObjType)) &&
                                    // end VER 3.1.0.0: check all rolled objects
                                    (ObjType != GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO))
                                {
                                    // get contrast
                                    uint AvailableImageInfo;
                                    RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetAvailableImageInfo(
                                        GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan),
                                        out AvailableImageInfo);
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                        return 0;
                                    }
                                    if ((AvailableImageInfo & GBMSAPI_NET_AvailableImageInfo.GBMSAPI_NET_AII_FINGERPRINT_CONTRAST) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintContrast(
                                            out GBMSAPI_Example_Globals.ImageContrast);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                            return 0;
                                        }
                                    }

                                    if ((AvailableImageInfo & GBMSAPI_NET_AvailableImageInfo.GBMSAPI_NET_AII_FINGERPRINT_SIZE) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetFingerprintSize(
                                            out GBMSAPI_Example_Globals.ImageSize);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                            return 0;
                                        }
                                    }
                                }

                                // if half lower palm get completeness
                                if ((ObjType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM))
                                {
                                    // get contrast
                                    uint AvailableImageInfo;
                                    RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetAvailableImageInfo(
                                        GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(GBMSAPI_Example_Globals.ObjToScan),
                                        out AvailableImageInfo);
                                    if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                        return 0;
                                    }
                                    if ((AvailableImageInfo & GBMSAPI_NET_AvailableImageInfo.GBMSAPI_NET_AII_LOWER_HALF_PALM_COMPLETENESS) != 0)
                                    {
                                        RetVal = GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetLowerHalfPalmCompleteness(
                                                out GBMSAPI_Example_Globals.HalfLowerPalmCompleteness);
                                        if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                                        {
                                            GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                            GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                            //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                            return 0;
                                        }
                                    }
                                }

                                // if Roll object get rolled info
                                    
                                if (
                                    // VER 3.1.0.0: check all rolled objects
                                    (GBMSAPI_Example_Util.IsRolled(ObjType))
                                    // end VER 3.1.0.0: check all rolled objects
                                )
                                {
									// ver 2.10.0.0: use "2" function, comment next lines
									//RetVal = GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions.GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_ROLL_GetCompositeImageInfo(
									//    out GBMSAPI_Example_Globals.RolledArtefactSize,
									//    out GBMSAPI_Example_Globals.MarkerFrame,
									//    out GBMSAPI_Example_Globals.NotWipedArtefactFrame,
									//    out GBMSAPI_Example_Globals.ImageSize,
									//    out GBMSAPI_Example_Globals.ImageContrast,
									//    out GBMSAPI_Example_Globals.FlatFingerprintSize
									//    );
									// end ver 2.10.0.0: use "2" function, comment next lines
									// ver 2.10.0.0: use "2" function, add next lines
									UInt32 imgsx, imgsy;
									RetVal = GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions.GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_ROLL_GetCompositeImageInfo2(
										out GBMSAPI_Example_Globals.RolledArtefactSize,
										out GBMSAPI_Example_Globals.MarkerFrame,
										out GBMSAPI_Example_Globals.NotWipedArtefactFrame,
										out GBMSAPI_Example_Globals.ImageSize,
										out GBMSAPI_Example_Globals.ImageContrast,
										out GBMSAPI_Example_Globals.FlatFingerprintSize,
										out imgsx, out imgsy
										);
									// end ver 2.10.0.0: use "2" function, add next lines
									if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR &&
                                        RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_UNAVAILABLE_OPTION
                                        // means that rolling has been stopped before roll begins
                                        )
                                    {
                                        GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                                        GBMSAPI_Example_Globals.LastErrorCode = RetVal;
                                        //GBMSAPI_Example_Globals.AcquisitionEnded = true;
                                        return 0;
                                    }
                                }
                            }

                            break;
                        }
                }


                return 1;
            }
            catch (Exception ex)
            {
                GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions.GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                MessageBox.Show("Exception in AcquisitionStateManagement: " + ex.Message);
                return 0;
            }
        }
    }
}
