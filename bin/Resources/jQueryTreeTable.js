var jQueryTreeTable = (function (_super) {
    __extends(jQueryTreeTable, _super);
    function jQueryTreeTable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }

    jQueryTreeTable.prototype.createContent = function () {
        var self = this;

        var element = this.CellElement;
        var cellTypeMetaData = element.CellType;
        var container = $("<div id='" + this.ID + "'></div>");

        var innerContainer = $(`
<div style='width:100%;height:100%'>
    Custom Cell
</div>`);

        container.append(innerContainer);

        return container;
    };

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