using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using NLith.KingGame.Backend.Service;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Shop<T>
    {
        public List<T> Items { get; set; }
        public Shop()
        {
            Items = new List<T>();
        }

        public Shop(string name, List<T> items)
        {
            Items = items;
        }

        public bool HasItemInStock(T item)
        {
            return Items.Contains(item);
        }

        public void RemoveItem(T item)
        {
            Items.Remove(item);
        }

        public void AddItem(T item)
        {
            Items.Remove(item);
        }

        public void AddRange(List<T> items)
        {
            Items.AddRange(items);
        }

        public void RestockShop()
        {
            Type type = typeof(T);
            if (type == typeof(Equipment))
            {
                // We don't have an Equipment Generator yet
            }
            else if (type == typeof(Tool))
            {
                ToolService toolService = new ToolService();
                System.Console.WriteLine("Generating Tools");
                Items = new List<T>(toolService.GenerateTools(10).Cast<T>());
                System.Console.WriteLine("Generated Tools");                

            }
            else
            {
                throw new Exception(string.Format("Invalid type {0}", type));
            }
        }
    }
}