using System;
using System.Collections.Generic;
using System.Linq;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Factories;

namespace Dukebox.Desktop.ViewModel
{
    public class MetadataColumnsSettingsViewModel : ViewModelBase, IMetadataColumnsSettingsViewModel
    {
        private readonly IDukeboxUserSettings _userSettings;

        public List<ExtendedMetadataColumnSetting> MetadataColumns { get; private set; }

        public MetadataColumnsSettingsViewModel(IDukeboxUserSettings userSettings, AudioFileMetadataFactory audioFileMetadataFactory)
        {
            _userSettings = userSettings;

            var metadataTagType = audioFileMetadataFactory.GetConcreteMetadataTagType();
            PopulateMetadataColumns(metadataTagType);

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

        private void PopulateMetadataColumns(Type metadataTagType)
        {
            MetadataColumns = metadataTagType
                .GetProperties()
                .Where(p => p.CanRead)
                .OrderBy(p => p.Name)
                .Select(p => new ExtendedMetadataColumnSetting { ColumnName = p.Name, IsEnabled = IsMetadataColumnEnabled(p.Name) })
                .ToList();
        }

        private bool IsMetadataColumnEnabled(string columnName)
        {
            return _userSettings.ExtendedMetadataColumnsToShow.Contains(columnName);
        }
    }
}
