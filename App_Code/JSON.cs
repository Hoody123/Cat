using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Dynamic;

/// <summary>
/// JSON 的摘要说明
/// </summary>
namespace JSON
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool default_head { get; set; }
        public User(MPUser user)
        {
            id = user.ID;
            name = user.Name;
            default_head = user.DefaultHead;
        }
    }
    public class Mention
    {
        public int id { get; set; }
        public int pos { get; set; }
        public int len { get; set; }
        public Mention(MPCommentMention mention)
        {
            id = mention.UserID;
            pos = mention.Position;
            len = mention.Length;
        }
    }
    public class Comment
    {
        public int id { get; set; }
        public JSON.User user { get; set; }
        public string text { get; set; }
        public string time { get; set; }
        public List<JSON.Mention> mentions { get; set; }
        public Comment(MPComment comment)
        {
            id = comment.ID;
            user = new JSON.User(new MPUser(comment.UserID));
            text = comment.Text;
            time = comment.CreatedTime.ToString();
            mentions = new List<JSON.Mention>();
            var res = DB.SExecuteReader("select id from comment_mention where commentid=?", id);
            foreach (var item in res)
            {
                mentions.Add(new JSON.Mention(new MPCommentMention(Convert.ToInt32(item[0]))));
            }

        }
    }

    public class UserDetail
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool default_head { get; set; }
        public int follows_count { set; get; }
        public int followers_count { set; get; }
        public int packages_count { set; get; }
        public int images_count { set; get; }
        public int praise_count { set; get; }
        public bool followed { set; get; }

        public UserDetail(MPUser user, MPUser currentUser)
        {
            id = user.ID;
            name = user.Name;
            default_head = user.DefaultHead;
            follows_count = Convert.ToInt32(DB.SExecuteScalar("select count(*) from following where userid=?", user.ID));
            followers_count = Convert.ToInt32(DB.SExecuteScalar("select count(*) from following where type=? and info=?", MPFollowingTypes.User, user.ID));
            packages_count = Convert.ToInt32(DB.SExecuteScalar("select count(*) from package where userid=?", user.ID));
            images_count = Convert.ToInt32(DB.SExecuteScalar("select count(*) from image where userid=?", user.ID));
            praise_count = Convert.ToInt32(DB.SExecuteScalar("select count(*) from praise where userid=?", user.ID));

            followed = false;
            if (currentUser != null && currentUser.ID != user.ID)
            {
                var res = DB.SExecuteScalar("select userid from following where userid=? and type=? and info=?", currentUser.ID, MPFollowingTypes.User, user.ID);
                if (res != null)
                    followed = true;
            }
        }

    }

    public class File
    {
        public string hash { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public File(MPFile file)
        {
            hash = file.MD5;
            width = file.Width;
            height = file.Height;
        }
    }

    public class Package
    {
        public int id { get; set; }
        public string title { get; set; }
        public Package(MPPackage package)
        {
            id = package.ID;
            title = package.Title;
        }
    }
    public class Image
    {
        public int id { get; set; }
        public JSON.File file { get; set; }
        public Image(MPImage image)
        {
            id = image.ID;
            file = new JSON.File(new MPFile(image.FileID));
        }

    }

    public class PackageDetail
    {
        public int id { get; set; }
        public JSON.User user { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<JSON.Image> thumbs { get; set; }
        public int imageCount { get; set; }
        public int followerCount { get; set; }
        public bool praised { get; set; }
        public bool followed { get; set; }

        public PackageDetail(MPPackage package, MPUser currentUser)
        {
            id = package.ID;
            title = package.Title;
            description = package.Description;
            thumbs = new List<Image>();
            var res = DB.SExecuteReader("select image.id from image left join package on image.id=package.coverid where packageid=? order by coverid desc ,image.id desc limit 4", package.ID);
            foreach (var item in res)
            {
                thumbs.Add(new JSON.Image(new MPImage(Convert.ToInt32(item[0]))));
            }
            imageCount = Convert.ToInt32(DB.SExecuteScalar("select count(*) from image where packageid=?", package.ID));
            followerCount = Convert.ToInt32(DB.SExecuteScalar("select count(*) from following where type=? and info=?", MPFollowingTypes.Package, package.ID));
            user = new JSON.User(new MPUser(package.UserID));
            if (currentUser != null && currentUser.ID != user.id)
            {
                praised = false;
                followed = false;

                if (DB.SExecuteScalar("select userid from praise where userid=? and type=? and info=?", currentUser.ID, MPPraiseTypes.Package, package.ID) != null)
                {
                    praised = true;
                }
                if (DB.SExecuteScalar("select userid from following where userid=? and type=? and info=?", currentUser.ID, MPFollowingTypes.Package, package.ID) != null)
                {
                    praised = true;
                }
            }
        }
    }

    public class ImageDetail
    {
        public int id { get; set; }
        public JSON.Package package { get; set; }
        public JSON.File file { get; set; }
        public JSON.User user { get; set; }
        public string description { get; set; }
        public bool praised { get; set; }
        public List<JSON.Comment> comments { get; set; }
        public string time { get; set; }

        public ImageDetail(MPImage image, MPUser currentUser)
        {
            id = image.ID;
            package = new JSON.Package(new MPPackage(image.PackageID));
            file = new JSON.File(new MPFile(image.FileID));
            user = new JSON.User(new MPUser(image.UserID));
            description = image.Description;
            praised = false;
            if (currentUser != null)
            {
                if (DB.SExecuteScalar("select userid from praise where userid=? and type=? and info=?", currentUser.ID, MPPraiseTypes.Image, image.ID) != null)
                {
                    praised = true;
                }
            }

            comments = new List<Comment>();
            var res = DB.SExecuteReader("select id from comment where imageid=?", image.ID);
            foreach (var item in res)
            {
                comments.Add(new JSON.Comment(new MPComment(Convert.ToInt32(item[0]))));
            }

            time = image.CreatedTime.ToString();
        }
    }
}

