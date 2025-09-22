using SnippetsCore.Json;
using SnippetsDatabase.DTOs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WebAPI.Configuration;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Npgsql;
using SnippetsCore.Configuration;
using WebAPI.Emailing.Configuration;
using WebAPI.Emailing;
using SnippetsCore.DAL;
using SnippetsCore.DAL.Configuration;
using SnippetsCore.DTOs;
using SnippetsCore.Enums;
using System.IO;
namespace SnippetsCore.Lists
{
    public class LinkedListEntry<TEntry>
    {
        public TEntry Entry { get; set; }
        public LinkedListEntry<TEntry> Next
        {
            get;
            set;
        }

        public LinkedListEntry<TEntry> Previous
        {
            get;
            set;
        }

        public LinkedListEntry(LinkedListEntry<TEntry> previous, TEntry entry) {
            Previous = previous;
            Entry = entry;
        }
    }
}
