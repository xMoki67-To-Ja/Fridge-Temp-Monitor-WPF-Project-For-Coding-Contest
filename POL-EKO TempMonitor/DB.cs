using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace POL_EKO_TempMonitor
{
    internal class DB
    {
        public string temp { get; set; }
        public string mTime { get; set; }
        public string dTime { get; set; }
        public string isWorking { get; set; }

        public DB() { }

        public DB(string temp, string mTime, string dTime, string isWorking)
        {
            this.temp = temp;
            this.mTime = mTime;
            this.dTime = dTime;
            this.isWorking = isWorking;
        }
    }
}
