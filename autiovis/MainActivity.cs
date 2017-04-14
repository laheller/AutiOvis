﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using Android.Runtime;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;
using Java.Util;

namespace autiovis
{
	[Activity(Label = "@string/app_name",
			  MainLauncher = true,
			  Icon = "@mipmap/autism",
			  Name = "hu.lheller.AutioVis",
			  /* ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, */
			  Theme = "@android:style/Theme.Holo.Light.DarkActionBar"
			 )]
	public class MainActivity : Activity, TextToSpeech.IOnInitListener
	{
		private readonly string TAG = "[AutiOvis]";
		private readonly string engine = "com.google.android.tts";
		//private readonly string engine = "com.samsung.SMT";
		private readonly string keyName = "cards";
		private readonly int RC = 0x00050001;
		private readonly int maxImages = 128;

		public static TextToSpeech tts = null;

		private VPAdapter vpa = null;
		private ViewPager pager = null;

		public void OnInit([GeneratedEnum] OperationResult status)
		{
			var loc = Locale.Default;
			switch (status)
			{
				case OperationResult.Success:
					loc = Locale.Default;
					break;
				case OperationResult.Error:
					loc = Locale.English;
					break;
				default:
					Log.Debug(TAG, "Should not happen!");
					break;
			}
			if (tts != null) tts.SetLanguage(loc);
			Log.Debug(TAG, "Locale set to: " + loc);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			tts = new TextToSpeech(this, this);
			var ttsEngines = tts.Engines;
			var ec = ttsEngines.Count;
			Log.Debug(TAG, "Installed TTS engines: " + ec);
			foreach (var e in ttsEngines)
			{
				Log.Debug(TAG, "Engine name: " + e.Name);
				Log.Debug(TAG, "Engine label: " + e.Label);
			}

			if (ec > 0)
			{
				// configure TTS engine
				tts.SetPitch(1.0f);
				tts.SetSpeechRate(1.0f);
			}
			else
			{
				var ad = new AlertDialog.Builder(this);
				ad.SetTitle("Warning!");
				ad.SetMessage("No TextToSpeech engines detected." + System.Environment.NewLine + "Install a TTS engine and restart this application!");
				ad.SetPositiveButton("OK", (sender, e) => { });
				ad.Show();
			}

			// configure viewpager
			vpa = new VPAdapter(this, maxImages);
			vpa.LoadList(keyName);
			pager = FindViewById<ViewPager>(Resource.Id.pager);
			pager.Adapter = vpa;

			// configure PagerTabStrip
			//var pts = pager.FindViewById<PagerTabStrip>(Resource.Id.pts);
			//pts.SetBackgroundColor(Android.Graphics.Color.AliceBlue);

			// get button and set click event handler
			var btnAddPic = FindViewById<Button>(Resource.Id.btnAddPic);
			btnAddPic.Click += (sender, e) =>
			{
				if (vpa.Count < maxImages)
				{
					var i = new Intent();
					i.SetType("image/*");
					i.SetAction(Intent.ActionGetContent);
					StartActivityForResult(Intent.CreateChooser(i, "Pick an image..."), RC);
				}
				else
				{
					Toast.MakeText(this, "Maximum allowed images: " + maxImages, ToastLength.Short).Show();
				}
			};
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			tts.Shutdown();
			vpa.SaveList(keyName);
			Log.Debug(TAG, "Destroyed...");
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if ((requestCode == RC) && (resultCode == Result.Ok) && (data != null))
			{
				Log.Debug(TAG, "Media selected: " + data.DataString);
				var uri = data.Data;

				var txt = new EditText(this);
				txt.Text = "Title";
				var ad = new AlertDialog.Builder(this);
				ad.SetTitle("Enter card title!");
				ad.SetMessage("Entered text will appear below the new card.");
				ad.SetView(txt);
				ad.SetPositiveButton("OK", (sender, e) =>
				{
					vpa.AddCard(uri, txt.Text);
					vpa.NotifyDataSetChanged();
					pager.SetCurrentItem(vpa.Count - 1, true);
				});
				ad.SetNegativeButton("Cancel", (sender, e) => { });
				ad.Show();
			}
		}
	}
}

