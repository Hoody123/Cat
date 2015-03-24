using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

public partial class following : MPPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var user = Session["user"] as MPUser;
        if (user == null)
            Response.Redirect("~/login.aspx");

        Ajax();
        MPData.images = GetData();
    }

    void Ajax()
    {
        if (Request.QueryString["max"] != null)
        {
            try
            {
                int max = Tools.GetInt32FromRequest(Request.QueryString["max"]);
                Response.Write(Tools.JSONStringify (GetData(max)));
                Response.End();
            }
            catch { }
        }
    }

    List<object> GetData(int max=0, int limit = 30)
    {
        MPUser user = Session["user"] as MPUser;
        if (max == 0)
            max = int.MaxValue;
        var res = DB.SExecuteReader("select id from image where id<? and (packageid in (select info from following where userid=? and type=?) or userid in (select info from following where userid=? and type=?) or userid=?) order by id desc limit ?", max, user.ID,MPFollowingTypes.Package,user.ID,MPFollowingTypes.User,user.ID, limit);
        List<object> dataList = new List<object>();
        foreach (var item in res)
        {
            try
            {
                dataList.Add(new JSON.ImageDetail(new MPImage(Convert.ToInt32(item[0])),user));
            }
            catch { }
        }
        return dataList;
    }
}