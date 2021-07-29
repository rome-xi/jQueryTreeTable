using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using jQueryTreeTable.Control;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;

namespace jQueryTreeTable
{
    [Designer("jQueryTreeTable.jQueryTreeTableDesigner, jQueryTreeTable")]
    [Icon("pack://application:,,,/jQueryTreeTable;component/Resources/Icon.png")]
    public class jQueryTreeTable : CellType, IReferenceListView
    {
        public jQueryTreeTable()
        {
        }
        [DisplayName("设置绑定表格参数")]
        public TableInfo SetBindingTable
        {
            get; set;
        }

        public IEnumerable<string> GetListViewNames()
        {
            yield return SetBindingTable.TableName;
        }

        public void RenameListviewName(string oldName, string newName)
        {
            if (string.Equals(SetBindingTable.TableName, oldName))
            {
                SetBindingTable.TableName = newName;
            }
        }

        public override string ToString()
        {
            return "树型表";
        }
    }
    public class jQueryTreeTableDesigner : CellTypeDesigner<jQueryTreeTable>
    {
        public override EditorSetting GetEditorSetting(PropertyDescriptor property, IBuilderContext builderContext)
        {
            return property.Name == nameof(jQueryTreeTable.SetBindingTable)
                ? new HyperlinkEditorSetting(new SetBindingTableCommand(builderContext))
                : base.GetEditorSetting(property, builderContext);
        }

        //public override FrameworkElement GetDrawingControl(ICellInfo cellInfo, IDrawingHelper drawingHelper)
        //{
        //    TableInfo tableInfo = this.CellType.SetBindingTable;
        //    ListView listView = new ListView();
        //    var tableName = tableInfo.TableName;
        //    var myFieldInfos = tableInfo.MyFieldInfos;
        //    var id = tableInfo.ID;
        //    var relatedParentID = tableInfo.RelatedParentID;

        //    List<string> columns = new List<string>();
        //    foreach(var s in myFieldInfos) {
        //        columns.Add(s.ShowField);
        //    }
        //    GridView gridView = new GridView();

        //    for (int c = 0; c < columns.Count; c++)
        //    {
        //        GridViewColumn title = new GridViewColumn
        //        {
        //            Header = columns[c],
        //            DisplayMemberBinding = new Binding(columns[c]),
        //            Width = 120
        //        };
        //        gridView.Columns.Add(title);
        //    }
        //    listView.View = gridView;
        //    columns.Add(id);
        //    columns.Add(relatedParentID);
        //    //get table data for preview.
        //    var tableData = drawingHelper.GetTableDataForPreview(tableName, columns, null, true);\
        //    var data = ReSortTable(tableData, relatedParentID, id);
        //    if (data != null)
        //    {
        //        foreach (Dictionary<string, object> row in data)
        //        {
        //            listView.Items.Add(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(row)));
        //        }
        //    }
        //    Grid grid = new Grid();
        //    grid.Children.Add(listView);

        //    return grid;
        //}

        private List<Dictionary<string, object>> ReSortTable(List<Dictionary<string, object>> tableData, string relatedParentID, string id)
        {
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            for (int i = 0; i < tableData.Count; i++)
            {
                if (tableData[i][relatedParentID] == null)
                {
                    data.Add(tableData[i]);
                    AddTreeNode(tableData, data, i, relatedParentID, id);
                }
            }
            return data;
        }

        private void AddTreeNode(List<Dictionary<string, object>> tableData, List<Dictionary<string, object>> data, int index, string relatedParentID, string id)
        {
            bool sign = false;
            for (int i = 0; i < tableData.Count; i++)
            {
                if (string.Equals(tableData[i][relatedParentID], tableData[index][id]))
                {
                    data.Add(tableData[i]);
                    sign = true;
                    AddTreeNode(tableData, data, i, relatedParentID, id);
                }
            }
            if (sign == false)
            {
                return;
            }
        }

    }
    public class SetBindingTableCommand : ICommand
    {
        private Window window;
        private IEditorSettingsDataContext dataContext;
        private IBuilderContext BuilderContext { get; set; }

        private SetBindingTable control;

        public SetBindingTableCommand(IBuilderContext builderContext)
        {
            BuilderContext = builderContext;
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
            StackPanel buttonControl = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 5, 10) };
            Button okButton = new Button() { Content = "确认", Width = 80 };
            okButton.Click += OkButton_Click;
            Button cancelButton = new Button() { Content = "取消", Width = 80, Margin = new Thickness(8, 0, 0, 0) };
            cancelButton.Click += CancelButton_Click;
            buttonControl.Children.Add(okButton);
            buttonControl.Children.Add(cancelButton);

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            grid.Children.Add(control);
            grid.Children.Add(buttonControl);
            Grid.SetRow(control, 0);
            Grid.SetRow(buttonControl, 1);

            window = new Window
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "设置绑定表格参数",
                Width = 410d,
                Height = 630d,
                Content = grid
            };
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
        public TableInfo(string tableName, string id, string relatedParentID, MyFieldInfo[] myFieldInfos)
        {
            TableName = tableName;
            ID = id;
            RelatedParentID = relatedParentID;
            MyFieldInfos = myFieldInfos;
        }
    }

}
