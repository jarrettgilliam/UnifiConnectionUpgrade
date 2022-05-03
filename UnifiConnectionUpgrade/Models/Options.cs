using System;
using System.Collections.Generic;
using CommandLine;

namespace UnifiConnectionUpgrade.Models
{
    internal class Options
    {
        [Option('b', "base-uri", Required = true, HelpText = "The base URI to the UniFi controller.")]
        public Uri? BaseUri { get; set; }

        [Option('u', "username", Required = true, HelpText = "The username to log in with.")]
        public string? Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "The password to log in with.")]
        public string? Password { get; set; }

        [Option('m', "minimum-signal-strength", Default = -55, HelpText = "The minimum signal strength to force a client to reconnect.")]
        public int? MinimumSignalStrength { get; set; }

        [Option('i', "insecure-tls", Default = false, HelpText = "Skips TLS validation.")]
        public bool InsecureTLS { get; set; }

        [Option('e', "exclude-mac", HelpText = "A list of MAC addresses to exclude. Useful for clients that only support 2.4 GHz WiFi.")]
        public IEnumerable<string>? ExcludedMacs { get; set; }
    }
}
