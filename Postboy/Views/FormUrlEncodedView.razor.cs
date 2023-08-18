using Microsoft.AspNetCore.Components;
using Postboy.Helpers;

namespace Postboy.Views
{
    public partial class FormUrlEncodedView
    {
        [Parameter]
        public string? Value { get; set; }

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        private List<(string key, string value)> _formData = new();

        private (string key, string value) _newEntry = (string.Empty, string.Empty);

        protected override Task OnParametersSetAsync()
        {
            try
            {
                _formData = ContentConversion.StringToKeyValuePairs(Value).Select(kv => (kv.Key, kv.Value)).ToList();
            }
            catch (Exception ex)
            {
                Notification.Warning(ex.Message);
                _formData = new();
            }
            return base.OnParametersSetAsync();
        }

        private void OnInput(int idx, (string key, string value) t)
        {
            _formData[idx] = t;
            OnChange();
        }

        private void OnChange()
        {
            var value = ContentConversion.KeyValuePairsToUrlEncodedString(_formData.Select(t => new KeyValuePair<string, string>(t.key, t.value)));
            ValueChanged.InvokeAsync(value);
        }

        private void AddEntry()
        {
            if (string.IsNullOrWhiteSpace(_newEntry.key) && string.IsNullOrWhiteSpace(_newEntry.value))
            {
                return;
            }
            _formData.Add(new(_newEntry.key, _newEntry.value));
            _newEntry = new(string.Empty, string.Empty);
            OnChange();
        }

        private void RemoveEntry((string key, string value) kv)
        {
            _formData.Remove(kv);
            OnChange();
        }
    }
}
