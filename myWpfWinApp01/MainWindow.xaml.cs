using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using myWpfWinApp01.Models;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Design;
using Microsoft.Win32;

namespace myWpfWinApp01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //For Tiles
        MapTileLayer tileLayer;

        Point currPoint, lastPoint;
        Location currLocation, lastLocation;

        bool _isStartNode = false;
        private const int IcoSize = 32;
        private const int IcoNodeSize = 14;

        //For Local Nodes Information
        Collection<NodeData> currCollection = new Collection<NodeData>();
        Collection<PathData> currPathDatas = new Collection<PathData>();

        int LastNodeId = 0, CurrNodeId = 0;
        NodeData currNodeData, lastNodeData;

        int LastPathId = 0, CurrPathId = 0;
        PathData currPathData, lastPathData;

        //For Progress UI
        private int _imax = 100, _icount = 0;
        private double _ivalue = 0.0;

        currentActiveRoute currRoute = new currentActiveRoute();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            myMap.MouseDoubleClick += new MouseButtonEventHandler(myMap_MouseDoubleClick);


            myMap.ViewChangeStart += new EventHandler<MapEventArgs>(myMap_ViewChangeStart);
            myMap.ViewChangeEnd += new EventHandler<MapEventArgs>(myMap_ViewChangeEnd);

            myMap.MouseLeftButtonUp += new MouseButtonEventHandler(myMap_MouseLeftButtonUp);

            currPoint.X = currPoint.Y = lastPoint.X = lastPoint.Y = 0.0;

            //Test Local MapTile
            AddTileOverlay();

            LeftFormInit();
        }

        /// <summary>
        /// Adds the node icon to map.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="nodeType">Type of the node.</param>
        /// <param name="iconPixelSize">Size of the icon pixel.</param>
        /// <param name="parLastNodeId">The par last node identifier.</param>
        private void AddNodeIconToMap(Location location, string nodeType, int iconPixelSize, int parLastNodeId)
        {
            MapLayer imageLayer = new MapLayer();

            Image image = new Image();
            image.Height = iconPixelSize;

            string strImgUri = MyGlobal.GetStrImageUri(nodeType);

            //Define the URI location of the image
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(strImgUri);

            //Define the image display properties
            myBitmapImage.DecodePixelHeight = iconPixelSize;
            myBitmapImage.EndInit();
            image.Source = myBitmapImage;
            image.Opacity = 1;
            image.Stretch = System.Windows.Media.Stretch.None;
            image.Cursor = Cursors.Hand;

            //For Icon Image Only
            if (nodeType != "node")
            {
                image.Tag = parLastNodeId.ToString();
            }
            else
            {
                image.Tag = "C" + parLastNodeId.ToString();
            }
            imageLayer.Tag = image.Tag;

            imageLayer.MouseLeftButtonDown += new MouseButtonEventHandler(myImage_MouseLeftButtonDown);
            imageLayer.MouseRightButtonDown += new MouseButtonEventHandler(myImage_MouseRightButtonDown);

            //Center the image around the location specified
            PositionOrigin position = PositionOrigin.Center;

            //Add the image to the defined map layer
            imageLayer.AddChild(image, location, position);
            //Add the image layer to the map
            myMap.Children.Add(imageLayer);
        }


        /// <summary>
        /// Adds the path between.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="curPoint">The current point.</param>
        private void AddPathBetween(Point startPoint, Point curPoint, List<string> typeList)
        {
            Location stLocation = myMap.ViewportPointToLocation(startPoint);
            Location curLocation = myMap.ViewportPointToLocation(curPoint);

            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new SolidColorBrush(MyGlobal.GetPathColor(typeList[typeList.Count - 1]));
            polyline.StrokeThickness = 6;
            polyline.Opacity = .5;
            polyline.Locations = new LocationCollection()
            {
                stLocation,curLocation
            };
            polyline.Tag = "E" + LastPathId;

            myMap.Children.Add(polyline);
        }


        private void AddPathBetweenNodeIds(int stNodeId, int endNodeId, string sType)
        {
            NodeData stNode = currCollection.FirstOrDefault(cc => cc.NodeId == stNodeId);
            NodeData endNode = currCollection.FirstOrDefault(cc => cc.NodeId == endNodeId);

            if (stNode != null && endNode != null)
            {
                Location stLocation = new Location()
                {
                    Latitude = stNode.Lat,
                    Longitude = stNode.Long
                };
                Location curLocation = new Location()
                {
                    Latitude = endNode.Lat,
                    Longitude = endNode.Long
                };

                MapPolyline polyline = new MapPolyline();
                polyline.Stroke = new SolidColorBrush(MyGlobal.GetPathColor(sType));
                polyline.StrokeThickness = 6;
                polyline.Opacity = .5;
                polyline.Locations = new LocationCollection()
                {
                    stLocation,
                    curLocation
                };
                polyline.Tag = "E" + LastPathId;

                myMap.Children.Add(polyline);

                // Add the Path into Collection
                var newPath = new PathData()
                {
                    PathId = LastPathId,
                    TypeList = GetSelectedPathType(),
                    FromNodeId = stNodeId,
                    ToNodeId = endNodeId,
                    BiDirectional = false,
                    Distance = MyGlobal.GetDistanceBetweenPoints(stNode.Lat,stNode.Long,endNode.Lat,endNode.Long)
                };
                AddPathToCollection(newPath);

            }
        }


        private void myMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point newPoint = e.GetPosition(this);
            Location newLocation = myMap.ViewportPointToLocation(newPoint);

            TbLatitude.Text = newLocation.Latitude.ToString();
            TbLongitude.Text = newLocation.Longitude.ToString();
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the myMap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void myMap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //NodeData fromNode, toNode;

            // Disables the default mouse click action.
            e.Handled = true;

            //Get the mouse click coordinates
            currPoint = e.GetPosition(this);
            if (lastPoint.X == 0.0)
                _isStartNode = true;

            //Convert the mouse coordinates to a locatoin on the map
            Point icoPoint = currPoint;
            Location currPin = myMap.ViewportPointToLocation(icoPoint);
            icoPoint.Y -= (IcoSize / 2) + 10;
            Location pinLocation = myMap.ViewportPointToLocation(icoPoint);

            //Adding to Node Collection
            AddNodeToCollection(currPin, currPoint);

            //Drawing Path
            if (!_isStartNode && lastPoint.X != 0.0 && lastNodeData != null)
            {
                List<string> typeList = GetSelectedPathType();
                if (typeList.Count == 0)
                {
                    MessageBox.Show("You dont select Path");
                }
                else
                {
                    if (lastPoint.X == 0.0)
                    {
                        MessageBox.Show("You have to select Node Start From");
                    }
                    else
                    {
                        //Adding to Path Collection
                        AddPathToCollection(lastNodeData, currNodeData);

                        //AddPathBetween(lastPoint, currPoint, typeList);
                        AddPathBetweenNodeIds(lastNodeData.NodeId, currNodeData.NodeId, typeList[typeList.Count - 1]);

                        //Try Redraw Previous Image Icon
                        if (lastNodeData != null)
                        {
                            RedrawImageIcon(lastNodeData.NodeId, "node", false);
                            RedrawImageIcon(lastNodeData.NodeId, "", false);
                        }
                    }
                }
            }

            string nType = GetSelectedNode("node");

            //Create Node Icon
            AddNodeIconToMap(currPin, "node", IcoNodeSize, LastNodeId);
            AddNodeIconToMap(pinLocation, nType, IcoSize, LastNodeId);

            LeftFormInit();

            lastPoint = currPoint;
            lastNodeData = currNodeData;
            if (lastPoint.X != 0.0)
                _isStartNode = false;
        }


        /// <summary>
        /// Adds the path to collection.
        /// </summary>
        /// <param name="fromNode">From node.</param>
        /// <param name="toNode">To node.</param>
        private void AddPathToCollection(NodeData fromNode, NodeData toNode)
        {
            double rangeInMeters = MyGlobal.GetDistanceBetweenPoints(fromNode.Lat, fromNode.Long, toNode.Lat, toNode.Long);

            //Default path speed is walk, transportation path is recalculated
            double speed = MyGlobal.GetTheSpeed("WALK");

            //Calculate Estimated Time
            double estimatedTime = MyGlobal.GetEstimatedTime(speed, rangeInMeters);

            LastPathId++;

            PathData pd = new PathData()
            {
                PathId = LastPathId,
                TimeNeeded = estimatedTime,
                FromNodeId = fromNode.NodeId,
                ToNodeId = toNode.NodeId,
                TypeList = GetSelectedPathType(),
                Distance = rangeInMeters
            };
            currPathDatas.Add(pd);

            currPathData = pd;
        }


        /// <summary>
        /// Gets the selected node.
        /// </summary>
        /// <param name="sType">Type of the s.</param>
        /// <returns></returns>
        private string GetSelectedNode(string sType)
        {
            string result = "";

            if (sType == "node")
            {
                result = RbNodeWalk.IsChecked == true ? "walk" : result;
                result = RbNodeBus.IsChecked == true ? "bus" : result;
                result = RbNodeTram.IsChecked == true ? "tram" : result;
                result = RbNodeMonorail.IsChecked == true ? "rail" : result;
                result = RbNodeAttraction.IsChecked == true ? "poi" : result;
                result = RbNodeCableCar.IsChecked == true ? "cable" : result;
                result = RbNodeCableway.IsChecked == true ? "cableway" : result;
            }

            return result;
        }


        /// <summary>
        /// Gets the type of the selected path.
        /// </summary>
        /// <returns></returns>
        private List<string> GetSelectedPathType()
        {
            List<string> result = new List<string>();
            if (RbPathWalk.IsChecked == true) result.Add("walk");
            if (RbPathBus.IsChecked == true) result.Add("bus");
            if (RbPathBusD.IsChecked == true) result.Add("bus-d");
            if (RbPathTram.IsChecked == true) result.Add("tram");
            if (RbPathMonorail.IsChecked == true) result.Add("rail");
            if (RbPathCableCar.IsChecked == true) result.Add("cable");
            return result;
        }


        /// <summary>
        /// Sets the selected node.
        /// </summary>
        /// <param name="sType">Type of the s.</param>
        /// <param name="sValue">The s value.</param>
        private void SetSelectedNode(string sType, string sValue)
        {
            if (sType == "node")
            {
                switch (sValue.ToLower())
                {
                    case "walk":
                        RbNodeWalk.IsChecked = true;
                        break;
                    case "bus":
                        RbNodeBus.IsChecked = true;
                        break;
                    case "tram":
                        RbNodeTram.IsChecked = true;
                        break;
                    case "rail":
                        RbNodeMonorail.IsChecked = true;
                        break;
                    case "poi":
                        RbNodeAttraction.IsChecked = true;
                        break;
                    case "cable":
                        RbNodeCableCar.IsChecked = true;
                        break;
                    default:
                        break;
                }
            }

            if (sType == "path")
            {
                string[] sPath = sValue.Split(',');
                for (int i = 0; i < sPath.Length; i++)
                {
                    switch (sPath[i].ToLower())
                    {
                        case "walk":
                            RbPathWalk.IsChecked = true;
                            break;
                        case "bus":
                            RbPathBus.IsChecked = true;
                            break;
                        case "bus-d":
                            RbPathBusD.IsChecked = true;
                            break;
                        case "tram":
                            RbPathTram.IsChecked = true;
                            break;
                        case "rail":
                            RbPathMonorail.IsChecked = true;
                            break;
                        case "cable":
                            RbPathCableCar.IsChecked = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Adds the node to currCollection.
        /// </summary>
        /// <param name="parLocation">The par location.</param>
        /// <param name="parPoint">The par point.</param>
        private void AddNodeToCollection(Location parLocation, Point parPoint)
        {
            NodeData nd = new NodeData();

            //Counter for Node ID
            LastNodeId++;

            nd.NodeId = LastNodeId;
            nd.Title = TbTitle.Text;
            nd.Lat = parLocation.Latitude;
            nd.Long = parLocation.Longitude;
            nd.PointX = parPoint.X;
            nd.PointY = parPoint.Y;

            nd.Type = GetSelectedNode("node");
            if (GetSelectedNode("node") == "walk" || GetSelectedNode("node") == "bus-d")
            {
                nd.BiDirectional = true;
            }
            else
            {
                nd.BiDirectional = false;
            }

            currCollection.Add(nd);

            //Show NodeNo
            TbNodeNo.Text = LastNodeId.ToString();

            nd.IsNewNode = true;

            currNodeData = nd;
        }

        /// <summary>
        /// Adds the path to collection.
        /// </summary>
        /// <param name="parPathData">The par path data.</param>
        private void AddPathToCollection(PathData parPathData)
        {
            LastPathId++;

            var addPath = new PathData()
            {
                PathId = LastPathId,
                TimeNeeded = parPathData.TimeNeeded,
                FromNodeId = parPathData.FromNodeId,
                ToNodeId = parPathData.ToNodeId,
                TypeList = parPathData.TypeList,
                BiDirectional = parPathData.BiDirectional
            };

            currPathDatas.Add(addPath);

            currPathData = addPath;
        }

        /// <summary>
        /// Lefts the form initialize.
        /// </summary>
        private void LeftFormInit()
        {
            TbNodeNo.Text = "[Auto]";
            TbTitle.Text = "";

            //RbNodeWalk.IsChecked = true;
            //RbNodeBus.IsChecked = false;
            //RbNodeTram.IsChecked = false;
            //RbNodeMonorail.IsChecked = false;
            //RbNodeAttraction.IsChecked = false;
            //RbNodeCableCar.IsChecked = false;
            //RbNodeCableway.IsChecked = false;

            RefreshPathTypes();
        }


        /// <summary>
        /// Refreshes the path types.
        /// </summary>
        private void RefreshPathTypes()
        {
            RbPathWalk.IsChecked = true;
            RbPathBus.IsChecked = false;
            RbPathBusD.IsChecked = false;
            RbPathTram.IsChecked = false;
            RbPathMonorail.IsChecked = false;
            RbNodeAttraction.IsChecked = false;
            RbPathCableCar.IsChecked = false;
            RbNodeCableway.IsChecked = false;
        }


        /// <summary>
        /// Handles the Click event of the BtSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            if (!currCollection.Any())
            {
                MessageBox.Show("Please draw the route first...");
            }
            else
            {
                new Thread(delegate()
                {
                    DoParseNodes();
                    //DoParseAndroid();

                    MessageBox.Show("Files created..");
                }).Start();
            }
        }

        /// <summary>
        /// Adds the tile overlay.
        /// </summary>
        private void AddTileOverlay()
        {
            tileLayer = new MyTileLayer();

            //Testing Tiles
            MyTileSource tileSource = new MyTileSource();

            // Add the tile overlay to the map layer
            tileLayer.TileSource = tileSource;

            // Add the map layer to the map
            if (!myMap.Children.Contains(tileLayer))
            {
                myMap.Children.Add(tileLayer);
            }
            tileLayer.Opacity = 1;

        }


        /// <summary>
        /// Handles the Checked event of the RbNode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RbNode_Checked(object sender, RoutedEventArgs e)
        {
            //RbPathWalk.IsChecked = true;

            RadioButton rb = (RadioButton)sender;
            switch (rb.Name)
            {
                //case "RbNodeWalk":
                //    RbPathWalk.IsChecked = true;
                //    break;
                case "RbNodeTram":
                    RbPathWalk.IsChecked = true;
                    RbPathTram.IsChecked = true;
                    break;
                case "RbNodeMonorail":
                    RbPathWalk.IsChecked = true;
                    RbPathMonorail.IsChecked = true;
                    break;
                case "RbNodeCableCar":
                    RbPathWalk.IsChecked = true;
                    RbPathCableCar.IsChecked = true;
                    break;
                case "RbNodeCableway":
                    RbPathWalk.IsChecked = true;
                    RbPathCableway.IsChecked = true;
                    break;
                //case "RbNodeBus":
                //case "RbNodeAttraction":
                //    RefreshPathTypes();
                //    break;
                default:
                    break;
            }
        }

        private void myMap_ViewChangeStart(object sender, MapEventArgs e)
        {

        }

        /// <summary>
        /// Handles the ViewChangeEnd event of the myMap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MapEventArgs"/> instance containing the event data.</param>
        private void myMap_ViewChangeEnd(object sender, MapEventArgs e)
        {
            //Reset the Last Point
            lastPoint = new Point() { X = 0.0, Y = 0.0 };

        }


        /// <summary>
        /// Handles the MouseLeftButtonDown event of the myImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var imgNode = sender as MapLayer;
            NodeData nd;
            if (imgNode != null && imgNode.GetType() == typeof(MapLayer))
            {
                //MessageBox.Show("Tag : " + imgNode.Tag);

                //Get The Node Information
                nd = currCollection.FirstOrDefault(cc => cc.NodeId.ToString() == imgNode.Tag.ToString());
                //PathData pd = currPathDatas.FirstOrDefault(cpd => cpd.ToNodeId.ToString() == imgNode.Tag.ToString());
                PathData pd = currPathDatas.FirstOrDefault(cpd => cpd.FromNodeId.ToString() == imgNode.Tag.ToString());

                if (nd != null)
                {
                    //Load Detail Information
                    TbNodeNo.Text = nd.NodeId.ToString();
                    TbTitle.Text = nd.Title;

                    //Set Node Detail Info
                    SetSelectedNode("node", nd.Type);

                    //Set Path Type by Refresh it first
                    RefreshPathTypes();
                    if (pd != null)
                    {
                        SetSelectedNode("path", string.Join(",", pd.TypeList.ToList()));
                    }

                    CurrNodeId = nd.NodeId;
                    currNodeData = nd;

                    //Selected Node become LastPoint
                    lastNodeData = nd;
                    lastPoint = new Point()
                    {
                        X = nd.PointX,
                        Y = nd.PointY
                    };
                }
            }
        }

        private void myImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var imgLayer = sender as MapLayer;
            if (imgLayer != null && imgLayer.GetType() == typeof(MapLayer))
            {
                NodeData nd = currCollection.FirstOrDefault(cc => cc.NodeId.ToString() == imgLayer.Tag.ToString());
                if (nd != null)
                {
                    //Point mPoint = new Point()
                    //{
                    //    X = nd.PointX,
                    //    Y = nd.PointY
                    //};

                    List<string> pList = GetSelectedPathType();
                    //AddPathBetween(lastPoint, mPoint, pList);
                    AddPathBetweenNodeIds(lastNodeData.NodeId, nd.NodeId, pList[pList.Count - 1]);

                    RedrawImageIcon(nd.NodeId, "node", false);
                    RedrawImageIcon(nd.NodeId, "", false);
                    RedrawImageIcon(CurrNodeId, "node", false);
                    RedrawImageIcon(CurrNodeId, "", false);

                    lastNodeData = nd;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the BtReset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtReset_Click(object sender, RoutedEventArgs e)
        {
            //Clear All

            //Clear Paths
            var paths = myMap.Children.OfType<MapPolyline>().ToList();
            foreach (var p in paths)
            {
                if (p.GetType() == typeof(MapPolyline))
                {
                    myMap.Children.Remove(p);
                }
            }
            //Clear Nodes
            var nodes = myMap.Children.OfType<MapLayer>().ToList();
            foreach (var n in nodes)
            {
                if (n.GetType() == typeof(MapLayer))
                {
                    myMap.Children.Remove(n);
                }
            }

            //Refresh Values
            currLocation = new Location();
            lastLocation = new Location();

            lastPoint = new Point() { X = 0.0, Y = 0.0 };

            LastNodeId = CurrNodeId = 0;

            currNodeData = lastNodeData = new NodeData();
            currPathData = lastPathData = new PathData();

            _nodesGenerated = new Collection<NodeDataViewModel>();
            _waitsGenerated = new Collection<WaitNodeViewModel>();
            _edgesGenerated = new Collection<WalkEdgeViewModel>();
            _edgeWaitsGenerated = new Collection<WaitEdgeViewModel>();

            _logs = new Collection<LogNodeUpdated>();

            currCollection = new Collection<NodeData>();
            currPathDatas = new Collection<PathData>();

            LeftFormInit();
            RefreshPathTypes();
        }


        /// <summary>
        /// Handles the Click event of the BtEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtEdit_Click(object sender, RoutedEventArgs e)
        {
            //Image imgNode = new Image();
            //Location imgLocation = new Location();
            //Point imgPoint = new Point();

            if (CurrNodeId > 0)
            {
                NodeData nd = currCollection.FirstOrDefault(cl => cl.NodeId == CurrNodeId);
                PathData pdFrom = currPathDatas.FirstOrDefault(pdl => pdl.FromNodeId == CurrNodeId);

                #region Changing Icon Image

                string nodeType = GetSelectedNode("node");
                if (nd != null) RedrawImageIcon(CurrNodeId, nodeType, true);

                #endregion

                #region Edit NodeData

                if (nd != null)
                {
                    //nd.Title = TbTitle.Text;
                    //nd.Type = nodeType;
                    currCollection.Where(cc => cc.NodeId == CurrNodeId).ToList().ForEach(cc => cc.Title = TbTitle.Text);
                    currCollection.Where(cc => cc.NodeId == CurrNodeId).ToList().ForEach(cc => cc.Type = nodeType);
                }

                if (pdFrom != null)
                {
                    //pdFrom.TypeList = GetSelectedPathType();
                    currPathDatas.Where(cp => cp.FromNodeId == CurrNodeId).ToList().ForEach(cp => cp.TypeList = GetSelectedPathType());
                }

                #endregion

                MessageBox.Show("Node is updated.");
            }
            else
            {
                MessageBox.Show("You must select 1 node to Edit");
            }

        }


        /// <summary>
        /// Handles the Click event of the BtDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtDelete_Click(object sender, RoutedEventArgs e)
        {
            string strMsg =
                "You gonna delete an imported node, and all edges with route included this node, Are You Sure?";

            if (CurrNodeId > 0)
            {
                NodeData nd = currCollection.FirstOrDefault(cl => cl.NodeId == CurrNodeId);
                if (nd != null)
                {
                    if (!nd.IsNewNode)
                    {
                        MessageBoxResult confirmResult = MessageBox.Show(strMsg,
                            "Deleting Node Id : " + CurrNodeId.ToString(),
                            MessageBoxButton.YesNo);
                        if (confirmResult == MessageBoxResult.Yes)
                        {
                            DeleteNodeAndEdges(CurrNodeId);
                        }
                    }
                    else
                    {
                        DeleteNodeAndEdges(CurrNodeId);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nothing to Delete..No Current Active Node.");
            }

        }

        private void DeleteNodeAndEdges(int delNodeId)
        {
            List<MapLayer> theMapLayers = new List<MapLayer>();
            theMapLayers = myMap.Children.OfType<MapLayer>().Where(ml => ml.Tag.ToString() == delNodeId.ToString()).ToList();
            //1. Get the Map Layer with NodeId as Tag
            if (theMapLayers.Any())
            {
                //2. Remove the MapLayer(Icon Image)
                foreach (var ml in theMapLayers)
                {
                    myMap.Children.Remove(ml);
                }

                //2b. Remove the MapLayer(Icon Node)
                theMapLayers = myMap.Children.OfType<MapLayer>().Where(ml => ml.Tag.ToString() == "C" + delNodeId.ToString()).ToList();
                if (theMapLayers.Any())
                {
                    foreach (var ml in theMapLayers)
                    {
                        myMap.Children.Remove(ml);
                    }
                }

                //3. Get Edges which include this Location
                var edges = currPathDatas.Where(cp => cp.FromNodeId == delNodeId || cp.ToNodeId == delNodeId).ToList();
                foreach (var e in edges)
                {
                    MapPolyline edgeLine =
                        myMap.Children.OfType<MapPolyline>().FirstOrDefault(pl => pl.Tag.ToString() == "E" + e.PathId.ToString());
                    if (edgeLine != null)
                    {
                        //4. Remove the Edges
                        myMap.Children.Remove(edgeLine);
                    }
                }

                //4. Remove Nodes and Edges from Collection
                var nodes = currCollection.Where(x => x.NodeId == delNodeId).ToList();
                if(nodes.Any())
                    RemoveNodesFromCollection(nodes);

                RemovePathsFromCollection(edges);
            }
        }

        #region RemoveData
        /// <summary>
        /// Removes the nodes from collection.
        /// </summary>
        /// <param name="toRemoves">To removes.</param>
        private void RemoveNodesFromCollection(List<NodeData> toRemoves)
        {
            if (toRemoves.Any())
            {
                foreach (var item in toRemoves)
                {
                    currCollection.Remove(item);
                }
            }            
        }
        /// <summary>
        /// Removes the paths from collection.
        /// </summary>
        /// <param name="toRemoves">To removes.</param>
        private void RemovePathsFromCollection(List<PathData> toRemoves)
        {
            if (toRemoves.Any())
            {
                foreach (var item in toRemoves)
                {
                    currPathDatas.Remove(item);
                }
            }
        }
        #endregion

        /// <summary>
        /// Redraws the image icon.
        /// </summary>
        /// <param name="parNodeId">The par node identifier.</param>
        /// <param name="parNodeType">Type of the par node.</param>
        /// <param name="isEditNode">if set to <c>true</c> [is edit node].</param>
        private void RedrawImageIcon(int parNodeId, string parNodeType, bool isEditNode)
        {
            Image imgNode = new Image();
            Location imgLocation = new Location();
            Point imgPoint = new Point();

            if (parNodeId > 0)
            {
                NodeData nd = currCollection.FirstOrDefault(cl => cl.NodeId == parNodeId);

                #region Changing Icon Image

                //MessageBox.Show("Current Node Id : " + CurrNodeId.ToString());
                var imgLayer = new MapLayer();

                string tagToSearch = parNodeType.ToLower() == "node"
                    ? ("C" + parNodeId.ToString())
                    : parNodeId.ToString();

                //imgLayer = myMap.Children.OfType<MapLayer>().FirstOrDefault(ml => ml.Tag.ToString() == parNodeId.ToString());
                imgLayer = myMap.Children.OfType<MapLayer>().FirstOrDefault(ml => ml.Tag.ToString() == tagToSearch);

                if (imgLayer != null && imgLayer.Children.Count > 0) imgNode = ((Image)imgLayer.Children[0]);

                if (imgLayer != null && imgNode != null && nd != null)
                {
                    imgLocation = new Location()
                    {
                        Latitude = nd.Lat,
                        Longitude = nd.Long
                    };

                    int icPointX = (int)myMap.LocationToViewportPoint(imgLocation).X;
                    int icPointY = (int)myMap.LocationToViewportPoint(imgLocation).Y;
                    icPointY = (parNodeType.ToLower() == "node") ? icPointY : (icPointY - ((IcoSize / 2) + 10));

                    imgPoint = new Point()
                    {
                        X = icPointX,
                        Y = icPointY
                    };

                    imgLocation = myMap.ViewportPointToLocation(imgPoint);

                    //Remove current Image
                    imgLayer.Children.Remove(imgNode);
                    //Remove current MapLayer
                    myMap.Children.Remove(imgLayer);

                    //string nodeType = GetSelectedNode("node");
                    //string strImgUri = MyGlobal.GetStrImageUri(nodeType);

                    string nodeType = string.IsNullOrEmpty(parNodeType) ? nd.Type : parNodeType;
                    string strImgUri = MyGlobal.GetStrImageUri(nodeType);

                    imgNode = new Image();

                    imgNode.Height = parNodeType.ToLower() == "node" ? IcoNodeSize : IcoSize;

                    imgNode.Cursor = Cursors.Hand;
                    imgNode.Tag = parNodeType.ToLower() == "node" ? "C" + parNodeId.ToString() : parNodeId.ToString();

                    //Define the URI location of the image
                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(strImgUri);

                    //Define the image display properties
                    myBitmapImage.DecodePixelHeight = parNodeType.ToLower() == "node" ? IcoNodeSize : IcoSize;

                    myBitmapImage.EndInit();

                    imgNode.Source = myBitmapImage;

                    //Recreate MapLayer
                    imgLayer = new MapLayer();
                    imgLayer.Tag = imgNode.Tag;

                    imgLayer.MouseLeftButtonDown += new MouseButtonEventHandler(myImage_MouseLeftButtonDown);
                    imgLayer.MouseRightButtonDown += new MouseButtonEventHandler(myImage_MouseRightButtonDown);

                    //Add the image to the defined map layer
                    imgLayer.AddChild(imgNode, imgLocation, PositionOrigin.Center);
                    //Add the image layer to the map
                    myMap.Children.Add(imgLayer);

                }
                #endregion
            }
        }


        /// <summary>
        /// Handles the Click event of the BtLoadJson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtLoadJson_Click(object sender, RoutedEventArgs e)
        {
            DoLoadJsonFiles();
        }



        //For Generation Purpose
        private string _currDirectory = Directory.GetCurrentDirectory() + "\\output";
        private string _edgeResult = "";

        Collection<NodeDataViewModel> _nodesGenerated = new Collection<NodeDataViewModel>();
        Collection<WaitNodeViewModel> _waitsGenerated = new Collection<WaitNodeViewModel>();

        Collection<WalkEdgeViewModel> _edgesGenerated = new Collection<WalkEdgeViewModel>();
        Collection<TransportationEdgeViewModel> _transGenerated = new Collection<TransportationEdgeViewModel>();

        Collection<WaitEdgeViewModel> _edgeWaitsGenerated = new Collection<WaitEdgeViewModel>();

        Collection<WaitEdgeViewModel> _tmpEdgeWaitsGenerated = new Collection<WaitEdgeViewModel>();

        Collection<TransportationEdgeAndroidViewModel> _transAndroidGenerated = new Collection<TransportationEdgeAndroidViewModel>();

        Collection<EdgeRawViewModel> _edgesRawGenerated = new Collection<EdgeRawViewModel>();

        Collection<LogNodeUpdated> _logs = new Collection<LogNodeUpdated>();

        // TempData
        List<PathData> myPathDatas = new List<PathData>();

        /// <summary>
        /// Does the parse nodes.
        /// Modified on May 7th, 2015. To Generate the Raw Json Format. (Android's Format without Waiting Nodes and Edges)
        /// </summary>
        private void DoParseNodes()
        {
            #region Parse Nodes for Nodes.Json

            string jsonNodes = "";
            int maxId = 0;

            //Add to View Model
            foreach (var node in currCollection)
            {
                NodeDataViewModel v = new NodeDataViewModel
                {
                    lat = node.Lat,
                    lon = node.Long,
                    type = node.Type.ToUpper(),
                    title = node.Title,
                    nodeId = node.NodeId
                };

                _nodesGenerated.Add(v);
            }

            //Generate Raw nodes.json from Drawing
            var outputJson = _nodesGenerated.Select(ng => new
            {
                ng.lat,
                ng.lon,
                ng.type,
                ng.title,
                ng.nodeId
            }).ToList();

            string nodeJson = "[";
            int nj = 0;
            foreach (var node in outputJson)
            {
                nodeJson += new JavaScriptSerializer().Serialize(node);
                nj++;
                if (nj < outputJson.Count)
                {
                    nodeJson += ",\r\n\n";
                }
            }
            nodeJson += "]";
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\nodes.json", nodeJson);

            maxId = _nodesGenerated.Max(n => n.nodeId);
            //MessageBox.Show("Max Node Id is " + maxId.ToString());

            var transNodes =
                _nodesGenerated.Where(ng => ng.type.ToLower() != "walk"
                    && ng.type.ToLower() != "poi"
                    && ng.type.ToLower() != "mrt"
                    && ng.type.ToLower() != "busint").ToList();

            //var waitNodes = new List<WaitNodeViewModel>();

            #region Android Nodes JSON Generated
            if (transNodes.Any())
            {
                foreach (NodeDataViewModel tr in transNodes)
                {
                    // New Method on May 26, 2015
                    // In order to create Waiting Nodes with lineColor

                    var greenNode = new NodeDataViewModel()
                    {
                        lat = tr.lat,
                        lon = tr.lon,
                        title = tr.title,
                        type = tr.type,
                        lineColor = tr.lineColor,
                        nodeId = tr.nodeId
                    };

                    //Re-initialize
                    var greenTransEdges = new List<PathData>();
                    var stop_nodes = new List<NodeDataViewModel>();

                    //Find edge connected to this specific node
                    currPathDatas.Where(cp => cp.ToNodeId == greenNode.nodeId || cp.FromNodeId == greenNode.nodeId).ToList().ForEach(
                        path =>
                        {
                            if (path.TypeList[path.TypeList.Count - 1].ToLower() != "walk")
                            {
                                greenTransEdges.Add(path);
                            }
                        });

                    //Collecting lineColor exists in edges for this specific node
                    var pcolors = new List<string>();
                    greenTransEdges.ForEach(path =>
                    {
                        pcolors.Add(path.LineColor);
                    });

                    pcolors = pcolors.Distinct().ToList();
                    for (int c = 0; c < pcolors.Count; c++)
                    {
                        if (c == 0)
                        {
                            maxId++;

                            // Create only 1 WAITING node
                            var waitNode = new WaitNodeViewModel()
                            {
                                lat = greenNode.lat,
                                lon = greenNode.lon,
                                title = "",//greenNode.title,
                                type = "WAITING", //"WAIT",
                                category = "",//greenNode.type,
                                lineColor = pcolors[c],
                                nodeId = greenNode.nodeId
                            };
                            //Add as Waiting Node, will be added eventually
                            _waitsGenerated.Add(waitNode);

                            // Change current node
                            _nodesGenerated.Where(ng => ng.nodeId == greenNode.nodeId).ToList().ForEach(n =>
                            {
                                n.lineColor = pcolors[c];
                                n.nodeId = maxId;
                            });
                            // Logging just for debug
                            ChangeNodeLogs(greenNode.nodeId, maxId, greenNode.type.ToUpper());

                        }
                        //else
                        //{
                        //maxId++;
                        // Add New Node lineColor
                        var stopNode = new NodeDataViewModel()
                        {
                            lat = greenNode.lat,
                            lon = greenNode.lon,
                            title = greenNode.title,
                            type = greenNode.type,
                            nodeId = maxId,
                            lineColor = pcolors[c]
                        };
                        stop_nodes.Add(stopNode);
                            //_nodesGenerated.Add(stopNode);

                        //}

                    }

                    //Update the greenTransEdges
                    foreach (var edge in greenTransEdges)
                    {
                        stop_nodes.Where(sn => sn.lineColor == edge.LineColor).ToList().ForEach(node =>
                        {
                            if (edge.FromNodeId == greenNode.nodeId) edge.FromNodeId = node.nodeId;
                            if (edge.ToNodeId == greenNode.nodeId) edge.ToNodeId = node.nodeId;
                        });
                    }

                    // TmpId, will be updated later
                    int tmpId = 1000;
                    stop_nodes.ForEach(node =>
                    {
                        //var newlogid = _logs.FirstOrDefault(l => l.NodeOriId == greenNode.nodeId);
                        //int theid = newlogid == null ? greenNode.nodeId : newlogid.NodeNewId;

                        tmpId++;
                        var toGreenNode = new WaitEdgeViewModel()
                        {
                            edgeId = tmpId,
                            fromNode = node.nodeId,
                            toNode = greenNode.nodeId,
                            time = 0,
                            type = "WAIT",
                            biDirectional = false,
                            lineColor = node.lineColor
                        };
                        _tmpEdgeWaitsGenerated.Add(toGreenNode);

                        int time = greenNode.type.ToLower() == "cable" ? 15 : 10;

                        tmpId++;
                        var fromGreenNode = new WaitEdgeViewModel()
                        {
                            edgeId = tmpId,
                            fromNode = greenNode.nodeId,
                            toNode = node.nodeId,
                            time = time * 60,
                            type = "WAIT",
                            biDirectional = false,
                            lineColor = node.lineColor
                        };
                        _tmpEdgeWaitsGenerated.Add(fromGreenNode);
                    });
                }
            }
            #endregion

            string androidNodes = "[";
            int i = 0;
            foreach (var n in _nodesGenerated)
            {
                if (n.type.ToLower() == "walk" || n.type.ToLower() == "poi"
                    || n.type.ToLower() == "mrt" || n.type.ToLower() == "busint")
                {
                    androidNodes += new JavaScriptSerializer().Serialize(new
                    {
                        lat = n.lat,
                        lon = n.lon,
                        type = n.type,
                        title = n.title,
                        nodeId = n.nodeId
                    });
                }
                else
                {
                    androidNodes += new JavaScriptSerializer().Serialize(new NodeDataViewModel()
                    {
                        lat = n.lat,
                        lon = n.lon,
                        type = n.type,
                        title = n.title,
                        nodeId = n.nodeId,
                        lineColor = n.lineColor
                    });
                }
                androidNodes += ",\r\n\n";
            }
            i = 0;
            foreach (var w in _waitsGenerated)
            {
                androidNodes += new JavaScriptSerializer().Serialize(w);
                i++;
                if (i < _waitsGenerated.Count)
                {
                    androidNodes += ",\r\n\n";
                }
            }
            androidNodes += "]";

            File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\Android\\nodes.json", androidNodes);

            //just for Debug purpose
            string logJson = "[";
            i = 0;
            foreach (var l in _logs)
            {
                logJson += new JavaScriptSerializer().Serialize(l);
                i++;
                if (i < _logs.Count)
                {
                    logJson += ",\r\n\n";
                }
            }
            logJson += "]";

            //Logs Node ID changes
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\Android\\NodeChangesLog.json", logJson);

            #endregion Parse The Nodes.Json



            #region PARSE EDGEs for Edges.Json

            int nEdgeId = 0;
            bool isStart = true;

            int startBus = 0, endBus = 0, startTram = 0, endTram = 0, startRail = 0, endRail = 0;

            //PathData lastPath = new PathData();
            foreach (var p in currPathDatas)
            {
                nEdgeId++;

                #region WillDelete
                //if (p.PathId == 831)
                //{
                //    MessageBox.Show("Debug : " + p.PathId);
                //}

                //NodeData nd = currCollection.FirstOrDefault(ng => ng.NodeId == p.FromNodeId);
                //NodeData ndTo = currCollection.FirstOrDefault(ng => ng.NodeId == p.ToNodeId);

                //var logFrom = _logs.FirstOrDefault(l => l.NodeNewId == p.FromNodeId);
                //var logTo = _logs.FirstOrDefault(l => l.NodeNewId == p.ToNodeId);
                //if (logFrom != null && logTo != null)
                //{
                //    nd = currCollection.FirstOrDefault(ng => ng.NodeId == logFrom.NodeOriId);
                //    ndTo = currCollection.FirstOrDefault(ng => ng.NodeId == logTo.NodeOriId);
                //}

                //else
                //{
                //    nd = currCollection.FirstOrDefault(ng => ng.NodeId == p.FromNodeId);
                //    ndTo = currCollection.FirstOrDefault(ng => ng.NodeId == p.ToNodeId);
                //}

                //if (nd != null && ndTo != null)
                //{
                #endregion

                string pType = p.TypeList[p.TypeList.Count - 1];
                if (pType.ToLower() != "walk")/*&& pType.ToLower() != "poi"
                        && pType.ToLower() != "mrt" && pType.ToLower() != "busint")*/
                {
                    GenerateTransMapOverLays(nEdgeId, p, pType);
                }
                else
                {
                    NodeData nd = currCollection.FirstOrDefault(ng => ng.NodeId == p.FromNodeId);
                    NodeData ndTo = currCollection.FirstOrDefault(ng => ng.NodeId == p.ToNodeId);

                    double rangeInMeters = MyGlobal.GetDistanceBetweenPoints(nd.Lat, nd.Long, ndTo.Lat, ndTo.Long);
                    double speed = MyGlobal.GetTheSpeed("WALK");

                    //Calculate Estimated Time
                    double estimatedTime = MyGlobal.GetEstimatedTime(speed, rangeInMeters);

                    WalkEdgeViewModel we = new WalkEdgeViewModel();
                    we.edgeId = p.PathId; //nEdgeId;
                    we.time = estimatedTime;
                    we.fromNode = p.FromNodeId;
                    we.toNode = p.ToNodeId;
                    we.type = "WALK";
                    we.biDirectional = true;

                    _edgesGenerated.Add(we);
                }

                //}

            }

            //Regenerating EdgeId Waiting Edge only
            nEdgeId++;
            foreach (var ew in _tmpEdgeWaitsGenerated)
            {
                var waitEdge = new WaitEdgeViewModel()
                {
                    edgeId = nEdgeId,
                    fromNode = ew.fromNode,
                    toNode = ew.toNode,
                    type = ew.type,
                    time = ew.time,
                    biDirectional = ew.biDirectional,
                    lineColor = ew.lineColor
                };
                _edgeWaitsGenerated.Add(waitEdge);
                nEdgeId++;
            }

            //Generate Raw edges.json from Drawing
            string edgeJson = "[";
            int ej = 0;
            foreach (var e in _edgesGenerated)
            {
                edgeJson += new JavaScriptSerializer().Serialize(e);
                edgeJson += ",\r\n\n";
            }
            foreach (var e in _edgesRawGenerated)
            {
                edgeJson += new JavaScriptSerializer().Serialize(e);
                ej++;
                if (ej < _edgesRawGenerated.Count)
                {
                    edgeJson += ",\r\n\n";
                }
            }
            edgeJson += "]";
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\edges.json", edgeJson);

            //Generate Android edges.json
            edgeJson = "[";
            ej = 0;
            foreach (var e in _edgesGenerated)
            {
                edgeJson += new JavaScriptSerializer().Serialize(e);
                edgeJson += ",\r\n\n";
            }
            foreach (var e in _edgesRawGenerated)
            {
                //Switching NodeId
                var logFrom = _logs.FirstOrDefault(l => l.NodeOriId == e.fromNode);
                var logTo = _logs.FirstOrDefault(l => l.NodeOriId == e.toNode);
                if (logFrom != null && logTo != null)
                {
                    e.fromNode = logFrom.NodeNewId;
                    e.toNode = logTo.NodeNewId;
                }

                edgeJson += new JavaScriptSerializer().Serialize(e);
                edgeJson += ",\r\n\n";
            }
            foreach (var e in _edgeWaitsGenerated)
            {
                edgeJson += new JavaScriptSerializer().Serialize(e);
                ej++;
                if (ej < _edgeWaitsGenerated.Count)
                {
                    edgeJson += ",\r\n\n";
                }
            }
            edgeJson += "]";
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\Android\\edges.json", edgeJson);

            #endregion


            //To Refresh in case want to Re-Create
            _nodesGenerated = new Collection<NodeDataViewModel>();
            _waitsGenerated = new Collection<WaitNodeViewModel>();

            _edgesGenerated = new Collection<WalkEdgeViewModel>();
            _transGenerated = new Collection<TransportationEdgeViewModel>();
            _edgeWaitsGenerated = new Collection<WaitEdgeViewModel>();

            _logs = new Collection<LogNodeUpdated>();
            maxId = 0;
            //End

        }

        /// <summary>
        /// Generates the trans map over lays.
        /// </summary>
        /// <param name="edgeId">The edge identifier.</param>
        /// <param name="parPathData">The par path data.</param>
        /// <param name="edgeType">Type of the edge.</param>
        private void GenerateTransMapOverLays(int edgeId, PathData parPathData, string edgeType)
        {
            EdgeRawViewModel tr = new EdgeRawViewModel();
            tr.edgeId = edgeId;

            tr.biDirectional = parPathData.TypeList.Contains("CABLE") ? true : false;

            //Same with Cableway
            tr.biDirectional = parPathData.TypeList.Contains("CABLEWAY") ? true : false;

            var logFrom = _logs.FirstOrDefault(l => l.NodeNewId == parPathData.FromNodeId);
            var logTo = _logs.FirstOrDefault(l => l.NodeNewId == parPathData.ToNodeId);
            if (logFrom != null && logTo != null)
            {
                tr.fromNode = logFrom.NodeOriId;
                tr.toNode = logTo.NodeOriId;
            }
            else
            {
                tr.fromNode = parPathData.FromNodeId;
                tr.toNode = parPathData.ToNodeId;
            }


            tr.type = edgeType.ToUpper();

            //int nRnodeCount = parPathData.routeNodes != null ? parPathData.routeNodes.Count : 0;
            if (parPathData.mapOverLays == null)
            {
                var nodes =
                    currCollection.Where(cc => cc.NodeId >= parPathData.FromNodeId && cc.NodeId <= parPathData.ToNodeId)
                        .OrderBy(cc => cc.NodeId);
                if (nodes.Any())
                {
                    double[][] mapLays = new double[nodes.Count()][];
                    for (int i = 0; i < mapLays.Length; i++)
                    {
                        mapLays[i] = new double[2];
                    }
                    int x = 0;
                    foreach (var n in nodes)
                    {
                        mapLays[x][0] = n.Lat;
                        mapLays[x][1] = n.Long;
                        x++;
                    }
                    parPathData.mapOverLays = mapLays;
                }
            }
            else
            {
                if (parPathData.mapOverLays.Any())
                {
                    double[][] mapLays = new double[parPathData.mapOverLays.Count()][];
                    double sumTimeNeeded = 0.0;

                    for (int i = 0; i < mapLays.Length; i++)
                    {
                        mapLays[i] = new double[2];
                    }
                    for (int i = 0; i < mapLays.Length; i++)
                    {
                        mapLays[i][0] = parPathData.mapOverLays[i][0];
                        mapLays[i][1] = parPathData.mapOverLays[i][1];

                        //Calculate Estimated Time
                        if (i > 0)
                        {
                            double rangeInMeters = MyGlobal.GetDistanceBetweenPoints(mapLays[i - 1][0],
                                mapLays[i - 1][1],
                                mapLays[i][0], mapLays[i][1]);
                            double speed = MyGlobal.GetTheSpeed(edgeType);

                            double estimatedTime = MyGlobal.GetEstimatedTime(speed, rangeInMeters);

                            //Cumulative Values
                            sumTimeNeeded += estimatedTime;
                        }

                    }
                    tr.mapOverlays = mapLays;
                    tr.time = sumTimeNeeded;
                }
            }

            tr.lineColor = parPathData.LineColor;

            _edgesRawGenerated.Add(tr);
        }


        /// <summary>
        /// Changes the node logs.
        /// </summary>
        /// <param name="nodeOriId">The node ori identifier.</param>
        /// <param name="nodeNewId">The node new identifier.</param>
        /// <param name="nodeType">Type of the node.</param>
        private void ChangeNodeLogs(int nodeOriId, int nodeNewId, string nodeType)
        {
            LogNodeUpdated newLog = new LogNodeUpdated()
            {
                NodeOriId = nodeOriId,
                NodeNewId = nodeNewId,
                NodeType = nodeType.ToUpper()
            };
            _logs.Add(newLog);
        }


        private string _jsonResult = "";

        #region Parsing For Android Format

        ///// <summary>
        ///// Does the parse android.
        ///// </summary>
        //private void DoParseAndroid()
        //{
        //    #region Parse Nodes for Nodes.Json

        //    int maxId = 0;
        //    //Add to View Model
        //    foreach (var node in currCollection)
        //    {
        //        NodeDataViewModel v = new NodeDataViewModel();
        //        v.lat = node.Lat;
        //        v.lon = node.Long;
        //        v.type = node.Type.ToUpper();
        //        v.title = node.Title;
        //        v.nodeId = node.NodeId;

        //        _nodesGenerated.Add(v);
        //    }

        //    maxId = _nodesGenerated.Max(n => n.nodeId);
        //    //MessageBox.Show("Max Node Id is " + maxId.ToString());

        //    #region For Bus Nodes

        //    //Get the BusNodeIds
        //    var busNodes = _nodesGenerated.Where(n => n.type.ToLower() == "bus").OrderBy(n => n.nodeId).ToList();
        //    if (busNodes.Any())
        //    {
        //        //Insert the Waiting Nodes

        //        foreach (NodeDataViewModel t in busNodes)
        //        {
        //            int currId = t.nodeId;
        //            maxId++;
        //            var wait = new WaitNodeViewModel()
        //            {
        //                lat = t.lat,
        //                lon = t.lon,
        //                type = "WAITING",
        //                title = "",
        //                lineColor = "",
        //                category = "BUS",
        //                nodeId = t.nodeId
        //            };
        //            _waitsGenerated.Add(wait);

        //            _nodesGenerated.Where(n => n.nodeId == currId).ToList().ForEach(n => n.nodeId = maxId);

        //            ChangeNodeLogs(currId, maxId, "BUS");
        //        }
        //    }

        //    #endregion

        //    #region For Tram Nodes

        //    //Get the TramNodeIds
        //    var tramNodes = _nodesGenerated.Where(n => n.type.ToLower() == "tram").OrderBy(n => n.nodeId).ToList();
        //    if (tramNodes.Any())
        //    {
        //        //Insert the Waiting Nodes
        //        foreach (NodeDataViewModel t in tramNodes)
        //        {
        //            int currId = t.nodeId;
        //            maxId++;
        //            var wait = new WaitNodeViewModel()
        //            {
        //                lat = t.lat,
        //                lon = t.lon,
        //                type = "WAITING",
        //                title = "",
        //                lineColor = "",
        //                category = "TRAM",
        //                nodeId = t.nodeId
        //            };
        //            _waitsGenerated.Add(wait);

        //            _nodesGenerated.Where(n => n.nodeId == currId).ToList().ForEach(n => n.nodeId = maxId);

        //            ChangeNodeLogs(currId, maxId, "TRAM");
        //        }
        //    }

        //    #endregion

        //    #region For Monorail Nodes

        //    //Get the MonorailNodeIds
        //    var railNodes = _nodesGenerated.Where(n => n.type.ToLower() == "rail").OrderBy(n => n.nodeId).ToList();
        //    if (railNodes.Any())
        //    {
        //        //Insert the Waiting Nodes
        //        foreach (NodeDataViewModel t in railNodes)
        //        {
        //            int currId = t.nodeId;
        //            maxId++;
        //            var wait = new WaitNodeViewModel()
        //            {
        //                lat = t.lat,
        //                lon = t.lon,
        //                type = "WAITING",
        //                title = "",
        //                lineColor = "",
        //                category = "RAIL",
        //                nodeId = t.nodeId
        //            };
        //            _waitsGenerated.Add(wait);

        //            _nodesGenerated.Where(n => n.nodeId == currId).ToList().ForEach(n => n.nodeId = maxId);

        //            ChangeNodeLogs(currId, maxId, "RAIL");
        //        }
        //    }

        //    #endregion

        //    //View Result
        //    string jsonNodes = new JavaScriptSerializer().Serialize(_nodesGenerated);
        //    if (_waitsGenerated.Any()) jsonNodes += new JavaScriptSerializer().Serialize(_waitsGenerated);
        //    jsonNodes = jsonNodes.Replace("][", ",");

        //    File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\Android\\nodes.json", jsonNodes);

        //    #endregion Parse The Nodes.Json


        //    #region Parse Edge for Edges.Json

        //    int nEdgeId = 0;
        //    bool isStart = true;

        //    int startBus = 0, endBus = 0, startTram = 0, endTram = 0, startRail = 0, endRail = 0;

        //    PathData lastPath = new PathData();

        //    foreach (var pathData in currPathDatas)
        //    {
        //        nEdgeId++;

        //        NodeData nd = currCollection.FirstOrDefault(ng => ng.NodeId == pathData.FromNodeId);
        //        NodeData ndTo = currCollection.FirstOrDefault(ng => ng.NodeId == pathData.ToNodeId);

        //        if (nd != null && ndTo != null)
        //        {
        //            if (nd.Type.ToLower() != "walk")
        //            {
        //                switch (nd.Type.ToLower())
        //                {
        //                    case "bus":
        //                        if (startBus == 0)
        //                        {
        //                            startBus = nd.NodeId;
        //                        }
        //                        else
        //                        {
        //                            endBus = nd.NodeId;

        //                            GenerateFieldMapOverlaysAndroid(nEdgeId, pathData, startBus, endBus, nd.Type);

        //                            startBus = endBus;
        //                            nEdgeId++;
        //                        }
        //                        break;
        //                    case "tram":
        //                        if (startTram == 0)
        //                        {
        //                            startTram = nd.NodeId;
        //                        }
        //                        else
        //                        {
        //                            endTram = nd.NodeId;

        //                            GenerateFieldMapOverlaysAndroid(nEdgeId, pathData, startTram, endTram, nd.Type);

        //                            startTram = endTram;
        //                            nEdgeId++;
        //                        }

        //                        break;
        //                    case "rail":
        //                        if (startRail == 0)
        //                        {
        //                            startRail = nd.NodeId;
        //                        }
        //                        else
        //                        {
        //                            endRail = nd.NodeId;

        //                            GenerateFieldMapOverlaysAndroid(nEdgeId, pathData, startRail, endRail, nd.Type);

        //                            startRail = endRail;
        //                            nEdgeId++;
        //                        }

        //                        break;
        //                    default:
        //                        break;
        //                }

        //            }

        //            double rangeInMeters = MyGlobal.GetDistanceBetweenPoints(nd.Lat, nd.Long, ndTo.Lat, ndTo.Long);
        //            double speed = MyGlobal.GetTheSpeed("WALK");

        //            //Calculate Estimated Time
        //            double estimatedTime = MyGlobal.GetEstimatedTime(speed, rangeInMeters);

        //            WalkEdgeViewModel we = new WalkEdgeViewModel();
        //            we.edgeId = nEdgeId;
        //            we.time = estimatedTime;
        //            we.fromNode = pathData.FromNodeId;
        //            we.toNode = pathData.ToNodeId;
        //            we.type = "WALK";
        //            we.biDirectional = true;

        //            _edgesGenerated.Add(we);
        //        }

        //        lastPath = pathData;
        //    }

        //    //Tmp for Debugging only
        //    nEdgeId++;
        //    NodeData ndEnd = currCollection.FirstOrDefault(ng => ng.NodeId == lastPath.ToNodeId);
        //    if (ndEnd != null)
        //    {
        //        switch (ndEnd.Type.ToLower())
        //        {
        //            case "bus":
        //                if (startBus == 0)
        //                {
        //                    startBus = ndEnd.NodeId;
        //                }
        //                endBus = ndEnd.NodeId;
        //                GenerateFieldMapOverlaysAndroid(nEdgeId, lastPath, startBus, endBus, ndEnd.Type);
        //                //startBus = endBus;
        //                break;
        //            case "tram":
        //                if (startTram == 0)
        //                {
        //                    startTram = ndEnd.NodeId;
        //                }
        //                endTram = ndEnd.NodeId;
        //                GenerateFieldMapOverlaysAndroid(nEdgeId, lastPath, startTram, endTram, ndEnd.Type);
        //                //startTram = endTram;
        //                break;
        //            case "rail":
        //                if (startRail == 0)
        //                {
        //                    startRail = ndEnd.NodeId;
        //                }
        //                endRail = ndEnd.NodeId;
        //                GenerateFieldMapOverlaysAndroid(nEdgeId, lastPath, startRail, endRail, ndEnd.Type);
        //                //startRail = endRail;
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    //Adding Waiting Edges
        //    int nodeOriId = 0, nodeId = 0;
        //    var waitNodes = _waitsGenerated.ToList();
        //    foreach (var w in waitNodes)
        //    {
        //        var log = _logs.FirstOrDefault(l => l.NodeOriId == w.nodeId);
        //        if (log != null)
        //        {
        //            nodeOriId = w.nodeId;
        //            nodeId = log.NodeNewId;

        //            nEdgeId++;
        //            WalkEdgeViewModel curr = new WalkEdgeViewModel()
        //            {
        //                edgeId = nEdgeId,
        //                time = 0,
        //                fromNode = nodeId,
        //                toNode = nodeOriId,
        //                type = "WAIT",
        //                biDirectional = false
        //            };
        //            _edgeWaitsGenerated.Add(curr);

        //            nEdgeId++;
        //            curr = new WalkEdgeViewModel()
        //            {
        //                edgeId = nEdgeId,
        //                time = 600,
        //                fromNode = nodeOriId,
        //                toNode = nodeId,
        //                type = "WAIT",
        //                biDirectional = false
        //            };
        //            _edgeWaitsGenerated.Add(curr);
        //        }
        //    }

        //    _edgeResult = new JavaScriptSerializer().Serialize(_edgesGenerated);
        //    if (_transAndroidGenerated.Any()) _edgeResult += new JavaScriptSerializer().Serialize(_transAndroidGenerated);
        //    if (_edgeWaitsGenerated.Any()) _edgeResult += new JavaScriptSerializer().Serialize(_edgeWaitsGenerated);
        //    _edgeResult = _edgeResult.Replace("][", ",");
        //    File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\Android\\edges.json", _edgeResult);

        //    #endregion

        //}

        //private void GenerateFieldMapOverlaysAndroid(int edgeId, PathData parPathData, int startNode, int endNode, string pathType)
        //{
        //    string nodesLatLon = "";

        //    //Using the Node Log to get Current Updated NodeId
        //    int sNode = 0, eNode = 0;
        //    var log = _logs.FirstOrDefault(l => l.NodeOriId == startNode);
        //    if (log != null)
        //        sNode = log.NodeNewId;

        //    log = _logs.FirstOrDefault(l => l.NodeOriId == endNode);
        //    if (log != null)
        //        eNode = log.NodeNewId;

        //    //Calculate Estimated Time
        //    NodeData nd = currCollection.FirstOrDefault(ng => ng.NodeId == startNode);
        //    NodeData ndTo = currCollection.FirstOrDefault(ng => ng.NodeId == endNode);
        //    double rangeInMeters = MyGlobal.GetDistanceBetweenPoints(nd.Lat, nd.Long, ndTo.Lat, ndTo.Long);
        //    double speed = MyGlobal.GetTheSpeed("WALK");

        //    double estimatedTime = MyGlobal.GetEstimatedTime(speed, rangeInMeters);

        //    var details = currCollection.Where(cc => cc.NodeId >= startNode && cc.NodeId <= endNode).ToList();
        //    TransportationEdgeAndroidViewModel tr = new TransportationEdgeAndroidViewModel();
        //    tr.edgeId = edgeId;
        //    tr.fromNode = sNode;
        //    tr.toNode = eNode;
        //    tr.type = pathType.ToUpper();
        //    tr.time = estimatedTime;
        //    tr.mapOverlays = "";

        //    nodesLatLon = "[";
        //    foreach (var d in details)
        //    {
        //        nodesLatLon += "[" + d.Lat.ToString() + "," + d.Long.ToString() + "],";
        //    }
        //    nodesLatLon = nodesLatLon.Substring(0, nodesLatLon.Length - 1);
        //    nodesLatLon += "]";

        //    tr.mapOverlays = nodesLatLon;
        //    tr.lineColor = "";
        //    _transAndroidGenerated.Add(tr);
        //}

        #endregion


        /// <summary>
        /// Does the load json files.
        /// Modified on May 06th, 2015 : Loading from Raw Json (Android's format without Waiting Nodes and Edges)
        /// </summary>
        private void DoLoadJsonFiles()
        {
            //1. Loading Json Nodes Only
            DoLoadNodesRawJson();

            //2. Loading Edges Only
            DoLoadEdgeRawJson();

            //3. Draw Nodes based on currCollection
            DrawNodesFromJson(currCollection);
        }

        private void DoLoadNodesRawJson()
        {
            List<NodeDataViewModel> nodeList = new List<NodeDataViewModel>();
            //1. Find file nodes.json
            using (StreamReader r = new StreamReader(Directory.GetCurrentDirectory() + "\\output\\nodes.json"))
            {
                string json = r.ReadToEnd();
                nodeList = new JavaScriptSerializer().Deserialize<List<NodeDataViewModel>>(json);
            }
            //MessageBox.Show(new JavaScriptSerializer().Serialize(nodeList));

            //2. Restore the Original Id to currCollection
            currCollection = new Collection<NodeData>();

            foreach (var n in nodeList)
            {
                Location l = new Location()
                {
                    Latitude = n.lat,
                    Longitude = n.lon
                };
                Point nodePoint = new Point();
                myMap.TryLocationToViewportPoint(l, out nodePoint);

                var log = _logs.FirstOrDefault(lo => lo.NodeNewId == n.nodeId);

                NodeData nd = new NodeData();
                nd.Lat = l.Latitude;
                nd.Long = l.Longitude;
                nd.PointX = (int)nodePoint.X;
                nd.PointY = (int)nodePoint.Y;
                nd.Type = n.type;
                nd.NodeId = log != null ? log.NodeOriId : n.nodeId;
                nd.Title = n.title;
                nd.IsNewNode = false;

                currCollection.Add(nd);
            }

            LastNodeId = currCollection.Max(cc => cc.NodeId);
        }

        private void DoLoadEdgeRawJson()
        {
            //1. Load from Edges.json
            List<EdgeRawViewModel> pathList = new List<EdgeRawViewModel>();
            using (StreamReader r = new StreamReader(Directory.GetCurrentDirectory() + "\\output\\edges.json"))
            {
                string json = r.ReadToEnd();
                json = json.Replace("\r", "").Replace("\n", "");
                pathList = new JavaScriptSerializer().Deserialize<List<EdgeRawViewModel>>(json);
            }

            //2. Restore view to currPathDatas
            currPathDatas = new Collection<PathData>();

            //var filteredPaths = pathList.Where(p => p.type != "BUS" && p.type != "TRAM" && p.type != "RAIL").ToList();

            if (pathList == null)
                return;

            Point startPoint = new Point();
            Point endPoint = new Point();
            foreach (var p in pathList)     //filteredPaths)
            {
                List<string> typeList = new List<string>();
                if (p.type != null) typeList.Add(p.type);

                PathData pd = new PathData()
                {
                    PathId = p.edgeId,
                    FromNodeId = p.fromNode,
                    ToNodeId = p.toNode,
                    TypeList = typeList,
                    BiDirectional = p.biDirectional,
                    LineColor = p.lineColor
                };

                if (p.type != null && p.type.ToUpper() == "WALK")
                {
                    var stNode = currCollection.FirstOrDefault(cc => cc.NodeId == p.fromNode);
                    var endNode = currCollection.FirstOrDefault(cc => cc.NodeId == p.toNode);
                    if (stNode != null && endNode != null)
                    {
                        startPoint = myMap.LocationToViewportPoint(new Location()
                        {
                            Latitude = stNode.Lat,
                            Longitude = stNode.Long
                        });
                        endPoint = myMap.LocationToViewportPoint(new Location()
                        {
                            Latitude = endNode.Lat,
                            Longitude = endNode.Long
                        });
                    }
                    //Drawing Paths Here
                    AddPathBetween(startPoint, endPoint, typeList);
                }
                else
                {
                    pd.LineColor = p.lineColor;

                    double[][] mapLays = p.mapOverlays;
                    if (mapLays != null)
                    {
                        pd.mapOverLays = mapLays;

                        Location stLocation = new Location();
                        Location endLocation = new Location();

                        bool isStartNode = true;

                        Collection<NodeData> rNodes = new Collection<NodeData>();

                        for (int i = 0; i < mapLays.Length; i++)
                        {
                            if (isStartNode)
                            {
                                stLocation = new Location()
                                {
                                    Latitude = mapLays[i][0],
                                    Longitude = mapLays[i][1]
                                };
                                isStartNode = false;
                            }
                            else
                            {
                                endLocation = stLocation;
                                stLocation = new Location()
                                {
                                    Latitude = mapLays[i][0],
                                    Longitude = mapLays[i][1]
                                };

                                startPoint = myMap.LocationToViewportPoint(stLocation);
                                endPoint = myMap.LocationToViewportPoint(endLocation);

                                //Drawing Transportation Paths Here
                                AddPathBetween(startPoint, endPoint, typeList);

                                var routenode =
                                    currCollection.FirstOrDefault(
                                        cc => cc.Lat == mapLays[i][0] && cc.Long == mapLays[i][1]);
                                if (routenode != null)
                                {
                                    rNodes.Add(routenode);
                                }
                            }
                        }
                        pd.routeNodes = rNodes;
                    }
                }

                //Add to current PathData List
                currPathDatas.Add(pd);

            }

            LastPathId = currPathDatas.Count > 0 ? currPathDatas.Max(p => p.PathId) : 1;

            //Temp for Debug
            //string jsonRawEdge = new JavaScriptSerializer().Serialize(currPathDatas);
            //File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\currPathDatas.json", jsonRawEdge);

            string edgeJson = "[";
            int ej = 0;
            foreach (var e in currPathDatas)
            {
                edgeJson += new JavaScriptSerializer().Serialize(e);
                ej++;
                if (ej < currPathDatas.Count)
                {
                    edgeJson += ",\r\n";
                }
            }
            edgeJson += "]";
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\output\\currPathDatas.json", edgeJson);
        }


        private void DrawNodesFromJson(Collection<NodeData> parNodeDatas)
        {
            foreach (var n in parNodeDatas)
            {
                Location l = new Location()
                {
                    Latitude = n.Lat,
                    Longitude = n.Long
                };
                currPoint = new Point()
                {
                    X = n.PointX,
                    Y = n.PointY
                };
                Point icoPoint = currPoint;
                icoPoint.Y -= (IcoSize / 2) + 10;
                Location pinLocation = myMap.ViewportPointToLocation(icoPoint);

                AddNodeIconToMap(l, "node", IcoNodeSize, n.NodeId);
                AddNodeIconToMap(pinLocation, n.Type, IcoSize, n.NodeId);

                lastPoint = currPoint;
                lastNodeData = n;
                if (lastPoint.X != 0.0)
                    _isStartNode = false;
            }
        }



    }
}
