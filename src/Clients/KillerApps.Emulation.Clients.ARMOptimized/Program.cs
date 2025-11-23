using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace KillerApps.Emulation.Clients.ARMOptimized
{
    class Program
    {
        static int Main(string[] args)
        {
            // Log ARM capabilities
            LogARMCapabilities();

            RootCommand rootCommand = new RootCommand("Atari Lynx Emulator (ARM Optimized)");
            rootCommand.TreatUnmatchedTokensAsErrors = false;

            rootCommand.Handler = CommandHandler.Create<EmulatorOptions>(Run);

            // Arguments
            rootCommand.AddArgument(new Argument<FileInfo>("gamerom", "Game ROM file"));

            // Options
            rootCommand.AddOption(new Option<bool>(new[] { "--fullscreen", "-f" }, () => false, "Run full screen"));
            rootCommand.AddOption(
                new Option<ControllerType>(
                    new string[] { "--controller", "-c" },
                    () => ControllerType.Keyboard,
                    "Type of controller to use"
                )
            );

            Option<int> magnificationOption = new Option<int>("--magnification", "Magnification of screen");
            magnificationOption.AddAlias("-m");
            magnificationOption.AddValidator(result =>
            {
                if (result.Token is null) return;
                if (!Int32.TryParse(result.Tokens[0].Value, out int value) || value <= 0 || value > 20)
                {
                    result.ErrorMessage = "Magnification must be an integer value between 1 and 20";
                }
            });
            magnificationOption.SetDefaultValue(4);
            magnificationOption.IsRequired = false;
            rootCommand.AddOption(magnificationOption);

            // ARM-specific options
            rootCommand.AddOption(new Option<bool>("--use-simd", () => true, "Use SIMD acceleration when available"));
            rootCommand.AddOption(new Option<OptimizationLevel>(
                "--optimization-level",
                () => OptimizationLevel.Balanced,
                "Optimization level (performance vs. power consumption)"
            ));

            Parser parser = new CommandLineBuilder(rootCommand).UseDefaults().Build();
            return parser.Invoke(args);
        }

        static void LogARMCapabilities()
        {
            Console.WriteLine($"ARM Capabilities:");
            Console.WriteLine($"- Vector64<T> supported: {Vector.IsHardwareAccelerated}");
            Console.WriteLine($"- AdvSimd supported: {AdvSimd.IsSupported}");
            Console.WriteLine($"- AdvSimd.Arm64 supported: {AdvSimd.Arm64.IsSupported}");
            Console.WriteLine($"- Aes supported: {Aes.IsSupported}");
            Console.WriteLine($"- Crc32 supported: {Crc32.IsSupported}");
            Console.WriteLine($"- Dp supported: {Dp.IsSupported}");
            Console.WriteLine($"- Rdm supported: {Rdm.IsSupported}");
            Console.WriteLine($"- Sha1 supported: {Sha1.IsSupported}");
            Console.WriteLine($"- Sha256 supported: {Sha256.IsSupported}");
            Console.WriteLine();
        }

        static void Run(EmulatorOptions options)
        {
            Console.WriteLine($"Starting Atari Lynx Emulator with ARM optimizations");
            Console.WriteLine($"- Game ROM: {options.GameRom?.FullName ?? "Built-in ROM"}");
            Console.WriteLine($"- Fullscreen: {options.FullScreen}");
            Console.WriteLine($"- Controller: {options.Controller}");
            Console.WriteLine($"- Magnification: {options.Magnification}");
            Console.WriteLine($"- Use SIMD: {options.UseSimd}");
            Console.WriteLine($"- Optimization Level: {options.OptimizationLevel}");
            Console.WriteLine();

            using (var game = new EmulatorClient(options))
            {
                game.Run();
            }
        }
    }

    public enum OptimizationLevel
    {
        PowerSaving,
        Balanced,
        Performance
    }
}
