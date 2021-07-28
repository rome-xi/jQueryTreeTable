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
    public class jQueryTreeTable : CellType, IReferenceTable
    {
        public override string ToString()
        {
            return "树型表";
        }

        public IEnumerable<LocatedObject<TableCheckInfo>> GetTableInfo(LocationIndicator location)
        {
            var result = new List<LocatedObject<TableCheckInfo>>();
            TableCheckInfo tableInfo = new TableCheckInfo(this.SetBindingTable.TableName);
            var myFieldInfos = this.SetBindingTable.MyFieldInfos;

            string[] columns = new string[myFieldInfos.Length];
            foreach (var s in myFieldInfos)
            {
                columns.Append(s.ShowField);
            }
            tableInfo.AddColumns(columns);
            //Add direct table info
            result.Add(new LocatedObject<TableCheckInfo>(tableInfo, location.AppendProperty("TableName")));
            //Add QueryCondition table info
            if (this.SetBindingTable.QueryCondition is IReferenceTable)
            {
                result.AddRange((this.SetBindingTable.QueryCondition as IReferenceTable).GetTableInfo(location.AppendProperty("QueryCondition")));
            }
            //Add SortCondition table info
            if (this.SetBindingTable.SortCondition is IReferenceTable)
            {
                result.AddRange((this.SetBindingTable.SortCondition as IReferenceTable).GetTableInfo(location.AppendProperty("SortCondition")));
            }
            return result;
        }

        public void RenameTableColumnName(string tableName, string oldName, string newName)
        {
            TableCheckInfo tableInfo = new TableCheckInfo(this.SetBindingTable.TableName);
            if (string.Equals(this.SetBindingTable.TableName, tableName))
            {
                var myFieldInfos = this.SetBindingTable.MyFieldInfos;

                foreach (var s in myFieldInfos)
                {
                    if (string.Equals(s.ShowField, oldName))
                    {
                        s.ShowField = newName;
                    }
                }
            }
            if (this.SetBindingTable.QueryCondition is IReferenceTable)
            {
                (this.SetBindingTable.QueryCondition as IReferenceTable).RenameTableColumnName(tableName, oldName, newName);
            }
            if (this.SetBindingTable.SortCondition is IReferenceTable)
            {
                (this.SetBindingTable.SortCondition as IReferenceTable).RenameTableColumnName(tableName, oldName, newName);
            }
        }

        public void RenameTableName(string oldName, string newName)
        {
            if (string.Equals(this.SetBindingTable.TableName, oldName))
            {
                this.SetBindingTable.TableName = newName;
            }
            if (this.SetBindingTable.QueryCondition is IReferenceTable)
            {
                (this.SetBindingTable.QueryCondition as IReferenceTable).RenameTableName(oldName, newName);
            }
            if (this.SetBindingTable.SortCondition is IReferenceTable)
            {
                (this.SetBindingTable.SortCondition as IReferenceTable).RenameTableName(oldName, newName);
            }
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
            var id = this.CellType.SetBindingTable.ID;
            var relatedParentID = this.CellType.SetBindingTable.RelatedParentID;

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
            columns.Add(id);
            columns.Add(relatedParentID);
            //get table data for preview.
            var tableData = drawingHelper.GetTableDataForPreview(tableName, columns, null, true);

            var data = ReSortTable(tableData, relatedParentID, id);
            if (data != null)
            {
                foreach (var row in data)
                {
                    listView.Items.Add(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(row)));
                }
            }
            Grid grid = new Grid();

            grid.Children.Add(listView);

            return grid;
        }

        private List<Dictionary<string, object>> ReSortTable(List<Dictionary<string, object>> tableData, string relatedParentID, string id)
        {
            var data =new List<Dictionary<string, object>>();
            for (var i = 0; i < tableData.Count; i++)
            {
                if (tableData[i][relatedParentID] == null)
                {
                    data.Add(tableData[i]);
                    addTreeNode(tableData, data, i, relatedParentID, id);
                }
            }
            return data;
        }

        private void addTreeNode(List<Dictionary<string, object>> tableData, List<Dictionary<string, object>> data, int index, string relatedParentID, string id)
        {
            var sign = false;
            for (var i = 0; i < tableData.Count; i++)
            {
                if (string.Equals(tableData[i][relatedParentID], tableData[index][id]))
                {
                    data.Add(tableData[i]);
                    sign = true;
                    addTreeNode(tableData, data, i, relatedParentID, id);
                }
            }
            if (sign == false)
            {
                return;
            }
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
