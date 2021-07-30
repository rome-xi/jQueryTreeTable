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
    public class jQueryTreeTable : CellType, IReferenceListView, IReferenceListViewColumn
    {
        public jQueryTreeTable()
        {
        }

        [DisplayName("设置绑定表格参数")]
        public ListViewInfo SetBindingListView
        {
            get; set;
        }

        [DisplayName("设置展开方式")]
        public UnfoldingMethod SetUnfoldingMethod
        {
            get; set;
        }

        public IEnumerable<string> GetListViewNames()
        {
            yield return SetBindingListView.ListViewName;
        }

        public void RenameListviewName(string oldName, string newName)
        {
            if (string.Equals(SetBindingListView.ListViewName, oldName))
            {
                SetBindingListView.ListViewName = newName;
            }
        }

        public void RenameListviewColumnName(string ListViewName, string oldName, string newName)
        {
            if (string.Equals(SetBindingListView.ListViewName, ListViewName))
            {
                if (string.Equals(SetBindingListView.ID, oldName))
                {
                    SetBindingListView.ID = newName;
                }
                if (string.Equals(SetBindingListView.RelatedParentID, oldName))
                {
                    SetBindingListView.RelatedParentID = newName;
                }
                foreach (var myFieldInfo in SetBindingListView.MyFieldInfos)
                {
                    if (string.Equals(myFieldInfo.ShowField, oldName))
                    {
                        myFieldInfo.ShowField = newName;
                    }
                }
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
            return property.Name == nameof(jQueryTreeTable.SetBindingListView)
                ? new HyperlinkEditorSetting(new SetBindingListViewCommand(builderContext))
                : base.GetEditorSetting(property, builderContext);
        }

        private List<Dictionary<string, object>> ReSortListView(List<Dictionary<string, object>> ListViewData, string relatedParentID, string id)
        {
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            for (int i = 0; i < ListViewData.Count; i++)
            {
                if (ListViewData[i][relatedParentID] == null)
                {
                    data.Add(ListViewData[i]);
                    AddTreeNode(ListViewData, data, i, relatedParentID, id);
                }
            }
            return data;
        }

        private void AddTreeNode(List<Dictionary<string, object>> ListViewData, List<Dictionary<string, object>> data, int index, string relatedParentID, string id)
        {
            bool sign = false;
            for (int i = 0; i < ListViewData.Count; i++)
            {
                if (string.Equals(ListViewData[i][relatedParentID], ListViewData[index][id]))
                {
                    data.Add(ListViewData[i]);
                    sign = true;
                    AddTreeNode(ListViewData, data, i, relatedParentID, id);
                }
            }
            if (sign == false)
            {
                return;
            }
        }

    }
    public class SetBindingListViewCommand : ICommand
    {
        private Window window;
        private IEditorSettingsDataContext dataContext;
        private IBuilderContext BuilderContext { get; set; }

        private SetBindingListView control;

        public SetBindingListViewCommand(IBuilderContext builderContext)
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

            control = new SetBindingListView(BuilderContext);
            control.ViewModel.Model = dataContext?.Value as ListViewInfo;
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
    public class ListViewInfo
    {
        public string ListViewName
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
        public ListViewInfo(string listViewName, string id, string relatedParentID, MyFieldInfo[] myFieldInfos)
        {
            ListViewName = listViewName;
            ID = id;
            RelatedParentID = relatedParentID;
            MyFieldInfos = myFieldInfos;
        }
    }
    public enum UnfoldingMethod
    {
        默认收起,
        默认展开
    }
}