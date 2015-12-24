using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;

namespace myWpfWinApp01
{
    public class MyTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            //string rootDir = "C:\\Users\\Admin\\Dropbox\\Sentosa Map Plotter\\mapPlotter\\tiles2";
            string rootDir = Directory.GetCurrentDirectory() + "\\tiles2";
            string path = System.IO.Path.Combine(rootDir, zoomLevel.ToString(), x.ToString(), y.ToString() + ".jpg");
            return new Uri(path);
        }
    }

    public class MyTileLayer : MapTileLayer
    {
        public MyTileLayer()
        {
            TileSource = new MyTileSource();
        }

        public string UriFormat
        {
            get { return TileSource.UriFormat; }
            set { TileSource.UriFormat = value; }
        }
    }

    
}
