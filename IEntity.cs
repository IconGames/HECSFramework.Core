﻿using HECSFramework.Core;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core 
{
    public interface IEntity : IDisposable, IHavePause, IEquatable<IEntity>
    {
        ICommandService EntityCommandService { get; }

        int WorldId { get; }
        World World { get; }


        Guid GUID { get; }
        HECSMask ComponentsMask { get; }

        IComponent[] GetAllComponents { get; }
        List<ISystem> GetAllSystems { get; }
        ComponentContext ComponentContext { get; }

        bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent;
        T GetOrAddComponent<T>(IEntity owner = null) where T : class, IComponent;
        void AddHecsComponent(IComponent component, IEntity owner = null, bool silently = false);
        
        void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem;

        void RemoveHecsComponent(IComponent component);
        void RemoveHecsComponent(HECSMask component);
        
        void RemoveHecsSystem(ISystem system);
        void Command<T>(T command) where T : ICommand;

        bool TryGetSystem<T>(out T system) where T : ISystem;
  
        void Init();
        void Init(int worldIndex);
        void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false);
        
        void GenerateGuid();
        void SetGuid(Guid guid);

        bool ContainsMask(ref HECSMask mask);

        void HecsDestroy();

        string ID { get; }
        string ContainerID { get; }

        bool IsInited { get; }
        bool IsAlive { get; }
        bool IsPaused { get; }
    }
}

