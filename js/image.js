/// <reference path="jquery.js" />
/// <reference path="Widget/Frame.js" />
/// <reference path="main.js" />
/// <reference path="Widget/image-view.js" />
$(function ()
{
    var frame = MPWidget.Frame.New();
    var image = MPData.image;
    var imageView = MPWidget.ImageView.New(image);
    frame.Body.append(imageView);

    $("body").append(frame);    
    imageView.Run();

    var description = image.description.length > 20 ? image.description.substring(0, 20) + "..." : image.description;
    var username = image.user.name;
    var packagetitle = image.package.title;
});