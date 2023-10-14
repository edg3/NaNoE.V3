using Android.App;
using Android.Runtime;

namespace NaNoE.V3;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
		PlatformSpec.Type = PlatformType.Mobile;
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
