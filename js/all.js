﻿/// <reference path="main.js" />
/// <reference path="Widget/Frame.js" />
/// <reference path="Widget/Image.js" />
$(function ()
{
    var frame = MPWidget.Frame.New();
    var waterfall = $("<div/>").addClass("waterfall");
    MPWidget.Image.Bind(waterfall);
    frame.Body.append(waterfall);
    $("body").append(frame);

    document.title = "最新图片_喵帕斯";

    var n = MPData.images.length;
    var fall1 = MPWaterFall.New($(window), waterfall, 4, 236, 7, 7, 7, 7);
    var max = fall1.Push(MPData.images, MPWidget.Image,null,"id");
    fall1.onBottom = function ()
    {
        fall1.BeginUpdate();
        $.getJSON("", { ajax: true, max: max }, function (data)
        {
            max = fall1.Push(data, MPWidget.Image, null, "id");
            fall1.EndUpdate();
        });
    }
})