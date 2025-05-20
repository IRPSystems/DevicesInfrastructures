
using DBCFileParser.Model;
using DeviceCommunicators.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace DeviceCommunicators.DBC
{
	public class DBC_ParamData: DeviceParameterData
	{
		public Signal Signal { get; set; }
		public Message ParentMessage { get; set; }

		public int Interval { get; set; }
		public string IntervalUnite { get; set; }

		public double GetValue(byte[] buffer)
		{
			int byteLength = Signal.Length / 8;  // Signal.Length is in bits
			int startByte = Signal.StartBit / 8; // Signal.StartBit is in bits

			switch (byteLength)
			{
				case 1: Value = buffer[startByte]; break;
				case 2:
					if (Signal.ValueType == DbcValueType.Unsigned)
						Value = BitConverter.ToUInt16(buffer, startByte);
					else if (Signal.ValueType == DbcValueType.Signed)
						Value = BitConverter.ToInt16(buffer, startByte);
					break;
				case 3:
				case 4:
					byte[] buffer4Bytes = new byte[4];
					Array.Copy(buffer, startByte, buffer4Bytes, 0, byteLength);
					Get4BytesValue(buffer4Bytes, startByte);
					break;
				case 5:
				case 6:
				case 7:
				case 8:
					byte[] buffer8Bytes = new byte[8];
					Array.Copy(buffer, startByte, buffer8Bytes, 0, byteLength);
					Get8BytesValue(buffer8Bytes, startByte);
					break;
			}

			double dVal = Convert.ToDouble(Value);
			dVal += Signal.Offset;
			dVal *= Signal.Factor;
			return dVal;
		}

		private void Get4BytesValue(
			byte[] buffer4Bytes,
			int startByte)
		{
			if (Signal.ValueType == DbcValueType.Unsigned)
				Value = BitConverter.ToUInt32(buffer4Bytes, startByte);
			else if (Signal.ValueType == DbcValueType.Signed)
				Value = BitConverter.ToInt32(buffer4Bytes, startByte);
			else if (Signal.ValueType == DbcValueType.IEEEFloat)
				Value = BitConverter.ToSingle(buffer4Bytes, startByte);
		}

		private void Get8BytesValue(
			byte[] buffer8Bytes,
			int startByte)
		{
			if (Signal.ValueType == DbcValueType.Unsigned)
				Value = BitConverter.ToUInt64(buffer8Bytes, startByte);
			else if (Signal.ValueType == DbcValueType.Signed)
				Value = BitConverter.ToInt64(buffer8Bytes, startByte);
			else if (Signal.ValueType == DbcValueType.IEEEDouble)
				Value = BitConverter.ToDouble(buffer8Bytes, startByte);
		}
	}

	public class DBC_ParamGroup : DeviceParameterData
	{
		public uint ID { get; set; }

		public ObservableCollection<DBC_ParamData> ParamsList { get; set; }

		public Message Message { get; set; }

		public override string ToString()
		{
			return Name + " - " + ID;
		}

		public void HideNotVisibleGroups()
		{
			bool isVisible = false;
			foreach (DBC_ParamData param in ParamsList)
			{
				if (param.Visibility == Visibility.Visible)
				{
					isVisible = true;
					break;
				}
			}

			if (isVisible)
				Visibility = Visibility.Visible;
			else
				Visibility = Visibility.Collapsed;
		}
	}

	public class DBC_File : DeviceParameterData
	{
		public string FilePath { get; set; }
		public ObservableCollection<DBC_ParamGroup> ParamsList { get; set; }

		public void HideNotVisibleGroups()
		{
			foreach (DBC_ParamGroup group in ParamsList)
			{
				group.HideNotVisibleGroups();
			}
		}
	}
}
