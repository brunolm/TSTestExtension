using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TSTestAdapter.Helpers
{
    public class JSRunner
    {
        public static string ExecutorPath;

        static JSRunner()
        {
            var executingPath = Assembly.GetExecutingAssembly().Location;
            var executingDir = Path.GetDirectoryName(executingPath);

            ExecutorPath = Path.GetFullPath(Path.Combine(executingDir, "phantomjs.exe"));
        }

        public static TestResult Run(string filePath)
        {
            var result = new TestResult();

            try
            {
                Process p = new Process();
                ProcessStartInfo psi = new ProcessStartInfo(ExecutorPath, filePath);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;

                p.StartInfo = psi;
                p.Start();

                var output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                result.ExitCode = p.ExitCode;
                result.Output = output;
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.Output = ex.StackTrace;
            }

            return result;
        }

        public class TestResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }

            public TestOutcome Outcome
            {
                get
                {
                    return ExitCode == 0 ? TestOutcome.Passed : TestOutcome.Failed;
                }
            }

            public TestResult()
            {
                ExitCode = -1000;
            }
        }

    }
}
