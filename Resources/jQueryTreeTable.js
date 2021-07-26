var jQueryTreeTable = (function (_super) {
    __extends(jQueryTreeTable, _super);
    function jQueryTreeTable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }

    jQueryTreeTable.prototype.createContent = function () {
        var self = this;

        var element = this.CellElement;
        var cellTypeMetaData = element.CellType;
        var tableName = cellTypeMetaData.SetBindingTable.TableName;
        var id = cellTypeMetaData.SetBindingTable.ID;
        var relatedParentID = cellTypeMetaData.SetBindingTable.RelatedParentID;
        var fieldInfos = cellTypeMetaData.SetBindingTable.MyFieldInfos;

        var innerContainer = $("<table id='" + this.ID + "t'></table>");
        var thead = $("<thead></thead>");
        var head_tr = $("<tr></tr>");
        s
        var fields = fieldInfos.map((info) => { return info.ShowField; });
        var names = fieldInfos.map((info) => { return info.FieldName; });

        for (var s in names) {
            var th = $("<th>" + names[s] + "</th>");
            head_tr.append(th);
        }
        thead.append(head_tr);

        fields.push(id);
        fields.push(relatedParentID);

        fields = Array.from(new Set(fields));
        var param = {
            TableName: tableName,
            Columns: fields,
            QueryCondition: cellTypeMetaData.SetBindingTable.QueryCondition,
            QueryPolicy: {
                Distinct: false,
                QueryNullPolicy: Forguncy.QueryNullPolicy.QueryAllItemsWhenValueIsNull,
                IgnoreCache: false
            },
            SortCondition: cellTypeMetaData.SetBindingTable.SortCondition
        };
        var formulaCalcContext = {
            IsInMasterPage: false
        };
        var tableData;
        Forguncy.getTableDataByCondition(param, formulaCalcContext, function (dataStr) {
            tableData = dataStr;
        }, false);

        data = this.ReSortTable(tableData, relatedParentID, id);

        var tbody = $("<tbody></tbody>")
        for (var i = 0; i < data.length; i++) {
            if (data[i][relatedParentID] === null) {
                var tr = $("<tr data-tt-id='" + data[i][id] + "'></tr>");
            } else {
                var tr = $("<tr data-tt-id='" + data[i][id] + "' data-tt-parent-id='" + data[i][relatedParentID] + "'></tr>")
            }
            var fields = fieldInfos.map((info) => { return info.ShowField; });
            for (let j = 0; j < fields.length; j++) {
                var temp = data[i][fields[j]];
                temp = temp === null ? "" : temp;
                var td = $("<td>" + temp + "</td>");
                tr.append(td);
            }
            tbody.append(tr);
        }

        var container = $("<div id='" + this.ID + "'></div>");

        innerContainer.append(thead);
        innerContainer.append(tbody);
        container.append(innerContainer);s

        return container;
    };
    //jQueryTreeTable要求表的记录顺序和展示顺序相同
    jQueryTreeTable.prototype.ReSortTable = function (tableData, relatedParentID, id) {
        var data = new Array();
        for (var i = 0; i < tableData.length; i++) {
            if (tableData[i][relatedParentID] === null) {
                data.push(tableData[i]);
                this.addTreeNode(tableData, data, i, relatedParentID, id);
            }
        }
        return data;
    }

    jQueryTreeTable.prototype.addTreeNode = function (tableData, data, index, relatedParentID, id) {
        var sign = false;
        for (var i = 0; i < tableData.length; i++) {
            if (tableData[i][relatedParentID] === tableData[index][id]) {
                data.push(tableData[i]);
                sign = true;
                this.addTreeNode(tableData, data, i, relatedParentID, id);
            }
        }
        if (sign === false) {
            return;
        }
    }

    jQueryTreeTable.prototype.onLoad = function () {


        $("#" + this.ID).css('overflow', 'auto');
        $("#" + this.ID).css('height', '100%');

        var id ="#" + this.ID + "t";
        $(id).treetable({ expandable: true });

        $(id + " tbody").on("mousedown", "tr", function () {
            $(".selected").not(this).removeClass("selected");
            $(this).toggleClass("selected");
        });


    }

    jQueryTreeTable.prototype.getValueFromElement = function () {
        return null;
    };

    jQueryTreeTable.prototype.setValueToElement = function (element, value) {

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