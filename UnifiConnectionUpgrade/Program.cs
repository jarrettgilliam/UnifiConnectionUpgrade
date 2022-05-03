﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using KoenZomers.UniFi.Api.Responses;
using UnifiConnectionUpgrade.Models;

namespace UnifiConnectionUpgrade;

class Program
{
    static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(ReconnectClients);
    }

    static async Task ReconnectClients(Options options)
    {
        try
        {
            var uniFiApi = new KoenZomers.UniFi.Api.Api(options.BaseUri);

            if (options.InsecureTLS)
            {
                uniFiApi.DisableSslValidation();
            }

            if (!await uniFiApi.Authenticate(options.Username, options.Password))
            {
                throw new Exception("Unable to authenticate.");
            }

            foreach (Clients? client in await uniFiApi.GetActiveClients())
            {
                if (ShouldReconnect(client, options))
                {
                    await uniFiApi.ReconnectClient(client.MacAddress);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Environment.Exit(1);
        }
    }

    static bool ShouldReconnect(Clients? client, Options options)
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

        if (options.ExcludedMacs?.Any(s => string.Equals(s, client.MacAddress, StringComparison.OrdinalIgnoreCase)) == true)
        {
            return false;
        }

        return true;
    }
}