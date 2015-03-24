using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

/// <summary>
/// MPPage 的摘要说明
/// </summary>
public class MPPage : System.Web.UI.Page
{
    public dynamic MPData { get; set; }
	public MPPage()
	{
        MPData = new ExpandoObject();
        Load += MPPage_Load;
	}

    void MPPage_Load(object sender, EventArgs e)
    {
        var user = Session["user"] as MPUser;
        if (user != null)
        {
            MPData.user =new JSON.User(user);
        }
        else
        {
            MPData.user = new JSON.User(new MPUser(0));
        }
        
    }
    
}