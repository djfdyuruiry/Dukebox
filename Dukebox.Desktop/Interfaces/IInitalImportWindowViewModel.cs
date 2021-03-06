﻿using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IInitalImportWindowViewModel
    {
        string ImportPath { get; }
        string NotificationText { get; }
        string StatusText { get; }
        double MaximumProgressValue { get; }
        double CurrentProgressValue { get; }
        bool ImportHasNotStarted { get; }
        ICommand SelectImportPath { get; }
        ICommand Import { get; }
        ICommand SkipImport { get; }
    }
}
