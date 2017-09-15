using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Adyen.Core.Models;
using Android.Graphics;
using Com.Adyen.Core.Utils;

namespace CustomUiApplication
{
    public class PaymentListAdapter : ArrayAdapter<PaymentMethod>
    {
        private static String TAG = "Morejump from Naxam";

        private Activity context;
        private List<PaymentMethod> paymentMethods;
        private LayoutInflater layoutInflater;
        private class ViewHolder : Java.Lang.Object
        {
            public TextView paymentMethodNameView;
            public ImageView imageView;
        }

        public PaymentListAdapter(Activity context, List<PaymentMethod> paymentMethods) : base(context, Resource.Layout.payment_method_list, paymentMethods)
        {
            this.context = context;
            this.paymentMethods = paymentMethods;
            this.layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        public override View GetView(int position, View view, ViewGroup parent)
        {
            //return base.GetView(position, convertView, parent);        }
            ViewHolder viewHolder;
            if (view == null)
            {
                viewHolder = new ViewHolder();
                view = layoutInflater.Inflate(Resource.Layout.payment_method_list, parent, false);
                viewHolder.paymentMethodNameView = (TextView)view.FindViewById(Resource.Id.paymentMethodName);
                viewHolder.imageView = (ImageView)view.FindViewById(Resource.Id.paymentMethodLogo);
                view.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder)view.Tag;
            }
            if (viewHolder != null)
            {
                if (viewHolder.paymentMethodNameView != null && viewHolder.imageView != null)
                {
                    viewHolder.paymentMethodNameView.Text = paymentMethods[position].Name;
                    Bitmap defaultImage = null;
                    AsyncImageDownloader.DownloadImage(context, viewHolder.imageView,
                            paymentMethods[position].LogoUrl, defaultImage);
                }
            }
            return view;

        }

    }
}
