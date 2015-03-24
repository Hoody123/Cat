/// <reference path="Widget/Image.js" />
/// <reference path="jquery.js" />
/// <reference path="carhartl-jquery-cookie-92b7715/jquery.cookie.js" />
/// <reference path="config.js" />

String.prototype.Format = function (arg1, arg2) {
    var args;
    if (arguments[0] instanceof Array)
        args = arguments[0];
    else
        args = arguments;
    return this.replace(/(\{\d+\})/g, function (word) {
        return MPHtmlEncode(args[parseInt(word.substring(1, word.length - 1))].toString());
    })
}
String.prototype.FormatNoEncode = function (arg1, arg2) {

    var args;
    if (arguments[0] instanceof Array)
        args = arguments[0];
    else
        args = arguments;
    return this.replace(/(\{\d+\})/g, function (word) {
        return args[parseInt(word.substring(1, word.length - 1))].toString();
    })
}

MPData = {};
MPWidget = {};
MPFormat = {};

var MPWaterFall = {
    Item: {
        New: function (element) {
            var item = {};
            item.Element = $(element);
            var _x;
            var _y;
            var _height = item.Element.outerHeight();
            var _wid;

            item.Height = function () {
                return _height;
            }
            item.X = function () {
                if (arguments.length == 0)
                    return _x;
                var value = arguments[0];
                if (value == _x)
                    return;

                item.Element.css("left", value + "px");
                _x = value;
            }
            item.Y = function () {
                if (arguments.length == 0)
                    return _y;

                var value = arguments[0];
                if (value == _y)
                    return;

                item.Element.css("top", value + "px");
                _y = value;
            }
            item.WID = function () {
                if (arguments.length == 0)
                    return _wid;

                var value = arguments[0];
                if (value == _wid)
                    return;

                item.Element.attr("wid", value);
                _wid = value;
            }
            return item;
        }
    },
    New: function (frame, container, columnCount, itemWidth, marginLeft, marginTop, marginRight, marginBottom, autoResize) {

        var waterFall = {};

        //定义是否自动排列,默认为自动,关闭自动排列该参数为false


        autoResize = ((autoResize!=undefined) ? autoResize : true);
        if (autoResize == true) {
                var wwidth = $(window).width();
                columnCount = countColumn();
                $(".wrapper").css("width", columnCount * (itemWidth + marginLeft + marginRight));
        }

        //列数 
        var _columnCount = 1;
        //设置列数,若参数为0,则返回当前列数,否则设定列数
        waterFall.ColumnCount = function () {
            if (arguments.length == 0)
                return _columnCount;

            var value = arguments[0];
            if (value == _columnCount)
                return;

            _columnHeights = [];
            for (var i = 0; i < value; i++) {
                _columnHeights[i] = 0;
            }
            _columnCount = value;
            Layout();
        }

        //瀑布流内元素列表,MPWaterFall.Item[]
        var _list = [];
        //列高度,int[]
        var _columnHeights = [];
        //当前插入的wid
        var _wid = 0;
        //瀑布流容器高度
        var _containerHeight = 0;
        //最矮列高度
        var _minColumnHeight = 0;
        //指示瀑布流是否正在更新
        var _isUpdating = false;
        //指示瀑布流是否全部内容显示完毕
        var _isComplete = false;
        //布局瀑布流        
        function Layout() {
            _containerHeight = 0;
            for (var i = 0; i < _columnCount; i++) {
                _columnHeights[i] = 0;
            }

            var n = _list.length;
            for (var i = 0; i < n; i++) {
                var item = _list[i];
                Arrange(item);
            }
        }
        //在瀑布尾放置元素
        function Arrange(item) {
            var targetColumn = Min(_columnHeights);
            item.X((itemWidth + marginLeft + marginRight) * targetColumn.index + marginLeft);
            item.Y(targetColumn.value + marginTop);
            var height = targetColumn.value + item.Height() + marginTop + marginBottom;
            _columnHeights[targetColumn.index] = height;

            if (height > _containerHeight)
                container.css("height", height);
            _minColumnHeight = Min(_columnHeights).value;
        }

        frame.scroll(function () {
            if (_isUpdating == true)
                return;

            if (_isComplete == true)
                return;

            var isWindow = (frame.get(0) == window);
            var frameTop = isWindow ? 0 : frame.offset().top;
            var containerTop = container.offset().top;
            var height = frame.height();
            var scrollTop = frame.scrollTop();

            var top = containerTop - frameTop + (isWindow ? 0 : scrollTop);
            var a = scrollTop + top + height;
            if (a > _minColumnHeight) {
                if (waterFall.onBottom != null) {
                    waterFall.onBottom();
                }
            }
        })

        function Max(arr) {
            if ((arr instanceof Array) == false)
                throw TypeError("函数只接受数组参数");
            var n = arr.length;
            var max = arr[0];
            var index = 0;
            for (var i = 1; i < n; i++) {
                if (arr[i] > max) {
                    max = arr[i];
                    index = i;
                }
            }

            return { index: index, value: max };
        }

        function Min(arr) {
            if ((arr instanceof Array) == false)
                throw TypeError("函数只接受数组参数");
            var n = arr.length;
            var min = arr[0];
            var index = 0;
            for (var i = 1; i < n; i++) {
                if (arr[i] < min) {
                    min = arr[i];
                    index = i;
                }
            }
            return { index: index, value: min };
        }

        function Add(item) {
            container.append(item);
            var a = MPWaterFall.Item.New(item);
            a.WID(_wid++);
            return a;
        }

        waterFall.Push = function (dataList, type, typeDetail, returnField) {
            var n = dataList.length;
            if (n == 0) {
                waterFall.Complete();
                return 0;
            }
            for (var i = 0; i < n; i++) {
                var item1 = Add(type.New(dataList[i], typeDetail));
                Arrange(item1);
                _list.push(item1);
            }
            return dataList[n - 1][returnField];
        }

        waterFall.Insert = function (startIndex, newItems) {
            var list = [];
            if (newItems instanceof Array) {
                var n = newItems.length;
                for (var i = 0; i < n; i++) {
                    list.push(Add(newItems[i]));
                }
            }
            else {
                list.push(Add(newItems));
            }
            var p1 = _list.slice(0, startIndex);
            var p2 = _list.slice(startIndex);
            var t = p1.concat(list, p2);
            _list = t;
            Layout();
        }

        waterFall.Delete = function (wid) {
            var n = _list.length;
            for (var i = 0; i < n; i++) {
                var item = _list[i];
                if (item.WID() == wid) {
                    _list.splice(i, 1);
                    item.Element.remove();
                    break;
                }
            }
        }

        waterFall.Clear = function () {
            container.empty();
            _list = [];
        }

        waterFall.ColumnCount(columnCount);

        waterFall.BeginUpdate = function () {
            _isUpdating = true;
        }

        waterFall.EndUpdate = function () {
            _isUpdating = false;
        }

        waterFall.Complete = function () {
            _isComplete = true;
        }

        waterFall.onBottom = null;

        Resize();

        function Resize() {
            if (autoResize) {
                $(window).on("resize", function () {
                        var c = countColumn();
                        $(".wrapper").css("width", c * (itemWidth + marginLeft + marginRight));
                        waterFall.ColumnCount(c);
                })
            }
        }

        function countColumn() {
            var w = itemWidth + marginLeft + marginRight;
            var wwidth = $(window).width();
            var count = 1;
            while (w * (count + 1) <= wwidth) {
                count++;
            }

            if (count < 4) {
                return 4;
            }
            else if (count > 6) {
                return 6;
            }
            else
                return count;
        }

        return waterFall;
    }
}

//窗口居中
function MPCenter(content, parent) {
    var cheight = content.height();
    var cwidth = content.width();

    content.css("margin-left", -Math.floor(cwidth / 2));
    content.css("margin-top", -Math.floor(cheight / 2));
}

function MPCheckEmail(e) {
    if (e == "") {
        MPMessageBox.New("alert", "提示", "邮箱不能为空!");
        return false;
    }
    if (!e.match(/^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/)) {
        MPMessageBox.New("alert", "提示", "邮箱格式不正确,请重新输入!");
        return false;
    }
    return true;
}

function MPCheckPassword(e) {
    if (e == "") {
        MPMessageBox.New("alert", "提示", "请输入密码");
        return false;
    }
    return true;
}

function MPHtmlEncode(e) {
    var d = $("<div></div>");
    d.text(e);
    return d.html();
}

function MPLogOut() {
    $.post(host + "/ajax.aspx?act=logout", {}, function (data) {
        if (data.code == 0) {
            $.removeCookie("login");
            alert("logout success");
        }
    }, "json")
}

function MPMenu(parent, menu, staytime, delaytime)//parent为点击目标 menu自行定义 staytime为鼠标离开menu后滞留时间 delaytime为点击后延时处理时间
{
    var _stayTime = staytime ? staytime : 1000;
    var _delayTime = delaytime ? delaytime : 0;
    var _timerIdDisplay;
    var _timerIdHide;
    var _parent = $(parent);
    var _menu = $(menu);
    _parent.mouseenter(function () {
        clearTimeout(_timerIdHide);
        _timerIdDisplay = setTimeout(function () {
            _menu.show();
        }, _delayTime);
    })
    _parent.mouseleave(function () {
        clearTimeout(_timerIdDisplay);
        _timerIdHide = setTimeout(function () {
            _menu.hide();
        }, _stayTime);
    })

    _menu.mouseenter(function () {
        clearTimeout(_timerIdHide);
    })

    _menu.mouseleave(function () {
        _timerIdHide = setTimeout(function () {
            _menu.hide();
        }, _stayTime);
    })
}

function MPPopUpMenu(parent, menu, callback)//parent为点击目标 menu为弹出窗口 callback为menu关闭后响应的事件
{
    var _parent = $(parent);
    var _menu = $(menu);
    _parent.click(function (e) {
        e.stopPropagation();
        _menu.show();
        var clickfn;
        $(window).on("click", clickfn = function (event) {
            var point = {};
            point.X = event.clientX;
            point.Y = event.clientY;
            if (MPCheckInEle(_menu, point)) {
                _menu.show();
            }
            else {
                _menu.hide();
                $(window).off("click", clickfn);
            }
            if (callback != undefined || callback != null) {
                callback();
            }
        })
    })
}

function MPCheckInEle(node, point) {
    var w = $(window);
    var mouseX = point.X;
    var mouseY = point.Y;
    var left = node.offset().left;
    var top = node.offset().top;
    var width = node.width();
    var height = node.height();
    if (mouseX > left - w.scrollLeft() && mouseX < left + width - w.scrollLeft() && mouseY > top - w.scrollTop() && mouseY < top + height - w.scrollTop()) {
        return true;
    }
    else {
        return false;
    }
}


