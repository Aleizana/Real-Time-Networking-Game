using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace OmegaRace
{
    class Program
    {
        //**********   Window auto adjustment stuff. Just ignore ***********
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static public void AdjustWindow()
        {
            Rect rc = System.Windows.SystemParameters.WorkArea;
            float aspectRatio = (float)OmegaRaceGame.Width / OmegaRaceGame.Height;
            int height = (int)rc.Height / 2;
            int width = (int)(aspectRatio * rc.Height / 2);

            //left/right
            int pos = 0;  // left = 0, right = (int)rc.Width / 2

            IntPtr hwndFound = GetConsoleWindow(); // Console window adjustment
            MoveWindow(hwndFound, pos, (int)rc.Height / 2, width, height, true);

            IntPtr hwndFoundg = FindWindow(null, OmegaRaceGame.WindowCaption); // Game window adjustment
            MoveWindow(hwndFoundg, pos, 0, width, height, true);
        }

        //*******************************************************************
        static void Main(string[] args)
        {
            // Create the instance
            OmegaRaceGame game = new OmegaRaceGame();

            // Start the game
            game.Run();
        }
    }
}
