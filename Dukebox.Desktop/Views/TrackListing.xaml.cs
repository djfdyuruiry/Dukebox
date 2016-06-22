using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Factories;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for LibraryListing.xaml
    /// </summary>
    public partial class TrackListing : UserControl
    {
        private const string tracksDataGridElementName = "TrackListingsGrid";

        private readonly IDukeboxUserSettings _settings; 
        private readonly DataGrid _tracksDataGrid;

        public TrackListing()
        {
            InitializeComponent();

            var userSettingsFactory = new DukeboxUserSettingsFactory();

            _settings = userSettingsFactory.GetInstance();
            _tracksDataGrid = FindName(tracksDataGridElementName) as DataGrid;

            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                if (nm.Notification == NotificationMessages.TrackListingDataGridColumnsUpdated)
                {
                    UpdateTracksDataGridColumns();
                }
            });

            UpdateTracksDataGridColumns();
        }

        #region Extended Metadata Rendering

        private void UpdateTracksDataGridColumns()
        {
            Dispatcher.Invoke(DoUpdateTracksDataGridColumns);
        }

        private void DoUpdateTracksDataGridColumns()
        {
            var extendedMetadataColumns = _settings.ExtendedMetadataColumnsToShow;

            var columnsToRemove = _tracksDataGrid.Columns.Where(c => extendedMetadataColumns.Contains(c.Header.ToString())).ToList();
            columnsToRemove.ForEach(c => _tracksDataGrid.Columns.Remove(c));

            extendedMetadataColumns.ForEach(AddMetadataColumnToGrid);

            ResizeMetadataColumns();
        }

        private void AddMetadataColumnToGrid(string metadataName)
        {
            var newColumn = CloneNewDataGridColumn(metadataName);

            newColumn.Header = metadataName;
            newColumn.SortMemberPath = null;

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

            var firstColumn = _tracksDataGrid.Columns.First() as DataGridTemplateColumn;
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

            metadataColumnXamlString = metadataColumnXamlString.Replace("Text=\"\"", string.Empty);
            metadataColumnXamlString = metadataColumnXamlString.Replace("<TextBlock ",
                $"<TextBlock Text=\"{bindingString}}}\" ");
            metadataColumnXamlString = metadataColumnXamlString.Replace("<TextBox ",
                $"<TextBox Text=\"{bindingString}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}\" ");

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

            var item = row.Item as TrackWrapper;
            var trackListingViewModel = DataContext as ITrackListingViewModel;

            trackListingViewModel?.LoadTrack?.Execute(item?.Data);
        }
    }
}
