/***************************************************************************/
// Copyright 2013-2016 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Diagnostics.Contracts;

internal static class AssemblyVerifier
{
    public static void Verify(string unprocessedAssemblyPath, string processedAssemblyPath)
    {
        Contract.Requires(unprocessedAssemblyPath != null);
        Contract.Requires(File.Exists(unprocessedAssemblyPath));
        Contract.Requires(processedAssemblyPath != null);
        Contract.Requires(File.Exists(processedAssemblyPath));

        bool isVerified;
        string output;

        RunVerifyProcessAndCollectOutput(unprocessedAssemblyPath, out isVerified, out output);
        Assert.That(isVerified, string.Format("Unprocessed assembly could not be verified: \n{0}", output));

        RunVerifyProcessAndCollectOutput(processedAssemblyPath, out isVerified, out output);
        Assert.That(isVerified, string.Format("Processed assembly could not be verified: \n{0}", output));
    }

    public static void RunVerifyProcessAndCollectOutput(string assemblyPath, out bool isVerified, out string output)
    {
        Contract.Requires(assemblyPath != null);
        Contract.Requires(File.Exists(assemblyPath));

        var process = Process.Start(
            new ProcessStartInfo(PEVerifyPath, string.Format("\"{0}\"", assemblyPath))
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

        if(!process.WaitForExit(10000))
        {
            throw new InvalidOperationException("PEVerify.exe did not end after 10 seconds");
        }

        isVerified = process.ExitCode == 0;
        output = process.StandardOutput.ReadToEnd().Trim();
    }

    private static string peVerifyPath;
    private static string PEVerifyPath
    {
        get
        {
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(File.Exists(Contract.Result<string>()));

            if (peVerifyPath == null)
            {
                var possiblePEVerifyPaths =
                    new string[]
                    {
                        @"%programfiles(x86)%\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\PEVerify.exe",
                        @"%programfiles(x86)%\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\PEVerify.exe",
                        @"%programfiles(x86)%\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\PEVerify.exe",
                        @"%programfiles(x86)%\Microsoft SDKs\Windows\v7.0A\bin\PEVerify.exe"
                    };

                foreach (var possiblePEVerifyPath in possiblePEVerifyPaths)
                {
                    var path = Environment.ExpandEnvironmentVariables(possiblePEVerifyPath);
                    if(File.Exists(path))
                    {
                        peVerifyPath = path;
                        break;
                    }
                }

                if (peVerifyPath == null)
                {
                    throw new InvalidOperationException("Cannot find PEVerify.exe");
                }
            }
            return peVerifyPath;
        }
    }
}
