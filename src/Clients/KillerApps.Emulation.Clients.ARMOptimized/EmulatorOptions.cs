using System;
using System.IO;

namespace KillerApps.Emulation.Clients.ARMOptimized
{
    public class EmulatorOptions
    {
        public FileInfo GameRom { get; set; }
        public FileInfo BootRom { get; set; }
        public bool FullScreen { get; set; }
        public ControllerType Controller { get; set; } = ControllerType.Keyboard;
        public int Magnification { get; set; } = 4;
        public bool UseSimd { get; set; } = true;
        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Balanced;

        public static EmulatorOptions Default => new EmulatorOptions
        {
            FullScreen = false,
            Controller = ControllerType.Keyboard,
            Magnification = 4
        };
    }
    
    public enum ControllerType
    {
        Keyboard,
        Gamepad
    }
}
