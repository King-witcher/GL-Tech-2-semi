﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GLTech2
{

    /// <summary>
    /// Provides an interface to render scenes and output the video in a window.
    /// </summary>
    public static partial class Renderer
    {
        unsafe static RenderCache* cache;
        static PixelBuffer frontBuffer;
        static Scene activeScene = null;
        static Camera activeCamera = null;

        // public static bool NativeRendering { get; } = false;

        /// <summary>
        ///     Gets and determines whether the renderer should use every CPU unit or just one.
        ///     <para>
        ///         true tells the engine to use every CPU; false tells to use one.
        ///     </para>
        /// </summary>
        public static bool ParallelRendering { get; set; } = true;

        /// <summary>
        /// Gets which scene is being or will be rendered.
        /// </summary>
        public static Scene ActiveScene => activeScene;

        private static float minframetime = 7;
        /// <summary>
        ///     Gets and sets the max framerate the engine can reach.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The value will always be clipped between 1 and 250.
        ///     </para>
        ///     <para>
        ///         Limiting framerate is important to avoid Time.DeltaTime from being to low an being extremelly affected by floating point accuracy problems.
        ///     </para>
        /// </remarks>
        public static int MaxFps
        {
            get => (int)(1000f / minframetime);
            set
            {
                Utilities.Clip(ref value, 1, 250);
                minframetime = 1000f / value;
            }
        }

        static bool doubleBuffer = true;
        /// <summary>
        ///     Gets and sets whether or not the engine should use different buffers to synthesise and display thet image.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Double buffer incresases input lat, reduces framerate but is important if you use post processing effects on the screen and needs to display each frame only after completely made.
        ///     </para>
        ///     <para>
        ///         This property cannot be changed if the Renderer is running.
        ///     </para>
        /// </remarks>
        public static bool DoubleBuffer
        {
            get => doubleBuffer;
            set => ChangeIfNotRunning("DoubleBuffer", ref doubleBuffer, value);
        }

        private static int customWidth = 960;
        /// <summary>
        ///     Gets and sets the custom width of the screen.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This property cannot be changed if the Renderer is running.
        ///     </para>
        /// </remarks>
        public static int CustomWidth
        {
            get => customWidth;
            set => ChangeIfNotRunning("CustomWidth", ref customWidth, value);
        }

        private static int customHeight = 520;
        /// <summary>
        ///     Gets and sets the custom height of the screen.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This property cannot be changed if the Renderer is running.
        ///     </para>
        /// </remarks>
        public static int CustomHeight
        {
            get => customHeight;
            set => ChangeIfNotRunning("CustomHeight", ref customHeight, value);
        }

        static bool fullScreen;
        /// <summary>
        ///     Gets and sets whether the renderer should display at fullscreen or windowed mode.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This property cannot be changed if the Renderer is running.
        ///     </para>
        /// </remarks>
        public static bool FullScreen
        {
            get => fullScreen;
            set
            {
                ChangeIfNotRunning("FullScreen", ref fullScreen, value);
                if (fullScreen == true)
                {
                    CustomWidth = Screen.PrimaryScreen.Bounds.Width;
                    customHeight = Screen.PrimaryScreen.Bounds.Height;
                }
            }
        }

        static float fieldOfView = 90f;
        /// <summary>
        ///     Gets and sets the field of view.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This property cannot be changed if the Renderer is running.
        ///     </para>
        /// </remarks>
        public static float FieldOfView
        {
            get => fieldOfView;
            set
            {
                Utilities.Clip(ref value, 1f, 179f);
                ChangeIfNotRunning("FieldOfView", ref fieldOfView, value);
            }
        }

        static bool captureMouse = false;
        /// <summary>
        ///     Gets and sets whether the engine should or not capture mouse movements.
        /// </summary>
        public static bool CaptureMouse
        {
            get => captureMouse;
            set => captureMouse = value;    // Revisar
        }

        /// <summary>
        ///     Gets whether or not the engine is running.
        /// </summary>
        public static bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Takes a screenshot of the current frame as PixelBuffer.
        /// </summary>
        /// <returns>A screenshot of the current frame.</returns>
        public static PixelBuffer GetScreenshot()
        {
            PixelBuffer pb = new PixelBuffer(CustomWidth, customHeight);
            pb.Clone(frontBuffer);
            return pb;
        }

        /// <summary>
        ///     Adds a new post processing effect to be applied every frame.
        /// </summary>
        /// <param name="postProcessing">Post processing effect to be applied</param>
        public static void AddEffect(PostProcessing postProcessing)
        {
            Renderer.postProcessing.Add(postProcessing);
        }

        /// <summary>
        ///     Adds a new instance of a given post processing effect to be applied every frame.
        /// </summary>
        /// <typeparam name="T">Post processing type</typeparam>
        /// <remarks>
        ///     <para>
        ///         Not every post processing effect can be added via Type because some needs to be setup manually.
        ///     </para>
        /// </remarks>
        public static void AddPostProcessing<T>() where T : PostProcessing, new()
        {
            AddEffect(new T());
        }

        public unsafe static void Start(Camera camera)
        {
            if (IsRunning)
                return;
            IsRunning = true;

            Scene scene = camera.scene;

            if (scene == null)
			{
                Debug.InternalLog(
                    origin: "Renderer",
                    message: "The camera you are trying to render isn't assigned to a scene. Renderer.Start() will be aborted.",
                    debugOption: Debug.Options.Error);
                return;
            }

            activeCamera = camera;

            activeScene = scene;

            // Unmanaged buffer where the video will be put.
            frontBuffer = new PixelBuffer(CustomWidth, customHeight);

            // Create a wrapper "Bitmap" that uses the same buffer.
            var sourceBitmap = new Bitmap(
                CustomWidth, CustomHeight,
                CustomWidth * sizeof(uint), PixelFormat.Format32bppRgb,
                (IntPtr)frontBuffer.uint0);

            var display = new Display(FullScreen, CustomWidth, CustomHeight, sourceBitmap);

            // We must define two booleans to communicate with the tread.
            // The first is necessary to send a stop request.
            // The second is necessary to be aware of when the renderer doesn't need our unmanaged resources and
            // then be able to realease them all.
            var stopRequest = false;
            var controlThreadRunning = true;

            // And then start the control thread, which is reponsible for distributing the buffer among the threads
            // and running the scene scripts.
            var controlThread = Task.Run(() => ControlTrhead(frontBuffer, in stopRequest, ref controlThreadRunning));

            // Finally passes control to the rendering screen and displays it.
            Application.Run(display);

            // Theese lines run after the renderer window is closed.
            stopRequest = true;

            // Wait for the control thread to stop using outputBuffer.
            while (controlThreadRunning)
                Thread.Yield();

            // Finally, dispose everythihng.
            display.Dispose();
            frontBuffer.Dispose();
            sourceBitmap.Dispose();
            activeCamera = null;

            IsRunning = false;
        }

        private static unsafe void ReloadCache()
        {
            if (cache != null)
                RenderCache.Delete(cache);
            cache = RenderCache.Create(CustomWidth, CustomHeight, FieldOfView);
        }

        private unsafe static void ControlTrhead(PixelBuffer frontBuffer, in bool cancellationRequest, ref bool controlThreadRunning)
        {
            ReloadCache();

            // Buffer where the image will be rendered
            PixelBuffer backBuffer = DoubleBuffer ?
                new PixelBuffer(frontBuffer.width, frontBuffer.height) :
                frontBuffer;

            #region Advices
            if (!DoubleBuffer && postProcessing.Count > 0)
                Debug.InternalLog(
                    origin: "Renderer",
                    message: "The renderer has post processing effects but DoubleBuffering is disabled. " +
                        "The engine will display incompletely post processed frames and cause a probably unexpected " +
                        "behaviour.",
                    debugOption: Debug.Options.Warning);

            if (DoubleBuffer && postProcessing.Count == 0)
                Debug.InternalLog(
                    origin: "Renderer", 
                    message: "DoubleBuffer is enabled but no post processing effect is active. If you need " +
                        "more performance or less input lag, consider disabling DoubleBuffering.",
                    debugOption: Debug.Options.Info);
            #endregion

            Stopwatch controlStopwatch = new Stopwatch();   // Need to cap framerate
            Behaviour.Frame.RestartFrame();
            Behaviour.Frame.BeginScript();
            activeScene.OnStart?.Invoke();
            Behaviour.Frame.EndScript();

            // While this variable is set to true, outputBuffer cannot be released by Renderer.Run() thread.
            controlThreadRunning = true;

            while (!cancellationRequest)
            {
                controlStopwatch.Restart();
                Behaviour.Frame.BeginRender();
                CLRRenderLegacy(backBuffer, activeScene.unmanaged);
                PostProcess(backBuffer);

                if (DoubleBuffer)
                    frontBuffer.FastClone(backBuffer);
                Behaviour.Frame.EndRender();

                while (controlStopwatch.ElapsedMilliseconds < minframetime)
                    Thread.Yield();

                Behaviour.Mouse.Measure();
                Behaviour.Frame.RestartFrame();
                Behaviour.Frame.BeginScript();
                activeScene.OnFrame?.Invoke();
                Behaviour.Frame.EndScript();
            }
            controlStopwatch.Stop();
            Behaviour.Frame.Stop();

            // OutputBuffer is up to be released.
            controlThreadRunning = false;
            if (DoubleBuffer)
                backBuffer.Dispose();
        }

        private static void LoadScene(Scene scene)
        {

        }

        private static List<PostProcessing> postProcessing = new List<PostProcessing>();
        private static void PostProcess(PixelBuffer target)
        {
            foreach (var effect in postProcessing)
                effect.Process(target);
        }

        static void ChangeIfNotRunning<T>(string name, ref T obj, T value)
        {
            if (IsRunning)
                Debug.InternalLog(
                    origin: "Renderer",
                    message: name + " cannot be modified while running.",
                    debugOption: Debug.Options.Warning);
            else
                obj = value;
        }
    }
}