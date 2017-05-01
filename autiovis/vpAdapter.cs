using Android.App;
using Android.Runtime;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Speech.Tts;
using Android.Net;
using Android.Support.V4.View;
using Java.Lang;
using System.Collections.Generic;
using Com.Bumptech.Glide;
using Newtonsoft.Json;

namespace autiovis
{
	public class VPAdapter : PagerAdapter
	{
		readonly string TAG = "[AutiOvis]";
		Context ctx = null;
		List<Categories.card> mCards = null;

		// constructor
		public VPAdapter(Context c, string jsonCards)
		{
			ctx = c;
			mCards = JsonConvert.DeserializeObject<List<Categories.card>>(jsonCards);
		}

		// helper method - adds new item to the list
		public void AddCard(Uri u, string d)
		{
			var o = new Categories.card();
			o.desc = d;
			o.url = u.ToString();
			mCards.Add(o);
		}

		// helper method - gets the whole list as json string
		public string GetListAsJsonString()
		{
			return JsonConvert.SerializeObject(mCards);
		}

		public override bool IsViewFromObject(View view, Object objectValue)
		{
			return view == objectValue;
		}

		public override int Count
		{
			get
			{
				//return mCards.Size();
				return mCards.Count;
			}
		}

		public override Object InstantiateItem(View container, int position)
		{
			var li = LayoutInflater.From(ctx);
			var ll = (LinearLayout)li.Inflate(Resource.Layout.Card, null, false);

			var iv = ll.FindViewById<ImageView>(Resource.Id.img);
			var url = Uri.Parse(mCards[position].url);
			Glide.With(ctx).LoadFromMediaStore(url).Thumbnail(0.1f).Into(iv);
			var desc = mCards[position].desc;
			iv.Click += (sender, e) =>
			{
				Log.Debug(TAG, "Clicked on position: " + position);
				if (!CardPager.tts.IsSpeaking) CardPager.tts.Speak(desc, QueueMode.Flush, null);
			};
			iv.LongClick += (sender, e) =>
			{
				//Toast.MakeText(mc, "LongClicked", ToastLength.Short).Show();
				var ct = new EditText(ctx);
				ct.Text = desc;
				var ad = new AlertDialog.Builder(ctx);
				ad.SetTitle("Change text");
				ad.SetMessage("Enter new text for card description!");
				ad.SetView(ct);
				ad.SetPositiveButton("OK", (o, ea) =>
				{
					var card = mCards[position];
					card.desc = ct.Text;
					mCards[position] = card;
					desc = ct.Text;
					NotifyDataSetChanged();
					var pager = container.JavaCast<ViewPager>();
					pager.SetCurrentItem(position, true);
				});
				ad.SetNegativeButton("Cancel", (o, ea) => { });
				ad.Show();
			};

			GetPageTitleFormatted(position);

			var vp = container.JavaCast<ViewPager>();
			vp.AddView(ll);
			return ll;
		}

		public override void DestroyItem(View container, int position, Object objectValue)
		{
			var vp = container.JavaCast<ViewPager>();
			vp.RemoveView(objectValue as View);
		}

		public override ICharSequence GetPageTitleFormatted(int position)
		{
			var desc = mCards[position].desc;
			return new String(string.Format("[{0}]", desc));
		}
	}
}
