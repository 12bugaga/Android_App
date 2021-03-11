using Plugin.FilePicker;
using System;
using Xamarin.Forms;

namespace App3
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void ButtonExit_Cliked (object sender, EventArgs e)
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        private async void ButtonSelectImage_Clicked(object sender, EventArgs e)
        {
            var file = await CrossFilePicker.Current.PickFile();

            if (file != null)
            { 
                string pathImage = file.FilePath;
                /*
                string command1 = "chmod -R 777 /xbin/";
                string command2 = "touch /xbin/img";

                string com3 = "sudo cp /storage/sdcard0/img /system/xbin/";
                Java.Lang.Runtime.GetRuntime().Exec(new string[] { "su", "-c", com3 });
                */

                LabelHallo.Text = pathImage;
                StartProgram.StartSeparate(pathImage);
            }
        }

        private async void ButtonSelectFile_Clicked(object sender, EventArgs e)
        {
            var file = await CrossFilePicker.Current.PickFile();

            if (file != null)
            {
                string pathMain = file.FilePath;
                LabelHallo.Text = pathMain;
                StartProgram.AssemblyImage(pathMain);
            }
        }
    }
}
