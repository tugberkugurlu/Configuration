﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Configuration.CommandLine;

namespace Microsoft.Framework.Configuration
{
    public static class CommandLineConfigurationExtension
    {
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder configuration, string[] args)
        {
            configuration.Add(new CommandLineConfigurationSource(args));
            return configuration;
        }

        public static IConfigurationBuilder AddCommandLine(
            this IConfigurationBuilder configuration,
            string[] args,
            IDictionary<string, string> switchMappings)
        {
            configuration.Add(new CommandLineConfigurationSource(args, switchMappings));
            return configuration;
        }
    }
}
