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
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ConstructionToolWithOptions_tf
{
    internal class BufferedLineTool : MapTool
    {
        public BufferedLineTool()
        {
            IsSketchTool = true;
            UseSnapping = true;
            // Select the type of construction tool you wish to implement.  
            // Make sure that the tool is correctly registered with the correct component category type in the daml 
            //SketchType = SketchGeometryType.Point;
             SketchType = SketchGeometryType.Line;
            //SketchType = SketchGeometryType.OpenLasso;
            // SketchType = SketchGeometryType.Polygon;
        }

        //private double BufferDistance = 25.0;
        private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);

        private double BufferDistance
        {
            get
            {
                if (ToolOptions == null)
                    return BufferedLineToolOptionsViewModel.DefaultBuffer;

                return ToolOptions.GetProperty(BufferedLineToolOptionsViewModel.BufferOptionName, BufferedLineToolOptionsViewModel.DefaultBuffer);
            }
        }
        private double BufferRatio
        {
            get
            {
                if (ToolOptions == null)
                    return BufferedLineToolOptionsViewModel.DefaultBufferRatio;

                return ToolOptions.GetProperty(BufferedLineToolOptionsViewModel.BufferOptionNameRatio, BufferedLineToolOptionsViewModel.DefaultBufferRatio);
            }
        }

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
        /// 
        private Task<Geometry> ConstructBuffers(Polyline SketchPolyline, Double BufferDistance, Double BufferRatio)
        {
            return QueuedTask.Run( () =>
            {
                List<MapPoint> pts = SketchPolyline.Points.ToList();

                double brpercent = BufferRatio / 100; //turn input buffer ratio number into percent

                Geometry bufferedGeometry1a = GeometryEngine.Instance.Buffer(pts, BufferDistance); //large circle at each vertex
                Geometry bufferedGeometry1b = GeometryEngine.Instance.Buffer(pts, BufferDistance* brpercent); //smaller circle at each vertex

                Geometry bufferedGeometry2 = GeometryEngine.Instance.Buffer(SketchPolyline, BufferDistance* brpercent); //buffered line

                Geometry bufferedGeometry1 = GeometryEngine.Instance.Union(bufferedGeometry1a, bufferedGeometry2); //union large circle and buffred line...
                Geometry bufferedGeometry = GeometryEngine.Instance.SymmetricDifference(bufferedGeometry1, bufferedGeometry1b); //... and erase small circle from result

                return bufferedGeometry;
            });
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {

            if (CurrentTemplate == null || geometry == null)
                return await Task.FromResult(false);

            Polyline polyline = geometry as ArcGIS.Core.Geometry.Polyline;
           

            //create the buffered geometry. change this to an async method that we can await
            //Geometry bufferedGeometry = GeometryEngine.Instance.Buffer(pts, BufferDistance);
            Geometry bufferedGeometry = await ConstructBuffers(polyline, BufferDistance, BufferRatio);



            // Create an edit operation
            var createOperation = new EditOperation();
            createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
            createOperation.SelectNewFeatures = true;

            // Queue feature creation
            createOperation.Create(CurrentTemplate, bufferedGeometry);

            // Execute the operation
            return await createOperation.ExecuteAsync();
        }
    }
}
