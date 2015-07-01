// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileProviders;

namespace Microsoft.Framework.Configuration.Ini
{
    /// <summary>
    /// An INI file based <see cref="ConfigurationSource"/>.
    /// Files are simple line structures (<a href="http://en.wikipedia.org/wiki/INI_file">INI Files on Wikipedia</a>)
    /// </summary>
    /// <examples>
    /// [Section:Header]
    /// key1=value1
    /// key2 = " value2 "
    /// ; comment
    /// # comment
    /// / comment
    /// </examples>
    public class IniFileConfigurationSource : ConfigurationSource
    {
        private readonly IFileProvider _fileProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="IniFileConfigurationSource"/>.
        /// </summary>
        /// <param name="fileProvider">The file system used to locate the configuration file based on <paramref name="subpath" />.</param>
        /// <param name="subpath">Relative path of the INI configuration file.</param>
        public IniFileConfigurationSource(IFileProvider fileProvider, string subpath)
            : this(fileprovider, subpath, optional: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IniFileConfigurationSource"/>.
        /// </summary>
        /// <param name="fileProvider">The file system used to locate the configuration file based on <paramref name="subpath" />.</param>
        /// <param name="subpath">Relative path of the INI configuration file.</param>
        /// <param name="optional">Determines if the configuration is optional.</param>
        public IniFileConfigurationSource(IFileProvider fileProvider, string subpath, bool optional)
        {
            if(fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            if (string.IsNullOrEmpty(subpath))
            {
                throw new ArgumentException(Resources.Error_InvalidFilePath, nameof(subpath));
            }

            _fileProvider = fileProvider;
            Optional = optional;
            Subpath = subpath;
        }

        /// <summary>
        /// Gets a value that determines if this instance of <see cref="IniFileConfigurationSource"/> is optional.
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// The relative path of the file backing this instance of <see cref="IniFileConfigurationSource"/>.
        /// </summary>
        public string Subpath { get; }

        /// <summary>
        /// Loads the contents of the file at <see cref="Subpath"/>.
        /// </summary>
        /// <exception cref="FileNotFoundException">If <see cref="Optional"/> is <c>false</c> and a
        /// file does not exist at <see cref="Subpath"/>.</exception>
        public override void Load()
        {
            var fileInfo = _fileProvider.GetFileInfo(Subpath);

            if (!fileInfo.Exists)
            {
                if (Optional)
                {
                    Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    throw new FileNotFoundException(Resources.FormatError_FileNotFound(Subpath), Subpath);
                }
            }
            else
            {
                using (var stream = fileInfo.CreateReadStream())
                {
                    Load(stream);
                }
            }
        }

        internal void Load(Stream stream)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var reader = new StreamReader(stream))
            {
                var sectionPrefix = string.Empty;

                while (reader.Peek() != -1)
                {
                    var rawLine = reader.ReadLine();
                    var line = rawLine.Trim();

                    // Ignore blank lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    // Ignore comments
                    if (line[0] == ';' || line[0] == '#' || line[0] == '/')
                    {
                        continue;
                    }
                    // [Section:header]
                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        // remove the brackets
                        sectionPrefix = line.Substring(1, line.Length - 2) + ":";
                        continue;
                    }

                    // key = value OR "value"
                    int separator = line.IndexOf('=');
                    if (separator < 0)
                    {
                        throw new FormatException(Resources.FormatError_UnrecognizedLineFormat(rawLine));
                    }

                    string key = sectionPrefix + line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();

                    // Remove quotes
                    if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (data.ContainsKey(key))
                    {
                        throw new FormatException(Resources.FormatError_KeyIsDuplicated(key));
                    }

                    data[key] = value;
                }
            }

            Data = data;
        }
    }
}
