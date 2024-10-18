using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoF_AbstractBuilderFactory
{
    public class ExcelReport : IReport
    {
        public void GenerateReport()
        {
            Console.WriteLine("generating Excel Report...");
        }
    }
}
