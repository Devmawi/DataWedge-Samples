using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutomaricImageCaptureDemoApp
{
    // https://techdocs.zebra.com/datawedge/latest/guide/programmers-guides/workflow-input/
    public class ImageProcessing
    {

        private String IMG_FORMAT_YUV = "YUV";
        private String IMG_FORMAT_Y8 = "Y8";

    private static ImageProcessing instance = null;

        public static ImageProcessing GetInstance()
        {

            if (instance == null)
            {
                instance = new ImageProcessing();
            }
            return instance;
        }

        private ImageProcessing()
        {
            //Private Constructor
        }

        public Bitmap GetBitmap(byte[] data, String imageFormat, int orientation, int stride, int width, int height)
        {
            if (imageFormat.ToUpper() == IMG_FORMAT_YUV)
            {
                MemoryStream outStream = new MemoryStream();
                int uvStride = ((stride + 1) / 2) * 2;  // calculate UV channel stride to support odd strides
                YuvImage yuvImage = new YuvImage(data, ImageFormatType.Nv21, width, height, new int[] { stride, uvStride });


                yuvImage.CompressToJpeg(new Rect(0, 0, stride, height), 100, outStream);
                yuvImage.GetYuvData();
                byte[] imageBytes = outStream.ToArray();

                // Save Image here
                string documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                string localFilename = $"Snapshot_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.jpg";
                string localPath = System.IO.Path.Combine(documentsPath, localFilename);
                if (System.IO.File.Exists(localPath))
                    System.IO.File.Delete(localPath);
                System.IO.File.WriteAllBytes(localPath, imageBytes);

                if (orientation != 0)
                {
                    Matrix matrix = new Matrix();
                    matrix.PostRotate(orientation);
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                }
                else
                {
                    return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            else if (imageFormat.ToUpper() == IMG_FORMAT_Y8)
            {
                return ConvertYtoJPG_CPU(data, orientation, stride, height);
            }

            return null;
        }


        private Bitmap ConvertYtoJPG_CPU(byte[] data, int orientation, int stride, int height)
        {
            int mLength = data.Length;
            int[] pixels = new int[mLength];
            for (int i = 0; i < mLength; i++)
            {
                long p = data[i] & 0xFF;
                // pixels[i] = 0xff000000 | p << 16 | p << 8 | p;
                pixels[i] = (int)(0xff000000 | p << 16 | p << 8 | p);
            }
            if (orientation != 0)
            {
                Matrix matrix = new Matrix();
                matrix.PostRotate(orientation);
                Bitmap bitmap = Bitmap.CreateBitmap(pixels, stride, height, Bitmap.Config.Argb8888);
                return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
            }
            else
            {
                return Bitmap.CreateBitmap(pixels, stride, height, Bitmap.Config.Argb8888);
            }
        }
    }
}