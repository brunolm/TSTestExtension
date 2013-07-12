using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSTestAdapter.Helpers
{
    public static class TypeScriptCompiler
    {
        public class Options
        {
            private static Options @default;
            public static Options Default
            {
                get
                {
                    if (@default == null)
                        @default = new Options();

                    return @default;
                }
            }

            public enum Version
            {
                ES5,
                ES3,
            }

            public bool EmitComments { get; set; }
            public bool GenerateDeclaration { get; set; }
            public bool GenerateSourceMaps { get; set; }
            public string OutPath { get; set; }
            public Version TargetVersion { get; set; }

            public Options(bool emitComments = false
                , bool generateDeclaration = false
                , bool generateSourceMaps = false
                , string outPath = null
                , Version targetVersion = Version.ES5)
            {
                EmitComments = emitComments;
                GenerateDeclaration = generateDeclaration;
                GenerateSourceMaps = generateSourceMaps;
                OutPath = outPath;
                TargetVersion = targetVersion;
            }
        }

        public static void Compile(string tsPath, Options options = null)
        {
            if (options == null)
                options = Options.Default;

            var d = new Dictionary<string, string>();

            if (options.EmitComments)
                d.Add("-c", null);

            if (options.GenerateDeclaration)
                d.Add("-d", null);

            if (options.GenerateSourceMaps)
                d.Add("--sourcemap", null);

            if (!String.IsNullOrEmpty(options.OutPath))
                d.Add("--out", options.OutPath);

            d.Add("--target", options.TargetVersion.ToString());

            Process p = new Process();

            ProcessStartInfo psi = new ProcessStartInfo("tsc", tsPath + " " + String.Join(" ", d.Select(o => o.Key + " " + o.Value)));
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            p.StartInfo = psi;

            p.Start();

            var msg = p.StandardError.ReadToEnd();

            p.WaitForExit();

            if (!String.IsNullOrEmpty(msg))
                throw new InvalidTypeScriptFileException(msg);
        }
    }

    public class InvalidTypeScriptFileException : Exception
    {
        public InvalidTypeScriptFileException(string message)
            : base(message)
        {

        }
    }
}
