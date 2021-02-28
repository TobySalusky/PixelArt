using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public static class Clipboard {

        public static Texture2D getTexture() {
            Image image = System.Windows.Forms.Clipboard.GetImage();

            if (image == null) return null;

            MemoryStream mem = new MemoryStream();
            image.Save(mem, ImageFormat.Bmp);
            
            return Texture2D.FromStream(Main.getGraphicsDevice(), mem);
        }
        
        public static Layer getLayer() {
            IDataObject dataObj = System.Windows.Forms.Clipboard.GetDataObject();
            string format = typeof(ClipboardLayer).FullName;
    
            if (dataObj == null) return null;
            
            if(dataObj.GetDataPresent(format)) {
                ClipboardLayer clip = (ClipboardLayer) dataObj.GetData(format);
                return new Layer(clip);
            }
            return null;
        }
    }
}