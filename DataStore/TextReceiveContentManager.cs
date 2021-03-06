﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickShare.DataStore
{
    public class TextReceiveContentManager : StorageManager<TextReceiveRow>
    {
        internal TextReceiveContentManager(string _dbPath) : base(_dbPath, "Data")
        {
        }

        public bool ContainsKey(Guid guid)
        {
            return data.Exists(x => x.Id == guid);
        }

        public void Add(Guid guid, string v)
        {
            if (ContainsKey(guid))
                Remove(guid);

            data.Insert(new TextReceiveRow
            {
                Id = guid,
                Content = v,
            });
        }

        public void Remove(Guid guid)
        {
            data.Delete(x => x.Id == guid);
        }

        internal TextReceiveRow GetItem(Guid guid)
        {
            return data.FindById(guid);
        }

        public string GetItemContent(Guid guid)
        {
            return GetItem(guid).Content;
        }
    }
}
