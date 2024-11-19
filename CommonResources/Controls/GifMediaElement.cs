using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CommonResources
{
    public class GifMediaElement : MediaElement
    {
        public GifMediaElement()
        {
            UnloadedBehavior = MediaState.Close;
            Loaded += GifMediaElement_Loaded;
            Unloaded += GifMediaElement_Unloaded;
            MediaEnded += GifMediaElement_MediaEnded;
        }


        private void GifMediaElement_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        private void GifMediaElement_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Close();
        }

        private void GifMediaElement_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Close();
        }

    }
}
