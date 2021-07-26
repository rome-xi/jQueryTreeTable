using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace jQueryTreeTable.Control
{
    /// <summary>
    /// SetBindingTable.xaml 的交互逻辑
    /// </summary>
    public partial class SetBindingTable : UserControl
    {
        public SetBindingTable()
        {
        }

        public SetBindingTable(IBuilderContext builderContext)
        {
            InitializeComponent();
            DataContext = new SetBindingTableViewModel(builderContext);
        }

        public SetBindingTableViewModel ViewModel
        {
            get
            {
                return this.DataContext as SetBindingTableViewModel;
            }
        }

        public void NewButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.MyFieldInfosViewModel.Add(new MyFieldInfoViewModel(this.ViewModel.ColumnsList));
            if (this.ListView.Items.Count == 1)
            {
                this.ListView.SelectedIndex = 0;
            }
        }
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var index = this.ListView.SelectedIndex;
            if (index != -1)
            {
                this.ViewModel.MyFieldInfosViewModel.RemoveAt(index);
            }
            if (this.ListView.Items.Count > 0)
            {
                this.ListView.SelectedIndex = 0;
            }
        }

        private void EditQueryConditionHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            var window = ViewModel.BuilderContext?.GetQueryConditionWindow(ViewModel.QueryCondition, ViewModel.TableName);
            if (window == null)
            {
                return;
            }

            window.Closed += (s, e2) =>
            {
                if (window.DialogResult == true)
                {
                    ViewModel.QueryCondition = window.QueryCondition;
                }
                ViewModel.BuilderContext.ShowParentDialog(this);
            };

            ViewModel.BuilderContext.HideParentDialog(this);
            window.ShowDialog();
        }

        private void EditSortConditionHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            var window = ViewModel.BuilderContext?.GetSortConditionWindow(ViewModel.SortCondition, ViewModel.TableName);
            if (window == null)
            {
                return;
            }

            window.Closed += (s, e2) =>
            {
                if (window.DialogResult == true)
                {
                    ViewModel.SortCondition = window.SortCondition;
                }
                ViewModel.BuilderContext.ShowParentDialog(this);
            };

            ViewModel.BuilderContext.HideParentDialog(this);
            window.ShowDialog();
        }
    }
    public class SetBindingTableViewModel: PropertyChangedObjectBase
    {
        public object QueryCondition { get; set; }

        public object SortCondition { get; set; }

        public IBuilderContext BuilderContext { get; set; }

        public SetBindingTableViewModel(IBuilderContext context)
        {
            this.BuilderContext = context;
        }

        public TableInfo Model
        {
            get
            {
                return new TableInfo(TableName, ID, RelatedParentID, ConverMyFieldInfoViewModel(MyFieldInfosViewModel), QueryCondition, SortCondition) ;
            }
            set
            {
                if (value == null) 
                {
                    return;
                }
                TableName = value.TableName;
                ID = value.ID;
                RelatedParentID = value.RelatedParentID;
                foreach(var item in value.MyFieldInfos)
                {
                    var itemViewModel = new MyFieldInfoViewModel(ColumnsList);
                    itemViewModel.Model = item;
                    MyFieldInfosViewModel.Add(itemViewModel);
                }
                QueryCondition = value.QueryCondition;
                SortCondition = value.SortCondition;
            }
        }

        private string tableName;
        public string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                if (this.tableName != value)
                {
                    this.tableName = value;
                    //
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ColumnsList));

                }
            }
        }


        public List<string> TablesList
        {
            get
            {
                return BuilderContext?.EnumAllTableInfos().Select(t => { return t.TableName; }).ToList() ?? new List<string>();
            }
        }
        public List<string> ColumnsList
        {
            get
            {
                if (string.IsNullOrEmpty(TableName))
                {
                    return new List<string>();
                }
                return BuilderContext?.EnumAllTableInfos().FirstOrDefault(t => t.TableName == TableName)?.Columns?
                        .Where(c => c.ColumnKind != TableColumnKind.StatisticsColumn)?
                        .Select(c => c.ColumnName).ToList() ?? new List<string>();
            }
        }

        private ObservableCollection<MyFieldInfoViewModel> myFieldInfos;

        public ObservableCollection<MyFieldInfoViewModel> MyFieldInfosViewModel
        {
            get
            {
                if (myFieldInfos == null)
                {
                    myFieldInfos = new ObservableCollection<MyFieldInfoViewModel>();
                }
                return myFieldInfos;
            }
            set
            {
                myFieldInfos = value;
            }

        }

        private string id;
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                this.OnPropertyChanged();
            }
        }

        private string parentId;
        public string RelatedParentID
        {
            get => parentId;
            set
            {
                parentId = value;
                this.OnPropertyChanged();
            }
        }

        public MyFieldInfo[] ConverMyFieldInfoViewModel(ObservableCollection<MyFieldInfoViewModel> myFieldInfosViewModel)
        {
            MyFieldInfo[] MyFieldInfos = new MyFieldInfo[myFieldInfosViewModel.Count];
            int index = 0;
            foreach (var myFieldInfoViewModel in myFieldInfosViewModel)
            {
                MyFieldInfos[index++] = new MyFieldInfo(myFieldInfoViewModel.ShowField, myFieldInfoViewModel.FieldName);
            }
            return MyFieldInfos;
        }
    }
    public class MyFieldInfoViewModel : PropertyChangedObjectBase
    {
        public MyFieldInfoViewModel(List<string> columnList)
        {
            this.FieldList = columnList;
        }
        private List<string> fieldList;
        public List<string> FieldList
        {
            get
            {
                return fieldList;
            }
            set
            {
                fieldList = value;
                this.OnPropertyChanged();
            }
        }

        private string showField;
        private string fieldName;

        public string ShowField
        {
            get
            {
                return showField;
            }
            set
            {
                showField = value;
                FieldName = showField;
                this.OnPropertyChanged();
            }
        }

        public string FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                fieldName = value;
                this.OnPropertyChanged();
            }
        }

        public MyFieldInfo Model
        {
            get
            {
                return new MyFieldInfo(ShowField, FieldName);
            }
            set
            {
                ShowField = value.ShowField;
                FieldName = value.FieldName;
            }
        }

    }
    public class PropertyChangedObjectBase : INotifyPropertyChanged
    {

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
