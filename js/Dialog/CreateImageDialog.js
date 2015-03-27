/// <reference path="TitleDialog.js" />
/// <reference path="../jquery.js" />
/// <reference path="MessageBox.js" />
/// <reference path="Dialog.js" />
MPCreateImageDialog =//创建图片
    {
        New: function (imageSrc, title, description) {
            var strVar = "";
            strVar += "<div class=\"dialog-mask\">";
            strVar += "    <div class=\"dialog-box\">";
            strVar += "        <div class=\"dialog-title\">";
            strVar += "            <span class=\"text\">{1}<\/span>";
            strVar += "            <div class=\"dialog-close\">";
            strVar += "            <\/div>";
            strVar += "        <\/div>";
            strVar += "        <div class=\"dialog-content\">";
            strVar += "            <div class=\"create-image-dialog\">";
            strVar += "                <div class=\"preview\">";
            strVar += "                    <img src=\"{0}\" width=\"180\">";
            strVar += "                <\/div>";
            strVar += "                <div class=\"right\">";
            strVar += "                    <div class=\"package-list\">";
            strVar += "                        <div class=\"current\">";
            strVar += "                            <div class=\"name\">&nbsp;";
            strVar += "                            <\/div>";
            strVar += "                            <div class=\"arrow\">";
            strVar += "                            <\/div>";
            strVar += "                        <\/div>";
            strVar += "                        <div class=\"drop-list\">";
            strVar += "                            <div class=\"selections\"><\/div>";
            strVar += "                            <div class=\"filtrate\">";
            strVar += "                                <div class=\"create\"><\/div>";
            strVar += "                            <\/div>";
            strVar += "                            <div class=\"filter\">";
            strVar += "                                <input type=\"text\" placeholder=\"快速筛选/创建图包\">";
            strVar += "                            <\/div>";
            strVar += "                        <\/div>";
            strVar += "                    <\/div>";
            strVar += "                    <div class=\"description\">";
            strVar += "                        <textarea>{2}<\/textarea>";
            strVar += "                        <div class=\"tip\">";
            strVar += "                            给图片添加 #标签#，可以更好地整理图片哦~";
            strVar += "                        <\/div>";
            strVar += "                    <\/div>";
            strVar += "                <\/div>";
            strVar += "            <\/div>";
            strVar += "        <\/div>";
            strVar += "        <div class=\"dialog-btns\">";
            strVar += "            <div class=\"ok\">确认<\/div>";
            strVar += "            <div class=\"cancel\">取消<\/div>";
            strVar += "        <\/div>";
            strVar += "    <\/div>";
            strVar += "<\/div>";

            title = title ? title : "";
            description = description ? description : "";

            var dialog = MPTitleDialog.New(strVar.Format(imageSrc, title, description));
            var description = dialog.Content.find(".description textarea");//描述
            var bCurrent = dialog.Content.find(".package-list");//当前图标按钮
            var dropList = dialog.Content.find(".drop-list");//点击后弹出列表
            var select = dialog.Content.find(".selections");//图包列表
            var filterate = dialog.Content.find(".filtrate");//筛选栏输入后显示的内容
            var filterSearch = dialog.Content.find(".filter input");//筛选栏
            dialog.onOK = null;
            dialog.description = "";
            dialog.packageId = 0;
            dialog.Title(title);

            $.post(host + "/ajax/query-packages", {}, function (data) {
                if (data.code == 0) {
                    var packagelist = data.packages;
                    var length = packagelist.length;
                    for (var i = 0; i < length; i++) {
                        var option = $("<div/>").addClass("package");
                        option.text(packagelist[i].title);
                        option.attr("data-package-id", packagelist[i].id);
                        select.append(option);
                    }
                    if (packagelist.length != 0) {
                        bCurrent.attr("data-package-id", packagelist[0].id);
                        bCurrent.find(".name").text(packagelist[0].title);
                    }
                }
            }, "json");//获取图包



            var dropListHide = function () {
                filterate.hide();
                select.show();
                filterSearch.val("");
            }

            MPPopUpMenu(bCurrent, dropList, dropListHide);

            dropList.on("click", ".package", function (e) {
                var a = $(this);
                bCurrent.attr("data-package-id", a.attr("data-package-id"));
                bCurrent.find(".name").text(a.text());
                dropList.hide();
                e.stopPropagation();
            });

            filterate.find(".create").click(function () {
                var a = $(this);
                if (a.text == null) {
                    return;
                }
                $.post(host + "/ajax/create-package", { title: MPHtmlEncode(filterSearch.val()) }, function (data) {
                    if (data.code == 0) {
                        bCurrent.attr("data-package-id", data.packageid);
                        bCurrent.find(".name").text(filterSearch.val());

                        var option = $("<div/>").addClass("package");
                        option.text(filterSearch.val());
                        option.attr("data-package-id", data.packageid);
                        select.append(option);//将新的选项添加到select中
                        dropListHide();
                        dropList.hide();
                    }
                    else {
                        MPMessageBox.New("warn", data.msg);
                    }
                }, "json");
            })

            filterSearch.keyup(function () {
                var val = $.trim(filterSearch.val());
                if (val == "") {
                    select.show();
                    filterate.hide();
                    return;
                }
                select.hide();
                filterate.show();

                //清空快速搜素列表
                filterate.find(".package").remove();
                //获取当前图包列表
                var packageList = select.find(".package");
                if (packageList.length == 0) {
                    filterate.find(".create").text("新建图包---" + val);
                }
                else {
                    packageList.each(function () {
                        if ($(this).text().indexOf(val) != -1) {
                            //找到了之后就复制一个放入候选列表
                            filterate.append($(this).clone());
                        }
                        else {
                            filterate.find(".create").text("新建图包---" + val);
                        }
                    });
                }
            });

            dialog.ButtonOK.click(function () {
                dialog.description = description.val();
                dialog.packageId = bCurrent.attr("data-package-id");
                if (dialog.packageId == "" || dialog.packageId == undefined) {
                    MPMessageBox.New(MPMessageBox.Icons.Warn, "请选择一个图包,如果没有图包请新建一个图包!");
                    return;
                }
                if (dialog.onOK != null) {
                    dialog.onOK();
                }
                else {
                    dialog.Close();
                }
            })

            dialog.Content.find(".cancel").click(function () {
                dialog.Close();
            })//取消按钮

            return dialog;
        }
    }