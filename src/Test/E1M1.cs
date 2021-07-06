﻿using GLTech2;
using GLTech2.Behaviours;
using GLTech2.Elements;

namespace Test
{
    partial class Program
    {
        class EnableNoclip : Behaviour
        {
            FlatMovement fm;

            public EnableNoclip(FlatMovement fm)
            {
                this.fm = fm;
            }

            void OnFrame()
            {
                if (Keyboard.IsKeyDown(Key.F5))
                    fm.HandleCollisions = false;
                if (Keyboard.IsKeyDown(Key.F6))
                    fm.HandleCollisions = true;
            }
        }

        static void E1M1()
        {
            // Buffers used
            using PixelBuffer textures = new PixelBuffer(WE1M1.WolfTextures);
            using PixelBuffer background_buffer = new PixelBuffer(WE1M1.Background);
            using PixelBuffer lula_buffer = new PixelBuffer(WE1M1.lula);
            using PixelBuffer pt_buffer = new PixelBuffer(WE1M1.pt);
            using PixelBuffer bolsonaro_buffer = new PixelBuffer(WE1M1.bolsonaro);
            using PixelBuffer dolar_buffer = new PixelBuffer(WE1M1._1dolar);

            Texture background = new Texture(background_buffer);
            using Scene scene = new Scene(background);

            // GridMap
            {
                using PixelBuffer grid = new PixelBuffer(WE1M1.MapGrid);
                GridMap.TextureBindings binds = new GridMap.TextureBindings();
                {
                    Texture blueStone1 = new Texture(
                        buffer: textures,
                        hrepeat: 1/6f,
                        vrepeat: 1/19f,
                        hoffset: 2/6f,
                        voffset: 2/19f);

                    Texture blueStone2 = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 4 / 6f,
                        voffset: 2 / 19f);

                    Texture bluestoneCell = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 2 / 6f,
                        voffset: 1 / 19f);

                    Texture bluestoneCellSkeleton = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 0 / 6f,
                        voffset: 2 / 19f);

                    Texture grayStone1 = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 0 / 6f,
                        voffset: 0 / 19f);

                    Texture grayStone2 = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 2 / 6f,
                        voffset: 0 / 19f);

                    Texture gs_naziFlag = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 4 / 6f,
                        voffset: 0 / 19f);

                    Texture gs_hitler = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 0 / 6f,
                        voffset: 1 / 19f);

                    Texture gs_goldEagle = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 4 / 6f,
                        voffset: 1 / 19f);

                    Texture woodPanelling = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 4 / 6f,
                        voffset: 3 / 19f);

                    Texture wp_whiteEagle = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 0 / 6f,
                        voffset: 3 / 19f);

                    Texture wp_hitler = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 2 / 6f,
                        voffset: 3 / 19f);

                    Texture elevator = new Texture(
                        buffer: textures,
                        hrepeat: 1 / 6f,
                        vrepeat: 1 / 19f,
                        hoffset: 0 / 6f,
                        voffset: 17 / 19f);

                    Texture pt = new Texture(pt_buffer);
                    Texture lula = new Texture(lula_buffer);
                    Texture bolsonaro = new Texture(bolsonaro_buffer);
                    Texture dolar = new Texture(dolar_buffer);

                    binds[(0, 0, 255)] = blueStone1;
                    binds[(0, 0, 128)] = blueStone2;
                    binds[(0, 0, 64)] = bluestoneCell;
                    binds[(0, 128, 0)] = bluestoneCellSkeleton;
                    binds[(128, 128, 128)] = grayStone1;
                    binds[(64, 64, 64)] = grayStone2;
                    binds[(255, 0, 0)] = gs_naziFlag;
                    binds[(128, 64, 0)] = gs_hitler;
                    binds[(255, 255, 0)] = gs_goldEagle;
                    binds[(255, 128, 0)] = woodPanelling;
                    binds[(255, 192, 128)] = wp_whiteEagle;
                    binds[(80, 40, 0)] = wp_hitler;
                    binds[(128, 255, 128)] = elevator;
                }

                GridMap gridMap = new GridMap(map: grid, textureBindings: binds);
                scene.AddElement(gridMap);
            }

            // Camera
            Camera camera = new Camera((57.5f, 29.5f), 0);
            {
                camera.AddBehaviour<DebugPerformanceStats>();
                camera.AddBehaviour<DebugSceneInfo>();
                camera.AddBehaviour(new MouseLook(2.2f));

                FlatMovement fm = new FlatMovement();
                fm.RunSpeed = 5f;
                fm.TurnSpeed = 180f;
                camera.AddBehaviour(fm);
                camera.AddBehaviour(new EnableNoclip(fm));

                scene.AddElement(camera);
            }

            // Renderer customization
            Renderer.FullScreen = true;
            Renderer.FieldOfView = 72f;
            Renderer.ParallelRendering = true;
            Renderer.DoubleBuffer = true;
            Renderer.CaptureMouse = true;

            // Run!
            Renderer.Start(camera);
        }
    }
}