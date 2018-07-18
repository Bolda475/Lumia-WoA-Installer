﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Installer.Core;
using Installer.Core.FullFx;
using Installer.Core.Services;
using Installer.ViewModels;
using Installer.Wpf.Core;
using Installer.Wpf.Core.Services;
using Intaller.Wpf.ViewModels;
using Intaller.Wpf.Views;
using MahApps.Metro.Controls.Dialogs;
using Serilog.Events;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common.SevenZip;
using SharpCompress.Common.Zip;

namespace Intaller.Wpf
{
    public static class CompositionRoot 
    {
        public static MainViewModel GetMainViewModel(IObservable<LogEvent> logEvents)
        {
            IDictionary<PhoneModel, IDeployer> deployerDict = new Dictionary<PhoneModel, IDeployer>
            {
                {PhoneModel.Lumia950Xl, GetDeployer(Path.Combine("Files", "Lumia 950 XL"))},
                {PhoneModel.Lumia950, GetDeployer(Path.Combine("Files", "Lumia 950"))},
            };

            var api = new LowLevelApi();
            var deployersItems = deployerDict.Select(pair => new DeployerItem(pair.Key, pair.Value)).ToList();
            var importerItems = new List<DriverPackageImporterItem>()
            {
                new DriverPackageImporterItem("7z",
                    new DriverPackageImporter<SevenZipArchiveEntry, SevenZipVolume, SevenZipArchive>(
                        s => SevenZipArchive.Open(s), "Files")),
                new DriverPackageImporterItem("zip",
                    new DriverPackageImporter<ZipArchiveEntry, ZipVolume, ZipArchive>(s => ZipArchive.Open(s), "Files"))
            };

            var viewService = new ViewService();
            viewService.Register("TextViewer", typeof(TextViewerWindow));
            var mainViewModel = new MainViewModel(logEvents, deployersItems, importerItems, new UIServices(new FilePicker(), viewService, new DialogService(DialogCoordinator.Instance)), new SettingsService(), async () => new Phone(await api.GetPhoneDisk()));
            return mainViewModel;
        }

        private static Deployer GetDeployer(string root)
        {
            return new Deployer(new CoreDeployer(root), new WindowsDeployer(new DismImageService(), new DriverPaths(root)) );
        }
    }
}