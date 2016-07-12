using System.Collections.Generic;
using System.Linq;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Factories;
using Dukebox.Library.Helper;

namespace Dukebox.Desktop.ViewModel
{
    public class MetadataColumnsSettingsViewModel : ViewModelBase, IMetadataColumnsSettingsViewModel
    {
        private readonly IDukeboxUserSettings _userSettings;

        public List<ExtendedMetadataColumnSetting> MetadataColumns { get; private set; }

        public MetadataColumnsSettingsViewModel(IDukeboxUserSettings userSettings, AudioFileMetadataFactory audioFileMetadataFactory)
        {
            _userSettings = userSettings;
            
            PopulateMetadataColumns();

            MetadataColumns.ForEach(s => s.PropertyChanged += 
                (o, e) => UpdateMetadatColumnSetting(o as ExtendedMetadataColumnSetting)); ;
        }

        private void UpdateMetadatColumnSetting(ExtendedMetadataColumnSetting extendedMetadataColumnSetting)
        {
            var columns = _userSettings.ExtendedMetadataColumnsToShow;
            var columnName = extendedMetadataColumnSetting.ColumnName;

            if (extendedMetadataColumnSetting.IsEnabled)
            {
                if (!columns.Contains(columnName))
                {
                    columns.Add(columnName);
                }
            }
            else
            {
                columns.Remove(columnName);
            }

            _userSettings.ExtendedMetadataColumnsToShow = columns;

            SendNotificationMessage(NotificationMessages.TrackListingDataGridColumnsUpdated);
        }

        private void PopulateMetadataColumns()
        {
            MetadataColumns = ExtendedMetadataHelper
                .MetadataPropertyNames
                .OrderBy(p => p)
                .Select(p => new ExtendedMetadataColumnSetting { ColumnName = p, IsEnabled = IsMetadataColumnEnabled(p) })
                .ToList();
        }

        private bool IsMetadataColumnEnabled(string columnName)
        {
            return _userSettings.ExtendedMetadataColumnsToShow.Contains(columnName);
        }
    }
}
