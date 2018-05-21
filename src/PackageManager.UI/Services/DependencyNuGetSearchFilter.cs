﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neptuo;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace PackageManager.Services
{
    public class DependencyNuGetSearchFilter : NuGetSearchService.IFilter
    {
        private readonly (string id, string version)[] dependencies;

        public DependencyNuGetSearchFilter((string id, string version)[] dependencies)
        {
            Ensure.NotNull(dependencies, "dependencies");
            this.dependencies = dependencies;
        }

        public bool IsPassed(IPackageSearchMetadata package)
        {
            if (!dependencies.Any())
                return true;

            foreach (var group in package.DependencySets)
            {
                if (group.TargetFramework == NuGetFramework.AnyFramework)
                {
                    foreach (var dependency in dependencies)
                    {
                        PackageDependency packageDependency = group.Packages.FirstOrDefault(p => p.Id == dependency.id);
                        if (packageDependency == null)
                            return false;

                        if (dependency.version != null && !packageDependency.VersionRange.Satisfies(new NuGetVersion(dependency.version)))
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
