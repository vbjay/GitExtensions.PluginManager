﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Neptuo;
using PackageManager.Models;

namespace PackageManager.Services
{
    internal partial class SelfUpdateService : ISelfUpdateService
    {
        private readonly IApplication application;
        private readonly ProcessService processes;

        public SelfUpdateService(IApplication application, ProcessService processes)
        {
            Ensure.NotNull(application, "application");
            Ensure.NotNull(processes, "processes");
            this.application = application;
            this.processes = processes;
        }

        public string CurrentFileName => Path.GetFileName(Assembly.GetExecutingAssembly().Location);

        public bool IsSelfUpdate => application.Args.IsSelfUpdate;

        public void Update(IPackage latest)
        {
            // Copy to temp.
            string current = Assembly.GetExecutingAssembly().Location;
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", string.Empty));
            string temp = Path.Combine(tempDirectory, CurrentFileName);

            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            File.Copy(current, temp, true);

            // Rerun with self update.
            application.Args.IsSelfUpdate = true;
            application.Args.SelfOriginalPath = current;
            string arguments = application.Args.ToString();

            processes.Run(temp, arguments);
            application.Shutdown();
        }

        public void RunNewInstance(IPackage package)
        {
            string target = null;
            if (application.Args.SelfOriginalPath != null)
            {
                target = application.Args.SelfOriginalPath;
            }
            else
            {
                target = Directory
                    .EnumerateFiles(application.Args.Path, CurrentFileName, SearchOption.AllDirectories)
                    .FirstOrDefault();
            }

            if (target != null)
            {
                application.Args.IsSelfUpdate = false;
                string arguments = application.Args.ToString();

                processes.Run(target, arguments);
            }
        }
    }
}
