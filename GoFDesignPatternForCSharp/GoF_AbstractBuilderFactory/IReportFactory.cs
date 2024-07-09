using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoF_AbstractBuilderFactory
{
    public interface IReportFactory
    {
        public IReport CreateReport();
    }
}
