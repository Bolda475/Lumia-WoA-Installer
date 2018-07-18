﻿using System;
using System.Threading.Tasks;
using Installer.UI;
using ReactiveUI;
using Serilog;

namespace Installer.ViewModels.Core
{
    public class CommandWrapper<T1, T2> : ReactiveObject
    {
        private readonly object parent;
        private readonly IDialogService dialogService;
        private readonly ObservableAsPropertyHelper<bool> isExecutingHelper;
        public ReactiveCommand<T1, T2> Command { get; }

        public CommandWrapper(object parent, ReactiveCommand<T1, T2> command, IDialogService dialogService)
        {
            this.parent = parent;
            this.dialogService = dialogService;
            Command = command;
            command.ThrownExceptions.Subscribe(async e => await HandleException(e));
            isExecutingHelper = command.IsExecuting.ToProperty(this, x => x.IsExecuting);
        }

        private async Task HandleException(Exception e)
        {
            Log.Error(e, "An error has ocurred");
            Log.Information($"Error: {e.Message}");
            await dialogService.ShowAlert(parent, "Error", $"{e.Message}");   
        }

        public bool IsExecuting => isExecutingHelper.Value;
    }
}