using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using System.Diagnostics;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ConstructionToolWithOptions_tf
{
    internal class ConstructionTool1 : MapTool
    {
        public ConstructionTool1()
        {
            IsSketchTool = true;
            UseSnapping = true;
            // Select the type of construction tool you wish to implement.  
            // Make sure that the tool is correctly registered with the correct component category type in the daml 
            //SketchType = SketchGeometryType.Point;
            SketchType = SketchGeometryType.Line;
            //SketchOutputMode = SketchOutputMode.Map;
            // SketchType = SketchGeometryType.Polygon;
        }

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
        /// 
        private Task<List<long>> GetRelateObjectIDs(Geometry geometry)
        {
            return QueuedTask.Run(() =>
            {
                var polygonLayers = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == esriGeometryType.esriGeometryPolygon);
                var relateObjectIDList = new List<long>();
                foreach (FeatureLayer polygonLayer in polygonLayers)
                {
                    using (RowCursor searchCursor = polygonLayer.Search())
                    {
                        while (searchCursor.MoveNext())
                        {
                            using (Feature feature = (Feature)searchCursor.Current)
                            {
                                //the sketch geometry needs to be projected to the polygon layer spatial reference
                                var sr = polygonLayer.GetSpatialReference();
                                Geometry geometry_prj = (Polyline)GeometryEngine.Instance.Project(geometry, sr);
                                
                                // Process the feature.
                                if (GeometryEngine.Instance.Relate(geometry_prj, feature.GetShape(), "F***T****")) 
                                {
                                    var oid = feature.GetObjectID();
                                    //Debug.WriteLine(feature.GetObjectID().ToString() + "passes test F***T****");
                                    relateObjectIDList.Add(oid);
                                }
                                else
                                {
                                    var oid = feature.GetObjectID();
                                    //Debug.WriteLine(feature.GetObjectID().ToString() + "does NOT pass test F***T****");
                                }
                            }
                        }
                        //Debug.WriteLine("list of featues that pass F***T**** test:");
                        //Debug.WriteLine(string.Join(",", relateObjectIDList.ToArray()));
                    }
                }
                return relateObjectIDList; //calling function can test for exmple that these are two seperate polygon object ID's
            });
        }


        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (CurrentTemplate == null || geometry == null)
                return await Task.FromResult(false);

            //determine get list of polygon layers in maps
            List<long> l = await GetRelateObjectIDs(geometry);

            //l should contain exactly two different numbers
            Debug.WriteLine(l.Count.ToString());
            if (l.Count() == 2)
            {
                if (l[0] != l[1]) //if you only have one polygon layer n the map this is kinda redundant
                {
                    var createOperation = new EditOperation();
                    createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
                    createOperation.SelectNewFeatures = true;
                    var attributes = new Dictionary<string, object>();
                    attributes.Add("SHAPE", geometry);
                    attributes.Add("Name", "Connects polygons with objectids " + string.Join(",", l.ToArray()));
                    createOperation.Create(CurrentTemplate.Layer, attributes);
                    return await createOperation.ExecuteAsync();
                }
                else
                {
                    MessageBox.Show("Polyline feature NOT created...that's the SAME polgon feature...somehow");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Polyline feature NOT created. Because requirement GeometryEngine.Instance.Relate 'F***T****' was not satisfied by the polyline and two polygon features.");
                return false;
            }
        }
    }
}
