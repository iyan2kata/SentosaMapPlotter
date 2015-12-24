using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myWpfWinApp01.Models
{
    public class PathData : INotifyPropertyChanged
    {
        private int _pathId;
        public int PathId
        {
            get { return _pathId; }
            set { _pathId = value; }
        }

        private double _time;
        public double TimeNeeded
        {
            get { return _time; }
            set
            {
                if (value != _time)
                {
                    _time = value;
                    OnPropertyChanged("TimeNeeded");
                }
            }
        }

        private int _from;
        public int FromNodeId
        {
            get { return _from; }
            set
            {
                if (value != _from)
                {
                    _from = value;
                    OnPropertyChanged("FromNodeId");
                }
            }
        }

        private int _to;
        public int ToNodeId
        {
            get { return _to; }
            set
            {
                if (value != _to)
                {
                    _to = value;
                    OnPropertyChanged("ToNodeId");
                }
            }
        }

        private bool _bidirect;
        public bool BiDirectional
        {
            get { return _bidirect; }
            set
            {
                if (value != _bidirect)
                {
                    _bidirect = value;
                    OnPropertyChanged("BiDirectional");
                }
            }
        }

        private double _distance;
        public double Distance
        {
            get { return _distance; }
            set
            {
                if (value != _distance)
                {
                    _distance = value;
                    OnPropertyChanged("Distance");
                }
            }
        }

        public List<string> TypeList { get; set; } 

        /// Added on May 7th 2015
        /// Needed for generate edges.json
        public double[][] mapOverLays { get; set; }

        /// <summary>
        /// Gets or sets the route nodes.
        /// </summary>
        /// <value>
        /// The route nodes.
        /// </value>
        public Collection<NodeData> routeNodes { get; set; }


        private string _lineColor;
        public string LineColor
        {
            get { return _lineColor; }
            set
            {
                if (value != _lineColor)
                {
                    _lineColor = value;
                    OnPropertyChanged("LineColor");
                }
            }
        }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    /// <summary>
    /// Raw Edge Data, from Node Drawing
    /// </summary>
    public class PathDataViewModel
    {
        public int edgeId { get; set; }
        public double time { get; set; }
        public int fromNode { get; set; }
        public int toNode { get; set; }
        public string type { get; set; }
        public bool biDirectional { get; set; }
    }

    /// <summary>
    /// Generated Format for WALK
    /// </summary>
    public class WalkEdgeViewModel
    {
        public int edgeId { get; set; }
        public double time { get; set; }
        public int fromNode { get; set; }
        public int toNode { get; set; }
        public string type { get; set; }
        public bool biDirectional { get; set; }
    }


    /// <summary>
    /// Generated Format for Waiting Edge Android
    /// </summary>
    public class WaitEdgeViewModel
    {
        public int edgeId { get; set; }
        public double time { get; set; }
        public int fromNode { get; set; }
        public int toNode { get; set; }
        public string type { get; set; }
        public bool biDirectional { get; set; }
        public string lineColor { get; set; }
    }

    /// <summary>
    /// Generated Format for non-WALK(Transportation)
    /// </summary>
    public class TransportationEdgeViewModel
    {
        public int edgeId { get; set; }
        public int fromNode { get; set; }
        public int toNode { get; set; }
        public string type { get; set; }
        public bool biDirectional { get; set; }
        public double time { get; set; }
        public Collection<NodeDataViewModel> mapOverlays { get; set; }
        public string lineColor { get; set; }
    }

    /// <summary>
    /// Generated Format for non-WALK(Transportation)
    /// </summary>
    public class EdgeRawViewModel
    {
        public int edgeId { get; set; }
        public int fromNode { get; set; }
        public int toNode { get; set; }
        public string type { get; set; }
        public bool biDirectional { get; set; }
        public double time { get; set; }
        public double[][] mapOverlays { get; set; }
        public string lineColor { get; set; }
    }

    /// <summary>
    /// Generated Format for non-WALK Edges (Android)
    /// </summary>
    public class TransportationEdgeAndroidViewModel
    {
        public int edgeId { get; set; }
        public int fromNode { get; set; }
        public int toNode { get; set; }
        public string type { get; set; }
        public bool biDirectional { get; set; }
        public double time { get; set; }
        public string mapOverlays { get; set; }
        public string lineColor { get; set; }
    }


    public class currentActiveRoute
    {
        public int PathId { get; set; }
        public int SrcNodeId { get; set; }
        public string Type { get; set; }
        public Collection<NodeData> CurrNodeDatas { get; set; }
    }

}
