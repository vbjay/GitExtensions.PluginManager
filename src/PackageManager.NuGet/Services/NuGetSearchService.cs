﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Neptuo;
using Neptuo.Activators;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using PackageManager.Models;

namespace PackageManager.Services
{
    public partial class NuGetSearchService : ISearchService
    {
        private readonly IFactory<SourceRepository, string> repositoryFactory;
        private readonly IFilter filter;

        public NuGetSearchService(IFactory<SourceRepository, string> repositoryFactory, IFilter filter = null)
        {
            Ensure.NotNull(repositoryFactory, "repositoryFactory");

            if (filter == null)
                filter = new NullFilter();

            this.repositoryFactory = repositoryFactory;
            this.filter = filter;
        }

        private SearchOptions EnsureOptions(SearchOptions options)
        {
            if (options == null)
            {
                options = new SearchOptions()
                {
                    PageIndex = 0,
                    PageSize = 10
                };
            }

            return options;
        }

        private Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(PackageSearchResource search, string searchText, SearchOptions options, CancellationToken cancellationToken)
            => search.SearchAsync(searchText, new SearchFilter(false), options.PageIndex * options.PageSize, options.PageSize, NullLogger.Instance, cancellationToken);

        public async Task<IEnumerable<IPackage>> SearchAsync(string packageSourceUrl, string searchText, SearchOptions options = default, CancellationToken cancellationToken = default)
        {
            options = EnsureOptions(options);

            SourceRepository repository = repositoryFactory.Create(packageSourceUrl);
            PackageSearchResource search = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
            if (search == null)
                return Enumerable.Empty<IPackage>();

            List<IPackage> result = new List<IPackage>();
            
            // Try to find N results passing filter (until zero results is returned).
            int i = 0;
            while (i < options.PageSize)
            {
                bool hasItems = false;
                foreach (IPackageSearchMetadata package in await SearchAsync(search, searchText, options, cancellationToken))
                {
                    hasItems = true;
                    if (i >= options.PageSize)
                        break;

                    if (filter.IsPassed(package))
                        result.Add(new NuGetPackage(package, repository));
                }

                if (!hasItems)
                    break;

                options = new SearchOptions()
                {
                    PageIndex = options.PageIndex + 1,
                    PageSize = options.PageSize
                };
            }

            return result;
        }

        public async Task<IPackage> FindLatestVersionAsync(string packageSourceUrl, IPackage package, CancellationToken cancellationToken = default)
        {
            Ensure.NotNull(package, "package");

            IEnumerable<IPackage> packages = await SearchAsync(packageSourceUrl, package.Id, new SearchOptions() { PageSize = 1 }, cancellationToken);
            IPackage latest = packages.FirstOrDefault();
            if (latest != null && latest.Id == package.Id)
                return latest;

            return null;
        }
    }
}
