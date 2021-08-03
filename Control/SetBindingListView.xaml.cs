﻿using GrapeCity.Forguncy.CellTypes;
using GrapeCity.Forguncy.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace jQueryTreeTable.Control
{
    public partial class SetBindingListView : UserControl
    {
        public SetBindingListView()
        {
        }
        public SetBindingListView(IBuilderContext builderContext)
        {
            InitializeComponent();
            DataContext = new SetBindingListViewViewModel(builderContext);
        }
        public SetBindingListViewViewModel ViewModel => this.DataContext as SetBindingListViewViewModel;
        public void NewButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.MyFieldInfosViewModel.Add(new MyFieldInfoViewModel(this.ViewModel.ColumnsList));
            if (this.ListView.Items.Count == 1)
            {
                this.ListView.SelectedIndex = 0;
            }
        }
        public void DeleteButtonClick(object sender, RoutedEventArgs e)
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
    }
    public class SetBindingListViewViewModel : PropertyChangedObjectBase
    {
        public IBuilderContext BuilderContext { get; set; }
        public SetBindingListViewViewModel(IBuilderContext context)
        {
            this.BuilderContext = context;
        }

        public ListViewInfo Model
        {
            get => new ListViewInfo(ListViewName, ID, RelatedParentID, ConverMyFieldInfoViewModel(MyFieldInfosViewModel));
            set
            {
                if (value == null)
                {
                    return;
                }
                ListViewName = value.ListViewName;
                ID = value.ID;
                RelatedParentID = value.RelatedParentID;
                foreach (var myFieldInfo in value.MyFieldInfos)
                {
                    var myFieldInfoViewModel = new MyFieldInfoViewModel(ColumnsList)
                    {
                        Model = myFieldInfo
                    };
                    MyFieldInfosViewModel.Add(myFieldInfoViewModel);
                }
            }
        }
        private string listViewName;
        public string ListViewName
        {
            get => listViewName;
            set
            {
                if (this.listViewName != value)
                {
                    this.listViewName = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ColumnsList));
                    MyFieldInfosViewModel.Clear();
                    this.OnPropertyChanged(nameof(MyFieldInfosViewModel));
                }
            }
        }

        public List<string> ListViewsList
        => BuilderContext?.EnumAllListViewInfos(BuilderContext.PageName).Select(t => { return t.ListViewName; }).ToList() ?? new List<string>();
        public List<string> ColumnsList
        => string.IsNullOrEmpty(ListViewName) ? new List<string>() : (BuilderContext?.EnumAllListViewInfos(BuilderContext.PageName).FirstOrDefault(t => t.ListViewName == ListViewName)?.GetAllColumnNames());

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
            get => id;
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

        private MyFieldInfo[] ConverMyFieldInfoViewModel(ObservableCollection<MyFieldInfoViewModel> myFieldInfosViewModel)
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
            get => fieldList;
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
            get => showField;
            set
            {
                showField = value;
                FieldName = showField;
                this.OnPropertyChanged();
            }
        }
        public string FieldName
        {
            get => fieldName;
            set
            {
                fieldName = value;
                this.OnPropertyChanged();
            }
        }

        public MyFieldInfo Model
        {
            get => new MyFieldInfo(ShowField, FieldName);
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
