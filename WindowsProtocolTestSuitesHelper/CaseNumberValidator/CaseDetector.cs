using Microsoft.Protocols.TestManager.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CaseNumberValidator
{
    /// <summary>
    /// Provides the methods of test case detection by both PTM kernel and reflection.
    /// </summary>
    public class CaseDetector
    {
        private Utility util;

        private List<TestSuiteInfo> testSuites;

        private string testSuiteName;

        /// <summary>
        /// Initializes a new instance of <see cref="CaseDetector"/>.
        /// </summary>
        /// <param name="testSuiteName">The name of a test suite installed on the local computer.</param>
        public CaseDetector(string testSuiteName)
        {
            this.testSuiteName = testSuiteName;
        }

        /// <summary>
        /// The method returns the test cases detected by PTM kernel.
        /// </summary>
        /// <returns>The list of all test cases detected by PTM kernel with all filter rules being applied.</returns>
        public List<TestCase> GetTestCasesByPtmKernel()
        {
            Init();
            LoadTestSuite(testSuiteName);

            ApplyAllRules();
            return util.GetSelectedCaseList();
        }

        /// <summary>
        /// The method returns the test cases dectection result obtained by reflection.
        /// The method must be called after the <see cref="GetTestCasesByPtmKernel"/> method was called to resolve assembly dependencies.
        /// </summary>
        /// <returns>The test cases dectection result obtained by reflection.</returns>
        public ReflectionDetectionResult GetTestCasesByReflection()
        {
            var tsInfo = testSuites.Find(ts => ts.TestSuiteName == testSuiteName);

            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var installDir = Path.GetFullPath(Path.Combine(exePath, ".."));

            var testSuiteDir = tsInfo.TestSuiteFolder + "\\";
            var appConfig = AppConfig.LoadConfig(
                    tsInfo.TestSuiteName,
                    tsInfo.TestSuiteVersion,
                    testSuiteDir,
                    installDir);

            return LoadTestCasesByReflection(appConfig.TestSuiteAssembly);
        }

        private void Init()
        {
            util = new Utility();
            testSuites = util.TestSuiteIntroduction.SelectMany(tsFamily => tsFamily).ToList();
        }

        private void LoadTestSuite(string testSuiteName)
        {
            var tsInfo = testSuites.Find(ts => ts.TestSuiteName == testSuiteName);
            util.LoadTestSuiteConfig(tsInfo);

            try
            {
                util.LoadTestSuiteAssembly();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ApplyAllRules()
        {
            foreach (var group in util.GetFilter())
            {
                group.SelectStatus = RuleSelectStatus.Selected;
            }
        }

        private ReflectionDetectionResult LoadTestCasesByReflection(List<string> assemblyNames)
        {
            var testCasesGroupByCategory = new Dictionary<string, List<string>>();
            var ignoredTestCases = new List<string>();
            var disabledTestCases = new List<string>();

            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                Type[] types = null;

                try
                {
                    assembly = Assembly.LoadFrom(assemblyName);
                    types = assembly.GetTypes();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                foreach (Type type in types)
                {
                    // Search for class, interfaces and other type
                    if (type.IsClass)
                    {
                        MethodInfo[] methods = type.GetMethods();
                        foreach (MethodInfo method in methods)
                        {
                            // Search for methods with TestMethodAttribute
                            object[] attributes = method.GetCustomAttributes(false);
                            var categories = new List<string>();

                            bool isTestMethod = false;
                            bool isIgnored = false;
                            bool isDisabled = false;

                            foreach (object attribute in attributes)
                            {
                                string name = attribute.GetType().Name;
                                if (name == "IgnoreAttribute")
                                {
                                    isIgnored = true;
                                }

                                // It's possible to have "IgnoreAttribute" after "TestMethodAttribute"
                                if (name == "TestMethodAttribute")
                                {
                                    isTestMethod = true;
                                }

                                if (name == "TestCategoryAttribute")
                                {
                                    PropertyInfo property = attribute.GetType().GetProperty("TestCategories");
                                    object category = property.GetValue(attribute, null);
                                    foreach (string str in (System.Collections.ObjectModel.ReadOnlyCollection<string>)category)
                                    {
                                        if (str == "Disabled")
                                        {
                                            isDisabled = true;
                                        }

                                        categories.Add(str);
                                    }
                                }
                            }

                            var caseFullName = method.DeclaringType.FullName + "." + method.Name;
                            if (isTestMethod && !isIgnored && !isDisabled)
                            {
                                foreach (var category in categories)
                                {
                                    AddTestCaseToDictionary(testCasesGroupByCategory, category, caseFullName);
                                }

                                if (!categories.Any())
                                {
                                    AddTestCaseToDictionary(testCasesGroupByCategory, SharedVariables.NoTraitsKey, caseFullName);
                                }
                            }

                            if (isTestMethod && isIgnored)
                            {
                                ignoredTestCases.Add(caseFullName);
                            }

                            if (isTestMethod && isDisabled)
                            {
                                disabledTestCases.Add(caseFullName);
                            }
                        }
                    }
                }
            }

            return new ReflectionDetectionResult(testCasesGroupByCategory, ignoredTestCases, disabledTestCases);
        }

        private static void AddTestCaseToDictionary(Dictionary<string, List<string>> testCasesGroupByCategory, string category, string caseFullName)
        {
            if (!testCasesGroupByCategory.ContainsKey(category))
            {
                testCasesGroupByCategory.Add(category, new List<string>());
            }

            testCasesGroupByCategory[category].Add(caseFullName);
        }
    }
}
