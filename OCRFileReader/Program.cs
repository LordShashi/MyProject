using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRFileReader
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            OCR ocr = new OCR();
          string  path = ConfigurationManager.AppSettings["MulkiyaFront_File_Path"];

            OCRFileModel result = await ocr.OCRMulkiyaFrontDetails(path, new OCRFileModel());

            Console.WriteLine("Mulkiya Name = "+result._MName);
            Console.WriteLine("Plate No = "+result._MPlateno);
            Console.WriteLine("Mulkiya Reg. Date = "+result._MRegdate);
            Console.WriteLine("Mulkiya Ins Exp Date = "+result._MInsexp);
            Console.WriteLine("Mulkiya Exp Date = " + result._MExpdate);

            Console.ReadLine();
        }
    }
}

