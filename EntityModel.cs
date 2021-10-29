using Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    public class EntityModel : IEntity
    {
        private const string DefaultContainerName = "Default";

        public ICommandService EntityCommandService => null;
        public int WorldId { get; private set; }
        public World World { get; private set; }
        public Guid GUID { get; private set; }
        public HECSMultiMask ComponentsMask { get; } = new HECSMultiMask();
        public IComponent[] GetAllComponents { get; } = new IComponent[TypesMap.SizeOfComponents];
        public List<ISystem> GetAllSystems { get; } = new List<ISystem>();
        public ComponentContext ComponentContext { get; } = new ComponentContext();
        public string ID => ContainerID;

        private HECSMask ActorContainerMask = HMasks.GetMask<ActorContainerID>();

        public string ContainerID 
        {
            get
            {
                if (TryGetHecsComponent(ActorContainerMask, out ActorContainerID actorContainerID))
                    return actorContainerID.ID;
                else
                    return DefaultContainerName;
            }
        }

        public bool IsInited => false;
        public bool IsAlive => false;
        public bool IsPaused => true;

        public EntityModel(int index, string ID)
        {
            WorldId = index;
            World = EntityManager.Worlds[index];
        }

        public void AddHecsComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            if (component == null)
                throw new Exception($"compontent is null " + ID);

            if (TypesMap.GetComponentInfo(component.GetTypeHashCode, out var info))
                component.ComponentsMask = info.ComponentsMask;
            else
                throw new Exception("we dont have needed type in TypesMap, u need to run codogen or check this type manualy" + component.GetType().Name);

            if (GetAllComponents[component.ComponentsMask.Index] != null)
                return;

            component.Owner = this;

            GetAllComponents[component.ComponentsMask.Index] = component;
            ComponentContext.AddComponent(component);
            component.IsAlive = true;

            if (component is IInitable initable)
                initable.Init();

            if (component is IAfterEntityInit afterEntityInit)
                afterEntityInit.AfterEntityInit();

            ComponentsMask.AddMask(component.ComponentsMask.Index);
        }

        public void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem
        {
            if (GetAllSystems.Any(x => x.GetTypeHashCode == system.GetTypeHashCode))
                return;

            system.Owner = this;
            GetAllSystems.Add(system);
        }

        public void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            if (GetAllComponents[component.ComponentsMask.Index] != null)
                RemoveHecsComponent(component.ComponentsMask);

            AddHecsComponent(component, owner, silently);
        }

        public void Command<T>(T command) where T : ICommand
        {
            return;
        }

        public bool ContainsMask(ref HECSMask mask)
        {
            return GetAllComponents[mask.Index] != null;
        }

        public bool ContainsMask(HECSMultiMask mask)
        {
            return ComponentsMask.Contains(mask);
        }

        public bool ContainsMask<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            return GetAllComponents[index] != null;
        }

        public void Dispose()
        {
            if (!EntityManager.IsAlive)
                return;

            EntityManager.RegisterEntity(this, false);

            for (int i = 0; i < GetAllComponents.Length; i++)
            {
                IComponent c = GetAllComponents[i];

                if (c != null)
                    RemoveHecsComponent(c);
            }

            ComponentContext.DisposeContext();
            Array.Clear(GetAllComponents, 0, GetAllComponents.Length);
        }

        public bool Equals(IEntity other)
        {
            return other.GUID == GUID;
        }

        public void GenerateGuid()
        {
            GUID = Guid.NewGuid();
        }

        public IEnumerable<T> GetComponentsByType<T>()
        {
            for (int i = 0; i < GetAllComponents.Length; i++)
            {
                if (GetAllComponents[i] != null && GetAllComponents[i] is T needed)
                    yield return needed;
            }
        }

        public void HecsDestroy()
        {
            Dispose();
        }

        public void Init(bool needRegister = true)
        {
            Init();
        }

        public void Init(int worldIndex, bool needRegister = true)
        {
            WorldId = worldIndex;
            World = EntityManager.Worlds[worldIndex];
        }

        public void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false)
        {
            throw new Exception("��� ������ ������, � ��� ������ ������, ���� ������ ��������� ������");
        }

        public void Pause()
        {
            return;
        }

        public void RemoveHecsComponent(IComponent component)
        {
            if (component == null)
                return;

            if (component is IDisposable disposable)
                disposable.Dispose();

            GetAllComponents[component.ComponentsMask.Index] = null;
            ComponentContext.RemoveComponent(component);
            ComponentsMask.RemoveMask(component.ComponentsMask.Index);

            component.IsAlive = false;
        }

        public void RemoveHecsComponent(HECSMask component)
        {
            var needed = GetAllComponents[component.Index];
            RemoveHecsComponent(needed);
        }

        public void RemoveHecsComponent<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            if (GetAllComponents[index] != null)
                RemoveHecsComponent(GetAllComponents[index]);
        }

        public void RemoveHecsSystem(ISystem system)
        {
            return;
        }

        public void SetGuid(Guid guid)
        {
            GUID = guid;
        }

        public bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent
        {
            var needed = GetAllComponents[mask.Index];

            if (needed != null && needed is T cast)
            {
                component = cast;
                return true;
            }

            component = default;
            return false;
        }

        public bool TryGetHecsComponent<T>(out T component) where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();

            if (GetAllComponents[index] != null)
            {
                component = (T)GetAllComponents[index];
                return true;
            }

            component = default;
            return false;
        }

        public bool TryGetSystem<T>(out T system) where T : ISystem
        {
            system = default;
            return false;
        }

        public void UnPause()
        {
            return;
        }

        T IEntity.GetOrAddComponent<T>(IEntity owner)
        {
            var index = TypesMap.GetIndexByType<T>();
            var needed = GetAllComponents[index];

            if (needed != null)
                return (T)needed;

            var newComp = TypesMap.GetComponentFromFactory<T>();
            AddHecsComponent(newComp, owner);
            return newComp;
        }
    }
}