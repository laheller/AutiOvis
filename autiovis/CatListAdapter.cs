using Android.Content;
using Android.Widget;
using Android.Views;
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
			vw.SetBackgroundResource(Resource.Drawable.CatListItemStyle);

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
	}
}
