using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class package_aspx : MPPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        MPPackage package = null;
        try
        {
            int packageid = Tools.GetInt32FromRequest(RouteData.Values["id"] as string);
            package = new MPPackage(packageid);
        }
        catch
        {
            Response.StatusCode = 404;
            Response.End();
        }

        int limit = 10;
        int max = Convert.ToInt32(Request.QueryString["max"]);
        if (max == 0)
            max = Int32.MaxValue;
        string sub1 = RouteData.Values["sub1"] as string;

        var list = new List<object>();
        switch(sub1)
        {
            case "follower":
                {
                    var res = DB.SExecuteReader("select userid from following where type=? and info=? and id<? order by id limit ?", MPFollowingTypes.Package, package.UserID, max, limit);
                    foreach (var item in res)
                    {
                        list.Add(new JSON.UserDetail(new MPUser(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                    }
                }
                break;
            default:
                {
                    var res = DB.SExecuteReader("select id from image where id<? and packageid=? order by id desc limit ?", max, package.ID, limit);
                    if (Request.QueryString["simple"] != null && Request.QueryString["ajax"] != null)
                    {
                        foreach (var item in res)
                        {
                            list.Add(new JSON.Image(new MPImage(Convert.ToInt32(item[0]))));
                        }
                    }
                    else
                    {
                        foreach (var item in res)
                        {
                            list.Add(new JSON.ImageDetail(new MPImage(Convert.ToInt32(item[0])), Session["user"] as MPUser));
                        }
                    }
                }
                break;
        }
        

        if(Request.QueryString["ajax"]!=null)
        {
            Response.Write(Tools.JSONStringify (list));
            Response.End();
            return;
        }

        var packageDetail=new JSON.PackageDetail(package, Session["user"] as MPUser);
        MPData.package =packageDetail;
        MPData.datas = list;
        string title = package.Title.Length > 20 ? package.Title.Substring(0, 20) + "..." : package.Title;
        Title = string.Format("{0}@{1}收集_喵帕斯", title, packageDetail.user.name);
        MetaDescription = package.Description;
        MetaKeywords = package.Title;
    }
}