using System;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Installer.Core.Exceptions;
using Installer.Core.FileSystem;
using Installer.Core.Services;
using Installer.Core.Utils;
using Serilog;

namespace Installer.Core.FullFx
{
    public class ImageFlasher : IImageFlasher
    {
        private readonly Regex percentRegex = new Regex(@"(\d*.\d*)%");

        public async Task Flash(Disk disk, string imagePath, IObserver<double> progressObserver = null)
        {
            //EnsureValidImage(imagePath, imageIndex);

            ISubject<string> outputSubject = new Subject<string>();
            IDisposable stdOutputSubscription = null;
            if (progressObserver != null)
            {
                stdOutputSubscription = outputSubject
                    .Select(GetPercentage)
                    .Where(d => !double.IsNaN(d))
                    .Subscribe(progressObserver);
            }
            
            //etcher.exe -d \\.\PHYSICALDRIVE3 "..\Tutorial Googulator\gpt.zip" --yes

            var gptSchemeImagePath = Path.Combine("Files", "Core", "Gpt.zip");

            var etcherPath = Path.Combine("Files", "Tools", "Etcher-Cli", "Etcher");
            var args = $@"-d \\.\PHYSICALDRIVE{disk.Number} ""{gptSchemeImagePath}"" --yes";
            Log.Verbose("We are about to run Etcher: {ExecName} {Parameters}", etcherPath, args);
            var resultCode = await ProcessUtils.RunProcessAsync(etcherPath, args, outputObserver: outputSubject);
            if (resultCode != 0)
            {
                throw new DeploymentException($"There has been a problem during deployment: Etcher exited with code {resultCode}.");
            }

            stdOutputSubscription?.Dispose();
        }

        private double GetPercentage(string output)
        {
            if (output == null)
            {
                return double.NaN;
            }

            var matches = percentRegex.Match(output);

            if (matches.Success)
            {
                var value = matches.Groups[1].Value;
                try
                {
                    var percentage = double.Parse(value, CultureInfo.InvariantCulture) / 100D;
                    return percentage;
                }
                catch (FormatException)
                {
                    Log.Warning($"Cannot convert {value} to double");
                }
            }

            return double.NaN;
        }
    }
}