using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class image_aspx : MPPage
{
    bool IsSpider()
    {
        var agent = Request.UserAgent.ToLower();
        if (agent.Contains("baiduspider") || agent.Contains("googlebot") || agent.Contains("360spider"))
            return true;
        return false;
    }

    public int NextID { get; set; }
    public int PrevID { get; set; }

    protected void Page_Load(object sender, EventArgs e)
    {
        MPImage image = null;
        try
        {
            int imageid = Tools.GetInt32FromRequest(RouteData.Values["id"] as string);
            image = new MPImage(imageid);
        }
        catch
        {
            Response.StatusCode = 404;
            Response.End();
        }

        var imageDetail = new JSON.ImageDetail(image, Session["user"] as MPUser);

        if (Request.QueryString["ajax"] != null)
        {
            Response.Write(Tools.JSONStringify(imageDetail));
            Response.End();
            return;
        }

        MPData.image = imageDetail;
        string keywords = image.Description.Length > 20 ? image.Description.Substring(0, 20) + "..." : image.Description;
        MetaKeywords = keywords;
        MetaDescription = image.Description;
        Title = keywords + "@" + imageDetail.user.name + "收集到" + imageDetail.package.title + "_喵帕斯";

        if(IsSpider())
        {
            PrevID=Convert.ToInt32( DB.SExecuteScalar("select id from image where id<? limit 1", image.ID));
            NextID = Convert.ToInt32(DB.SExecuteScalar("select id from image where id>? limit 1", image.ID));
        }

    }
}