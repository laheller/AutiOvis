﻿using Android.Content;
using Android.Widget;
using Android.Views;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace autiovis
{
	public class CatListAdapter : ArrayAdapter<Categories.category>
	{
		readonly string SPNAME = "AutiOvis_DB";
		Context ctx = null;
		List<Categories.category> items = null;

		public CatListAdapter(Context c, int trID, int max) : base(c, trID)
		{
			ctx = c;
			items = new List<Categories.category>(max);
		}

		public void AddItem(Categories.category cat)
		{
			items.Add(cat);
		}

		public override int Count
		{
			get
			{
				return items.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var li = (LayoutInflater)ctx.GetSystemService(Context.LayoutInflaterService);
			var vw = (RelativeLayout)li.Inflate(Resource.Layout.CategoryItem, null, false);
			var cr = DP2PX(15);
			//vw.SetBackgroundColor(Color.LightSkyBlue);
			var lp = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
			lp.SetMargins(cr, cr, cr, cr);
			vw.LayoutParameters = lp;
			vw.Background = CreateShape(cr, Color.LightSkyBlue, 1.0f);

			var iv1 = vw.FindViewById<ImageView>(Resource.Id.imageView1);
			iv1.SetImageResource(Android.Resource.Mipmap.SymDefAppIcon);

			var tv1 = vw.FindViewById<TextView>(Resource.Id.textView1);
			tv1.Text = items[position].name;
			tv1.Gravity = GravityFlags.Center;

			return vw;
		}

		public bool SaveSPData(string key)
		{
			if (items.Count == 0) return false;
			var sp = ctx.GetSharedPreferences(SPNAME, FileCreationMode.Private);
			var ed = sp.Edit();
			var json = JsonConvert.SerializeObject(items);
			ed.Remove(key);
			ed.PutString(key, json);
			return ed.Commit();
		}

		public void LoadSPData(string key)
		{
			var sp = ctx.GetSharedPreferences(SPNAME, FileCreationMode.Private);
			var json = sp.GetString(key, "nothing");
			if (json != "nothing")
			{
				items = JsonConvert.DeserializeObject<List<Categories.category>>(json);
			}
		}

		public Categories.category GetItemAt(int position)
		{
			return items[position];
		}

		public void SetItemAt(int position, string name, string logo, string jsonString)
		{
			var cat = items[position];
			//cat.name = name;
			//cat.logo = logo;
			cat.cards = JsonConvert.DeserializeObject<List<Categories.card>>(jsonString);
			items[position] = cat;
		}

		private int DP2PX(int dp)
		{
			//px = dp*dpi/160
			return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, ctx.Resources.DisplayMetrics);
		}

		private ShapeDrawable CreateShape(int cr, Color c, float sw)
		{
			var shd = new ShapeDrawable(new RoundRectShape(new float[] { cr, cr, cr, cr, cr, cr, cr, cr }, null, null));
			shd.Paint.Flags = PaintFlags.AntiAlias;
			shd.Paint.Color = c;
			shd.Paint.SetStyle(Paint.Style.FillAndStroke);
			shd.Paint.StrokeCap = Paint.Cap.Round;
			shd.Paint.StrokeWidth = sw;
			return shd;
		}
	}
}
