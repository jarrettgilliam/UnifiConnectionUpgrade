namespace UnifiConnectionUpgrade;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using KoenZomers.UniFi.Api.Responses;
using UnifiConnectionUpgrade.Models;

internal class Program
{
    private const int NUMBER_OF_RETRIES = 5;
    private static readonly TimeSpan RETRY_DELAY = TimeSpan.FromSeconds(5);

    private static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<OptionsModel>(args).WithParsedAsync(ReconnectClients);
    }

    private static async Task ReconnectClients(OptionsModel options)
    {
        try
        {
            MergeOptions(options);

            var uniFiApi = new KoenZomers.UniFi.Api.Api(options.BaseUri);

            if (options.InsecureTLS == true)
            {
                LogVerbose(options, "Skipping TLS validation.");
                uniFiApi.DisableSslValidation();
            }

            bool success = false;
            List<Exception> exceptions = new();

            for (int i = 0; i < NUMBER_OF_RETRIES; i++)
            {
                try
                {
                    if (!await uniFiApi.Authenticate(options.Username, options.Password))
                    {
                        throw new Exception("Unable to authenticate.");
                    }

                    foreach (Clients? client in await uniFiApi.GetActiveClients())
                    {
                        if (ShouldReconnect(client, options))
                        {
                            LogVerbose(options, $"Reconnecting {client.Hostname ?? client.MacAddress}");
                            await uniFiApi.ReconnectClient(client.MacAddress);
                        }
                    }

                    success = true;
                    break;
                }
                catch (Exception ex) when (ex is WebException)
                {
                    exceptions.Add(ex);
                    LogVerbose(options, $"An exception occurred, retrying in {RETRY_DELAY.TotalSeconds} seconds.");
                    await Task.Delay(RETRY_DELAY);
                }
            }

            if (!success)
            {
                throw new AggregateException(exceptions);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Environment.Exit(1);
        }
    }

    private static bool ShouldReconnect(Clients? client, OptionsModel options)
    {
        if (client == null)
        {
            return false;
        }

        if (client.IsWired == true)
        {
            return false;
        }

        if (client.RadioProtocol != "ng")
        {
            return false;
        }

        if (client.SignalStrength < options.MinimumSignalStrength)
        {
            return false;
        }

        if (options.ExcludedMacs.Any(s => string.Equals(s, client.MacAddress, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (options.ExcludedSSIDs.Any(s => string.Equals(s, client.EssId, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    private static void MergeOptions(OptionsModel options)
    {
        string optionsFile = !string.IsNullOrEmpty(options.OptionsFile)
            ? Path.GetFullPath(options.OptionsFile)
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                nameof(UnifiConnectionUpgrade),
                "config.json");

        if (File.Exists(optionsFile))
        {
            options.Merge(
                JsonSerializer.Deserialize(
                    File.ReadAllText(optionsFile),
                    OptionsJsonSerializerContext.Default.OptionsModel));

            LogVerbose(options, $"Loaded options from {optionsFile}");
        }

        options.Merge(OptionsModel.Default);

        options.ThrowIfInvalid();
    }

    private static void LogVerbose(OptionsModel options, string message)
    {
        if (options.Verbose)
        {
            Console.WriteLine(message);
        }
    }
}