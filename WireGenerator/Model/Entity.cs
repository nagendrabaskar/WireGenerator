﻿using System;
using System.Collections.Generic;

namespace WireGenerator.Model
{
    public class Entity
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public bool HasSearch { get; set; }
        public bool IsListScreenWithSelector { get; set; }
        public string WorkflowAliasName { get; set; }
        public ICollection<Field> ListScreenFields { get; set; }
        public ICollection<Action> ListScreenActions { get; set; }
        public ICollection<Section> AddScreenSections { get; set; }
        public ICollection<WorkflowPhase> Workflow { get; set; }
    }
}
