using Android.App;
using Android.Content;
using Task2;

namespace izci
{
    [BroadcastReceiver(Enabled = true, DirectBootAware = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class BootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            
            //Toast.MakeText(context, "BOOT RECEIVED", ToastLength.Long).Show();          
            Intent start = new Intent(context, Java.Lang.Class.FromType(typeof(MainActivity)));
            start.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(start);

        }
    }
}