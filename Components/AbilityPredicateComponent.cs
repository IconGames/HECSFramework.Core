﻿using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Abilities, "Компонент в котором хранятся предикаты для абилки, цели абилки, и для владельца абилки")]
    public partial class AbilityPredicateComponent : BaseComponent, IInitable, IPoolableComponent
    {
        public PredicatesComponent AbilityPredicates = new PredicatesComponent();
        public PredicatesComponent TargetPredicates = new PredicatesComponent();
        public PredicatesComponent AbilityOwnerPredicates = new PredicatesComponent();

        public void Init()
        {
            AbilityPredicates.Owner = Owner;
            TargetPredicates.Owner = Owner;
            AbilityOwnerPredicates.Owner = Owner;

            //todo нужно обработать кейс когда надо инитить предикаты без бп
            InitBP();
        }

        partial void InitBP();
    }
}