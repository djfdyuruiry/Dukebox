using Dukebox.Library;
using Dukebox.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    /// <summary>
    /// 
    /// </summary>
    class AlbumBrowser : TableLayoutPanel
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private static readonly int imageWidth = 100;
        private static readonly int imageHeight = 150;

        private List<string> _imagesCached;
        private List<string> _albumArtFetched;

        /// <summary>
        /// 
        /// </summary>
        public AlbumBrowser() : base()
        {
            if (Directory.Exists("./albumArtCache"))
            {
                _imagesCached = new List<string>(Directory.GetFiles("./albumArtCache"));
                _albumArtFetched = new List<string>();

                (new Thread(LoadInitalRows)).Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadInitalRows()
        {
            Queue<Image> albumArt = FetchAlbumArtImages(10);

            Invoke(new ValueUpdateDelegate(()=>
            {
                ColumnCount = 5;
            }));

            FetchNextRow();
            FetchNextRow();

            Invoke(new ValueUpdateDelegate(()=>
            {
                Width = ColumnCount * imageWidth;
                Height = imageHeight * RowCount;
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadNextRow()
        {
            (new Thread(LoadNextRow)).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void FetchNextRow()
        {
            Queue<Image> albumArt = FetchAlbumArtImages(5);

            Invoke(new ValueUpdateDelegate(() => RowCount++ ));

            for (int column = 0; column < 5; column++)
            {
                Invoke(new ValueUpdateDelegate(()=>
                {
                    Controls.Add(new PictureBox() { Image = new Bitmap(albumArt.Dequeue(), imageWidth, imageHeight), Width = imageWidth, Height = imageHeight },
                                    column,
                                    RowCount - 1);
                }
                ));
            }

            Invoke(new ValueUpdateDelegate(() =>
            {
                Width += imageWidth;
                Height += imageHeight;
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Queue<Image> FetchAlbumArtImages(int numImages)
        {
            DateTime startTime = DateTime.Now;
            double timeTaken = 0d;

            Queue<Image> albumArt = new Queue<Image>();

            while (albumArt.Count != numImages)
            {
                string albumArtImage = _imagesCached.FirstOrDefault();
                
                albumArt.Enqueue(Image.FromFile(albumArtImage));

                _imagesCached.Remove(albumArtImage);
            }

            timeTaken = (DateTime.Now - startTime).TotalMilliseconds;
            Logger.Info("Fetching " + numImages + " album art images took " + timeTaken + "ms");

            return albumArt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
    }
}
