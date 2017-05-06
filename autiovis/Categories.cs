using Android.App;
using Android.OS;
using Android.Widget;
using Android.Util;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace autiovis
{
	[Activity(Label = "@string/app_name",
			  MainLauncher = true,
			  Icon = "@mipmap/autism",
			  Name = "hu.lheller.Autiovis",
			  /* ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, */
			  Theme = "@android:style/Theme.Holo.Light.DarkActionBar"
			 )]

	public class Categories : Activity
	{
		readonly int maxCategories = 20;
		readonly int CPRC = 0x06751920;
		readonly string KEYNAME = "categories";
		readonly string TAG = "[AutiOvis]";

		CatListAdapter ada = null;
		int lastPosition = 0;

		[JsonObject]
		public struct card
		{
			[JsonProperty]
			public string url;

			[JsonProperty]
			public string desc;
		}

		[JsonObject]
		public struct category
		{
			[JsonProperty]
			public Guid id;

			[JsonProperty]
			public string name;

			[JsonProperty]
			public string logo;

			[JsonProperty]
			public List<card> cards;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Categories);

			ada = new CatListAdapter(this, Resource.Id.textView1, maxCategories);
			ada.LoadSPData(KEYNAME);
			var lvCats = FindViewById<ListView>(Resource.Id.catList);

			//var cr = DP2PX(15);
			//lvCats.Background = CreateShape(cr, Color.LightSkyBlue, 10.0f);
			lvCats.SetBackgroundResource(Resource.Drawable.CatListStyle);
			lvCats.Adapter = ada;
			lvCats.ItemClick += (sender, e) =>
			{
				lastPosition = e.Position;
				var c = ada.GetItemAt(lastPosition);
				if (c.cards == null) c.cards = new List<card>();
				var i = new Android.Content.Intent(this, typeof(CardPager));
				i.PutExtra("CARDS", JsonConvert.SerializeObject(c.cards));
				StartActivityForResult(i, CPRC);
			};
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			ada.SaveSPData(KEYNAME);
		}

		public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
		{
			var item1 = menu.Add(new Java.Lang.String("+"));
			//item1.SetIcon(Android.Resource.Mipmap.SymDefAppIcon);
			item1.SetShowAsAction(Android.Views.ShowAsAction.IfRoom);

			var item2 = menu.Add(new Java.Lang.String("-"));

			var item3 = menu.Add(new Java.Lang.String("Exit"));
			return true;
		}

		public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
		{
			if (item.TitleFormatted.ToString() == "+")
			{
				if (ada.Count < maxCategories)
				{
					var adb = new AlertDialog.Builder(this);
					adb.SetTitle("Add new category");
					adb.SetMessage("Please specify the name for new category!");
					var edt = new EditText(this) { Hint = "Category name", Id = 0x00050728 };
					adb.SetView(edt);
					adb.SetPositiveButton("OK", (sender, e) =>
					{
						ada.AddItem(new category() { id = Guid.NewGuid(), name = edt.Text, logo = null, cards = null });
						ada.NotifyDataSetChanged();
					});
					adb.SetNegativeButton("Cancel", (sender, e) => { });
					adb.Show();
				}
				else
				{
					Toast.MakeText(this, "Maximum allowed categories: " + maxCategories, ToastLength.Short).Show();
				}
			}
			else if (item.TitleFormatted.ToString() == "-")
			{
				Toast.MakeText(this, "Remove item selected", ToastLength.Short).Show();
			}
			return true;
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == CPRC && resultCode == Result.Ok && data != null)
			{
				var jsonString = data.GetStringExtra("jsonString");
				Toast.MakeText(this, "Back from Cards activity...", ToastLength.Short).Show();
				Log.Debug(TAG, "jsonString >> " + jsonString);
				ada.SetItemAt(lastPosition, null, null, jsonString);
			}
		}

		//private int DP2PX(int dp)
		//{
		//	//px = dp*dpi/160
		//	return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, Resources.DisplayMetrics);
		//}

		//private ShapeDrawable CreateShape(int cr, Color c, float sw)
		//{
		//	var shd = new ShapeDrawable(new RoundRectShape(new float[] { cr, cr, cr, cr, cr, cr, cr, cr }, null, null));
		//	shd.Paint.Flags = PaintFlags.AntiAlias;
		//	shd.Paint.Color = c;
		//	shd.Paint.SetStyle(Paint.Style.Stroke);
		//	shd.Paint.StrokeCap = Paint.Cap.Butt;
		//	shd.Paint.StrokeWidth = sw;
		//	return shd;
		//}
	}
}
