using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

public partial class all_aspx : MPPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Ajax();
        MPData.images = GetData();
        Title = "最新图片_喵帕斯";
        MetaKeywords = "动漫 二次元 图片 图包 ";
        MetaDescription = "喵帕斯是一个专注于动漫图片的收集和分享的网站";
    }

    void Ajax()
    {
        if (Request.QueryString["max"] != null)
        {
            try
            {
                int max = Tools.GetInt32FromRequest(Request.QueryString["max"]);
                Response.Write(Tools.JSONStringify(GetData(max)));
                Response.End();
            }
            catch { }
        }
    }

    List<object> GetData(int max=0, int limit = 10)
    {
        MPUser user = Session["user"] as MPUser;
        if (max == 0)
            max = int.MaxValue;
        var res = DB.SExecuteReader("select id from image where id<? order by id desc limit ?", max, limit);
        List<object> dataList = new List<object>();
        foreach (var item in res)
        {
            try
            {
                dataList.Add(new JSON.ImageDetail(new MPImage(Convert.ToInt32(item[0])), user));
            }
            catch { }
        }
        return dataList;
    }
}