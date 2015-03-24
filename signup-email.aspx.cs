using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class signup_email : MPPage
{
    public string Email { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        string token = RouteData.Values["token"] as string;

        var res = DB.SExecuteScalar("select email from signup_email where token=? and expire>?", token, DateTime.Now);
        if (res != null)
        {
            Email = (string)res;
        }
        else
        {
            Response.StatusCode = 404;
            Response.End();
        }

        if (Request.HttpMethod == "POST")
        {
            try
            {
                string username =Tools.GetStringFromRequest( Request.Form["username"]);
                string password = Tools.GetStringFromRequest(Request.Form["password"]);

                int id= MPUser.Create(username, password, Email);
                Session["user"] = new MPUser(id);

                DB.SExecuteNonQuery("delete from signup_email where token=?", token);

                Response.Write(Tools.JSONStringify(new { code = 0, msg = "ok" }));
            }
            catch (MiaopassException exception)
            {
                Response.Write(Tools.JSONStringify(new { code = exception.Code, msg = exception.Message }));
            }

            Response.End();
        }
    }
}