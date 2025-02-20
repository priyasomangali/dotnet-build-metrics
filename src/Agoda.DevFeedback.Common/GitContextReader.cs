﻿using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Agoda.DevFeedback.Common
{
    public static class GitContextReader
    {
        public static GitContext GetGitContext()
        {
            string url = RunCommand("config --get remote.origin.url");
            string branch = RunCommand("rev-parse --abbrev-ref HEAD");

            if (string.IsNullOrEmpty(url))
            {
                throw new GitContextException("Unable to get git remote url.");
            }

            if (string.IsNullOrEmpty(branch))
            {
                throw new GitContextException("Unable to get git branch.");
            }

            url = CleanGitlabCIToken(url);
            return new GitContext
            {
                RepositoryUrl = url,
                RepositoryName = GetRepositoryNameFromUrl(url),
                BranchName = branch
            };
        }

        static string RunCommand(string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                (
                    fileName: Environment.OSVersion.Platform == PlatformID.Win32NT ? "git.exe" : "git"
                )
                {
                    UseShellExecute = false,
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = args
                }
            };

            try
            {
                process.Start();
            }
            catch(Win32Exception ex)
            {
                throw new GitContextException("Failed to run git command.", ex);
            }

            return process.StandardOutput.ReadLine();
        }

        internal static string GetRepositoryNameFromUrl(string url)
        {
            var repositoryName = url.Substring(url.LastIndexOf('/') + 1);

            return repositoryName.EndsWith(".git")
                ? repositoryName.Substring(0, repositoryName.LastIndexOf('.'))
                : repositoryName;
        }

        internal static string CleanGitlabCIToken(string url)
        {
            if (url.Contains("@") && url.StartsWith("https"))
            {
                url = "https://" + url.Split('@')[1];
            }
            return url;
        }
    }
}
