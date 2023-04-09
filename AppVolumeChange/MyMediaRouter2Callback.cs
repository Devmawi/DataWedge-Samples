using Android.App;
using Android.Content;
using Android.Database;
using Android.Media;
using Android.OS;
using Android.Runtime;
using System.Xml.Serialization;
using static Android.Media.MediaRouter;

namespace AppVolumeChange
{
    public class MyMediaRouter2Callback : MediaRouter.Callback
    {
        public override void OnRouteAdded(MediaRouter router, RouteInfo info)
        {
           
        }

        public override void OnRouteChanged(MediaRouter router, RouteInfo info)
        {
          
        }

        public override void OnRouteGrouped(MediaRouter router, RouteInfo info, RouteGroup group, int index)
        {
          
        }

        public override void OnRouteRemoved(MediaRouter router, RouteInfo info)
        {
           
        }

        public override void OnRouteSelected(MediaRouter router, [GeneratedEnum] MediaRouteType type, RouteInfo info)
        {
        
        }

        public override void OnRouteUngrouped(MediaRouter router, RouteInfo info, RouteGroup group)
        {
            
        }

        public override void OnRouteUnselected(MediaRouter router, [GeneratedEnum] MediaRouteType type, RouteInfo info)
        {
          
        }

        public override void OnRouteVolumeChanged(MediaRouter router, RouteInfo info)
        {
          
        }
    }
}