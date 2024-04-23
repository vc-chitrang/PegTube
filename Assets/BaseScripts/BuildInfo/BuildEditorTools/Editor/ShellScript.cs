#if UNITY_EDITOR
using System.Diagnostics;
using System.Text;
using static Modules.Utility.Utility;
using UnityEngine;
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
    public abstract class ShellScript {
        private static void SplitLog(LogType type, string prefix, string postfix, string fullLog
#if UNITY_EDITOR
            , int lineCap = 1) {
#else
			, int lineCap = 1024) {
#endif

            if (string.IsNullOrWhiteSpace(fullLog)) {
                return;
            }

            string[] lines = fullLog.Split(new[] { '\n' });
            if (lines.Length < lineCap) {
                UnityEngine.Debug.unityLogger.Log(type, $"{prefix}{fullLog}{postfix}");
                return;
            }

            StringBuilder s = new StringBuilder();
            s.Append(prefix);
            for (int i = 0; i < lines.Length / lineCap; ++i) {
                for (int j = 0; j < lineCap; j++) {
                    string line = lines[i * lineCap + j];
                    if (!string.IsNullOrWhiteSpace(line)) {
                        s.AppendLine(line);
                    }
                }

                string combinedOut = s.ToString();
                if (!string.IsNullOrWhiteSpace(combinedOut)) {
                    UnityEngine.Debug.unityLogger.Log(type, combinedOut);
                }

                s = new StringBuilder();
            }

            UnityEngine.Debug.unityLogger.Log(type, postfix);
        }

        public static bool Execute(string path, string[] args, bool verbose) {
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = path //"/bin/sh";
            };
            StringBuilder s = new StringBuilder();
            //s.Append(path);
            if (args != null) {
                foreach (string arg in args) {
                    s.Append(" ");
                    s.Append(arg);
                }
            }

            psi.CreateNoWindow = true;
            psi.Arguments = s.ToString();
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.ErrorDialog = true;

            Log(string.Format("[ShellScript] Attempting to execute: \ncmd({0})\nargs({1})\n", path, psi.Arguments));

            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode == 0) {
                if (!verbose)
                    return true;
                if (!string.IsNullOrWhiteSpace(error)) {
                    Log("[ShellScript] Success on Run({path}) - but had some errors (below)");
                    SplitLog(LogType.Log, $"vvv StdErr vvv: {path}\n", "\n^^^^ StdErr ^^^^", error);
                }

                Log($"[ShellScript] Success on Run({path}) - script output below");
                SplitLog(LogType.Log, $"vvv StdOut vvv: {path}\n", "\n^^^^StdOut^^^^", output);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(error)) {
                LogError(string.Format("[ShellScript] Failure on Run({path}, exit={p.ExitCode}) - had some errors (below)"));
                SplitLog(LogType.Error, $"vvv StdErr vvv: {path}\n", "\n^^^^StdErr^^^^", error);
            }

            LogError(string.Format("[ShellScript] Failure on Run({path}, exit={p.ExitCode}) - script output below (script stderr above)"));
            SplitLog(LogType.Error, $"vvv StdOut vvv: {path}\n", "\n^^^^StdOut^^^^", output);
            return false;
        }
    }
}

#endif