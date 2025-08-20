
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeviceHandler.Plots
{
	public class RegisterViewModel: ObservableObject
	{
		public enum BitNumEnum { Bit8, Bit16, Bit32 };

		#region Properties
		public MCU_ParamData ParamData { get; set; }

		public ObservableCollection<RegisterBitData> RegisterBitsList { get; set; }

		public bool Is8Bit { get; set; }
		public bool Is16Bit { get; set; }
		public bool Is32Bit { get; set; }

		public string GroupName { get; set; }

		#endregion Properties


		#region Fields

		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		#endregion Fields

		#region Constructor

		public RegisterViewModel(
			MCU_ParamData paramData,
			BitNumEnum numOfBits = BitNumEnum.Bit16)
		{
			BitSizeChangedCommand = new RelayCommand(BitSizeChanged);

			ParamData = paramData;
			GroupName = "BitSize_" + paramData.Name;

			switch(numOfBits)
			{
				case BitNumEnum.Bit8: Is8Bit = true; break;
				case BitNumEnum.Bit16: Is16Bit = true; break;
				case BitNumEnum.Bit32: Is32Bit = true; break;
			}

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			BuildRegisterBitsList();
			HandleParameters();
		}

		#endregion Constructor

		#region Methods

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
		}

		private void BuildRegisterBitsList()
		{
			int bitsSize = 32;
			if (Is8Bit) bitsSize = 8;
			if (Is16Bit) bitsSize = 16;
			if (Is32Bit) bitsSize = 32;

			RegisterBitsList = new ObservableCollection<RegisterBitData>();
			if (ParamData.DropDown != null && ParamData.DropDown.Count > 0)
			{
				int counter = -1;
				for(int i = 0; i <= bitsSize; i++)
				{
					counter++;

					if (i >= ParamData.DropDown.Count)
					{
						AddBitNotInDropDown(counter);
						continue;
					}

					var bit = ParamData.DropDown[i];
					if (bit.Value == "0")
					{
						counter--;
						continue;
					}


					RegisterBitData bitData = new RegisterBitData()
					{
						Name = bit.Name,
						DecValue = bit.Value,
						Index = counter,
					};

					RegisterBitsList.Add(bitData);

					uint uVal;
					bool res = uint.TryParse(bit.Value, out uVal);
					if (res == false)
						continue;
					bitData.HexValue = "0x" + uVal.ToString("X");
				}
			}
			else
			{
				for (int i = 0; i < bitsSize; i++)
				{
					AddBitNotInDropDown(i);
				}
			}
		}

		private void HandleParameters()
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{

					ValueChanged();

					System.Threading.Thread.Sleep(40);
				}
			}, _cancellationToken);
		}

		private void AddBitNotInDropDown(int i)
		{
			int decValue = (int)Math.Pow(2, i);

			RegisterBitData bitData = new RegisterBitData()
			{
				Name = "N/A",
				DecValue = decValue.ToString(),
				Index = i,
			};

			RegisterBitsList.Add(bitData);

			bitData.HexValue = "0x" + decValue.ToString("X");
		}

		private void BitSizeChanged()
		{
			BuildRegisterBitsList();
		}

		private void ValueChanged()
		{
			try
			{
				int bitsSize = 0;
				if (Is8Bit) bitsSize = 8;
				if (Is16Bit) bitsSize = 16;
				if (Is32Bit) bitsSize = 32;

				double dVal = 0;
				if (ParamData.Value is string str)
				{
					if (str == null)
						return;

					bool res = double.TryParse(str, out dVal);
					if (res == false && ParamData.DropDown != null)
					{
						DropDownParamData dd =
							ParamData.DropDown.Find((i) => i.Name == str);
						if (dd == null)
							return;

						res = double.TryParse(dd.Value, out dVal);
						if (res == false)
							return;
					}
				}
				else
				{
					bool res = double.TryParse(ParamData.Value.ToString(), out dVal);
					if (res == false)
						return;
				}

				uint uVal = (uint)dVal;
				for (int i = 0; i < bitsSize && i < RegisterBitsList.Count; i++)
				{
					RegisterBitsList[i].IsOn = (((uVal >> i) & 1U) == 1U);
				}
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to set the value", ex);
			}
		}

		#endregion Methods

		#region Commands

		public RelayCommand BitSizeChangedCommand { get; set; }		

		#endregion Commands
	}
}
