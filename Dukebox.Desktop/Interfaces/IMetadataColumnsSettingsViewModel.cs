using System.Collections.Generic;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop.Interfaces
{
    public interface IMetadataColumnsSettingsViewModel
    {
        List<ExtendedMetadataColumnSetting> MetadataColumns { get; }
    }
}
