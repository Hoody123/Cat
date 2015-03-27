/// <reference path="window.js" />
/// <reference path="../Dialog/CreateImageDialog.js" />
/// <reference path="../Format/User.js" />
/// <reference path="../main.js" />
/// <reference path="../Dialog/MessageBox.js" />

MPWidget.Image = {};
MPWidget.Image.Description = function (description)
{
        description = MPHtmlEncode(description);
        return description.replace(/(#.*#)/g, function (word)
        {
            var w = $.trim(word.substring(1, word.length - 1));
            if (w == "")
                return word;

            return "<a href=\"/search/{0}\">{1}</a>".FormatNoEncode(encodeURIComponent(w), word);
        });
};
MPWidget.Image.New = function (image)
{
    var fuser = MPFormat.User.New(image.user);
    var strVar = "";
    strVar += "<div class=\"widget-image\" data-id=\"{0}\">".Format(image.id);
    strVar += "    <div class=\"actions\">";
    strVar += "         <div class=\"left\">";
    strVar += "             <div class=\"repin\" title=\"转存到我的图包\" data-id=\"{0}\" data-hash=\"{1}\" data-description=\"{2}\">转存<\/div>".Format(image.id,image.file.hash,image.description);
    strVar += "         <\/div>";
    if (image.user.id == MPData.user.id)
    {
        strVar += "<div class=\"right\">";
        strVar += "    <div class=\"edit\" title=\"编辑\" data-id=\"{0}\" >编辑<\/div>".Format(image.id);
        strVar += "<\/div>";
    }
    else
    {
        strVar += "<div class=\"right\">";
        strVar += "    <div class=\"praise\" title=\"赞一个\" data-id=\"{0}\" >赞<\/div>".Format(image.id);
        strVar += "<\/div>";
    }
    strVar += "    <\/div>";
    strVar += "    <a class=\"img\" href=\"{0}\">".Format("/image/"+image.id);
    strVar += "        <img src=\"{0}\" width=\"236\" height=\"{1}\" />".Format(imageHost + "/" + image.file.hash + "_fw236", Math.ceil(236 * image.file.height / image.file.width));
    strVar += "        <div class=\"cover\"><\/div>";
    strVar += "    <\/a>";
    strVar += "    <div class=\"description\">{0}<\/div>".FormatNoEncode(this.Description(image.description));
    strVar += "    <div class=\"info\">";
    strVar += "        <a class=\"avt\" href=\"{0}\">".Format(fuser.Home());
    strVar += "            <img src=\"{0}\" />".Format(fuser.Avt());
    strVar += "        <\/a>";
    strVar += "        <div class=\"text\">";
    strVar += "            <div class=\"line\"><a href=\"{0}\">{1}<\/a><span>收集到<\/span><\/div>".Format(fuser.Home(), fuser.Name());
    strVar += "            <div class=\"line\"><a href=\"{0}\">{1}<\/a><\/div>".Format("/package/" + image.package.id, image.package.title);
    strVar += "        <\/div>";
    strVar += "    <\/div>";
    strVar += "<\/div>";

    return $(strVar);
};
MPWidget.Image.Bind = function ()
{
    //点击编辑按钮
    $(document).on("click", ".widget-image .edit", edit_click)
        //点击转存按钮
    .on("click", ".widget-image .repin", repin_click)
        //点击赞按钮
    .on("click", ".widget-image .praise", praise_click)
        //点击取消赞
    .on("click", ".widget-image .unpraise", unpraise_click)
        //点击图像,点击了以后阻止a标签的click,防止页面跳转,并显示图片,这个函数暂时先不实现
    .on("click", ".widget-image a.img", img_click)
        //鼠标进入,显示action
    .on("mouseenter", ".widget-image", mouse_enter)
        //鼠标离开,隐藏action
    .on("mouseleave", ".widget-image", mouse_leave);

    function edit_click()
    {
        var id = $(this).attr("data-id");
        location.href = "/image/" + id + "/edit";
    }

    function img_click(e)
    {
        //防止浏览器跳转
        e.preventDefault();
        var viewerWindow = $(".widget_window");
        if(viewerWindow.length==0)
        {
            var viewerWindow = MPWidget.Window.New();
            var body = $("body");
            viewerWindow.onClose = function ()
            {
                body.removeAttr("style");
            }
            $("body").append(viewerWindow).css("overflow","hidden");            
        }
        viewerWindow.Init($(this).parents(".widget-image"));
    }

    function repin_click()
    {
        var t = $(this);
        var id = t.attr("data-id");
        var hash = t.attr("data-hash");
        //获取要转存图片的描述内容用作初始描述
        var description = t.attr("data-description");
        var dialog = MPCreateImageDialog.New(imageHost + "/" + hash + "_fw236", "转存", description);
        dialog.onOK = function ()
        {
            $.post(host + "/ajax/resave", { image_id: id, package_id: dialog.packageId, description: MPHtmlEncode(dialog.description) }, function (data)
            {
                if (data.code == 0)
                {
                    var box = MPMessageBox.New("ok", "转存成功");
                    box.onOK = function () {
                        dialog.Close();
                    }
                }
            }, "json");
        };
    }

    function praise_click()
    {
        var id = $(this).attr("data-id");
        $.post(host + "/ajax/praise-image", { iamge_id: id }, function (data)
        {
            if (data.code == 0)
            {
                $(this).removeClass("praise");
                $(this).addClass("unpraise");
            }
        }, "json");
    }

    function unpraise_click()
    {
        var id = $(this).attr("data-id");
        $.post(host + "/ajax/unpraise-image", { image_id: id }, function (data)
        {
            if (data.code == 0)
            {
                $(this).removeClass("unpraise");
                $(this).addClass("praise");
            }
        }, "json");
    }

    function mouse_enter()
    {
        $(this).find(".actions").show();
    }

    function mouse_leave()
    {
        $(this).find(".actions").hide();
    }
};
