using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

public static class NativeShare
{
	const string ScreenshotFilename = "screenshot.png";

	#if !UNITY_EDITOR && UNITY_IOS
	[DllImport("__Internal")]
	static extern void _CNativeShare(string text, string image, string url);
	#endif

	public static void Share(string text = null, Texture2D image = null, string url = null)
	{
		string imagePath = null;

		if (image != null)
		{
			byte[] png = image.EncodeToPNG();

			imagePath =	Application.persistentDataPath + "/" + ScreenshotFilename;

			File.WriteAllBytes(imagePath, png);
		}

		#if UNITY_EDITOR
		//Nothing
		#elif UNITY_IOS
		_CNativeShare(text, imagePath, url);
		#elif UNITY_ANDROID

		using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
		{
			using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
			{
				intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
				intentObject.Call<AndroidJavaObject>("setType", "image/*");

				//Text
				string extraText = text;
				string htmlText = text;

				if (url != null)
				{
					extraText = (extraText == null) ? url : extraText + " " + url;
					htmlText = (htmlText == null) ? string.Format("<a href=\"{0}\">{0}</a>", url) : string.Format("{0} <a href=\"{1}\">{1}</a>", text, url);
				}

				if (extraText != null)
				{
					intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), extraText);
					intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_HTML_TEXT"), htmlText);
				}

				if (imagePath != null)
				{
					AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");

					AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", imagePath);
					AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);

					bool exists = fileObject.Call<bool>("exists");

					if (exists)
						intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
				}

				AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
				currentActivity.Call("startActivity", intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, ""));
			}
		}

		#endif
	}
}
