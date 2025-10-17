using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseNumberValidator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(parameters => ValidateCaseNumber(parameters));
        }

        private static void ValidateCaseNumber(Parameters parameters)
        {
            var caseValidator = new CaseValidator(parameters.TestSuiteName);
            var report = caseValidator.GenerateValidationReport();

            Console.WriteLine(report);
            File.WriteAllText(parameters.ReportOutputPath, report);
        }
    }
}
