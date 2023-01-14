﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial interface IData { }

namespace HECSFramework.Core
{
    public partial class World
    {
        private ConcurrencyList<ushort> updatedEntities = new ConcurrencyList<ushort>(1024);
        private Dictionary<int, ComponentProvider> componentProvidersByTypeIndex = new Dictionary<int, ComponentProvider>(256);
        private ConcurrencyList<FastEntitiesFilter> filters = new ConcurrencyList<FastEntitiesFilter>(16);

        public bool FastEntitiesIsDirty;

        public FastEntity[] FastEntities = new FastEntity[1024];
        private Queue<ushort> freeEntities = new Queue<ushort>(1024);

        private TypeRegistrator[] typeRegistrators = new TypeRegistrator[0];

        partial void InitFastWorld()
        {
            FillTypeRegistrators();

            for (int i = 1; i < FastEntities.Length; i++)
            {
                CreateNewEntity(i);
            }

            foreach (var t in typeRegistrators)
                t.RegisterWorld(this);

            GlobalUpdateSystem.FinishUpdate += UpdateFilters;
        }

        private void CreateNewEntity(int i)
        {
            ref var fast = ref FastEntities[i];
            fast.World = this;
            fast.ComponentIndeces = new HashSet<int>(8);
            fast.Index = (ushort)i;
            freeEntities.Enqueue((ushort)i);
        }

        partial void FillTypeRegistrators();

        public ref FastEntity GetFastEntity()
        {
            if (freeEntities.TryDequeue(out var index))
            {
                ref var fastEntity = ref FastEntities[index];
                fastEntity.IsReady = true;
                RegisterUpdatedFastEntity(ref fastEntity);
                return ref FastEntities[index];
            }

            return ref ResizeAndReturn();
        }

        public void DestroyFastEntity(ushort index)
        {
            ref var fastEntity = ref FastEntities[index];
            fastEntity.IsReady = false;
            fastEntity.Generation++;
            fastEntity.ComponentIndeces.Clear();
            freeEntities.Enqueue(index);
            RegisterUpdatedFastEntity(ref fastEntity);
        }

        private ref FastEntity ResizeAndReturn()
        {
            var currentLenght = FastEntities.Length;
            Array.Resize(ref FastEntities, currentLenght*2);

            for (int i = currentLenght; i < FastEntities.Length; i++)
            {
                if (!FastEntities[i].IsReady)
                {
                    CreateNewEntity(i);
                }
            }

            foreach (var p in componentProvidersByTypeIndex)
            {
                p.Value.Resize();
            }

            return ref GetFastEntity();
        }

        public ComponentProvider GetComponentProvider(int typeIndex)
        {
            return componentProvidersByTypeIndex[typeIndex];
        }

        public void RegisterProvider(ComponentProvider componentProvider)
        {
            componentProvidersByTypeIndex.Add(componentProvider.TypeIndexProvider, componentProvider);
        }

        public FastEntitiesFilter GetFastFilter()
        {
            var filter = new FastEntitiesFilter(this);
            filters.Add(filter);
            return filter;
        }

        private void UpdateFilters()
        {
            foreach (var filter in filters)
            {
                if (filter.IsNeedFullUpdate)
                {
                    filter.UpdateFilter(updatedEntities.Data, updatedEntities.Count);
                    filter.IsNeedFullUpdate = false;
                }
                else
                    filter.UpdateFilter(updatedEntities.Data, updatedEntities.Count);
            }

            foreach (var e in updatedEntities)
                FastEntities[e].Updated = false;

            updatedEntities.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterUpdatedFastEntity(ushort index)
        {
            ref var fastEntity = ref FastEntities[index];

            if (fastEntity.Updated)
                return;

            fastEntity.Updated = true;
            updatedEntities.Add(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterUpdatedFastEntity(ref FastEntity fastEntity)
        {
            if (fastEntity.Updated)
                return;

            fastEntity.Updated = true;
            updatedEntities.Add(fastEntity.Index);
        }

        partial void FastWorldDispose()
        {
            GlobalUpdateSystem.FinishUpdate -= UpdateFilters;

            foreach (var t in typeRegistrators)
                t.UnRegisterWorld(this);

            componentProvidersByTypeIndex.Clear();
        }
    }
}