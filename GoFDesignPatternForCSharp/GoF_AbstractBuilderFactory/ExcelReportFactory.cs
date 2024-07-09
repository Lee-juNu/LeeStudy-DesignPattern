using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoF_AbstractBuilderFactory
{
    public class ExcelReportFactory : IReportFactory
    {
        public IReport CreateReport() => new ExcelReport();
    }
}
