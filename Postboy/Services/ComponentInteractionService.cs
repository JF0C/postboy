namespace Postboy.Services
{
    public class ComponentInteractionService : IComponentInteractionService
    {
        private static List<KeyValuePair<string, Action>> _actions = new();
        public void Notify(string name)
        {
            foreach(var kv in _actions.Where(kv => kv.Key == name))
            {
                kv.Value.Invoke();
            }
        }
        public void Subscribe(string name, Action action)
        {
            if (_actions.Any(kv => kv.Key == name))
            {
                _actions.Remove(_actions.FirstOrDefault(kv => kv.Key == name));
            }
            _actions.Add(new KeyValuePair<string, Action>(name, action));
        }
        public void Unsubscribe(string name) 
        {
            var newList = _actions.Where(kv => kv.Key != name).ToList();
            _actions.Clear();
            _actions.AddRange(newList);
        }
    }
}
