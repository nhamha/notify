using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ServerCore.PortalAPI.CaptchaNew;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace ServerCore.Utilities.Captcha
{
    public class Captcha
    {
        private readonly IDistributedCache _cache;

        public Captcha(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Sinh mã captcha
        /// </summary>
        /// <returns>
        ///     string[]
        ///     [0]: verify
        ///     [1]: image data contain captcha text
        /// </returns>

        public string[] GetCaptcha(int length, int width, int height, string oldVerify = "")
        {
            string verify = string.Empty;
            string captchaText = string.Empty;

            if (length > 0)
            {
                string token = Security.Security.GetVerifyToken(ref verify, ref captchaText, length);
                var cacheEntryOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3));

                _cache.SetString(verify, token, cacheEntryOptions);

                var imageData = ImageGenarateStyte2(captchaText, width, height);
                return new string[] { verify, imageData };
            }

            return null;
        }

        public CaptchaModel GetCaptcha(int length, int width, int height)
        {
            if (length > 0)
            {
                string verify = string.Empty;
                string captchaText = string.Empty;

                string token = Security.Security.GetVerifyToken(ref verify, ref captchaText, length);
                var cacheEntryOptions = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.SetString(verify, token, cacheEntryOptions);

                var imageData = ImageGenarateStyte2(captchaText, width, height);
                return new CaptchaModel(verify, imageData);
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của Captcha
        /// </summary>
        /// <param name="captchaText"></param>
        /// <param name="verify"></param>
        /// <returns>
        ///  0: Captcha hợp lệ
        /// -1: Captcha không chính xác
        /// -2: Captcha hết hạn
        /// 
        /// -99: Exception -> Captcha không hợp lệ
        /// </returns>
        public int VerifyCaptcha(string captchaText, string verify)
        {
            //NLogManager.Info("VerifyCaptcha: " + captchaText + ":" + verify);
            try
            {
                if (string.IsNullOrEmpty(captchaText) || string.IsNullOrEmpty(verify))
                    return -1;

                var saveToken = _cache.GetString(verify);
                if (string.IsNullOrEmpty(saveToken))
                {
                    NLogManager.Info("saveToken null");
                    return -1;
                }

                var time = Security.Security.GetTokenTime(verify);
                if (DateTime.Compare(time.AddMinutes(30), DateTime.Now) < 0)
                {
                    //remove expired captcha
                    _cache.Remove(verify);
                    return -1;
                }
                //string captchaDes = System.Web.HttpUtility.UrlEncode(Security.Security.TripleDESEncrypt(Security.Security.MD5Encrypt(Environment.MachineName), captchaText.ToUpper()));
                if (saveToken.Equals(Security.Security.MD5Encrypt(captchaText.ToUpper())))
                {
                    //if verified then remove captcha
                    _cache.Remove(verify);
                    return (int)ErrorCodes.SUCCESS;
                }

                _cache.Remove(verify);
                return -1;
            }
            catch (Exception exception)
            {
                _cache.Remove(verify);
                NLogManager.Exception(exception);
                return -99;
            }
        }


        private string ImageGenarate(string captchaText, int width = 60, int height = 26)
        {
            CaptchaImage image = new CaptchaImage(captchaText, width, height);
            var iNum = image.RenderImage();
            var codec = GetEncoderInfo("image/jpeg");

            // set image quality
            var eps = new EncoderParameters();
            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)95);
            var ms = new MemoryStream();
            iNum.Save(ms, codec, eps);

            byte[] bitmapBytes = ms.GetBuffer();
            string result = Convert.ToBase64String(bitmapBytes, Base64FormattingOptions.InsertLineBreaks);

            ms.Close();
            ms.Dispose();
            iNum.Dispose();

            return result;
        }

        private string ImageGenarateStyte2(string captchaText, int width = 60, int height = 26)
        {
            var rnd = new Random();

            string[] aFontNames = new string[]
            {
                "Comic Sans MS",
                 "Arial",
                 "Times New Roman",
                 "Georgia",
                 "Verdana",
                 "Geneva"
            };

            FontStyle[] aFontStyles = new FontStyle[]
            {
                 FontStyle.Bold,
                 FontStyle.Italic,
                 FontStyle.Regular,
                 FontStyle.Strikeout,
                 FontStyle.Underline
            };

            HatchStyle[] aHatchStyles = new HatchStyle[]
            {
                 HatchStyle.BackwardDiagonal, HatchStyle.Cross,
                    HatchStyle.DashedDownwardDiagonal, HatchStyle.DashedHorizontal,
                 HatchStyle.DashedUpwardDiagonal, HatchStyle.DashedVertical,
                    HatchStyle.DiagonalBrick, HatchStyle.DiagonalCross,
                 HatchStyle.Divot, HatchStyle.DottedDiamond, HatchStyle.DottedGrid,
                    HatchStyle.ForwardDiagonal, HatchStyle.Horizontal,
                 HatchStyle.HorizontalBrick, HatchStyle.LargeCheckerBoard,
                    HatchStyle.LargeConfetti, HatchStyle.LargeGrid,
                 HatchStyle.LightDownwardDiagonal, HatchStyle.LightHorizontal,
                    HatchStyle.LightUpwardDiagonal, HatchStyle.LightVertical,
                 HatchStyle.Max, HatchStyle.Min, HatchStyle.NarrowHorizontal,
                    HatchStyle.NarrowVertical, HatchStyle.OutlinedDiamond,
                 HatchStyle.Plaid, HatchStyle.Shingle, HatchStyle.SmallCheckerBoard,
                    HatchStyle.SmallConfetti, HatchStyle.SmallGrid,
                 HatchStyle.SolidDiamond, HatchStyle.Sphere, HatchStyle.Trellis,
                    HatchStyle.Vertical, HatchStyle.Wave, HatchStyle.Weave,
                 HatchStyle.WideDownwardDiagonal, HatchStyle.WideUpwardDiagonal, HatchStyle.ZigZag
            };

            var iNum = new Bitmap(width, height);
            var minFontSize = height / 3;
            var maxFontSize = height / 2;

            var gf = Graphics.FromImage(iNum);
            gf.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //var br = new SolidBrush(Color.FromArgb(255, 255, 255));
            var br = new HatchBrush(aHatchStyles[rnd.Next
                (aHatchStyles.Length - 1)], Color.FromArgb((rnd.Next(100, 255)),
                (rnd.Next(100, 255)), (rnd.Next(100, 255))), Color.White);
            gf.FillRectangle(br, 0, 0, width, height);
            var strFormat = new StringFormat { Alignment = StringAlignment.Center };

            var w = rnd.Next(5);
            for (var i = 0; i < captchaText.Length; i++)
            {
                //var cFont = new Font("arial", rnd.Next(minFontSize, maxFontSize), FontStyle.Bold);
                var cFont = new Font(aFontNames[rnd.Next(aFontNames.Length - 1)],
                rnd.Next(minFontSize, maxFontSize), aFontStyles[rnd.Next(aFontStyles.Length - 1)]);

                int fontHeight = (int)cFont.GetHeight();
                int fontWidth = fontHeight * 1 / 2;
                var h = rnd.Next(height - fontHeight);
                //gf.DrawString(captchaText[i].ToString(CultureInfo.InvariantCulture), cFont, Brushes.Black, new RectangleF(w, h, w + fontWidth, h + fontHeight), strFormat);
                gf.DrawString(captchaText[i].ToString(CultureInfo.InvariantCulture), cFont,
                    new SolidBrush(Color.FromArgb(rnd.Next(0, 100), rnd.Next(0, 100), rnd.Next(0, 100))),
                    new RectangleF(w + 5, h, w + fontWidth, h + fontHeight), strFormat);

                w += fontWidth + rnd.Next(5);
            }

            var codec = GetEncoderInfo("image/jpeg");

            // set image quality
            var eps = new EncoderParameters();
            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)95);
            var ms = new MemoryStream();
            iNum.Save(ms, codec, eps);

            byte[] bitmapBytes = ms.GetBuffer();
            string result = Convert.ToBase64String(bitmapBytes, Base64FormattingOptions.InsertLineBreaks);

            ms.Close();
            ms.Dispose();
            iNum.Dispose();
            br.Dispose();

            return result;
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var myEncoders =
                ImageCodecInfo.GetImageEncoders();

            foreach (var myEncoder in myEncoders)
                if (myEncoder.MimeType == mimeType)
                    return myEncoder;
            return null;
        }
    }
}
