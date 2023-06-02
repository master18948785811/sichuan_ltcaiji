using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
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

namespace GBMSAPI_CS_Example.UTILITY
{
    class GBMSAPI_Example_Util
    {
        public static Byte[] CutImage(
            Byte[] FramePtr,
            int OrigSizeX, int OrigSizeY,
            int ClPosX, int ClPosY,
            int ClSizeX, int ClSizeY
        )
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

        public static String GBMSAPI_Example_GetDevNameFromDevID(Byte DevID)
        {
            try
            {
                String RetVal = null;
                switch (DevID)
                {
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS84_NameStr;
                            break;
                        }
                    // ver 3.4.0.0
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS84t_NameStr;
                            break;
                        }
                    // end ver 3.4.0.0
                    // ver 4.0.0.0
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.Did20_NameStr;
                            break;
                        }
                    // end ver 4.0.0.0
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS500:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MS500_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS1000:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MS1000_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_VS3:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.VS3_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_PS2:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.PS2_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS40_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS40I_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS26_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC500:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MC500_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC500:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MSC500_NameStr;
                            break;
                        }
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS84C_NameStr;
                            break;
                        }
                    // VER 2800
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MC517_NameStr;
                            break;
                        }
                    // VER 2800
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC517:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MSC517_NameStr;
                            break;
                        }
                    // VER 2800
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.DS32_NameStr;
                            break;
                        }
                    // Ver 2.10.0.0
                    case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527:
                        {
                            RetVal = GBMSAPI_Example_DevNameStrings.MS527_NameStr;
                            break;
                        }
                    // end Ver 2.10.0.0
                    default:
                        break;
                }

                return RetVal;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in GBMSAPI_Example_GetDevNameFromDevID: " + ex.Message);
                return null;
            }
        }

        public static Byte GBMSAPI_Example_GetDevIDFromDevName(String DevName)
        {
            try
            {
                Byte RetVal = 0;

                if (DevName == GBMSAPI_Example_DevNameStrings.DS40_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40;
                }
                if (DevName == GBMSAPI_Example_DevNameStrings.DS40I_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.DS84_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84;
                }
                // ver 3.4.0.0
                else if (DevName == GBMSAPI_Example_DevNameStrings.DS84t_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t;
                }
                // end ver 3.4.0.0
                // ver 4.0.0.0
                else if (DevName == GBMSAPI_Example_DevNameStrings.Did20_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20;
                }
                // end ver 4.0.0.0
                else if (DevName == GBMSAPI_Example_DevNameStrings.DS84C_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C;
                }
                // VER 2800
                else if (DevName == GBMSAPI_Example_DevNameStrings.MC517_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517;
                }
                // VER 2800
                else if (DevName == GBMSAPI_Example_DevNameStrings.MSC517_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC517;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.DS26_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.MS1000_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS1000;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.MS500_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS500;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.MC500_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC500;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.MSC500_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC500;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.PS2_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_PS2;
                }
                else if (DevName == GBMSAPI_Example_DevNameStrings.VS3_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_VS3;
                }
                // VER 2800
                else if (DevName == GBMSAPI_Example_DevNameStrings.DS32_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32;
                }
                // Ver 2.10.0.0
                else if (DevName == GBMSAPI_Example_DevNameStrings.MS527_NameStr)
                {
                    RetVal = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527;
                }
                // end Ver 2.10.0.0

                return RetVal;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in GBMSAPI_Example_GetDevIDFromDevName: " + ex.Message);
                return 0;
            }
        }

        public static String GBMSAPI_Example_GetErrorStringFromCode(Int32 ErrorCode)
        {
            switch (ErrorCode)
            {
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR):
                    {
                        return "NO_ERROR";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_DRIVER):
                    {
                        return "USB_DRIVER";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_DEVICE_NOT_FOUND):
                    {
                        return "DEVICE_NOT_FOUND";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_THREAD):
                    {
                        return "USB_THREAD";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_PARAMETER):
                    {
                        return "PARAMETER";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCANNER_NOT_CONFIGURED):
                    {
                        return "SCANNER_NOT_CONFIGURED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_DEVICE_NOT_RESPONDING):
                    {
                        return "DEVICE_NOT_RESPONDING";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCANNER_COMMUNICATION):
                    {
                        return "SCANNER_COMMUNICATION";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_UNAVAILABLE_OPTION):
                    {
                        return "UNAVAILABLE_OPTION";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_INTERNAL):
                    {
                        return "INTERNAL";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SPECIFIC_DLL_NOT_LOADED):
                    {
                        return "SPECIFIC_DLL_NOT_LOADED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_METHOD_NOT_SUPPORTED):
                    {
                        return "METHOD_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_OBJECT_TYPE_NOT_SUPPORTED):
                    {
                        return "OBJECT_TYPE_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCAN_AREA_NOT_SUPPORTED):
                    {
                        return "SCAN_AREA_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_ACQUISITION_THREAD):
                    {
                        return "ACQUISITION_THREAD";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_ACQUISITION_ALREADY_STARTED):
                    {
                        return "ACQUISITION_ALREADY_STARTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_FEATURE_NOT_SUPPORTED):
                    {
                        return "FEATURE_NOT_SUPPORTED";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_CURRENT_DEV_NOT_SET):
                    {
                        return "CURRENT_DEV_NOT_SET";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_MEMORY_ALLOCATION):
                    {
                        return "MEMORY_ALLOCATION";
                    }
                case (GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_GENERIC):
                    {
                        return "GENERIC";
                    }
            }
            return "";
        }

        public static void GBMSAPI_Example_ManageErrors(Int32 ErrorCode, String FunctionName)
        {
            try
            {
                String StrToShow = "Error Code = " + GBMSAPI_Example_GetErrorStringFromCode(ErrorCode) + " in function " + FunctionName;
                if (ErrorCode == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_DRIVER)
                {
                    UInt32 USBErrorCode = 0;
                    int RetVal = GBMSAPI_NET_AuxiliaryRoutines.GBMSAPI_NET_GetUSBErrorCode(out USBErrorCode);
                    if (RetVal == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        StrToShow += "; USB Error Code = 0x" + USBErrorCode.ToString("X8");
                    }
                }
                if (ErrorCode == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_EXCEPTION)
                {
                    StrToShow += "; Exception: " + GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_EXCEPTION_STRING;
                }
                MessageBox.Show(StrToShow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in GBMSAPI_Example_ManageErrors: " + ex.Message);
                return;
            }
        }

        public static void GBMSAPI_Example_ManageErrors(Int32 ErrorCode, uint USBErrorCode, String FunctionName)
        {
            try
            {
                String StrToShow = "Error Code = " + GBMSAPI_Example_GetErrorStringFromCode(ErrorCode) + " in function " + FunctionName;
                StrToShow += "; USB Error Code = 0x" + USBErrorCode.ToString("X8");

                MessageBox.Show(StrToShow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in GBMSAPI_Example_ManageErrors: " + ex.Message);
                return;
            }
        }

        public static List<String> GetScannableTypesListFromMask(uint ScannableTypesMask)
        {
            List<String> RetVal = new List<String>();
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RollSingleFinger);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SINGLE_FINGER) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatSingleFinger);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_4) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatSlap4);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_2) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatSlap2);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_THUMBS_2) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatThumbs2);
            }
            // VER 2000
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_INDEXES_2) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatIndexes2);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatLHP);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_UPPER_HALF_PALM) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatUHP);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_WRITER_PALM) != 0)
            {
                RetVal.Add(ScannableTypesStrings.FlatWP);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO) != 0)
            {
                RetVal.Add(ScannableTypesStrings.Photo);
            }
            // Ver 3.3.0.0
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_HYPOTHENAR) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledHypoThenar);
            }
            // end Ver 3.3.0.0
            // Ver 2.10.0.0
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_THENAR) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledThenar);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledJoint);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_RIGHT_SIDE) != 0)
            {
                RetVal.Add(ScannableTypesStrings.PlainJointRightSide);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_LEFT_SIDE) != 0)
            {
                RetVal.Add(ScannableTypesStrings.PlainJointLeftSide);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT_CENTER) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledJointCenter);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_TIP) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledTip);
            }
            // end Ver 2.10.0.0
            // ver 3.2.0.0
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_UP) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledTop);
            }
            if ((ScannableTypesMask & GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_DOWN) != 0)
            {
                RetVal.Add(ScannableTypesStrings.RolledDown);
            }
            // end ver 3.2.0.0
            return RetVal;
        }

        public static String[] GetObjectsToScanListFromObjectType(uint ObjType)
        {
            // check that ObjType is supported
            uint SupportedTypes = 0;

            int RetVal = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetScannableTypes(out SupportedTypes);
            if (RetVal != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                GBMSAPI_Example_Util.GBMSAPI_Example_ManageErrors(RetVal,
                    "GetObjectsToScanListFromObjectType,GBMSAPI_GetScannableTypes");
                return null;
            }

            if ((ObjType & SupportedTypes) == 0)
            {
                MessageBox.Show("GetObjectsToScanListFromObjectType error: Object not supported");
                return null;
            }

            switch (ObjType)
            {
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RollLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RollLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.RollLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.RollLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.RollLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.RollRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.RollRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.RollRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.RollRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.RollRightThumb_Str;

                        return ObjToScanList;
                    }

                // ver 3.2.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_UP:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftThumb_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftIndex_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftLittle_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftRing_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftLittle_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.RolledTopRightThumb_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.RolledTopRightIndex_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.RolledTopRightMiddle_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.RolledTopRightRing_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.RolledTopRightLittle_Str;

                        return ObjToScanList;
                    }
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_DOWN:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftThumb_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftIndex_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftLittle_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftRing_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.RolledDownRightLittle_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.RolledDownRightThumb_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.RolledDownRightIndex_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.RolledDownRightMiddle_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.RolledDownRightRing_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.RolledDownRightLittle_Str;

                        return ObjToScanList;
                    }
                // end ver 3.2.0.0

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SINGLE_FINGER:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.FlatLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.FlatLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.FlatLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.FlatLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.FlatLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.FlatRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.FlatRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.FlatRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.FlatRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.FlatRightThumb_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_THUMBS_2:
                    {
                        String[] ObjToScanList = new String[1];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.Slap2Thumbs_Str;

                        return ObjToScanList;
                    }

                // VER 2000
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_INDEXES_2:
                    {
                        String[] ObjToScanList = new String[1];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.Slap2Indexes_Str;

                        return ObjToScanList;
                    }
                // end VER 2000

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_4:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.Slap4Right_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.Slap4Left_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_2:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.Slap2Right_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.Slap2Left_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_UPPER_HALF_PALM:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.UpperHalfPalmRight_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.UpperHalfPalmLeft_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.LowerHalfPalmRight_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.LowerHalfPalmLeft_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_WRITER_PALM:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.WriterPalmRight_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.WriterPalmLeft_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO:
                    {
                        String[] ObjToScanList = new String[1];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.Photo_Str;

                        return ObjToScanList;
                    }
                // Ver 3.3.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_HYPOTHENAR:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledHypoThenarRight_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledHypoThenarLeft_Str;

                        return ObjToScanList;
                    }
                // end Ver 3.3.0.0

                // Ver 2.10.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_THENAR:
                    {
                        String[] ObjToScanList = new String[2];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledThenarRight_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledThenarLeft_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.RolledJointRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.RolledJointRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.RolledJointRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.RolledJointRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.RolledJointRightThumb_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_LEFT_SIDE:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightThumb_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_RIGHT_SIDE:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightThumb_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_TIP:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.RolledTipRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.RolledTipRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.RolledTipRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.RolledTipRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.RolledTipRightThumb_Str;

                        return ObjToScanList;
                    }

                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT_CENTER:
                    {
                        String[] ObjToScanList = new String[10];
                        if (ObjToScanList == null)
                        {
                            MessageBox.Show("MEMORY ERROR!!! Close application");
                            return null;
                        }
                        ObjToScanList[0] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftIndex_Str;
                        ObjToScanList[1] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftMiddle_Str;
                        ObjToScanList[2] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftRing_Str;
                        ObjToScanList[3] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftLittle_Str;
                        ObjToScanList[4] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftThumb_Str;
                        ObjToScanList[5] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightIndex_Str;
                        ObjToScanList[6] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightMiddle_Str;
                        ObjToScanList[7] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightRing_Str;
                        ObjToScanList[8] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightLittle_Str;
                        ObjToScanList[9] = GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightThumb_Str;

                        return ObjToScanList;
                    }
                // end Ver 2.10.0.0
            }

            return null;
        }

        public static uint GetObjectToScanIDFromName(String ObjToScanName)
        {
            // FLAT FINGERS
            if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.FlatLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_LITTLE;
            }
            // ROLL FINGERS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RollLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE;
            }
            // ver 3.2.0.0
            // ROLLED UPFINGERS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTopLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_LITTLE;
            }
            // ROLLED DOWN FINGERS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledDownLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_LITTLE;
            }
            // end ver 3.2.0.0
            // TWO THUMBS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Slap2Thumbs_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS;
            }
            // VER 2000
            // TWO THUMBS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Slap2Indexes_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_INDEXES;
            }
            // end VER 2000
            // FOUR FINGERS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Slap4Left_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Slap4Right_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT;
            }
            // TWO FINGERS
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Slap2Left_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Slap2Right_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT;
            }
            // UPPER HALF PALM
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.UpperHalfPalmLeft_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.UpperHalfPalmRight_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT;
            }
            // LOWER HALF PALM
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.LowerHalfPalmLeft_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.LowerHalfPalmRight_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_RIGHT;
            }
            // WRITER PALM
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.WriterPalmLeft_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.WriterPalmRight_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_RIGHT;
            }
            // PHOTO
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.Photo_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PHOTO;
            }
            // Ver 3.3.0.0
            // ROLLED THENAR
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledHypoThenarLeft_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_HYPOTHENAR_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledHypoThenarRight_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_HYPOTHENAR_RIGHT;
            }
            // end Ver 3.3.0.0
            // Ver 2.10.0.0
            // ROLLED THENAR
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledThenarLeft_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_THENAR_LEFT;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledThenarRight_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_THENAR_RIGHT;
            }
            // ROLLED JOINT
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_LITTLE;
            }
            // PLAIN JOINT LEFT SIDE
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_LITTLE;
            }
            // PLAIN JOINT RIGHT SIDE
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_LITTLE;
            }
            // ROLLED JOINT CENTER
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_LITTLE;
            }
            // ROLLED TIP
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipRightThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipRightIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipRightMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipRightRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipRightLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_LITTLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftThumb_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_THUMB;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftIndex_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_INDEX;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftMiddle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_MIDDLE;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftRing_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_RING;
            }
            else if (ObjToScanName == GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftLittle_Str)
            {
                return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_LITTLE;
            }
            // end Ver 2.10.0.0
            else return 0;
        }

        public static List<String> GetDiagStringsFromDiagMask(uint DiagMask)
        {
            List<String> RetVal = new List<String>();

            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_SURFACE_NOT_NORMA) != 0)
            {
                RetVal.Add(DiagnosticStrings.ScannerSurfaceNotNormalDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_FAILURE) != 0)
            {
                RetVal.Add(DiagnosticStrings.ScannerFailureDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_COMPOSITION_SLOW) != 0)
            {
                RetVal.Add(DiagnosticStrings.CompositionSlowDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_SLIDING) != 0)
            {
                RetVal.Add(DiagnosticStrings.FlatFingerSlidingDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_EXT_LIGHT_TOO_STRONG) != 0)
            {
                RetVal.Add(DiagnosticStrings.ExternalLightDiagMessString);
            }
            if (((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_LEFT) != 0) ||
                ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_OUTSIDE_BORDER_LEFT) != 0))
            {
                RetVal.Add(DiagnosticStrings.FingerOutOfRegionLeftDiagMessString);
            }
            if (((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_RIGHT) != 0) ||
                ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_OUTSIDE_BORDER_RIGHT) != 0))
            {
                RetVal.Add(DiagnosticStrings.FingerOutOfRegionRightDiagMessString);
            }
            if (((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_TOP) != 0) ||
                ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_OUTSIDE_BORDER_TOP)) != 0)
            {
                RetVal.Add(DiagnosticStrings.FingerOutOfRegionTopDiagMessString);
            }
            if (((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_DISPLACED_DOWN) != 0) ||
                ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_DISPLACED_DOWN) != 0))
            {
                RetVal.Add(DiagnosticStrings.FingerDisplacedDownDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_IMPROPER_ROLL) != 0)
            {
                RetVal.Add(DiagnosticStrings.ImproperRollDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_TOO_FAST_ROLL) != 0)
            {
                RetVal.Add(DiagnosticStrings.TooFastRollDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_TOO_NARROW_ROLL) != 0)
            {
                RetVal.Add(DiagnosticStrings.TooNarrowRollDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_RIGHT) != 0)
            {
                RetVal.Add(DiagnosticStrings.RollDirRightDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_LEFT) != 0)
            {
                RetVal.Add(DiagnosticStrings.RollDirLeftDiagMessString);
            }
            // VER 2.9.0.0
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_DRY_FINGER) != 0)
            {
                RetVal.Add(DiagnosticStrings.DryFingerDiagMessString);
            }
            // VER 2.9.0.0
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_WET_FINGER) != 0)
            {
                RetVal.Add(DiagnosticStrings.WetFingerDiagMessString);
            }
            // VER 3.1.0.0
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_TOO_SHORT_VERTICAL_ROLL) != 0)
            {
                RetVal.Add(DiagnosticStrings.TooShortVerticalRollDiagMessString);
            }
            // end VER 3.1.0.0
            // VER 3.2.0.0
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_UP) != 0)
            {
                RetVal.Add(DiagnosticStrings.RollDirUpDiagMessString);
            }
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_DOWN) != 0)
            {
                RetVal.Add(DiagnosticStrings.RollDirDownDiagMessString);
            }
            // end VER 3.2.0.0
            // ver 4.1.0.0
            if ((DiagMask & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FAKE_FINGER_DETECTED_AUTO_CAPTURE_BLOCKED) != 0)
            {
                bool FfdFlag;
                uint FfdDiag;
                int FfdError = GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_HardwareFakeFingerDetection(out FfdFlag, out FfdDiag);
                if (FfdError == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    List<String> FFdDiagList = GBMSAPI_Example_Util.GetFfdDiagStringsFromDiagMask(FfdDiag, true);
                    if (FFdDiagList.Count > 0)
                    {
                        foreach (String item in FFdDiagList)
                        {
                            RetVal.Add(item);
                        }
                    }
                    else
                    {
                        RetVal.Add(DiagnosticStrings.AutoCaptureBlockedForFakeFingerDetectionString);
                    }
                }
                else
                {
                    RetVal.Add(DiagnosticStrings.AutoCaptureBlockedForFakeFingerDetectionString);
                }
            }
            // end ver 4.1.0.0

            return RetVal;
        }

        // VER 4.0.0.0
        // ver 4.1.0.0: strings re-written for more meaningful messages
        public static List<String> GetFfdDiagStringsFromDiagMask(uint DiagMask, bool isAutocaptureBlocked)
        {
            List<String> RetVal = new List<String>();
            if (DiagMask == 0)
            {
                if (isAutocaptureBlocked) RetVal.Add("Fake finger detected, lift finger and retry");
                else RetVal.Add("Fake finger detected");
            }
            else
            {
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_AREA_SMALL) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Area Small (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Area Small (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_DISPLACED_UP) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Displaced Up (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Displaced Up (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_DISPLACED_DOWN) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Displaced Down (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Displaced Down (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_DISPLACED_LEFT) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Displaced Left (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Displaced Left (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_DISPLACED_RIGHT) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Displaced Right (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Displaced Right (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_SHIFT_DETECTED) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Shift Detected (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Shift Detected (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_FINGER_REMOVED_TOO_EARLY) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Finger Removed Too Early (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Finger Removed Too Early (classified as FAKE)");
                }
                if ((DiagMask & GBMSAPI_NET_HwAntifakeDiagnosticValues.GBMSAPI_NET_HW_ANTIFAKE_DIAGNOSTIC_PRESS_FINGER_HARDER) != 0)
                {
                    if (isAutocaptureBlocked) RetVal.Add("Press Finger Harder (classified as FAKE), lift finger and retry");
                    else RetVal.Add("Press Finger Harder (classified as FAKE)");
                }
            }

            return RetVal;
        }
        // end ver 4.1.0.0: strings re-written for more meaningful messages
        // end VER 4.0.0.0

        public static String GetObjectToScanNameFromID(uint ObjToScan)
        {
            switch (ObjToScan)
            {
                // FLAT FINGERS
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.FlatLeftLittle_Str;
                    }
                // ROLL FINGERS
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RollLeftLittle_Str;
                    }
                // TWO THUMBS 
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Slap2Thumbs_Str;
                    }
                // VER 2000
                // TWO INDEXES 
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_INDEXES:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Slap2Indexes_Str;
                    }
                // end VER 2000
                // FOUR FINGERS
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Slap4Left_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Slap4Right_Str;
                    }
                // TWO FINGERS
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Slap2Left_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Slap2Right_Str;
                    }
                // UPPER HALF PALM
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.UpperHalfPalmLeft_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.UpperHalfPalmRight_Str;
                    }
                // LOWER HALF PALM
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.LowerHalfPalmLeft_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.LowerHalfPalmRight_Str;
                    }
                // WRITER PALM
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.WriterPalmLeft_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.WriterPalmRight_Str;
                    }
                // PHOTO
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PHOTO:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.Photo_Str;
                    }
                // Ver 2.10.0.0
                // ROLLED THENAR
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_THENAR_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledThenarLeft_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_THENAR_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledThenarRight_Str;
                    }
                // ROLLED JOINT
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointLeftLittle_Str;
                    }
                // PLAIN JOINT LEFT SIDE
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointLeftSideLeftLittle_Str;
                    }
                // PLAIN JOINT RIGHT SIDE
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.PlainJointRightSideLeftLittle_Str;
                    }
                // ROLLED JOINT CENTER
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledJointCenterLeftLittle_Str;
                    }
                // ROLLED TIP
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipRightThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipRightIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipRightMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipRightRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipRightLittle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_THUMB:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftThumb_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_INDEX:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftIndex_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_MIDDLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftMiddle_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_RING:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftRing_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_LITTLE:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledTipLeftLittle_Str;
                    }
                // end Ver 2.10.0.0
                // Ver 3.3.0.0
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_HYPOTHENAR_LEFT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledHypoThenarLeft_Str;
                    }
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_HYPOTHENAR_RIGHT:
                    {
                        return GBMSAPI_Example_ScannableObjectStrings.RolledHypoThenarRight_Str;
                    }
            }

            return null;
        }

        public static String GetAutoCapturePhaseStringFromValue(int AutoCapturePhaseValue)
        {
            switch (AutoCapturePhaseValue)
            {
                case (GBMSAPI_NET_AutoCapturePhase.GBMSAPI_NET_ACP_OFF):
                    {
                        return AutoCapturePhaseStrings.AutoCaptureOffString;
                    }
                case (GBMSAPI_NET_AutoCapturePhase.GBMSAPI_NET_ACP_WAIT_FINGER):
                    {
                        return AutoCapturePhaseStrings.AutoCaptureWaitFingerString;
                    }
                case (GBMSAPI_NET_AutoCapturePhase.GBMSAPI_NET_ACP_SELECT_IMAGE):
                    {
                        return AutoCapturePhaseStrings.AutoCaptureSelectImageString;
                    }
                case (GBMSAPI_NET_AutoCapturePhase.GBMSAPI_NET_ACP_BEST_IMAGE_STOP):
                    {
                        return AutoCapturePhaseStrings.AutoCaptureBestImageStop;
                    }
                case (-1):
                    {
                        return AutoCapturePhaseStrings.AutoCaptureBlocked;
                    }
                default:
                    return AutoCapturePhaseStrings.AutoCaptureOffString;
            }
        }

        // ver 2.10.0.0
        public static UInt32 GetScanAreaFromObjectType(UInt32 objType, bool flatFingerOnRollArea, bool rollAreaGA)
        {
            switch (objType)
            {
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_INDEXES_2: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SINGLE_FINGER:
                    {
                        if (flatFingerOnRollArea)
                        {
                            if (rollAreaGA) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                            else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                        }
                        else
                        {
                            return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                        }
                    }
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_2:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_4:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_THUMBS_2: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_UPPER_HALF_PALM:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_WRITER_PALM: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_PHOTO;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_LEFT_SIDE:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_RIGHT_SIDE:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER:
                    {
                        if (rollAreaGA) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                    }
                // ver 3.2.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_UP:
                    {
                        if (rollAreaGA) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                    }
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_DOWN:
                    {
                        if (rollAreaGA) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                    }
                // end ver 3.2.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT_CENTER: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_THENAR: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_TIP: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                // Ver 3.3.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_HYPOTHENAR: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR;
                // end Ver 3.3.0.0
                default: return 0;
            }
        }

        // ver 3.1.0.0
        public static void GBMSAPI_Example_LogError(string strToLog)
        {
            System.IO.File.AppendAllText("GBMSAPI_EX_CS_LOG.log", strToLog + "\n");
        }

        public static void GBMSAPI_Example_LogStart()
        {
            System.IO.File.WriteAllText("GBMSAPI_EX_CS_LOG.log", "");
        }

        public static void GBMSAPI_Example_LogBuffer(string path, byte[] buffToSave)
        {
            System.IO.File.WriteAllBytes(path, buffToSave);
        }
        // end ver 3.1.0.0

        public static void CopyRawImageIntoBitmap(Byte[] RawImage, ref Bitmap bmpImage)
        {
            try
            {
                BitmapData bmData;
                int iw = bmpImage.Width, ih = bmpImage.Height;
                Rectangle bmpRect = new Rectangle(0, 0, iw, ih);

                bmpImage = new Bitmap(iw, ih, PixelFormat.Format8bppIndexed);

                bmData = bmpImage.LockBits(bmpRect,
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format8bppIndexed);

                int diff = bmData.Stride - bmpImage.Width;
                if (diff == 0)
                {
                    Marshal.Copy(RawImage, 0, bmData.Scan0, bmpImage.Width * bmpImage.Height);
                }
                else
                {
                    int RawIndex = 0;
                    int Ptr = (int)bmData.Scan0;
                    for (int i = 0; i < bmpImage.Height; i++, RawIndex += bmpImage.Width)
                    {
                        Marshal.Copy(RawImage, RawIndex, (IntPtr)Ptr, bmpImage.Width);
                        Ptr += bmData.Stride;
                    }
                }

                // Unlock the bits
                bmpImage.UnlockBits(bmData);

                ColorPalette pal = bmpImage.Palette;
                for (int i = 0; i < pal.Entries.Length; i++)
                {
                    pal.Entries[i] = Color.FromArgb(255, i, i, i);
                }
                bmpImage.Palette = pal;
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message + " in CopyRawImageIntoBitmap", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        // end ver 2.10.0.0

        // ver 3.1.0.0
        public static void CalculatePaintBoxSizeAndLocation(
            uint imsx, uint imsy,
            System.Drawing.Size pbOriginalSize, System.Drawing.Point pbOriginalPoint,
            out System.Drawing.Size pbSize, out System.Drawing.Point pbLocation)
        {
            pbLocation = new Point(0, 0);
            pbSize = new Size(0, 0);

            if (imsx == imsy)
            {
                pbLocation = pbOriginalPoint;
                pbSize = pbOriginalSize;
                return;
            }
            if (imsx > imsy)
            {
                pbLocation = pbOriginalPoint;
                double dIsx = (double)imsx, dIsy = (double)imsy,
                    orSx = (double)pbOriginalSize.Width, orSy = (double)pbOriginalSize.Height,
                    nSx, nsY;

                nSx = orSx;
                nsY = orSy * (dIsy / dIsx);
                pbSize = new Size((int)nSx, (int)nsY);
                return;
            }
            else
            {
                double dIsx = (double)imsx, dIsy = (double)imsy,
                    orSx = (double)pbOriginalSize.Width, orSy = (double)pbOriginalSize.Height,
                    nSx, nSy;
                double orLx = (double)pbOriginalPoint.X, orLy = (double)pbOriginalPoint.Y,
                    nLx, nLy;

                nSy = orSy;
                nSx = orSx * (dIsx / dIsy);
                pbSize = new Size((int)nSx, (int)nSy);
                nLy = orLy;
                nLx = orLx + (orSx - nSx) / 2;
                pbLocation = new Point((int)nLx, (int)nLy);
                return;
            }
        }
        // end ver 3.1.0.0

        // ver 3.1.0.0
        public static bool IsRolled(UInt32 ObjToScanType)
        {
            if ((ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER) ||
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT) ||
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT_CENTER) ||
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_THENAR) ||
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_TIP) ||
                // ver 3.2.0.0
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_UP) ||
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_DOWN) ||
                // end ver 3.2.0.0 ||
                // ver 3.3.0.0
                (ObjToScanType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_HYPOTHENAR)
                // end ver 3.3.0.0
            )
            {
                return true;
            }
            return false;
        }
        // end ver 3.1.0.0
    }


}
