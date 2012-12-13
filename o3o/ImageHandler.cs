using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Net;
using System.Drawing;

namespace o3o
{
    class ImageHandler
    {
        public Dictionary<decimal, string> ImageCache;
        public Dictionary<decimal, BitmapImage> MemoryCache = new Dictionary<decimal, BitmapImage>();

        public ImageHandler()
        {
            if (File.Exists("ImageCache.bin"))
            {
                LoadCache();
            }
            else
            {
                ImageCache = new Dictionary<decimal, string>();
            }

            if(!Directory.Exists("Cache"))
            {
                Directory.CreateDirectory("Cache");
            }
           
        }

        ~ImageHandler()
        {
            SaveCache();
        }


        public void LoadCache()
        {
            Stream s = File.Open("ImageCache.bin", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            ImageCache = (Dictionary<decimal, string>)bf.Deserialize(s);
            s.Close();
        }

        public void SaveCache()
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream s = File.Open("ImageCache.bin", FileMode.Create);
            bf.Serialize(s, ImageCache);
            s.Close();
        }

        public void StoreImage(BitmapImage image, string imagename, decimal userID)
        {
           
            string path = Directory.GetCurrentDirectory() + "\\Cache\\" + imagename + ".png";
            ImageCache.Add(userID, path);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        public ImageSource FetchImage(string imageloc, decimal id)
        {
            BitmapImage bit = new BitmapImage(new Uri(imageloc));
            MemoryCache.Add(id, bit);
            return (ImageSource)bit;
        }

        public ImageSource GetImage(decimal UserId, string ImageLocation)
        {
            if (MemoryCache.ContainsKey(UserId))
            {
                return MemoryCache[UserId];
            }
            else
            {
                if (ImageCache.ContainsKey(UserId))
                {
                    string Imagelocation = ImageCache[UserId];
                    return FetchImage(Imagelocation, UserId);
                }
                else
                {
                    BitmapImage newimage;

                    if (ImageLocation.Length > 0)
                    {
                        try
                        {
                            int BytesToRead = 100;
                            WebRequest request = WebRequest.Create(new Uri(ImageLocation));
                            request.Timeout = -1;
                            WebResponse response = request.GetResponse();
                            Stream responseStream = response.GetResponseStream();
                            BinaryReader reader = new BinaryReader(responseStream);
                            MemoryStream memoryStream = new MemoryStream();

                            byte[] bytebuffer = new byte[BytesToRead];
                            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                            while (bytesRead > 0)
                            {
                                memoryStream.Write(bytebuffer, 0, bytesRead);
                                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                            }
                            BitmapImage _image = new BitmapImage();
                            _image.BeginInit();
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            _image.StreamSource = memoryStream;
                            _image.EndInit();



                            newimage = _image;

                            request = null;
                            response = null;
                            responseStream = null;
                            reader = null;
                            memoryStream = null;
                            bytebuffer = null;
                            bytesRead = 0;
                            BytesToRead = 0;
                        }
                        catch (Exception)
                        {
                            //newimage = new BitmapImage(new Uri("/o3o;component/Images/image_Failed.png", UriKind.Relative));
                            newimage = tobitmapimage( new Bitmap(o3o.Properties.Resources.image_Failed) );
                        }
                    }
                    else
                    {
                        newimage = tobitmapimage(new Bitmap(o3o.Properties.Resources.image_Failed));
                    }
                    StoreImage(newimage, UserId.ToString(), UserId);
                    MemoryCache.Add(UserId, newimage);
                    return (ImageSource)newimage;
                }
                
            }


        }

        private BitmapImage tobitmapimage(System.Drawing.Image img)
        {
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage newim = new BitmapImage();
            img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            newim.BeginInit();
            newim.StreamSource = memoryStream;
            newim.EndInit();
            return newim;
        }

    }
}
