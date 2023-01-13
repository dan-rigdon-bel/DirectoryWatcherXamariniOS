using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace DirectoryWatcherXamariniOS.Example
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations


        private FileSystemWatcherTouch DocumentsDirectoryWatcher { get; set; }

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // create a new window instance based on the screen size
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            Window.RootViewController = new UIViewController();

            // make the window visible
            Window.MakeKeyAndVisible();

            // Start watching the Documents folder...
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DocumentsDirectoryWatcher = new FileSystemWatcherTouch(documentsFolder);
            DocumentsDirectoryWatcher.Changed += OnDirectoryDidChange;

            Task.Run(async () =>
            {
                await Task.Delay(5000);

                var filePath = Path.Combine(documentsFolder, "file.txt");

                File.WriteAllText(filePath, "hello");

                await Task.Delay(2000);

                File.Delete(filePath);
            });

            return true;
        }

        public void OnDirectoryDidChange(object sender, EventArgs args)
        {
            Console.WriteLine("Change detected in the Documents folder");
            // Handle the change...
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background execution this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transition from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            base.WillTerminate(application);

            // Stop watching the Documents folder...
            DocumentsDirectoryWatcher.Changed -= OnDirectoryDidChange;
            DocumentsDirectoryWatcher.Dispose(); // This calls Invalidate() on the native watcher.
        }
    }
}


