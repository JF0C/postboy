using Microsoft.AspNetCore.Components;

namespace Postboy.Views
{
    public partial class HeadersView
    {
        [Parameter]
        public Dictionary<string, string>? Value { get; set; }

        [Parameter]
        public EventCallback<Dictionary<string, string>> ValueChanged { get; set; }

        private List<(string Key, string Value)>? ModifyableHeaders
        {
            get
            {
                return Value?.Select(kv => (kv.Key, kv.Value)).ToList();
            }
            set
            {
                try
                {
                    var dict = value?.ToDictionary(t => t.Key, t => t.Value);
                    ValueChanged.InvokeAsync(dict);
                }
                catch (Exception ex)
                {
                    Notification.Error(ex.Message);
                }
            }
        }

        private string newKey = "", newValue = "";

        private void Add()
        {
            if (ModifyableHeaders is not null && !string.IsNullOrEmpty(newKey) && !string.IsNullOrEmpty(newValue))
            {
                var header = (newKey, newValue);
                var list = ModifyableHeaders;
                list.Add(header);
                ModifyableHeaders = list;
                newKey = "";
                newValue = "";
            }
        }

        private void Remove(string key)
        {
            if (ModifyableHeaders is not null && !string.IsNullOrEmpty(key))
            {
                ModifyableHeaders = ModifyableHeaders.Where(t => t.Key != key).ToList();
            }
        }

        private void OnKeydownKey(int index, string key)
        {
            if (ModifyableHeaders is null) return;
            var value = ModifyableHeaders[index].Value;
            var list = ModifyableHeaders.ToList();
            list[index] = (key, value);
            ModifyableHeaders = list;
        }

        private void OnKeydownValue(int index, string value)
        {
            if (ModifyableHeaders is null) return;
            var key = ModifyableHeaders[index].Key;
            var list = ModifyableHeaders.ToList();
            list[index] = (key, value);
            ModifyableHeaders = list;
        }
    }
}
