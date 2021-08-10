var jQueryTreeTable = (function (_super) {
    __extends(jQueryTreeTable, _super);
    function jQueryTreeTable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }

    jQueryTreeTable.prototype.createContent = function () {
        var self = this;
        var page = Forguncy.Page;
        var container = $("<div id='" + self.ID + "'></div>");
        page.bind("PageDefaultDataLoaded", function () {
            var cellTypeMetaData = self.CellElement.CellType;
            var unfoldingMethod = cellTypeMetaData.SetUnfoldingMethod;
            var listViewData = self.getListViewData(cellTypeMetaData);
            var data = self.reSortTable(cellTypeMetaData, listViewData);
            var innerContainer = self.createTable(cellTypeMetaData, data);
            container.append(innerContainer);
            self.decorateTable(unfoldingMethod);
            self.selectTreeNode(self.getValueFromElement());
            self.bindEvents(cellTypeMetaData);
        });
        return container;
    };

    jQueryTreeTable.prototype.getListViewData = function (cellTypeMetaData) {
        var listViewName = cellTypeMetaData.SetBindingListView.ListViewName;
        var id = cellTypeMetaData.SetBindingListView.ID;
        var relatedParentID = cellTypeMetaData.SetBindingListView.RelatedParentID;
        var fieldInfos = cellTypeMetaData.SetBindingListView.MyFieldInfos;
        var fields = fieldInfos.map((info) => { return info.ShowField; });

        fields.push(id);
        fields.push(relatedParentID);
        var queryFields = Array.from(new Set(fields));

        var page = Forguncy.Page;
        var listView = page.getListView(listViewName);
        var listViewData = new Array();
        for (var i = 0; i < listView.getRowCount(); i++) {
            var map = new Map();
            for (var j = 0; j < queryFields.length; j++) {
                var queryField = "" + queryFields[j];
                map.set(queryFields[j], listView.getText(i, queryField));
            }
            listViewData.push(map);
        }
        return listViewData;
    };
    jQueryTreeTable.prototype.reloadData = function () {
        var self = this;
        var container = $("#" + self.ID);
        container.empty();
        var cellTypeMetaData = self.CellElement.CellType;
        var unfoldingMethod = cellTypeMetaData.SetUnfoldingMethod;
        var listViewData = self.getListViewData(cellTypeMetaData);
        var data = self.reSortTable(cellTypeMetaData, listViewData);
        var innerContainer = self.createTable(cellTypeMetaData, data);
        container.append(innerContainer);
        self.decorateTable(unfoldingMethod);
        self.selectTreeNode(self.getValueFromElement());
    };

    jQueryTreeTable.prototype.bindEvents = function (cellTypeMetaData) {
        var self = this;
        var listViewName = cellTypeMetaData.SetBindingListView.ListViewName;
        var listView = Forguncy.Page.getListView(listViewName);
        listView.bind("reloaded", function () {
            Forguncy.DelayRefresh.Push(self, function () {
                Forguncy.DelayRefresh.Push(self, function () {
                    self.reloadData();
                }, "jQueryTreeTable_reloaded1");
            }, "jQueryTreeTable_reloaded2");
        });
    };

    jQueryTreeTable.prototype.createTable = function (cellTypeMetaData, data) {
        var id = cellTypeMetaData.SetBindingListView.ID;
        var relatedParentID = cellTypeMetaData.SetBindingListView.RelatedParentID;
        var fieldInfos = cellTypeMetaData.SetBindingListView.MyFieldInfos;
        var fields = fieldInfos.map((info) => { return info.ShowField; });
        var names = fieldInfos.map((info) => { return info.FieldName; });

        var innerContainer = $("<table id='" + this.ID + "t'></table>");
        var thead = $("<thead></thead>");
        var tbody = $("<tbody></tbody>")
        var head_tr = $("<tr></tr>");

        for (var name in names) {
            var th = $("<th>" + names[name] + "</th>");
            head_tr.append(th);
        }
        thead.append(head_tr);

        for (var i = 0; i < data.length; i++) {
            if (data[i].get(relatedParentID) === null) {
                var tr = $("<tr data-tt-id='" + data[i].get(id) + "'></tr>");
            } else {
                var tr = $("<tr data-tt-id='" + data[i].get(id) + "' data-tt-parent-id='" + data[i].get(relatedParentID) + "'></tr>")
            }
            for (let j = 0; j < fields.length; j++) {
                var temp = data[i].get(fields[j]);
                temp = temp === null ? "" : temp;
                var td = $("<td>" + temp + "</td>");
                tr.append(td);
            }
            tbody.append(tr);
        }
        innerContainer.append(thead);
        innerContainer.append(tbody);

        return innerContainer;
    };

    jQueryTreeTable.prototype.decorateTable = function (unfoldingMethod) {
        var id = "#" + this.ID;
        var self = this;

        $(id).css('overflow', 'auto');
        $(id).css('height', "100%");
        $(id + "t").treetable({ expandable: true });
        if (unfoldingMethod === 1) {
            $(id + "t").treetable('expandAll');
        }
        this.addStyle(id);
        if (this.CellElement.CellType.GridLineShow) {
            this.printGrid(id);
        }
    };

    jQueryTreeTable.prototype.addStyle = function (id) {
        var self = this;
        $(id + "t thead").addClass("jQueryTreeTablejQueryTreeTable-" + this.CellElement.CellType.TemplateKey + "-tableHead");
        $(id + "t tbody tr").addClass("jQueryTreeTablejQueryTreeTable-" + this.CellElement.CellType.TemplateKey + "-tableBody");
        $(id + "t tbody").on("mousedown", "tr", function () {
            if (!$(this).hasClass("selected")) {
                $(".selected").not(this).removeClass("selected");
                $(this).toggleClass("selected");
            }
            self.CellElement.Value = this.dataset.ttId;
            self.commitValue();
        });
    };

    jQueryTreeTable.prototype.printGrid = function (id) {
        var color = Forguncy.ConvertToCssColor(this.CellElement.CellType.GridLineColor);
        var width = this.CellElement.CellType.GridLineWidth;
        var headColor = Forguncy.ConvertToCssColor(this.CellElement.StyleTemplate.Styles.tableHead.NormalStyle.Background);

        $(id + " th").css("border", width + "px solid " + color);
        $(id + " td").css("border", width + "px solid " + color);
    };

    //jQueryTreeTable要求表的记录顺序和展示顺序相同
    jQueryTreeTable.prototype.reSortTable = function (cellTypeMetaData, tableData) {
        var id = cellTypeMetaData.SetBindingListView.ID;
        var relatedParentID = cellTypeMetaData.SetBindingListView.RelatedParentID;
        var data = new Array();
        for (var i = 0; i < tableData.length; i++) {
            if (tableData[i].get(relatedParentID) === "") {
                data.push(tableData[i]);
                this.addTreeNode(tableData, data, i, relatedParentID, id);
            }
        }
        return data;
    };

    jQueryTreeTable.prototype.addTreeNode = function (tableData, data, index, relatedParentID, id) {
        var sign = false;
        for (var i = 0; i < tableData.length; i++) {
            if (tableData[i].get(relatedParentID) === tableData[index].get(id)) {
                data.push(tableData[i]);
                sign = true;
                this.addTreeNode(tableData, data, i, relatedParentID, id);
            }
        }
        if (sign === false) {
            return;
        }
    };

    jQueryTreeTable.prototype.getValueFromElement = function () {
        return this.CellElement.Value;
    };

    jQueryTreeTable.prototype.setValueToElement = function (element, value) {
        if (this.CellElement.Value !== value) {
            this.CellElement.Value = value;
        }
        this.selectTreeNode(value);
    };

    jQueryTreeTable.prototype.selectTreeNode = function (value) {

        var trList = $("#" + this.ID + "t tbody").children("tr");

        for (var i = 0; i < trList.length; i++) {
            if (trList[i].dataset.ttId === ("" + value)) {
                $(trList[i]).not(this).removeClass("selected");
                $(trList[i]).toggleClass("selected");
            }
        };
    };

    jQueryTreeTable.prototype.disable = function () {
        _super.prototype.disable.call(this);
    };

    jQueryTreeTable.prototype.enable = function () {
        _super.prototype.enable.call(this);
    };

    return jQueryTreeTable;
}(Forguncy.CellTypeBase));

// Key format is "Namespace.ClassName, AssemblyName"
Forguncy.Plugin.CellTypeHelper.registerCellType("jQueryTreeTable.jQueryTreeTable, jQueryTreeTable", jQueryTreeTable);