﻿using System;
using System.Collections.Generic;

namespace NaNoE.V2.Data
{
    public class NHelper : IComparable
    {
        /// <summary>
        /// Helper ID
        /// </summary>
        private int _id;
        public int ID
        {
            get => _id;
        }

        /// <summary>
        /// Helper Name
        ///  - note:
        ///    - [A] => chapter
        ///    - [C] => Characters
        ///    - [I] => Items
        ///    
        ///  - [A:0] => chapter ID 0 start, need to consider update of other chapters
        ///    - this means can have interesting 'add new chapter' forms, but that will be a touch
        ///      difficult to process properly I believe
        ///  - Similarly [C:M] => main character(s), [C:S] => side characters
        ///  - Similarly [I:M] => main item(s), [C:S] => side items if needed
        ///  
        /// - The idea behind this structure is to have an organised helping list with "everything" so to speak
        /// - it's mostly client side 'additions' I think
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
        }

        /// <summary>
        /// Create NHelper
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public NHelper(int id, string name)
        {
            _id = id;
            _name = name;

            Items = new List<NHelperItem>();
        }

        /// <summary>
        /// Helper Items based on this object's ID
        /// </summary>
        public List<NHelperItem> Items { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            return String.Compare(this.Name, ((NHelper)obj).Name);
        }

        public void RefreshNotes()
        {
            Items.Clear();
            var items = DataConnection.Instance.GetHelperItems(ID);
            Items.AddRange(items);
        }
    }
}
