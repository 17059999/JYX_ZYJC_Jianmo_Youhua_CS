using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseNumberValidator
{
    /// <summary>
    /// The result obtained by detecting test cases by reflection.
    /// </summary>
    public class ReflectionDetectionResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ReflectionDetectionResult"/>.
        /// </summary>
        /// <param name="testCasesGroupByCategory">The dictionary which contains mappings of test categories and test cases.</param>
        /// <param name="ignoredTestCases">The test cases which are marked with an IgnoreAttribute.</param>
        /// <param name="disabledTestCases">The test cases which are in the Disabled test category.</param>
        public ReflectionDetectionResult(Dictionary<string, List<string>> testCasesGroupByCategory, List<string> ignoredTestCases, List<string> disabledTestCases)
        {
            TestCasesGroupByCategory = testCasesGroupByCategory;
            IgnoredTestCases = ignoredTestCases;
            DisabledTestCases = disabledTestCases;
        }

        /// <summary>
        /// The dictionary which contains mappings of test categories and test cases.
        /// </summary>
        public Dictionary<string, List<string>> TestCasesGroupByCategory { get; }

        /// <summary>
        /// The test cases which are marked with an IgnoreAttribute.
        /// </summary>
        public List<string> IgnoredTestCases { get; }

        /// <summary>
        /// The test cases which are in the Disabled test category.
        /// </summary>
        public List<string> DisabledTestCases { get; }
    }
}
