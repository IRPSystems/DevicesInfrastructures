using System;

namespace DeviceCommunicators.Interfaces
{
    public interface IWT1804E
    { 
   
        int device_index{get; set;}

        bool IsInitialized { get; set; }


		void Init(string IP);
        void Dispose();
        bool send(string data);
        string Read_data();
    }

}
