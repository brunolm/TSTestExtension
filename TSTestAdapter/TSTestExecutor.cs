using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TSTestAdapter.Helpers;

namespace TSTestAdapter
{
    [ExtensionUri(TSTestExecutor.ExecutorUriString)]
    public class TSTestExecutor : ITestExecutor
    {
        private bool canceled = false;

        public const string ExecutorUriString = "executor://typescriptexecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(TSTestExecutor.ExecutorUriString);

        public void Cancel()
        {
            canceled = true;
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var tests = TSTestDiscoverer.GetTests(sources, null);
            RunTests(tests, runContext, frameworkHandle);
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var olock = new Object();
            var cache = new Dictionary<string, string>();

            Parallel.ForEach(tests, test =>
            {
                var result = new TestResult(test);

                // full path to temporary file
                string filePath = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + DateTime.Now.ToString("HH-mm-ss-fff")), ".js");

                try
                {
                    lock (olock)
                    {
                        if (!cache.ContainsKey(test.CodeFilePath))
                        {
                            TypeScriptCompiler.Compile(test.CodeFilePath, new TypeScriptCompiler.Options(outPath: filePath));
                            cache.Add(test.CodeFilePath, filePath);
                        }
                        else
                        {
                            filePath = cache[test.CodeFilePath];
                        }
                    }

                    var testResult = new JSRunner.TestResult();

                    var scriptFilePath = filePath + Guid.NewGuid().ToString("N") + "exec.js";

                    using (var fs = new FileStream(scriptFilePath, FileMode.Create))
                    {
                        using (var sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("try{");
                            sw.Write(File.ReadAllText(filePath));

                            var className = test.FullyQualifiedName.Substring(0, test.FullyQualifiedName.LastIndexOf("."));
                            var methodName = test.FullyQualifiedName.Substring(test.FullyQualifiedName.LastIndexOf(".") + 1);

                            sw.WriteLine("var ____TSTestExecutor____ = new " + className + "();____TSTestExecutor____." + methodName + "();");

                            sw.WriteLine("phantom.exit(0)}catch(ex){console.log(ex);phantom.exit(-1)}");
                            sw.Flush();
                        }
                    }
                    testResult = JSRunner.Run(scriptFilePath);

                    result.Outcome = testResult.Outcome;
                    if (result.Outcome != TestOutcome.Passed)
                    {
                        result.ErrorMessage = testResult.Output;
                    }

                    try
                    {
                        File.Delete(scriptFilePath);
                    }
                    catch { }
                }
                catch (InvalidTypeScriptFileException ex)
                {
                    result.Outcome = TestOutcome.Failed;
                    result.ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    result.Outcome = TestOutcome.Failed;
                    result.ErrorMessage = ex.Message + ex.StackTrace;
                }

                frameworkHandle.RecordResult(result);
            });

            foreach (KeyValuePair<string, string> item in cache)
            {
                try
                {
                    File.Delete(item.Value);
                }
                catch { }
            }
        }
    }
}
