using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWorkload
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                return;
            }
            long maxItem;
            try
            {
                maxItem = long.Parse(args[0]) * 1024 * 1024 / 8;

                var list = new List<Int64>();
                for(int i = 0; i < maxItem; i++)
                {
                    list.Add(1L);
                }
            }
            catch(Exception)
            {
                Environment.Exit(-1);
            }
        }
    }
}
