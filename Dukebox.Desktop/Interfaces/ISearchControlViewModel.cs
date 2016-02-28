using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface ISearchControlViewModel
    {
        ICommand ClearSearch { get; }
        string SearchText { get; set; }
        bool SearchEnabled { get; }
    }
}
