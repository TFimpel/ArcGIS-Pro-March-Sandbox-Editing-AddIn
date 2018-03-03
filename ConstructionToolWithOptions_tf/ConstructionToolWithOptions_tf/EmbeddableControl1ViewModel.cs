using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using ArcGIS.Desktop.Editing;
using System.Windows.Media;

namespace ConstructionToolWithOptions_tf
{
    internal class EmbeddableControl1ViewModel : EmbeddableControl, IEditingCreateToolControl
    {
        public EmbeddableControl1ViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }

        /// <summary>
        /// Text shown in the control.
        /// </summary>
        private string _text = "Embeddable Control";
        public string Text
        {
            get { return _text; }
            set
            {
                SetProperty(ref _text, value, () => Text);
            }
        }

        #region IEditingCreateToolControl

        /// <summary>
        /// Gets the optional icon that will appear in the active template pane as a button selector.  
        /// If returning null then the Tool's small image that is defined in DAML will be used. 
        /// </summary>
        ImageSource IEditingCreateToolControl.ActiveTemplateSelectorIcon => null;

        private bool _isValid;
        /// <summary>
        /// Set this flag to indicate if too options are valid and ready for saving.
        /// </summary>
        /// <remarks>
        /// When this IEditingCreateToolControl is being displayed in the Template Properties
        /// dialog, calling code will use this property to determine if the current Template
        /// Properties may be saved.
        /// </remarks>
        bool IEditingCreateToolControl.IsValid => _isValid;

        private bool _isDirty;
        /// <summary>
        /// Set this flag when any tool options have been changed.
        /// </summary>
        /// <remaarks>
        /// When this IEditingCreateToolControl is being displayed in the Template Properties
        /// dialog, the calling code will use this property to determine if any changes have
        /// been made.  
        /// </remaarks>
        bool IEditingCreateToolControl.IsDirty => _isDirty;

        /// <summary>
        /// Gets if the contents of this control is auto-Opened in the Active Template Pane when the 
        /// tool is activated.
        /// </summary>
        /// <param name="toolID">the ID of the current tool</param>
        /// <returns>True the active template pane will be opened to the tool's options view.
        /// False, nothing in the active template pane changes when the tool is selected.</returns>
        bool IEditingCreateToolControl.AutoOpenActiveTemplatePane(string toolID)
        {
            return true;
        }

        /// <summary>
        /// Called just before ArcGIS.Desktop.Framework.Controls.EmbeddableControl.OpenAsync
        /// when this IEditingCreateToolControl is being used within the ActiveTemplate pane.
        /// </summary>
        /// <param name="options">tool options obtained from the template for the given toolID</param>
        /// <returns>true if the control is to be displayed in the ActiveTemplate pane.. False otherwise</returns>
        bool IEditingCreateToolControl.InitializeForActiveTemplate(ToolOptions options)
        {
            // assign the current options
            ToolOptions = options;
            // initialize the view
            InitializeOptions();
            return true;    // true <==> do show me in ActiveTemplate;   false <==> don't show me
        }

        /// <summary>
        /// Called just before ArcGIS.Desktop.Framework.Controls.EmbeddableControl.OpenAsync
        /// when this IEditingCreateToolControl is being used within the Template Properties
        /// dialog
        /// </summary>
        /// <param name="options">tool options obtained from the template for the given toolID</param>
        /// <returns>true if the control is to be displayed in Template Properties. False otherwise</returns>
        bool IEditingCreateToolControl.InitializeForTemplateProperties(ToolOptions options)
        {
            return false;     // don't show the options in template properties
        }

        #endregion

        internal const string CircleOptionName = "Radius";
        internal const double DefaultCircleOptionName = 100;

        internal const string CircleOptionNameNumberOfPoints = "NumberOfPoints";
        internal const double DefaultCircleOptionNameNumberOfPoints = 36;



        private ToolOptions ToolOptions { get; set; }
        private void InitializeOptions()
        {
            // no options
            if (ToolOptions == null)
                return;

            // if buffer exists in options, retrieve it
            if (ToolOptions.ContainsKey(CircleOptionName))
                _circle = (double)ToolOptions[CircleOptionName];
            else
            {
                // otherwise assign the default value and add to the ToolOptions dictionary
                _circle = DefaultCircleOptionName;
                ToolOptions.Add(CircleOptionName, _circle);
                // ensure options are notified that changes have been made
                NotifyPropertyChanged(CircleOptionName);
            }

            // if bufferratio exists in options, retrieve it
            if (ToolOptions.ContainsKey(CircleOptionNameNumberOfPoints))
                _circlePoints = (double)ToolOptions[CircleOptionNameNumberOfPoints];
            else
            {
                // otherwise assign the default value and add to the ToolOptions dictionary
                _circlePoints = DefaultCircleOptionNameNumberOfPoints;
                ToolOptions.Add(CircleOptionNameNumberOfPoints, _circlePoints);
                // ensure options are notified that changes have been made
                NotifyPropertyChanged(CircleOptionNameNumberOfPoints);
            }
        }

        // binds in xaml
        private double _circle;
        public double Circle
        {
            get { return _circle; }
            set
            {
                if (SetProperty(ref _circle, value))
                {
                    _isDirty = true;
                    _isValid = true;
                    // add/update the buffer value to the tool options
                    if (!ToolOptions.ContainsKey(CircleOptionName))
                        ToolOptions.Add(CircleOptionName, value);
                    else
                        ToolOptions[CircleOptionName] = value;
                    // ensure options are notified
                    NotifyPropertyChanged(CircleOptionName);
                }
            }
        }


        // binds in xaml
        private double _circlePoints;
        public double CirclePoints
        {
            get { return _circlePoints; }
            set
            {
                if (SetProperty(ref _circlePoints, value))
                {
                    _isDirty = true;
                    _isValid = true;
                    // add/update the buffer value to the tool options
                    if (!ToolOptions.ContainsKey(CircleOptionNameNumberOfPoints))
                        ToolOptions.Add(CircleOptionNameNumberOfPoints, value);
                    else
                        ToolOptions[CircleOptionNameNumberOfPoints] = value;
                    // ensure options are notified
                    NotifyPropertyChanged(CircleOptionNameNumberOfPoints);
                }
            }
        }
    }
}
