namespace UnifiConnectionUpgrade.Models;

using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using CommandLine;

internal class Options
{
    public static Options Default => new()
    {
        MinimumSignalStrength = -55,
        InsecureTLS = false
    };
        
    [Option('b', "base-uri", HelpText = "The base URI to the UniFi controller.")]
    public Uri? BaseUri { get; set; }

    [Option('u', "username", HelpText = "The username to log in with.")]
    public string? Username { get; set; }

    [Option('p', "password", HelpText = "The password to log in with.")]
    public string? Password { get; set; }

    [Option('m', "minimum-signal-strength", HelpText = "The minimum signal strength to force a client to reconnect.")]
    public int? MinimumSignalStrength { get; set; }

    [Option('i', "insecure-tls", HelpText = "Skips TLS validation.")]
    public bool? InsecureTLS { get; set; }

    [Option('e', "exclude-mac", HelpText = "A list of MAC addresses to exclude. Useful for clients that only support 2.4 GHz WiFi.")]
    public IEnumerable<string> ExcludedMacs { get; set; } = Array.Empty<string>();
        
    [JsonIgnore]
    [Option('o', "options-file", HelpText = "The path to a JSON formatted options file.")]
    public string? OptionsFile { get; set; }

    public void Merge(Options other)
    {
        this.BaseUri ??= other.BaseUri;
        this.Username ??= other.Username;
        this.Password ??= other.Password;
        this.MinimumSignalStrength ??= other.MinimumSignalStrength;
        this.InsecureTLS ??= other.InsecureTLS;
        this.ExcludedMacs = this.ExcludedMacs.Concat(other.ExcludedMacs);
    }

    public void ThrowIfInvalid()
    {
        if (this.BaseUri is null)
        {
            throw new ArgumentNullException("base-uri", "Base URI is required");
        }

        if (string.IsNullOrEmpty(this.Username))
        {
            throw new ArgumentNullException("username", "User name is required");
        }

        if (string.IsNullOrEmpty(this.Password))
        {
            throw new ArgumentNullException("password", "Password is required");
        }
    }
}