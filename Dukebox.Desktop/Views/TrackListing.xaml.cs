using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Factories;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.ViewModel;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Views
{
    public partial class TrackListing : UserControl
    {
        private const string tracksDataGridElementName = "TrackListingsGrid";
        private const string layoutGridElementName = "TrackListingLayoutGrid";
        private const float trackListingPreviewSearchControlHeight = 0.15f;

        private readonly List<string> _columnsToKeep = new List<string> { "Artist", "Album", "Title" };

        private readonly IDukeboxUserSettings _settings; 
        private readonly DataGrid _tracksDataGrid;
        private readonly Grid _layoutGrid;

        public TrackListing()
        {
            InitializeComponent();

            var userSettingsFactory = new DukeboxUserSettingsFactory();

            _settings = userSettingsFactory.GetInstance();
            _tracksDataGrid = FindName(tracksDataGridElementName) as DataGrid;
            _layoutGrid = FindName(layoutGridElementName) as Grid;

            SetupEventAndMessageHandlers();

            UpdateTracksDataGridColumns();
        }

        private void SetupEventAndMessageHandlers()
        {
            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                if (nm.Notification == NotificationMessages.TrackListingDataGridColumnsUpdated)
                {
                    UpdateTracksDataGridColumns();
                }
            });

            DataContextChanged += (o, e) =>
            {
                if (e.NewValue is TrackListingPreviewViewModel)
                {
                    _layoutGrid.RowDefinitions[0].Height = new GridLength(trackListingPreviewSearchControlHeight, GridUnitType.Star);
                }
            };
        }

        #region Extended Metadata Rendering

        private void UpdateTracksDataGridColumns()
        {
            Dispatcher.Invoke(DoUpdateTracksDataGridColumns);
        }

        private void DoUpdateTracksDataGridColumns()
        {
            var extendedMetadataColumns = _settings.ExtendedMetadataColumnsToShow;

            var columnsToRemove = _tracksDataGrid.Columns
                .Where(c =>
                {
                    var headerName = c.Header.ToString();

                    return !_columnsToKeep.Contains(headerName) &&
                        !extendedMetadataColumns.Contains(headerName) && 
                        !string.IsNullOrEmpty(headerName);
                })
                .ToList();            

            columnsToRemove.ForEach(c => _tracksDataGrid.Columns.Remove(c));

            var columnsToAdd = extendedMetadataColumns
                .Where(ec => !_tracksDataGrid.Columns.Any(c => c.Header.ToString().Equals(ec)))
                .ToList();

            columnsToAdd.ForEach(AddMetadataColumnToGrid);

            ResizeMetadataColumns();
        }

        private void AddMetadataColumnToGrid(string metadataName)
        {
            var newColumn = CloneNewDataGridColumn(metadataName);

            newColumn.Header = metadataName;
            newColumn.SortMemberPath = null;
            newColumn.DisplayIndex = _tracksDataGrid.Columns.Count;

            _tracksDataGrid.Columns.Add(newColumn);
        }

        private DataGridTemplateColumn CloneNewDataGridColumn(string metadataName)
        {
            var xmlStringBuilder = new StringBuilder();
            var xmlWriter = XmlWriter.Create(xmlStringBuilder, new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });
            var xamlDesignerSerializationManager = new XamlDesignerSerializationManager(xmlWriter) { XamlWriterMode = XamlWriterMode.Expression };

            var firstColumn = _tracksDataGrid.Columns[1] as DataGridTemplateColumn;
            XamlWriter.Save(firstColumn, xamlDesignerSerializationManager);

            var firstColumnXamlString = xmlStringBuilder.ToString();
            firstColumnXamlString = InjectExtendedMetadataBindings(firstColumnXamlString, metadataName);

            var xmlStringReader = new StringReader(firstColumnXamlString);
            var xmlReader = XmlReader.Create(xmlStringReader);

            var newDataGridColumn = XamlReader.Load(xmlReader) as DataGridTemplateColumn;

            return newDataGridColumn;
        }

        private string InjectExtendedMetadataBindings(string metadataColumnXamlString, string metadataName)
        {
            var bindingString = $"{{Binding ExtendedMetadata, Converter={{StaticResource ListDictionaryKeyToValueConverter}}, ConverterParameter = '{metadataName}'";
            
            metadataColumnXamlString = metadataColumnXamlString.Replace("<TextBlock Text=\"\"",
                $"<TextBlock Text=\"{bindingString}}}\" ");
            metadataColumnXamlString = metadataColumnXamlString.Replace("<TextBox Visibility=\"Visible\"",
                $"<TextBox Text=\"{bindingString}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}\" Visibility=\"Visible\"");
            metadataColumnXamlString = metadataColumnXamlString.Replace("Width=\"{TemplateBinding assembly:TextBoxHelper.ButtonWidth}\" Visibility=\"Visible\"",
                "Width=\"{TemplateBinding assembly:TextBoxHelper.ButtonWidth}\" Visibility=\"Collapsed\"");

            return metadataColumnXamlString;
        }

        private void ResizeMetadataColumns()
        {
            foreach (var column in _tracksDataGrid.Columns)
            {
                column.Width = column.ActualWidth;
                column.Width = DataGridLength.Auto;
            }
        }

        #endregion

        private void TrackListingRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;

            if (row == null)
            {
                return;
            }

            var item = row.Item as ITrack;
            var trackListingViewModel = DataContext as ITrackListingViewModel;

            trackListingViewModel?.LoadTrack?.Execute(item);
        }
    }
}
