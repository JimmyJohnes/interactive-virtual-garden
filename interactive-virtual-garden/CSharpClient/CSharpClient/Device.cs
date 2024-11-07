using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpClient
{
    public class Device
    {
        public String name { get; set; }
        public String address { get; set; }
        public int score;
        public List<String> unlockables { get; set; }
        public List<String> states { get; set; }
        public List<String> seeds { get; set; }
        public List<int> phases { get; set; }
        public Device()
        {
            this.name = "";
            this.address = "";
            this.score = 0;
            this.states = new List<String>();
            this.seeds = new List<string>();
            this.unlockables = new List<String>();
        }
        public Device(String name, String address, int score, List<String> unlockables)
        {
            this.name = name;
            this.address = address;
            this.unlockables.Add(name);
            this.score = score;
        }
    }

}
