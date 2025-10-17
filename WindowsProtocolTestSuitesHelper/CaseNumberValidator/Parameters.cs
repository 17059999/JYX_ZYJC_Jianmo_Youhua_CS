using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseNumberValidator
{
    /// <summary>
    /// The paramters for case validation process.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// The name of a test suite installed on the local computer.
        /// </summary>
        [Option("TestSuiteName", Required = true, HelpText = "The name of a test suite installed on the local computer. Please refer to TestSuiteIntro.xml in the binary root to find the correct name of a test suite.")]
        public string TestSuiteName { get; set; }

        /// <summary>
        /// The path to save the generated case number validation report.
        /// </summary>
        [Option("ReportOutputPath", Required = true, HelpText = "The path to save the generated case number validation report.")]
        public string ReportOutputPath { get; set; }
    }
}
