using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Replacement.Models
{
    public class Replacement
    {

        private String str { get; set; }
        private Image img { get; }


        public Replacement(String text, Font font, Color textColor, Color backColor)
        {
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
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            img = newImg;
        }
    }
}
