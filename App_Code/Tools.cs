using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;

public static class Tools
{
    public static string ImageServerHost = ConfigurationManager.AppSettings["BaseUrl"];
    public static string Host = ConfigurationManager.AppSettings["Host"];
    public static string MiaopassString = " - 喵帕斯~(*´▽｀)ノ";

    public static string JSONStringify(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
    public static byte[] Md5(string str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        return MD5.Create().ComputeHash(buffer);
    }

    public static string SHA256Hash(string str)
    {
        return BytesToString(SHA256.Create().ComputeHash(UTF8Encoding.UTF8.GetBytes(str)));
    }

    public static string ComputeFileSize(int size)
    {
        if (size < 1.0 * 1024 * 0.9)
            return size + "字节";
        else if (size < 1.0 * 1024 * 1024 * 0.9)
            return (int)(1.0 * size / 1024) + "KB";
        else if (size < 1.0 * 1024 * 1024 * 1024 * 0.9)
            return (int)(1.0 * size / 1024 / 1024) + "MB";
        else if (size < 1.0 * 1024 * 1024 * 1024 * 1024 * 0.9)
            return (int)(1.0 * size / 1024 / 1024 / 1024) + "GB";
        else
            return (int)(1.0 * size / 1024 / 1024 / 1024 / 1024) + "TB";
    }

    public static string FileMd5(string path)
    {
        using (var fs = File.OpenRead(path))
        {
            return FileMd5(fs);
        }
    }

    public static string FileMd5(Stream s)
    {
        s.Position = 0;
        return BytesToString(MD5.Create().ComputeHash(s));
    }

    public static byte[] StringToBytes(string ss)
    {
        byte[] bytes = new byte[16];
        try
        {
            for (int i = 0; i < 16; i++)
            {
                bytes[i] = Convert.ToByte(ss.Substring(i * 2, 2), 16);
            }
        }
        catch
        {
            throw new Exception("无法转换");
        }

        return bytes;
    }
    public static string BytesToString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    public static string GetStringFromRequest(string request)
    {
        if (request == null)
            throw new MiaopassParamsErrorException();
        return HttpContext.Current.Server.HtmlDecode(request).Trim();
    }

    public static int GetInt32FromRequest(string request)
    {
        if (request == null)
            throw new MiaopassParamsErrorException();
        try
        {
            return Convert.ToInt32(request);
        }
        catch
        {
            throw new MiaopassParamsErrorException();
        }
    }

    public static int GetId(byte[] token, DB db)
    {
        object o = db.ExecuteScalar("select id from t_user where token=?", token);
        if (o == null)
        {
            return 0;
        }
        else
        {
            db.ExecuteNonQuery("update t_user set last_access_time=now() where id =?", Convert.ToInt32(o));
            return Convert.ToInt32(o);
        }
    }
    public static void Write(this Stream stream, Stream input)
    {
        int bufferSize = 1024 * 4;
        int a = bufferSize;
        byte[] buffer = new byte[bufferSize];
        while (a == bufferSize)
        {
            a = input.Read(buffer, 0, bufferSize);
            stream.Write(buffer, 0, a);
        }
    }

    //扩展Image
    /// <summary>
    /// 截取方形图像
    /// </summary>
    public static Image Square(this Image img,int size)
    {
        int top, left, s;
        if(img.Width>img.Height)
        {
            s = img.Height;
            top = 0;
            left = (img.Width - img.Height) / 2;
        }
        else
        {
            s = img.Width;
            left = 0;
            top = (img.Height - img.Width) / 2;
        }

        Image desc = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(desc))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, new Rectangle(0, 0, size, size), new Rectangle(left, top, s, s), GraphicsUnit.Pixel);
        }
        return desc;
    }

    /// <summary>
    /// 高质量缩小图像尺寸,保证缩小后不模糊
    /// </summary>
    public static Image FixWidth(this Image img, int width)
    {
        if (width >= img.Width)
            return new Bitmap(img);

        int height = (int)(1.0 * width * img.Height / img.Width);
        Image desc = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(desc))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.DrawImage(img, new Rectangle(0, 0, desc.Width, desc.Height), new Rectangle(0,0,img.Width,img.Height), GraphicsUnit.Pixel);
        }
        return desc;
    }

    public static Stream SaveAsJpeg(this Image img, int quality = 90)
    {
        MemoryStream ms = new MemoryStream();
        ImageCodecInfo myImageCodecInfo;
        System.Drawing.Imaging.Encoder myEncoder;
        EncoderParameter myEncoderParameter;
        EncoderParameters myEncoderParameters;

        myImageCodecInfo = GetEncoderInfo("image/jpeg");

        myEncoder = System.Drawing.Imaging.Encoder.Quality;
        myEncoderParameters = new EncoderParameters(1);

        myEncoderParameter = new EncoderParameter(myEncoder, quality);
        myEncoderParameters.Param[0] = myEncoderParameter;

        img.Save(ms, myImageCodecInfo, myEncoderParameters);

        return ms;
    }

    private static ImageCodecInfo GetEncoderInfo(String mimeType)
    {
        int j;
        ImageCodecInfo[] encoders;
        encoders = ImageCodecInfo.GetImageEncoders();
        for (j = 0; j < encoders.Length; ++j)
        {
            if (encoders[j].MimeType == mimeType)
                return encoders[j];
        }
        return null;
    }
}
