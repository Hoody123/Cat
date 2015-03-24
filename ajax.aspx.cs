using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;

public partial class ajax : System.Web.UI.Page
{
    MPUser CheckLogin()
    {
        MPUser user = Session["user"] as MPUser;
        if (user == null)
        {
            throw new MiaopassNotLoginException();
        }

        return user;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string act = RouteData.Values["act"] as string;
            switch (act)
            {
                #region login 登录
                case "login":
                    {
                        if (Session["user"] != null)
                        {
                            throw new MiaopassAlreadyLoginException();
                        }
                        string email = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["email"]).Trim());
                        string password = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["password"]));
                        var res = DB.SExecuteScalar("select id from user where email=? and password=?", email, Tools.SHA256Hash(password));
                        if (res == null)
                            throw new MiaopassLoginErrorException();

                        var user = new MPUser(Convert.ToInt32(res));
                        Session["user"] = user;

                        while (true)
                        {
                            try
                            {
                                string cookie = user.ID + "_" + Tools.BytesToString(Guid.NewGuid().ToByteArray());
                                DB.SExecuteNonQuery("insert into login_cookie (userid,cookievalue,expire) values (?,?,?)", user.ID, cookie, DateTime.Now.AddMonths(1));
                                Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok", cookie = cookie }));
                                break;
                            }
                            catch { }
                        }
                    }
                    break;
                #endregion
                #region signup-email 申请邮箱注册
                case "signup-email":
                    {
                        if (Session["user"] != null)
                        {
                            throw new MiaopassAlreadyLoginException();
                        }

                        string email = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["email"]).Trim());
                        if (MPUser.IsEmailExist(email))
                        {
                            throw new MiaopassEmailConflictException();
                        }

                        var res = DB.SExecuteScalar("select token from signup_email where email=? and expire>?", email, DateTime.Now);
                        if (res == null)
                        {
                            string token = Tools.BytesToString(Guid.NewGuid().ToByteArray());

                            DB.SExecuteNonQuery("delete from signup_email where email=?", email);
                            DB.SExecuteNonQuery("insert into signup_email (email,token,expire) values (?,?,?)", email, token, DateTime.Now.AddDays(1));

                            Mail.SendConfirmSignupEmail(email, token);
                        }
                        else
                        {
                            Mail.SendConfirmSignupEmail(email, (string)res);
                        }
                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region confirm-signup-email 确认邮箱注册
                case "confirm-signup-email":
                    {
                        if (Session["user"] != null)
                        {
                            throw new MiaopassAlreadyLoginException();
                        }

                        string email = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["email"]));
                        string username = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["username"]));
                        string password = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["password"]));

                        var res = DB.SExecuteScalar("select email from signup_email where email=? and expire>?", email, DateTime.Now);
                        if (res == null)
                        {
                            throw new MiaopassException("邮箱未申请注册或注册链接已失效,请重新注册");
                        }

                        MPUser.Create(username, Tools.SHA256Hash(password), email);
                        DB.SExecuteNonQuery("delete from signup_email where email=?", email);

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region create-package 创建新图包
                case "create-package":
                    {
                        var user = Session["user"] as MPUser;
                        if (user == null)
                        {
                            throw new MiaopassNotLoginException();
                        }

                        string packageTitle = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["title"]));

                        MPPackage.Create(user.ID, packageTitle);

                        var res = DB.SExecuteScalar("select id from package where userid=? and title=?", user.ID, packageTitle);

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok", packageid = res }));

                    }
                    break;
                #endregion
                #region upload-begin 上传文件开始
                case "upload-begin":
                    {
                        if (Session["user"] == null)
                        {
                            throw new MiaopassNotLoginException();
                        }

                        string tempFileName = Tools.BytesToString(Guid.NewGuid().ToByteArray());

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok", name = tempFileName }));
                    }
                    break;
                #endregion
                #region upload-chunk 上传文件分块
                case "upload-chunk":
                    {
                        CheckLogin();

                        int chunks = Tools.GetInt32FromRequest(Request.QueryString["chunks"]);
                        int chunk = Tools.GetInt32FromRequest(Request.QueryString["chunk"]);
                        string tempFileName = Tools.GetStringFromRequest(Request.QueryString["name"]);

                        try
                        {
                            using (FileStream fs = File.Create(Server.MapPath("~/MP_Temp/" + tempFileName + "_" + chunk)))
                            {
                                fs.Write(Request.InputStream);
                            }
                        }
                        catch
                        {
                            throw new MiaopassUploadErrorException();
                        }

                        if (chunk == (chunks - 1))
                        {
                            try
                            {
                                string mergerName = Server.MapPath("~/MP_Done/" + tempFileName);
                                using (var merger = File.Create(mergerName))
                                {
                                    for (int i = 0; i < chunks; i++)
                                    {
                                        string fragmentName = Server.MapPath("~/MP_Temp/" + tempFileName + "_" + i);
                                        using (var fragment = File.OpenRead(fragmentName))
                                        {
                                            merger.Write(fragment);
                                        }
                                        File.Delete(fragmentName);
                                    }
                                    MPFile.Create(merger);
                                    Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok", hash = Tools.FileMd5(merger) }));
                                }
                                File.Delete(mergerName);
                            }
                            catch
                            {
                                throw new MiaopassUploadErrorException();
                            }
                        }
                        else
                        {
                            Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                        }
                    }
                    break;
                #endregion
                #region create-image 创建上传图片
                case "create-image":
                    {
                        MPUser user = Session["user"] as MPUser;
                        if (user == null)
                            throw new MiaopassNotLoginException();
                        int packageId = Tools.GetInt32FromRequest(Request.Form["package_id"]);
                        string md5 = Tools.GetStringFromRequest(Request.Form["file_hash"]);
                        string description = Server.HtmlDecode(Tools.GetStringFromRequest(Request.Form["description"]));

                        MPPackage package = new MPPackage(packageId);
                        MPFile file = new MPFile(md5);
                        MPImage.Create(package.ID, file.ID, user.ID, MPImageFromTypes.Upload, 0, "", description);
                        MPImage image = new MPImage(packageId, file.ID);

                        int begin = 0;
                        int end = 0;
                        while (true)
                        {
                            begin = description.IndexOf('#', end);
                            if (begin == -1)
                                break;
                            begin = begin + 1;

                            end = description.IndexOf('#', begin);
                            if (end == -1)
                                break;

                            string text = description.Substring(begin, end - begin);
                            MPTag.Create(text);

                            end = end + 1;
                        }

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region logout 退出登录
                case "logout":
                    {
                        Session["user"] = null;
                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region query-packages 查询自己的图包
                case "query-packages":
                    {
                        if (Session["user"] == null)
                            throw new MiaopassNotLoginException();

                        MPUser user = Session["user"] as MPUser;

                        List<object> list = new List<object>();
                        var res = DB.SExecuteReader("select id from package where userid=?", user.ID);
                        foreach (var item in res)
                        {
                            try
                            {
                                MPPackage p = new MPPackage(Convert.ToInt32(item[0]));
                                list.Add(new { id = p.ID, title = p.Title });
                            }
                            catch { }
                        }

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok", packages = list }));
                    }
                    break;
                #endregion
                #region reset-password 重置密码
                case "reset-password":
                    {
                        string email = Tools.GetStringFromRequest(Request.Form["email"]);
                        if (MPUser.IsEmailExist(email) == false)
                        {
                            throw new MiaopassException("无效邮箱地址");
                        }

                        string token = "";
                        var res = DB.SExecuteScalar("select token from reset_password where email=?", email);
                        if (res != null)
                        {
                            token = (string)res;
                        }
                        else
                        {
                            token = Tools.BytesToString(Guid.NewGuid().ToByteArray());
                            DB.SExecuteNonQuery("insert into reset_password (email,expire,token) values (?,?,?)", email, DateTime.Now.AddDays(1), token);
                        }

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region praise-image 赞图片
                case "praise-image":
                    {
                        var user = CheckLogin();
                        var image = new MPImage(Convert.ToInt32(Tools.GetInt32FromRequest(Request.Form["image_id"])));

                        DB.SExecuteNonQuery("insert ignore into praise (userid,type,info) values (?,?,?)", user.ID, MPPraiseTypes.Image, image.ID);

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region unpraise-image 取消赞图片
                case "unpraise-image":
                    {
                        var user = CheckLogin();
                        var image = new MPImage(Convert.ToInt32(Tools.GetInt32FromRequest(Request.Form["image_id"])));

                        DB.SExecuteNonQuery("delete from praise where userid=? and type=? and info=?", user.ID, MPPraiseTypes.Image, image.ID);

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region resave 转存图片
                case "resave":
                    {
                        var user = CheckLogin();
                        var image = new MPImage(Tools.GetInt32FromRequest(Request.Form["image_id"]));
                        var package = new MPPackage(Tools.GetInt32FromRequest(Request.Form["package_id"]));
                        var description = Tools.GetStringFromRequest(Request.Form["description"]);

                        MPImage.Create(package.ID, image.FileID, user.ID, MPImageFromTypes.Repin, image.ID, "", description);

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region pick 收集图片
                case "pick":
                    {
                        var user = CheckLogin();
                        var package = new MPPackage(Tools.GetInt32FromRequest(Request.Form["package_id"]));
                        var description = Tools.GetStringFromRequest(Request.Form["description"]);
                        var source = Tools.GetStringFromRequest(Request.Form["source"]);
                        var from = Tools.GetStringFromRequest(Request.Form["from"]);

                        DB.SExecuteNonQuery("insert into task_download(packageid,userid,source,`from`,description) values (?,?,?,?,?)", package.ID, user.ID, source, from, description);

                        Response.Write(JsonConvert.SerializeObject(new { code = 0, msg = "ok" }));
                    }
                    break;
                #endregion
                #region add-comment 添加评论
                case "add-comment":
                    {
                        DateTime lastTime = Convert.ToDateTime(Session["lastAddCommentTime"]);
                        DateTime now = DateTime.Now;
                        if ((now - lastTime).TotalSeconds < 20)
                        {
                            throw new MiaopassException("发表评论过于频繁,请于20秒后重试");
                        }
                        var user = CheckLogin();
                        int imageId = Tools.GetInt32FromRequest(Request.Form["image_id"]);
                        string text = Tools.GetStringFromRequest(Request.QueryString["text"]);

                        var image = new MPImage(imageId);

                        int commentId = MPComment.Create(image.ID, user.ID, text);
                        Response.Write(Tools.JSONStringify(new { code = 0, msg = "ok", id = commentId }));
                    }
                    break;
                #endregion
                #region delete-comment 删除评论
                case "delete-comment":
                    {
                        var user = CheckLogin();
                        int commentId = Tools.GetInt32FromRequest(Request.Form["id"]);
                        var comment = new MPComment(commentId);
                        if(comment.UserID!=user.ID)
                        {
                            throw new MiaopassException("无操作权限");
                        }

                        DB.SExecuteNonQuery("delete from comment_mention where commentid=?",commentId);
                        DB.SExecuteNonQuery("delete from comment where id=?", commentId);
                    }
                    break;
                #endregion
            }
        }
        catch (MiaopassException exception)
        {
            Response.Write(JsonConvert.SerializeObject(new { code = exception.Code, msg = exception.Message }));
        }

        Response.End();
    }
}