// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Runtime.CompilerServices;

using Scrutinize.Models;

namespace Scrutinize.Controllers
{
    public class WorkspaceController : INotifyPropertyChanged
    {
        Workspace? _workspace;
        public Workspace Workspace
        {
            get => _workspace ?? Workspace.Empty;
            set
            {
                if (_workspace is object &&
                    value is object &&
                    _workspace.Equals(value))
                    return;

                _workspace = value;

                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        void NotifyPropertyChanged(
            [CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
    }
}
