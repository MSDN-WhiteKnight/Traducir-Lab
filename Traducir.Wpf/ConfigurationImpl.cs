/* Traducir Windows client
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Traducir.Wpf
{
    class ConfigurationImpl : IConfiguration
    {
        class SectionImpl : IConfigurationSection
        {
            string key;
            string val;

            public SectionImpl(string k, string v) { this.key = k; this.val = v; }

            public string this[string key]
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public string Key => this.key;

            public string Path => this.key;

            public string Value
            {
                get => this.val;
                set => throw new NotImplementedException();
            }

            public IEnumerable<IConfigurationSection> GetChildren()
            {
                throw new NotImplementedException();
            }

            public IChangeToken GetReloadToken()
            {
                throw new NotImplementedException();
            }

            public IConfigurationSection GetSection(string key)
            {
                throw new NotImplementedException();
            }
        }

        Dictionary<string, IConfigurationSection> sec = new Dictionary<string, IConfigurationSection>();

        public ConfigurationImpl()
        {
            this.sec["CONNECTION_STRING"] = new SectionImpl(
                "CONNECTION_STRING",
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Traducir.Ru;Integrated Security=True;Connect Timeout=30;"
                );
        }

        public string this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return this.sec.Values;
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            if (this.sec.ContainsKey(key)) return this.sec[key];
            else return new SectionImpl("", "");
        }
    }
}
