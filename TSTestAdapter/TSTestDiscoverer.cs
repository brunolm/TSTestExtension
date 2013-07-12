using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TSTestAdapter
{
    [DefaultExecutorUri(TSTestExecutor.ExecutorUriString)]
    [FileExtension(".ts")]
    //[FileExtension(".test.ts")]
    public class TSTestDiscoverer : ITestDiscoverer
    {
        public static Regex TestFinderRegex = new Regex(@"TestBase.RegisterTestMethod\s*\("
                + @"\s*(?<Method>[^\s]*?)\s*(,|\))"
                + @"(\s*""(?<Name>.*?)""\s*(,|\))"
                + @"(\s*""(?<Description>.*?)""\s*\))?)?", RegexOptions.Compiled | RegexOptions.Multiline);

        public void DiscoverTests(IEnumerable<string> sources
            , IDiscoveryContext discoveryContext
            , Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.IMessageLogger logger
            , ITestCaseDiscoverySink discoverySink)
        {
            GetTests(sources, discoverySink);
        }

        public static IEnumerable<TestCase> GetTests(IEnumerable<string> sourceFiles, ITestCaseDiscoverySink discoverySink)
        {
            var tests = new List<TestCase>();

            Parallel.ForEach(sourceFiles, s =>
            {
                var sourceCode = File.ReadAllText(s);

                var matches = TestFinderRegex.Matches(sourceCode);
                foreach (Match m in matches)
                {
                    var methodParts = m.Groups["Method"].Value.Split(new string[] { ".prototype." }, System.StringSplitOptions.None);
                    var testClass = methodParts[0];
                    var testMethod = methodParts[1];
                    var testName = m.Groups["Name"].Value;
                    var testDescription = m.Groups["Description"].Value;
                    var testCase = new TestCase(String.Join(".", methodParts), TSTestExecutor.ExecutorUri, s)
                    {
                        CodeFilePath = s,
                        DisplayName = testName,
                    };

                    if (discoverySink != null)
                    {
                        discoverySink.SendTestCase(testCase);
                    }
                    tests.Add(testCase);
                }
            });

            return tests;
        }
    }
}
