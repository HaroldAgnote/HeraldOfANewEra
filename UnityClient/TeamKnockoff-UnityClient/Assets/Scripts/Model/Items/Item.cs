﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Model.Items
{
    public abstract class Item 
    {
        public string Name { get; }

        public Item(string name) {
            Name = name;
        }
    }
}
