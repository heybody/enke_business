﻿using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//kqz 2017-1-1添加 
namespace DTU.GateWay.Protocol //水位预警阈值设置
{ 
    public class WaterCmd_28_1:WaterBaseMessage
    {
        public WaterCmd_28_1()
        {
            AFN = (byte)WaterBaseProtocol.AFN._28;
            UpOrDown = (int)WaterBaseProtocol.UpOrDown.Down;
            DataBeginChar = (byte)WaterBaseProtocol.DataBeginChar.STX;
            DataEndChar = (byte)WaterBaseProtocol.DataEndChar_Down.ENQ;
            SerialNumber = 0;
        }

        /// <summary>
        /// 流水号
        /// </summary>
        public short SerialNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 发报时间
        /// </summary>
        public DateTime SendTime
        {
            get;
            set;
        }
        /// <summary>
        /// 黄色水位预警值
        /// </summary>
        public short YellowLevel
        {
            get;
            set;
        }
        /// <summary>
        /// 橙色水位预警值
        /// </summary>
        public short OrangeLevel
        {
            get;
            set;
        }
        /// <summary>
        /// 红色水位预警值
        /// </summary>
        public short RedLevel
        {
            get;
            set;
        }

        public string WriteMsg()
        {
            UserData = "";
            UserData += SerialNumber.ToString("X").PadLeft(4, '0');
            UserData += SendTime.ToString("yyMMddHHmmss").PadLeft(12, '0');

            UserData += YellowLevel.ToString("X").PadLeft(4, '0');
            UserData += OrangeLevel.ToString("X").PadLeft(4, '0');
            UserData += RedLevel.ToString("X").PadLeft(4, '0');

            UserDataBytes = HexStringUtility.HexStringToByteArray(UserData);
            return WriteMsgBase();
        }

        public string ReadMsg()
        {
            if (UserDataBytes == null || UserDataBytes.Length == 0)
            {
                if (ShowLog) logHelper.Error("无信息，无法分析！");
                return "无信息，无法分析！";
            }

            UserData = HexStringUtility.ByteArrayToHexString(UserDataBytes);

            try
            {
                SerialNumber = Convert.ToInt16(UserData.Substring(0, 4), 16);
            }
            catch (Exception ex)
            {
                if (ShowLog) logHelper.Error(ex.Message + Environment.NewLine + "获取流水号出错" + " " + RawDataStr);
                return "获取流水号出错";
            }

            try
            {
                SendTime = DateTime.ParseExact("20" + UserData.Substring(4, 12), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                if (ShowLog) logHelper.Error(ex.Message + Environment.NewLine + "获取发报时间出错" + " " + RawDataStr);
                return "获取发报时间出错";
            }

            try
            {
                YellowLevel = Convert.ToInt16(UserData.Substring(16, 4), 16);
            }
            catch (Exception ex)
            {
                if (ShowLog) logHelper.Error(ex.Message + Environment.NewLine + "获取水位黄色预警阈值出错" + " " + RawDataStr);
                return "获取水位黄色预警阈值出错";
            }
            try
            {
                OrangeLevel = Convert.ToInt16(UserData.Substring(20, 4), 16);
            }
            catch (Exception ex)
            {
                if (ShowLog) logHelper.Error(ex.Message + Environment.NewLine + "获取水位橙色预警阈值出错" + " " + RawDataStr);
                return "获取水位橙色预警阈值出错";
            }
            try
            {
                RedLevel = Convert.ToInt16(UserData.Substring(24, 4), 16);
            }
            catch (Exception ex)
            {
                if (ShowLog) logHelper.Error(ex.Message + Environment.NewLine + "获取水位红色预警阈值出错" + " " + RawDataStr);
                return "获取水位红色预警阈值出错";
            }
            return "";
        }
    }
}
//kqz 2017-1-1添加