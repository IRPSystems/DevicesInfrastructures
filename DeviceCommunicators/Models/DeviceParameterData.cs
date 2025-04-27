
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Enums;
using System.Windows;
using System;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Data;
using Services.Services;

namespace DeviceCommunicators.Models
{
	public class DeviceParameterData:ObservableObject, ICloneable
	{

		public string Name { get; set; }
		public string Units { get; set; }

		protected object _value;
		public virtual object Value
		{
			get
			{
				if (IsAbsolute && _value != null)
				{
					double d;
					bool res = double.TryParse(_value.ToString(), out d);
					if (!res)
						return _value;

					return Math.Abs(d);
				}
				else
					return _value;

			}
			set
			{
				_value = value;


			}
		}

		[JsonIgnore]
		public virtual object EditValue { get; set; }


		[JsonIgnore]
		public DeviceData Device { get; set; }
		public DeviceTypesEnum DeviceType { get; set; }


		[JsonIgnore]
		public bool IsExpanded { get; set; }
		[JsonIgnore]
		public bool IsEnabled { get; set; }
		[JsonIgnore]
		public bool IsEditing { get; set; }
		[JsonIgnore]
		public bool IsSelected { get; set; }
		[JsonIgnore]
		public Visibility Visibility { get; set; }
		[JsonIgnore]
		public Visibility GetSetVisibility { get; set; }
		[JsonIgnore]
		public Brush Background { get; set; }
		[JsonIgnore]
		public Brush Foreground { get; set; }
		[JsonIgnore]
		public string ErrorDescription { get; set; }

		[JsonIgnore]
		public bool IsAbsolute { get; set; }

		public string ToolTip { get; set; }

		public int CommunicationTimeout { get; set; }

		[JsonIgnore]
		public CommSendResLog CommSendResLog { get; set; } = new();

		public DeviceParameterData()
		{
            
        }

		public enum SendOrRecieve
        {
            Send,
            Recieve
        }

        public virtual void UpdateSendResLog(string command, SendOrRecieve sendOrRecieve, string CommErrorMsg = "No Error", int amountOfRetries = 1)
        {
			try
			{
                if (sendOrRecieve == SendOrRecieve.Send)
                {
                    CommSendResLog.SendCommand = command;
                    CommSendResLog.CommErrorMsg = CommErrorMsg;
                }
                else
                {
                    CommSendResLog.ReceivedValue = command;
                    CommSendResLog.CommErrorMsg = CommErrorMsg;
                }
                CommSendResLog.NumberOfTries = amountOfRetries;
                CommSendResLog.timeStamp = DateTime.UtcNow;
            }
            catch (Exception ex)
			{
				LoggerService.Error(this, "Error while updating send res log: " + ex.InnerException.Message);
			}
        }

		public virtual object Clone()
		{
			return MemberwiseClone();
		}


		public override string ToString()
		{
			string str = Name;
			if(Device != null) 
				str += " (" + Device.Name + ")";

			return str;
		}
	}
}
