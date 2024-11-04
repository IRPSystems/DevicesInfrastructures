using System;
using System.Text;

namespace DeviceCommunicators.Interfaces
{
    public interface IWT1804E
    { 
   
        int device_index{get; set;}

        bool IsInitialized { get; set; }


		void Init(string IP);
        void Dispose();
        bool Send(string data);
        string Read_data();
        int Receive(out StringBuilder temp);

	}

}
