using System;
using System.Collections.Generic;
using GameLogic;

namespace GameLogic
{
    public sealed class GameObjectComponent : Component
    {
        public UnityEngine.GameObject GameObject { get; private set; }


        public override void Awake()
        {
            GameObject = new UnityEngine.GameObject(Entity.GetType().Name);
            var view = GameObject.AddComponent<ComponentView>();
            view.type = GameObject.name;
            view.Component = this;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnityEngine.Object.Destroy(GameObject);
        }
        
        public void OnNameChanged(string name)
        {
            GameObject.name = $"{Entity.GetType().Name}: {name}";
        }

        public void OnAddComponent(Component component)
        {
            var view = GameObject.AddComponent<ComponentView>();
            view.type = component.GetType().Name;
            view.Component = component;
        }

        public void OnRemoveComponent(Component component)
        {
            var comps = GameObject.GetComponents<ComponentView>();
            foreach (var item in comps)
            {
                if (item.Component == component)
                {
                    UnityEngine.Object.Destroy(item);
                }
            }
        }

        public void OnAddChild(Entity child)
        {
            if (child.GetComponent<GameObjectComponent>() != null)
            {
                child.GetComponent<GameObjectComponent>().GameObject.transform.SetParent(GameObject.transform);
            }
        }
    }
}