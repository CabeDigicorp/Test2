using DevExpress.Xpf.Editors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using WebServiceClient;
using Syncfusion.XlsIO;

namespace MasterDetailWpf
{
    public class HyperlinkCached : HyperlinkEdit
    {
        static HashSet<string> _browserFileExt = new HashSet<string>()
        {
            ".pdf",
            ".jpg",
            ".png",
        };

        public HyperlinkCached()
        {
            AllowAutoNavigate = false;

        }



        #region FileIdProperty
        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty FileIdProperty =
            DependencyProperty.Register("FileId", typeof(Guid), typeof(HyperlinkCached),
            new FrameworkPropertyMetadata(Guid.Empty,
                  FrameworkPropertyMetadataOptions.AffectsRender |
                  FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public Guid FileId
        {
            get { return (Guid)GetValue(FileIdProperty); }
            set { SetValue(FileIdProperty, value); }
        }
        #endregion


        public async override void DoNavigate()
        {

            string url = ActualNavigationUrl;
            string pathToOpen = url;

            if (string.IsNullOrEmpty(pathToOpen))
                return;

            FileInfo fileInfo = new FileInfo(pathToOpen);
            if (!fileInfo.Exists)
            {
                //file su web 

                if (CacheManager.IsJoinApiSource(url))
                {
                    string fileExt = Path.GetExtension(pathToOpen);
                    if (!_browserFileExt.Contains(fileExt))
                    {
                        //file da scaricare in cache
                        pathToOpen = await CacheManager.Download(/*FileId, */pathToOpen);
                    }
                }

            }

            if (string.IsNullOrEmpty(pathToOpen))
                pathToOpen = url;

            if (File.Exists(pathToOpen))
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = pathToOpen;
                process.Start();

                base.DoNavigate();
            }
            else
            {
                // validate HTTP URL

                Uri uriResult;
                bool result = Uri.TryCreate(pathToOpen, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (result)
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = pathToOpen;
                    process.Start();

                    base.DoNavigate();
                }
            }
        }




    }
}
