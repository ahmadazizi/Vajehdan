﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Vajehdan.Properties;
using Vajehdan.Views;

namespace Vajehdan
{
    public static class Helper
    {
        public static async Task CheckUpdate()
        {
            string changes = string.Empty;
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("App"));
                Release lastRelease = await client.Repository.Release.GetLatest(Settings.Default.GithubId, Settings.Default.GithubRepo);
                List<Release> allReleases = (await client.Repository.Release.GetAll(Settings.Default.GithubId, Settings.Default.GithubRepo)).ToList();
                Version latestVersion = new Version(lastRelease.TagName);
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (latestVersion > currentVersion)
                {
                    foreach (Release release in allReleases)
                    {
                        Version ver = new Version(release.TagName);
                        if (ver > currentVersion)
                        {
                            string v = release.TagName;
                            if (ver.IsBeta())
                            {
                                v += " (آزمایشی)";
                            }
                            changes += $"نسخۀ {v} -------------------------------" +
                                       Environment.NewLine + release.Body + Environment.NewLine + Environment.NewLine;
                        }
                    }

                    new ChangelogWindow(changes).Show();
                }
            }
            catch { }
        }

        public static bool IsBeta(this Version version) => version < new Version(1, 0, 0);

        public static string ToSemVersion(this Version ver)
        {
            int major = ver.Major;
            int minor = ver.Minor;
            int patch = ver.Build;

            if (major!=0 && minor!=0 && patch == 0)
                return $"واژه‌دان {major}.{minor}";

            if (major!=0 && minor==0 && patch==0)
                return $"واژه‌دان {major}";
            
            return $"{major}.{minor}.{patch}";
        }
    }
}
