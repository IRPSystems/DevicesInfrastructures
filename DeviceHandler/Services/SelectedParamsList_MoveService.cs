
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using System.Collections.Generic;
using System.Linq;

namespace DeviceHandler.Services
{
	public class SelectedParamsList_MoveService
	{

		public void MoveByArrows(
			List<DeviceParameterData> parametersList,
			List<RecordData> parametersList_WithIndex,
			List<RecordData> paramToMoveList,
			RecordData droppedOnParam)
		{
			int sourceIndex = parametersList_WithIndex.IndexOf(paramToMoveList[0]);
			int destIndex = parametersList_WithIndex.IndexOf(droppedOnParam);


			if (destIndex >= (parametersList_WithIndex.Count - paramToMoveList.Count + 1))
				return;

			parametersList.RemoveRange(sourceIndex, paramToMoveList.Count);

			parametersList_WithIndex.RemoveRange(sourceIndex, paramToMoveList.Count);

			List<DeviceParameterData> list =
				new List<DeviceParameterData>(paramToMoveList.Select((rp) => rp.Data));



			if (destIndex < 0)
			{
				parametersList.AddRange(list);
				parametersList_WithIndex.AddRange(paramToMoveList);
			}
			else
			{
				parametersList.InsertRange(destIndex, list);
				parametersList_WithIndex.InsertRange(destIndex, paramToMoveList);
			}

		}

		public void MoveByDragAndDrop(List<DeviceParameterData> parametersList,
			List<RecordData> parametersList_WithIndex,
			List<RecordData> paramToMoveList,
			RecordData droppedOnParam)
		{
			int sourceIndex = parametersList_WithIndex.IndexOf(paramToMoveList[0]);
			int destIndex = parametersList_WithIndex.IndexOf(droppedOnParam);

			bool isMovingUp = (sourceIndex > destIndex);

			if (!isMovingUp)
				destIndex -= paramToMoveList.Count;


			if (destIndex >= (parametersList_WithIndex.Count - paramToMoveList.Count + 1))
				return;


			parametersList.RemoveRange(sourceIndex, paramToMoveList.Count);

			parametersList_WithIndex.RemoveRange(sourceIndex, paramToMoveList.Count);

			List<DeviceParameterData> list =
				new List<DeviceParameterData>(paramToMoveList.Select((rp) => rp.Data));



			if (destIndex < 0)
			{
				parametersList.AddRange(list);
				parametersList_WithIndex.AddRange(paramToMoveList);
			}
			else
			{
				parametersList.InsertRange(destIndex, list);
				parametersList_WithIndex.InsertRange(destIndex, paramToMoveList);
			}

		}

	}
}
