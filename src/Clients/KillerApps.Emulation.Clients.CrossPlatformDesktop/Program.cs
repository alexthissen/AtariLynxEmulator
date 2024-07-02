using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop
{
    public static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand("Atari Lynx Emulator");
            rootCommand.TreatUnmatchedTokensAsErrors = false;

            rootCommand.Handler = CommandHandler.Create<EmulatorClientOptions>(Run);

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

            Parser parser = new CommandLineBuilder(rootCommand).UseDefaults().Build();
            return parser.Invoke(args);
        }

        private static void Run(int magnification, bool fullScreen, ControllerType controller, FileInfo bootRom, FileInfo gameRom)
        {
            EmulatorClientOptions options = new EmulatorClientOptions()
            {
                FullScreen = fullScreen,
                Magnification = magnification,
                BootRom = bootRom,
                GameRom = gameRom,
                Controller = controller
            };

            Run(options);
        }

        private static void Run(EmulatorClientOptions options)
        {
            using (var game = new EmulatorClient(options))
            {
                game.Run();
            }
        }
    }
}
