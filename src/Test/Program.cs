﻿using GLTech2;

namespace Test
{
    static internal partial class Program
    {
        static void Main()
        {
            Debug.ConsoleEnabled = true;

            Debug.Pause("Press any key to start.");
            ExtremePlanes();

            Debug.Pause("Press any key to close.");
            Debug.Log("Releasing resources...");
        }
    }
}
