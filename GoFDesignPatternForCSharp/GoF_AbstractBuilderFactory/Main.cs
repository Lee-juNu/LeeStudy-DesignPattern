// generate basic Main Class
using System;

namespace GoF_AbstractBuilderFactory
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            IReportFactory pdfFactory = new PDFReportFactory();
            IReport pdfReport = pdfFactory.CreateReport();
            pdfReport.GenerateReport();
        
            IReportFactory excelFactory = new ExcelReportFactory();
            IReport excelReport = excelFactory.CreateReport();
            excelReport.GenerateReport();
        }
    }
}