namespace Postboy.Services
{
    public interface IComponentInteractionService
    {
        void Subscribe(string name, Action action);

        void Notify(string name);

        void Unsubscribe(string name);
    }
}
