using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseNumberValidator
{
    /// <summary>
    /// Provides the case validation report.
    /// </summary>
    public class CaseValidator
    {
        private CaseDetector caseDetector;

        /// <summary>
        /// Initializes a new instance of <see cref="CaseValidator"/>.
        /// </summary>
        /// <param name="testSuiteName">The name of a test suite installed on the local computer.</param>
        public CaseValidator(string testSuiteName)
        {
            caseDetector = new CaseDetector(testSuiteName);
        }

        /// <summary>
        /// The method generates the case number validation report for a certain test suite in plain text format.
        /// </summary>
        /// <returns>The validation report in plain text fromat.</returns>
        public string GenerateValidationReport()
        {
            var testCasesByPtmKernel = caseDetector.GetTestCasesByPtmKernel();
            var testCaseFullNamesByPtmKernel = testCasesByPtmKernel.Select(tc => tc.FullName).ToList();
            var detectionResultByReflection = caseDetector.GetTestCasesByReflection();

            var testCasesByReflection = detectionResultByReflection.TestCasesGroupByCategory
                .Values
                .SelectMany(t => t)
                .Distinct()
                .ToList();

            var reportSb = new StringBuilder();

            var countByPtmKernel = testCasesByPtmKernel.Count;
            var countByReflection = testCasesByReflection.Count;
            reportSb.AppendLine($"Total count of test cases in PTM: {countByPtmKernel}");
            reportSb.AppendLine($"Total count of test cases detected by reflection: {countByReflection}");

            var testCasesNotInPtm = testCasesByReflection.Where(t => !testCaseFullNamesByPtmKernel.Contains(t)).ToList();
            if (testCasesNotInPtm.Any())
            {
                reportSb.AppendLine();
                reportSb.AppendLine($"Test cases excluded from PTM: {testCasesNotInPtm.Count}");
                foreach (var t in testCasesNotInPtm)
                {
                    reportSb.AppendLine(t);
                }
            }

            if (detectionResultByReflection.TestCasesGroupByCategory.ContainsKey(SharedVariables.NoTraitsKey) &&
                detectionResultByReflection.TestCasesGroupByCategory[SharedVariables.NoTraitsKey].Any())
            {
                reportSb.AppendLine();
                reportSb.AppendLine($"Test cases without categories: {detectionResultByReflection.TestCasesGroupByCategory[SharedVariables.NoTraitsKey].Count}");
                foreach (var t in detectionResultByReflection.TestCasesGroupByCategory[SharedVariables.NoTraitsKey])
                {
                    reportSb.AppendLine(t);
                }
            }

            if (detectionResultByReflection.IgnoredTestCases.Any())
            {
                reportSb.AppendLine();
                reportSb.AppendLine($"Ignored test cases: {detectionResultByReflection.IgnoredTestCases.Count}");
                foreach (var t in detectionResultByReflection.IgnoredTestCases)
                {
                    reportSb.AppendLine(t);
                }
            }

            if (detectionResultByReflection.DisabledTestCases.Any())
            {
                reportSb.AppendLine();
                reportSb.AppendLine($"Disabled test cases: {detectionResultByReflection.DisabledTestCases.Count}");
                foreach (var t in detectionResultByReflection.DisabledTestCases)
                {
                    reportSb.AppendLine(t);
                }
            }

            return reportSb.ToString();
        }
    }
}
