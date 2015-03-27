/// <reference path="Widget/package.js" />
/// <reference path="main.js" />
/// <reference path="Widget/user-info.js" />
/// <reference path="Widget/Image.js" />
/// <reference path="Widget/package.js" />
$(function () {
    var frame = MPWidget.Frame.New();
    var userInfo = MPWidget.UserInfo.New();
    frame.Body.append(userInfo);
    $("body").append(frame);

    switch (MPData.sub1) {
        case "image":
            waterfallPush(userInfo, MPWidget.Image,null,"id");
            break;

        case "praise":
            {
                switch (MPData.sub2) {
                    case "package":
                        waterfallPush(userInfo, MPWidget.Package,null,"id");
                        break;
                    default:
                        waterfallPush(userInfo, MPWidget.Image,null,"id");
                        break;
                }
            }
            break;

        case "following":
            {
                switch (MPData.sub2) {
                    case "package":
                        waterfallPush(userInfo, MPWidget.Package,null,"id");
                        break;
                    default:
                        waterfallPush(userInfo, MPWidget.User,null,"id");
                        break;
                }
            }
            break;

        case "follower":
            waterfallPush(userInfo, MPWidget.User,null,"id");
            break;

        default:
            waterfallPush(userInfo, MPWidget.Package,null,"id");
            break;
    }
    userInfo.waterfall.Resize();
    function waterfallPush(userinfo, type, typeDetail, returnField) {
        var max = userinfo.waterfall.Push(MPData.datas, type, typeDetail, returnField);
        userinfo.waterfall.onBottom = function () {
            userinfo.waterfall.BeginUpdate();
            $.getJSON("", { ajax: true, max: max }, function (data) {
                max = userinfo.waterfall.Push(data, type, typeDetail, returnField);
                userinfo.waterfall.EndUpdate();
            });
        }
    }
})