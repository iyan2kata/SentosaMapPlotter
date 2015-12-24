using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace myWpfWinApp01.Models
{
    public class NodeData : INotifyPropertyChanged
    {
        private int _nodeId;
        public int NodeId
        {
            get { return _nodeId; }
            set { _nodeId = value; }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private double _lat;
        public double Lat
        {
            get { return _lat; }
            set
            {
                if (value != _lat)
                {
                    _lat = value;
                    OnPropertyChanged("Lat");
                }
            }
        }

        private double _long;
        public double Long
        {
            get { return _long; }
            set
            {
                if (value != _long)
                {
                    _long = value;
                    OnPropertyChanged("Long");
                }
            }
        }

        private double _pointX;
        public double PointX
        {
            get { return _pointX;}
            set
            {
                if (value != _pointX)
                {
                    _pointX = value;
                    OnPropertyChanged("PointX");
                }
            }
        }

        private double _pointY;
        public double PointY
        {
            get { return _pointY; }
            set
            {
                if (value != _pointY)
                {
                    _pointY = value;
                    OnPropertyChanged("PointY");
                }
            }
        }

        private string _nodeType;

        public string Type
        {
            get { return _nodeType; }
            set
            {
                if (value != _nodeType)
                {
                    _nodeType = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        private bool _biDirect;

        public bool BiDirectional
        {
            get { return _biDirect; }
            set
            {
                if (value != _biDirect)
                {
                    _biDirect = value;
                    OnPropertyChanged("BiDirectional");
                }
            }
        }

        private bool _isNewNode;

        public bool IsNewNode
        {
            get { return _isNewNode;}
            set
            {
                if (value != IsNewNode)
                {
                    _isNewNode = value;
                    OnPropertyChanged("IsNewNode");
                }
            }
        }

        //public Collection<NodeType> NodeTypes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public class NodeType
    {
        private int _nodeType;
        public int NodeTypeId
        {
            get { return _nodeType;}
            set { _nodeType = value; }
        }

        private string _nodeName;
        public string TypeName
        {
            get { return _nodeName; }
            set { _nodeName = value; }
        }
    }


    /// <summary>
    /// for Nodes
    /// </summary>
    public class NodeDataViewModel
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public int nodeId { get; set; }
        //public int oriId { get; set; }
        public string lineColor { get; set; }
    }


    /// <summary>
    /// for Waiting Nodes
    /// </summary>
    public class WaitNodeViewModel
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string type { get; set; }
        public string lineColor { get; set; }
        public string title { get; set; }
        public string category { get; set; }
        public int nodeId { get; set; }
        //public int forNodeId { get; set; }
    }

    /// <summary>
    /// Just for log IDs changing
    /// </summary>
    public class LogNodeUpdated
    {
        public int NodeOriId { get; set; }
        public int NodeNewId { get; set; }
        public string NodeType { get; set; }
    }



}
