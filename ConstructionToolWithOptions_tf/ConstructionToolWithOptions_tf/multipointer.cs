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

namespace ConstructionToolWithOptions_tf
{
    internal class multipointer : MapTool
    {
        public multipointer()
        {
            IsSketchTool = true;
            UseSnapping = true;
            // Select the type of construction tool you wish to implement.  
            // Make sure that the tool is correctly registered with the correct component category type in the daml 
            //SketchType = SketchGeometryType.Multipoint;
            // SketchType = SketchGeometryType.Line;
            // SketchType = SketchGeometryType.Polygon;
            SketchType = SketchGeometryType.Point;
        }

        private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);

        private double Circle
        {
            get
            {
                if (ToolOptions == null)
                {
                    Debug.WriteLine("37");
                    return EmbeddableControl1ViewModel.DefaultCircleOptionName;
                }


                return ToolOptions.GetProperty(EmbeddableControl1ViewModel.CircleOptionName, EmbeddableControl1ViewModel.DefaultCircleOptionName);
            }
        }
        private double CircelNumberOfPoints
        {
            get
            {
                if (ToolOptions == null)
                {
                    Debug.WriteLine("48");
                    return EmbeddableControl1ViewModel.DefaultCircleOptionNameNumberOfPoints;
                }


                return ToolOptions.GetProperty(EmbeddableControl1ViewModel.CircleOptionNameNumberOfPoints, EmbeddableControl1ViewModel.DefaultCircleOptionNameNumberOfPoints);
            }
        }

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (CurrentTemplate == null || geometry == null)
                return Task.FromResult(false);
        
            // Create an edit operation
            var createOperation = new EditOperation();
            createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
            createOperation.SelectNewFeatures = true;

            // Queue feature creation
            createOperation.Create(CurrentTemplate, geometry); //this is the centerpoint

            var anglebetweenpoints_degrees = 360 / CircelNumberOfPoints;
            var anglebetweenpoints_radians = Math.PI * anglebetweenpoints_degrees / 180.0;
            var radius = Circle;
            for (int i = 0; i < CircelNumberOfPoints; i++)
            {
                var xoffset = radius  * Math.Cos( (i*anglebetweenpoints_radians));
                var yoffset = radius  * Math.Sin( (i*anglebetweenpoints_radians));
                Geometry geom = GeometryEngine.Instance.Move(geometry, xoffset, yoffset);

                var attributes = new Dictionary<string, object>();
                attributes.Add("SHAPE", geom);
                attributes.Add("Name", (i * anglebetweenpoints_degrees).ToString());
                createOperation.Create(CurrentTemplate.Layer, attributes);
            }

            // Execute the operation
            return createOperation.ExecuteAsync();
        }
    }
}
