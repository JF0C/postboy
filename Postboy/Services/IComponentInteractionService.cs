namespace Postboy.Services
{
    public interface IComponentInteractionService
    {
        void Subscribe(string name, Action action);
        void Notify(string name);
        void Unsubscribe(string name);
        void Subscribe<T>(string name, Action<T> action);
        void Notify<T>(string name, T value);
        void Unsubscribe<T>(string name);
    }
}
