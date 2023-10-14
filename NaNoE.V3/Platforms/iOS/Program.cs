using ObjCRuntime;
using UIKit;

namespace NaNoE.V3;

public class Program
{
	// This is the main entry point of the application.
	static void Main(string[] args)
    {
        PlatformSpec.Type = PlatformType.Mobile;
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
	}
}
