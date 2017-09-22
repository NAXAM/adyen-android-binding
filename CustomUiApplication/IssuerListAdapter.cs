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
using Com.Adyen.Core.Models.Paymentdetails;
using Com.Adyen.Core.Utils;
using Android.Util;

namespace CustomUiApplication
{
    public class IssuerListAdapter : ArrayAdapter<InputDetail.Item>
    {
        private String TAG = "IssuerListAdapter";
        private Activity context;
        private List<InputDetail.Item> issuers;
        private class ViewHolder : Java.Lang.Object
        {
            public TextView paymentMethodNameView;
            public ImageView imageView;
        }

        public IssuerListAdapter(Activity context, List<InputDetail.Item> issuers) : base(context, Resource.Layout.payment_method_list, issuers)
        {
            Log.Debug(TAG, "IssuerListAdapter()");
            this.context = context;
            this.issuers = issuers;
        }

        public override View GetView(int position, View view, ViewGroup parent)
        {
            ViewHolder viewHolder;
            if (view == null)
            {
                viewHolder = new ViewHolder();
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

                view = inflater.Inflate(Resource.Layout.payment_method_list, parent, false);

                viewHolder.paymentMethodNameView = (TextView)view.FindViewById(Resource.Id.paymentMethodName);
                viewHolder.imageView = (ImageView)view.FindViewById(Resource.Id.paymentMethodLogo);
                view.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder)view.Tag;
            }

            viewHolder.paymentMethodNameView.Text = issuers[position].Name;
            AsyncImageDownloader.DownloadImage(context, viewHolder.imageView, issuers[position].ImageUrl, null);
            return view;

        }
    }
}

