using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class MPComment
{
    public int ID { get; set; }
    public int ImageID { get; set; }
    public int UserID { get; set; }
    public string Text { get; set; }
    public DateTime CreatedTime { get; set; }
    public MPComment(int id)
    {
        var res = DB.SExecuteReader("select id,imageid,userid,`text`,createdtime from comment where id=?", id);
        if (res.Count == 0)
        {
            throw new MiaopassException("没有该评论或评论已经删除");
        }

        var row = res[0];
        ID = Convert.ToInt32(row[0]);
        ImageID = Convert.ToInt32(row[1]);
        UserID = Convert.ToInt32(row[2]);
        Text = (string)row[3];
        CreatedTime = Convert.ToDateTime(row[4]);
    }

    public static int Create(int imageid, int userid, string text)
    {
        int id = DB.SInsert("insert into comment (imageid,userid,`text`) values (?,?,?)", imageid, userid, text);
        int pos = 0;
        while (true)
        {
            pos = text.IndexOf('@', pos);
            if (pos == -1)
                break;

            int end = text.IndexOf(' ', pos);
            int length = end - pos;

            string name = text.Substring(pos + 1, length - 1);
            try
            {
                var user = new MPUser(name, MPUserConstructTypes.Username);
                MPCommentMention.Create(id, user.ID, pos, length);
            }
            catch (MiaopassException) { }

            pos = end + 1;
        }
        return id;
    }
}

public class MPCommentMention
{
    public int ID { get; set; }
    public int CommentID { get; set; }
    public int UserID { get; set; }
    public int Position { get; set; }
    public int Length { get; set; }
    public MPCommentMention(int id)
    {
        var res = DB.SExecuteReader("select id,commentid,userid,position,length from comment_mention where id=?", id);
        if (res.Count==0)
        {
            throw new MiaopassException("没有@这个人");
        }

        var row=res[0];
        ID = Convert.ToInt32(row[0]);
        CommentID = Convert.ToInt32(row[1]);
        UserID = Convert.ToInt32(row[2]);
        Position = Convert.ToInt32(row[3]);
        Length = Convert.ToInt32(row[4]);
    }

    public static void Create(int commentId, int userId, int position, int length)
    {
        DB.SExecuteNonQuery("insert into comment_mention (commentid,userid,position,length) values (?,?,?,?)", commentId, userId, position, length);
    }
}