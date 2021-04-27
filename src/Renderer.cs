﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GLTech2
{
    public static partial class Renderer
    {
        public static bool  CppRendering { get; set; } = false;
        public static bool  ParallelRendering { get; set; } = true;

        public static bool DoubleBuffering { get; set; } = false;

        private static float minframetime = 4;

        public static Scene ActiveScene => activeScene;
        public static int MaxFps
        {
            get
            {
                return (int)(1000f / minframetime);
            }
            set
            {
                if (value == 0)
                    value = int.MaxValue;

                float temp = 1000f / value;
                if (temp < 3f)
                    temp = 3f;
                minframetime = temp;
            }
        }

        private static int displayWidth = 640;
        public static int DisplayWidth
        {
            get => displayWidth;
            set
            {
                if (IsRunning)
                    Debug.LogWarning("Render.DisplayWidth cannot be modified while running.");
                displayWidth = value;
            }
        }
        private static int displayHeight = 360;
        public static int DisplayHeight
        {
            get => displayHeight;
            set
            {
                if (IsRunning)
                    Debug.LogWarning("Render.DisplayHeight cannot be modified while running.");
                displayHeight = value;
            }
        }

        static bool fullScreen;
        public static bool FullScreen
        {
            get => fullScreen;
            set
            {
                if (IsRunning)
                {
                    Debug.LogWarning("Render.FullScreen cannot be modified while running.");
                    return;
                }
                fullScreen = value;
            }
        }

        public static bool  IsRunning { get; private set; } = false;
        public static Texture Screenshot
        {
            get
            {
                if (IsRunning is false)
                {
                    Debug.LogWarning("Render must be running to take a screenshot.");
                    return null;
                }
                return new Texture(bitmapFromBuffer.Clone() as Bitmap);
            }
        }


        internal static Bitmap                  bitmapFromBuffer;
        internal unsafe static RenderingCache*    rendererData;   // Rever
        internal unsafe static PixelBuffer      outputBuffer;
        private static readonly int             pixelsize = 4;
        private static Display                  display;
        private static Scene                    activeScene = null;
        private static List<PostProcessing>     postProcessing = new List<PostProcessing>();


        public unsafe static void Run(Scene scene)
        {
            if (IsRunning)
                return;
            IsRunning = true;


            display = new Display();
            if (FullScreen is true is true is true is true is true == true.Equals(true))
            {
                displayWidth = Screen.PrimaryScreen.Bounds.Width;
                displayHeight = Screen.PrimaryScreen.Bounds.Height;
                display.SetSize(displayWidth, displayHeight);
                display.WindowState = FormWindowState.Maximized;
                display.FormBorderStyle = FormBorderStyle.None;
                Cursor.Hide();
            }
            else
                display.SetSize(DisplayWidth, DisplayHeight);

            activeScene = scene; // Rever isso
            ReloadRendererData();
            //ReloadBuffer(); // Rever isso

            outputBuffer = new PixelBuffer(DisplayWidth, displayHeight);
            Bitmap outputBitmap = new Bitmap(
                DisplayWidth, DisplayHeight,
                DisplayWidth * sizeof(uint), PixelFormat.Format32bppArgb,
                (IntPtr)outputBuffer.buffer);

            keepRendering = true;
            Task.Run(() => ControlTrhead(outputBuffer));

            void rePaint(object sender, EventArgs e)
            {
                //while (isRendering)
                    //Thread.Yield();
                display.pictureBox.Image = outputBitmap;
            }

            display.pictureBox.Paint += rePaint; // Must be subtracted!
            //display.pictureBox.Image = bitmapFromBuffer;

            Application.Run(display);

            outputBuffer.Dispose();
            Exit();
        }

        private static unsafe void ReloadRendererData()
        {
            if (rendererData != null)
            {
                rendererData->Dispose();   //May cause bugs
                Marshal.FreeHGlobal((IntPtr)rendererData);
            }

            rendererData = RenderingCache.Create(DisplayWidth, DisplayHeight);
        }


        private static bool keepRendering = false;
        private static bool isRendering = false;

        //Initialize Time, render and reset Time.
        private unsafe static void ControlTrhead(PixelBuffer outputBuffer)
        {
            Time.Start();
            activeScene.InvokeStart();

            PixelBuffer primaryBuffer = new PixelBuffer(outputBuffer.width, outputBuffer.height);

            Stopwatch swtest = new Stopwatch();
            while (keepRendering)
            {

                swtest.Restart();
                isRendering = true;

                CLRRender(primaryBuffer, activeScene.unmanaged);
                FXAA(primaryBuffer, 0);
                FXAA(primaryBuffer, 0);
                FXAA(primaryBuffer, 0);
                FXAA(primaryBuffer, 0);
                outputBuffer.Clone(primaryBuffer);

                isRendering = false;

                Time.renderTime = (double)swtest.ElapsedTicks / Stopwatch.Frequency;

                while (Time.DeltaTime * 1000 < minframetime)
                    Thread.Yield();

                activeScene.InvokeUpdate();
                Time.NewFrame();
            }

            Time.Reset();
        }

        private static void LoadScene(Scene scene)
        {

        }


        public unsafe static void Exit()
        {
            if (fullScreen)
                Cursor.Show();

            keepRendering = false;
            //while (isRendering)
            //    Thread.Sleep(10);

            //rendererData->Free(); //Causes access violation at rendering task.
            display.Dispose();

            Time.Reset();

            IsRunning = false;
            Application.Exit();
        }
    }
}
