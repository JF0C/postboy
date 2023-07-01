namespace Postboy.Services
{
    public class ComponentInteractionService : IComponentInteractionService
    {
        private static List<KeyValuePair<string, Action>> _actions = new();
        private static List<KeyValuePair<string, object>> _typedAcitons = new();
        public void Notify(string name)
        {
            foreach(var kv in _actions.Where(kv => kv.Key == name))
            {
                kv.Value.Invoke();
            }
        }
        public void Subscribe(string name, Action action)
        {
            Unsubscribe(name);
            _actions.Add(new KeyValuePair<string, Action>(name, action));
        }
        public void Unsubscribe(string name)
        {
            while (_actions.Any(kv => kv.Key == name))
            {
                _actions.Remove(_actions.FirstOrDefault(kv => kv.Key == name));
            }
        }

        public void Notify<T> (string name, T value)
        {
            foreach(var kv in _typedAcitons.Where(kv => kv.Key == name))
            {
                (kv.Value as Action<T>)?.Invoke(value);
            }
        }
        public void Subscribe<T>(string name, Action<T> action)
        {
            Unsubscribe(name);
            _typedAcitons.Add(new KeyValuePair<string, object>(name, action));
        }
        public void Unsubscribe<T>(string name)
        {
            while (_typedAcitons.Any(kv => kv.Key == name))
            {
                _typedAcitons.Remove(_typedAcitons.FirstOrDefault(kv => kv.Key == name));
            }
        }
    }
}
