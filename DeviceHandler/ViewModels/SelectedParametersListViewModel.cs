
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using Services.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using System.IO;
using DeviceHandler.Services;
using System.Text;
using DeviceCommunicators.MCU;
using Entities.Enums;

namespace DeviceHandler.ViewModels
{
	public class SelectedParametersListViewModel: ObservableObject
	{
		#region Properties

		public string Title { get; set; }

		
		public ObservableCollection<DeviceParameterData> ParametersList { get; set; }
		public ObservableCollection<RecordData> ParametersList_WithIndex { get; set; }

		public bool IsLimitParametersList { get; set; }
		public int LimitOfParametersList { get; set; }

		#endregion Properties

		#region Fields


		protected DevicesContainer _devicesContainer;


		private System.Collections.IList _selectedItemsList;

		public const string DragDropFormat = "RecordParameter";

		private DragDropData _designDragDropData;

		private SelectedParamsList_MoveService _selectedParamsList_Move;


		#endregion Fields

		#region Constructor

		public SelectedParametersListViewModel(
			DevicesContainer devicesContainer,
			string title)
		{
			IsLimitParametersList = false;

			_devicesContainer = devicesContainer;
			Title = title;

			_designDragDropData = new DragDropData();


			DeletParameterLogListCommand = new RelayCommand<System.Collections.IList>(DeletParameterLogList);
			MoveUpCommand = new RelayCommand<RecordData>(MoveUp);
			MoveDownCommand = new RelayCommand<RecordData>(MoveDown);
			SaveParametersListCommand = new RelayCommand(SaveParametersList);
			LoadParametersListCommand = new RelayCommand(LoadParametersList);

			_selectedParamsList_Move = new SelectedParamsList_MoveService();

			ParametersList = new ObservableCollection<DeviceParameterData>();
			ParametersList_WithIndex = new ObservableCollection<RecordData>();
		}

		#endregion Constructor

		#region Methods

		#region Save / Load

		public void SaveParametersList()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "JSON Files | *.json|Text Files | *.txt";
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = saveFileDialog.FileName;
			
			string extension = Path.GetExtension(path);

			if (extension == ".json")
			{
				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;
				settings.TypeNameHandling = TypeNameHandling.All;
				var sz = JsonConvert.SerializeObject(ParametersList, settings);
				System.IO.File.WriteAllText(path, sz);
			}
			else if (extension == ".txt")
			{
				StringBuilder text = new StringBuilder();
				foreach (DeviceParameterData p in ParametersList)
				{
					string name = p.Name.Replace("\n", "\\n");
					name += " ;; " + "MCU";
					text.AppendLine(name);
				}

				//! Create text file:
				File.WriteAllText(path, text.ToString());
			}
		}

		public void LoadParametersList()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files | *.json|Text Files | *.txt";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = openFileDialog.FileName;

			string extension = Path.GetExtension(path);
			ObservableCollection<DeviceParameterData> parametersList = null;
			if (extension == ".json")
			{
				FixJson(path);


				string jsonString = System.IO.File.ReadAllText(path);

				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;
				settings.TypeNameHandling = TypeNameHandling.All;
				parametersList = JsonConvert.DeserializeObject(jsonString, settings) as
					ObservableCollection<DeviceParameterData>;

				GetActualParameters_Json(parametersList);
			}
			else if (extension == ".txt")
			{
				string paramsString = System.IO.File.ReadAllText(path);
				string[] paramsList = paramsString.Split("\r\n");

				GetActualParameters_txt(paramsList.ToList());
			}


			
			SendRECORD_LIST_CHANGEDMessage();
		}

		#endregion Save / Load

		protected void GetActualParameters_Json(ObservableCollection<DeviceParameterData> parametersList)
		{
			if (ParametersList == null)
				ParametersList = new ObservableCollection<DeviceParameterData>();

			if (ParametersList_WithIndex == null)
				ParametersList_WithIndex = new ObservableCollection<RecordData>();

			ParametersList.Clear();
			ParametersList_WithIndex.Clear();

			foreach (DeviceParameterData parameterData in parametersList)
			{
				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(parameterData.DeviceType) == false)
					continue;

				DeviceFullData deviceFullData =
					_devicesContainer.TypeToDevicesFullData[parameterData.DeviceType];
				if (deviceFullData == null || deviceFullData.Device == null)
					continue;

				DeviceParameterData actualParameterData =
					deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == parameterData.Name);
				if (actualParameterData == null)
					continue;

				ParametersList.Add(actualParameterData);
				ParametersList_WithIndex.Add(new RecordData() { Data = actualParameterData });
			}



			while (IsLimitParametersList && ParametersList.Count > LimitOfParametersList)
			{
				ParametersList.RemoveAt(ParametersList.Count - 1);
				ParametersList_WithIndex.RemoveAt(ParametersList.Count - 1);
			}

			SetIndeces();
		}

		protected void GetActualParameters_txt(List<string> parametersList)
		{
			ParametersList.Clear();
			ParametersList_WithIndex.Clear();

			foreach (string param in parametersList)
			{
				if (string.IsNullOrEmpty(param))
					continue;

				string[] parts = param.Split(" ;; ");
				if (parts.Length < 2)
					continue;

				string name = parts[0];
				DeviceTypesEnum deviceType;
				bool res = Enum.TryParse(parts[1], out deviceType);

				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(deviceType) == false)
					continue;

				DeviceFullData deviceFullData = _devicesContainer.TypeToDevicesFullData[deviceType];

				DeviceParameterData actualParameterData =
					deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == name);
				if (actualParameterData == null)
					continue;

				ParametersList.Add(actualParameterData);
				ParametersList_WithIndex.Add(new RecordData() { Data = actualParameterData });

			}

			SetIndeces();
		}


		private void DeletParameterLogList(System.Collections.IList paramsList)
		{
			List<RecordData> list = new List<RecordData>();
			foreach (RecordData data in paramsList)
				list.Add(data);

			foreach (RecordData data in list)
			{
				ParametersList.Remove(data.Data);
				ParametersList_WithIndex.Remove(data);
			}

			SetIndeces();

			SendRECORD_LIST_CHANGEDMessage();
		}

		private void FixJson(string path)
		{
			string fileData = null;
			using (StreamReader sr = new StreamReader(path))
			{
				fileData = sr.ReadToEnd();
			}

			fileData = fileData.Replace(
				"Entities.Models.DeviceData, Entities",
				"DeviceCommunicators.Models.DeviceData, DeviceCommunicators");

			fileData = fileData.Replace(
				"Entities.Models.DeviceParameterData, Entities",
				"DeviceCommunicators.Models.DeviceParameterData, DeviceCommunicators");

			using (StreamWriter sw = new StreamWriter(path))
			{
				sw.Write(fileData);
			}
		}

		#region Drag

		private void RecordingList_MouseEnter(MouseEventArgs e)
		{
			if (_designDragDropData.IsIgnor)
				return;

			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				_designDragDropData.IsMouseDown = true;
			else
				_designDragDropData.IsMouseDown = false;
		}

		private void RecordingList_PreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (_designDragDropData.IsIgnor)
				return;

			_designDragDropData.IsMouseDown = true;
			_designDragDropData.StartPoint = e.GetPosition(null);
		}

		private void RecordingList_PreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_designDragDropData.IsMouseDown = false;
		}

		private void RecordingList_MouseMove(MouseEventArgs e)
		{
			if (_designDragDropData.IsMouseDown == false)
				return;

			DragObject(e);
		}

		private void DragObject(MouseEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is draged");

			Point mousePos = e.GetPosition(null);
			Vector diff = _designDragDropData.StartPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
				Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{

				ListView listView =
					FindAncestorService.FindAncestor<ListView>((DependencyObject)e.OriginalSource);
				if (listView == null)
					return;

				ListViewItem listViewItem =
						FindAncestorService.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
				if (listViewItem == null)
					return;


				if (!listView.SelectedItems.Contains(listViewItem.DataContext))
				{
					DataObject dragData = new DataObject(DragDropFormat, listViewItem.DataContext);
					DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
				}
				else
				{
					DataObject dragData = new DataObject(DragDropFormat, listView.SelectedItems);
					DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Move);
				}

			}
		}



		#endregion Drag


		#region Drop

		private void RecordingList_Drop(DragEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is dropped");

			if (e.Data.GetDataPresent(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat))
			{
				int droppedOnIndex = -1;
				ListViewItem listViewItem =
						FindAncestorService.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
				if (listViewItem != null)
				{
					if (listViewItem.DataContext is RecordData recordData)
					{
						droppedOnIndex = ParametersList_WithIndex.IndexOf(recordData);
					}
				}




				var dragData = e.Data.GetData(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat);

				

				if (dragData is ObservableCollection<object> list)
				{
					foreach (object obj in list)
					{
						if (!(obj is DeviceParameterData param))
							continue;

						if (IsLimitParametersList && ParametersList.Count == LimitOfParametersList)
						{
							MessageBox.Show("Only up to 40 parameters are allowed");
							return;
						}

						AddParamToLogList(param, droppedOnIndex);

					}
				}
				else if (dragData is DeviceParameterData param)
				{
					LoggerService.Inforamtion(this, "Dropping parameter \"" + param.Name + "\"");
					AddParamToLogList(param, droppedOnIndex);
				}



			}
			else if (e.Data.GetDataPresent(DragDropFormat))
			{
				var dragData = e.Data.GetData(DragDropFormat);

				if (dragData is System.Collections.IList)
				{

					RecordData droppedOn = null;
					ListViewItem listViewItem =
						FindAncestorService.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
					if (listViewItem != null)
					{
						droppedOn = listViewItem.DataContext as RecordData;
						if (droppedOn == null)
							return;
					}

					MoveGroupOfParam(droppedOn);


				}
			}
		}

		private void AddParamToLogList(
			DeviceParameterData param,
			int droppedOnIndex)
		{
			int index = ParametersList.IndexOf(param);
			if (index >= 0)
			{
				MessageBox.Show("The parameter already exist");
				return;
			}

			if (droppedOnIndex == -1)
			{
				ParametersList.Add(param);
				ParametersList_WithIndex.Add(new RecordData() { Data = param });
			}
			else
			{
				ParametersList.Insert(droppedOnIndex, param);
				ParametersList_WithIndex.Insert(droppedOnIndex, new RecordData() { Data = param });
			}

			SetIndeces();

			SendRECORD_LIST_CHANGEDMessage();
		}


		private void RecordingList_DragEnter(DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat))
			{
				e.Effects = DragDropEffects.None;
			}
		}

		#endregion Drop		


		#region Move UP/DOWN

		private void MoveGroupOfParam(
			RecordData droppedOnParam)
		{
			List<RecordData> list = new List<RecordData>();
			foreach (var item in _selectedItemsList)
				list.Add(item as RecordData);

			List<DeviceParameterData> parametersList = ParametersList.ToList();
			List<RecordData> parametersList_WithIndex = ParametersList_WithIndex.ToList();
			_selectedParamsList_Move.MoveByDragAndDrop(
				parametersList,
				parametersList_WithIndex,
				list,
				droppedOnParam);


			ParametersList =
				new ObservableCollection<DeviceParameterData>(parametersList);
			ParametersList_WithIndex =
				new ObservableCollection<RecordData>(parametersList_WithIndex);


			SetIndeces();

			SendRECORD_LIST_CHANGEDMessage();
		}

		

		private void MoveUp(RecordData param)
		{
			LoggerService.Inforamtion(this, "Moving parameter UP");

			int index = ParametersList_WithIndex.IndexOf(param);
			if (index == 0)
				return;

			List<RecordData> list = new List<RecordData>();
			foreach (var item in _selectedItemsList)
				list.Add(item as RecordData);

			List<DeviceParameterData> parametersList = ParametersList.ToList();
			List<RecordData> parametersList_WithIndex = ParametersList_WithIndex.ToList();
			_selectedParamsList_Move.MoveByArrows(
				parametersList,
				parametersList_WithIndex,
				list,
				ParametersList_WithIndex[index - 1]);


			ParametersList =
				new ObservableCollection<DeviceParameterData>(parametersList);
			ParametersList_WithIndex =
				new ObservableCollection<RecordData>(parametersList_WithIndex);

			SetIndeces();

			SendRECORD_LIST_CHANGEDMessage();
		}

		private void MoveDown(RecordData param)
		{
			LoggerService.Inforamtion(this, "Moving node DOWN");

			int index = ParametersList_WithIndex.IndexOf(param);
			if (index == (ParametersList_WithIndex.Count - 1))
				return;

			List<RecordData> list = new List<RecordData>();
			foreach (var item in _selectedItemsList)
				list.Add(item as RecordData);

			List<DeviceParameterData> parametersList = ParametersList.ToList();
			List<RecordData> parametersList_WithIndex = ParametersList_WithIndex.ToList();
			_selectedParamsList_Move.MoveByArrows(
				parametersList,
				parametersList_WithIndex,
				list,
				ParametersList_WithIndex[index + 1]);


			ParametersList =
				new ObservableCollection<DeviceParameterData>(parametersList);
			ParametersList_WithIndex =
				new ObservableCollection<RecordData>(parametersList_WithIndex);


			SetIndeces();

			SendRECORD_LIST_CHANGEDMessage();
		}

		#endregion Move UP/DOWN

		private void RecordingList_SelectionChanged(SelectionChangedEventArgs e)
		{
			if (!(e.Source is ListView lv))
				return;

			_selectedItemsList = lv.SelectedItems;
		}

		private void RecordingList_KeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				DeletParameterLogList(_selectedItemsList);
			}
		}

		protected void SetIndeces()
		{
			for (int i = 0; i < ParametersList_WithIndex.Count; i++)
			{
				ParametersList_WithIndex[i].Index = i + 1;
			}
		}

		protected virtual void SendRECORD_LIST_CHANGEDMessage()
		{
			
		}

		#endregion Methods


		#region Commands

		public RelayCommand<System.Collections.IList> DeletParameterLogListCommand { get; private set; }
		public RelayCommand<RecordData> MoveUpCommand { get; private set; }
		public RelayCommand<RecordData> MoveDownCommand { get; private set; }

		public RelayCommand SaveParametersListCommand { get; private set; }
		public RelayCommand LoadParametersListCommand { get; private set; }


		#region Drag

		private RelayCommand<MouseEventArgs> _RecordingList_MouseEnterCommand;
		public RelayCommand<MouseEventArgs> RecordingList_MouseEnterCommand
		{
			get
			{
				return _RecordingList_MouseEnterCommand ?? (_RecordingList_MouseEnterCommand =
					new RelayCommand<MouseEventArgs>(RecordingList_MouseEnter));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _RecordingList_PreviewMouseLeftButtonDownCommant;
		public RelayCommand<MouseButtonEventArgs> RecordingList_PreviewMouseLeftButtonDownCommant
		{
			get
			{
				return _RecordingList_PreviewMouseLeftButtonDownCommant ?? (_RecordingList_PreviewMouseLeftButtonDownCommant =
					new RelayCommand<MouseButtonEventArgs>(RecordingList_PreviewMouseLeftButtonDown));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _RecordingList_PreviewMouseLeftButtonUpCommant;
		public RelayCommand<MouseButtonEventArgs> RecordingList_PreviewMouseLeftButtonUpCommant
		{
			get
			{
				return _RecordingList_PreviewMouseLeftButtonUpCommant ?? (_RecordingList_PreviewMouseLeftButtonUpCommant =
					new RelayCommand<MouseButtonEventArgs>(RecordingList_PreviewMouseLeftButtonUp));
			}
		}

		private RelayCommand<MouseEventArgs> _RecordingList_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> RecordingList_MouseMoveCommand
		{
			get
			{
				return _RecordingList_MouseMoveCommand ?? (_RecordingList_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(RecordingList_MouseMove));
			}
		}





		#endregion Drag


		#region Drop

		private RelayCommand<DragEventArgs> _RecordingList_DropCommand;
		public RelayCommand<DragEventArgs> RecordingList_DropCommand
		{
			get
			{
				return _RecordingList_DropCommand ?? (_RecordingList_DropCommand =
					new RelayCommand<DragEventArgs>(RecordingList_Drop));
			}
		}

		private RelayCommand<DragEventArgs> _RecordingList_DragEnterCommand;
		public RelayCommand<DragEventArgs> RecordingList_DragEnterCommand
		{
			get
			{
				return _RecordingList_DragEnterCommand ?? (_RecordingList_DragEnterCommand =
					new RelayCommand<DragEventArgs>(RecordingList_DragEnter));
			}
		}

		#endregion Drop



		private RelayCommand<SelectionChangedEventArgs> _RecordingList_SelectionChangedCommand;
		public RelayCommand<SelectionChangedEventArgs> RecordingList_SelectionChangedCommand
		{
			get
			{
				return _RecordingList_SelectionChangedCommand ?? (_RecordingList_SelectionChangedCommand =
					new RelayCommand<SelectionChangedEventArgs>(RecordingList_SelectionChanged));
			}
		}

		private RelayCommand<KeyEventArgs> _RecordingList_KeyDownCommand;
		public RelayCommand<KeyEventArgs> RecordingList_KeyDownCommand
		{
			get
			{
				return _RecordingList_KeyDownCommand ?? (_RecordingList_KeyDownCommand =
					new RelayCommand<KeyEventArgs>(RecordingList_KeyDown));
			}
		}


		#endregion Commands
	}
}
