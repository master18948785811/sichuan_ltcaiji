/*
 *  Author : Chusong Zhuang
 *  Date : 2021/7/26 9:27:19
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// 对 <see cref="Bitmap"/> 的拓展。
/// </summary>
public static class BitmapEx
{
    /// <summary>
    /// 将 <see cref="Bitmap"/> 转换成 <see cref="byte"/>[]。
    /// </summary>
    /// <param name="bmp">一个 <see cref="Bitmap"/>。</param>
    /// <param name="imageFormat">图像格式，默认是 <see cref="ImageFormat.Jpeg"/></param>
    /// <returns>成功返回对应 <see cref="byte"/>[]，反之，返回 <see langword="null"/>。</returns>
    public static byte[] ToByteArray(this Bitmap bmp, ImageFormat imageFormat = null)
    {
        if (bmp == null) return null;

        using (var ms = new MemoryStream())
        {
            bmp.Save(ms, imageFormat ?? ImageFormat.Jpeg);
            return ms.GetBuffer();
        }
    }

    /// <summary>
    /// 将 <see cref="Bitmap"/> 转换成 Base64 字符串。
    /// </summary>
    /// <param name="bmp">一个 <see cref="Bitmap"/>。</param>
    /// <param name="imageFormat">图像格式，默认是 <see cref="ImageFormat.Jpeg"/></param>
    /// <returns>成功返回对应字符串，反之，返回 <see langword="null"/>。</returns>
    public static string ToBase64String(this Bitmap bmp, ImageFormat imageFormat = null)
    {
        if (bmp != null)
        {
            try
            {
                if (imageFormat != null)
                {
                    var temp = Path.GetTempFileName();
                    bmp.Save(temp, imageFormat);

                    var bytes = File.ReadAllBytes(temp);
                    File.Delete(temp);

                    return Convert.ToBase64String(bytes);
                }
                else
                {
                    return Convert.ToBase64String(bmp.ToByteArray());
                }
            }
            catch
            {
            }
        }
        return null;
    }

    /// <summary>
    /// 裁剪图片至指定矩形。
    /// </summary>
    /// <param name="bmp">原图。</param>
    /// <param name="rect">新矩形。</param>
    /// <returns>成功返回裁剪的图片，失败返回原图。</returns>
    public static Bitmap Crop(this Bitmap bmp, Rectangle rect)
    {
        try
        {
            if (bmp == null) return null;

            var b = new Bitmap(rect.Width, rect.Height);
            var g = Graphics.FromImage(b);

            g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), new Rectangle(rect.X + bmp.Width / 2 - rect.Width / 2, rect.Y, rect.Width, rect.Height), GraphicsUnit.Pixel);
            g.Dispose();

            return b;
        }
        catch
        {
        }
        return bmp;
    }

    private static Bitmap BuiltGrayBitmap(byte[] rawValues, int width, int height)
    {
        // 新建一个8位灰度位图，并锁定内存区域操作  
        Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
             ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

        // 计算图像参数  
        int offset = bmpData.Stride - bmpData.Width;        // 计算每行未用空间字节数  
        IntPtr ptr = bmpData.Scan0;                         // 获取首地址  
        int scanBytes = bmpData.Stride * bmpData.Height;    // 图像字节数 = 扫描字节数 * 高度  
        byte[] grayValues = new byte[scanBytes];            // 为图像数据分配内存  

        // 为图像数据赋值  
        int posSrc = 0, posScan = 0;                        // rawValues和grayValues的索引  
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                grayValues[posScan++] = rawValues[posSrc++];
            }
            // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
            posScan += offset;
        }

        // 内存解锁  
        Marshal.Copy(grayValues, 0, ptr, scanBytes);
        bitmap.UnlockBits(bmpData);  // 解锁内存区域  

        // 修改生成位图的索引表，从伪彩修改为灰度  
        ColorPalette palette;
        // 获取一个Format8bppIndexed格式图像的Palette对象  
        using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
        {
            palette = bmp.Palette;
        }
        for (int i = 0; i < 256; i++)
        {
            palette.Entries[i] = Color.FromArgb(i, i, i);
        }
        // 修改生成位图的索引表  
        bitmap.Palette = palette;

        return bitmap;
    }

    /// <summary>
    /// 转灰度图。
    /// </summary>
    /// <param name="original">原始图像。</param>
    /// <returns>返回灰度图。</returns>
    public static Bitmap ToGrayScale(this Bitmap original)
    {
        if (original != null)
        {
            // 将源图像内存区域锁定  
            var rect = new Rectangle(0, 0, original.Width, original.Height);
            var bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                 original.PixelFormat);

            // 获取图像参数  
            int width = bmpData.Width;
            int height = bmpData.Height;
            int stride = bmpData.Stride;  // 扫描线的宽度  
            int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙  
            IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置  
            int scanBytes = stride * height;  // 用stride宽度，表示这是内存区域的大小  

            // 分别设置两个位置指针，指向源数组和目标数组  
            int posScan = 0, posDst = 0;
            byte[] rgbValues = new byte[scanBytes];  // 为目标数组分配内存  
            Marshal.Copy(ptr, rgbValues, 0, scanBytes);  // 将图像数据拷贝到rgbValues中  
                                                         // 分配灰度数组  
            byte[] grayValues = new byte[width * height]; // 不含未用空间。  
                                                          // 计算灰度数组  
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double temp = rgbValues[posScan++] * 0.11 +
                        rgbValues[posScan++] * 0.59 +
                        rgbValues[posScan++] * 0.3;
                    grayValues[posDst++] = (byte)temp;
                }
                // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
                posScan += offset;
            }

            // 内存解锁  
            Marshal.Copy(rgbValues, 0, ptr, scanBytes);
            original.UnlockBits(bmpData);  // 解锁内存区域  

            // 构建8位灰度位图  
            return BuiltGrayBitmap(grayValues, width, height);
        }
        else
        {
            return null;
        }
    }
}