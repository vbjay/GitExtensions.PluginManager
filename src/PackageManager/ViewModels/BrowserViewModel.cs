﻿using Neptuo.Observables;
using Neptuo.Observables.Collections;
using PackageManager.Models;
using PackageManager.Services;
using PackageManager.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PackageManager.ViewModels
{
    public class BrowserViewModel : ObservableModel
    {
        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PagingViewModel Paging { get; }
        public ObservableCollection<IPackage> Packages { get; }
        public SearchCommand Search { get; }
        public InstallCommand Install { get; }
        public ReinstallCommand Reinstall { get; }
        public UninstallCommand Uninstall { get; }

        public BrowserViewModel(IPackageSourceSelector packageSource, ISearchService search, IInstallService install, SelfPackageConfiguration selfPackageConfiguration)
        {
            Packages = new ObservableCollection<IPackage>();
            Search = new SearchCommand(this, packageSource, search);
            Paging = new PagingViewModel(Search);
            Install = new InstallCommand(install);
            Reinstall = new ReinstallCommand(install, selfPackageConfiguration);
            Uninstall = new UninstallCommand(install, selfPackageConfiguration);
        }
    }
}
