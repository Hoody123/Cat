/// <reference path="Widget/Image.js" />
/// <reference path="Widget/package.js" />
/// <reference path="main.js" />
/// <reference path="Widget/Image.js" />
/// <reference path="Widget/package.js" />
/// <reference path="Widget/package-info.js" />

$(function () {
    var frame = MPWidget.Frame.New();
    var packageInfo = MPWidget.PackageInfo.New(MPData.package);
    frame.Body.append(packageInfo);
    $("body").append(frame);

    switch (MPData.sub1) {

        case "follower":
            waterfallPush(packageInfo, MPWidget.User, null, "id");
            break;

        default:
            waterfallPush(packageInfo, MPWidget.Image, null, "id");
            break;
    }

    packageInfo.waterfall.Resize();
    function waterfallPush(packageinfo, type, typeDetail, returnField) {
        var max = packageinfo.waterfall.Push(MPData.datas, type, typeDetail, returnField);
        packageinfo.waterfall.onBottom = function () {
            packageinfo.waterfall.BeginUpdate();
            $.getJSON("", { ajax: true, max: max }, function (data) {
                max = packageinfo.waterfall.Push(data, type, typeDetail, returnField);
                packageinfo.waterfall.EndUpdate();
            });
        }
    }
})
