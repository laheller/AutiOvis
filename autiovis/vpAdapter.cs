using Android.Runtime;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Speech.Tts;
using Android.Net;
using Android.Support.V4.View;
using System.Collections.Generic;
using Java.Lang;
using Com.Bumptech.Glide;
using GoogleGson;
using GoogleGson.Reflect;
using Microsoft.CSharp;

namespace autiovis
{
	public class VPAdapter : PagerAdapter
	{
		private readonly string TAG = "[AutiOvis]";
		private Context mc = null;
		private struct card
		{
			public Uri url;
			public string desc;
		}
		private List<card> mCards = null;

		// constructor
		public VPAdapter(Context c, int max)
		{
			mc = c;
			mCards = new List<card>(max);
		}

		// helper method - adds new item to the list
		public void AddCard(Uri u, string d)
		{
			mCards.Add(new card() { url = u, desc = d });
		}

		// helper method - saves the whole list to as preference
		public bool SaveList(string key)
		{
			if (mCards.Count == 0) return false;
			var sp = mc.GetSharedPreferences("cards", FileCreationMode.Private);
			var ed = sp.Edit();
			var gson = new Gson();
			var arr = new JsonArray();
			foreach (card item in mCards) arr.Add(item.url.ToString() + "|" + item.desc);
			var json = gson.ToJson(arr);
			Log.Debug(TAG, json);
			ed.Remove(key);
			ed.PutString(key, json);
			return ed.Commit();
		}

		// helper method - loads the list from preference
		public void LoadList(string key)
		{
			var sp = mc.GetSharedPreferences("cards", FileCreationMode.Private);
			var str = sp.GetString(key, "nothing");
			if (str != "nothing")
			{
				var jp = new JsonParser();
				var arr = jp.Parse(str).AsJsonArray;
				Log.Debug(TAG, "Arr: " + arr.ToString());
				Log.Debug(TAG, "Size: " + arr.Size());
				for (int i = 0; i < arr.Size(); i++)
				{
					var item = arr.Get(i);
					Log.Debug(TAG, "Item1: " + item.AsString);
					var sc = item.AsString.Split(new char[] { '|' });
					this.AddCard(Uri.Parse(sc[0]), sc[1]);
				}
			}
		}

		public override bool IsViewFromObject(View view, Object objectValue)
		{
			return view == objectValue;
		}

		public override int Count
		{
			get
			{
				return mCards.Count;
			}
		}

		public override Object InstantiateItem(View container, int position)
		{
			var li = LayoutInflater.From(mc);
			var ll = (LinearLayout)li.Inflate(Resource.Layout.Card, null, false);

			var iv = ll.FindViewById<ImageView>(Resource.Id.img);
			Glide.With(mc).LoadFromMediaStore(mCards[position].url).Thumbnail(0.1f).Into(iv);
			iv.Click += (sender, e) =>
			{
				Log.Debug(TAG, "Clicked on position: " + position);
				if (!MainActivity.tts.IsSpeaking) MainActivity.tts.Speak(mCards[position].desc, QueueMode.Flush, null);
			};

			var tv = ll.FindViewById<TextView>(Resource.Id.txt);
			tv.Text = mCards[position].desc;
			this.GetPageTitleFormatted(position);

			var vp = container.JavaCast<ViewPager>();
			vp.AddView(ll);
			return ll;

			//var ad = new Android.App.AlertDialog.Builder(mc);
			//ad.SetTitle("Exception!");
			//ad.SetMessage(ex.Message);
			//ad.SetPositiveButton("OK", (sender, e) => { });
			//ad.Show();
			//Log.Error(TAG, ex.ToString());
		}

		public override void DestroyItem(View container, int position, Object objectValue)
		{
			var vp = container.JavaCast<ViewPager>();
			vp.RemoveView(objectValue as View);
		}

		public override ICharSequence GetPageTitleFormatted(int position)
		{
			return new String(string.Format("[ {0} ]", mCards[position].desc));
		}
	}
}
