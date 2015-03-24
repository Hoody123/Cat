using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class user_aspx : MPPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sub1 = RouteData.Values["sub1"] == null ? "" : RouteData.Values["sub1"] as string;
        string sub2 = RouteData.Values["sub2"] == null ? "" : RouteData.Values["sub2"] as string;

        int id = Convert.ToInt32(RouteData.Values["id"]);

        MPUser pageUser = null;
        try
        {
            pageUser = new MPUser(id);
        }
        catch (MiaopassException)
        {
            Response.StatusCode = 404;
            Response.End();
        }

        int max = 0;
        try
        {
            max = Tools.GetInt32FromRequest(Request.QueryString["max"]);
        }
        catch { }

        if (max == 0)
        {
            max = Int32.MaxValue;
        }

        int limit = 10;
        var datas = new List<object>();

        switch (sub1)
        {
            case "image":
                {
                    var res = DB.SExecuteReader("select id from image where userid=? and id<? order by id desc limit ?", pageUser.ID, max, limit);
                    foreach (var item in res)
                    {
                        datas.Add(new JSON.ImageDetail(new MPImage(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                    }
                }
                break;
            case "praise":
                {
                    switch (sub2)
                    {
                        case "package":
                            {
                                var res = DB.SExecuteReader("select info from praise where userid=? and type=? and id<? order by id desc limit ?", pageUser.ID, MPPraiseTypes.Package, max, limit);
                                foreach (var item in res)
                                {
                                    datas.Add(new JSON.PackageDetail(new MPPackage(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                                }
                            }
                            break;
                        default:
                            {
                                var res = DB.SExecuteReader("select info from praise where userid=? and type=? and id<? order by id desc limit ?", pageUser.ID, MPPraiseTypes.Image, max, limit);
                                foreach (var item in res)
                                {
                                    datas.Add(new JSON.ImageDetail(new MPImage(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                                }
                            }
                            break;
                    }
                }
                break;
            case "following":
                {
                    switch (sub2)
                    {
                        case "package":
                            {
                                var res = DB.SExecuteReader("select info from following where userid=? and type=? and id<? order by id desc limit ?", pageUser.ID, MPFollowingTypes.Package, max, limit);
                                foreach (var item in res)
                                {
                                    datas.Add(new JSON.PackageDetail(new MPPackage(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                                }
                            }
                            break;
                        default:
                            {
                                var res = DB.SExecuteReader("select info from following where userid=? and type=? and id<? order by id desc limit ?", pageUser.ID, MPFollowingTypes.User, max, limit);
                                foreach (var item in res)
                                {
                                    datas.Add(new JSON.UserDetail(new MPUser(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                                }
                            }
                            break;
                    }
                }
                break;
            case "follower":
                {
                    var res = DB.SExecuteReader("select userid from following where info=? and type=? and id<? order by id desc limit ?", pageUser.ID, MPFollowingTypes.User, max, limit);
                    foreach (var item in res)
                    {
                        datas.Add(new JSON.UserDetail(new MPUser(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                    }
                }
                break;
            default:
                {
                    var res = DB.SExecuteReader("select id from package where userid=? and id<? order by id desc limit 10", pageUser.ID, max);
                    foreach (var item in res)
                    {
                        datas.Add(new JSON.PackageDetail(new MPPackage(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                    }
                }
                break;
        }

        if (Request.QueryString["ajax"] != null)
        {
            Response.Write(Tools.JSONStringify(datas));
            Response.End();
            return;
        }

        MPData.page_user =new JSON.UserDetail(pageUser, Session["user"] as MPUser);
        MPData.sub1 = sub1;
        MPData.sub2 = sub2;
        MPData.datas = datas;
        Title = string.Format("{0}的主页_喵帕斯", pageUser.Name);
        MetaKeywords = string.Format("{0}收集的图片,图包",pageUser.Name);
        MetaDescription = pageUser.Description;
    }

}