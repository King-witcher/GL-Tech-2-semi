﻿using GLTech2;
using GLTech2.PrefabBehaviours;
using GLTech2.PrefabElements;

namespace Test
{
    partial class Program
    {
        static void GridExample()
        {
            // Buffers used
            using PixelBuffer bricks_buffer = new PixelBuffer(DemoTextures.Bricks);
            using PixelBuffer wood_buffer = new PixelBuffer(DemoTextures.Wood);
            using PixelBuffer hexagons_buffer = new PixelBuffer(DemoTextures.GrayHexagons);
            using PixelBuffer background_buffer = new PixelBuffer(DemoTextures.HellSky);

            Texture background = new Texture(background_buffer);

            using Scene scene = new Scene(background);

            // GridMap
            {
                using PixelBuffer grid = new PixelBuffer(DemoTextures.MapGrid);
                GridMap.TextureBindings binds = new GridMap.TextureBindings();
                {
                    Texture bricks = new Texture(
                        buffer: bricks_buffer,
                        hrepeat: 2f);
                    Texture wood = new Texture(
                        buffer: wood_buffer,
                        hrepeat: 1f);
                    Texture hexagons = new Texture(
                        buffer: hexagons_buffer,
                        hrepeat: 2f);

                    binds[(255, 255, 255)] = bricks;
                    binds[(0, 192, 0)] = wood;
                    binds[(128, 0, 255)] = hexagons;
                }

                GridMap gridMap = new GridMap(grid, binds);
                scene.AddElement(gridMap);
            }

            // Observer
            {
                Observer pov = new Observer((5, 5), 180);

                pov.AddBehaviour<DebugFPS>();
                pov.AddBehaviour<DebugPosition>();
                pov.AddBehaviour<DebugWallCount>();
                pov.AddBehaviour<FlatMovement>();
                pov.AddBehaviour(new MouseLook(2.2f));

                scene.AddElement(pov);
            }

            // Renderer customization
            Renderer.FullScreen = true;
            Renderer.FieldOfView = 110f;
            Renderer.ParallelRendering = true;
            Renderer.DoubleBuffering = true;
            Renderer.CaptureMouse = true;

            // Run!
            Renderer.Run(scene);
        }
    }
}