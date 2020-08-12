using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Replacement.Models;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Drawing;
using System.Web;
using System.Drawing.Text;

namespace Replacement.Controllers
{
  
    public class HomeController : Controller
    {
        private static string keyFileName = "symmetric_key.config";

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Encode(string text)  {

             GenerateKey();

            return Json(new { url = $"/home/image?text={HttpUtility.UrlEncode(Convert.ToBase64String(EncryptData(text)))}"});
        }

        public IActionResult Image(string text)
        {


            byte[] bytes = Convert.FromBase64String(text);
          
            string decodeStr = DecryptData(bytes);

            Image img = CreateImg(decodeStr);
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(img, typeof(byte[]));

            return new FileContentResult(xByte, "image/png");
        }




        public static Image CreateImg(string text)
        {

            Font font = new Font("Arial", 11);
          
            //first, create a dummy bitmap just to get a graphics object
            Image newImg = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(newImg);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            newImg.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            newImg = new Bitmap((int)textSize.Width, (int)textSize.Height);


            drawing = Graphics.FromImage(newImg);
        
            //paint the background

            drawing.Clear(Color.FromArgb(255, 255, 255));

            //create a brush for the text
            Brush textBrush = new SolidBrush(Color.FromArgb(0,0,0));
           

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return newImg;
        }





        public static void GenerateKey()
        {
            // Создать алгоритм
            SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create("DES");
            Algorithm.GenerateKey();

            // Получить ключ
            byte[] Key = Algorithm.Key;

     

            // Сохранить ключ в файле key.config
            using (FileStream fs = new FileStream(keyFileName, FileMode.Create))
            {
                fs.Write(Key, 0, Key.Length);
            }
        }


        public static void ReadKey(SymmetricAlgorithm algorithm)
        {
            byte[] Key;

            using (FileStream fs = new FileStream(keyFileName, FileMode.Open))
            {
                Key = new byte[fs.Length];
                fs.Read(Key, 0, (int)fs.Length);
            }

                algorithm.Key = Key;
        }

        public static byte[] EncryptData(string data)
        {
            // Преобразовать строку data в байтовый массив
            byte[] ClearData = Encoding.UTF8.GetBytes(data);

            // Создать алгоритм шифрования
            SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create("DES");
            ReadKey(Algorithm);

            // Зашифровать информацию
            MemoryStream Target = new MemoryStream();

            // Сгенерировать случайный вектор инициализации (IV)
            // для использования с алгоритмом
            Algorithm.GenerateIV();
            Target.Write(Algorithm.IV, 0, Algorithm.IV.Length);

            // Зашифровать реальные данные
            CryptoStream cs = new CryptoStream(Target, Algorithm.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(ClearData, 0, ClearData.Length);
            cs.FlushFinalBlock();

            // Вернуть зашифрованный поток данных в виде байтового массива
            return Target.ToArray();
        }

        public static string DecryptData(byte[] data)
        {
            // Создать алгоритм
            SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create("DES");
            ReadKey(Algorithm);

            // Расшифровать информацию
            MemoryStream Target = new MemoryStream();

            // Прочитать вектор инициализации (IV)
            // и инициализировать им алгоритм
            int ReadPos = 0;
            byte[] IV = new byte[Algorithm.IV.Length];
            Array.Copy(data, IV, IV.Length);
            Algorithm.IV = IV;
            ReadPos += Algorithm.IV.Length;

            CryptoStream cs = new CryptoStream(Target, Algorithm.CreateDecryptor(),
                CryptoStreamMode.Write);
            cs.Write(data, ReadPos, data.Length - ReadPos);
            cs.FlushFinalBlock();

            // Получить байты из потока в памяти и преобразовать их в текст
            return Encoding.UTF8.GetString(Target.ToArray());
        }

    }
}
