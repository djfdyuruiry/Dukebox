using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;
using Dukebox.Desktop.Factories;

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
        }

        private void AddMetadataColumnToGrid(string metadataName)
        {
            var newColumn = CloneNewDataGridTemplateColumn();

            newColumn.Header = metadataName;

            _tracksDataGrid.Columns.Add(newColumn);
        }

        private DataGridTemplateColumn CloneNewDataGridTemplateColumn()
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

            var xmlStringReader = new StringReader(xmlStringBuilder.ToString());
            var xmlReader = XmlReader.Create(xmlStringReader);

            var dataGridColumn = XamlReader.Load(xmlReader) as DataGridTemplateColumn;

            return dataGridColumn;
        }

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
