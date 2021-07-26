using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Plugin;
using GrapeCity.Forguncy.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using jQueryTreeTable.Control;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;

namespace jQueryTreeTable
{
    [Designer("jQueryTreeTable.jQueryTreeTableDesigner, jQueryTreeTable")]
    [Icon("pack://application:,,,/jQueryTreeTable;component/Resources/Icon.png")]
    public class jQueryTreeTable : CellType
    {
        public override string ToString()
        {
            return "树型表";
        }
        public jQueryTreeTable()
        {
        }
        [DisplayName("设置绑定表格参数")]
        public TableInfo SetBindingTable
        {
            get; set;
        }


    }
    public class jQueryTreeTableDesigner : CellTypeDesigner<jQueryTreeTable>
    {
        public override EditorSetting GetEditorSetting(PropertyDescriptor property, IBuilderContext builderContext)
        {
            if (property.Name == nameof(jQueryTreeTable.SetBindingTable))
            {
                return new HyperlinkEditorSetting(new SetBindingTableCommand(builderContext));
            }
            return base.GetEditorSetting(property, builderContext);
        }

        public override FrameworkElement GetDrawingControl(ICellInfo cellInfo, IDrawingHelper drawingHelper)
        {
            ListView listView= new ListView();
            var tableName = this.CellType.SetBindingTable.TableName;
            var myFieldInfos = this.CellType.SetBindingTable.MyFieldInfos;
            List<string> columns = new List<string>();
            foreach(var s in myFieldInfos) {
                columns.Add(s.ShowField);
            }

            GridView gridView = new GridView();

            for (int c = 0; c < columns.Count; c++)
            {
                GridViewColumn title = new GridViewColumn
                {
                    Header = columns[c],
                    DisplayMemberBinding = new Binding(columns[c]),
                    Width = 120
                };
                gridView.Columns.Add(title);
            }

            listView.View = gridView;

            //get table data for preview.
            var tableData = drawingHelper.GetTableDataForPreview(tableName, columns, null, true);
            if (tableData != null)
            {
                foreach (var row in tableData)
                {
                    listView.Items.Add(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(row)));
                }
            }
            Grid grid = new Grid();

            grid.Children.Add(listView);

            return grid;
        }

    }

    public class ItemModel 
    {
        public string ColumnName { get; set; }

        public object ColumnValue { get; set; }
    }

    public class ItemViewModel : PropertyChangedObjectBase 
    {
        
    }

    public class SetBindingTableCommand : ICommand
    {
        private Window window;
        private IEditorSettingsDataContext dataContext;
        private IBuilderContext BuilderContext { get; set; }

        private SetBindingTable control;

        public SetBindingTableCommand(IBuilderContext builderContext)
        {
            this.BuilderContext = builderContext;
        }
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            dataContext = parameter as IEditorSettingsDataContext;

            control = new SetBindingTable(BuilderContext);
            control.ViewModel.Model = dataContext?.Value as TableInfo;
            var buttonControl = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 5, 10) };
            var okButton = new Button() { Content = "OK", Width = 80 };
            okButton.Click += OkButton_Click;
            var cancelButton = new Button() { Content = "Cancel", Width = 80, Margin = new Thickness(8, 0, 0, 0) };
            cancelButton.Click += CancelButton_Click;
            buttonControl.Children.Add(okButton);
            buttonControl.Children.Add(cancelButton);

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            grid.Children.Add(control);
            grid.Children.Add(buttonControl);
            Grid.SetRow(control, 0);
            Grid.SetRow(buttonControl, 1);

            window = new Window();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "设置绑定表格参数";
            window.Width = 410d;
            window.Height = 630d;
            window.Content = grid;
            window.ShowDialog();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            window.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            dataContext.Value = control.ViewModel.Model;
            window.Close();
        }
    }

    public class MyFieldInfo
    {
        public string ShowField
        {
            get; set;
        }
        public string FieldName
        {
            get; set;
        }
        public MyFieldInfo(string showField, string fieldName)
        {
            ShowField = showField;
            FieldName = fieldName;
        }
        public MyFieldInfo Clone()
        {
            return new MyFieldInfo(ShowField, FieldName);
        }
    }
    public class TableInfo
    {
        public string TableName
        {
            get; set;
        }
        public string ID
        {
            get; set;
        }
        public string RelatedParentID
        {
            get; set;
        }
        public MyFieldInfo[] MyFieldInfos
        {
            get; set;
        }
        public object QueryCondition
        {
            get; set;
        }
        public object SortCondition
        {
            get; set;
        }

        public TableInfo(string tableName, string id, string relatedParentID, MyFieldInfo[] myFieldInfos, object queryCondition, object sortCondition)
        {
            TableName = tableName;
            ID = id;
            RelatedParentID = relatedParentID;
            MyFieldInfos = myFieldInfos;
            QueryCondition = queryCondition;
            SortCondition = sortCondition;
        }
    }

}
